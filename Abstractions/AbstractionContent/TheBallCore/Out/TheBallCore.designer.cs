 


using DOM=TheBall.CORE;

namespace TheBall.CORE {
	public static partial class OwnerInitializer
	{
		private static void DOMAININIT_TheBall_CORE(IContainerOwner owner)
		{
			DOM.DomainInformationSupport.EnsureMasterCollections(owner);
			DOM.DomainInformationSupport.RefreshMasterCollections(owner);
		}
	}
}


namespace TheBall.CORE { 
		using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.WindowsAzure.StorageClient;
using TheBall;
using TheBall.CORE;



		public static class DomainInformationSupport
		{
            public static void EnsureMasterCollections(IContainerOwner owner)
            {
                {
                    var masterCollection = InvoiceCollection.GetMasterCollectionInstance(owner);
                    if(masterCollection == null)
                    {
                        masterCollection = InvoiceCollection.CreateDefault();
                        masterCollection.RelativeLocation =
                            InvoiceCollection.GetMasterCollectionLocation(owner);
                        StorageSupport.StoreInformation(masterCollection, owner);
                    }
					IInformationCollection collection = masterCollection;
					collection.SubscribeToContentSource();
                }
            }

            public static void RefreshMasterCollections(IContainerOwner owner)
            {
                {
                    IInformationCollection masterCollection = InvoiceCollection.GetMasterCollectionInstance(owner);
                    if (masterCollection == null)
                        throw new InvalidDataException("Master collection InvoiceCollection missing for owner");
                    masterCollection.RefreshContent();
                    StorageSupport.StoreInformation((IInformationObject) masterCollection, owner);
                }
            }
		}
			[DataContract]
			public partial class InvoiceFiscalExportSummary : IInformationObject 
			{
				public InvoiceFiscalExportSummary()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceFiscalExportSummary";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceFiscalExportSummary/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceFiscalExportSummary), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceFiscalExportSummary", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceFiscalExportSummary RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceFiscalExportSummary(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceFiscalExportSummary");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceFiscalExportSummary), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceFiscalExportSummary RetrieveInvoiceFiscalExportSummary(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceFiscalExportSummary) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceFiscalExportSummary), null, owner);
                    return result;
                }

				public static InvoiceFiscalExportSummary RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceFiscalExportSummary.RetrieveInvoiceFiscalExportSummary("Content/TheBall.CORE/InvoiceFiscalExportSummary/" + contentName, containerOwner);
					var result = InvoiceFiscalExportSummary.RetrieveInvoiceFiscalExportSummary("TheBall.CORE/InvoiceFiscalExportSummary/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceFiscalExportSummary/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceFiscalExportSummary/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceFiscalExportSummary) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceFiscalExportSummary));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceFiscalExportSummary DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceFiscalExportSummary));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceFiscalExportSummary) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceFiscalExportSummary", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceFiscalExportSummary", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceFiscalExportSummary customDemoObject);



				public static InvoiceFiscalExportSummary CreateDefault()
				{
					var result = new InvoiceFiscalExportSummary();
					result.ExportedInvoices = InvoiceCollection.CreateDefault();
					return result;
				}

				public static InvoiceFiscalExportSummary CreateDemoDefault()
				{
					InvoiceFiscalExportSummary customDemo = null;
					InvoiceFiscalExportSummary.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceFiscalExportSummary();
					result.ExportedInvoices = InvoiceCollection.CreateDemoDefault();
				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(masterInstance is InvoiceCollection) {
						CollectionUpdateImplementation.Update_InvoiceFiscalExportSummary_ExportedInvoices(this, localCollection:ExportedInvoices, masterCollection:(InvoiceCollection) masterInstance);
					} else if(ExportedInvoices != null) {
						((IInformationObject) ExportedInvoices).UpdateCollections(masterInstance);
					}
				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = ExportedInvoices;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = ExportedInvoices;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) ExportedInvoices;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(FiscalInclusiveStartDate != _unmodified_FiscalInclusiveStartDate)
							return true;
						if(FiscalInclusiveEndDate != _unmodified_FiscalInclusiveEndDate)
							return true;
						if(ExportedInvoices != _unmodified_ExportedInvoices)
							return true;
						{
							IInformationObject item = (IInformationObject) ExportedInvoices;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(ExportedInvoices != null) {
						if(ExportedInvoices.ID == replacingObject.ID)
							ExportedInvoices = (InvoiceCollection) replacingObject;
						else {
							IInformationObject iObject = ExportedInvoices;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(InvoiceFiscalExportSummary sourceObject)
				{
					FiscalInclusiveStartDate = sourceObject.FiscalInclusiveStartDate;
					FiscalInclusiveEndDate = sourceObject.FiscalInclusiveEndDate;
					ExportedInvoices = sourceObject.ExportedInvoices;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_FiscalInclusiveStartDate = FiscalInclusiveStartDate;
					_unmodified_FiscalInclusiveEndDate = FiscalInclusiveEndDate;
				
					_unmodified_ExportedInvoices = ExportedInvoices;
					if(ExportedInvoices != null)
						((IInformationObject) ExportedInvoices).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "FiscalInclusiveStartDate":
							FiscalInclusiveStartDate = DateTime.Parse(value);
							break;
						case "FiscalInclusiveEndDate":
							FiscalInclusiveEndDate = DateTime.Parse(value);
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public DateTime FiscalInclusiveStartDate { get; set; }
			private DateTime _unmodified_FiscalInclusiveStartDate;
			[DataMember]
			public DateTime FiscalInclusiveEndDate { get; set; }
			private DateTime _unmodified_FiscalInclusiveEndDate;
			[DataMember]
			public InvoiceCollection ExportedInvoices { get; set; }
			private InvoiceCollection _unmodified_ExportedInvoices;
			
			}
			[DataContract]
			public partial class InvoiceSummaryContainer : IInformationObject 
			{
				public InvoiceSummaryContainer()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceSummaryContainer";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceSummaryContainer/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceSummaryContainer), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceSummaryContainer", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceSummaryContainer RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceSummaryContainer(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceSummaryContainer");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceSummaryContainer), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceSummaryContainer RetrieveInvoiceSummaryContainer(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceSummaryContainer) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceSummaryContainer), null, owner);
                    return result;
                }

				public static InvoiceSummaryContainer RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceSummaryContainer.RetrieveInvoiceSummaryContainer("Content/TheBall.CORE/InvoiceSummaryContainer/" + contentName, containerOwner);
					var result = InvoiceSummaryContainer.RetrieveInvoiceSummaryContainer("TheBall.CORE/InvoiceSummaryContainer/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceSummaryContainer/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceSummaryContainer/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceSummaryContainer) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceSummaryContainer));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceSummaryContainer DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceSummaryContainer));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceSummaryContainer) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceSummaryContainer", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceSummaryContainer", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceSummaryContainer customDemoObject);



				public static InvoiceSummaryContainer CreateDefault()
				{
					var result = new InvoiceSummaryContainer();
					result.OpenInvoices = InvoiceCollection.CreateDefault();
					result.PredictedInvoices = InvoiceCollection.CreateDefault();
					result.PaidInvoicesActiveYear = InvoiceCollection.CreateDefault();
					result.PaidInvoicesLast12Months = InvoiceCollection.CreateDefault();
					return result;
				}

				public static InvoiceSummaryContainer CreateDemoDefault()
				{
					InvoiceSummaryContainer customDemo = null;
					InvoiceSummaryContainer.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceSummaryContainer();
					result.OpenInvoices = InvoiceCollection.CreateDemoDefault();
					result.PredictedInvoices = InvoiceCollection.CreateDemoDefault();
					result.PaidInvoicesActiveYear = InvoiceCollection.CreateDemoDefault();
					result.PaidInvoicesLast12Months = InvoiceCollection.CreateDemoDefault();
				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(masterInstance is InvoiceCollection) {
						CollectionUpdateImplementation.Update_InvoiceSummaryContainer_OpenInvoices(this, localCollection:OpenInvoices, masterCollection:(InvoiceCollection) masterInstance);
					} else if(OpenInvoices != null) {
						((IInformationObject) OpenInvoices).UpdateCollections(masterInstance);
					}
					if(masterInstance is InvoiceCollection) {
						CollectionUpdateImplementation.Update_InvoiceSummaryContainer_PredictedInvoices(this, localCollection:PredictedInvoices, masterCollection:(InvoiceCollection) masterInstance);
					} else if(PredictedInvoices != null) {
						((IInformationObject) PredictedInvoices).UpdateCollections(masterInstance);
					}
					if(masterInstance is InvoiceCollection) {
						CollectionUpdateImplementation.Update_InvoiceSummaryContainer_PaidInvoicesActiveYear(this, localCollection:PaidInvoicesActiveYear, masterCollection:(InvoiceCollection) masterInstance);
					} else if(PaidInvoicesActiveYear != null) {
						((IInformationObject) PaidInvoicesActiveYear).UpdateCollections(masterInstance);
					}
					if(masterInstance is InvoiceCollection) {
						CollectionUpdateImplementation.Update_InvoiceSummaryContainer_PaidInvoicesLast12Months(this, localCollection:PaidInvoicesLast12Months, masterCollection:(InvoiceCollection) masterInstance);
					} else if(PaidInvoicesLast12Months != null) {
						((IInformationObject) PaidInvoicesLast12Months).UpdateCollections(masterInstance);
					}
				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = OpenInvoices;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					{ // Scoping block for variable name reusability
						IInformationObject item = PredictedInvoices;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					{ // Scoping block for variable name reusability
						IInformationObject item = PaidInvoicesActiveYear;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					{ // Scoping block for variable name reusability
						IInformationObject item = PaidInvoicesLast12Months;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = OpenInvoices;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					{
						var item = PredictedInvoices;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					{
						var item = PaidInvoicesActiveYear;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					{
						var item = PaidInvoicesLast12Months;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) OpenInvoices;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
					{
						var item = (IInformationObject) PredictedInvoices;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
					{
						var item = (IInformationObject) PaidInvoicesActiveYear;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
					{
						var item = (IInformationObject) PaidInvoicesLast12Months;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(OpenInvoices != _unmodified_OpenInvoices)
							return true;
						if(PredictedInvoices != _unmodified_PredictedInvoices)
							return true;
						if(PaidInvoicesActiveYear != _unmodified_PaidInvoicesActiveYear)
							return true;
						if(PaidInvoicesLast12Months != _unmodified_PaidInvoicesLast12Months)
							return true;
						{
							IInformationObject item = (IInformationObject) OpenInvoices;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
						{
							IInformationObject item = (IInformationObject) PredictedInvoices;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
						{
							IInformationObject item = (IInformationObject) PaidInvoicesActiveYear;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
						{
							IInformationObject item = (IInformationObject) PaidInvoicesLast12Months;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(OpenInvoices != null) {
						if(OpenInvoices.ID == replacingObject.ID)
							OpenInvoices = (InvoiceCollection) replacingObject;
						else {
							IInformationObject iObject = OpenInvoices;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
					if(PredictedInvoices != null) {
						if(PredictedInvoices.ID == replacingObject.ID)
							PredictedInvoices = (InvoiceCollection) replacingObject;
						else {
							IInformationObject iObject = PredictedInvoices;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
					if(PaidInvoicesActiveYear != null) {
						if(PaidInvoicesActiveYear.ID == replacingObject.ID)
							PaidInvoicesActiveYear = (InvoiceCollection) replacingObject;
						else {
							IInformationObject iObject = PaidInvoicesActiveYear;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
					if(PaidInvoicesLast12Months != null) {
						if(PaidInvoicesLast12Months.ID == replacingObject.ID)
							PaidInvoicesLast12Months = (InvoiceCollection) replacingObject;
						else {
							IInformationObject iObject = PaidInvoicesLast12Months;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(InvoiceSummaryContainer sourceObject)
				{
					OpenInvoices = sourceObject.OpenInvoices;
					PredictedInvoices = sourceObject.PredictedInvoices;
					PaidInvoicesActiveYear = sourceObject.PaidInvoicesActiveYear;
					PaidInvoicesLast12Months = sourceObject.PaidInvoicesLast12Months;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
				
					_unmodified_OpenInvoices = OpenInvoices;
					if(OpenInvoices != null)
						((IInformationObject) OpenInvoices).SetInstanceTreeValuesAsUnmodified();

					_unmodified_PredictedInvoices = PredictedInvoices;
					if(PredictedInvoices != null)
						((IInformationObject) PredictedInvoices).SetInstanceTreeValuesAsUnmodified();

					_unmodified_PaidInvoicesActiveYear = PaidInvoicesActiveYear;
					if(PaidInvoicesActiveYear != null)
						((IInformationObject) PaidInvoicesActiveYear).SetInstanceTreeValuesAsUnmodified();

					_unmodified_PaidInvoicesLast12Months = PaidInvoicesLast12Months;
					if(PaidInvoicesLast12Months != null)
						((IInformationObject) PaidInvoicesLast12Months).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public InvoiceCollection OpenInvoices { get; set; }
			private InvoiceCollection _unmodified_OpenInvoices;
			[DataMember]
			public InvoiceCollection PredictedInvoices { get; set; }
			private InvoiceCollection _unmodified_PredictedInvoices;
			[DataMember]
			public InvoiceCollection PaidInvoicesActiveYear { get; set; }
			private InvoiceCollection _unmodified_PaidInvoicesActiveYear;
			[DataMember]
			public InvoiceCollection PaidInvoicesLast12Months { get; set; }
			private InvoiceCollection _unmodified_PaidInvoicesLast12Months;
			
			}
			[DataContract]
			public partial class InvoiceCollection : IInformationObject , IInformationCollection
			{
				public InvoiceCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceCollection RetrieveInvoiceCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceCollection), null, owner);
                    return result;
                }

				public static InvoiceCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceCollection.RetrieveInvoiceCollection("Content/TheBall.CORE/InvoiceCollection/" + contentName, containerOwner);
					var result = InvoiceCollection.RetrieveInvoiceCollection("TheBall.CORE/InvoiceCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return true;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					return GetMasterCollectionLocation(owner);
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					return GetMasterCollectionInstance(owner);
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = Invoice.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
					// DirectoryToMaster
					string itemDirectory = GetItemDirectory();
					IInformationObject[] informationObjects = StorageSupport.RetrieveInformationObjects(itemDirectory,
																								 typeof(Invoice));
                    Array.ForEach(informationObjects, io => io.MasterETag = io.ETag);
					CollectionContent.Clear();
					CollectionContent.AddRange(informationObjects.Select(obj => (Invoice) obj));
            
				}

				public static InvoiceCollection GetMasterCollectionInstance(IContainerOwner owner)
				{
					return InvoiceCollection.RetrieveFromOwnerContent(owner, "MasterCollection");
				}

				public void SubscribeToContentSource()
				{
					// DirectoryToCollection
					string itemDirectory = GetItemDirectory();
					SubscribeSupport.AddSubscriptionToObject(itemDirectory, RelativeLocation,
															 SubscribeSupport.SubscribeType_DirectoryToCollection, null, typeof(InvoiceCollection).FullName);
				}

				public static string GetMasterCollectionLocation(IContainerOwner owner)
				{
					return StorageSupport.GetBlobOwnerAddress(owner, "TheBall.CORE/InvoiceCollection/" + "MasterCollection");
				}



                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static InvoiceCollection CreateDefault()
				{
					var result = new InvoiceCollection();
					return result;
				}

				public static InvoiceCollection CreateDemoDefault()
				{
					InvoiceCollection customDemo = null;
					InvoiceCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceCollection();
					result.CollectionContent.Add(Invoice.CreateDemoDefault());
					//result.CollectionContent.Add(Invoice.CreateDemoDefault());
					//result.CollectionContent.Add(Invoice.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<Invoice> CollectionContent = new List<Invoice>();
				private Invoice[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public Invoice[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (Invoice )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(InvoiceCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class Invoice : IInformationObject 
			{
				public Invoice()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "Invoice";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/Invoice/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(Invoice), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "Invoice", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static Invoice RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoice(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: Invoice");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(Invoice), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static Invoice RetrieveInvoice(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (Invoice) StorageSupport.RetrieveInformation(relativeLocation, typeof(Invoice), null, owner);
                    return result;
                }

				public static Invoice RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = Invoice.RetrieveInvoice("Content/TheBall.CORE/Invoice/" + contentName, containerOwner);
					var result = Invoice.RetrieveInvoice("TheBall.CORE/Invoice/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/Invoice/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/Invoice/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((Invoice) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(Invoice));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static Invoice DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(Invoice));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (Invoice) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "Invoice", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "Invoice", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref Invoice customDemoObject);



				public static Invoice CreateDefault()
				{
					var result = new Invoice();
					result.ReferenceToInformation = ReferenceToInformation.CreateDefault();
					result.InvoiceDetails = InvoiceDetails.CreateDefault();
					result.InvoiceUsers = InvoiceUserCollection.CreateDefault();
					return result;
				}

				public static Invoice CreateDemoDefault()
				{
					Invoice customDemo = null;
					Invoice.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new Invoice();
					result.ReferenceToInformation = ReferenceToInformation.CreateDemoDefault();
					result.InvoiceName = @"Invoice.InvoiceName";

					result.InvoiceID = @"Invoice.InvoiceID";

					result.InvoicedAmount = @"Invoice.InvoicedAmount";

					result.PaidAmount = @"Invoice.PaidAmount";

					result.FeesAndInterestAmount = @"Invoice.FeesAndInterestAmount";

					result.UnpaidAmount = @"Invoice.UnpaidAmount";

					result.InvoiceDetails = InvoiceDetails.CreateDemoDefault();
					result.InvoiceUsers = InvoiceUserCollection.CreateDemoDefault();
				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(InvoiceDetails != null) {
						((IInformationObject) InvoiceDetails).UpdateCollections(masterInstance);
					}

					if(InvoiceUsers != null) {
						((IInformationObject) InvoiceUsers).UpdateCollections(masterInstance);
					}

				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = InvoiceDetails;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					{ // Scoping block for variable name reusability
						IInformationObject item = InvoiceUsers;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
						{
							IInformationObject item = ReferenceToInformation;
							if(item != null)
							{
								item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
							}
						}
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = ReferenceToInformation;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					{
						var item = InvoiceDetails;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					{
						var item = InvoiceUsers;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) ReferenceToInformation;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
					{
						var item = (IInformationObject) InvoiceDetails;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
					{
						var item = (IInformationObject) InvoiceUsers;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(ReferenceToInformation != _unmodified_ReferenceToInformation)
							return true;
						if(InvoiceName != _unmodified_InvoiceName)
							return true;
						if(InvoiceID != _unmodified_InvoiceID)
							return true;
						if(InvoicedAmount != _unmodified_InvoicedAmount)
							return true;
						if(CreateDate != _unmodified_CreateDate)
							return true;
						if(DueDate != _unmodified_DueDate)
							return true;
						if(PaidAmount != _unmodified_PaidAmount)
							return true;
						if(FeesAndInterestAmount != _unmodified_FeesAndInterestAmount)
							return true;
						if(UnpaidAmount != _unmodified_UnpaidAmount)
							return true;
						if(InvoiceDetails != _unmodified_InvoiceDetails)
							return true;
						if(InvoiceUsers != _unmodified_InvoiceUsers)
							return true;
						{
							IInformationObject item = (IInformationObject) InvoiceDetails;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
						{
							IInformationObject item = (IInformationObject) InvoiceUsers;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(ReferenceToInformation != null) {
						if(ReferenceToInformation.ID == replacingObject.ID)
							ReferenceToInformation = (ReferenceToInformation) replacingObject;
						else {
							IInformationObject iObject = ReferenceToInformation;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
					if(InvoiceDetails != null) {
						if(InvoiceDetails.ID == replacingObject.ID)
							InvoiceDetails = (InvoiceDetails) replacingObject;
						else {
							IInformationObject iObject = InvoiceDetails;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
					if(InvoiceUsers != null) {
						if(InvoiceUsers.ID == replacingObject.ID)
							InvoiceUsers = (InvoiceUserCollection) replacingObject;
						else {
							IInformationObject iObject = InvoiceUsers;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(Invoice sourceObject)
				{
					ReferenceToInformation = sourceObject.ReferenceToInformation;
					InvoiceName = sourceObject.InvoiceName;
					InvoiceID = sourceObject.InvoiceID;
					InvoicedAmount = sourceObject.InvoicedAmount;
					CreateDate = sourceObject.CreateDate;
					DueDate = sourceObject.DueDate;
					PaidAmount = sourceObject.PaidAmount;
					FeesAndInterestAmount = sourceObject.FeesAndInterestAmount;
					UnpaidAmount = sourceObject.UnpaidAmount;
					InvoiceDetails = sourceObject.InvoiceDetails;
					InvoiceUsers = sourceObject.InvoiceUsers;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_InvoiceName = InvoiceName;
					_unmodified_InvoiceID = InvoiceID;
					_unmodified_InvoicedAmount = InvoicedAmount;
					_unmodified_CreateDate = CreateDate;
					_unmodified_DueDate = DueDate;
					_unmodified_PaidAmount = PaidAmount;
					_unmodified_FeesAndInterestAmount = FeesAndInterestAmount;
					_unmodified_UnpaidAmount = UnpaidAmount;
				
					_unmodified_ReferenceToInformation = ReferenceToInformation;
					if(ReferenceToInformation != null)
						((IInformationObject) ReferenceToInformation).SetInstanceTreeValuesAsUnmodified();

					_unmodified_InvoiceDetails = InvoiceDetails;
					if(InvoiceDetails != null)
						((IInformationObject) InvoiceDetails).SetInstanceTreeValuesAsUnmodified();

					_unmodified_InvoiceUsers = InvoiceUsers;
					if(InvoiceUsers != null)
						((IInformationObject) InvoiceUsers).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "InvoiceName":
							InvoiceName = value;
							break;
						case "InvoiceID":
							InvoiceID = value;
							break;
						case "InvoicedAmount":
							InvoicedAmount = value;
							break;
						case "CreateDate":
							CreateDate = DateTime.Parse(value);
							break;
						case "DueDate":
							DueDate = DateTime.Parse(value);
							break;
						case "PaidAmount":
							PaidAmount = value;
							break;
						case "FeesAndInterestAmount":
							FeesAndInterestAmount = value;
							break;
						case "UnpaidAmount":
							UnpaidAmount = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public ReferenceToInformation ReferenceToInformation { get; set; }
			private ReferenceToInformation _unmodified_ReferenceToInformation;
			[DataMember]
			public string InvoiceName { get; set; }
			private string _unmodified_InvoiceName;
			[DataMember]
			public string InvoiceID { get; set; }
			private string _unmodified_InvoiceID;
			[DataMember]
			public string InvoicedAmount { get; set; }
			private string _unmodified_InvoicedAmount;
			[DataMember]
			public DateTime CreateDate { get; set; }
			private DateTime _unmodified_CreateDate;
			[DataMember]
			public DateTime DueDate { get; set; }
			private DateTime _unmodified_DueDate;
			[DataMember]
			public string PaidAmount { get; set; }
			private string _unmodified_PaidAmount;
			[DataMember]
			public string FeesAndInterestAmount { get; set; }
			private string _unmodified_FeesAndInterestAmount;
			[DataMember]
			public string UnpaidAmount { get; set; }
			private string _unmodified_UnpaidAmount;
			[DataMember]
			public InvoiceDetails InvoiceDetails { get; set; }
			private InvoiceDetails _unmodified_InvoiceDetails;
			[DataMember]
			public InvoiceUserCollection InvoiceUsers { get; set; }
			private InvoiceUserCollection _unmodified_InvoiceUsers;
			
			}
			[DataContract]
			public partial class InvoiceDetails : IInformationObject 
			{
				public InvoiceDetails()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceDetails";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceDetails/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceDetails), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceDetails", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceDetails RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceDetails(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceDetails");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceDetails), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceDetails RetrieveInvoiceDetails(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceDetails) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceDetails), null, owner);
                    return result;
                }

				public static InvoiceDetails RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceDetails.RetrieveInvoiceDetails("Content/TheBall.CORE/InvoiceDetails/" + contentName, containerOwner);
					var result = InvoiceDetails.RetrieveInvoiceDetails("TheBall.CORE/InvoiceDetails/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceDetails/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceDetails/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceDetails) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceDetails));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceDetails DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceDetails));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceDetails) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceDetails", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceDetails", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceDetails customDemoObject);



				public static InvoiceDetails CreateDefault()
				{
					var result = new InvoiceDetails();
					return result;
				}

				public static InvoiceDetails CreateDemoDefault()
				{
					InvoiceDetails customDemo = null;
					InvoiceDetails.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceDetails();
					result.MonthlyFeesTotal = @"InvoiceDetails.MonthlyFeesTotal";

					result.OneTimeFeesTotal = @"InvoiceDetails.OneTimeFeesTotal";

					result.UsageFeesTotal = @"InvoiceDetails.UsageFeesTotal";

					result.InterestFeesTotal = @"InvoiceDetails.InterestFeesTotal";

					result.PenaltyFeesTotal = @"InvoiceDetails.PenaltyFeesTotal";

					result.TotalFeesTotal = @"InvoiceDetails.TotalFeesTotal";

				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(MonthlyFeesTotal != _unmodified_MonthlyFeesTotal)
							return true;
						if(OneTimeFeesTotal != _unmodified_OneTimeFeesTotal)
							return true;
						if(UsageFeesTotal != _unmodified_UsageFeesTotal)
							return true;
						if(InterestFeesTotal != _unmodified_InterestFeesTotal)
							return true;
						if(PenaltyFeesTotal != _unmodified_PenaltyFeesTotal)
							return true;
						if(TotalFeesTotal != _unmodified_TotalFeesTotal)
							return true;
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
				}


				private void CopyContentFrom(InvoiceDetails sourceObject)
				{
					MonthlyFeesTotal = sourceObject.MonthlyFeesTotal;
					OneTimeFeesTotal = sourceObject.OneTimeFeesTotal;
					UsageFeesTotal = sourceObject.UsageFeesTotal;
					InterestFeesTotal = sourceObject.InterestFeesTotal;
					PenaltyFeesTotal = sourceObject.PenaltyFeesTotal;
					TotalFeesTotal = sourceObject.TotalFeesTotal;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_MonthlyFeesTotal = MonthlyFeesTotal;
					_unmodified_OneTimeFeesTotal = OneTimeFeesTotal;
					_unmodified_UsageFeesTotal = UsageFeesTotal;
					_unmodified_InterestFeesTotal = InterestFeesTotal;
					_unmodified_PenaltyFeesTotal = PenaltyFeesTotal;
					_unmodified_TotalFeesTotal = TotalFeesTotal;
				
				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "MonthlyFeesTotal":
							MonthlyFeesTotal = value;
							break;
						case "OneTimeFeesTotal":
							OneTimeFeesTotal = value;
							break;
						case "UsageFeesTotal":
							UsageFeesTotal = value;
							break;
						case "InterestFeesTotal":
							InterestFeesTotal = value;
							break;
						case "PenaltyFeesTotal":
							PenaltyFeesTotal = value;
							break;
						case "TotalFeesTotal":
							TotalFeesTotal = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public string MonthlyFeesTotal { get; set; }
			private string _unmodified_MonthlyFeesTotal;
			[DataMember]
			public string OneTimeFeesTotal { get; set; }
			private string _unmodified_OneTimeFeesTotal;
			[DataMember]
			public string UsageFeesTotal { get; set; }
			private string _unmodified_UsageFeesTotal;
			[DataMember]
			public string InterestFeesTotal { get; set; }
			private string _unmodified_InterestFeesTotal;
			[DataMember]
			public string PenaltyFeesTotal { get; set; }
			private string _unmodified_PenaltyFeesTotal;
			[DataMember]
			public string TotalFeesTotal { get; set; }
			private string _unmodified_TotalFeesTotal;
			
			}
			[DataContract]
			public partial class InvoiceUserCollection : IInformationObject , IInformationCollection
			{
				public InvoiceUserCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceUserCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceUserCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceUserCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceUserCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceUserCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceUserCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceUserCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceUserCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceUserCollection RetrieveInvoiceUserCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceUserCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceUserCollection), null, owner);
                    return result;
                }

				public static InvoiceUserCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceUserCollection.RetrieveInvoiceUserCollection("Content/TheBall.CORE/InvoiceUserCollection/" + contentName, containerOwner);
					var result = InvoiceUserCollection.RetrieveInvoiceUserCollection("TheBall.CORE/InvoiceUserCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceUserCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceUserCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceUserCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceUserCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceUserCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceUserCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceUserCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceUserCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceUserCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceUserCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = InvoiceUser.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static InvoiceUserCollection CreateDefault()
				{
					var result = new InvoiceUserCollection();
					return result;
				}

				public static InvoiceUserCollection CreateDemoDefault()
				{
					InvoiceUserCollection customDemo = null;
					InvoiceUserCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceUserCollection();
					result.CollectionContent.Add(InvoiceUser.CreateDemoDefault());
					//result.CollectionContent.Add(InvoiceUser.CreateDemoDefault());
					//result.CollectionContent.Add(InvoiceUser.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<InvoiceUser> CollectionContent = new List<InvoiceUser>();
				private InvoiceUser[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public InvoiceUser[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (InvoiceUser )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(InvoiceUserCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class InvoiceUser : IInformationObject 
			{
				public InvoiceUser()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceUser";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceUser/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceUser), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceUser", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceUser RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceUser(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceUser");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceUser), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceUser RetrieveInvoiceUser(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceUser) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceUser), null, owner);
                    return result;
                }

				public static InvoiceUser RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceUser.RetrieveInvoiceUser("Content/TheBall.CORE/InvoiceUser/" + contentName, containerOwner);
					var result = InvoiceUser.RetrieveInvoiceUser("TheBall.CORE/InvoiceUser/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceUser/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceUser/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceUser) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceUser));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceUser DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceUser));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceUser) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceUser", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceUser", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceUser customDemoObject);



				public static InvoiceUser CreateDefault()
				{
					var result = new InvoiceUser();
					result.InvoiceRowGroupCollection = InvoiceRowGroupCollection.CreateDefault();
					result.InvoiceEventDetailGroupCollection = InvoiceEventDetailGroupCollection.CreateDefault();
					return result;
				}

				public static InvoiceUser CreateDemoDefault()
				{
					InvoiceUser customDemo = null;
					InvoiceUser.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceUser();
					result.UserName = @"InvoiceUser.UserName";

					result.UserID = @"InvoiceUser.UserID";

					result.UserPhoneNumber = @"InvoiceUser.UserPhoneNumber";

					result.UserSubscriptionNumber = @"InvoiceUser.UserSubscriptionNumber";

					result.UserInvoiceTotalAmount = @"InvoiceUser.UserInvoiceTotalAmount";

					result.InvoiceRowGroupCollection = InvoiceRowGroupCollection.CreateDemoDefault();
					result.InvoiceEventDetailGroupCollection = InvoiceEventDetailGroupCollection.CreateDemoDefault();
				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(InvoiceRowGroupCollection != null) {
						((IInformationObject) InvoiceRowGroupCollection).UpdateCollections(masterInstance);
					}

					if(InvoiceEventDetailGroupCollection != null) {
						((IInformationObject) InvoiceEventDetailGroupCollection).UpdateCollections(masterInstance);
					}

				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = InvoiceRowGroupCollection;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					{ // Scoping block for variable name reusability
						IInformationObject item = InvoiceEventDetailGroupCollection;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = InvoiceRowGroupCollection;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					{
						var item = InvoiceEventDetailGroupCollection;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) InvoiceRowGroupCollection;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
					{
						var item = (IInformationObject) InvoiceEventDetailGroupCollection;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(UserName != _unmodified_UserName)
							return true;
						if(UserID != _unmodified_UserID)
							return true;
						if(UserPhoneNumber != _unmodified_UserPhoneNumber)
							return true;
						if(UserSubscriptionNumber != _unmodified_UserSubscriptionNumber)
							return true;
						if(UserInvoiceTotalAmount != _unmodified_UserInvoiceTotalAmount)
							return true;
						if(InvoiceRowGroupCollection != _unmodified_InvoiceRowGroupCollection)
							return true;
						if(InvoiceEventDetailGroupCollection != _unmodified_InvoiceEventDetailGroupCollection)
							return true;
						{
							IInformationObject item = (IInformationObject) InvoiceRowGroupCollection;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
						{
							IInformationObject item = (IInformationObject) InvoiceEventDetailGroupCollection;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(InvoiceRowGroupCollection != null) {
						if(InvoiceRowGroupCollection.ID == replacingObject.ID)
							InvoiceRowGroupCollection = (InvoiceRowGroupCollection) replacingObject;
						else {
							IInformationObject iObject = InvoiceRowGroupCollection;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
					if(InvoiceEventDetailGroupCollection != null) {
						if(InvoiceEventDetailGroupCollection.ID == replacingObject.ID)
							InvoiceEventDetailGroupCollection = (InvoiceEventDetailGroupCollection) replacingObject;
						else {
							IInformationObject iObject = InvoiceEventDetailGroupCollection;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(InvoiceUser sourceObject)
				{
					UserName = sourceObject.UserName;
					UserID = sourceObject.UserID;
					UserPhoneNumber = sourceObject.UserPhoneNumber;
					UserSubscriptionNumber = sourceObject.UserSubscriptionNumber;
					UserInvoiceTotalAmount = sourceObject.UserInvoiceTotalAmount;
					InvoiceRowGroupCollection = sourceObject.InvoiceRowGroupCollection;
					InvoiceEventDetailGroupCollection = sourceObject.InvoiceEventDetailGroupCollection;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_UserName = UserName;
					_unmodified_UserID = UserID;
					_unmodified_UserPhoneNumber = UserPhoneNumber;
					_unmodified_UserSubscriptionNumber = UserSubscriptionNumber;
					_unmodified_UserInvoiceTotalAmount = UserInvoiceTotalAmount;
				
					_unmodified_InvoiceRowGroupCollection = InvoiceRowGroupCollection;
					if(InvoiceRowGroupCollection != null)
						((IInformationObject) InvoiceRowGroupCollection).SetInstanceTreeValuesAsUnmodified();

					_unmodified_InvoiceEventDetailGroupCollection = InvoiceEventDetailGroupCollection;
					if(InvoiceEventDetailGroupCollection != null)
						((IInformationObject) InvoiceEventDetailGroupCollection).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "UserName":
							UserName = value;
							break;
						case "UserID":
							UserID = value;
							break;
						case "UserPhoneNumber":
							UserPhoneNumber = value;
							break;
						case "UserSubscriptionNumber":
							UserSubscriptionNumber = value;
							break;
						case "UserInvoiceTotalAmount":
							UserInvoiceTotalAmount = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public string UserName { get; set; }
			private string _unmodified_UserName;
			[DataMember]
			public string UserID { get; set; }
			private string _unmodified_UserID;
			[DataMember]
			public string UserPhoneNumber { get; set; }
			private string _unmodified_UserPhoneNumber;
			[DataMember]
			public string UserSubscriptionNumber { get; set; }
			private string _unmodified_UserSubscriptionNumber;
			[DataMember]
			public string UserInvoiceTotalAmount { get; set; }
			private string _unmodified_UserInvoiceTotalAmount;
			[DataMember]
			public InvoiceRowGroupCollection InvoiceRowGroupCollection { get; set; }
			private InvoiceRowGroupCollection _unmodified_InvoiceRowGroupCollection;
			[DataMember]
			public InvoiceEventDetailGroupCollection InvoiceEventDetailGroupCollection { get; set; }
			private InvoiceEventDetailGroupCollection _unmodified_InvoiceEventDetailGroupCollection;
			
			}
			[DataContract]
			public partial class InvoiceRowGroupCollection : IInformationObject , IInformationCollection
			{
				public InvoiceRowGroupCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceRowGroupCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceRowGroupCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceRowGroupCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceRowGroupCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceRowGroupCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceRowGroupCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceRowGroupCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceRowGroupCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceRowGroupCollection RetrieveInvoiceRowGroupCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceRowGroupCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceRowGroupCollection), null, owner);
                    return result;
                }

				public static InvoiceRowGroupCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceRowGroupCollection.RetrieveInvoiceRowGroupCollection("Content/TheBall.CORE/InvoiceRowGroupCollection/" + contentName, containerOwner);
					var result = InvoiceRowGroupCollection.RetrieveInvoiceRowGroupCollection("TheBall.CORE/InvoiceRowGroupCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceRowGroupCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceRowGroupCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceRowGroupCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceRowGroupCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceRowGroupCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceRowGroupCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceRowGroupCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceRowGroupCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceRowGroupCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceRowGroupCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = InvoiceRowGroup.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static InvoiceRowGroupCollection CreateDefault()
				{
					var result = new InvoiceRowGroupCollection();
					return result;
				}

				public static InvoiceRowGroupCollection CreateDemoDefault()
				{
					InvoiceRowGroupCollection customDemo = null;
					InvoiceRowGroupCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceRowGroupCollection();
					result.CollectionContent.Add(InvoiceRowGroup.CreateDemoDefault());
					//result.CollectionContent.Add(InvoiceRowGroup.CreateDemoDefault());
					//result.CollectionContent.Add(InvoiceRowGroup.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<InvoiceRowGroup> CollectionContent = new List<InvoiceRowGroup>();
				private InvoiceRowGroup[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public InvoiceRowGroup[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (InvoiceRowGroup )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(InvoiceRowGroupCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class InvoiceEventDetailGroupCollection : IInformationObject , IInformationCollection
			{
				public InvoiceEventDetailGroupCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceEventDetailGroupCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceEventDetailGroupCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceEventDetailGroupCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceEventDetailGroupCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceEventDetailGroupCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceEventDetailGroupCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceEventDetailGroupCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceEventDetailGroupCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceEventDetailGroupCollection RetrieveInvoiceEventDetailGroupCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceEventDetailGroupCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceEventDetailGroupCollection), null, owner);
                    return result;
                }

				public static InvoiceEventDetailGroupCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceEventDetailGroupCollection.RetrieveInvoiceEventDetailGroupCollection("Content/TheBall.CORE/InvoiceEventDetailGroupCollection/" + contentName, containerOwner);
					var result = InvoiceEventDetailGroupCollection.RetrieveInvoiceEventDetailGroupCollection("TheBall.CORE/InvoiceEventDetailGroupCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceEventDetailGroupCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceEventDetailGroupCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceEventDetailGroupCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceEventDetailGroupCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceEventDetailGroupCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceEventDetailGroupCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceEventDetailGroupCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceEventDetailGroupCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceEventDetailGroupCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceEventDetailGroupCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = InvoiceEventDetailGroup.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static InvoiceEventDetailGroupCollection CreateDefault()
				{
					var result = new InvoiceEventDetailGroupCollection();
					return result;
				}

				public static InvoiceEventDetailGroupCollection CreateDemoDefault()
				{
					InvoiceEventDetailGroupCollection customDemo = null;
					InvoiceEventDetailGroupCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceEventDetailGroupCollection();
					result.CollectionContent.Add(InvoiceEventDetailGroup.CreateDemoDefault());
					//result.CollectionContent.Add(InvoiceEventDetailGroup.CreateDemoDefault());
					//result.CollectionContent.Add(InvoiceEventDetailGroup.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<InvoiceEventDetailGroup> CollectionContent = new List<InvoiceEventDetailGroup>();
				private InvoiceEventDetailGroup[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public InvoiceEventDetailGroup[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (InvoiceEventDetailGroup )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(InvoiceEventDetailGroupCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class InvoiceRowGroup : IInformationObject 
			{
				public InvoiceRowGroup()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceRowGroup";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceRowGroup/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceRowGroup), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceRowGroup", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceRowGroup RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceRowGroup(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceRowGroup");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceRowGroup), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceRowGroup RetrieveInvoiceRowGroup(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceRowGroup) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceRowGroup), null, owner);
                    return result;
                }

				public static InvoiceRowGroup RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceRowGroup.RetrieveInvoiceRowGroup("Content/TheBall.CORE/InvoiceRowGroup/" + contentName, containerOwner);
					var result = InvoiceRowGroup.RetrieveInvoiceRowGroup("TheBall.CORE/InvoiceRowGroup/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceRowGroup/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceRowGroup/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceRowGroup) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceRowGroup));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceRowGroup DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceRowGroup));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceRowGroup) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceRowGroup", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceRowGroup", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceRowGroup customDemoObject);



				public static InvoiceRowGroup CreateDefault()
				{
					var result = new InvoiceRowGroup();
					result.InvoiceRowCollection = InvoiceRowCollection.CreateDefault();
					return result;
				}

				public static InvoiceRowGroup CreateDemoDefault()
				{
					InvoiceRowGroup customDemo = null;
					InvoiceRowGroup.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceRowGroup();
					result.GroupName = @"InvoiceRowGroup.GroupName";

					result.GroupTotalPriceWithoutTaxes = @"InvoiceRowGroup.GroupTotalPriceWithoutTaxes";

					result.GroupTotalTaxes = @"InvoiceRowGroup.GroupTotalTaxes";

					result.GroupTotalPriceWithTaxes = @"InvoiceRowGroup.GroupTotalPriceWithTaxes";

					result.InvoiceRowCollection = InvoiceRowCollection.CreateDemoDefault();
				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(InvoiceRowCollection != null) {
						((IInformationObject) InvoiceRowCollection).UpdateCollections(masterInstance);
					}

				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = InvoiceRowCollection;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = InvoiceRowCollection;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) InvoiceRowCollection;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(GroupName != _unmodified_GroupName)
							return true;
						if(GroupTotalPriceWithoutTaxes != _unmodified_GroupTotalPriceWithoutTaxes)
							return true;
						if(GroupTotalTaxes != _unmodified_GroupTotalTaxes)
							return true;
						if(GroupTotalPriceWithTaxes != _unmodified_GroupTotalPriceWithTaxes)
							return true;
						if(InvoiceRowCollection != _unmodified_InvoiceRowCollection)
							return true;
						{
							IInformationObject item = (IInformationObject) InvoiceRowCollection;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(InvoiceRowCollection != null) {
						if(InvoiceRowCollection.ID == replacingObject.ID)
							InvoiceRowCollection = (InvoiceRowCollection) replacingObject;
						else {
							IInformationObject iObject = InvoiceRowCollection;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(InvoiceRowGroup sourceObject)
				{
					GroupName = sourceObject.GroupName;
					GroupTotalPriceWithoutTaxes = sourceObject.GroupTotalPriceWithoutTaxes;
					GroupTotalTaxes = sourceObject.GroupTotalTaxes;
					GroupTotalPriceWithTaxes = sourceObject.GroupTotalPriceWithTaxes;
					InvoiceRowCollection = sourceObject.InvoiceRowCollection;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_GroupName = GroupName;
					_unmodified_GroupTotalPriceWithoutTaxes = GroupTotalPriceWithoutTaxes;
					_unmodified_GroupTotalTaxes = GroupTotalTaxes;
					_unmodified_GroupTotalPriceWithTaxes = GroupTotalPriceWithTaxes;
				
					_unmodified_InvoiceRowCollection = InvoiceRowCollection;
					if(InvoiceRowCollection != null)
						((IInformationObject) InvoiceRowCollection).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "GroupName":
							GroupName = value;
							break;
						case "GroupTotalPriceWithoutTaxes":
							GroupTotalPriceWithoutTaxes = value;
							break;
						case "GroupTotalTaxes":
							GroupTotalTaxes = value;
							break;
						case "GroupTotalPriceWithTaxes":
							GroupTotalPriceWithTaxes = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public string GroupName { get; set; }
			private string _unmodified_GroupName;
			[DataMember]
			public string GroupTotalPriceWithoutTaxes { get; set; }
			private string _unmodified_GroupTotalPriceWithoutTaxes;
			[DataMember]
			public string GroupTotalTaxes { get; set; }
			private string _unmodified_GroupTotalTaxes;
			[DataMember]
			public string GroupTotalPriceWithTaxes { get; set; }
			private string _unmodified_GroupTotalPriceWithTaxes;
			[DataMember]
			public InvoiceRowCollection InvoiceRowCollection { get; set; }
			private InvoiceRowCollection _unmodified_InvoiceRowCollection;
			
			}
			[DataContract]
			public partial class InvoiceEventDetailGroup : IInformationObject 
			{
				public InvoiceEventDetailGroup()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceEventDetailGroup";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceEventDetailGroup/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceEventDetailGroup), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceEventDetailGroup", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceEventDetailGroup RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceEventDetailGroup(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceEventDetailGroup");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceEventDetailGroup), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceEventDetailGroup RetrieveInvoiceEventDetailGroup(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceEventDetailGroup) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceEventDetailGroup), null, owner);
                    return result;
                }

				public static InvoiceEventDetailGroup RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceEventDetailGroup.RetrieveInvoiceEventDetailGroup("Content/TheBall.CORE/InvoiceEventDetailGroup/" + contentName, containerOwner);
					var result = InvoiceEventDetailGroup.RetrieveInvoiceEventDetailGroup("TheBall.CORE/InvoiceEventDetailGroup/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceEventDetailGroup/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceEventDetailGroup/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceEventDetailGroup) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceEventDetailGroup));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceEventDetailGroup DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceEventDetailGroup));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceEventDetailGroup) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceEventDetailGroup", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceEventDetailGroup", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceEventDetailGroup customDemoObject);



				public static InvoiceEventDetailGroup CreateDefault()
				{
					var result = new InvoiceEventDetailGroup();
					result.InvoiceEventDetailCollection = InvoiceEventDetailCollection.CreateDefault();
					return result;
				}

				public static InvoiceEventDetailGroup CreateDemoDefault()
				{
					InvoiceEventDetailGroup customDemo = null;
					InvoiceEventDetailGroup.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceEventDetailGroup();
					result.GroupName = @"InvoiceEventDetailGroup.GroupName";

					result.InvoiceEventDetailCollection = InvoiceEventDetailCollection.CreateDemoDefault();
				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(InvoiceEventDetailCollection != null) {
						((IInformationObject) InvoiceEventDetailCollection).UpdateCollections(masterInstance);
					}

				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = InvoiceEventDetailCollection;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = InvoiceEventDetailCollection;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) InvoiceEventDetailCollection;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(GroupName != _unmodified_GroupName)
							return true;
						if(InvoiceEventDetailCollection != _unmodified_InvoiceEventDetailCollection)
							return true;
						{
							IInformationObject item = (IInformationObject) InvoiceEventDetailCollection;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(InvoiceEventDetailCollection != null) {
						if(InvoiceEventDetailCollection.ID == replacingObject.ID)
							InvoiceEventDetailCollection = (InvoiceEventDetailCollection) replacingObject;
						else {
							IInformationObject iObject = InvoiceEventDetailCollection;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(InvoiceEventDetailGroup sourceObject)
				{
					GroupName = sourceObject.GroupName;
					InvoiceEventDetailCollection = sourceObject.InvoiceEventDetailCollection;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_GroupName = GroupName;
				
					_unmodified_InvoiceEventDetailCollection = InvoiceEventDetailCollection;
					if(InvoiceEventDetailCollection != null)
						((IInformationObject) InvoiceEventDetailCollection).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "GroupName":
							GroupName = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public string GroupName { get; set; }
			private string _unmodified_GroupName;
			[DataMember]
			public InvoiceEventDetailCollection InvoiceEventDetailCollection { get; set; }
			private InvoiceEventDetailCollection _unmodified_InvoiceEventDetailCollection;
			
			}
			[DataContract]
			public partial class InvoiceEventDetailCollection : IInformationObject , IInformationCollection
			{
				public InvoiceEventDetailCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceEventDetailCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceEventDetailCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceEventDetailCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceEventDetailCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceEventDetailCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceEventDetailCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceEventDetailCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceEventDetailCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceEventDetailCollection RetrieveInvoiceEventDetailCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceEventDetailCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceEventDetailCollection), null, owner);
                    return result;
                }

				public static InvoiceEventDetailCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceEventDetailCollection.RetrieveInvoiceEventDetailCollection("Content/TheBall.CORE/InvoiceEventDetailCollection/" + contentName, containerOwner);
					var result = InvoiceEventDetailCollection.RetrieveInvoiceEventDetailCollection("TheBall.CORE/InvoiceEventDetailCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceEventDetailCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceEventDetailCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceEventDetailCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceEventDetailCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceEventDetailCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceEventDetailCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceEventDetailCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceEventDetailCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceEventDetailCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceEventDetailCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = InvoiceEventDetail.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static InvoiceEventDetailCollection CreateDefault()
				{
					var result = new InvoiceEventDetailCollection();
					return result;
				}

				public static InvoiceEventDetailCollection CreateDemoDefault()
				{
					InvoiceEventDetailCollection customDemo = null;
					InvoiceEventDetailCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceEventDetailCollection();
					result.CollectionContent.Add(InvoiceEventDetail.CreateDemoDefault());
					//result.CollectionContent.Add(InvoiceEventDetail.CreateDemoDefault());
					//result.CollectionContent.Add(InvoiceEventDetail.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<InvoiceEventDetail> CollectionContent = new List<InvoiceEventDetail>();
				private InvoiceEventDetail[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public InvoiceEventDetail[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (InvoiceEventDetail )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(InvoiceEventDetailCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class InvoiceRowCollection : IInformationObject , IInformationCollection
			{
				public InvoiceRowCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceRowCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceRowCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceRowCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceRowCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceRowCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceRowCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceRowCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceRowCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceRowCollection RetrieveInvoiceRowCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceRowCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceRowCollection), null, owner);
                    return result;
                }

				public static InvoiceRowCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceRowCollection.RetrieveInvoiceRowCollection("Content/TheBall.CORE/InvoiceRowCollection/" + contentName, containerOwner);
					var result = InvoiceRowCollection.RetrieveInvoiceRowCollection("TheBall.CORE/InvoiceRowCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceRowCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceRowCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceRowCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceRowCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceRowCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceRowCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceRowCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceRowCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceRowCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceRowCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = InvoiceRow.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static InvoiceRowCollection CreateDefault()
				{
					var result = new InvoiceRowCollection();
					return result;
				}

				public static InvoiceRowCollection CreateDemoDefault()
				{
					InvoiceRowCollection customDemo = null;
					InvoiceRowCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceRowCollection();
					result.CollectionContent.Add(InvoiceRow.CreateDemoDefault());
					//result.CollectionContent.Add(InvoiceRow.CreateDemoDefault());
					//result.CollectionContent.Add(InvoiceRow.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<InvoiceRow> CollectionContent = new List<InvoiceRow>();
				private InvoiceRow[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public InvoiceRow[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (InvoiceRow )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(InvoiceRowCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class InvoiceEventDetail : IInformationObject 
			{
				public InvoiceEventDetail()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceEventDetail";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceEventDetail/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceEventDetail), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceEventDetail", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceEventDetail RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceEventDetail(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceEventDetail");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceEventDetail), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceEventDetail RetrieveInvoiceEventDetail(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceEventDetail) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceEventDetail), null, owner);
                    return result;
                }

				public static InvoiceEventDetail RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceEventDetail.RetrieveInvoiceEventDetail("Content/TheBall.CORE/InvoiceEventDetail/" + contentName, containerOwner);
					var result = InvoiceEventDetail.RetrieveInvoiceEventDetail("TheBall.CORE/InvoiceEventDetail/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceEventDetail/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceEventDetail/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceEventDetail) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceEventDetail));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceEventDetail DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceEventDetail));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceEventDetail) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceEventDetail", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceEventDetail", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceEventDetail customDemoObject);



				public static InvoiceEventDetail CreateDefault()
				{
					var result = new InvoiceEventDetail();
					return result;
				}

				public static InvoiceEventDetail CreateDemoDefault()
				{
					InvoiceEventDetail customDemo = null;
					InvoiceEventDetail.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceEventDetail();
					result.IndentMode = @"InvoiceEventDetail.IndentMode";

					result.ReceivingParty = @"InvoiceEventDetail.ReceivingParty";

					result.AmountOfUnits = @"InvoiceEventDetail.AmountOfUnits";

					result.Duration = @"InvoiceEventDetail.Duration";

					result.UnitPrice = @"InvoiceEventDetail.UnitPrice";

					result.PriceWithoutTaxes = @"InvoiceEventDetail.PriceWithoutTaxes";

					result.Taxes = @"InvoiceEventDetail.Taxes";

					result.PriceWithTaxes = @"InvoiceEventDetail.PriceWithTaxes";

				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(IndentMode != _unmodified_IndentMode)
							return true;
						if(EventStartDateTime != _unmodified_EventStartDateTime)
							return true;
						if(EventEndDateTime != _unmodified_EventEndDateTime)
							return true;
						if(ReceivingParty != _unmodified_ReceivingParty)
							return true;
						if(AmountOfUnits != _unmodified_AmountOfUnits)
							return true;
						if(Duration != _unmodified_Duration)
							return true;
						if(UnitPrice != _unmodified_UnitPrice)
							return true;
						if(PriceWithoutTaxes != _unmodified_PriceWithoutTaxes)
							return true;
						if(Taxes != _unmodified_Taxes)
							return true;
						if(PriceWithTaxes != _unmodified_PriceWithTaxes)
							return true;
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
				}


				private void CopyContentFrom(InvoiceEventDetail sourceObject)
				{
					IndentMode = sourceObject.IndentMode;
					EventStartDateTime = sourceObject.EventStartDateTime;
					EventEndDateTime = sourceObject.EventEndDateTime;
					ReceivingParty = sourceObject.ReceivingParty;
					AmountOfUnits = sourceObject.AmountOfUnits;
					Duration = sourceObject.Duration;
					UnitPrice = sourceObject.UnitPrice;
					PriceWithoutTaxes = sourceObject.PriceWithoutTaxes;
					Taxes = sourceObject.Taxes;
					PriceWithTaxes = sourceObject.PriceWithTaxes;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_IndentMode = IndentMode;
					_unmodified_EventStartDateTime = EventStartDateTime;
					_unmodified_EventEndDateTime = EventEndDateTime;
					_unmodified_ReceivingParty = ReceivingParty;
					_unmodified_AmountOfUnits = AmountOfUnits;
					_unmodified_Duration = Duration;
					_unmodified_UnitPrice = UnitPrice;
					_unmodified_PriceWithoutTaxes = PriceWithoutTaxes;
					_unmodified_Taxes = Taxes;
					_unmodified_PriceWithTaxes = PriceWithTaxes;
				
				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "IndentMode":
							IndentMode = value;
							break;
						case "EventStartDateTime":
							EventStartDateTime = DateTime.Parse(value);
							break;
						case "EventEndDateTime":
							EventEndDateTime = DateTime.Parse(value);
							break;
						case "ReceivingParty":
							ReceivingParty = value;
							break;
						case "AmountOfUnits":
							AmountOfUnits = value;
							break;
						case "Duration":
							Duration = value;
							break;
						case "UnitPrice":
							UnitPrice = value;
							break;
						case "PriceWithoutTaxes":
							PriceWithoutTaxes = value;
							break;
						case "Taxes":
							Taxes = value;
							break;
						case "PriceWithTaxes":
							PriceWithTaxes = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public string IndentMode { get; set; }
			private string _unmodified_IndentMode;
			[DataMember]
			public DateTime EventStartDateTime { get; set; }
			private DateTime _unmodified_EventStartDateTime;
			[DataMember]
			public DateTime EventEndDateTime { get; set; }
			private DateTime _unmodified_EventEndDateTime;
			[DataMember]
			public string ReceivingParty { get; set; }
			private string _unmodified_ReceivingParty;
			[DataMember]
			public string AmountOfUnits { get; set; }
			private string _unmodified_AmountOfUnits;
			[DataMember]
			public string Duration { get; set; }
			private string _unmodified_Duration;
			[DataMember]
			public string UnitPrice { get; set; }
			private string _unmodified_UnitPrice;
			[DataMember]
			public string PriceWithoutTaxes { get; set; }
			private string _unmodified_PriceWithoutTaxes;
			[DataMember]
			public string Taxes { get; set; }
			private string _unmodified_Taxes;
			[DataMember]
			public string PriceWithTaxes { get; set; }
			private string _unmodified_PriceWithTaxes;
			
			}
			[DataContract]
			public partial class InvoiceRow : IInformationObject 
			{
				public InvoiceRow()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InvoiceRow";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InvoiceRow/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InvoiceRow), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InvoiceRow", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InvoiceRow RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInvoiceRow(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InvoiceRow");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InvoiceRow), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InvoiceRow RetrieveInvoiceRow(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InvoiceRow) StorageSupport.RetrieveInformation(relativeLocation, typeof(InvoiceRow), null, owner);
                    return result;
                }

				public static InvoiceRow RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InvoiceRow.RetrieveInvoiceRow("Content/TheBall.CORE/InvoiceRow/" + contentName, containerOwner);
					var result = InvoiceRow.RetrieveInvoiceRow("TheBall.CORE/InvoiceRow/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InvoiceRow/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InvoiceRow/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InvoiceRow) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceRow));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InvoiceRow DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InvoiceRow));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InvoiceRow) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InvoiceRow", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InvoiceRow", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InvoiceRow customDemoObject);



				public static InvoiceRow CreateDefault()
				{
					var result = new InvoiceRow();
					result.ReferenceToInformation = ReferenceToInformation.CreateDefault();
					return result;
				}

				public static InvoiceRow CreateDemoDefault()
				{
					InvoiceRow customDemo = null;
					InvoiceRow.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InvoiceRow();
					result.ReferenceToInformation = ReferenceToInformation.CreateDemoDefault();
					result.IndentMode = @"InvoiceRow.IndentMode";

					result.AmountOfUnits = @"InvoiceRow.AmountOfUnits";

					result.Duration = @"InvoiceRow.Duration";

					result.UnitPrice = @"InvoiceRow.UnitPrice";

					result.PriceWithoutTaxes = @"InvoiceRow.PriceWithoutTaxes";

					result.Taxes = @"InvoiceRow.Taxes";

					result.PriceWithTaxes = @"InvoiceRow.PriceWithTaxes";

				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					if(searchWithinCurrentMasterOnly == false)
					{
						{
							IInformationObject item = ReferenceToInformation;
							if(item != null)
							{
								item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
							}
						}
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = ReferenceToInformation;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) ReferenceToInformation;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(ReferenceToInformation != _unmodified_ReferenceToInformation)
							return true;
						if(IndentMode != _unmodified_IndentMode)
							return true;
						if(AmountOfUnits != _unmodified_AmountOfUnits)
							return true;
						if(Duration != _unmodified_Duration)
							return true;
						if(UnitPrice != _unmodified_UnitPrice)
							return true;
						if(PriceWithoutTaxes != _unmodified_PriceWithoutTaxes)
							return true;
						if(Taxes != _unmodified_Taxes)
							return true;
						if(PriceWithTaxes != _unmodified_PriceWithTaxes)
							return true;
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(ReferenceToInformation != null) {
						if(ReferenceToInformation.ID == replacingObject.ID)
							ReferenceToInformation = (ReferenceToInformation) replacingObject;
						else {
							IInformationObject iObject = ReferenceToInformation;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(InvoiceRow sourceObject)
				{
					ReferenceToInformation = sourceObject.ReferenceToInformation;
					IndentMode = sourceObject.IndentMode;
					AmountOfUnits = sourceObject.AmountOfUnits;
					Duration = sourceObject.Duration;
					UnitPrice = sourceObject.UnitPrice;
					PriceWithoutTaxes = sourceObject.PriceWithoutTaxes;
					Taxes = sourceObject.Taxes;
					PriceWithTaxes = sourceObject.PriceWithTaxes;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_IndentMode = IndentMode;
					_unmodified_AmountOfUnits = AmountOfUnits;
					_unmodified_Duration = Duration;
					_unmodified_UnitPrice = UnitPrice;
					_unmodified_PriceWithoutTaxes = PriceWithoutTaxes;
					_unmodified_Taxes = Taxes;
					_unmodified_PriceWithTaxes = PriceWithTaxes;
				
					_unmodified_ReferenceToInformation = ReferenceToInformation;
					if(ReferenceToInformation != null)
						((IInformationObject) ReferenceToInformation).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "IndentMode":
							IndentMode = value;
							break;
						case "AmountOfUnits":
							AmountOfUnits = value;
							break;
						case "Duration":
							Duration = value;
							break;
						case "UnitPrice":
							UnitPrice = value;
							break;
						case "PriceWithoutTaxes":
							PriceWithoutTaxes = value;
							break;
						case "Taxes":
							Taxes = value;
							break;
						case "PriceWithTaxes":
							PriceWithTaxes = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public ReferenceToInformation ReferenceToInformation { get; set; }
			private ReferenceToInformation _unmodified_ReferenceToInformation;
			[DataMember]
			public string IndentMode { get; set; }
			private string _unmodified_IndentMode;
			[DataMember]
			public string AmountOfUnits { get; set; }
			private string _unmodified_AmountOfUnits;
			[DataMember]
			public string Duration { get; set; }
			private string _unmodified_Duration;
			[DataMember]
			public string UnitPrice { get; set; }
			private string _unmodified_UnitPrice;
			[DataMember]
			public string PriceWithoutTaxes { get; set; }
			private string _unmodified_PriceWithoutTaxes;
			[DataMember]
			public string Taxes { get; set; }
			private string _unmodified_Taxes;
			[DataMember]
			public string PriceWithTaxes { get; set; }
			private string _unmodified_PriceWithTaxes;
			
			}
			[DataContract]
			public partial class CategoryCollection : IInformationObject , IInformationCollection
			{
				public CategoryCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "CategoryCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/CategoryCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(CategoryCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "CategoryCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static CategoryCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveCategoryCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: CategoryCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(CategoryCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static CategoryCollection RetrieveCategoryCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (CategoryCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(CategoryCollection), null, owner);
                    return result;
                }

				public static CategoryCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = CategoryCollection.RetrieveCategoryCollection("Content/TheBall.CORE/CategoryCollection/" + contentName, containerOwner);
					var result = CategoryCollection.RetrieveCategoryCollection("TheBall.CORE/CategoryCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/CategoryCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/CategoryCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((CategoryCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(CategoryCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static CategoryCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(CategoryCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (CategoryCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "CategoryCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "CategoryCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref CategoryCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = Category.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static CategoryCollection CreateDefault()
				{
					var result = new CategoryCollection();
					return result;
				}

				public static CategoryCollection CreateDemoDefault()
				{
					CategoryCollection customDemo = null;
					CategoryCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new CategoryCollection();
					result.CollectionContent.Add(Category.CreateDemoDefault());
					//result.CollectionContent.Add(Category.CreateDemoDefault());
					//result.CollectionContent.Add(Category.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<Category> CollectionContent = new List<Category>();
				private Category[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public Category[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (Category )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(CategoryCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class Category : IInformationObject 
			{
				public Category()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "Category";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/Category/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(Category), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "Category", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static Category RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveCategory(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: Category");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(Category), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static Category RetrieveCategory(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (Category) StorageSupport.RetrieveInformation(relativeLocation, typeof(Category), null, owner);
                    return result;
                }

				public static Category RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = Category.RetrieveCategory("Content/TheBall.CORE/Category/" + contentName, containerOwner);
					var result = Category.RetrieveCategory("TheBall.CORE/Category/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/Category/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/Category/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((Category) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(Category));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static Category DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(Category));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (Category) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "Category", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "Category", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref Category customDemoObject);



				public static Category CreateDefault()
				{
					var result = new Category();
					return result;
				}

				public static Category CreateDemoDefault()
				{
					Category customDemo = null;
					Category.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new Category();
					result.CategoryName = @"Category.CategoryName";

				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(CategoryName != _unmodified_CategoryName)
							return true;
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
				}


				private void CopyContentFrom(Category sourceObject)
				{
					CategoryName = sourceObject.CategoryName;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CategoryName = CategoryName;
				
				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "CategoryName":
							CategoryName = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public string CategoryName { get; set; }
			private string _unmodified_CategoryName;
			
			}
			[DataContract]
			public partial class Process : IInformationObject 
			{
				public Process()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "Process";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/Process/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(Process), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "Process", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static Process RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveProcess(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: Process");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(Process), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static Process RetrieveProcess(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (Process) StorageSupport.RetrieveInformation(relativeLocation, typeof(Process), null, owner);
                    return result;
                }

				public static Process RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = Process.RetrieveProcess("Content/TheBall.CORE/Process/" + contentName, containerOwner);
					var result = Process.RetrieveProcess("TheBall.CORE/Process/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/Process/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/Process/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((Process) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(Process));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static Process DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(Process));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (Process) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "Process", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "Process", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref Process customDemoObject);



				public static Process CreateDefault()
				{
					var result = new Process();
					result.CategoryCollection = CategoryCollection.CreateDefault();
					return result;
				}

				public static Process CreateDemoDefault()
				{
					Process customDemo = null;
					Process.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new Process();
					result.ProcessID = @"Process.ProcessID";

					result.ProcessName = @"Process.ProcessName";

					result.CategoryCollection = CategoryCollection.CreateDemoDefault();
				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(CategoryCollection != null) {
						((IInformationObject) CategoryCollection).UpdateCollections(masterInstance);
					}

				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = CategoryCollection;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = CategoryCollection;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) CategoryCollection;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(ProcessID != _unmodified_ProcessID)
							return true;
						if(ProcessName != _unmodified_ProcessName)
							return true;
						if(CategoryCollection != _unmodified_CategoryCollection)
							return true;
						{
							IInformationObject item = (IInformationObject) CategoryCollection;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(CategoryCollection != null) {
						if(CategoryCollection.ID == replacingObject.ID)
							CategoryCollection = (CategoryCollection) replacingObject;
						else {
							IInformationObject iObject = CategoryCollection;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(Process sourceObject)
				{
					ProcessID = sourceObject.ProcessID;
					ProcessName = sourceObject.ProcessName;
					CategoryCollection = sourceObject.CategoryCollection;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_ProcessID = ProcessID;
					_unmodified_ProcessName = ProcessName;
				
					_unmodified_CategoryCollection = CategoryCollection;
					if(CategoryCollection != null)
						((IInformationObject) CategoryCollection).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "ProcessID":
							ProcessID = value;
							break;
						case "ProcessName":
							ProcessName = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public string ProcessID { get; set; }
			private string _unmodified_ProcessID;
			[DataMember]
			public string ProcessName { get; set; }
			private string _unmodified_ProcessName;
			[DataMember]
			public CategoryCollection CategoryCollection { get; set; }
			private CategoryCollection _unmodified_CategoryCollection;
			
			}
			[DataContract]
			public partial class ReferenceToInformation : IInformationObject 
			{
				public ReferenceToInformation()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "ReferenceToInformation";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/ReferenceToInformation/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(ReferenceToInformation), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "ReferenceToInformation", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static ReferenceToInformation RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveReferenceToInformation(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: ReferenceToInformation");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(ReferenceToInformation), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static ReferenceToInformation RetrieveReferenceToInformation(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (ReferenceToInformation) StorageSupport.RetrieveInformation(relativeLocation, typeof(ReferenceToInformation), null, owner);
                    return result;
                }

				public static ReferenceToInformation RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = ReferenceToInformation.RetrieveReferenceToInformation("Content/TheBall.CORE/ReferenceToInformation/" + contentName, containerOwner);
					var result = ReferenceToInformation.RetrieveReferenceToInformation("TheBall.CORE/ReferenceToInformation/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/ReferenceToInformation/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/ReferenceToInformation/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return true;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((ReferenceToInformation) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(ReferenceToInformation));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static ReferenceToInformation DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(ReferenceToInformation));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (ReferenceToInformation) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "ReferenceToInformation", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "ReferenceToInformation", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref ReferenceToInformation customDemoObject);



				public static ReferenceToInformation CreateDefault()
				{
					var result = new ReferenceToInformation();
					return result;
				}

				public static ReferenceToInformation CreateDemoDefault()
				{
					ReferenceToInformation customDemo = null;
					ReferenceToInformation.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new ReferenceToInformation();
					result.Title = @"ReferenceToInformation.Title";

					result.URL = @"ReferenceToInformation.URL";

				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(Title != _unmodified_Title)
							return true;
						if(URL != _unmodified_URL)
							return true;
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
				}


				private void CopyContentFrom(ReferenceToInformation sourceObject)
				{
					Title = sourceObject.Title;
					URL = sourceObject.URL;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_Title = Title;
					_unmodified_URL = URL;
				
				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "Title":
							Title = value;
							break;
						case "URL":
							URL = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public string Title { get; set; }
			private string _unmodified_Title;
			[DataMember]
			public string URL { get; set; }
			private string _unmodified_URL;
			
			}
			[DataContract]
			public partial class ReferenceCollection : IInformationObject , IInformationCollection
			{
				public ReferenceCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "ReferenceCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/ReferenceCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(ReferenceCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "ReferenceCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static ReferenceCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveReferenceCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: ReferenceCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(ReferenceCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static ReferenceCollection RetrieveReferenceCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (ReferenceCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(ReferenceCollection), null, owner);
                    return result;
                }

				public static ReferenceCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = ReferenceCollection.RetrieveReferenceCollection("Content/TheBall.CORE/ReferenceCollection/" + contentName, containerOwner);
					var result = ReferenceCollection.RetrieveReferenceCollection("TheBall.CORE/ReferenceCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/ReferenceCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/ReferenceCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((ReferenceCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(ReferenceCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static ReferenceCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(ReferenceCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (ReferenceCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "ReferenceCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "ReferenceCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref ReferenceCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = ReferenceToInformation.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static ReferenceCollection CreateDefault()
				{
					var result = new ReferenceCollection();
					return result;
				}

				public static ReferenceCollection CreateDemoDefault()
				{
					ReferenceCollection customDemo = null;
					ReferenceCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new ReferenceCollection();
					result.CollectionContent.Add(ReferenceToInformation.CreateDemoDefault());
					//result.CollectionContent.Add(ReferenceToInformation.CreateDemoDefault());
					//result.CollectionContent.Add(ReferenceToInformation.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<ReferenceToInformation> CollectionContent = new List<ReferenceToInformation>();
				private ReferenceToInformation[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public ReferenceToInformation[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (ReferenceToInformation )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(ReferenceCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					if(searchWithinCurrentMasterOnly == false) {
						foreach(IInformationObject iObject in CollectionContent)
							iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
					}
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class InformationOwnerInfo : IInformationObject 
			{
				public InformationOwnerInfo()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "InformationOwnerInfo";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/InformationOwnerInfo/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(InformationOwnerInfo), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "InformationOwnerInfo", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static InformationOwnerInfo RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveInformationOwnerInfo(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: InformationOwnerInfo");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(InformationOwnerInfo), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static InformationOwnerInfo RetrieveInformationOwnerInfo(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (InformationOwnerInfo) StorageSupport.RetrieveInformation(relativeLocation, typeof(InformationOwnerInfo), null, owner);
                    return result;
                }

				public static InformationOwnerInfo RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = InformationOwnerInfo.RetrieveInformationOwnerInfo("Content/TheBall.CORE/InformationOwnerInfo/" + contentName, containerOwner);
					var result = InformationOwnerInfo.RetrieveInformationOwnerInfo("TheBall.CORE/InformationOwnerInfo/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/InformationOwnerInfo/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/InformationOwnerInfo/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((InformationOwnerInfo) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InformationOwnerInfo));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static InformationOwnerInfo DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(InformationOwnerInfo));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (InformationOwnerInfo) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "InformationOwnerInfo", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "InformationOwnerInfo", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref InformationOwnerInfo customDemoObject);



				public static InformationOwnerInfo CreateDefault()
				{
					var result = new InformationOwnerInfo();
					return result;
				}

				public static InformationOwnerInfo CreateDemoDefault()
				{
					InformationOwnerInfo customDemo = null;
					InformationOwnerInfo.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new InformationOwnerInfo();
					result.OwnerType = @"InformationOwnerInfo.OwnerType";

					result.OwnerIdentifier = @"InformationOwnerInfo.OwnerIdentifier";

				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(OwnerType != _unmodified_OwnerType)
							return true;
						if(OwnerIdentifier != _unmodified_OwnerIdentifier)
							return true;
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
				}


				private void CopyContentFrom(InformationOwnerInfo sourceObject)
				{
					OwnerType = sourceObject.OwnerType;
					OwnerIdentifier = sourceObject.OwnerIdentifier;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_OwnerType = OwnerType;
					_unmodified_OwnerIdentifier = OwnerIdentifier;
				
				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "OwnerType":
							OwnerType = value;
							break;
						case "OwnerIdentifier":
							OwnerIdentifier = value;
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public string OwnerType { get; set; }
			private string _unmodified_OwnerType;
			[DataMember]
			public string OwnerIdentifier { get; set; }
			private string _unmodified_OwnerIdentifier;
			
			}
			[DataContract]
			public partial class UsageMonitorItem : IInformationObject 
			{
				public UsageMonitorItem()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "UsageMonitorItem";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/UsageMonitorItem/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(UsageMonitorItem), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "UsageMonitorItem", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static UsageMonitorItem RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveUsageMonitorItem(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: UsageMonitorItem");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(UsageMonitorItem), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static UsageMonitorItem RetrieveUsageMonitorItem(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (UsageMonitorItem) StorageSupport.RetrieveInformation(relativeLocation, typeof(UsageMonitorItem), null, owner);
                    return result;
                }

				public static UsageMonitorItem RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = UsageMonitorItem.RetrieveUsageMonitorItem("Content/TheBall.CORE/UsageMonitorItem/" + contentName, containerOwner);
					var result = UsageMonitorItem.RetrieveUsageMonitorItem("TheBall.CORE/UsageMonitorItem/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/UsageMonitorItem/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/UsageMonitorItem/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((UsageMonitorItem) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(UsageMonitorItem));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static UsageMonitorItem DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(UsageMonitorItem));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (UsageMonitorItem) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "UsageMonitorItem", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "UsageMonitorItem", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref UsageMonitorItem customDemoObject);



				public static UsageMonitorItem CreateDefault()
				{
					var result = new UsageMonitorItem();
					result.OwnerInfo = InformationOwnerInfo.CreateDefault();
					result.ProcessorUsages = ProcessorUsageCollection.CreateDefault();
					result.StorageTransactionUsages = StorageTransactionUsageCollection.CreateDefault();
					result.StorageUsages = StorageUsageCollection.CreateDefault();
					result.NetworkUsages = NetworkUsageCollection.CreateDefault();
					return result;
				}

				public static UsageMonitorItem CreateDemoDefault()
				{
					UsageMonitorItem customDemo = null;
					UsageMonitorItem.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new UsageMonitorItem();
					result.OwnerInfo = InformationOwnerInfo.CreateDemoDefault();
					result.ProcessorUsages = ProcessorUsageCollection.CreateDemoDefault();
					result.StorageTransactionUsages = StorageTransactionUsageCollection.CreateDemoDefault();
					result.StorageUsages = StorageUsageCollection.CreateDemoDefault();
					result.NetworkUsages = NetworkUsageCollection.CreateDemoDefault();
				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(OwnerInfo != null) {
						((IInformationObject) OwnerInfo).UpdateCollections(masterInstance);
					}

					if(ProcessorUsages != null) {
						((IInformationObject) ProcessorUsages).UpdateCollections(masterInstance);
					}

					if(StorageTransactionUsages != null) {
						((IInformationObject) StorageTransactionUsages).UpdateCollections(masterInstance);
					}

					if(StorageUsages != null) {
						((IInformationObject) StorageUsages).UpdateCollections(masterInstance);
					}

					if(NetworkUsages != null) {
						((IInformationObject) NetworkUsages).UpdateCollections(masterInstance);
					}

				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = OwnerInfo;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					{ // Scoping block for variable name reusability
						IInformationObject item = ProcessorUsages;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					{ // Scoping block for variable name reusability
						IInformationObject item = StorageTransactionUsages;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					{ // Scoping block for variable name reusability
						IInformationObject item = StorageUsages;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					{ // Scoping block for variable name reusability
						IInformationObject item = NetworkUsages;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = OwnerInfo;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					{
						var item = ProcessorUsages;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					{
						var item = StorageTransactionUsages;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					{
						var item = StorageUsages;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					{
						var item = NetworkUsages;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) OwnerInfo;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
					{
						var item = (IInformationObject) ProcessorUsages;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
					{
						var item = (IInformationObject) StorageTransactionUsages;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
					{
						var item = (IInformationObject) StorageUsages;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
					{
						var item = (IInformationObject) NetworkUsages;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(OwnerInfo != _unmodified_OwnerInfo)
							return true;
						if(ProcessorUsages != _unmodified_ProcessorUsages)
							return true;
						if(StorageTransactionUsages != _unmodified_StorageTransactionUsages)
							return true;
						if(StorageUsages != _unmodified_StorageUsages)
							return true;
						if(NetworkUsages != _unmodified_NetworkUsages)
							return true;
						{
							IInformationObject item = (IInformationObject) OwnerInfo;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
						{
							IInformationObject item = (IInformationObject) ProcessorUsages;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
						{
							IInformationObject item = (IInformationObject) StorageTransactionUsages;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
						{
							IInformationObject item = (IInformationObject) StorageUsages;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
						{
							IInformationObject item = (IInformationObject) NetworkUsages;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(OwnerInfo != null) {
						if(OwnerInfo.ID == replacingObject.ID)
							OwnerInfo = (InformationOwnerInfo) replacingObject;
						else {
							IInformationObject iObject = OwnerInfo;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
					if(ProcessorUsages != null) {
						if(ProcessorUsages.ID == replacingObject.ID)
							ProcessorUsages = (ProcessorUsageCollection) replacingObject;
						else {
							IInformationObject iObject = ProcessorUsages;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
					if(StorageTransactionUsages != null) {
						if(StorageTransactionUsages.ID == replacingObject.ID)
							StorageTransactionUsages = (StorageTransactionUsageCollection) replacingObject;
						else {
							IInformationObject iObject = StorageTransactionUsages;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
					if(StorageUsages != null) {
						if(StorageUsages.ID == replacingObject.ID)
							StorageUsages = (StorageUsageCollection) replacingObject;
						else {
							IInformationObject iObject = StorageUsages;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
					if(NetworkUsages != null) {
						if(NetworkUsages.ID == replacingObject.ID)
							NetworkUsages = (NetworkUsageCollection) replacingObject;
						else {
							IInformationObject iObject = NetworkUsages;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(UsageMonitorItem sourceObject)
				{
					OwnerInfo = sourceObject.OwnerInfo;
					ProcessorUsages = sourceObject.ProcessorUsages;
					StorageTransactionUsages = sourceObject.StorageTransactionUsages;
					StorageUsages = sourceObject.StorageUsages;
					NetworkUsages = sourceObject.NetworkUsages;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
				
					_unmodified_OwnerInfo = OwnerInfo;
					if(OwnerInfo != null)
						((IInformationObject) OwnerInfo).SetInstanceTreeValuesAsUnmodified();

					_unmodified_ProcessorUsages = ProcessorUsages;
					if(ProcessorUsages != null)
						((IInformationObject) ProcessorUsages).SetInstanceTreeValuesAsUnmodified();

					_unmodified_StorageTransactionUsages = StorageTransactionUsages;
					if(StorageTransactionUsages != null)
						((IInformationObject) StorageTransactionUsages).SetInstanceTreeValuesAsUnmodified();

					_unmodified_StorageUsages = StorageUsages;
					if(StorageUsages != null)
						((IInformationObject) StorageUsages).SetInstanceTreeValuesAsUnmodified();

					_unmodified_NetworkUsages = NetworkUsages;
					if(NetworkUsages != null)
						((IInformationObject) NetworkUsages).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public InformationOwnerInfo OwnerInfo { get; set; }
			private InformationOwnerInfo _unmodified_OwnerInfo;
			[DataMember]
			public ProcessorUsageCollection ProcessorUsages { get; set; }
			private ProcessorUsageCollection _unmodified_ProcessorUsages;
			[DataMember]
			public StorageTransactionUsageCollection StorageTransactionUsages { get; set; }
			private StorageTransactionUsageCollection _unmodified_StorageTransactionUsages;
			[DataMember]
			public StorageUsageCollection StorageUsages { get; set; }
			private StorageUsageCollection _unmodified_StorageUsages;
			[DataMember]
			public NetworkUsageCollection NetworkUsages { get; set; }
			private NetworkUsageCollection _unmodified_NetworkUsages;
			
			}
			[DataContract]
			public partial class ProcessorUsageCollection : IInformationObject , IInformationCollection
			{
				public ProcessorUsageCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "ProcessorUsageCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/ProcessorUsageCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(ProcessorUsageCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "ProcessorUsageCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static ProcessorUsageCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveProcessorUsageCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: ProcessorUsageCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(ProcessorUsageCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static ProcessorUsageCollection RetrieveProcessorUsageCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (ProcessorUsageCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(ProcessorUsageCollection), null, owner);
                    return result;
                }

				public static ProcessorUsageCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = ProcessorUsageCollection.RetrieveProcessorUsageCollection("Content/TheBall.CORE/ProcessorUsageCollection/" + contentName, containerOwner);
					var result = ProcessorUsageCollection.RetrieveProcessorUsageCollection("TheBall.CORE/ProcessorUsageCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/ProcessorUsageCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/ProcessorUsageCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((ProcessorUsageCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(ProcessorUsageCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static ProcessorUsageCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(ProcessorUsageCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (ProcessorUsageCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "ProcessorUsageCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "ProcessorUsageCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref ProcessorUsageCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = ProcessorUsage.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static ProcessorUsageCollection CreateDefault()
				{
					var result = new ProcessorUsageCollection();
					return result;
				}

				public static ProcessorUsageCollection CreateDemoDefault()
				{
					ProcessorUsageCollection customDemo = null;
					ProcessorUsageCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new ProcessorUsageCollection();
					result.CollectionContent.Add(ProcessorUsage.CreateDemoDefault());
					//result.CollectionContent.Add(ProcessorUsage.CreateDemoDefault());
					//result.CollectionContent.Add(ProcessorUsage.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<ProcessorUsage> CollectionContent = new List<ProcessorUsage>();
				private ProcessorUsage[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public ProcessorUsage[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (ProcessorUsage )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(ProcessorUsageCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class ProcessorUsage : IInformationObject 
			{
				public ProcessorUsage()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "ProcessorUsage";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/ProcessorUsage/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(ProcessorUsage), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "ProcessorUsage", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static ProcessorUsage RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveProcessorUsage(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: ProcessorUsage");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(ProcessorUsage), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static ProcessorUsage RetrieveProcessorUsage(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (ProcessorUsage) StorageSupport.RetrieveInformation(relativeLocation, typeof(ProcessorUsage), null, owner);
                    return result;
                }

				public static ProcessorUsage RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = ProcessorUsage.RetrieveProcessorUsage("Content/TheBall.CORE/ProcessorUsage/" + contentName, containerOwner);
					var result = ProcessorUsage.RetrieveProcessorUsage("TheBall.CORE/ProcessorUsage/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/ProcessorUsage/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/ProcessorUsage/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((ProcessorUsage) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(ProcessorUsage));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static ProcessorUsage DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(ProcessorUsage));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (ProcessorUsage) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "ProcessorUsage", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "ProcessorUsage", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref ProcessorUsage customDemoObject);



				public static ProcessorUsage CreateDefault()
				{
					var result = new ProcessorUsage();
					result.TimeRange = TimeRange.CreateDefault();
					return result;
				}

				public static ProcessorUsage CreateDemoDefault()
				{
					ProcessorUsage customDemo = null;
					ProcessorUsage.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new ProcessorUsage();
					result.TimeRange = TimeRange.CreateDemoDefault();
					result.UsageType = @"ProcessorUsage.UsageType";

				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(TimeRange != null) {
						((IInformationObject) TimeRange).UpdateCollections(masterInstance);
					}

				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = TimeRange;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = TimeRange;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) TimeRange;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(TimeRange != _unmodified_TimeRange)
							return true;
						if(UsageType != _unmodified_UsageType)
							return true;
						if(AmountOfTicks != _unmodified_AmountOfTicks)
							return true;
						if(FrequencyTicksPerSecond != _unmodified_FrequencyTicksPerSecond)
							return true;
						{
							IInformationObject item = (IInformationObject) TimeRange;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(TimeRange != null) {
						if(TimeRange.ID == replacingObject.ID)
							TimeRange = (TimeRange) replacingObject;
						else {
							IInformationObject iObject = TimeRange;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(ProcessorUsage sourceObject)
				{
					TimeRange = sourceObject.TimeRange;
					UsageType = sourceObject.UsageType;
					AmountOfTicks = sourceObject.AmountOfTicks;
					FrequencyTicksPerSecond = sourceObject.FrequencyTicksPerSecond;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_UsageType = UsageType;
					_unmodified_AmountOfTicks = AmountOfTicks;
					_unmodified_FrequencyTicksPerSecond = FrequencyTicksPerSecond;
				
					_unmodified_TimeRange = TimeRange;
					if(TimeRange != null)
						((IInformationObject) TimeRange).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "UsageType":
							UsageType = value;
							break;
						case "AmountOfTicks":
							AmountOfTicks = double.Parse(value);
							break;
						case "FrequencyTicksPerSecond":
							FrequencyTicksPerSecond = double.Parse(value);
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public TimeRange TimeRange { get; set; }
			private TimeRange _unmodified_TimeRange;
			[DataMember]
			public string UsageType { get; set; }
			private string _unmodified_UsageType;
			[DataMember]
			public double AmountOfTicks { get; set; }
			private double _unmodified_AmountOfTicks;
			[DataMember]
			public double FrequencyTicksPerSecond { get; set; }
			private double _unmodified_FrequencyTicksPerSecond;
			
			}
			[DataContract]
			public partial class StorageTransactionUsageCollection : IInformationObject , IInformationCollection
			{
				public StorageTransactionUsageCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "StorageTransactionUsageCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/StorageTransactionUsageCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(StorageTransactionUsageCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "StorageTransactionUsageCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static StorageTransactionUsageCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveStorageTransactionUsageCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: StorageTransactionUsageCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(StorageTransactionUsageCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static StorageTransactionUsageCollection RetrieveStorageTransactionUsageCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (StorageTransactionUsageCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(StorageTransactionUsageCollection), null, owner);
                    return result;
                }

				public static StorageTransactionUsageCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = StorageTransactionUsageCollection.RetrieveStorageTransactionUsageCollection("Content/TheBall.CORE/StorageTransactionUsageCollection/" + contentName, containerOwner);
					var result = StorageTransactionUsageCollection.RetrieveStorageTransactionUsageCollection("TheBall.CORE/StorageTransactionUsageCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/StorageTransactionUsageCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/StorageTransactionUsageCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((StorageTransactionUsageCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(StorageTransactionUsageCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static StorageTransactionUsageCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(StorageTransactionUsageCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (StorageTransactionUsageCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "StorageTransactionUsageCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "StorageTransactionUsageCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref StorageTransactionUsageCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = StorageTransactionUsage.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static StorageTransactionUsageCollection CreateDefault()
				{
					var result = new StorageTransactionUsageCollection();
					return result;
				}

				public static StorageTransactionUsageCollection CreateDemoDefault()
				{
					StorageTransactionUsageCollection customDemo = null;
					StorageTransactionUsageCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new StorageTransactionUsageCollection();
					result.CollectionContent.Add(StorageTransactionUsage.CreateDemoDefault());
					//result.CollectionContent.Add(StorageTransactionUsage.CreateDemoDefault());
					//result.CollectionContent.Add(StorageTransactionUsage.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<StorageTransactionUsage> CollectionContent = new List<StorageTransactionUsage>();
				private StorageTransactionUsage[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public StorageTransactionUsage[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (StorageTransactionUsage )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(StorageTransactionUsageCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class StorageTransactionUsage : IInformationObject 
			{
				public StorageTransactionUsage()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "StorageTransactionUsage";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/StorageTransactionUsage/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(StorageTransactionUsage), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "StorageTransactionUsage", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static StorageTransactionUsage RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveStorageTransactionUsage(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: StorageTransactionUsage");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(StorageTransactionUsage), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static StorageTransactionUsage RetrieveStorageTransactionUsage(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (StorageTransactionUsage) StorageSupport.RetrieveInformation(relativeLocation, typeof(StorageTransactionUsage), null, owner);
                    return result;
                }

				public static StorageTransactionUsage RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = StorageTransactionUsage.RetrieveStorageTransactionUsage("Content/TheBall.CORE/StorageTransactionUsage/" + contentName, containerOwner);
					var result = StorageTransactionUsage.RetrieveStorageTransactionUsage("TheBall.CORE/StorageTransactionUsage/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/StorageTransactionUsage/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/StorageTransactionUsage/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((StorageTransactionUsage) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(StorageTransactionUsage));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static StorageTransactionUsage DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(StorageTransactionUsage));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (StorageTransactionUsage) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "StorageTransactionUsage", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "StorageTransactionUsage", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref StorageTransactionUsage customDemoObject);



				public static StorageTransactionUsage CreateDefault()
				{
					var result = new StorageTransactionUsage();
					result.TimeRange = TimeRange.CreateDefault();
					return result;
				}

				public static StorageTransactionUsage CreateDemoDefault()
				{
					StorageTransactionUsage customDemo = null;
					StorageTransactionUsage.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new StorageTransactionUsage();
					result.TimeRange = TimeRange.CreateDemoDefault();
					result.UsageType = @"StorageTransactionUsage.UsageType";

				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(TimeRange != null) {
						((IInformationObject) TimeRange).UpdateCollections(masterInstance);
					}

				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = TimeRange;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = TimeRange;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) TimeRange;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(TimeRange != _unmodified_TimeRange)
							return true;
						if(UsageType != _unmodified_UsageType)
							return true;
						if(AmountOfTransactions != _unmodified_AmountOfTransactions)
							return true;
						{
							IInformationObject item = (IInformationObject) TimeRange;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(TimeRange != null) {
						if(TimeRange.ID == replacingObject.ID)
							TimeRange = (TimeRange) replacingObject;
						else {
							IInformationObject iObject = TimeRange;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(StorageTransactionUsage sourceObject)
				{
					TimeRange = sourceObject.TimeRange;
					UsageType = sourceObject.UsageType;
					AmountOfTransactions = sourceObject.AmountOfTransactions;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_UsageType = UsageType;
					_unmodified_AmountOfTransactions = AmountOfTransactions;
				
					_unmodified_TimeRange = TimeRange;
					if(TimeRange != null)
						((IInformationObject) TimeRange).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "UsageType":
							UsageType = value;
							break;
						case "AmountOfTransactions":
							AmountOfTransactions = long.Parse(value);
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public TimeRange TimeRange { get; set; }
			private TimeRange _unmodified_TimeRange;
			[DataMember]
			public string UsageType { get; set; }
			private string _unmodified_UsageType;
			[DataMember]
			public long AmountOfTransactions { get; set; }
			private long _unmodified_AmountOfTransactions;
			
			}
			[DataContract]
			public partial class StorageUsageCollection : IInformationObject , IInformationCollection
			{
				public StorageUsageCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "StorageUsageCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/StorageUsageCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(StorageUsageCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "StorageUsageCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static StorageUsageCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveStorageUsageCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: StorageUsageCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(StorageUsageCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static StorageUsageCollection RetrieveStorageUsageCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (StorageUsageCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(StorageUsageCollection), null, owner);
                    return result;
                }

				public static StorageUsageCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = StorageUsageCollection.RetrieveStorageUsageCollection("Content/TheBall.CORE/StorageUsageCollection/" + contentName, containerOwner);
					var result = StorageUsageCollection.RetrieveStorageUsageCollection("TheBall.CORE/StorageUsageCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/StorageUsageCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/StorageUsageCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((StorageUsageCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(StorageUsageCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static StorageUsageCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(StorageUsageCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (StorageUsageCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "StorageUsageCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "StorageUsageCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref StorageUsageCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = StorageUsage.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static StorageUsageCollection CreateDefault()
				{
					var result = new StorageUsageCollection();
					return result;
				}

				public static StorageUsageCollection CreateDemoDefault()
				{
					StorageUsageCollection customDemo = null;
					StorageUsageCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new StorageUsageCollection();
					result.CollectionContent.Add(StorageUsage.CreateDemoDefault());
					//result.CollectionContent.Add(StorageUsage.CreateDemoDefault());
					//result.CollectionContent.Add(StorageUsage.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<StorageUsage> CollectionContent = new List<StorageUsage>();
				private StorageUsage[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public StorageUsage[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (StorageUsage )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(StorageUsageCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class StorageUsage : IInformationObject 
			{
				public StorageUsage()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "StorageUsage";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/StorageUsage/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(StorageUsage), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "StorageUsage", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static StorageUsage RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveStorageUsage(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: StorageUsage");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(StorageUsage), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static StorageUsage RetrieveStorageUsage(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (StorageUsage) StorageSupport.RetrieveInformation(relativeLocation, typeof(StorageUsage), null, owner);
                    return result;
                }

				public static StorageUsage RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = StorageUsage.RetrieveStorageUsage("Content/TheBall.CORE/StorageUsage/" + contentName, containerOwner);
					var result = StorageUsage.RetrieveStorageUsage("TheBall.CORE/StorageUsage/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/StorageUsage/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/StorageUsage/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((StorageUsage) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(StorageUsage));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static StorageUsage DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(StorageUsage));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (StorageUsage) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "StorageUsage", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "StorageUsage", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref StorageUsage customDemoObject);



				public static StorageUsage CreateDefault()
				{
					var result = new StorageUsage();
					return result;
				}

				public static StorageUsage CreateDemoDefault()
				{
					StorageUsage customDemo = null;
					StorageUsage.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new StorageUsage();
					result.UsageType = @"StorageUsage.UsageType";

					result.UsageUnit = @"StorageUsage.UsageUnit";

				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(SnapshotTime != _unmodified_SnapshotTime)
							return true;
						if(UsageType != _unmodified_UsageType)
							return true;
						if(UsageUnit != _unmodified_UsageUnit)
							return true;
						if(AmountOfUnits != _unmodified_AmountOfUnits)
							return true;
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
				}


				private void CopyContentFrom(StorageUsage sourceObject)
				{
					SnapshotTime = sourceObject.SnapshotTime;
					UsageType = sourceObject.UsageType;
					UsageUnit = sourceObject.UsageUnit;
					AmountOfUnits = sourceObject.AmountOfUnits;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_SnapshotTime = SnapshotTime;
					_unmodified_UsageType = UsageType;
					_unmodified_UsageUnit = UsageUnit;
					_unmodified_AmountOfUnits = AmountOfUnits;
				
				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "SnapshotTime":
							SnapshotTime = DateTime.Parse(value);
							break;
						case "UsageType":
							UsageType = value;
							break;
						case "UsageUnit":
							UsageUnit = value;
							break;
						case "AmountOfUnits":
							AmountOfUnits = double.Parse(value);
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public DateTime SnapshotTime { get; set; }
			private DateTime _unmodified_SnapshotTime;
			[DataMember]
			public string UsageType { get; set; }
			private string _unmodified_UsageType;
			[DataMember]
			public string UsageUnit { get; set; }
			private string _unmodified_UsageUnit;
			[DataMember]
			public double AmountOfUnits { get; set; }
			private double _unmodified_AmountOfUnits;
			
			}
			[DataContract]
			public partial class NetworkUsageCollection : IInformationObject , IInformationCollection
			{
				public NetworkUsageCollection()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "NetworkUsageCollection";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/NetworkUsageCollection/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(NetworkUsageCollection), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "NetworkUsageCollection", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static NetworkUsageCollection RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveNetworkUsageCollection(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: NetworkUsageCollection");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(NetworkUsageCollection), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static NetworkUsageCollection RetrieveNetworkUsageCollection(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (NetworkUsageCollection) StorageSupport.RetrieveInformation(relativeLocation, typeof(NetworkUsageCollection), null, owner);
                    return result;
                }

				public static NetworkUsageCollection RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = NetworkUsageCollection.RetrieveNetworkUsageCollection("Content/TheBall.CORE/NetworkUsageCollection/" + contentName, containerOwner);
					var result = NetworkUsageCollection.RetrieveNetworkUsageCollection("TheBall.CORE/NetworkUsageCollection/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/NetworkUsageCollection/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/NetworkUsageCollection/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((NetworkUsageCollection) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(NetworkUsageCollection));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static NetworkUsageCollection DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(NetworkUsageCollection));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (NetworkUsageCollection) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "NetworkUsageCollection", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "NetworkUsageCollection", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref NetworkUsageCollection customDemoObject);


				
				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.UpdateCollections(masterInstance);
					}
				}



				bool IInformationCollection.IsMasterCollection {
					get {
						return false;
					}
				}

				string IInformationCollection.GetMasterLocation()
				{
					throw new NotSupportedException("Master collection location only supported for master collections");
					
				}

				IInformationCollection IInformationCollection.GetMasterInstance()
				{
					throw new NotSupportedException("Master collection instance only supported for master collections");
					
				}


				public string GetItemDirectory()
				{
					string dummyItemLocation = NetworkUsage.GetRelativeLocationFromID("dummy");
					string nonOwnerDirectoryLocation = SubscribeSupport.GetParentDirectoryTarget(dummyItemLocation);
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					string ownerDirectoryLocation = StorageSupport.GetBlobOwnerAddress(owner, nonOwnerDirectoryLocation);
					return ownerDirectoryLocation;
				}

				public void RefreshContent()
				{
				}


				public void SubscribeToContentSource()
				{
				}




                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				
		
				public static NetworkUsageCollection CreateDefault()
				{
					var result = new NetworkUsageCollection();
					return result;
				}

				public static NetworkUsageCollection CreateDemoDefault()
				{
					NetworkUsageCollection customDemo = null;
					NetworkUsageCollection.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new NetworkUsageCollection();
					result.CollectionContent.Add(NetworkUsage.CreateDemoDefault());
					//result.CollectionContent.Add(NetworkUsage.CreateDemoDefault());
					//result.CollectionContent.Add(NetworkUsage.CreateDemoDefault());
					return result;
				}

		
				[DataMember] public List<NetworkUsage> CollectionContent = new List<NetworkUsage>();
				private NetworkUsage[] _unmodified_CollectionContent;

				[DataMember] public bool IsCollectionFiltered;
				private bool _unmodified_IsCollectionFiltered;
				
				[DataMember] public List<string> OrderFilterIDList = new List<string>();
				private string[] _unmodified_OrderFilterIDList;

				public string SelectedIDCommaSeparated
				{
					get
					{
						string[] sourceArray;
						if (OrderFilterIDList != null)
							sourceArray = OrderFilterIDList.ToArray();
						else
							sourceArray = CollectionContent.Select(item => item.ID).ToArray();
						return String.Join(",", sourceArray);
					}
					set 
					{
						if (value == null)
							return;
						string[] valueArray = value.Split(',');
						OrderFilterIDList = new List<string>();
						OrderFilterIDList.AddRange(valueArray);
						OrderFilterIDList.RemoveAll(item => CollectionContent.Any(colItem => colItem.ID == item) == false);
					}
				}

				public NetworkUsage[] GetIDSelectedArray()
				{
					if (IsCollectionFiltered == false || this.OrderFilterIDList == null)
						return CollectionContent.ToArray();
					return
						this.OrderFilterIDList.Select(id => CollectionContent.FirstOrDefault(item => item.ID == id)).Where(item => item != null).ToArray();
				}


				public void ParsePropertyValue(string propertyName, string propertyValue)
				{
					switch(propertyName)
					{
						case "SelectedIDCommaSeparated":
							SelectedIDCommaSeparated = propertyValue;
							break;
						case "IsCollectionFiltered":
							IsCollectionFiltered = bool.Parse(propertyValue);
							break;
						default:
							throw new NotSupportedException("No ParsePropertyValue supported for property: " + propertyName);
					}
				}


				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					for(int i = 0; i < CollectionContent.Count; i++) // >
					{
						if(CollectionContent[i].ID == replacingObject.ID)
							CollectionContent[i] = (NetworkUsage )replacingObject;
						else { // Cannot have circular reference, so can be in else branch
							IInformationObject iObject = CollectionContent[i];
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}

				
				bool IInformationObject.IsInstanceTreeModified {
					get {
						bool collectionModified = CollectionContent.SequenceEqual(_unmodified_CollectionContent) == false;
						if(collectionModified)
							return true;
						//if((OrderFilterIDList == null && _unmodified_OrderFilterIDList != null) || _unmodified_OrderFilterIDList
						if(IsCollectionFiltered != _unmodified_IsCollectionFiltered)
							return true;
						// For non-master content
						foreach(IInformationObject item in CollectionContent)
						{
							bool itemTreeModified = item.IsInstanceTreeModified;
							if(itemTreeModified)
								return true;
						}
							
						return false;
					}
				}
				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_CollectionContent = CollectionContent.ToArray();
					_unmodified_IsCollectionFiltered = IsCollectionFiltered;
					if(OrderFilterIDList == null)
						_unmodified_OrderFilterIDList = null;
					else
						_unmodified_OrderFilterIDList = OrderFilterIDList.ToArray();
					foreach(IInformationObject iObject in CollectionContent)
						iObject.SetInstanceTreeValuesAsUnmodified();
				}

				private void CopyContentFrom(NetworkUsageCollection sourceObject)
				{
					CollectionContent = sourceObject.CollectionContent;
					_unmodified_CollectionContent = sourceObject._unmodified_CollectionContent;
				}
				
				private object FindFromObjectTree(string objectId)
				{
					foreach(var item in CollectionContent)
					{
						object result = item.FindObjectByID(objectId);
						if(result != null)
							return result;
					}
					return null;
				}

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					foreach(IInformationObject iObject in CollectionContent)
						iObject.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
				}


				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						bool doAdd = true;
						if(filterOnFalse != null)
							doAdd = filterOnFalse(iObject);
						if(doAdd) {
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					foreach(IInformationObject item in CollectionContent)
					{
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}
				}


			
			}
			[DataContract]
			public partial class NetworkUsage : IInformationObject 
			{
				public NetworkUsage()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "NetworkUsage";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/NetworkUsage/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(NetworkUsage), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "NetworkUsage", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static NetworkUsage RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveNetworkUsage(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: NetworkUsage");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(NetworkUsage), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static NetworkUsage RetrieveNetworkUsage(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (NetworkUsage) StorageSupport.RetrieveInformation(relativeLocation, typeof(NetworkUsage), null, owner);
                    return result;
                }

				public static NetworkUsage RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = NetworkUsage.RetrieveNetworkUsage("Content/TheBall.CORE/NetworkUsage/" + contentName, containerOwner);
					var result = NetworkUsage.RetrieveNetworkUsage("TheBall.CORE/NetworkUsage/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/NetworkUsage/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/NetworkUsage/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((NetworkUsage) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(NetworkUsage));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static NetworkUsage DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(NetworkUsage));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (NetworkUsage) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "NetworkUsage", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "NetworkUsage", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref NetworkUsage customDemoObject);



				public static NetworkUsage CreateDefault()
				{
					var result = new NetworkUsage();
					result.TimeRange = TimeRange.CreateDefault();
					return result;
				}

				public static NetworkUsage CreateDemoDefault()
				{
					NetworkUsage customDemo = null;
					NetworkUsage.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new NetworkUsage();
					result.TimeRange = TimeRange.CreateDemoDefault();
					result.UsageType = @"NetworkUsage.UsageType";

				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
					if(TimeRange != null) {
						((IInformationObject) TimeRange).UpdateCollections(masterInstance);
					}

				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					{ // Scoping block for variable name reusability
						IInformationObject item = TimeRange;
						if(item != null)
						{
							item.FindObjectsFromTree(result, filterOnFalse, searchWithinCurrentMasterOnly);
						}
					} // Scoping block end

					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					{
						var item = TimeRange;
						if(item != null)
						{
							object result = item.FindObjectByID(objectId);
							if(result != null)
								return result;
						}
					}
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}
					{
						var item = (IInformationObject) TimeRange;
						if(item != null)
							item.CollectMasterObjectsFromTree(result, filterOnFalse);
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(TimeRange != _unmodified_TimeRange)
							return true;
						if(UsageType != _unmodified_UsageType)
							return true;
						if(AmountOfBytes != _unmodified_AmountOfBytes)
							return true;
						{
							IInformationObject item = (IInformationObject) TimeRange;
							if(item != null) 
							{
								bool isItemTreeModified = item.IsInstanceTreeModified;
								if(isItemTreeModified)
									return true;
							}
						}
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
					if(TimeRange != null) {
						if(TimeRange.ID == replacingObject.ID)
							TimeRange = (TimeRange) replacingObject;
						else {
							IInformationObject iObject = TimeRange;
							iObject.ReplaceObjectInTree(replacingObject);
						}
					}
				}


				private void CopyContentFrom(NetworkUsage sourceObject)
				{
					TimeRange = sourceObject.TimeRange;
					UsageType = sourceObject.UsageType;
					AmountOfBytes = sourceObject.AmountOfBytes;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_UsageType = UsageType;
					_unmodified_AmountOfBytes = AmountOfBytes;
				
					_unmodified_TimeRange = TimeRange;
					if(TimeRange != null)
						((IInformationObject) TimeRange).SetInstanceTreeValuesAsUnmodified();

				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "UsageType":
							UsageType = value;
							break;
						case "AmountOfBytes":
							AmountOfBytes = long.Parse(value);
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public TimeRange TimeRange { get; set; }
			private TimeRange _unmodified_TimeRange;
			[DataMember]
			public string UsageType { get; set; }
			private string _unmodified_UsageType;
			[DataMember]
			public long AmountOfBytes { get; set; }
			private long _unmodified_AmountOfBytes;
			
			}
			[DataContract]
			public partial class TimeRange : IInformationObject 
			{
				public TimeRange()
				{
					this.ID = Guid.NewGuid().ToString();
				    this.OwnerID = StorageSupport.ActiveOwnerID;
				    this.SemanticDomainName = "TheBall.CORE";
				    this.Name = "TimeRange";
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static IInformationObject[] RetrieveCollectionFromOwnerContent(IContainerOwner owner)
				{
					//string contentTypeName = ""; // SemanticDomainName + "." + Name
					string contentTypeName = "TheBall.CORE/TimeRange/";
					List<IInformationObject> informationObjects = new List<IInformationObject>();
					var blobListing = StorageSupport.GetContentBlobListing(owner, contentType: contentTypeName);
					foreach(CloudBlockBlob blob in blobListing)
					{
						if (blob.GetBlobInformationType() != StorageSupport.InformationType_InformationObjectValue)
							continue;
						IInformationObject informationObject = StorageSupport.RetrieveInformation(blob.Name, typeof(TimeRange), null, owner);
					    informationObject.MasterETag = informationObject.ETag;
						informationObjects.Add(informationObject);
					}
					return informationObjects.ToArray();
				}

                public static string GetRelativeLocationFromID(string id)
                {
                    return Path.Combine("TheBall.CORE", "TimeRange", id).Replace("\\", "/");
                }

				public void UpdateRelativeLocationFromID()
				{
					RelativeLocation = GetRelativeLocationFromID(ID);
				}

				public static TimeRange RetrieveFromDefaultLocation(string id, IContainerOwner owner = null)
				{
					string relativeLocation = GetRelativeLocationFromID(id);
					return RetrieveTimeRange(relativeLocation, owner);
				}

				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing, out bool initiated)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster == false)
						throw new NotSupportedException("Cannot retrieve master for non-master type: TimeRange");
					initiated = false;
					VirtualOwner owner = VirtualOwner.FigureOwner(this);
					var master = StorageSupport.RetrieveInformation(RelativeLocation, typeof(TimeRange), null, owner);
					if(master == null && initiateIfMissing)
					{
						StorageSupport.StoreInformation(this, owner);
						master = this;
						initiated = true;
					}
					return master;
				}


				IInformationObject IInformationObject.RetrieveMaster(bool initiateIfMissing)
				{
					bool initiated;
					IInformationObject iObject = this;
					return iObject.RetrieveMaster(initiateIfMissing, out initiated);
				}


                public static TimeRange RetrieveTimeRange(string relativeLocation, IContainerOwner owner = null)
                {
                    var result = (TimeRange) StorageSupport.RetrieveInformation(relativeLocation, typeof(TimeRange), null, owner);
                    return result;
                }

				public static TimeRange RetrieveFromOwnerContent(IContainerOwner containerOwner, string contentName)
				{
					// var result = TimeRange.RetrieveTimeRange("Content/TheBall.CORE/TimeRange/" + contentName, containerOwner);
					var result = TimeRange.RetrieveTimeRange("TheBall.CORE/TimeRange/" + contentName, containerOwner);
					return result;
				}

				public void SetLocationAsOwnerContent(IContainerOwner containerOwner, string contentName)
                {
                    // RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "Content/TheBall.CORE/TimeRange/" + contentName);
                    RelativeLocation = StorageSupport.GetBlobOwnerAddress(containerOwner, "TheBall.CORE/TimeRange/" + contentName);
                }

				partial void DoInitializeDefaultSubscribers(IContainerOwner owner);

			    public void InitializeDefaultSubscribers(IContainerOwner owner)
			    {
					DoInitializeDefaultSubscribers(owner);
			    }

				partial void DoPostStoringExecute(IContainerOwner owner);

				public void PostStoringExecute(IContainerOwner owner)
				{
					DoPostStoringExecute(owner);
				}

				partial void DoPostDeleteExecute(IContainerOwner owner);

				public void PostDeleteExecute(IContainerOwner owner)
				{
					DoPostDeleteExecute(owner);
				}


			    public void SetValuesToObjects(NameValueCollection nameValueCollection)
			    {
                    foreach(string key in nameValueCollection.AllKeys)
                    {
                        if (key.StartsWith("Root"))
                            continue;
                        int indexOfUnderscore = key.IndexOf("_");
						if (indexOfUnderscore < 0) // >
                            continue;
                        string objectID = key.Substring(0, indexOfUnderscore);
                        object targetObject = FindObjectByID(objectID);
                        if (targetObject == null)
                            continue;
                        string propertyName = key.Substring(indexOfUnderscore + 1);
                        string propertyValue = nameValueCollection[key];
                        dynamic dyn = targetObject;
                        dyn.ParsePropertyValue(propertyName, propertyValue);
                    }
			    }

			    public object FindObjectByID(string objectId)
			    {
                    if (objectId == ID)
                        return this;
			        return FindFromObjectTree(objectId);
			    }

				bool IInformationObject.IsIndependentMaster { 
					get {
						return false;
					}
				}

				void IInformationObject.UpdateMasterValueTreeFromOtherInstance(IInformationObject sourceMaster)
				{
					if (sourceMaster == null)
						throw new ArgumentNullException("sourceMaster");
					if (GetType() != sourceMaster.GetType())
						throw new InvalidDataException("Type mismatch in UpdateMasterValueTree");
					IInformationObject iObject = this;
					if(iObject.IsIndependentMaster == false)
						throw new InvalidDataException("UpdateMasterValueTree called on non-master type");
					if(ID != sourceMaster.ID)
						throw new InvalidDataException("UpdateMasterValueTree is supported only on masters with same ID");
					CopyContentFrom((TimeRange) sourceMaster);
				}


				Dictionary<string, List<IInformationObject>> IInformationObject.CollectMasterObjects(Predicate<IInformationObject> filterOnFalse)
				{
					Dictionary<string, List<IInformationObject>> result = new Dictionary<string, List<IInformationObject>>();
					IInformationObject iObject = (IInformationObject) this;
					iObject.CollectMasterObjectsFromTree(result, filterOnFalse);
					return result;
				}

				public string SerializeToXml(bool noFormatting = false)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(TimeRange));
					using (var output = new StringWriter())
					{
						using (var writer = new XmlTextWriter(output))
						{
                            if(noFormatting == false)
						        writer.Formatting = Formatting.Indented;
							serializer.WriteObject(writer, this);
						}
						return output.GetStringBuilder().ToString();
					}
				}

				public static TimeRange DeserializeFromXml(string xmlString)
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(TimeRange));
					using(StringReader reader = new StringReader(xmlString))
					{
						using (var xmlReader = new XmlTextReader(reader))
							return (TimeRange) serializer.ReadObject(xmlReader);
					}
            
				}

				[DataMember]
				public string ID { get; set; }

			    [IgnoreDataMember]
                public string ETag { get; set; }

                [DataMember]
                public Guid OwnerID { get; set; }

                [DataMember]
                public string RelativeLocation { get; set; }

                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public string SemanticDomainName { get; set; }

				[DataMember]
				public string MasterETag { get; set; }

				public void SetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					RelativeLocation = GetRelativeLocationAsMetadataTo(masterRelativeLocation);
				}

				public static string GetRelativeLocationAsMetadataTo(string masterRelativeLocation)
				{
					return Path.Combine("TheBall.CORE", "TimeRange", masterRelativeLocation + ".metadata").Replace("\\", "/"); 
				}

				public void SetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
				{
				    RelativeLocation = GetLocationRelativeToContentRoot(referenceLocation, sourceName);
				}

                public string GetLocationRelativeToContentRoot(string referenceLocation, string sourceName)
                {
                    string relativeLocation;
                    if (String.IsNullOrEmpty(sourceName))
                        sourceName = "default";
                    string contentRootLocation = StorageSupport.GetContentRootLocation(referenceLocation);
                    relativeLocation = Path.Combine(contentRootLocation, "TheBall.CORE", "TimeRange", sourceName).Replace("\\", "/");
                    return relativeLocation;
                }

				static partial void CreateCustomDemo(ref TimeRange customDemoObject);



				public static TimeRange CreateDefault()
				{
					var result = new TimeRange();
					return result;
				}

				public static TimeRange CreateDemoDefault()
				{
					TimeRange customDemo = null;
					TimeRange.CreateCustomDemo(ref customDemo);
					if(customDemo != null)
						return customDemo;
					var result = new TimeRange();
				
					return result;
				}


				void IInformationObject.UpdateCollections(IInformationCollection masterInstance)
				{
					//Type collType = masterInstance.GetType();
					//string typeName = collType.Name;
				}


                public void SetMediaContent(IContainerOwner containerOwner, string contentObjectID, object mediaContent)
                {
                    IInformationObject targetObject = (IInformationObject) FindObjectByID(contentObjectID);
                    if (targetObject == null)
                        return;
					if(targetObject == this)
						throw new InvalidDataException("SetMediaContent referring to self (not media container)");
                    targetObject.SetMediaContent(containerOwner, contentObjectID, mediaContent);
                }

				void IInformationObject.FindObjectsFromTree(List<IInformationObject> result, Predicate<IInformationObject> filterOnFalse, bool searchWithinCurrentMasterOnly)
				{
					if(filterOnFalse(this))
						result.Add(this);
					if(searchWithinCurrentMasterOnly == false)
					{
					}					
				}


				private object FindFromObjectTree(string objectId)
				{
					return null;
				}

				void IInformationObject.CollectMasterObjectsFromTree(Dictionary<string, List<IInformationObject>> result, Predicate<IInformationObject> filterOnFalse)
				{
					IInformationObject iObject = (IInformationObject) this;
					if(iObject.IsIndependentMaster)
					{
						if(filterOnFalse == null || filterOnFalse(iObject)) 
						{
							string key = iObject.ID;
							List<IInformationObject> existingValue;
							bool keyFound = result.TryGetValue(key, out existingValue);
							if(keyFound == false) {
								existingValue = new List<IInformationObject>();
								result.Add(key, existingValue);
							}
							existingValue.Add(iObject);
						}
					}

				}

				bool IInformationObject.IsInstanceTreeModified {
					get {
						if(StartTime != _unmodified_StartTime)
							return true;
						if(EndTime != _unmodified_EndTime)
							return true;
				
						return false;
					}
				}

				void IInformationObject.ReplaceObjectInTree(IInformationObject replacingObject)
				{
				}


				private void CopyContentFrom(TimeRange sourceObject)
				{
					StartTime = sourceObject.StartTime;
					EndTime = sourceObject.EndTime;
				}
				


				void IInformationObject.SetInstanceTreeValuesAsUnmodified()
				{
					_unmodified_StartTime = StartTime;
					_unmodified_EndTime = EndTime;
				
				
				}




				public void ParsePropertyValue(string propertyName, string value)
				{
					switch (propertyName)
					{
						case "StartTime":
							StartTime = DateTime.Parse(value);
							break;
						case "EndTime":
							EndTime = DateTime.Parse(value);
							break;
						default:
							throw new InvalidDataException("Primitive parseable data type property not found: " + propertyName);
					}
	        }
			[DataMember]
			public DateTime StartTime { get; set; }
			private DateTime _unmodified_StartTime;
			[DataMember]
			public DateTime EndTime { get; set; }
			private DateTime _unmodified_EndTime;
			
			}
 } 