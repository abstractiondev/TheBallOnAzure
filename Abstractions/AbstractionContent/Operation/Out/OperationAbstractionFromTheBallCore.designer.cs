 

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;

		namespace AaltoGlobalImpact.OIP { 
				public class CreateGroupParameters 
		{
				public string GroupName ;
				public string AccountID ;
				}
		
		public class CreateGroup 
		{
				private static void PrepareParameters(CreateGroupParameters parameters)
		{
					}
				public static void Execute(CreateGroupParameters parameters)
		{
						PrepareParameters(parameters);
					TBRGroupRoot GroupRoot = CreateGroupImplementation.GetTarget_GroupRoot(parameters.GroupName);	
				TBRAccountRoot AccountRoot = CreateGroupImplementation.GetTarget_AccountRoot(parameters.AccountID);	
				TBEmail[] AccountEmails = CreateGroupImplementation.GetTarget_AccountEmails(AccountRoot);	
				CreateGroupImplementation.ExecuteMethod_AddAsInitiatorToGroupRoot(GroupRoot, AccountEmails);		
				CreateGroupImplementation.ExecuteMethod_StoreObjects(GroupRoot);		
				CreateGroupImplementation.ExecuteMethod_InitializeGroupContentAndMasters(GroupRoot);		
				
		{ // Local block to allow local naming
			RefreshAccountGroupMembershipsParameters operationParameters = CreateGroupImplementation.RefreshAccountAndGroupContainers_GetParameters(parameters.AccountID, GroupRoot);
			RefreshAccountGroupMemberships.Execute(operationParameters);
									
		} // Local block closing
				}
				}
				public class InviteMemberToGroupParameters 
		{
				public string MemberEmailAddress ;
				public string GroupID ;
				}
		
		public class InviteMemberToGroup 
		{
				private static void PrepareParameters(InviteMemberToGroupParameters parameters)
		{
					}
				public static void Execute(InviteMemberToGroupParameters parameters)
		{
						PrepareParameters(parameters);
					TBRGroupRoot GroupRoot = InviteMemberToGroupImplementation.GetTarget_GroupRoot(parameters.GroupID);	
				TBEmailValidation EmailValidation = InviteMemberToGroupImplementation.GetTarget_EmailValidation(parameters.MemberEmailAddress, parameters.GroupID);	
				string AccountID = InviteMemberToGroupImplementation.GetTarget_AccountID(parameters.MemberEmailAddress);	
				InviteMemberToGroupImplementation.ExecuteMethod_AddAsPendingInvitationToGroupRoot(parameters.MemberEmailAddress, GroupRoot);		
				InviteMemberToGroupImplementation.ExecuteMethod_StoreObjects(GroupRoot, EmailValidation);		
				InviteMemberToGroupImplementation.ExecuteMethod_SendEmailConfirmation(EmailValidation, GroupRoot);		
				
		{ // Local block to allow local naming
			RefreshAccountGroupMembershipsParameters operationParameters = InviteMemberToGroupImplementation.RefreshAccountAndGroupContainers_GetParameters(GroupRoot, AccountID);
			RefreshAccountGroupMemberships.Execute(operationParameters);
									
		} // Local block closing
				}
				}
				public class RemoveMemberFromGroupParameters 
		{
				public string EmailAddress ;
				public string AccountID ;
				public string GroupID ;
				}
		
		public class RemoveMemberFromGroup 
		{
				private static void PrepareParameters(RemoveMemberFromGroupParameters parameters)
		{
					}
				public static void Execute(RemoveMemberFromGroupParameters parameters)
		{
						PrepareParameters(parameters);
					TBRGroupRoot GroupRoot = RemoveMemberFromGroupImplementation.GetTarget_GroupRoot(parameters.GroupID);	
				string MemberAccountID = RemoveMemberFromGroupImplementation.GetTarget_MemberAccountID(parameters.EmailAddress, parameters.AccountID);	
				TBRAccountRoot AccountRoot = RemoveMemberFromGroupImplementation.GetTarget_AccountRoot(MemberAccountID);	
				string MemberEmailAddress = RemoveMemberFromGroupImplementation.GetTarget_MemberEmailAddress(parameters.EmailAddress, AccountRoot, GroupRoot);	
				RemoveMemberFromGroupImplementation.ExecuteMethod_RemoveMemberFromGroup(MemberEmailAddress, GroupRoot);		
				RemoveMemberFromGroupImplementation.ExecuteMethod_StoreObjects(GroupRoot);		
				
		{ // Local block to allow local naming
			RefreshAccountGroupMembershipsParameters operationParameters = RemoveMemberFromGroupImplementation.RefreshAccountAndGroupContainers_GetParameters(GroupRoot, MemberAccountID);
			RefreshAccountGroupMemberships.Execute(operationParameters);
									
		} // Local block closing
				}
				}
				public class ConfirmInviteToJoinGroupParameters 
		{
				public string MemberEmailAddress ;
				public string GroupID ;
				}
		
		public class ConfirmInviteToJoinGroup 
		{
				private static void PrepareParameters(ConfirmInviteToJoinGroupParameters parameters)
		{
					}
				public static void Execute(ConfirmInviteToJoinGroupParameters parameters)
		{
						PrepareParameters(parameters);
					TBRGroupRoot GroupRoot = ConfirmInviteToJoinGroupImplementation.GetTarget_GroupRoot(parameters.GroupID);	
				string AccountID = ConfirmInviteToJoinGroupImplementation.GetTarget_AccountID(parameters.MemberEmailAddress);	
				ConfirmInviteToJoinGroupImplementation.ExecuteMethod_ConfirmPendingInvitationToGroupRoot(parameters.MemberEmailAddress, GroupRoot);		
				ConfirmInviteToJoinGroupImplementation.ExecuteMethod_StoreObjects(GroupRoot);		
				
		{ // Local block to allow local naming
			RefreshAccountGroupMembershipsParameters operationParameters = ConfirmInviteToJoinGroupImplementation.RefreshAccountAndGroupContainers_GetParameters(GroupRoot, AccountID);
			RefreshAccountGroupMemberships.Execute(operationParameters);
									
		} // Local block closing
				}
				}
				public class AssignCollaboratorRoleParameters 
		{
				public string GroupID ;
				public GroupContainer GroupContainer ;
				public string CollaboratorID ;
				public string RoleToAssign ;
				}
		
		public class AssignCollaboratorRole 
		{
				private static void PrepareParameters(AssignCollaboratorRoleParameters parameters)
		{
					}
				public static void Execute(AssignCollaboratorRoleParameters parameters)
		{
						PrepareParameters(parameters);
					TBRGroupRoot GroupRoot = AssignCollaboratorRoleImplementation.GetTarget_GroupRoot(parameters.GroupID);	
				Collaborator Collaborator = AssignCollaboratorRoleImplementation.GetTarget_Collaborator(parameters.GroupContainer, parameters.CollaboratorID);	
				string AccountID = AssignCollaboratorRoleImplementation.GetTarget_AccountID(Collaborator);	
				TBRAccountRoot AccountRoot = AssignCollaboratorRoleImplementation.GetTarget_AccountRoot(AccountID);	
				string EmailAddress = AssignCollaboratorRoleImplementation.GetTarget_EmailAddress(GroupRoot, AccountRoot);	
				TBCollaboratorRole TBCollaboratorRole = AssignCollaboratorRoleImplementation.GetTarget_TBCollaboratorRole(GroupRoot, EmailAddress);	
				AssignCollaboratorRoleImplementation.ExecuteMethod_AssignCollaboratorRole(parameters.RoleToAssign, TBCollaboratorRole);		
				AssignCollaboratorRoleImplementation.ExecuteMethod_StoreObjects(GroupRoot);		
				
		{ // Local block to allow local naming
			RefreshAccountGroupMembershipsParameters operationParameters = AssignCollaboratorRoleImplementation.RefreshAccountAndGroupContainers_GetParameters(GroupRoot, AccountID);
			RefreshAccountGroupMemberships.Execute(operationParameters);
									
		} // Local block closing
				}
				}
				public class UpdateAccountContainerFromAccountRootParameters 
		{
				public string AccountID ;
				}
		
		public class UpdateAccountContainerFromAccountRoot 
		{
				private static void PrepareParameters(UpdateAccountContainerFromAccountRootParameters parameters)
		{
					}
				public static void Execute(UpdateAccountContainerFromAccountRootParameters parameters)
		{
						PrepareParameters(parameters);
					TBRAccountRoot AccountRoot = UpdateAccountContainerFromAccountRootImplementation.GetTarget_AccountRoot(parameters.AccountID);	
				AccountContainer AccountContainer = UpdateAccountContainerFromAccountRootImplementation.GetTarget_AccountContainer(AccountRoot);	
				UpdateAccountContainerFromAccountRootImplementation.ExecuteMethod_UpdateAccountContainerLogins(AccountRoot, AccountContainer);		
				UpdateAccountContainerFromAccountRootImplementation.ExecuteMethod_UpdateAccountContainerEmails(AccountRoot, AccountContainer);		
				UpdateAccountContainerFromAccountRootImplementation.ExecuteMethod_StoreObjects(AccountContainer);		
				}
				}
				public class UnlinkEmailAddressParameters 
		{
				public string AccountID ;
				public AccountContainer AccountContainerBeforeGroupRemoval ;
				public string EmailAddressID ;
				}
		
		public class UnlinkEmailAddress 
		{
				private static void PrepareParameters(UnlinkEmailAddressParameters parameters)
		{
					}
				public static void Execute(UnlinkEmailAddressParameters parameters)
		{
						PrepareParameters(parameters);
					TBRAccountRoot AccountRootBeforeGroupRemoval = UnlinkEmailAddressImplementation.GetTarget_AccountRootBeforeGroupRemoval(parameters.AccountID);	
				string EmailAddress = UnlinkEmailAddressImplementation.GetTarget_EmailAddress(parameters.AccountContainerBeforeGroupRemoval, parameters.EmailAddressID);	
				TBRGroupRoot[] GroupRoots = UnlinkEmailAddressImplementation.GetTarget_GroupRoots(AccountRootBeforeGroupRemoval, EmailAddress);	
				UnlinkEmailAddressImplementation.ExecuteMethod_RemoveGroupMemberships(EmailAddress, GroupRoots);		
				TBRAccountRoot AccountRootAfterGroupRemoval = UnlinkEmailAddressImplementation.GetTarget_AccountRootAfterGroupRemoval(parameters.AccountID);	
				TBREmailRoot EmailRoot = UnlinkEmailAddressImplementation.GetTarget_EmailRoot(EmailAddress);	
				UnlinkEmailAddressImplementation.ExecuteMethod_RemoveEmailFromAccountRoot(AccountRootAfterGroupRemoval, EmailAddress);		
				UnlinkEmailAddressImplementation.ExecuteMethod_DeleteEmailRoot(EmailRoot);		
				UnlinkEmailAddressImplementation.ExecuteMethod_StoreObjects(AccountRootAfterGroupRemoval);		
				
		{ // Local block to allow local naming
			UpdateAccountContainerFromAccountRootParameters operationParameters = UnlinkEmailAddressImplementation.UpdateAccountContainer_GetParameters(parameters.AccountID);
			UpdateAccountContainerFromAccountRoot.Execute(operationParameters);
									
		} // Local block closing
				
		{ // Local block to allow local naming
			UpdateAccountRootToReferencesParameters operationParameters = UnlinkEmailAddressImplementation.UpdateAccountRoot_GetParameters(parameters.AccountID);
			UpdateAccountRootToReferences.Execute(operationParameters);
									
		} // Local block closing
				}
				}
				public class UpdateAccountRootToReferencesParameters 
		{
				public string AccountID ;
				}
		
		public class UpdateAccountRootToReferences 
		{
				private static void PrepareParameters(UpdateAccountRootToReferencesParameters parameters)
		{
					}
				public static void Execute(UpdateAccountRootToReferencesParameters parameters)
		{
						PrepareParameters(parameters);
					TBRAccountRoot AccountRoot = UpdateAccountRootToReferencesImplementation.GetTarget_AccountRoot(parameters.AccountID);	
				TBRLoginRoot[] AccountLogins = UpdateAccountRootToReferencesImplementation.GetTarget_AccountLogins(AccountRoot);	
				TBREmailRoot[] AccountEmails = UpdateAccountRootToReferencesImplementation.GetTarget_AccountEmails(AccountRoot);	
				UpdateAccountRootToReferencesImplementation.ExecuteMethod_UpdateAccountToLogins(AccountRoot, AccountLogins);		
				UpdateAccountRootToReferencesImplementation.ExecuteMethod_UpdateAccountToEmails(AccountRoot, AccountEmails);		
				UpdateAccountRootToReferencesImplementation.ExecuteMethod_StoreObjects(AccountLogins, AccountEmails);		
				}
				}
				public class RefreshAccountGroupMembershipsParameters 
		{
				public TBRGroupRoot GroupRoot ;
				public string AccountID ;
				}
		
		public class RefreshAccountGroupMemberships 
		{
				private static void PrepareParameters(RefreshAccountGroupMembershipsParameters parameters)
		{
					}
				public static void Execute(RefreshAccountGroupMembershipsParameters parameters)
		{
						PrepareParameters(parameters);
					
		{ // Local block to allow local naming
			UpdateAccountRootGroupMembershipParameters operationParameters = RefreshAccountGroupMembershipsImplementation.UpdateAccountRoot_GetParameters(parameters.GroupRoot, parameters.AccountID);
			UpdateAccountRootGroupMembership.Execute(operationParameters);
									
		} // Local block closing
				
		{ // Local block to allow local naming
			UpdateAccountRootToReferencesParameters operationParameters = RefreshAccountGroupMembershipsImplementation.UpdateAccountRootReferences_GetParameters(parameters.AccountID);
			UpdateAccountRootToReferences.Execute(operationParameters);
									
		} // Local block closing
				
		{ // Local block to allow local naming
			UpdateLoginGroupPermissionsParameters operationParameters = RefreshAccountGroupMembershipsImplementation.UpdateAccountLoginGroups_GetParameters(parameters.AccountID);
			UpdateLoginGroupPermissions.Execute(operationParameters);
									
		} // Local block closing
				
		{ // Local block to allow local naming
			UpdateGroupContainersGroupMembershipParameters operationParameters = RefreshAccountGroupMembershipsImplementation.UpdateGroupContainers_GetParameters(parameters.GroupRoot);
			UpdateGroupContainersGroupMembership.Execute(operationParameters);
									
		} // Local block closing
				
		{ // Local block to allow local naming
			UpdateAccountContainersGroupMembershipParameters operationParameters = RefreshAccountGroupMembershipsImplementation.UpdateAccountContainers_GetParameters(parameters.GroupRoot, parameters.AccountID);
			UpdateAccountContainersGroupMembership.Execute(operationParameters);
									
		} // Local block closing
				}
				}
				public class UpdateGroupContainersGroupMembershipParameters 
		{
				public TBRGroupRoot GroupRoot ;
				}
		
		public class UpdateGroupContainersGroupMembership 
		{
				private static void PrepareParameters(UpdateGroupContainersGroupMembershipParameters parameters)
		{
					}
				public static void Execute(UpdateGroupContainersGroupMembershipParameters parameters)
		{
						PrepareParameters(parameters);
					AccountRootAndContainer[] AccountRootsAndContainers = UpdateGroupContainersGroupMembershipImplementation.GetTarget_AccountRootsAndContainers(parameters.GroupRoot);	
				GroupContainer GroupContainer = UpdateGroupContainersGroupMembershipImplementation.GetTarget_GroupContainer(parameters.GroupRoot);	
				UpdateGroupContainersGroupMembershipImplementation.ExecuteMethod_UpdateGroupContainerMembership(parameters.GroupRoot, AccountRootsAndContainers, GroupContainer);		
				UpdateGroupContainersGroupMembershipImplementation.ExecuteMethod_StoreObjects(GroupContainer);		
				}
				}
				public class DeleteInformationObjectParameters 
		{
				public TheBall.CORE.IInformationObject ObjectToDelete ;
				}
		
		public class DeleteInformationObject 
		{
				private static void PrepareParameters(DeleteInformationObjectParameters parameters)
		{
					}
				public static void Execute(DeleteInformationObjectParameters parameters)
		{
						PrepareParameters(parameters);
					DeleteInformationObjectImplementation.ExecuteMethod_DeleteObjectViews(parameters.ObjectToDelete);		
				DeleteInformationObjectImplementation.ExecuteMethod_DeleteObject(parameters.ObjectToDelete);		
				}
				}
				public class UpdateLoginGroupPermissionsParameters 
		{
				public string AccountID ;
				}
		
		public class UpdateLoginGroupPermissions 
		{
				private static void PrepareParameters(UpdateLoginGroupPermissionsParameters parameters)
		{
					}
				public static void Execute(UpdateLoginGroupPermissionsParameters parameters)
		{
						PrepareParameters(parameters);
					TBRAccountRoot AccountRoot = UpdateLoginGroupPermissionsImplementation.GetTarget_AccountRoot(parameters.AccountID);	
				TBRLoginGroupRoot[] LoginGroupRoots = UpdateLoginGroupPermissionsImplementation.GetTarget_LoginGroupRoots(AccountRoot);	
				UpdateLoginGroupPermissionsImplementation.ExecuteMethod_SynchronizeLoginGroupRoots(AccountRoot, LoginGroupRoots);		
				}
				}
				public class UpdateAccountRootGroupMembershipParameters 
		{
				public TBRGroupRoot GroupRoot ;
				public string AccountID ;
				}
		
		public class UpdateAccountRootGroupMembership 
		{
				private static void PrepareParameters(UpdateAccountRootGroupMembershipParameters parameters)
		{
					}
				public static void Execute(UpdateAccountRootGroupMembershipParameters parameters)
		{
						PrepareParameters(parameters);
					TBRAccountRoot AccountRoot = UpdateAccountRootGroupMembershipImplementation.GetTarget_AccountRoot(parameters.AccountID);	
				UpdateAccountRootGroupMembershipImplementation.ExecuteMethod_UpdateAccountRootGroupMemberships(parameters.GroupRoot, AccountRoot);		
				UpdateAccountRootGroupMembershipImplementation.ExecuteMethod_StoreObjects(AccountRoot);		
				}
				}
				public class UpdateAccountContainersGroupMembershipParameters 
		{
				public TBRGroupRoot GroupRoot ;
				public string AccountID ;
				}
		
		public class UpdateAccountContainersGroupMembership 
		{
				private static void PrepareParameters(UpdateAccountContainersGroupMembershipParameters parameters)
		{
					}
				public static void Execute(UpdateAccountContainersGroupMembershipParameters parameters)
		{
						PrepareParameters(parameters);
					GroupContainer GroupContainer = UpdateAccountContainersGroupMembershipImplementation.GetTarget_GroupContainer(parameters.GroupRoot);	
				Group Group = UpdateAccountContainersGroupMembershipImplementation.GetTarget_Group(GroupContainer);	
				TBRAccountRoot AccountRoot = UpdateAccountContainersGroupMembershipImplementation.GetTarget_AccountRoot(parameters.AccountID);	
				AccountContainer AccountContainer = UpdateAccountContainersGroupMembershipImplementation.GetTarget_AccountContainer(parameters.AccountID);	
				GroupSummaryContainer GroupSummaryContainer = UpdateAccountContainersGroupMembershipImplementation.GetTarget_GroupSummaryContainer(parameters.AccountID);	
				UpdateAccountContainersGroupMembershipImplementation.ExecuteMethod_UpdateGroupSummaryContainerMemberships(parameters.GroupRoot, Group, AccountRoot, GroupSummaryContainer);		
				UpdateAccountContainersGroupMembershipImplementation.ExecuteMethod_UpdateAccountContainerMemberships(parameters.GroupRoot, Group, GroupSummaryContainer, AccountRoot, AccountContainer);		
				UpdateAccountContainersGroupMembershipImplementation.ExecuteMethod_StoreObjects(AccountContainer, GroupSummaryContainer);		
				}
				}
				public class PerformWebActionParameters 
		{
				public string TargetObjectID ;
				public string CommandName ;
				public TheBall.CORE.IContainerOwner Owner ;
				public InformationSourceCollection InformationSources ;
				public string[] FormSourceNames ;
				public NameValueCollection FormSubmitContent ;
				}
		
		public class PerformWebAction 
		{
				private static void PrepareParameters(PerformWebActionParameters parameters)
		{
					}
				public static PerformWebActionReturnValue Execute(PerformWebActionParameters parameters)
		{
						PrepareParameters(parameters);
					bool ExecuteActualOperationOutput = PerformWebActionImplementation.ExecuteMethod_ExecuteActualOperation(parameters.TargetObjectID, parameters.CommandName, parameters.Owner, parameters.InformationSources, parameters.FormSourceNames, parameters.FormSubmitContent);		
				PerformWebActionReturnValue returnValue = PerformWebActionImplementation.Get_ReturnValue(ExecuteActualOperationOutput);
		return returnValue;
				}
				}
				public class PerformWebActionReturnValue 
		{
				public bool RenderPageAfterOperation ;
				}
				public class PublishGroupContentToPublicAreaParameters 
		{
				public string GroupID ;
				public bool UseWorker ;
				}
		
		public class PublishGroupContentToPublicArea 
		{
				private static void PrepareParameters(PublishGroupContentToPublicAreaParameters parameters)
		{
					}
				public static void Execute(PublishGroupContentToPublicAreaParameters parameters)
		{
						PrepareParameters(parameters);
					string CurrentContainerName = PublishGroupContentToPublicAreaImplementation.GetTarget_CurrentContainerName(parameters.GroupID);	
				string PublicContainerName = PublishGroupContentToPublicAreaImplementation.GetTarget_PublicContainerName(parameters.GroupID);	
				PublishGroupContentToPublicAreaImplementation.ExecuteMethod_PublishGroupContent(parameters.GroupID, parameters.UseWorker, CurrentContainerName, PublicContainerName);		
				}
				}
				public class PublishGroupContentToWwwParameters 
		{
				public string GroupID ;
				public bool UseWorker ;
				}
		
		public class PublishGroupContentToWww 
		{
				private static void PrepareParameters(PublishGroupContentToWwwParameters parameters)
		{
					}
				public static void Execute(PublishGroupContentToWwwParameters parameters)
		{
						PrepareParameters(parameters);
					string CurrentContainerName = PublishGroupContentToWwwImplementation.GetTarget_CurrentContainerName(parameters.GroupID);	
				string WwwContainerName = PublishGroupContentToWwwImplementation.GetTarget_WwwContainerName(parameters.GroupID);	
				PublishGroupContentToWwwImplementation.ExecuteMethod_PublishGroupContentToWww(parameters.GroupID, parameters.UseWorker, CurrentContainerName, WwwContainerName);		
				}
				}
				public class UpdatePageContentParameters 
		{
				public string changedInformation ;
				}
		
		public class UpdatePageContent 
		{
				private static void PrepareParameters(UpdatePageContentParameters parameters)
		{
					}
				public static void Execute(UpdatePageContentParameters parameters)
		{
						PrepareParameters(parameters);
					UpdatePageContentImplementation.ExecuteMethod_UpdatePage();		
				}
				}
				public class CreateAdditionalMediaFormatsParameters 
		{
				public string MasterRelativeLocation ;
				}
		
		public class CreateAdditionalMediaFormats 
		{
				private static void PrepareParameters(CreateAdditionalMediaFormatsParameters parameters)
		{
					}
				public static void Execute(CreateAdditionalMediaFormatsParameters parameters)
		{
						PrepareParameters(parameters);
					Bitmap BitmapData = CreateAdditionalMediaFormatsImplementation.GetTarget_BitmapData(parameters.MasterRelativeLocation);	
				object VideoData = CreateAdditionalMediaFormatsImplementation.GetTarget_VideoData(parameters.MasterRelativeLocation);	
				CreateAdditionalMediaFormatsImplementation.ExecuteMethod_CreateImageMediaFormats(parameters.MasterRelativeLocation, BitmapData);		
				CreateAdditionalMediaFormatsImplementation.ExecuteMethod_CreateVideoMediaFormats(parameters.MasterRelativeLocation, VideoData);		
				}
				}
				public class ClearAdditionalMediaFormatsParameters 
		{
				public string MasterRelativeLocation ;
				}
		
		public class ClearAdditionalMediaFormats 
		{
				private static void PrepareParameters(ClearAdditionalMediaFormatsParameters parameters)
		{
					}
				public static void Execute(ClearAdditionalMediaFormatsParameters parameters)
		{
						PrepareParameters(parameters);
					ClearAdditionalMediaFormatsImplementation.ExecuteMethod_ClearImageMediaFormats(parameters.MasterRelativeLocation);		
				}
				}
		 }  

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;

		namespace TheBall.CORE { 
				public class CreateInvoiceForGroupParameters 
		{
				public string GroupID ;
				}
		
		public class CreateInvoiceForGroup 
		{
				private static void PrepareParameters(CreateInvoiceForGroupParameters parameters)
		{
					}
				public static void Execute(CreateInvoiceForGroupParameters parameters)
		{
						PrepareParameters(parameters);
					TBRGroupRoot GroupRoot = CreateInvoiceForGroupImplementation.GetTarget_GroupRoot(parameters.GroupID);	
				}
				}
		 } 