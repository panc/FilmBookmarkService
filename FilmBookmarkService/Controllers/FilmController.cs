using System.Linq;
using System.Web;
using System.Web.Mvc;
using FilmBookmarkService.Core;
using Microsoft.AspNet.Identity.Owin;

namespace FilmBookmarkService.Controllers
{
    public class FilmController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public FilmController()
        {
            _dbContext = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
        }

        public ActionResult Index()
        {
            return View(_dbContext.Films.ToList());
        }
    }
}