using Microsoft.AspNetCore.Http;
using Plantpedia.Models;
using Plantpedia.Repository;
using System.Security.Claims;

namespace Plantpedia.Service
{
    /// <summary>
    /// Helper class cung cấp các method tiện ích để làm việc với JWT authentication
    /// </summary>
    public static class JwtHelper
    {
        #region Constants
        private const string USER_ID_ITEM_KEY = "UserId";
        private const string USERNAME_ITEM_KEY = "Username";
        private const string LOGIN_URL = "/Auth/Login";
        private const string RETURN_URL_PARAM = "returnUrl";
        #endregion

        #region Public Methods - User Information
        /// <summary>
        /// Lấy UserId từ JWT token trong HttpContext
        /// Ưu tiên: JWT Claims > Context Items > Session (backward compatibility)
        /// </summary>
        /// <param name="context">HttpContext hiện tại</param>
        /// <returns>UserId hoặc null nếu không tìm thấy</returns>
        public static int? GetCurrentUserId(HttpContext context)
        {
            if (context == null) return null;

            // 1. Thử lấy từ JWT claims (ưu tiên cao nhất)
            if (IsJwtAuthenticated(context))
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }

            // 2. Fallback: lấy từ context items (được set bởi middleware)
            if (context.Items.TryGetValue(USER_ID_ITEM_KEY, out var userIdFromItems) && 
                userIdFromItems is int userIdInt)
            {
                return userIdInt;
            }

            // 3. Fallback: lấy từ session (backward compatibility)
            return context.Session.GetInt32(USER_ID_ITEM_KEY);
        }

        /// <summary>
        /// Lấy Username từ JWT token trong HttpContext
        /// Ưu tiên: JWT Claims > Context Items > Session (backward compatibility)
        /// </summary>
        /// <param name="context">HttpContext hiện tại</param>
        /// <returns>Username hoặc null nếu không tìm thấy</returns>
        public static string? GetCurrentUsername(HttpContext context)
        {
            if (context == null) return null;

            // 1. Thử lấy từ JWT claims (ưu tiên cao nhất)
            if (IsJwtAuthenticated(context))
            {
                var usernameClaim = context.User.FindFirst(ClaimTypes.Name);
                if (usernameClaim != null)
                {
                    return usernameClaim.Value;
                }
            }

            // 2. Fallback: lấy từ context items
            if (context.Items.TryGetValue(USERNAME_ITEM_KEY, out var usernameFromItems) && 
                usernameFromItems is string username)
            {
                return username;
            }

            // 3. Fallback: lấy từ session
            return context.Session.GetString(USERNAME_ITEM_KEY);
        }

        /// <summary>
        /// Kiểm tra user có đăng nhập không (bất kỳ phương thức nào)
        /// </summary>
        /// <param name="context">HttpContext hiện tại</param>
        /// <returns>true nếu user đã đăng nhập</returns>
        public static bool IsUserAuthenticated(HttpContext context)
        {
            return GetCurrentUserId(context).HasValue;
        }

        /// <summary>
        /// Lấy thông tin user hiện tại từ database
        /// </summary>
        /// <param name="context">HttpContext hiện tại</param>
        /// <param name="userRepository">UserRepository instance</param>
        /// <returns>UserAccount hoặc null nếu không tìm thấy</returns>
        public static async Task<UserAccount?> GetCurrentUserAsync(HttpContext context, UserRepository userRepository)
        {
            if (context == null || userRepository == null) return null;

            var userId = GetCurrentUserId(context);
            if (!userId.HasValue)
            {
                return null;
            }

            try
            {
                return await userRepository.GetUserByIdAsync(userId.Value);
            }
            catch
            {
                // Log error nếu cần
                return null;
            }
        }
        #endregion

        #region Public Methods - Authentication Control
        /// <summary>
        /// Redirect đến trang login nếu user chưa đăng nhập
        /// </summary>
        /// <param name="context">HttpContext hiện tại</param>
        /// <param name="redirectUrl">URL để redirect sau khi đăng nhập thành công</param>
        /// <returns>true nếu đã redirect, false nếu user đã đăng nhập</returns>
        public static bool RequireAuthentication(HttpContext context, string? redirectUrl = null)
        {
            if (context == null) return false;

            if (!IsUserAuthenticated(context))
            {
                var loginUrl = LOGIN_URL;
                
                // Thêm returnUrl nếu được cung cấp
                if (!string.IsNullOrWhiteSpace(redirectUrl))
                {
                    loginUrl += $"?{RETURN_URL_PARAM}={Uri.EscapeDataString(redirectUrl)}";
                }
                
                context.Response.Redirect(loginUrl);
                return true;
            }
            
            return false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Kiểm tra xem user có được authenticate bằng JWT không
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>true nếu JWT authentication thành công</returns>
        private static bool IsJwtAuthenticated(HttpContext context)
        {
            return context.User?.Identity?.IsAuthenticated == true;
        }
        #endregion
    }
}

