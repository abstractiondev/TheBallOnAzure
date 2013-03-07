using System;
using TheBall;
using TheBall.CORE;

namespace AaltoGlobalImpact.OIP
{
    partial class TBRLoginRoot
    {
        public static TBRLoginRoot GetOrCreateLoginRootWithAccount(string loginUrl, bool isAccountRequest)
        {
            string loginRootID = TBLoginInfo.GetLoginIDFromLoginURL(loginUrl);
            var loginRoot = RetrieveFromDefaultLocation(loginRootID);
            if (loginRoot == null)
            {
                // Storing loginroot
                loginRoot = TBRLoginRoot.CreateDefault();
                loginRoot.ID = loginRootID;
                loginRoot.UpdateRelativeLocationFromID();
                StorageSupport.StoreInformation(loginRoot);

                // Creating login info for account and storing the account
                TBLoginInfo loginInfo = TBLoginInfo.CreateDefault();
                loginInfo.OpenIDUrl = loginUrl;

                TBRAccountRoot accountRoot = TBRAccountRoot.CreateAndStoreNewAccount();
                accountRoot.Account.Logins.CollectionContent.Add(loginInfo);
                string accountID = accountRoot.ID;
                accountRoot.StoreInformation();

                UpdateAccountRootToReferences.Execute(new UpdateAccountRootToReferencesParameters
                {
                    AccountID = accountID
                });
                UpdateAccountContainerFromAccountRoot.Execute(new UpdateAccountContainerFromAccountRootParameters
                {
                    AccountID = accountID
                });

                // If this request is for account, we propagate the pages immediately
                bool useBackgroundWorker = isAccountRequest == false;
                RenderWebSupport.RefreshAccountTemplates(accountRoot.ID, useBackgroundWorker);
                if(isAccountRequest)
                {

                    accountRoot.Account.InitializeAndConnectMastersAndCollections();
                }
            }
            loginRoot = RetrieveFromDefaultLocation(loginRootID);
            return loginRoot;
        }

    }
}