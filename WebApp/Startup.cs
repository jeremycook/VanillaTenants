using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebApp.Mods.Common;
using WebApp.Mods.Users;

namespace WebApp;

public class Startup
{
    private readonly IWebHostEnvironment env;

    public Startup(IWebHostEnvironment env)
    {
        this.env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddControllers();
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();

        services
            .AddAuthorization(options =>
            {
                // Fall back to the default authorization policy.
                options.FallbackPolicy = options.DefaultPolicy;
            });

        services
            .AddDbContextFactory<AppDbContext>(options => options
                .UseNpgsql("Name=ConnectionStrings:App")
                .UseSnakeCaseNamingConvention()
            )
            .AddScoped<UserDb>()
            .AddScoped<VerifyUsernameAndPassword>();
    }

    public void Configure(IApplicationBuilder app)
    {
        if (env.IsDevelopment())
        {
            using var scope = app.ApplicationServices.CreateScope();

            // TODO: Decide if we want this for production too
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = factory.CreateDbContext();
            db.Database.Migrate();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }


        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(routes =>
        {
            routes.MapRazorPages();
        });
    }
}