using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FilmBookmarkService.Core;
using Microsoft.AspNet.Identity.Owin;

namespace FilmBookmarkService.Controllers
{
    [Authorize]
    public class FilmController : Controller
    {
        private readonly Lazy<DataStore> _dataStore;

        public FilmController()
        {
            _dataStore = new Lazy<DataStore>(
               () => HttpContext.GetOwinContext().Get<DataStore>());
        }

        private DataStore DataStore
        {
            get { return _dataStore.Value; }
        }

        public ActionResult Index()
        {
            var list = DataStore.Films.ToList();
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> GetStream(int id)
        {
            var film = DataStore.Films.SingleOrDefault(x => x.Id == id);

            if (film == null)
                return _Failure("Film with id {0} not found!", id);

            return await _GetStream(film);
        }

        [HttpPost]
        public async Task<ActionResult> NextEpisode(int id)
        {
            try
            {
                var film = DataStore.Films.SingleOrDefault(x => x.Id == id);

                if (film == null)
                    return _Failure("Film with id {0} not found!", id);
                
                var result = await film.Parser.GetNextEpisode(film.Url, film.Season, film.Episode);

                if (result == null)
                    return _Failure("No more episodes available!");

                film.Season = result.Season;
                film.Episode = result.Episode;
                await DataStore.SaveChangesAsync();

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
                var film = DataStore.Films.SingleOrDefault(x => x.Id == id);
                
                if (film == null)
                    return _Failure("Film with id {0} not found!", id);
                
                var result = await film.Parser.GetPrevEpisode(film.Url, film.Season, film.Episode);

                if (result == null)
                    return _Failure("No more episodes for the configured streaming service available!"); 
                
                film.Season = result.Season;
                film.Episode = result.Episode;
                await DataStore.SaveChangesAsync();

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
        public async Task<ActionResult> IsAnotherEpisodeAvailable(int id)
        {
            try
            {
                var film = DataStore.Films.SingleOrDefault(x => x.Id == id);

                if (film == null)
                    return _Failure("Film with id {0} not found!", id);

                var isAnotherEpisodeAvailable = await film.Parser.IsAnotherEpisodeAvailable(film.Url, film.Season, film.Episode);

                return isAnotherEpisodeAvailable
                    ? _Success()
                    : _Failure("");
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

            film.Name = updatedFilm.Name;
            film.Url = updatedFilm.Url;
            film.Season = updatedFilm.Season;
            film.Episode = updatedFilm.Episode;
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

        private async Task<ActionResult> _GetStream(Film film)
        {
            try
            {
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