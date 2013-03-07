using System.IO;
using TheBall;
using TheBall.CORE;

namespace TheBall.CORE
{
    public class VirtualOwner : IContainerOwner
    {
        private string containerName;
        private string locationPrefix;

        public static VirtualOwner FigureOwner(IInformationObject ownedObject)
        {
            string relativeLocation = ownedObject.RelativeLocation;
            return FigureOwner(relativeLocation);
        }

        public static VirtualOwner FigureOwner(string relativeLocation)
        {
            if (relativeLocation.StartsWith("acc/") || relativeLocation.StartsWith("grp/"))
                return new VirtualOwner(relativeLocation.Substring(0, 3),
                    relativeLocation.Substring(4, StorageSupport.GuidLength));
            throw new InvalidDataException("Cannot figure owner of: " + relativeLocation);
        }

        public VirtualOwner(string containerName, string locationPrefix)
        {
            this.containerName = containerName;
            this.locationPrefix = locationPrefix;
        }

        public string ContainerName
        {
            get { return containerName; }
        }

        public string LocationPrefix
        {
            get { return locationPrefix; }
        }
    }
}