using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(FilmBookmarkService.Startup))]

namespace FilmBookmarkService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
