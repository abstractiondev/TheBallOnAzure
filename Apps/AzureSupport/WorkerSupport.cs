using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using AaltoGlobalImpact.OIP;
using Microsoft.WindowsAzure.StorageClient;
using TheBall.CORE;

namespace TheBall
{
    public static class WorkerSupport
    {
        class BlobCopyItem
        {
            public CloudBlob SourceBlob;
            public CloudBlob TargetBlob;
        }

        public enum SyncOperationType
        {
            Copy,
            Delete,
        }

        public delegate bool PerformCustomOperation(CloudBlob source, CloudBlob target, SyncOperationType operationType);

        public static void ExecuteSubscription(Subscription subscription)
        {
            if (String.IsNullOrEmpty(subscription.SubscriptionType))
                return;
            if(subscription.TargetRelativeLocation == subscription.SubscriberRelativeLocation)
                throw new InvalidDataException("Self-circular subscription for target: " + subscription.TargetRelativeLocation);
            if (subscription.SubscriptionType == SubscribeSupport.SubscribeType_WebPageToSource)
            {
                try
                {
                    CloudBlob cloudBlob =
                        StorageSupport.CurrActiveContainer.GetBlockBlobReference(subscription.SubscriberRelativeLocation);
                    RenderWebSupport.RefreshContent(cloudBlob, true);
                } catch(Exception ex)
                {
                    StorageClientException storageClientException = ex as StorageClientException;
                    if(storageClientException != null)
                    {
                        if (storageClientException.ErrorCode == StorageErrorCode.BlobNotFound)
                            return;
                    }
                    // TODO: Detect and remove the missing subscribers
                    ErrorSupport.ReportException(ex);
                }
            }
            else if(subscription.SubscriptionType == SubscribeSupport.SubscribeType_MasterToReferenceUpdate)
            {
                try
                {
                    string containerLocation = subscription.SubscriberRelativeLocation;
                    string containerType = subscription.SubscriberInformationObjectType;
                    string masterLocation = subscription.TargetRelativeLocation;
                    string masterType = subscription.TargetInformationObjectType;
                    UpdateContainerFromMaster(containerLocation, containerType, masterLocation, masterType);
                } catch(Exception ex)
                {
                    StorageClientException storageClientException = ex as StorageClientException;
                    if (storageClientException != null)
                    {
                        if (storageClientException.ErrorCode == StorageErrorCode.BlobNotFound)
                            return;
                    }
                    ErrorSupport.ReportException(ex);
                }
            } else if(subscription.SubscriptionType == SubscribeSupport.SubscribeType_DirectoryToCollection)
            {
                string directoryLocation = subscription.TargetRelativeLocation;
                string collectionType = subscription.SubscriberInformationObjectType;
                string collectionLocation = subscription.SubscriberRelativeLocation;
                UpdateCollectionFromDirectory(collectionType, collectionLocation, directoryLocation);
            } else if(subscription.SubscriptionType == SubscribeSupport.SubscribeType_CollectionToCollectionUpdate)
            {
                string masterCollectionLocation = subscription.TargetRelativeLocation;
                string masterCollectionType = subscription.TargetInformationObjectType;
                string referenceCollectionLocation = subscription.SubscriberRelativeLocation;
                string referenceCollectionType = subscription.SubscriberInformationObjectType;
                UpdateCollectionFromMasterCollection(referenceCollectionType, referenceCollectionLocation,
                                                     masterCollectionType, masterCollectionLocation);
            }
            else if (subscription.SubscriptionType == SubscribeSupport.SubscribeType_MasterCollectionToContainerUpdate)
            {
                string masterCollectionLocation = subscription.TargetRelativeLocation;
                string masterCollectionType = subscription.TargetInformationObjectType;
                string containerLocation = subscription.SubscriberRelativeLocation;
                string containerType = subscription.SubscriberInformationObjectType;
                UpdateContainerFromMasterCollection(containerType, containerLocation,
                                                     masterCollectionType, masterCollectionLocation);
            }
            else
                throw new InvalidDataException(String.Format(
                    "Unsupported subscription type {0} for object: {1} by {2}", subscription.SubscriptionType,
                    subscription.TargetRelativeLocation, subscription.SubscriberRelativeLocation));
        }

        private static void UpdateContainerFromMasterCollection(string containerType, string containerLocation, string masterCollectionType, string masterCollectionLocation)
        {
            IInformationObject containerObject = StorageSupport.RetrieveInformation(containerLocation, containerType);
            IInformationObject masterCollectionObject = StorageSupport.RetrieveInformation(masterCollectionLocation,
                                                                                           masterCollectionType);
            IInformationCollection masterCollection = (IInformationCollection) masterCollectionObject;
            // TODO: Revisit why this can be null
            if (containerObject == null || masterCollection == null)
                return;
            containerObject.UpdateCollections(masterCollection);
            StorageSupport.StoreInformation(containerObject);
        }

        private static void UpdateCollectionFromMasterCollection(string referenceCollectionType, string referenceCollectionLocation, string masterCollectionType, string masterCollectionLocation)
        {
            IInformationObject referenceCollectionObject = StorageSupport.RetrieveInformation(referenceCollectionLocation,
                                                                                        referenceCollectionType);
            IInformationCollection referenceCollection = (IInformationCollection) referenceCollectionObject;
            // TODO: Revisit why this can be null
            if (referenceCollection == null)
                return;
            referenceCollection.RefreshContent();
            StorageSupport.StoreInformation(referenceCollectionObject);
        }

        private static void UpdateCollectionFromDirectory(string collectionType, string collectionLocation, string directoryLocation)
        {
            IInformationObject collectionObject = StorageSupport.RetrieveInformation(collectionLocation, collectionType);
            IInformationCollection collection = (IInformationCollection) collectionObject;
            collection.RefreshContent();
            StorageSupport.StoreInformation(collectionObject);
        }

        public static void UpdateContainerFromMaster(string containerLocation, string containerType, string masterLocation, string masterType)
        {
            bool masterEtagUpdated = false;
            //do
            //{
                masterEtagUpdated = false;
                IInformationObject container = StorageSupport.RetrieveInformation(containerLocation, containerType);
                IInformationObject referenceToMaster = StorageSupport.RetrieveInformation(masterLocation, masterType);
                string masterEtag = referenceToMaster.ETag;
                string masterID = referenceToMaster.ID;
                Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
                container.CollectMasterObjectsFromTree(result, candidate => candidate.ID == masterID);
                bool foundOutdatedMaster =
                    result.Values.SelectMany(item => item).Count(candidate => candidate.MasterETag != masterEtag) > 0;
                if(foundOutdatedMaster)
                {
                    referenceToMaster.MasterETag = referenceToMaster.ETag;
                    container.ReplaceObjectInTree(referenceToMaster);
                    container.StoreInformation();
                    referenceToMaster = StorageSupport.RetrieveInformation(masterLocation, masterType);
                    masterEtagUpdated = referenceToMaster.ETag != masterEtag;
                }
            //} while (masterEtagUpdated);
        }

        public static int WebContentSync(string sourceContainerName, string sourcePathRoot, string targetContainerName, string targetPathRoot, PerformCustomOperation customHandler = null)
        {
            //requestOptions.BlobListingDetails = BlobListingDetails.Metadata;
            string sourceSearchRoot = sourceContainerName + "/" + sourcePathRoot;
            string targetSearchRoot = targetContainerName + "/" + targetPathRoot;
            CloudBlobContainer targetContainer = StorageSupport.CurrBlobClient.GetContainerReference(targetContainerName);
            BlobRequestOptions requestOptions = new BlobRequestOptions
                                                    {
                                                        UseFlatBlobListing = true,
                                                        BlobListingDetails = BlobListingDetails.Metadata,
                                                    };
            var sourceBlobList = StorageSupport.CurrBlobClient.ListBlobsWithPrefix(sourceSearchRoot, requestOptions).
                OfType<CloudBlob>().OrderBy(blob => blob.Name).ToArray();
            var targetBlobList = StorageSupport.CurrBlobClient.ListBlobsWithPrefix(targetSearchRoot, requestOptions).
                OfType<CloudBlob>().OrderBy(blob => blob.Name).ToArray();
            List<CloudBlob> targetBlobsToDelete;
            List<BlobCopyItem> blobCopyList;
            int sourcePathLen = sourcePathRoot.Length;
            int targetPathLen = targetPathRoot.Length;
            CompareSourceToTarget(sourceBlobList, targetBlobList, sourcePathLen, targetPathLen, 
                out blobCopyList, out targetBlobsToDelete);
            foreach(var blobToDelete in targetBlobsToDelete)
            {
                bool handled = false;
                if(customHandler != null)
                {
                    handled = customHandler(null, blobToDelete, SyncOperationType.Delete);
                }
                if(handled == false)
                    blobToDelete.DeleteWithoutFiringSubscriptions();
            }
            foreach(var blobCopyItem in blobCopyList)
            {
                CloudBlob targetBlob;
                if (blobCopyItem.TargetBlob == null)
                {
                    string sourceBlobNameWithoutSourcePrefix = blobCopyItem.SourceBlob.Name.Substring(sourcePathRoot.Length);
                    string targetBlobName;
                    if (sourceBlobNameWithoutSourcePrefix.StartsWith("/") && String.IsNullOrEmpty(targetPathRoot))
                        targetBlobName = sourceBlobNameWithoutSourcePrefix.Substring(1);
                    else
                        targetBlobName = targetPathRoot + sourceBlobNameWithoutSourcePrefix;
                    //string targetBlobName = String.IsNullOrEmpty(targetPathRoot) ? sourceBlobName.
                    //string targetBlobName = 
                    //    blobCopyItem.SourceBlob.Name.Replace(sourcePathRoot, targetPathRoot);
                    targetBlob = targetContainer.GetBlobReference(targetBlobName);
                }
                else
                    targetBlob = blobCopyItem.TargetBlob;
                bool handled = false;
                Console.WriteLine("Processing sync: " + blobCopyItem.SourceBlob.Name + " => " + targetBlob.Name);
                if(customHandler != null)
                {
                    handled = customHandler(blobCopyItem.SourceBlob, targetBlob, SyncOperationType.Copy);
                }
                if(handled == false)
                    targetBlob.CopyFromBlob(blobCopyItem.SourceBlob);
            }
            return targetBlobsToDelete.Count + blobCopyList.Count;
        }

        private static void CompareSourceToTarget(CloudBlob[] sourceBlobs, CloudBlob[] targetBlobs, int sourcePathLen, int targetPathLen, out List<BlobCopyItem> blobCopyList, out List<CloudBlob> targetBlobsToDelete)
        {
            blobCopyList = new List<BlobCopyItem>();
            targetBlobsToDelete = new List<CloudBlob>();
            int currTargetIX = 0;
            int currSourceIX = 0;
            while(currSourceIX < sourceBlobs.Length && currTargetIX < targetBlobs.Length)
            {
                CloudBlob currSourceItem = sourceBlobs[currSourceIX];
                CloudBlob currTargetItem = targetBlobs[currTargetIX];
                string sourceCompareName = currSourceItem.Name.Substring(sourcePathLen).ToLower();
                string targetCompareName = currTargetItem.Name.Substring(targetPathLen).ToLower();
                int compareResult = String.Compare(sourceCompareName, targetCompareName);
                bool namesMatch = compareResult == 0;
                bool sourceButNoTarget = compareResult < 0;
                if (namesMatch)
                {
                    // Compare blob contents
                    bool sourceIsTemplate = currSourceItem.GetBlobInformationType() ==
                                            StorageSupport.InformationType_WebTemplateValue;
                    bool targetIsTemplate = currTargetItem.GetBlobInformationType() ==
                                            StorageSupport.InformationType_WebTemplateValue;
                    if ((sourceIsTemplate && !targetIsTemplate) || currSourceItem.Properties.ContentMD5 != currTargetItem.Properties.ContentMD5)
                        blobCopyList.Add(new BlobCopyItem
                                             {
                                                 SourceBlob = currSourceItem,
                                                 TargetBlob = currTargetItem
                                             });
                    currSourceIX++;
                    currTargetIX++;
                }
                else if (sourceButNoTarget)
                {
                    blobCopyList.Add(new BlobCopyItem
                                         {
                                             SourceBlob = currSourceItem,
                                             TargetBlob = null
                                         });
                    currSourceIX++;
                }
                else // target but no source
                {
                    targetBlobsToDelete.Add(currTargetItem);
                    currTargetIX++;
                }
            }
            // if the target and source ixs are matched exit here
            if (currSourceIX == sourceBlobs.Length && currTargetIX == targetBlobs.Length)
                return;

            // Delete targets blobs that weren't enumerated before the sources did end
            while(currTargetIX < targetBlobs.Length)
                targetBlobsToDelete.Add(targetBlobs[currTargetIX++]);

            // Copy the source blobs that weren't enumerated before the targets did end
            while(currSourceIX < sourceBlobs.Length)
                blobCopyList.Add(new BlobCopyItem
                                     {
                                         SourceBlob = sourceBlobs[currSourceIX++],
                                         TargetBlob = null
                                     });
        }

        private static int counter = 0;
        public static void ProcessMessage(QueueEnvelope envelope, bool reportEnvelopeError = true)
        {
            try
            {
                InformationContext.Current.InitializeCloudStorageAccess(envelope.ActiveContainerName);
                if (envelope.SingleOperation != null)
                    ProcessSingleOperation(envelope.SingleOperation);
                if (envelope.OrderDependentOperationSequence != null)
                {
                    Exception firstException = null;
                    //envelope.OrderDependentOperationSequence.CollectionContent.ForEach(ProcessSingleOperation);
                    foreach(var singleOperation in envelope.OrderDependentOperationSequence.CollectionContent)
                    {
                        try
                        {
                            ProcessSingleOperation(singleOperation);
                        } catch(Exception ex)
                        {
                            firstException = ex;
                            ErrorSupport.ReportException(ex);
                        }
                    }
                    if (firstException != null)
                        throw firstException;
                }
            }
            catch (Exception ex)
            {
                if (reportEnvelopeError)
                    ErrorSupport.ReportEnvelopeWithException(envelope, ex);
                throw;
            } finally
            {
                InformationContext.ProcessAndClearCurrent();
            }

            counter++;
            if (counter >= 1000)
            {
                QueueSupport.ReportStatistics("Processed " + counter + " messages...");
                counter = 0;
            }

        }

        private static void ProcessSingleOperation(OperationRequest operationRequest)
        {
            if (operationRequest.UpdateWebContentOperation != null)
                ProcessUpdateWebContent(operationRequest.UpdateWebContentOperation);
            if (operationRequest.SubscriberNotification != null)
                WorkerSupport.ExecuteSubscription(operationRequest.SubscriberNotification);
            if (operationRequest.RefreshDefaultViewsOperation != null)
                WorkerSupport.RefreshDefaultViews(operationRequest.RefreshDefaultViewsOperation);
            if (operationRequest.DeleteEntireOwner != null)
            {
                VirtualOwner virtualOwner = new VirtualOwner(operationRequest.DeleteEntireOwner.ContainerName,
                    operationRequest.DeleteEntireOwner.LocationPrefix);
                DeleteEntireOwner(virtualOwner);
            }
            if(operationRequest.DeleteOwnerContent != null)
            {
                VirtualOwner virtualOwner = new VirtualOwner(operationRequest.DeleteOwnerContent.ContainerName,
                    operationRequest.DeleteOwnerContent.LocationPrefix);
                DeleteOwnerContent(virtualOwner);
            }
            if(operationRequest.SubscriptionChainRequest != null)
                WorkerSupport.ExecuteSubscriptionChain(operationRequest.SubscriptionChainRequest);
        }

        public static bool ProcessOwnerSubscriptionChains(IContainerOwner lockedOwner, string acquiredEtag, string containerName)
        {
            try
            {
                if(containerName != null)
                    InformationContext.Current.InitializeCloudStorageAccess(containerName:containerName);
                string[] blobs = SubscribeSupport.GetChainRequestList(lockedOwner);
                var chainContent = blobs.Select(blob => StorageSupport.RetrieveInformation(blob, typeof(SubscriptionChainRequestContent))).Cast<SubscriptionChainRequestContent>().ToArray();
                // Updated the timeout to be half an hour instead of 10 minutes
                const double invalidSubscriptionSubmissionTimeInSeconds = 1800;
                if (chainContent.Any(item => item.SubmitTime < DateTime.UtcNow.AddSeconds(-invalidSubscriptionSubmissionTimeInSeconds)))
                    return false;
                WorkerSupport.ExecuteSubscriptionChains(chainContent);
                foreach (string blob in blobs)
                    StorageSupport.DeleteBlob(blob);
            }
            catch (Exception ex)
            {
                ErrorSupport.ReportException(ex);
                throw;
            }
            finally
            {
                SubscribeSupport.ReleaseChainLock(lockedOwner, acquiredEtag);
                if(containerName != null)
                    InformationContext.ProcessAndClearCurrent();
            }
            counter++;
            if (counter >= 1000)
            {
                QueueSupport.ReportStatistics("Processed " + counter + " messages...");
                counter = 0;
            }
            return true;
        }


        public static bool PollAndExecuteChainSubscription()
        {
            var result = SubscribeSupport.GetOwnerChainsInOrderOfSubmission();
            if (result.Length == 0)
                return false;
            string acquiredEtag = null;
            var firstLockedOwner =
                result.FirstOrDefault(
                    lockCandidate => SubscribeSupport.AcquireChainLock(lockCandidate, out acquiredEtag));
            if (firstLockedOwner == null)
                return false;
            ProcessOwnerSubscriptionChains(firstLockedOwner, acquiredEtag, null);
            return true;
        }


        public static void ExecuteSubscriptionChains(params SubscriptionChainRequestContent[] contentList)
        {
            InformationContext.Current.IsExecutingSubscriptions = true;
            try
            {

                string[] subscriptionTargetList =
                    contentList.SelectMany(reqContent => reqContent.SubscriptionTargetCollection.CollectionContent).Select(subTarget => subTarget.BlobLocation)
                        .ToArray();
                var subscriptions = SubscribeSupport.GetSubscriptionChainItemsInOrderOfExecution(subscriptionTargetList);
                int currSubscription = 1;
                var informationObjectSubscriptions =
                    subscriptions.Where(sub => sub.SubscriptionType != SubscribeSupport.SubscribeType_WebPageToSource).
                        ToArray();
                var webPageSubscriptions =
                    subscriptions.Where(sub => sub.SubscriptionType == SubscribeSupport.SubscribeType_WebPageToSource).
                        ToArray();
                foreach (var subscription in informationObjectSubscriptions)
                {
                    ExecuteSubscription(subscription);
                    Debug.WriteLine("Executing subscription {0} of total {1} of {2} for {3}", currSubscription++, subscriptions.Length, subscription.SubscriptionType,
                        subscription.SubscriberRelativeLocation);
                }
                foreach (var subscription in webPageSubscriptions)
                {
                    //ExecuteSubscription(subscription);
                    OperationRequest operationRequest = new OperationRequest();
                    operationRequest.SubscriberNotification = subscription;
                    QueueSupport.PutToOperationQueue(operationRequest);
                    Debug.WriteLine("Executing subscription {0} of total {1} of {2} for {3}", currSubscription++, subscriptions.Length, subscription.SubscriptionType,
                        subscription.SubscriberRelativeLocation);
                }
            }
            finally
            {
                InformationContext.Current.IsExecutingSubscriptions = false;
            }
        }

        public static void ExecuteSubscriptionChain(SubscriptionChainRequestMessage subscriptionChainRequest)
        {
            InformationContext.Current.IsExecutingSubscriptions = true;
            SubscriptionChainRequestContent requestContent =
                SubscriptionChainRequestContent.RetrieveFromDefaultLocation(subscriptionChainRequest.ContentItemID);
            requestContent.ProcessingStartTime = DateTime.UtcNow;
            requestContent.StoreInformation();
            string[] subscriptionTargetList =
                requestContent.SubscriptionTargetCollection.CollectionContent.Select(subTarget => subTarget.BlobLocation)
                    .ToArray();
            var subscriptions = SubscribeSupport.GetSubscriptionChainItemsInOrderOfExecution(subscriptionTargetList);
            int currSubscription = 1;
            var informationObjectSubscriptions =
                subscriptions.Where(sub => sub.SubscriptionType != SubscribeSupport.SubscribeType_WebPageToSource).
                    ToArray();
            var webPageSubscriptions =
                subscriptions.Where(sub => sub.SubscriptionType == SubscribeSupport.SubscribeType_WebPageToSource).
                    ToArray();
            foreach (var subscription in informationObjectSubscriptions)
            {
                ExecuteSubscription(subscription);
                Debug.WriteLine("Executing subscription {0} of total {1} of {2} for {3}", currSubscription++, subscriptions.Length, subscription.SubscriptionType,
                    subscription.SubscriberRelativeLocation);
            }
            requestContent.ProcessingEndTimeInformationObjects = DateTime.UtcNow;
            requestContent.StoreInformation();
            foreach (var subscription in webPageSubscriptions)
            {
                //ExecuteSubscription(subscription);
                OperationRequest operationRequest = new OperationRequest();
                operationRequest.SubscriberNotification = subscription;
                QueueSupport.PutToOperationQueue(operationRequest);
                Debug.WriteLine("Executing subscription {0} of total {1} of {2} for {3}", currSubscription++, subscriptions.Length, subscription.SubscriptionType,
                    subscription.SubscriberRelativeLocation);
            }
            requestContent.ProcessingEndTimeWebTemplatesRendering = DateTime.UtcNow;
            requestContent.ProcessingEndTime = DateTime.UtcNow;
            requestContent.StoreInformation();
            InformationContext.Current.IsExecutingSubscriptions = false;
        }

        public static void RefreshDefaultViews(RefreshDefaultViewsOperation refreshDefaultViewsOperation)
        {
            Type type = Assembly.GetExecutingAssembly().GetType(refreshDefaultViewsOperation.TypeNameToRefresh);
            DefaultViewSupport.RefreshDefaultViews(refreshDefaultViewsOperation.ViewLocation, type);
        }

        private static void DeleteOwnerContent(VirtualOwner containerOwner)
        {
            StorageSupport.DeleteContentsFromOwner(containerOwner);
        }

        public static void ProcessUpdateWebContent(UpdateWebContentOperation operation)
        {
            string sourceContainerName = operation.SourceContainerName;
            string sourcePathRoot = operation.SourcePathRoot;
            string targetContainerName = operation.TargetContainerName;
            string targetPathRoot = operation.TargetPathRoot;
            bool renderWhileSync = operation.RenderWhileSync;
            WorkerSupport.WebContentSync(sourceContainerName, sourcePathRoot, targetContainerName, targetPathRoot,
                                         renderWhileSync
                                             ? (WorkerSupport.PerformCustomOperation)RenderWebSupport.RenderingSyncHandler
                                             : (WorkerSupport.PerformCustomOperation)RenderWebSupport.CopyAsIsSyncHandler);
        }


        public static void DeleteEntireOwner(IContainerOwner containerOwner)
        {
            StorageSupport.DeleteEntireOwner(containerOwner);
        }

        public static Task GetFirstCompleted(Task[] tasks, out int availableIx)
        {
            Task currArrayTask = null;
            for (availableIx = 0; availableIx < tasks.Length; availableIx++)
            {
                currArrayTask = tasks[availableIx];
                if (currArrayTask.IsCompleted)
                    break;
            }
            if (currArrayTask == null)
                throw new NotSupportedException("Cannot find completed task in array when there is supposed to be one");
            return currArrayTask;
        }


    }
}