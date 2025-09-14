using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plantpedia.Repository;
using Plantpedia.Service;
using Plantpedia.Models;

namespace Plantpedia.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly UserRepository _userRepository;

        public DashboardModel(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public UserAccount? CurrentUser { get; set; }
        public string? Username { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra authentication
            if (JwtHelper.RequireAuthentication(HttpContext))
            {
                return Page();
            }

            // Lấy thông tin user hiện tại
            CurrentUser = await JwtHelper.GetCurrentUserAsync(HttpContext, _userRepository);
            Username = JwtHelper.GetCurrentUsername(HttpContext);

            if (CurrentUser == null)
            {
                // Nếu không lấy được thông tin user, redirect về login
                return RedirectToPage("/Auth/Login");
            }

            return Page();
        }
    }
}

