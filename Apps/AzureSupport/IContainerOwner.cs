using System;
using System.Linq;
using AaltoGlobalImpact.OIP;

namespace TheBall.CORE
{
    public interface IContainerOwner
    {
        string ContainerName { get; }
        string LocationPrefix { get; }
    }

    public static partial class ExtIContainerOwner
    {
        public static bool IsAccountContainer(this IContainerOwner owner)
        {
            return owner.ContainerName == "acc";
        }

        public static bool IsGroupContainer(this IContainerOwner owner)
        {
            return owner.ContainerName == "grp" || owner.ContainerName == "dev";
        }

        public static void ReconnectMastersAndCollectionsForOwner(this IContainerOwner owner)
        {
            //string myLocalAccountID = "0c560c69-c3a7-4363-b125-ba1660d21cf4";
            //string acctLoc = "acc/" + myLocalAccountID + "/";

            string ownerLocation = owner.ContainerName + "/" + owner.LocationPrefix + "/";

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
                }
                catch (Exception ex)
                {
                    bool ignoreException = false;
                    if (ignoreException == false)
                        throw;
                }
            }
        }


    }
}