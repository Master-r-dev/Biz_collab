using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Biz_collab.Startup))]
namespace Biz_collab
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
