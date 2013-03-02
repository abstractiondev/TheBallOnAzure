using System;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Web;
using System.Web.Security;
using AaltoGlobalImpact.OIP;
using DotNetOpenAuth.OpenId.RelyingParty;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using TheBall;

namespace WebInterface
{
    public class AnonymousBlobStorageHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string user = context.User.Identity.Name;
            HttpRequest request = context.Request;
            if(request.Path.EndsWith("proxy.cgi") && false)
            {
                ProcessProxyRequest(context);
                return;
            }
            HttpResponse response = context.Response;
            //if(request.Path.StartsWith("/public/") || request.Path.StartsWith("/www-public/"))
            //{
                if (request.Path.EndsWith("/oip-layout-register.phtml"))
                {
                    ProcessDynamicRegisterRequest(request, response);
                    return;
                }

                ProcessAnonymousRequest(request, response);
                return;
            //}
        }

        private void ProcessProxyRequest(HttpContext context)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(context.Request["url"]);
            request.UserAgent = context.Request.UserAgent;
            request.ContentType = context.Request.ContentType;
            request.Method = "POST";

            Stream nstream = request.GetRequestStream();
            byte[] trans = new byte[1024];
            int offset = 0;
            int offcnt = 0;
            while (offset < context.Request.ContentLength)
            {
                offcnt = context.Request.InputStream.Read(trans, offset, 1024);
                if (offcnt > 0)
                {
                    nstream.Write(trans, 0, offcnt);
                    offset += offcnt;
                }
            }
            nstream.Close();

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                context.Response.ContentType = response.ContentType;

                using (Stream receiveStream = response.GetResponseStream())
                {
                    BinaryReader reader = new BinaryReader(receiveStream);
                    BinaryWriter writer = new BinaryWriter(context.Response.OutputStream);
                    int readBytes;
                    do
                    {
                        readBytes = reader.Read(trans, 0, trans.Length);
                        if (readBytes > 0)
                            writer.Write(trans, 0, readBytes);
                    } while (readBytes > 0);
                    writer.Flush();
                    writer.Close();
                    reader.Close();
                }
            }
        }

        private void ProcessAnonymousRequest(HttpRequest request, HttpResponse response)
        {
            CloudBlobClient publicClient = new CloudBlobClient("http://theball.blob.core.windows.net/");
            string blobPath = GetBlobPath(request);
            CloudBlob blob = publicClient.GetBlobReference(blobPath);
            response.Clear();
            try
            {
                blob.FetchAttributes();
                response.ContentType = blob.Properties.ContentType;
                blob.DownloadToStream(response.OutputStream);
            } catch(StorageClientException scEx)
            {
                if (scEx.ErrorCode == StorageErrorCode.BlobNotFound || scEx.ErrorCode == StorageErrorCode.ResourceNotFound || scEx.ErrorCode == StorageErrorCode.BadRequest)
                {
                    response.Write("Blob not found or bad request: " + blob.Name + " (original path: " + request.Path + ")");
                    response.StatusCode = (int)scEx.StatusCode;
                }
                else
                {
                    response.Write("Errorcode: " + scEx.ErrorCode.ToString() + Environment.NewLine);
                    response.Write(scEx.ToString());
                    response.StatusCode = (int) scEx.StatusCode;
                }
            } finally
            {
                response.End();
            }
        }

        private static string GetBlobPath(HttpRequest request)
        {
            string hostName = request.Url.DnsSafeHost;
            if (hostName == "localhost")
            {
                //                hostName = "demopublicoip.aaltoglobalimpact.org";
                hostName = "demowww.aaltoglobalimpact.org";
            }
            if (hostName == "localhost" || hostName == "oip.msunit.citrus.fi")
                return request.Path.Replace("/public/", "pub/");
            string containerName = hostName.Replace('.', '-');
            return containerName + request.Path;
        }

        private static void ProcessDynamicRegisterRequest(HttpRequest request, HttpResponse response)
        {
            CloudBlobClient publicClient = new CloudBlobClient("http://theball.blob.core.windows.net/");
            string blobPath = GetBlobPath(request);
            CloudBlob blob = publicClient.GetBlobReference(blobPath);
            response.Clear();
            try
            {
                string template = blob.DownloadText();
                string returnUrl = request.Params["ReturnUrl"];
                TBRegisterContainer registerContainer = GetRegistrationInfo(returnUrl, request.Url.DnsSafeHost);
                string responseContent = RenderWebSupport.RenderTemplateWithContent(template, registerContainer);
                response.ContentType = blob.Properties.ContentType;
                response.Write(responseContent);
            } catch(StorageClientException scEx)
            {
                response.Write(scEx.ToString());
                response.StatusCode = (int)scEx.StatusCode;
            } finally
            {
                response.End();
            }
        }

        private static TBRegisterContainer GetRegistrationInfo(string returnUrl, string hostName)
        {
            TBRegisterContainer registerContainer = TBRegisterContainer.CreateWithLoginProviders(returnUrl, title: "Sign in", subtitle: "... or register", absoluteLoginUrl:GetAbsoluteLoginUrl(hostName:hostName));
            return registerContainer;
        }

        private static string GetAbsoluteLoginUrl(string hostName)
        {
            switch(hostName)
            {
                case "oip.msunit.citrus.fi":
                    return "http://oip.msunit.citrus.fi/TheBallLogin.aspx";
                case "demopublicoip.aaltoglobalimpact.org":
                    return "http://demooip.aaltoglobalimpact.org/TheBallLogin.aspx";
                case "localhost":
                    return null;
                default:
                    return null;
            }
        }

        #endregion
    }
}
