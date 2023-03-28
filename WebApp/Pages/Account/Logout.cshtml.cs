using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Account;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    public async Task OnGetAsync() =>
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    public async Task OnPostAsync() =>
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}
