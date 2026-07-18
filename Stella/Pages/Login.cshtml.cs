using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stella.Abstractions.Configuration;

namespace Stella.WebUI.Pages;

public class LoginModel : PageModel
{
    [BindProperty] public string? Password { get; set; }
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password == StellaOptions.WebUIPassword)
        {
            var principal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[] { new System.Security.Claims.Claim("name", "admin") },
                    WebUIServiceExtensions.AuthScheme));
            await HttpContext.SignInAsync(WebUIServiceExtensions.AuthScheme, principal);
            return RedirectToPage("/Index");
        }
        Error = "Incorrect password.";
        return Page();
    }
}
