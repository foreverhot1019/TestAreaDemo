using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TestAreaDemo.Startup))]
namespace TestAreaDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            
        }
    }
}
