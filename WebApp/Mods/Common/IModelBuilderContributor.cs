using Microsoft.EntityFrameworkCore;

namespace WebApp.Mods.Common
{
    public interface IModelBuilderContributor<TDbContext>
        where TDbContext : DbContext
    {
        void BuildModel(ModelBuilder modelBuilder);
    }
}
