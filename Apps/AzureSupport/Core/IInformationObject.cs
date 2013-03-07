using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace TheBall.CORE
{
    public interface IInformationObject
    {
        Guid OwnerID { get; set; }
        string ID { get; set; }
        string ETag { get; set; }
        string MasterETag { get; set; }
        string RelativeLocation { get; set; }
        string SemanticDomainName { get; set; }
        string Name { get; set; }
        bool IsIndependentMaster { get; }
        void InitializeDefaultSubscribers(IContainerOwner owner);
        void SetValuesToObjects(NameValueCollection form);
        void PostStoringExecute(IContainerOwner owner);
        void PostDeleteExecute(IContainerOwner owner);
        void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName);
        string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName);
        void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent);
        void ReplaceObjectInTree(IInformationObject replacingObject);
        Dictionary<string, List<IInformationObject>> CollectMasterObjects(Predicate<IInformationObject> filterOnFalse = null);
        void CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse = null);
        IInformationObject RetrieveMaster(bool initiateIfMissing);
        IInformationObject RetrieveMaster(bool initiateIfMissing, out bool initiated);
        bool IsInstanceTreeModified { get; }
        void SetInstanceTreeValuesAsUnmodified();
        void UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceInstance);
        void FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly);
        void UpdateCollections(IInformationCollection masterInstance);
    }
}