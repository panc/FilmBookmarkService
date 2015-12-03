using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using FilmBookmarkService.Core;
using FilmBookmarkService.Models;

namespace FilmBookmarkService.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly Lazy<SettingsStore> _lazySettingsStore;

        public SettingsController()
        {
            _lazySettingsStore = new Lazy<SettingsStore>(() =>
            {
                var appDataPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString(); ;
                return SettingsStore.Create(appDataPath);
            });
        }

        private SettingsStore SettingsStore => _lazySettingsStore.Value;

        public ActionResult Index()
        {
            var settings = new SettingsViewModel
            {
                UseProxy = SettingsStore.Settings.UseProxy,
                ProxyAddress = SettingsStore.Settings.ProxyAddress
            };

            ViewBag.Success = false;

            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(SettingsViewModel settings)
        {
            if (ModelState.IsValid)
            {
                SettingsStore.Settings.UseProxy = settings.UseProxy;
                SettingsStore.Settings.ProxyAddress = settings.ProxyAddress;

                await SettingsStore.SaveChangesAsync();
            }
            else
            {
                ModelState.AddModelError("", "Failed to save the settings!");
            }

            ViewBag.Success = ModelState.IsValid;

            return View(settings);
        }
    }
}