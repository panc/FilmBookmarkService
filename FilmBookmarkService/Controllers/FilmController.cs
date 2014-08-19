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
            var list = await DbContext.Films.ToListAsync();
            return View(list);
        }

        [HttpGet]
        public async Task<ActionResult> GetStream(int id)
        {
            var film = await DbContext.Films.SingleOrDefaultAsync(x => x.Id == id);
            var streamUrl = await film.Parser.GetNextStreamUrl(film.Url, film.Season, film.Episode);
            
            return Json(new
            {
                success = true, 
                streamUrl = streamUrl,
                season = film.Season,
                episode = film.Episode
            }, 
            JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> NextEpisode(int id)
        {
            var film = await DbContext.Films.SingleOrDefaultAsync(x => x.Id == id);
            film.Episode++; // todo
            DbContext.SaveChanges();

            return Json(new
            {
                success = true,
                season = film.Season,
                episode = film.Episode
            });
        }

        [HttpPost]
        public async Task<ActionResult> PrevEpisode(int id)
        {
            var film = await DbContext.Films.SingleOrDefaultAsync(x => x.Id == id);
            film.Episode--; // todo
            DbContext.SaveChanges();

            return Json(new
            {
                success = true,
                season = film.Season,
                episode = film.Episode
            });
        }

        [HttpPost]
        public async Task<ActionResult> AddFilm(Film film)
        {      
            var parser = await WebsiteParsingHelper.GetParser(film.Url);

            if (parser == null)
                return _Failure("No parser found for {0}!", film.Url);

            film.SetParser(parser);

            DbContext.Films.Add(film);
            await DbContext.SaveChangesAsync();

            return _Success();
        }

        [HttpPost]
        public async Task<ActionResult> EditFilm([Bind]Film film)
        {
            var parser = await WebsiteParsingHelper.GetParser(film.Url);
            if (parser == null)
                return _Failure("No parser found for {0}!", film.Url);

            film.SetParser(parser);
            
            DbContext.Entry(film).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();

            return _Success();
        }
        
        [HttpPost]
        public async Task<ActionResult> RemoveFilm(int id)
        {
            var film = await DbContext.Films.SingleOrDefaultAsync(x => x.Id == id);
            if (film == null)
                return _Failure("Film with id {0} not found!", id);

            DbContext.Films.Remove(film);
            await DbContext.SaveChangesAsync();

            return _Success();
        }

        private ActionResult _Success()
        {
            return Json(new { success = true });
        }

        private ActionResult _Failure(string message, params object[] parameter)
        {
            return Json(new { success = false, message = string.Format(message, parameter) });
        }
    }
}