using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebsocketChat.Client.Filters;
using WebsocketChat.Client.HttpClients;
using WebsocketChat.Client.JwtAuth;

namespace WebsocketChat.Client
{
    public class Program
    {
        private static IConfiguration configuration { get; set; }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

            builder.Services.AddHttpContextAccessor();
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>(
                    JwtBearerDefaults.AuthenticationScheme, null);
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient<IApiHttpClient, ApiHttpClient>(c =>
            {
                c.BaseAddress = new Uri(configuration["ApiAddress"]);
            });

            var app = builder.Build();

            app.UseMiddleware<RedirectOnUnauthorizedMiddleware>();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
