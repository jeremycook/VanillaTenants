using Microsoft.EntityFrameworkCore;
using WebApp.Mods.Common;

namespace WebApp.Mods.Users
{
    public class ModelBuilderContributor : IModelBuilderContributor<AppDbContext>
    {
        public void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPasswordLogin>();
        }
    }
}
