using System.Reflection;

namespace WebApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        string applicationName =
            Environment.GetEnvironmentVariable("ASPNETCORE_APPLICATIONNAME") ??
            Assembly.GetEntryAssembly()!.GetName().Name!;

        string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        bool isDevelopment = envName == "Development";

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        TenantOptions[] tenantsOptions = configuration
            .GetRequiredSection("Tenants")
            .Get<TenantOptions[]>();

        //TenantOptions[] tenantsOptions = Enumerable.Range(1, 1000)
        //    .Select(i => new TenantOptions
        //    {
        //        Id = "Tenant" + i,
        //        Startup = nameof(BasicTenant),
        //        Urls = new[] { "https://localhost:" + (7000 + i) },
        //    })
        //    .ToArray();

        Type[] startupClasses = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.ExportedTypes)
            .Where(t =>
                t.GetCustomAttribute<TenantAttribute>() is not null ||
                t.Name == "Startup" ||
                t.IsAssignableTo(typeof(IStartup))
            )
            .ToArray();

        Dictionary<string, Type> startupClassDictionary = startupClasses
            .ToDictionary(t => t.FullName + ", " + t.Assembly.GetName().Name);
        foreach (var group in startupClasses
                     .GroupBy(t => t.Name + ", " + t.Assembly.GetName().Name)
                     .Where(g => g.Count() == 1))
        {
            startupClassDictionary.Add(group.Key, group.First());
        }

        foreach (var group in startupClasses
                     .GroupBy(t => t.FullName!)
                     .Where(g => g.Count() == 1))
        {
            startupClassDictionary.Add(group.Key, group.First());
        }

        foreach (var group in startupClasses
                     .GroupBy(t => t.Name)
                     .Where(g => g.Count() == 1))
        {
            startupClassDictionary.Add(group.Key, group.First());
        }

        var tenantRunners = tenantsOptions
            .Select(tenant => Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    string tenantContentRoot = Path.GetFullPath($"../Tenants/{tenant.Id}");

                    if (isDevelopment)
                    {
                        // Automatically create tenant directories for development
                        Directory.CreateDirectory(tenantContentRoot);
                    }

                    IConfigurationRoot tenantConfiguration = new ConfigurationBuilder()
                        .SetBasePath(tenantContentRoot)
                        .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                        .Build();

                    webHostBuilder
                        .UseSetting(WebHostDefaults.ApplicationKey, $"{tenant.Id}, {applicationName}")
                        .UseContentRoot(tenantContentRoot)
                        .UseConfiguration(tenantConfiguration)
                        .UseUrls(tenant.Urls)
                        .ConfigureServices(services => services.AddSingleton(tenant))
                        .UseStartup(startupClassDictionary[tenant.Startup]);
                })
                .Build()
                .RunAsync()
            )
            .ToArray();

        await Task.WhenAll(tenantRunners);
    }
}