using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plantpedia.Repository;
using Plantpedia.Models;
using Plantpedia.Service;

namespace MyRazorApp.Pages
{
    /// <summary>
    /// Trang đăng nhập với JWT authentication
    /// </summary>
    public class LoginModel : PageModel
    {
        #region Private Fields
        private readonly UserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        /// <summary>
        /// Khởi tạo LoginModel
        /// </summary>
        /// <param name="userRepository">UserRepository để xác thực user</param>
        /// <param name="jwtService">JwtService để tạo JWT token</param>
        /// <param name="configuration">IConfiguration để đọc cấu hình</param>
        public LoginModel(UserRepository userRepository, JwtService jwtService, IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Username từ form input
        /// </summary>
        [BindProperty]
        public string? Username { get; set; }

        /// <summary>
        /// Password từ form input
        /// </summary>
        [BindProperty]
        public string? Password { get; set; }

        /// <summary>
        /// Trạng thái đăng nhập thất bại
        /// </summary>
        public bool LoginFailed { get; set; } = false;

        /// <summary>
        /// Thông báo lỗi đăng nhập
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// URL để redirect sau khi đăng nhập thành công
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Xử lý GET request - hiển thị trang đăng nhập
        /// </summary>
        /// <returns>PageResult</returns>
        public IActionResult OnGet()
        {
            // Nếu đã đăng nhập, redirect đến trang chính
            if (JwtHelper.IsUserAuthenticated(HttpContext))
            {
                return RedirectToPage("/Admin/Dashboard");
            }

            return Page();
        }

        /// <summary>
        /// Xử lý POST request - thực hiện đăng nhập
        /// </summary>
        /// <returns>IActionResult</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // Validate input
            if (!ValidateInput())
            {
                return Page();
            }

            try
            {
                // Xác thực user
                var user = await AuthenticateUserAsync(Username!, Password!);

                if (user != null)
                {
                    // Đăng nhập thành công
                    await HandleSuccessfulLoginAsync(user);
                    return RedirectAfterLogin();
                }
                else
                {
                    // Đăng nhập thất bại
                    HandleFailedLogin("Tên đăng nhập hoặc mật khẩu không đúng");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                HandleFailedLogin($"Lỗi đăng nhập: {ex.Message}");
                return Page();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Validate input từ form
        /// </summary>
        /// <returns>true nếu input hợp lệ</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                HandleFailedLogin("Vui lòng nhập tên đăng nhập");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                HandleFailedLogin("Vui lòng nhập mật khẩu");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Xác thực user với database
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>UserAccount nếu thành công, null nếu thất bại</returns>
        private async Task<UserAccount?> AuthenticateUserAsync(string username, string password)
        {
            return await _userRepository.LoginAsync(username, password);
        }

        /// <summary>
        /// Xử lý đăng nhập thành công
        /// </summary>
        /// <param name="user">Thông tin user</param>
        private async Task HandleSuccessfulLoginAsync(UserAccount user)
        {
            // Tạo JWT token
            var token = _jwtService.GenerateToken(user);

            // Lưu token vào cookie
            SetJwtCookie(token);

            // Lưu thông tin vào session (backward compatibility)
            SetSessionData(user);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Xử lý đăng nhập thất bại
        /// </summary>
        /// <param name="message">Thông báo lỗi</param>
        private void HandleFailedLogin(string message)
        {
            LoginFailed = true;
            ErrorMessage = message;
        }

        /// <summary>
        /// Set JWT token vào cookie
        /// </summary>
        /// <param name="token">JWT token</param>
        private void SetJwtCookie(string token)
        {
            var cookieName = _configuration["Jwt:CookieName"] ?? "PlantpediaJWT";
            var expirationMinutes = GetExpirationMinutes();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Bảo mật - không cho JavaScript truy cập
                Secure = Request.IsHttps, // Chỉ gửi qua HTTPS nếu có
                SameSite = Request.IsHttps ? SameSiteMode.Strict : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            Response.Cookies.Append(cookieName, token, cookieOptions);
        }

        /// <summary>
        /// Set thông tin user vào session (backward compatibility)
        /// </summary>
        /// <param name="user">Thông tin user</param>
        private void SetSessionData(UserAccount user)
        {
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", Username!);
        }

        /// <summary>
        /// Lấy thời gian hết hạn token từ cấu hình
        /// </summary>
        /// <returns>Số phút hết hạn</returns>
        private int GetExpirationMinutes()
        {
            if (int.TryParse(_configuration["Jwt:ExpirationMinutes"], out int minutes) && minutes > 0)
            {
                return minutes;
            }
            return 60; // Default 60 phút
        }

        /// <summary>
        /// Redirect sau khi đăng nhập thành công
        /// </summary>
        /// <returns>IActionResult</returns>
        private IActionResult RedirectAfterLogin()
        {
            // Ưu tiên ReturnUrl nếu có
            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            // Default redirect đến Dashboard
            return RedirectToPage("/Admin/Dashboard");
        }
        #endregion
    }
}
