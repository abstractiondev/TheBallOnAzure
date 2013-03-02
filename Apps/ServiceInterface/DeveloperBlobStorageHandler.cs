using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Security;
using AaltoGlobalImpact.OIP;
using AzureSupport;
using DotNetOpenAuth.OpenId.RelyingParty;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using TheBall;
using System.Linq;
using TheBall.CORE;

namespace WebInterface
{
    public class DeveloperBlobStorageHandler : IHttpHandler
    {
        private static string DeveloperWebRootFolder
        {
            get
            {
                return ConfigurationManager.AppSettings["DeveloperWebRootFolder"];
            }
        }
        private static string DeveloperBlobContainerName
        {
            get
            {
                return ConfigurationManager.AppSettings["DeveloperBlobContainerName"];
            }
        }
        private static string DeveloperAuthPrefix
        {
            get
            {
                return ConfigurationManager.AppSettings["DeveloperAuthPrefix"];
            }
        }
        private static string DeveloperGroupID
        {
            get
            {
                return ConfigurationManager.AppSettings["DeveloperGroupID"];
            }
        }


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

        private const string AuthDeveloperPrefix = "/dev/";
        private int AuthDeveloperPrefixLen;
        private int GuidIDLen;


        public DeveloperBlobStorageHandler()
        {
            AuthDeveloperPrefixLen = AuthDeveloperPrefix.Length;
            GuidIDLen = Guid.Empty.ToString().Length;
        }


        public void ProcessRequest(HttpContext context)
        {
            string user = context.User.Identity.Name;
            bool isAuthenticated = true;
            var request = context.Request;
            var response = context.Response;
            string devRoot = DeveloperWebRootFolder;
            WebSupport.InitializeContextStorage(context.Request);

            if (isAuthenticated == false)
            {
                return;
            }
            try
            {
                if (request.Path.StartsWith(AuthDeveloperPrefix))
                {
                    HandleDeveloperRequest(context);
                }

            }
            finally
            {
                //InformationContext.ProcessAndClearCurrent();
            }
        }
/*
        private static void ProcessDynamicRegisterRequest(HttpRequest request, HttpResponse response)
        {
            string blobPath = GetBlobPath(request);
            CloudBlob blob = StorageSupport.CurrActiveContainer.GetBlobReference(blobPath);
            response.Clear();
            try
            {
                string template = blob.DownloadText();
                string returnUrl = request.Params["ReturnUrl"];
                TBRegisterContainer registerContainer = GetRegistrationInfo(returnUrl);
                string responseContent = RenderWebSupport.RenderTemplateWithContent(template, registerContainer);
                response.ContentType = blob.Properties.ContentType;
                response.Write(responseContent);
            }
            catch (StorageClientException scEx)
            {
                response.Write(scEx.ToString());
                response.StatusCode = (int)scEx.StatusCode;
            }
            finally
            {
                response.End();
            }
        }
*/
        private static TBRegisterContainer GetRegistrationInfo(string returnUrl)
        {
            TBRegisterContainer registerContainer = TBRegisterContainer.CreateWithLoginProviders(returnUrl, title: "Sign in", subtitle: "... or register", absoluteLoginUrl: null);
            return registerContainer;
        }

        /*
        private static string GetBlobPath(HttpRequest request)
        {
            string contentPath = request.Path;
            if (contentPath.StartsWith(AboutPrefix) == false)
                throw new NotSupportedException("Content path for other than about/ is not supported");
            return contentPath.Substring(1);
        }*/

        private void HandleDeveloperRequest(HttpContext context)
        {
            string requestPath = context.Request.Path;
            string groupID = GetGroupID(context.Request.Path);
            string roleValue = TBCollaboratorRole.CollaboratorRoleValue;
            InformationContext.Current.CurrentGroupRole = roleValue;
            string contentPath;
            if(requestPath.Contains("MediaContent"))
                contentPath = requestPath.Substring(AuthDeveloperPrefixLen);
            else
                contentPath = requestPath.Substring(AuthDeveloperPrefixLen + GuidIDLen + 1);
            VirtualOwner owner = new VirtualOwner(DeveloperAuthPrefix, DeveloperGroupID);
            HandleOwnerRequest(owner, context, contentPath, roleValue);
        }

        private string GetGroupID(string path)
        {
            return path.Substring(AuthDeveloperPrefixLen, GuidIDLen);
        }

        private void HandleOwnerRequest(IContainerOwner containerOwner, HttpContext context, string contentPath, string role)
        {
            if (context.Request.RequestType == "POST")
            {
                // Do first post, and then get to the same URL
                if (TBCollaboratorRole.HasCollaboratorRights(role) == false)
                    throw new SecurityException("Role '" + role + "' is not authorized to do changing POST requests to web interface");
                HandleOwnerPostRequest(containerOwner, context, contentPath);
            }
            HandleOwnerGetRequest(containerOwner, context, contentPath);
        }

        private void HandleOwnerPostRequest(IContainerOwner containerOwner, HttpContext context, string contentPath)
        {
            HttpRequest request = context.Request;
            var form = request.Unvalidated().Form;

            string sourceNamesCommaSeparated = form["RootSourceName"];
            bool isCancelButton = form["btnCancel"] != null;
            if (isCancelButton)
                return;
            string actionName = form["RootSourceAction"];
            string objectFieldID = form["ObjectFieldID"];
            string objectFieldValue = form["Text_Short"];
            if (objectFieldValue == null)
                objectFieldValue = form["Text_Long"];

            CloudBlob webPageBlob = StorageSupport.CurrActiveContainer.GetBlob(contentPath, containerOwner);
            InformationSourceCollection sources = webPageBlob.GetBlobInformationSources();
            if (sources == null)
                throw new InvalidDataException("Postback to page with no information sources defined - where there should be");
            if (sourceNamesCommaSeparated == null)
                sourceNamesCommaSeparated = "";
            sourceNamesCommaSeparated += ",";
            string[] sourceNames = sourceNamesCommaSeparated.Split(',').Distinct().ToArray();

            return;
            if (objectFieldID != null)
            {
                var result = PerformWebAction.Execute(new PerformWebActionParameters
                {
                    CommandName = actionName,
                    FormSourceNames = sourceNames,
                    InformationSources = sources,
                    Owner = containerOwner,
                    TargetObjectID = objectFieldID
                });
                if (result.RenderPageAfterOperation)
                    RenderWebSupport.RefreshContent(webPageBlob);
                return;
            }

            InformationSource[] sourceArray =
                sources.CollectionContent.Where(
                    src => src.IsDynamic || (src.IsInformationObjectSource && sourceNames.Contains(src.SourceName))).ToArray();
            foreach (InformationSource source in sourceArray)
            {
                string oldETag = source.SourceETag;
                IInformationObject rootObject = source.RetrieveInformationObject();
                if (oldETag != rootObject.ETag)
                {
                    RenderWebSupport.RefreshContent(webPageBlob);
                    throw new InvalidDataException("Information under editing was modified during display and save");
                }
                rootObject.SetValuesToObjects(form);
                IAddOperationProvider addOperationProvider = rootObject as IAddOperationProvider;
                bool skipNormalStoreAfterAdd = false;
                if (addOperationProvider != null)
                {
                    skipNormalStoreAfterAdd = addOperationProvider.PerformAddOperation(actionName, sources, contentPath, request.Files);
                }
                if (skipNormalStoreAfterAdd == false)
                {
                    // If not add operation, set media content to stored object
                    foreach (string contentID in request.Files.AllKeys)
                    {
                        HttpPostedFile postedFile = request.Files[contentID];
                        if (String.IsNullOrWhiteSpace(postedFile.FileName))
                            continue;
                        rootObject.SetMediaContent(containerOwner, contentID, postedFile);
                    }
                    rootObject.StoreInformationMasterFirst(containerOwner, false);
                }
            }
            RenderWebSupport.RefreshContent(webPageBlob);
            // Temporary live to pub sync below, to be removed
            //SyncTemplatesToSite(StorageSupport.CurrActiveContainer.Name,
            //    String.Format("grp/f8e1d8c6-0000-467e-b487-74be4ad099cd/{0}/", "livesite"),
            //    StorageSupport.CurrAnonPublicContainer.Name,
            //                    String.Format("grp/default/{0}/", "livepubsite"), true);

        }

        /*
        private void HandleAboutGetRequest(HttpContext context, string contentPath)
        {
            var response = context.Response;
            var request = context.Request;
            string blobPath = GetBlobPath(request);
            CloudBlob blob = StorageSupport.CurrActiveContainer.GetBlobReference(blobPath);  //publicClient.GetBlobReference(blobPath);
            response.Clear();
            try
            {
                blob.FetchAttributes();
                response.ContentType = blob.Properties.ContentType;
                blob.DownloadToStream(response.OutputStream);
            }
            catch (StorageClientException scEx)
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
                    response.StatusCode = (int)scEx.StatusCode;
                }
            }
            finally
            {
                response.End();
            }

        }*/

        Regex defaultViewRegex = new Regex("(?<typename>[^_/]*)_(?<id>[^_]*)_DefaultView.phtml", RegexOptions.Compiled);

        private void HandleOwnerGetRequest(IContainerOwner containerOwner, HttpContext context, string contentPath)
        {
            //CloudBlob blob = StorageSupport.GetOwnerBlobReference(containerOwner, contentPath);

            // Read blob content to response.
            context.Response.Clear();
            try
            {
                string contentType = StorageSupport.GetMimeType(Path.GetExtension(contentPath));
                context.Response.ContentType = contentType;
                string prefixStrippedContent = contentPath; //contentPath.Substring(AuthDeveloperPrefixLen + GuidIDLen + 1);
                string fileName = Path.Combine(DeveloperWebRootFolder, prefixStrippedContent);

                //blob.FetchAttributes();
                var matches = defaultViewRegex.Matches(contentPath);
                if (matches.Count > 0)
                {
                    var match = matches[0];
                    string informationObjectType = match.Groups["typename"].Value;
                    string objectID = match.Groups["id"].Value;
                    Type objectType = typeof(IInformationObject).Assembly.GetType(informationObjectType);
                    MethodInfo retrieveFromDefaultLocation = objectType.GetMethod("RetrieveFromDefaultLocation");
                    IInformationObject iObject = (IInformationObject)retrieveFromDefaultLocation.Invoke(null, new object[] { objectID, containerOwner });
                    string templateName = DefaultViewSupport.GetDefaultStaticTemplateName(iObject);
                    foreach(string groupTemplateDirectory in DefaultViewSupport.FixedGroupSiteLocations)
                    {
                        string filesystemDirectory = Path.Combine(DeveloperWebRootFolder, groupTemplateDirectory);
                        string templateFileName = Path.Combine(filesystemDirectory, templateName);
                        string templateContent;
                        try
                        {
                            templateContent = GetFixedContent(templateFileName);
                        }
                        catch
                        {
                            continue;
                        }
                        string blobDirectory = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(contentPath)),
                                                            groupTemplateDirectory);
                        string templateBlobPath = Path.Combine(blobDirectory, templateName).Replace("\\", "/");
                        var templateBlob = StorageSupport.UploadOwnerBlobText(containerOwner, templateBlobPath,
                                                                              templateContent,
                                                                              StorageSupport.
                                                                                  InformationType_WebTemplateValue);
                    }
                    var blob = DefaultViewSupport.CreateDefaultViewRelativeToRequester(contentPath, iObject, containerOwner);
                    blob.DownloadToStream(context.Response.OutputStream);
                }
                else
                {
                    string fixedContent = GetFixedContent(fileName);
                    if (fixedContent != null)
                    {
                        string templateBlobPath = Path.Combine("template", contentPath).Replace("\\", "/");
                        var templateBlob = StorageSupport.UploadOwnerBlobText(containerOwner, templateBlobPath,
                                                                              fixedContent,
                                                                              StorageSupport.
                                                                                  InformationType_WebTemplateValue);
                        var targetBlob = StorageSupport.GetOwnerBlobReference(containerOwner, contentPath);
                        RenderWebSupport.RenderTemplateWithContentToBlob(templateBlob, targetBlob);
                        targetBlob.DownloadToStream(context.Response.OutputStream);
                        //List<RenderWebSupport.ContentItem> contentRoots = new List<RenderWebSupport.ContentItem>();
                        //string rendered = RenderWebSupport.RenderTemplateWithContentRoots(fixedContent, contentRoots); //RenderWebSupport.Ren
                        //context.Response.Output.Write(rendered);
                    }
                    else
                    {
                        if(File.Exists(fileName))
                        {
                            var fileStream = File.OpenRead(fileName);
                            fileStream.CopyTo(context.Response.OutputStream);
                            fileStream.Close();
                        } else
                        {
                            var targetBlob = StorageSupport.GetOwnerBlobReference(containerOwner, contentPath);
                            targetBlob.DownloadToStream(context.Response.OutputStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.Response.Write(ex.ToString());
            }
            context.Response.End();
        }


        static string GetFixedContent(string fileName)
        {
            List<ErrorItem> errorList = new List<ErrorItem>();
            Dictionary<string, bool> processedDict = new Dictionary<string, bool>();
            string fixedContent = FileSystemSupport.GetFixedContent(fileName, errorList, processedDict);
            return fixedContent;
        }

        #endregion
    }
}
