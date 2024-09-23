using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CarPacking.Startup))]
namespace CarPacking
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
