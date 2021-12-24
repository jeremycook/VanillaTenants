namespace WebApp;

[Tenant]
public class BasicTenant
{
    private readonly IWebHostEnvironment env;

    public BasicTenant(IWebHostEnvironment env)
    {
        this.env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddControllers();

        services.AddSingleton(new TenantId("One"));
    }

    public void Configure(
      IApplicationBuilder app,
      ILoggerFactory loggerFactory)
    {
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        //app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        //app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(routes =>
        {
            routes.MapRazorPages();
        });
    }
}
