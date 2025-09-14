using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyRazorApp.Pages
{
    public class AdminModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirect đến trang login
            return RedirectToPage("/Auth/Login");
        }
    }
}
