using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using FilmBookmarkService.Core;
using FilmBookmarkService.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;

namespace FilmBookmarkService.Controllers
{
    public class AccountController : Controller
    {
        private IAuthenticationManager Authentication
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid && _Authenticate(model.UserName, model.Password))
            {
                var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
                identity.AddClaim(new Claim(ClaimTypes.Name, model.UserName));

                Authentication.SignIn(new AuthenticationProperties(), identity);

                return new RedirectResult("~/");
            }

            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        private bool _Authenticate(string userName, string password)
        {
            var hashedPassword = _Hash(password);
            var userStore = HttpContext.GetOwinContext().Get<UserStore>();
            
            return userStore.Users.Any(x => x.UserName == userName && x.Password == hashedPassword);
        }

        private string _Hash(string password)
        {
            using (var sha = new SHA512Managed())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return new RedirectResult("~/");
        }
    }
}