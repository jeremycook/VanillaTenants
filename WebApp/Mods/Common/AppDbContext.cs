using Microsoft.EntityFrameworkCore;

namespace WebApp.Mods.Common
{
    public class AppDbContext : DbContext
    {
        public static List<IModelBuilderContributor<AppDbContext>> Contributors { get; } =
            typeof(AppDbContext).Assembly.ExportedTypes
            .Where(t => t.IsAssignableTo(typeof(IModelBuilderContributor<AppDbContext>)))
            .Select(t => (IModelBuilderContributor<AppDbContext>)Activator.CreateInstance(t)!)
            .ToList();

        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options
                    .UseNpgsql("Name=ConnectionStrings:App")
                    .UseSnakeCaseNamingConvention();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var contributor in Contributors)
            {
                contributor.BuildModel(modelBuilder);
            }
        }
    }
}
