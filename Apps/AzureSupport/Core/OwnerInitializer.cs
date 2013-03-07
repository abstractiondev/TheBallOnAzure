using System;
using System.Linq;
using System.Reflection;

namespace TheBall.CORE
{
    public static partial class OwnerInitializer
    {
        public static void InitializeAndConnectMastersAndCollections(this IContainerOwner owner)
        {
            Type myType = typeof(OwnerInitializer);
            var myMethods = myType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var myMethod in myMethods.Where(method => method.Name.StartsWith("DOMAININIT_")))
            {
                myMethod.Invoke(null, new object[] { owner });
            }
            owner.ReconnectMastersAndCollectionsForOwner();
        }
    }
}