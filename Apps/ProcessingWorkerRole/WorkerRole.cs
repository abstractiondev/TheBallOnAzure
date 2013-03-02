using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AaltoGlobalImpact.OIP;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using TheBall;

namespace CaloomWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        CloudQueueClient Client;
        bool IsStopped;
        bool GracefullyStopped;
        private CloudQueue CurrQueue;
        private CloudTableClient CurrTable;
        private LocalResource LocalStorageResource;
        private CloudBlobContainer AnonWebContainer;



        public override void Run()
        {
            GracefullyStopped = false;
            //ThreadPool.SetMinThreads(3, 3);
            Task[] tasks = new Task[]
                               {
                                   Task.Factory.StartNew(() => {}), 
                                   Task.Factory.StartNew(() => {}), 
                                   Task.Factory.StartNew(() => {}), 
                                   //Task.Factory.StartNew(() => {}), 
                                   //Task.Factory.StartNew(() => {}), 
                                   //Task.Factory.StartNew(() => {}), 
                               };
            QueueSupport.ReportStatistics("Starting worker: " + CurrWorkerID, TimeSpan.FromDays(1));
            while (!IsStopped)
            {
                try
                {
                    Task.WaitAny(tasks);
                    if (IsStopped)
                        break;
                    int availableIx;
                    Task availableTask = WorkerSupport.GetFirstCompleted(tasks, out availableIx);
                    bool handledSubscriptionChain = PollAndHandleSubscriptionChain(tasks, availableIx, availableTask);
                    if (handledSubscriptionChain)
                    {
                        // TODO: Fix return value check
                        Thread.Sleep(1000);
                        continue;
                    }
                    bool handledMessage = PollAndHandleMessage(tasks, availableIx, availableTask);
                    if (handledMessage)
                        continue;
                    Thread.Sleep(1000);
                }
                catch (AggregateException ae)
                {
                    foreach (var e in ae.Flatten().InnerExceptions)
                    {
                        ErrorSupport.ReportException(e);
                    }
                    Thread.Sleep(10000);
                    // or ...
                    // ae.Flatten().Handle((ex) => ex is MyCustomException);
                }
                    /*
                catch (MessagingException e)
                {
                    if (!e.IsTransient)
                    {
                        Trace.WriteLine(e.Message);
                        throw;
                    }
                    Thread.Sleep(10000);
                }*/
                catch (OperationCanceledException e)
                {
                    if (!IsStopped)
                    {
                        Trace.WriteLine(e.Message);
                        throw;
                    }
                } 
                catch(Exception ex)
                {
                    ErrorSupport.ReportException(ex);
                    throw;
                }
            }
            Task.WaitAll(tasks);
            foreach (var task in tasks.Where(task => task.Exception != null))
            {
                ErrorSupport.ReportException(task.Exception);
            }
            QueueSupport.ReportStatistics("Stopped: " + CurrWorkerID, TimeSpan.FromDays(1));
            GracefullyStopped = true;
        }

        private bool PollAndHandleSubscriptionChain(Task[] tasks, int availableIx, Task availableTask)
        {
            var result = SubscribeSupport.GetOwnerChainsInOrderOfSubmission();
            if (result.Length == 0)
                return false;
            string acquiredEtag = null;
            var firstLockedOwner =
                result.FirstOrDefault(
                    lockCandidate => SubscribeSupport.AcquireChainLock(lockCandidate, out acquiredEtag));
            if (firstLockedOwner == null)
                return false;
            var executing = Task.Factory.StartNew(() => WorkerSupport.ProcessOwnerSubscriptionChains(firstLockedOwner, acquiredEtag, CURRENT_HARDCODED_CONTAINER_NAME));
            tasks[availableIx] = executing;
            if (availableTask.Exception != null)
                ErrorSupport.ReportException(availableTask.Exception);
            return true;
        }


        private bool PollAndHandleMessage(Task[] tasks, int availableIx, Task availableTask)
        {
            CloudQueueMessage message;
            QueueEnvelope envelope = QueueSupport.GetFromDefaultQueue(out message);
            if (envelope != null)
            {
                Task executing = Task.Factory.StartNew(() => WorkerSupport.ProcessMessage(envelope));
                tasks[availableIx] = executing;
                QueueSupport.CurrDefaultQueue.DeleteMessage(message);
                if (availableTask.Exception != null)
                    ErrorSupport.ReportException(availableTask.Exception);
                return true;
            }
            else 
            {
                if(message != null)
                {
                    QueueSupport.CurrDefaultQueue.DeleteMessage(message);
                    ErrorSupport.ReportMessageError(message);
                }
                GC.Collect();
                return false;
            }
        }

        protected string CurrWorkerID { get; private set; }
        public const string CURRENT_HARDCODED_CONTAINER_NAME = "demooip-aaltoglobalimpact-org";

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            CurrWorkerID = DateTime.Now.ToString();
            ServicePointManager.DefaultConnectionLimit = 12;
            ServicePointManager.UseNagleAlgorithm = false;
            string connStr;
            const string ConnStrFileName = @"C:\work\abs\ConnectionStringStorage\theballconnstr.txt";
            if (File.Exists(ConnStrFileName))
                connStr = File.ReadAllText(ConnStrFileName);
            else
                connStr = CloudConfigurationManager.GetSetting("StorageConnectionString");
            StorageSupport.InitializeWithConnectionString(connStr);
            InformationContext.InitializeFunctionality(3, allowStatic:true);
            InformationContext.Current.InitializeCloudStorageAccess(CURRENT_HARDCODED_CONTAINER_NAME);
            CurrQueue = QueueSupport.CurrDefaultQueue;
            IsStopped = false;
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            IsStopped = true;
            while(GracefullyStopped == false)
                Thread.Sleep(1000);
            base.OnStop();
        }
    }
}
