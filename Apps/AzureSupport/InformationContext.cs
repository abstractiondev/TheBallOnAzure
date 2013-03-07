using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AaltoGlobalImpact.OIP;
using Amazon.IdentityManagement.Model;
using Microsoft.WindowsAzure.StorageClient;
using TheBall.CORE;

namespace TheBall
{
    public class InformationContext
    {
        private const string KEYNAME = "INFOCTX";
        private static ConcurrentDictionary<int, InformationContext> _syncStorage = null;
        public string CurrentGroupRole;

        public static void InitializeFunctionality(int concurrentWorkers, bool allowStatic = false)
        {
            if(_syncStorage != null)
                throw new InvalidOperationException("InformationContext already initialized!");
            _syncStorage = new ConcurrentDictionary<int, InformationContext>(concurrentWorkers, concurrentWorkers);
            AllowStatic = allowStatic;
        }

        public InformationContext()
        {
            FinalizingOperationQueue = new List<OperationRequest>();
            SubscriptionChainTargetsToUpdate = new List<OwnerSubscriptionItem>();
        }

        protected List<OwnerSubscriptionItem> SubscriptionChainTargetsToUpdate { get; private set; }

        public static bool AllowStatic { get; private set; }

        public static InformationContext Current
        {
            get
            {
                var httpContext = HttpContext.Current;
                if(httpContext != null)
                {
                    if (httpContext.Items.Contains(KEYNAME))
                        return (InformationContext) httpContext.Items[KEYNAME];
                    InformationContext informationContext = InformationContext.Create();
                    httpContext.Items.Add(KEYNAME, informationContext);
                    return informationContext;
                }
                var currTaskID = Task.CurrentId;
                if(currTaskID.HasValue || AllowStatic)
                {
                    if(_syncStorage == null)
                        throw new InvalidOperationException("InformationContext not initialized for Task required storage");
                    // Return 0 in case of null, for AllowStatic
                    int currKey = currTaskID.GetValueOrDefault(0);
                    InformationContext informationContext = _syncStorage.GetOrAdd(currKey, CreateWithID);
                    return informationContext;
                }
                throw new NotSupportedException("InformationContext requires either HttpContext.Current, Task.CurrentId or to be available or AllowStatic to be defined");
            }
        }

        public static void ProcessAndClearCurrent()
        {
            Current.PerformFinalizingActions();
            var httpContext = HttpContext.Current;
            if(httpContext != null)
            {
                if(httpContext.Items.Contains(KEYNAME))
                {
                    httpContext.Items.Remove(KEYNAME);
                    return;
                }
            }
            var currTaskID = Task.CurrentId;
            if (currTaskID.HasValue || AllowStatic)
            {
                int currKey = currTaskID.GetValueOrDefault(0);
                InformationContext informationContext;
                bool result = _syncStorage.TryRemove(currKey, out informationContext);
                if (result)
                    return;
            }
            throw new InvalidOperationException("InformationContext ClearCurrent failed - no active context set");
        }

        private static InformationContext CreateWithID(int id)
        {
            InformationContext result = InformationContext.Create();
            result.ID = id;
            return result;
        }

        public int ID { get; private set; }

        private static InformationContext Create()
        {
            return new InformationContext();
        }

        public void AddSubscriptionUpdateTarget(OwnerSubscriptionItem targetLocation)
        {
            SubscriptionChainTargetsToUpdate.Add(targetLocation);
        }

        public void AddOperationRequestToFinalizingQueue(OperationRequest operationRequest)
        {
            FinalizingOperationQueue.Add(operationRequest);
        }

        public void PerformFinalizingActions()
        {
            var grouped = SubscriptionChainTargetsToUpdate.GroupBy(item => item.Owner);
            foreach(var grpItem in grouped)
            {
                SubscribeSupport.AddPendingRequests(grpItem.Key, grpItem.Select(item => item.TargetLocation).ToArray());
            }
            FinalizingOperationQueue.ForEach(oper => QueueSupport.PutToOperationQueue(oper));
        }

        protected List<OperationRequest> FinalizingOperationQueue { get; private set; }

        private string initializedContainerName = null;
        public void InitializeCloudStorageAccess(string containerName)
        {
            if(containerName == null)
                throw new ArgumentNullException("containerName");
            if(containerName == "")
                throw new ArgumentException("Invalid container name", "containerName");
            if(initializedContainerName != null)
            {
                if (containerName == initializedContainerName)
                    return;
                if(containerName != initializedContainerName)
                    throw new NotSupportedException("InitializeCloudStorageAccess already initialized with container name: " 
                        + initializedContainerName + " (tried to initialize with: "
                        + containerName + ")");
            }
            CloudBlobClient blobClient = StorageSupport.CurrStorageAccount.CreateCloudBlobClient();
            blobClient.RetryPolicy = RetryPolicies.Retry(10, TimeSpan.FromMilliseconds(300));
            CurrBlobClient = blobClient;

            var activeContainer = blobClient.GetContainerReference(containerName.ToLower());
            activeContainer.CreateIfNotExist();
            CurrActiveContainer = activeContainer;
            initializedContainerName = containerName;
        }

        private CloudBlobContainer _currActiveContainer;
        public CloudBlobContainer CurrActiveContainer { 
            get
            {
                if(_currActiveContainer == null)
                    throw new NotSupportedException("CurrActiveContainer needs to be initialized with InitializeCloudStorageAccess method");
                return _currActiveContainer;
            }
            private set
            {
                if(_currActiveContainer != null)
                    throw new NotSupportedException("CurrActiveContainer can only be set once");
                _currActiveContainer = value;
            }
        }

        private CloudBlobClient _currBlobClient;
        public bool IsExecutingSubscriptions;

        public CloudBlobClient CurrBlobClient { 
            get
            {
                if(_currBlobClient == null)
                    throw new NotSupportedException(
                        "CurrBlobClient needs to be initialized with InitializeCloudStorageAccess method");
                return _currBlobClient;
            }
            private set
            {
                if(_currBlobClient != null)
                    throw new NotSupportedException("CurrBlobClient can only be set once");
                _currBlobClient = value;
            }
        }

        private IContainerOwner _currentOwner;
        public IContainerOwner CurrentOwner
        {
            get { 
                if (_currentOwner == null)  
                    throw new NotSupportedException("Current owner needs to be initialized before using");
                return _currentOwner;
            }
            set
            {
                if(_currentOwner != null)
                    throw new NotSupportedException("Current owner can only be set once");
                _currentOwner = value;
            }
        }
    }
}