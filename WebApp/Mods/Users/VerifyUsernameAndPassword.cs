using Microsoft.EntityFrameworkCore;

namespace WebApp.Mods.Users
{
    public class VerifyUsernameAndPassword
    {
        private readonly UserDb db;

        public VerifyUsernameAndPassword(UserDb db)
        {
            this.db = db;
        }

        public async virtual Task<bool> Verify(string username, string password)
        {
            var user = await db.UserPasswordLogins.SingleOrDefaultAsync(o => o.NormalizedUsername == username.ToUpper());

            if (user == null)
            {
                return false;
            }

            switch (user.VerifyPassword(password))
            {
                case Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed:
                    return false;
                case Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success:
                    return true;
                case Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded:
                    user.SetPasswordDigest(password);
                    await db.Db.SaveChangesAsync();
                    return true;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
