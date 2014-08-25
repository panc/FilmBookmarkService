using System;
using System.Security.Claims;
using System.Web.Helpers;
using FilmBookmarkService.Core;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace FilmBookmarkService
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            var appDataPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString(); ;

            // Configure the datastore to use a single instance per request
            app.CreatePerOwinContext(() => DataStore.Create(appDataPath));

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider(),
            });

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
        }
    }
}