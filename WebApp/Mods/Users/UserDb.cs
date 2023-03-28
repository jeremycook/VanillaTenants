using Microsoft.EntityFrameworkCore;
using WebApp.Mods.Common;

namespace WebApp.Mods.Users
{
    public class UserDb
    {
        public AppDbContext Db { get; }

        private UserDb(AppDbContext db)
        {
            Db = db;
        }
        public UserDb(IDbContextFactory<AppDbContext> factory)
            : this(factory.CreateDbContext())
        {
        }

        public static UserDb Create(AppDbContext db)
        {
            return new(db);
        }

        public DbSet<UserPasswordLogin> UserPasswordLogins => Db.Set<UserPasswordLogin>();
    }
}
