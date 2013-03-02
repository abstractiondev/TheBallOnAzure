using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using TheBall;

namespace WebInterface
{
    public partial class TheBallLogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(Request.Params["SignOut"] != null)
            {
                AuthenticationSupport.ClearAuthenticationCookie(Response);
                Response.Redirect("/", true);
                return;
            }
            openIdBox.Focus();         
            OpenIdRelyingParty openid = new OpenIdRelyingParty();        
            var response = openid.GetResponse();
            ClaimsResponse profileFields;
            string friendlyName;
            string idprovider = Request.Params["idprovider"];
            string idProviderUrl = Request.Params["idProviderUrl"];
            if (response != null)
            {
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        // This is where you would look for any OpenID extension responses included
                        // in the authentication assertion.                                
                        var claimsResponse = response.GetExtension<ClaimsResponse>();
                        profileFields = claimsResponse;
                        // Store off the "friendly" username to display -- NOT for username lookup                                
                        friendlyName = response.FriendlyIdentifierForDisplay;
                        // Use FormsAuthentication to tell ASP.NET that the user is now logged in,                                
                        // with the OpenID Claimed Identifier as their username. 
                        string userName = response.ClaimedIdentifier.ToString();
                        //FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userName,
                        //                                                                 DateTime.Now,
                        //                                                                 DateTime.Now.AddDays(10),
                        //                                                                 true, "user custom data");

                        AuthenticationSupport.SetAuthenticationCookie(Response, userName);
                        //FormsAuthentication.RedirectFromLoginPage(response.ClaimedIdentifier, false);
                        //string redirectUrl = FormsAuthentication.GetRedirectUrl(userName, true);
                        string redirectUrl = Request.Params["ReturnUrl"];
                        if (redirectUrl == null)
                            redirectUrl = FormsAuthentication.DefaultUrl;
                        Response.Redirect(redirectUrl, true);
                        break;
                    case AuthenticationStatus.Canceled:
                        this.loginCanceledLabel.Visible = true;
                        break;
                    case AuthenticationStatus.Failed:
                        this.loginFailedLabel.Visible = true;
                        break;
                }
            } else
            {
                if(String.IsNullOrEmpty(idProviderUrl) == false)
                {
                    CreateOpenIDRequestAndRedirect(idProviderUrl);
                    return;
                }
                if (idprovider != null)
                {
                    switch (idprovider)
                    {
                        case "google":
                            PerformGoogleLogin();
                            return;
                        case "yahoo":
                            PerformYahooLogin();
                            return;
                        case "aol":
                            PerformAOLLogin();
                            return;
                        case "wordpress":
                            openIdBox.Text = "http://ENTER-YOUR-BLOG-NAME-HERE.wordpress.com";
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        protected void openidValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            // This catches common typos that result in an invalid OpenID Identifier.        
            args.IsValid = Identifier.IsValid(args.Value);
        }

        protected void loginButton_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
                // don't login if custom validation failed.        
            }
            string openIdUrl = this.openIdBox.Text;
            CreateOpenIDRequestAndRedirect(openIdUrl);
        }

        private void CreateOpenIDRequestAndRedirect(string openIdUrl)
        {
            try
            {
                using (OpenIdRelyingParty openid = new OpenIdRelyingParty())
                {
                    IAuthenticationRequest request = openid.CreateRequest(openIdUrl);
                    // This is where you would add any OpenID extensions you wanted                        
                    // to include in the authentication request.                        
                    request.AddExtension(new ClaimsRequest
                                             {
                                                 //Country = DemandLevel.Request,
                                                 //Email = DemandLevel.Request,
                                                 //Gender = DemandLevel.Require,
                                                 //PostalCode = DemandLevel.Require,
                                                 //TimeZone = DemandLevel.Require,
                                             }); // Send your visitor to their Provider for authentication.
                    request.RedirectToProvider();
                }
            }
            catch (ProtocolException ex)
            {
                // The user probably entered an Identifier that                
                // was not a valid OpenID endpoint.                
                this.openidValidator.Text = ex.Message;
                this.openidValidator.IsValid = false;
            }
        }

        protected void bGoogleLogin_Click(object sender, EventArgs e)
        {
            PerformGoogleLogin();
        }



        private void PerformAOLLogin()
        {
            CreateOpenIDRequestAndRedirect("https://www.aol.com");
        }


        private void PerformGoogleLogin()
        {
            CreateOpenIDRequestAndRedirect("https://www.google.com/accounts/o8/id");
        }

        private void PerformYahooLogin()
        {
            CreateOpenIDRequestAndRedirect("https://me.yahoo.com");
        }


    }
}