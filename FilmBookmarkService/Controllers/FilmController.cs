using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FilmBookmarkService.Core;
using Microsoft.AspNet.Identity;

namespace FilmBookmarkService.Controllers
{
    [Authorize]
    public class FilmController : Controller
    {
        private readonly Lazy<FilmStore> _dataStore;

        public FilmController()
        {
            _dataStore = new Lazy<FilmStore>(() =>
            {
                var appDataPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString(); ;
                var user = HttpContext.User.Identity.GetUserName();
                return FilmStore.Create(appDataPath, user);
            });
        }

        private FilmStore FilmStore
        {
            get { return _dataStore.Value; }
        }

        public ActionResult Index()
        {
            var list = FilmStore.Films.OrderBy(x => x.SortIndex).ToList();
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> GetStream(int id)
        {
            try
            {
                var film = FilmStore.Films.SingleOrDefault(x => x.Id == id);

                if (film == null)
                    return _Failure("Film with id {0} not found!", id);

                var streamUrl = await film.Parser.GetStreamUrl(film.Url, film.Season, film.Episode);
                var numberOfEpisodes = await film.Parser.GetNumberOfEpisodes(film.Url, film.Season);

                return Json(new
                {
                    success = true,
                    streamUrl = streamUrl,
                    season = film.Season,
                    episode = film.Episode,
                    numberOfEpisodes = numberOfEpisodes
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
                var film = FilmStore.Films.SingleOrDefault(x => x.Id == id);

                if (film == null)
                    return _Failure("Film with id {0} not found!", id);

                var result = await film.Parser.GetNextEpisode(film.Url, film.Season, film.Episode);

                if (result == null)
                    return _Failure("No more episodes available!");

                film.Season = result.Season;
                film.Episode = result.Episode;
                await FilmStore.SaveChangesAsync();

                var numberOfEpisodes = await film.Parser.GetNumberOfEpisodes(film.Url, film.Season);

                return Json(new
                {
                    success = true,
                    streamUrl = result.StreamUrl,
                    season = result.Season,
                    episode = result.Episode,
                    numberOfEpisodes = numberOfEpisodes
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
                var film = FilmStore.Films.SingleOrDefault(x => x.Id == id);

                if (film == null)
                    return _Failure("Film with id {0} not found!", id);

                var result = await film.Parser.GetPrevEpisode(film.Url, film.Season, film.Episode);

                if (result == null)
                    return _Failure("No more episodes for the configured streaming service available!");

                film.Season = result.Season;
                film.Episode = result.Episode;
                await FilmStore.SaveChangesAsync();

                var numberOfEpisodes = await film.Parser.GetNumberOfEpisodes(film.Url, film.Season);

                return Json(new
                {
                    success = true,
                    streamUrl = result.StreamUrl,
                    season = result.Season,
                    episode = result.Episode,
                    numberOfEpisodes = numberOfEpisodes
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
                var film = FilmStore.Films.SingleOrDefault(x => x.Id == id);

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

            film.CoverUrl = await parser.GetCoverUrl(film.Url);
            film.SetParser(parser);

            FilmStore.AddFilm(film);
            await FilmStore.SaveChangesAsync();

            return _Success();
        }

        [HttpPost]
        public async Task<ActionResult> EditFilm(int id, Film updatedFilm)
        {
            var film = FilmStore.Films.SingleOrDefault(x => x.Id == id);

            if (film == null)
                return _Failure("Film with id {0} not found!", id);

            var parser = await WebsiteParsingHelper.GetParser(film.Url);
            if (parser == null)
                return _Failure("No parser found for {0}!", film.Url);

            film.Name = updatedFilm.Name;
            film.Url = updatedFilm.Url;
            film.Season = updatedFilm.Season;
            film.Episode = updatedFilm.Episode;
            film.CoverUrl = await parser.GetCoverUrl(film.Url);

            film.SetParser(parser);

            await FilmStore.SaveChangesAsync();

            return _Success();
        }

        [HttpPost]
        public async Task<ActionResult> UpdateSortOrder(string[] positions)
        {
            var sortedIds = positions.ToList();

            foreach (var film in FilmStore.Films)
            {
                var index = sortedIds.IndexOf(film.Id.ToString());
                film.SortIndex = index + 1;
            }

            await FilmStore.SaveChangesAsync();

            return _Success();
        }

        [HttpPost]
        public async Task<ActionResult> RemoveFilm(int id)
        {
            var film = FilmStore.Films.SingleOrDefault(x => x.Id == id);
            if (film == null)
                return _Failure("Film with id {0} not found!", id);

            FilmStore.RemoveFilm(film);
            await FilmStore.SaveChangesAsync();

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