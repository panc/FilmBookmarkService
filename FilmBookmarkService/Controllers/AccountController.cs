using System.Web.Mvc;
using System.Web.Security;
using FilmBookmarkService.Models;

namespace FilmBookmarkService.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            // var password = FormsAuthentication.HashPasswordForStoringInConfigFile("christoph.pangerl", "SHA1");

            if (ModelState.IsValid && FormsAuthentication.Authenticate(model.UserName, model.Password))
            {
                FormsAuthentication.SetAuthCookie(model.UserName, true);
                return new RedirectResult("~/");
            }

            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return new RedirectResult("~/");
        }
    }
}