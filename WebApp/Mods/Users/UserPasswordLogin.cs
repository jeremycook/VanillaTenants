using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using WebApp.Mods.Entities;

namespace WebApp.Mods.Users;

public class UserPasswordLogin
{
    [Key]
    public string Username { get; set; } = null!;
    public string NormalizedUsername { get => Username?.ToUpper()!; private set { } }
    public string PasswordDigest { get; set; } = null!;

    public Guid EntityId { get; set; }
    public Entity? Entity { get; private set; }

    public void SetPasswordDigest(string password)
    {
        PasswordHasher<StubUser> passwordHasher = new();
        PasswordDigest = passwordHasher.HashPassword(StubUser.Singleton, password);
    }

    public PasswordVerificationResult VerifyPassword(string password)
    {
        PasswordHasher<StubUser> passwordHasher = new();
        return passwordHasher.VerifyHashedPassword(StubUser.Singleton, PasswordDigest, password);
    }

    private sealed class StubUser
    {
        public static StubUser Singleton { get; } = new();
        private StubUser() { }
    }
}
