using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Mods.Users;

namespace WebApp.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> logger;
    private readonly VerifyUsernameAndPassword verify;

    public LoginModel(ILogger<LoginModel> logger, VerifyUsernameAndPassword verify)
    {
        this.logger = logger;
        this.verify = verify;
    }

    [BindProperty]
    public LoginInput Form { get; set; } = new();

    public class LoginInput
    {
        [RegularExpression("^[A-Za-z][A-Za-z0-9]*$")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = default!;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = default!;
    }

    public void OnGet()
    {
    }

    public async Task<ActionResult> OnPost(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var isValid = await verify.Verify(Form.Username, Form.Password);

        if (!isValid)
        {
            ModelState.AddModelError("", $"Password authentication failed for user \"{Form.Username}\".");
            return Page();
        }

        // TODO: Set other information like email and roles
        var claims = new List<Claim>
            {
                new Claim("sub", Form.Username), // TODO
                new Claim("username", Form.Username),
                //new Claim("email", user.Email),
            };
        //claims.AddRange(result.Roles.Select(role => new Claim("role", role)));

        var claimsIdentity = new ClaimsIdentity(
            claims: claims,
            authenticationType: CookieAuthenticationDefaults.AuthenticationScheme,
            nameType: "username",
            roleType: "role");

        var authProperties = new AuthenticationProperties
        {
            //AllowRefresh = <bool>,
            // Refreshing the authentication session should be allowed.

            //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
            // The time at which the authentication ticket expires. A 
            // value set here overrides the ExpireTimeSpan option of 
            // CookieAuthenticationOptions set with AddCookie.

            //IsPersistent = true,
            // Whether the authentication session is persisted across 
            // multiple requests. When used with cookies, controls
            // whether the cookie's lifetime is absolute (matching the
            // lifetime of the authentication ticket) or session-based.

            //IssuedUtc = <DateTimeOffset>,
            // The time at which the authentication ticket was issued.

            //RedirectUri = <string>
            // The full path or absolute URI to be used as an http 
            // redirect response value.
        };

        await HttpContext.SignInAsync(
            scheme: CookieAuthenticationDefaults.AuthenticationScheme,
            principal: new ClaimsPrincipal(claimsIdentity),
            properties: authProperties);

        logger.LogInformation("Logged in {Username}.", claimsIdentity.Name);

        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return Redirect("~/");
        }
    }
}
