using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Plantpedia.Middleware
{
    /// <summary>
    /// Middleware xử lý JWT authentication từ cookie
    /// Tự động validate JWT token và set user context
    /// </summary>
    public class JwtCookieMiddleware
    {
        #region Private Fields
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _cookieName;
        #endregion

        #region Constructor
        /// <summary>
        /// Khởi tạo JwtCookieMiddleware
        /// </summary>
        /// <param name="next">RequestDelegate tiếp theo trong pipeline</param>
        /// <param name="configuration">IConfiguration để đọc cấu hình JWT</param>
        public JwtCookieMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            // Đọc cấu hình JWT
            _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey không được cấu hình");
            _issuer = _configuration["Jwt:Issuer"] ?? "Plantpedia";
            _audience = _configuration["Jwt:Audience"] ?? "PlantpediaUsers";
            _cookieName = _configuration["Jwt:CookieName"] ?? "PlantpediaJWT";
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Xử lý HTTP request - validate JWT token từ cookie
        /// </summary>
        /// <param name="context">HttpContext hiện tại</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null) return;

            // Lấy JWT token từ cookie
            var token = GetTokenFromCookie(context);

            if (!string.IsNullOrEmpty(token))
            {
                // Validate và xử lý token
                await ProcessJwtTokenAsync(context, token);
            }

            // Tiếp tục pipeline
            await _next(context);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Lấy JWT token từ cookie
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>JWT token string hoặc null</returns>
        private string? GetTokenFromCookie(HttpContext context)
        {
            return context.Request.Cookies[_cookieName];
        }

        /// <summary>
        /// Xử lý và validate JWT token
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="token">JWT token string</param>
        private async Task ProcessJwtTokenAsync(HttpContext context, string token)
        {
            try
            {
                // Validate token
                var principal = ValidateJwtToken(token);
                
                if (principal != null)
                {
                    // Set user context
                    context.User = principal;
                    
                    // Lưu thông tin user vào context items để dễ truy cập
                    SetUserContextItems(context, principal);
                }
            }
            catch (SecurityTokenExpiredException)
            {
                // Token hết hạn - xóa cookie
                await ClearInvalidTokenAsync(context);
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                // Token không hợp lệ - xóa cookie
                await ClearInvalidTokenAsync(context);
            }
            catch (Exception)
            {
                // Lỗi khác - xóa cookie để đảm bảo an toàn
                await ClearInvalidTokenAsync(context);
            }
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <param name="token">JWT token string</param>
        /// <returns>ClaimsPrincipal nếu token hợp lệ, null nếu không</returns>
        private ClaimsPrincipal? ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Không cho phép sai lệch thời gian
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }

        /// <summary>
        /// Set thông tin user vào context items
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="principal">ClaimsPrincipal</param>
        private static void SetUserContextItems(HttpContext context, ClaimsPrincipal principal)
        {
            // Lấy UserId
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Items["UserId"] = userId;
            }

            // Lấy Username
            var usernameClaim = principal.FindFirst(ClaimTypes.Name);
            if (usernameClaim != null)
            {
                context.Items["Username"] = usernameClaim.Value;
            }
        }

        /// <summary>
        /// Xóa cookie khi token không hợp lệ
        /// </summary>
        /// <param name="context">HttpContext</param>
        private async Task ClearInvalidTokenAsync(HttpContext context)
        {
            // Xóa cookie
            context.Response.Cookies.Delete(_cookieName);
            
            // Log warning nếu cần (có thể thêm logging service)
            // _logger.LogWarning("Invalid JWT token detected and cleared");
            
            await Task.CompletedTask;
        }
        #endregion
    }

    /// <summary>
    /// Extension methods cho JwtCookieMiddleware
    /// </summary>
    public static class JwtCookieMiddlewareExtensions
    {
        /// <summary>
        /// Đăng ký JwtCookieMiddleware vào pipeline
        /// </summary>
        /// <param name="builder">IApplicationBuilder</param>
        /// <returns>IApplicationBuilder</returns>
        public static IApplicationBuilder UseJwtCookieAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieMiddleware>();
        }
    }
}
