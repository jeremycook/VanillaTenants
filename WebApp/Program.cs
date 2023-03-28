using System.Reflection;
using WebApp.Mods.Tenants;

namespace WebApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        string applicationName =
            Environment.GetEnvironmentVariable("ASPNETCORE_APPLICATIONNAME") ??
            Assembly.GetEntryAssembly()!.GetName().Name!;

        string envName =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
            "Production";
        bool isDevelopment = envName == "Development";

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        TenantOptions[] tenantsOptions = configuration
            .GetRequiredSection("Tenants")
            .Get<TenantOptions[]>();

        // Tenant post configuration
        foreach (var tenant in tenantsOptions)
        {
            if (!tenant.Urls.Any())
                throw new InvalidOperationException($"The {tenant.Id} tenant does not bind to any URLs. It must bind to at least one URL.");

            if (string.IsNullOrWhiteSpace(tenant.Id))
                tenant.Id = tenant.Urls[0];

            if (string.IsNullOrWhiteSpace(tenant.Startup))
                tenant.Startup = typeof(Startup).FullName!; // Name of the default startup class
        }

        var duplicateIds = tenantsOptions.GroupBy(o => o.Id).Where(g => g.Count() > 1).Select(g => g.Key);
        if (duplicateIds.Any())
            throw new InvalidOperationException("Every tenant ID must be unique. These are not unique: " + duplicateIds);

        var duplicateUrls = tenantsOptions.SelectMany(o => o.Urls).GroupBy(o => o).Where(g => g.Count() > 1).Select(g => g.Key);
        if (duplicateUrls.Any())
            throw new InvalidOperationException("A tenant URL can only be bound once. These URLs are reused: " + duplicateUrls);

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
                (t.Name == "Startup" && t.Namespace == t.Assembly.GetName().Name)
            )
            .ToArray();

        Dictionary<string, Type> startupTypeLookup = startupClasses
            .ToDictionary(t => t.FullName + ", " + t.Assembly.GetName().Name);
        foreach (var group in startupClasses
            .GroupBy(t => t.Name + ", " + t.Assembly.GetName().Name)
            .Where(g => g.Count() == 1))
        {
            startupTypeLookup.Add(group.Key, group.First());
        }
        foreach (var group in startupClasses
            .GroupBy(t => t.FullName!)
            .Where(g => g.Count() == 1))
        {
            startupTypeLookup.Add(group.Key, group.First());
        }
        foreach (var group in startupClasses
            .GroupBy(t => t.Name)
            .Where(g => g.Count() == 1))
        {
            startupTypeLookup.Add(group.Key, group.First());
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
                        .UseStartup(startupTypeLookup[tenant.Startup]);
                })
                .Build()
                .RunAsync()
            )
            .ToArray();

        await Task.WhenAll(tenantRunners);
    }
}