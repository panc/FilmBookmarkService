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
        private readonly Lazy<FilmStore> _lazyFilmStore;
        private readonly Lazy<WebsiteParserFactory> _lazyParserFactory;

        public FilmController()
        {
            _lazyFilmStore = new Lazy<FilmStore>(() =>
            {
                var appDataPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString(); ;
                var user = HttpContext.User.Identity.GetUserName();
                return FilmStore.Create(appDataPath, user);
            });

            _lazyParserFactory = new Lazy<WebsiteParserFactory>(() =>
            {
                var appDataPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString(); ;
                var settingsStore = SettingsStore.Create(appDataPath);
                
                return new WebsiteParserFactory(settingsStore.Settings.UseProxy, settingsStore.Settings.ProxyAddress);
            });
        }

        private FilmStore FilmStore => _lazyFilmStore.Value;
        private WebsiteParserFactory WebsiteParserFactory => _lazyParserFactory.Value;

        public ActionResult Index(bool allFilms = false)
        {
            ViewBag.FavoritesClass = allFilms ? "" : "selected";
            ViewBag.AllFilmsClass = allFilms ? "selected" : "";
            ViewBag.CanChangeSortIndex = allFilms == false;

            var list = allFilms
                ? FilmStore.Films.OrderBy(x => x.Name).ToList()
                : FilmStore.Films.Where(x => x.IsFavorite).OrderBy(x => x.SortIndex).ToList();

            var films = list.Select(f => new FilmViewModel
            {
                Id = f.Id,
                Name = f.Name,
                Season = f.Season,
                Episode = f.Episode,
                IsFavorite = f.IsFavorite,
                Url = WebProxyHelper.DecorateUrl(f.Url),
                UndecoratedUrl = f.Url,
                CoverUrl = f.CoverUrl
            });

            return View(films.ToList());
        }

        [HttpPost]
        public async Task<ActionResult> GetMirrors(int id)
        {
            try
            {
                var film = FilmStore.Films.SingleOrDefault(x => x.Id == id);

                if (film == null)
                    return _Failure("Film with id {0} not found!", id);

                var parser = await WebsiteParserFactory.CreateParserForUrl(film.Url);
                var mirrors = await parser.GetMirrors(film.Url, film.Season, film.Episode);
                var numberOfEpisodes = await parser.GetNumberOfEpisodes(film.Url, film.Season);

                return Json(new
                {
                    success = true,
                    mirrors = mirrors,
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
        public async Task<ActionResult> GetStream(int id, string url)
        {
            try
            {
                var film = FilmStore.Films.SingleOrDefault(x => x.Id == id);

                if (film == null)
                    return _Failure("Film with id {0} not found!", id);

                var parser = await WebsiteParserFactory.CreateParserForUrl(film.Url);
                var streamUrl = await parser.GetStreamUrl(url);

                return Json(new
                {
                    success = true,
                    streamUrl = streamUrl,
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

                var parser = await WebsiteParserFactory.CreateParserForUrl(film.Url);
                var result = await parser.GetNextEpisode(film.Url, film.Season, film.Episode);

                if (result == null)
                    return _Failure("No more episodes available!");

                film.Season = result.Season;
                film.Episode = result.Episode;
                await FilmStore.SaveChangesAsync();

                var numberOfEpisodes = await parser.GetNumberOfEpisodes(film.Url, film.Season);

                return Json(new
                {
                    success = true,
                    mirrors = result.Mirrors,
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

                var parser = await WebsiteParserFactory.CreateParserForUrl(film.Url);
                var result = await parser.GetPrevEpisode(film.Url, film.Season, film.Episode);

                if (result == null)
                    return _Failure("No more episodes for the configured streaming service available!");

                film.Season = result.Season;
                film.Episode = result.Episode;
                await FilmStore.SaveChangesAsync();

                var numberOfEpisodes = await parser.GetNumberOfEpisodes(film.Url, film.Season);

                return Json(new
                {
                    success = true,
                    mirrors = result.Mirrors,
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

                var parser = await WebsiteParserFactory.CreateParserForUrl(film.Url);
                var isAnotherEpisodeAvailable = await parser.IsAnotherEpisodeAvailable(film.Url, film.Season, film.Episode);
                
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
        public async Task<ActionResult> AddFilm(FilmViewModel model)
        {
            var url = WebProxyHelper.RemoveProxyDecoration(model.Url);

            var film = new Film
            {
                Name = model.Name,
                Url = url,
                CoverUrl = model.CoverUrl,
                Season = model.Season > 0 ? model.Season : 1,
                Episode = model.Episode > 0 ? model.Episode : 1,
                IsFavorite = model.IsFavorite
            };

            var parser = await WebsiteParserFactory.CreateParserForUrl(url);
            if (parser == null)
                return _Failure("No parser found for {0}!", url);

            film.IsFavorite = true;

            FilmStore.AddFilm(film);
            await FilmStore.SaveChangesAsync();

            return _Success();
        }

        [HttpPost]
        public async Task<ActionResult> EditFilm(int id, FilmViewModel updatedFilm)
        {
            var film = FilmStore.Films.SingleOrDefault(x => x.Id == id);
            var url = WebProxyHelper.RemoveProxyDecoration(updatedFilm.Url);

            if (film == null)
                return _Failure("Film with id {0} not found!", id);

            var parser = await WebsiteParserFactory.CreateParserForUrl(url);
            if (parser == null)
                return _Failure("No parser found for {0}!", url);

            film.Name = updatedFilm.Name;
            film.Url = url;
            film.Season = updatedFilm.Season;
            film.Episode = updatedFilm.Episode;
            film.CoverUrl = updatedFilm.CoverUrl;

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
        public async Task<ActionResult> SetIsFavorite(int id)
        {
            var film = FilmStore.Films.SingleOrDefault(x => x.Id == id);

            if (film == null)
                return _Failure("Film with id {0} not found!", id);

            film.IsFavorite = !film.IsFavorite;
            await FilmStore.SaveChangesAsync();

            return Json(new
            {
                isFavorite = film.IsFavorite 
            });
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