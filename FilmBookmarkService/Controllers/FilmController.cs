using System;
using System.Data.Entity;
using System.Linq;
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
        private readonly Lazy<DataStore> _dataStore;

        public FilmController()
        {
            _dbContext = new Lazy<ApplicationDbContext>(
                () => HttpContext.GetOwinContext().Get<ApplicationDbContext>());

            _dataStore = new Lazy<DataStore>(
               () => HttpContext.GetOwinContext().Get<DataStore>());
        }

        private ApplicationDbContext DbContext
        {
            get { return _dbContext.Value; }
        }

        private DataStore DataStore
        {
            get { return _dataStore.Value; }
        }

        public async Task<ActionResult> Index()
        {
            var list = await DbContext.Films.ToListAsync();
            var l = DataStore.Films.ToList();
            return View(l);
        }

        [HttpPost]
        public async Task<ActionResult> GetStream(int id)
        {
            try
            {
                var film = await DbContext.Films.SingleOrDefaultAsync(x => x.Id == id);
                var streamUrl = await film.Parser.GetStreamUrl(film.Url, film.Season, film.Episode);

                return Json(new
                {
                    success = true,
                    streamUrl = streamUrl,
                    season = film.Season,
                    episode = film.Episode
                });
            }
            catch (Exception ex)
            {
                return _Failure(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> NextEpisode(int id)
        {
            try
            {
                var film = await DbContext.Films.SingleOrDefaultAsync(x => x.Id == id);
                var result = await film.Parser.GetNextEpisode(film.Url, film.Season, film.Episode);

                if (result == null)
                    return _Failure("No more episodes available!");

                film.Season = result.Season;
                film.Episode = result.Episode;
                DbContext.SaveChanges();

                return Json(new
                {
                    success = true,
                    streamUrl = result.StreamUrl,
                    season = result.Season,
                    episode = result.Episode
                });
            }
            catch (Exception ex)
            {
                return _Failure(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> PrevEpisode(int id)
        {
            try
            {
                var film = await DbContext.Films.SingleOrDefaultAsync(x => x.Id == id);
                var result = await film.Parser.GetPrevEpisode(film.Url, film.Season, film.Episode);

                if (result == null)
                    return _Failure("No more episodes available!"); 
                
                film.Season = result.Season;
                film.Episode = result.Episode;
                DbContext.SaveChanges();

                return Json(new
                {
                    success = true,
                    streamUrl = result.StreamUrl,
                    season = result.Season,
                    episode = result.Episode
                });
            }
            catch (Exception ex)
            {
                return _Failure(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddFilm(Film film)
        {      
            var parser = await WebsiteParsingHelper.GetParser(film.Url);

            if (parser == null)
                return _Failure("No parser found for {0}!", film.Url);

            film.SetParser(parser);

            DataStore.AddFilm(film);
            await DataStore.SaveChangesAsync();

            return _Success();
        }

        [HttpPost]
        public async Task<ActionResult> EditFilm(int id, Film updatedFilm)
        {
            var film = DataStore.Films.SingleOrDefault(x => x.Id == id);

            if (film == null)
                return _Failure("Film with id {0} not found!", id);

            var parser = await WebsiteParsingHelper.GetParser(film.Url);
            if (parser == null)
                return _Failure("No parser found for {0}!", film.Url);

            film.SetParser(parser);
                        
            await DataStore.SaveChangesAsync();

            return _Success();
        }
        
        [HttpPost]
        public async Task<ActionResult> RemoveFilm(int id)
        {
            var film = DataStore.Films.SingleOrDefault(x => x.Id == id);
            if (film == null)
                return _Failure("Film with id {0} not found!", id);

            DataStore.RemoveFilm(film);
            await DataStore.SaveChangesAsync();

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