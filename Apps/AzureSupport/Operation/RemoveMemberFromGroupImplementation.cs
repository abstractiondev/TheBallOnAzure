using System;
using System.IO;
using System.Linq;
using TheBall;

namespace AaltoGlobalImpact.OIP
{
    public static class RemoveMemberFromGroupImplementation
    {
        public static TBRGroupRoot GetTarget_GroupRoot(string groupID)
        {
            return TBRGroupRoot.RetrieveFromDefaultLocation(groupID);
        }

        public static TBRAccountRoot GetTarget_AccountRoot(string accountID)
        {
            var accountRoot = TBRAccountRoot.RetrieveFromDefaultLocation(accountID);
            if(accountRoot == null)
                throw new InvalidDataException("AccountRoot not found for: " + accountID);
            return accountRoot;
        }

        public static string GetTarget_MemberAccountID(string memberEmailAddress, string accountID)
        {
            if(memberEmailAddress != null && accountID != null)
                throw new NotSupportedException("Both parameters email and accountID not supported");
            if (accountID != null)
                return accountID;
            string emailRootID = TBREmailRoot.GetIDFromEmailAddress(memberEmailAddress);
            TBREmailRoot emailRoot = TBREmailRoot.RetrieveFromDefaultLocation(emailRootID);
            return emailRoot.Account.ID;
        }

        public static string GetTarget_MemberEmailAddress(string emailAddress, TBRAccountRoot accountRoot, TBRGroupRoot groupRoot)
        {
            if (emailAddress != null)
                return emailAddress;
            var groupEmails = groupRoot.Group.Roles.CollectionContent.Select(role => role.Email.EmailAddress).ToArray();
            var matchingEmail =
                accountRoot.Account.Emails.CollectionContent.FirstOrDefault(
                    email => groupEmails.Contains(email.EmailAddress));
            if(matchingEmail == null)
                throw new InvalidDataException("Account email not found on group in RemoveMemberFromGroupImplementation");
            return matchingEmail.EmailAddress;
        }


        public static void ExecuteMethod_RemoveMemberFromGroup(string memberEmailAddress, TBRGroupRoot groupRoot)
        {
            groupRoot.Group.Roles.CollectionContent.
                RemoveAll(role =>
                              {
                                  bool toRemove = role.Email.EmailAddress == memberEmailAddress;
                                  if(toRemove && TBCollaboratorRole.HasInitiatorRights(role.Role))
                                      throw new InvalidOperationException("Cannot remove initiator from group");
                                  return toRemove;
                              });
        }

        public static void ExecuteMethod_StoreObjects(TBRGroupRoot groupRoot)
        {
            groupRoot.StoreInformation();
        }

        public static RefreshAccountGroupMembershipsParameters RefreshAccountAndGroupContainers_GetParameters(TBRGroupRoot groupRoot, string accountID)
        {
            return new RefreshAccountGroupMembershipsParameters {AccountID = accountID, GroupRoot = groupRoot};
        }

    }
}