using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WamlAndWaws.Startup))]
namespace WamlAndWaws
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
