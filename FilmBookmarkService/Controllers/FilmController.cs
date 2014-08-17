using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FilmBookmarkService.Core;
using Microsoft.AspNet.Identity.Owin;

namespace FilmBookmarkService.Controllers
{
    public class FilmController : Controller
    {
        private readonly Lazy<ApplicationDbContext> _dbContext;

        public FilmController()
        {
            _dbContext = new Lazy<ApplicationDbContext>(
                () => HttpContext.GetOwinContext().Get<ApplicationDbContext>());
        }

        private ApplicationDbContext DbContext
        {
            get { return _dbContext.Value; }
        }

        public async Task<ActionResult> Index()
        {
            //AddFilm("Castl", "http://kinox.to/Stream/Castle.html");

            var list = await DbContext.Films.ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> AddFilm(string name, string link)
        {           
            var film = new Film
            {
                Name = name,
                Season = 1,
                Episode = 1
            };

            var parser = await WebsiteParsingHelper.GetParser(link);
            film.SetParser(parser);

            DbContext.Films.Add(film);
            await DbContext.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}