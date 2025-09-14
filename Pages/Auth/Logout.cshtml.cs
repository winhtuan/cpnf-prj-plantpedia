using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Plantpedia.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LogoutModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult OnGet()
        {
            // Xóa JWT token cookie
            var cookieName = _configuration["Jwt:CookieName"] ?? "PlantpediaJWT";
            Response.Cookies.Delete(cookieName);

            // Xóa session data
            HttpContext.Session.Clear();

            return Page();
        }

        public IActionResult OnPost()
        {
            // Xóa JWT token cookie
            var cookieName = _configuration["Jwt:CookieName"] ?? "PlantpediaJWT";
            Response.Cookies.Delete(cookieName);

            // Xóa session data
            HttpContext.Session.Clear();

            return RedirectToPage("/Auth/Login");
        }
    }
}

