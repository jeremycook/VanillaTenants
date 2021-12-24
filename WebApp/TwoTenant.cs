namespace WebApp;

[Tenant]
public class TwoTenant
{
    private readonly IWebHostEnvironment env;

    public TwoTenant(IWebHostEnvironment env)
    {
        this.env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(new TenantId("Two"));

        services.AddRazorPages();
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app)
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