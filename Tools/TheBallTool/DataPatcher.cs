using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AaltoGlobalImpact.OIP;
using TheBall;
using TheBall.CORE;
using OIPDomain = AaltoGlobalImpact.OIP.DomainInformationSupport;
using CoreDomain = TheBall.CORE.DomainInformationSupport;

namespace TheBallTool
{
    public static class DataPatcher
    {
        public static void SetAllInvitedViewerMembersAsFullCollaborators()
        {
            var accountIDs = TBRAccountRoot.GetAllAccountIDs();
            foreach(var acctID in accountIDs)
            {
                TBRAccountRoot accountRoot = TBRAccountRoot.RetrieveFromDefaultLocation(acctID);
                TBAccount account = accountRoot.Account;
                foreach(var grpRole in account.GroupRoleCollection.CollectionContent)
                {
                    if (TBCollaboratorRole.IsRoleStatusValidMember(grpRole.RoleStatus) == false)
                        grpRole.RoleStatus = TBCollaboratorRole.RoleStatusMemberValue;
                    if (grpRole.GroupRole == TBCollaboratorRole.ViewerRoleValue)
                        grpRole.GroupRole = TBCollaboratorRole.CollaboratorRoleValue;
                }
                account.StoreAccountToRoot();
            }

        }

        public static void UpdateReferenceInformationsInAllAcountsAndGroups()
        {
            var ownerLocations = GetAllOwnerLocations();
            int totalCount = ownerLocations.Length;
            int currIX = 0;
            foreach(var ownerLocation in ownerLocations)
            {
                Console.WriteLine("Updating number " + (++currIX) + " out of " + totalCount);
                VirtualOwner owner = VirtualOwner.FigureOwner(ownerLocation);
                var informationObjects = StorageSupport.CurrActiveContainer.
                    GetInformationObjects(ownerLocation,
                                          iObj =>
                                          iObj is Blog ||
                                          iObj is Activity ||
                                          iObj is AddressAndLocation);
                foreach (var iObj in informationObjects)
                {
                    try
                    {
                        StorageSupport.StoreInformationMasterFirst(iObj, owner, true);
                        InformationContext.ProcessAndClearCurrent();
                        InformationContext.Current.InitializeCloudStorageAccess(Properties.Settings.Default.CurrentActiveContainerName);
                    } catch(Exception ex)
                    {
                        bool letThrow = false;
                        if (letThrow)
                            throw;
                    }
                }

            }
        }

        public static void EnsureAndRefreshMasterCollections()
        {
            var accountIDs = TBRAccountRoot.GetAllAccountIDs();
            foreach (string accountID in accountIDs)
            {
                string acctLocation = "acc/" + accountID + "/";
                VirtualOwner owner = VirtualOwner.FigureOwner(acctLocation);
                //CoreDomain.EnsureMasterCollections(owner);
                //CoreDomain.RefreshMasterCollections(owner);
                OIPDomain.EnsureMasterCollections(owner);
                OIPDomain.RefreshMasterCollections(owner);
            }
            var groupIDs = TBRGroupRoot.GetAllGroupIDs();
            foreach (string groupID in groupIDs)
            {
                string grpLocation = "grp/" + groupID + "/";
                VirtualOwner owner = VirtualOwner.FigureOwner(grpLocation);
                //CoreDomain.EnsureMasterCollections(owner);
                //CoreDomain.RefreshMasterCollections(owner);
                OIPDomain.EnsureMasterCollections(owner);
                OIPDomain.RefreshMasterCollections(owner);
            }
        }

        public static string[] GetAllOwnerLocations()
        {
            var accountIDs = TBRAccountRoot.GetAllAccountIDs();
            var accountLocs = accountIDs.Select(accID => "acc/" + accID + "/");
            var groupLocs = GetAllGroupLocations();
            return accountLocs.Union(groupLocs).ToArray();
        }

        public static string[] GetAllGroupLocations()
        {
            var groupIDs = TBRGroupRoot.GetAllGroupIDs();
            var groupLocs = groupIDs.Select(grpID => "grp/" + grpID + "/");
            return groupLocs.ToArray();
        }



        public static string[] GetAllAccountLocations()
        {
            var accountIDs = TBRAccountRoot.GetAllAccountIDs();
            var accountLocs = accountIDs.Select(accID => "acc/" + accID + "/");
            return accountLocs.ToArray();
        }

        public static void ReconnectGroupsMastersAndCollections()
        {
            var groupLocs = GetAllGroupLocations();
            foreach(var grpLoc in groupLocs)
                ReconnectMastersAndCollections(grpLoc);
        }

        public static void ReconnectAccountsMastersAndCollections()
        {
            var acctLocs = GetAllAccountLocations();
            foreach (var acctLoc in acctLocs)
                ReconnectMastersAndCollections(acctLoc);
        }

        private static IInformationObject[] GetAllInformationObjects(Predicate<string> filterByFullName,  Predicate<IInformationObject> filterIfFalse)
        {
            string[] ownerLocations = GetAllOwnerLocations();
            List<IInformationObject> result = new List<IInformationObject>();
            foreach(string ownerLocation in ownerLocations)
            {
                var ownerObjects = StorageSupport.CurrActiveContainer.GetInformationObjects(ownerLocation, filterByFullName,  filterIfFalse);
                result.AddRange(ownerObjects);
            }
            return result.ToArray();
        }

        private static void ReconnectMastersAndCollections(string ownerLocation)
        {
            //string myLocalAccountID = "0c560c69-c3a7-4363-b125-ba1660d21cf4";
            //string acctLoc = "acc/" + myLocalAccountID + "/";

            VirtualOwner me = VirtualOwner.FigureOwner(ownerLocation);

            var informationObjects = StorageSupport.CurrActiveContainer.GetInformationObjects(ownerLocation, null, 
                                                                                              nonMaster =>
                                                                                              nonMaster.
                                                                                                  IsIndependentMaster ==
                                                                                              false && (nonMaster is TBEmailValidation == false)).ToArray();
            foreach (var iObj in informationObjects)
            {
                try
                {
                    iObj.ReconnectMastersAndCollections(true);
                } catch(Exception ex)
                {
                    bool ignoreException = false;
                    if (ignoreException == false)
                        throw;
                }
            }
        }



        private static void DoCustomCleanup(string groupLoc)
        {
            var defaultBlogToDelete = StorageSupport.CurrActiveContainer.
                GetInformationObjects(groupLoc, null,
                                      item => item is Blog && item.RelativeLocation.EndsWith("/default")).ToArray();
            foreach (Blog blog in defaultBlogToDelete)
            {
                StorageSupport.DeleteInformationObject(blog);
            }
            InformationContext.ProcessAndClearCurrent();
            InformationContext.Current.InitializeCloudStorageAccess(Properties.Settings.Default.CurrentActiveContainerName);
        }

        private static void SyncWwwPublicFromDefaultGroup()
        {
            string publicSite = "demowww.aaltoglobalimpact.org";
            string[] folderList = new[] {"bootstrap-default", "oip-additions", "www-public"};
            foreach(string folder in folderList)
            {
                string sourceFolder = folder;
                //if (sourceFolder == "www-public")
                //    sourceFolder = "oip-public";
                var operationRequest = RenderWebSupport.SyncTemplatesToSite(StorageSupport.CurrActiveContainer.Name, "grp/9798daca-afc4-4046-a99b-d0d88bb364e0/wwwsite/" + sourceFolder,
                                                     publicSite.Replace('.', '-'), folder, true, false);
                QueueSupport.PutToOperationQueue(operationRequest);
            }
        }

        private static void RefreshAllAccounts()
        {
            var accountIDs = TBRAccountRoot.GetAllAccountIDs();
            foreach (var accountID in accountIDs)
            {
                var accountRoot = TBRAccountRoot.RetrieveFromDefaultLocation(accountID);
                accountRoot.Account.StoreAccountToRoot();
            }

        }

        private static void FixGroupMastersAndCollections(string groupID)
        {
            TBRGroupRoot groupRoot = TBRGroupRoot.RetrieveFromDefaultLocation(groupID);
            OIPDomain.EnsureMasterCollections(groupRoot.Group);
            OIPDomain.RefreshMasterCollections(groupRoot.Group);
            groupRoot.Group.ReconnectMastersAndCollectionsForOwner();
        }

        private static void AddLegacyGroupWithInitiator(string groupID, string initiatorEmailAddress)
        {
            var groupRoot = TBRGroupRoot.CreateLegacyNewWithGroup(groupID);
            groupRoot.Group.JoinToGroup(initiatorEmailAddress, TBCollaboratorRole.InitiatorRoleValue);
            //groupRoot.Group.JoinToGroup("jeroen@caloom.com", "moderator");
            StorageSupport.StoreInformation(groupRoot);
            OIPDomain.EnsureMasterCollections(groupRoot.Group);
            OIPDomain.RefreshMasterCollections(groupRoot.Group);
            groupRoot.Group.ReconnectMastersAndCollectionsForOwner();
        }

        private static void RemoveBlogLocationsOnce()
        {
            var blogs = GetAllInformationObjects(name => name.Contains("Blog"), io => io is Blog).Cast<Blog>().ToArray();
            foreach (var blog in blogs)
            {
                //blog.Location = null;
                //blog.StoreInformation();
            }
        }

        private static void RemoveActivityLocationsOnce()
        {
            var activities = GetAllInformationObjects(null, io => io is Activity).Cast<Activity>().ToArray();
            foreach (var activity in activities)
            {
                //activity.Location = null;
                //activity.StoreInformation();
            }
        }

        private static void InitBlogProfileAndIconOnce()
        {
            var blogs = GetAllInformationObjects(name => name.Contains("/Blog/"), io => io is Blog).Cast<Blog>().ToArray();
            try
            {
                foreach (var blog in blogs)
                {
                    blog.IconImage = Image.CreateDefault();
                    blog.ProfileImage = Image.CreateDefault();
                    VirtualOwner owner = VirtualOwner.FigureOwner(blog);
                    blog.StoreInformationMasterFirst(owner, false);
                }
            }
            finally
            {
                InformationContext.ProcessAndClearCurrent();
                InformationContext.Current.InitializeCloudStorageAccess(Properties.Settings.Default.CurrentActiveContainerName);
            }
        }

        private static void InitBlogGroupActivityImageGroupCollectionsOnce()
        {
            var blogsGroupsActivities = GetAllInformationObjects(null, io => io is Activity || io is Blog || io is GroupContainer).ToArray();
            var blogs = blogsGroupsActivities.Where(ba => ba is Blog).Cast<Blog>().ToArray();
            var activities = blogsGroupsActivities.Where(ba => ba is Activity).Cast<Activity>().ToArray();
            var groupContainers = blogsGroupsActivities.Where(ba => ba is GroupContainer).Cast<GroupContainer>().ToArray();
            foreach (var blog in blogs.Where(bl => bl.ImageGroupCollection == null))
            {
                blog.ImageGroupCollection = ImageGroupCollection.CreateDefault();
                blog.StoreInformation();
                blog.ReconnectMastersAndCollections(false);
            }
            foreach (var activity in activities.Where(act => act.ImageGroupCollection == null))
            {
                activity.ImageGroupCollection = ImageGroupCollection.CreateDefault();
                activity.StoreInformation();
                activity.ReconnectMastersAndCollections(false);
            }
            foreach (var groupContainer in groupContainers.Where(grpC => grpC.ImageGroupCollection == null))
            {
                groupContainer.ImageGroupCollection = ImageGroupCollection.CreateDefault();
                groupContainer.StoreInformation();
                groupContainer.ReconnectMastersAndCollections(false);
            }

        }


        private static void InitBlogAndActivityLocationCollectionsOnce()
        {
            var blogsAndActivities = GetAllInformationObjects(null, io => io is Activity || io is Blog).ToArray();
            var blogs = blogsAndActivities.Where(ba => ba is Blog).Cast<Blog>().ToArray();
            var activities = blogsAndActivities.Where(ba => ba is Activity).Cast<Activity>().ToArray();
            foreach (var blog in blogs.Where(bl => bl.LocationCollection == null))
            {
                blog.LocationCollection = AddressAndLocationCollection.CreateDefault();
                blog.StoreInformation();
                blog.ReconnectMastersAndCollections(false);
            }
            foreach (var activity in activities.Where(act => act.LocationCollection == null))
            {
                activity.LocationCollection = AddressAndLocationCollection.CreateDefault();
                activity.StoreInformation();
                activity.ReconnectMastersAndCollections(false);
            }
        }

        private static void ConnectMapContainerToCollections()
        {
            var mapContainers = GetAllInformationObjects(null, io => io is MapContainer).Cast<MapContainer>().ToArray();
            foreach (var mapContainer in mapContainers)
            {
                mapContainer.MarkerSourceActivities = ActivityCollection.CreateDefault();
                mapContainer.MarkerSourceBlogs = BlogCollection.CreateDefault();
                mapContainer.MarkerSourceLocations = AddressAndLocationCollection.CreateDefault();
                mapContainer.ReconnectMastersAndCollections(true);
            }
        }

        private static void ClearEmptyLocations()
        {
            var locations =
                GetAllInformationObjects(null, io => io is AddressAndLocation).Cast<AddressAndLocation>().ToArray();
            foreach(var loc in locations)
            {
                if(String.IsNullOrEmpty(loc.Location.LocationName))
                {
                    try
                    {
                        StorageSupport.DeleteInformationObject(loc);
                    } finally
                    {
                        InformationContext.ProcessAndClearCurrent();
                        InformationContext.Current.InitializeCloudStorageAccess(Properties.Settings.Default.CurrentActiveContainerName);
                    }
                }
            }
        }

        private static void UpdateAllImageFormatsCustomGroup()
        {
            //var images =
            //    GetAllInformationObjects(name => name.Contains("/Image/") && name.Contains("/9798daca-"), io => io is Image).Cast<Image>().ToArray();
            var ownerLocations = GetAllOwnerLocations();
            var ownerLocation = ownerLocations.Where(loc => loc.Contains("/9798daca-")).SingleOrDefault();
            var images = StorageSupport.CurrActiveContainer.GetInformationObjects(ownerLocation, name => name.Contains("/Image/"), io => io is Image).Cast<Image>().ToArray();
            int currImageIndex = 0;
            foreach (var image in images)
            {
                if (image.ImageData.ID.Contains("a25982") == false)
                    continue;
                image.ImageData.UpdateAdditionalMediaFormats();
                Console.WriteLine("Processed Image: " + ++currImageIndex + " out of " + images.Length);
            }
            InformationContext.ProcessAndClearCurrent();
            InformationContext.Current.InitializeCloudStorageAccess(Properties.Settings.Default.CurrentActiveContainerName);
        }

        private static void ReportAllSubscriptionCounts()
        {
            //var informationObjects = GetAllInformationObjects(io => SubscribeSupport.GetSubscriptions(io.RelativeLocation) != null).ToArray();
            long memBefore = GC.GetTotalMemory(false);
            string interestGroupLocation = "grp/" + RenderWebSupport.DefaultGroupID + "/";
            var informationObjects = StorageSupport.CurrActiveContainer.GetInformationObjects(interestGroupLocation, null, io =>  io is AddressAndLocation && 
                SubscribeSupport.GetSubscriptions(io.RelativeLocation) != null).ToArray();

            int currMaxSubs = 0;
            int currMaxDistinct = 0;
            Dictionary<string, SubcriptionGraphItem> lookupDictionary = new Dictionary<string, SubcriptionGraphItem>();
            //lookupDictionary = null;
            DateTime before = DateTime.Now;
            foreach(var iObject in informationObjects)
            {
                int subCount = GetTotalSubscriberCount(iObject, ref currMaxSubs, ref currMaxDistinct, lookupDictionary);
            }
            DateTime after = DateTime.Now;
            var executionOrder = lookupDictionary.GetExecutionOrder();
            var subscriptionsToExecute = executionOrder.SelectMany(exec => exec.GetMySubscriptionsFromTargets()).ToArray();
            DateTime afterWards = DateTime.Now;
            TimeSpan duration1 = after - before;
            TimeSpan duration2 = afterWards - before;
            var filteredListToExecute =
                SubscribeSupport.GetSubscriptionChainItemsInOrderOfExecution(
                    informationObjects.Select(io => io.RelativeLocation).ToArray());
            long memAfter = GC.GetTotalMemory(false);
        }

        private static void TestWorkerSubscriberChainExecutionPerformance()
        {
            string interestGroupLocation = "grp/" + RenderWebSupport.DefaultGroupID + "/";
            var informationObjects =
                StorageSupport.CurrActiveContainer.GetInformationObjects(interestGroupLocation, null, 
                                                                         io => io is AddressAndLocation &&
                                                                               SubscribeSupport.GetSubscriptions(
                                                                                   io.RelativeLocation) != null).ToArray
                    ();
            OperationRequest operationRequest = new OperationRequest();
            SubscriptionChainRequestContent content = SubscriptionChainRequestContent.CreateDefault();
            SubscriptionChainRequestMessage message = SubscriptionChainRequestMessage.CreateDefault();
            message.ContentItemID = content.ID;
            content.SubmitTime = DateTime.UtcNow;
            SubscriptionTarget[] targets = informationObjects.
                Select(io =>
                           {
                               SubscriptionTarget target = SubscriptionTarget.CreateDefault();
                               target.BlobLocation = io.RelativeLocation;
                               return target;
                           }).ToArray();
            content.SubscriptionTargetCollection.CollectionContent.AddRange(targets);
            content.StoreInformation();
            operationRequest.SubscriptionChainRequest = message;
            QueueSupport.PutToOperationQueue(operationRequest);
        }

        private static void TestSubscriptionExecution()
        {
            string interestGroupLocation = "grp/" + RenderWebSupport.DefaultGroupID + "/";
            var informationObjects =
                StorageSupport.CurrActiveContainer.GetInformationObjects(interestGroupLocation, null, 
                                                                         io => io is AddressAndLocation &&
                                                                               SubscribeSupport.GetSubscriptions(
                                                                                   io.RelativeLocation) != null).ToArray
                    ();
            OperationRequest operationRequest = new OperationRequest();
            SubscriptionChainRequestContent content = SubscriptionChainRequestContent.CreateDefault();
            SubscriptionChainRequestMessage message = SubscriptionChainRequestMessage.CreateDefault();
            message.ContentItemID = content.ID;
            content.SubmitTime = DateTime.UtcNow;
            SubscriptionTarget[] targets = informationObjects.
                Select(io =>
                {
                    SubscriptionTarget target = SubscriptionTarget.CreateDefault();
                    target.BlobLocation = io.RelativeLocation;
                    return target;
                }).ToArray();
            content.SubscriptionTargetCollection.CollectionContent.AddRange(targets);
            content.StoreInformation();
            WorkerSupport.ExecuteSubscriptionChain(message);
        }

        private static void ExecuteSubscriptionChain(string groupID)
        {
            string interestGroupLocation = SubscribeSupport.ChainRequestDirectory + "grp/" + groupID + "/";
            var submissions =
                StorageSupport.CurrActiveContainer.GetInformationObjects(interestGroupLocation, null,
                                                                         io => io is SubscriptionChainRequestContent).
                    Cast<SubscriptionChainRequestContent>().ToArray();
            WorkerSupport.ExecuteSubscriptionChains(submissions);
        }

        private static int GetTotalSubscriberCount(IInformationObject informationObject, ref int CurrMaxSubs, ref int CurrMaxDistinct, Dictionary<string, SubcriptionGraphItem> lookupDictionary)
        {
            string location = informationObject.RelativeLocation;
            //SubscribeSupport.GetSubscriptionDictionary(location, populatedDictionary);

            List<Subscription> result = new List<Subscription>();
            Stack<string> subscriberStack = new Stack<string>();
            SubscribeSupport.GetSubcriptionList(location, result, subscriberStack, lookupDictionary);
            int count = result.Count;
            int distinctCount = result.Select(sub => sub.SubscriberRelativeLocation).Distinct().Count();
            if(result.Count >= CurrMaxSubs || distinctCount >= CurrMaxDistinct)
            {
                if (count > CurrMaxSubs)
                    CurrMaxSubs = count;
                if (distinctCount > CurrMaxDistinct)
                    CurrMaxDistinct = distinctCount;
                Console.WriteLine(count + " / " + distinctCount + " : " + location);
            }
            return count;
        }

        private static void UpdateAccountAndGroups(string accountEmail)
        {
            string emailID = TBREmailRoot.GetIDFromEmailAddress(accountEmail);
            TBREmailRoot emailRoot = TBREmailRoot.RetrieveFromDefaultLocation(emailID);
            TBRAccountRoot accountRoot = TBRAccountRoot.RetrieveFromDefaultLocation(emailRoot.Account.ID);
            foreach(var groupRole in accountRoot.Account.GroupRoleCollection.CollectionContent)
            {
                TBRGroupRoot groupRoot = TBRGroupRoot.RetrieveFromDefaultLocation(groupRole.GroupID);
                RefreshAccountGroupMemberships.Execute(new RefreshAccountGroupMembershipsParameters
                {
                    AccountID = accountRoot.Account.ID,
                    GroupRoot = groupRoot
                });
                InformationContext.ProcessAndClearCurrent();
                InformationContext.Current.InitializeCloudStorageAccess(Properties.Settings.Default.CurrentActiveContainerName);
            }
        }

        private static void RemoveMemberFromGroup(string groupID, string memberEmail)
        {
            AaltoGlobalImpact.OIP.RemoveMemberFromGroup.Execute(new RemoveMemberFromGroupParameters()
                                                                    {
                                                                        GroupID = groupID,
                                                                        EmailAddress = memberEmail
                                                                    });
            InformationContext.ProcessAndClearCurrent();
            InformationContext.Current.InitializeCloudStorageAccess(Properties.Settings.Default.CurrentActiveContainerName);
        }

        private static void RenderAllPagesInWorker()
        {
            RenderWebSupport.RefreshAllAccountAndGroupTemplates(true, "AaltoGlobalImpact.OIP.Blog", "AaltoGlobalImpact.OIP.Activity", "AaltoGlobalImpact.OIP.AddressAndLocation",
                "AaltoGlobalImpact.OIP.Image", "AaltoGlobalImpact.OIP.ImageGroup", "AaltoGlobalImpact.OIP.Category");
        }

        private static void TestSubscriptionChainPick()
        {
            bool result = WorkerSupport.PollAndExecuteChainSubscription();
        }

        private static void PatchSubscriptionsToSubmitted()
        {
            string subscriptionChainLocation = SubscribeSupport.ChainRequestDirectory;
            var submissions =
                StorageSupport.CurrActiveContainer.GetInformationObjects(subscriptionChainLocation, null, 
                                                                         io => io is SubscriptionChainRequestContent).
                    Cast<SubscriptionChainRequestContent>().ToArray();
            foreach(var submission in submissions)
            {
                submission.SubmitTime = DateTime.UtcNow;
                submission.StoreInformation();
            }
        }

        private static void PatchAccountsUpToDateWithRoot()
        {
            var accountIDs = TBRAccountRoot.GetAllAccountIDs();
            foreach (var accountID in accountIDs)
            {
                UpdateAccountRootToReferences.Execute(new UpdateAccountRootToReferencesParameters
                                                          {
                                                              AccountID = accountID
                                                          });
            }
        }

        private static void PatchDefaultValues()
        {
            // TODO: Something to 
            // AccountContainer.AccountModule.Introduction
            // Patch & Fix existing activities, blogs, groups with titles 

        }

        public static bool DoPatching()
        {
            return false;
            Debugger.Break();
            bool skip = false;
            if (skip == false)
                throw new NotSupportedException("Skip this with debugger");

            //InitBlogProfileAndIconOnce();
            PatchSubscriptionsToSubmitted();

            //EnsureAndRefreshMasterCollections();
            //ReconnectAccountsMastersAndCollections();
            //ReconnectGroupsMastersAndCollections();
            //RenderAllPagesInWorker();

            //SyncWwwPublicFromDefaultGroup();
            //AddLegacyGroupWithInitiator("9798daca-afc4-4046-a99b-d0d88bb364e0", "kalle.launiala@citrus.fi");
            //FixGroupMastersAndCollections("9798daca-afc4-4046-a99b-d0d88bb364e0");

            
            //InitBlogAndActivityLocationCollectionsOnce();
            //InitBlogGroupActivityImageGroupCollectionsOnce();

            //ReconnectAccountsMastersAndCollections();
            //ReconnectGroupsMastersAndCollections();
            //EnsureAndRefreshMasterCollections();
            //ConnectMapContainerToCollections();
            //ClearEmptyLocations();


            //RenderAllPagesInWorker();
            //ReportAllSubscriptionCounts();
            //TestWorkerSubscriberChainExecutionPerformance();
            //TestSubscriptionExecution();
            //TestSubscriptionChainPick();
            
            //ExecuteSubscriptionChain(RenderWebSupport.DefaultGroupID);
            //PatchAccountsUpToDateWithRoot();
            //PatchBlogsAndActivitiesSelectedCollections();

            //UpdateAccountAndGroups(accountEmail: "kalle.launiala@citrus.fi");
            //UpdateAccountAndGroups(accountEmail: "kalle.launiala@gmail.com");
            //RemoveMemberFromGroup(groupID: "9798daca-afc4-4046-a99b-d0d88bb364e0",
            //                      memberEmail: "kalle.launiala@gmail.com");

            return true;
        }

        private static void PatchBlogsAndActivitiesSelectedCollections()
        {
            var ownerLocations = GetAllOwnerLocations();
            int totalCount = ownerLocations.Length;
            int currIX = 0;
            foreach (var ownerLocation in ownerLocations)
            {
                Console.WriteLine("Updating number " + (++currIX) + " out of " + totalCount);
                VirtualOwner owner = VirtualOwner.FigureOwner(ownerLocation);
                var informationObjects = StorageSupport.CurrActiveContainer.
                    GetInformationObjects(ownerLocation,
                                          iObj =>
                                          iObj is Blog ||
                                          iObj is Activity).ToArray();
                foreach (var iObj in informationObjects)
                {
                    try
                    {
                        //StorageSupport.StoreInformationMasterFirst(iObj, owner, true);
                        StorageSupport.StoreInformationMasterFirst(iObj, owner, false);
                    }
                    catch (Exception ex)
                    {
                        bool letThrow = false;
                        if (letThrow)
                            throw;
                    }
                }
            }
            InformationContext.ProcessAndClearCurrent();
            InformationContext.Current.InitializeCloudStorageAccess(Properties.Settings.Default.CurrentActiveContainerName);
        }
    }
}