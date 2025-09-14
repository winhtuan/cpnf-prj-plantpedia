using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plantpedia.Service;
using Plantpedia.Repository;
using Plantpedia.Models;

namespace MyRazorApp.Pages
{
    /// <summary>
    /// Trang Admin Panel - quản lý hệ thống Plantpedia
    /// </summary>
    public class AdminModel : PageModel
    {
        #region Private Fields
        private readonly UserRepository _userRepository;
        #endregion

        #region Constructor
        /// <summary>
        /// Khởi tạo AdminModel
        /// </summary>
        /// <param name="userRepository">UserRepository để lấy thông tin user</param>
        public AdminModel(UserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Thông tin user hiện tại từ database
        /// </summary>
        public UserAccount? CurrentUser { get; set; }

        /// <summary>
        /// Username của user hiện tại
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Trạng thái authentication
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// JWT token hiện tại (để hiển thị)
        /// </summary>
        public string? JwtToken { get; set; }

        /// <summary>
        /// Thông báo lỗi nếu có
        /// </summary>
        public string? ErrorMessage { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Xử lý GET request - hiển thị trang Admin Panel
        /// </summary>
        /// <returns>IActionResult</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Kiểm tra authentication
                if (!await CheckAuthenticationAsync())
                {
                    return RedirectToLogin();
                }

                // Load thông tin user
                await LoadUserInformationAsync();

                return Page();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                ErrorMessage = $"Lỗi tải trang: {ex.Message}";
                return RedirectToLogin();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Kiểm tra authentication và load thông tin cơ bản
        /// </summary>
        /// <returns>true nếu đã authenticate</returns>
        private Task<bool> CheckAuthenticationAsync()
        {
            IsAuthenticated = JwtHelper.IsUserAuthenticated(HttpContext);
            
            if (!IsAuthenticated)
            {
                return Task.FromResult(false);
            }

            // Lấy thông tin cơ bản
            Username = JwtHelper.GetCurrentUsername(HttpContext);
            JwtToken = GetJwtTokenFromCookie();

            return Task.FromResult(true);
        }

        /// <summary>
        /// Load thông tin user từ database
        /// </summary>
        private async Task LoadUserInformationAsync()
        {
            CurrentUser = await JwtHelper.GetCurrentUserAsync(HttpContext, _userRepository);
            
            if (CurrentUser == null)
            {
                // Nếu không lấy được thông tin user, có thể token đã hết hạn
                ErrorMessage = "Không thể lấy thông tin user. Vui lòng đăng nhập lại.";
            }
        }

        /// <summary>
        /// Lấy JWT token từ cookie để hiển thị
        /// </summary>
        /// <returns>JWT token string hoặc null</returns>
        private string? GetJwtTokenFromCookie()
        {
            var cookieName = HttpContext.RequestServices.GetService<IConfiguration>()?["Jwt:CookieName"] ?? "PlantpediaJWT";
            return Request.Cookies[cookieName];
        }

        /// <summary>
        /// Redirect đến trang login
        /// </summary>
        /// <returns>IActionResult</returns>
        private IActionResult RedirectToLogin()
        {
            return RedirectToPage("/Auth/Login");
        }
        #endregion
    }
}
