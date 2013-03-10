using System;
using System.Security.Principal;
using System.Web;
using AzureSupport;

namespace TheBall
{
    public static class AuthenticationSupport
    {
        private const string AuthCookieName = "TheBall_AUTH";
        private const int TimeoutSeconds = 3600;
        public static string DefaultUser = null;

        static AuthenticationSupport()
        {
            //DefaultUser = "localdeveloperauth";
        }

        public static void SetAuthenticationCookie(HttpResponse response, string validUserName)
        {
            WebSupport.InitializeContextStorage(HttpContext.Current.Request);
            string authString = EncryptionSupport.EncryptStringToBase64(validUserName);
            if(response.Cookies[AuthCookieName] != null)
                response.Cookies.Remove(AuthCookieName);
            HttpCookie cookie = new HttpCookie(AuthCookieName, authString);
            cookie.HttpOnly = false;
            cookie.Expires = DateTime.Now.AddSeconds(TimeoutSeconds);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void SetUserFromCookieIfExists(HttpContext context)
        {
            var request = HttpContext.Current.Request;
            var encCookie = request.Cookies[AuthCookieName];
            if (encCookie != null)
            {
                try
                {
                    WebSupport.InitializeContextStorage(context.Request);
                    string userName = EncryptionSupport.DecryptStringFromBase64(encCookie.Value);
                    context.User = new GenericPrincipal(new GenericIdentity(userName, "theball"), new string[0]);
                    // Reset cookie time to be again timeout from this request
                    encCookie.Expires = DateTime.Now.AddSeconds(TimeoutSeconds);
                    context.Response.Cookies.Set(encCookie);
                } catch
                {
                    ClearAuthenticationCookie(context.Response);
                }
            } else if (String.IsNullOrEmpty(DefaultUser) == false)
            {
                SetAuthenticationCookie(context.Response, DefaultUser);
                context.User = new GenericPrincipal(new GenericIdentity(DefaultUser, "theball"), new string[0]);
            }
            
        }

        public static void ClearAuthenticationCookie(HttpResponse response)
        {
            HttpCookie cookie = new HttpCookie(AuthCookieName);
            cookie.Expires = DateTime.Today.AddDays(-1);
            if(response.Cookies[AuthCookieName] != null)
                response.Cookies.Set(cookie);
            else
                response.Cookies.Add(cookie);
        }
    }
}