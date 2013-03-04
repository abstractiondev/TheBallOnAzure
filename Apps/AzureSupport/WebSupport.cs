using System.Web;
using TheBall;

namespace AzureSupport
{
    public static class WebSupport
    {
        public static string GetLoginUrl(HttpContext context)
        {
            return context.User.Identity.Name;
        }

        static string GetContainerName(HttpRequest request)
        {
            // For the Demo purposes, this is hardcoded; for multi-site-running on same runtime, this MUST be domain-bound
            return "theballdemo";
            string hostName = request.Url.DnsSafeHost;
            if (hostName == "localhost")
                hostName = "theballdemo.realdomain.org";
            return hostName.Replace('.', '-').ToLower();
        }

        public static void InitializeContextStorage(HttpRequest request)
        {
            string containerName = GetContainerName(request);
            InformationContext.Current.InitializeCloudStorageAccess(containerName);
        }
    }
}