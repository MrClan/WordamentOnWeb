using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Wordament_SignalR_MVC.Startup))]
namespace Wordament_SignalR_MVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
