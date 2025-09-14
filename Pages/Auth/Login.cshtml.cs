using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plantpedia.Repository;         // Add this
using Plantpedia.Models;             // Add this
using System.Threading.Tasks;

namespace MyRazorApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UserRepository _userRepository;

        public LoginModel(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [BindProperty]
        public string? Username { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        public bool LoginFailed { get; set; } = false;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                LoginFailed = true;
                return Page();
            }

            var user = await _userRepository.LoginAsync(Username, Password);

            if (user != null)
            {
                // Lưu user vào Session (hoặc cookie)
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", Username);

                // Có thể redirect đến trang admin/dashboard
                return RedirectToPage("/Admin/Dashboard"); // Đổi lại nếu route khác
            }
            else
            {
                LoginFailed = true;
                return Page();
            }
        }
    }
}
