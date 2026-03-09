using Web.Blazor.Areas.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Web.Blazor.Areas.Identity.IdentityHostingStartup))]
namespace Web.Blazor.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            //builder.ConfigureServices((context, services) => {
            //    services.AddDbContext<PublicationsContext>(options =>
            //        options.UseSqlServer(
            //            context.Configuration.GetConnectionString("SqlServerCon")));

            //    services.AddDefaultIdentity<IdentityUser>(options => 
            //        options.SignIn.RequireConfirmedAccount = true)
            //        .AddEntityFrameworkStores<PublicationsContext>();
            //});
        }
    }
}