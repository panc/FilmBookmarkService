using System.Collections.Generic;
using System.Web.Mvc;
using FilmBookmarkService.Models;
using WebGrease;

namespace FilmBookmarkService.Controllers
{
    public class FilmController : Controller
    {
        public ActionResult Index()
        {
            var films = new List<FilmModel>
            {
                new FilmModel { Name = "Hugo" },
                new FilmModel { Name = "Sep" },
                new FilmModel { Name = "Ich" }
            };

            return View(films);
        }
    }
}