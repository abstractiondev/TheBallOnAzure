using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AaltoGlobalImpact.OIP;
using Microsoft.WindowsAzure.StorageClient;
using TheBall;
using TheBall.CORE;

namespace TheBallTool
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Usage: TheBallTool.exe <web template root directory> <connectionString>");
                }

                //string directory = Directory.GetCurrentDirectory();
                string directory = args[0];
                string connStr = args[1];

                //string connStr = String.Format("DefaultEndpointsProtocol=http;AccountName=theball;AccountKey={0}",
                //                               args[0]);
                //connStr = "UseDevelopmentStorage=true";
                bool debugMode = false;


                StorageSupport.InitializeWithConnectionString(connStr, debugMode);
                InformationContext.InitializeFunctionality(3, true);
                InformationContext.Current.InitializeCloudStorageAccess(Properties.Settings.Default.CurrentActiveContainerName);

                if(DataPatcher.DoPatching())
                    return;

                UploadAccountTemplates(directory);
                //UploadAccountGroupPublicWwwWithCommonFilesUnderRoot(directory);

                Console.WriteLine("Queued sync... (press enter to continue)");
                Console.ReadLine();
            } 
                catch(InvalidDataException ex)
            {
                Console.WriteLine("Error exit: " + ex.ToString());
            }
        }

        private static void UploadAccountTemplates(string directory)
        {
            if (directory.EndsWith("\\") == false)
                directory = directory + "\\";
            string oldDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(directory);
                string[] accountFiles =
                    Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                             .Select(str => str.Substring(directory.Length))
                             .ToArray();
                UploadAccountFiles(accountFiles);
            }
            finally
            {
                Directory.SetCurrentDirectory(oldDir);
            }
            RenderWebSupport.RefreshAllAccountAndGroupTemplates(true);
        }

        /// <summary>
        /// Uploads multiple parallel templates that share common files in root
        /// </summary>
        /// <param name="directory">Root directory containing the account, group, public and www-public templates</param>
        private static void UploadAccountGroupPublicWwwWithCommonFilesUnderRoot(string directory)
        {
            string templateLocation = "livetemplate";
            string privateSiteLocation = "livesite";
            string publicSiteLocation = "livepubsite";
            const string accountNamePart = "oip-account\\";
            const string publicGroupNamePart = "oip-public\\";
            const string groupNamePart = "oip-group\\";
            const string wwwNamePart = "www-public\\";

            if (directory.EndsWith("\\") == false)
                directory = directory + "\\";
            string[] allFiles =
                Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Select(
                    str => str.Substring(directory.Length)).Where(str => str.StartsWith("theball-") == false).ToArray();
            string[] groupTemplates =
                allFiles.Where(
                    file =>
                    file.StartsWith(accountNamePart) == false && file.StartsWith(publicGroupNamePart) == false &&
                    file.StartsWith(wwwNamePart) == false).
                         ToArray();
            string[] publicGroupTemplates =
                allFiles.Where(
                    file =>
                    file.StartsWith(accountNamePart) == false && file.StartsWith(groupNamePart) == false &&
                    file.StartsWith(wwwNamePart) == false).
                         ToArray();
            string[] accountTemplates =
                allFiles.Where(
                    file =>
                    file.StartsWith(groupNamePart) == false && file.StartsWith(publicGroupNamePart) == false &&
                    file.StartsWith(wwwNamePart) == false).
                         ToArray();
            string[] wwwTemplates =
                allFiles.Where(
                    file =>
                    file.StartsWith(groupNamePart) == false && file.StartsWith(publicGroupNamePart) == false &&
                    file.StartsWith(accountNamePart) == false).
                         ToArray();
            UploadAndMoveUnused(accountTemplates, groupTemplates, publicGroupTemplates, wwwTemplates);

            RenderWebSupport.RefreshAllAccountAndGroupTemplates(true, "AaltoGlobalImpact.OIP.Blog",
                                                                "AaltoGlobalImpact.OIP.Activity",
                                                                "AaltoGlobalImpact.OIP.AddressAndLocation",
                                                                "AaltoGlobalImpact.OIP.Image",
                                                                "AaltoGlobalImpact.OIP.ImageGroup",
                                                                "AaltoGlobalImpact.OIP.Category");
        }

        private static void ProcessErrors(bool useWorker)
        {
            // NOTE: Not validated, not used for quite a while... might not work at all right now ++Kalle
            if (useWorker)
            {
                List<QueueEnvelope> envelopes = new List<QueueEnvelope>();
                List<CloudQueueMessage> messages = new List<CloudQueueMessage>();
                CloudQueueMessage message = null;
                QueueEnvelope envelope = ErrorSupport.RetrieveRetryableEnvelope(out message);
                while (envelope != null)
                {
                    //WorkerSupport.ProcessMessage(envelope, false);
                    //QueueSupport.CurrErrorQueue.DeleteMessage(message);
                    messages.Add(message);
                    envelope.CurrentRetryCount++;
                    envelopes.Add(envelope);
                    envelope = ErrorSupport.RetrieveRetryableEnvelope(out message);
                }
                envelopes.ForEach(QueueSupport.PutToDefaultQueue);
                messages.ForEach(msg => QueueSupport.CurrErrorQueue.DeleteMessage(msg));
            }
            else
            {
                CloudQueueMessage message = null;
                QueueEnvelope envelope = ErrorSupport.RetrieveRetryableEnvelope(out message);
                while (envelope != null)
                {
                    WorkerSupport.ProcessMessage(envelope, false);
                    QueueSupport.CurrErrorQueue.DeleteMessage(message);
                    envelope = ErrorSupport.RetrieveRetryableEnvelope(out message);
                }
            }
        }

        private static void UploadAccountFiles(string[] accountTemplates)
        {
            FileSystemSupport.UploadTemplateContent(accountTemplates, TBSystem.CurrSystem,
                                                    RenderWebSupport.DefaultAccountTemplates, true);
        }

        private static void UploadAndMoveUnused(string[] accountTemplates, string[] groupTemplates, string[] publicTemplates, string[] wwwTemplates)
        {
            string[] accountUnusedFiles = null;
            if(accountTemplates != null)
                accountUnusedFiles = FileSystemSupport.UploadTemplateContent(accountTemplates, TBSystem.CurrSystem, RenderWebSupport.DefaultAccountTemplates, true);
            string[] groupUnusedFiles = null;
            if(groupTemplates != null)
                groupUnusedFiles = FileSystemSupport.UploadTemplateContent(groupTemplates, TBSystem.CurrSystem, RenderWebSupport.DefaultGroupTemplates, true);
            string[] publicUnusedFiles = null;
            if(publicTemplates != null)
                publicUnusedFiles = FileSystemSupport.UploadTemplateContent(publicTemplates, TBSystem.CurrSystem, RenderWebSupport.DefaultPublicGroupTemplates, true);
            string[] wwwUnusedFiles = null;
            wwwUnusedFiles = FileSystemSupport.UploadTemplateContent(wwwTemplates, TBSystem.CurrSystem,
                                                                     RenderWebSupport.DefaultPublicWwwTemplates, true);
            if(accountTemplates != null && groupTemplates != null && publicTemplates != null && wwwUnusedFiles != null)
            {
                string[] everyWhereUnusedFiles =
                    accountUnusedFiles.Intersect(groupUnusedFiles).Intersect(publicUnusedFiles).Intersect(wwwUnusedFiles).ToArray();
                //FileSystemSupport.MoveUnusedTxtFiles(everyWhereUnusedFiles);
            }
        }

        private static void ReportInfo(string text)
        {
            Console.WriteLine(text);
        }

    }
}
