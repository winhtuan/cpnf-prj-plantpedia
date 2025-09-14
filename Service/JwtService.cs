using Microsoft.IdentityModel.Tokens;
using Plantpedia.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Plantpedia.Service
{
    /// <summary>
    /// Service xử lý JWT token - tạo, validate và quản lý token
    /// </summary>
    public class JwtService
    {
        #region Private Fields
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;
        #endregion

        #region Constructor
        /// <summary>
        /// Khởi tạo JwtService với cấu hình từ appsettings.json
        /// </summary>
        /// <param name="configuration">IConfiguration để đọc cấu hình JWT</param>
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            // Đọc cấu hình JWT từ appsettings.json
            _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey không được cấu hình");
            _issuer = _configuration["Jwt:Issuer"] ?? "Plantpedia";
            _audience = _configuration["Jwt:Audience"] ?? "PlantpediaUsers";
            
            // Parse expiration time với validation
            if (!int.TryParse(_configuration["Jwt:ExpirationMinutes"], out _expirationMinutes) || _expirationMinutes <= 0)
            {
                _expirationMinutes = 60; // Default 60 phút
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Tạo JWT token cho user đã đăng nhập
        /// </summary>
        /// <param name="user">Thông tin user từ database</param>
        /// <returns>JWT token string</returns>
        /// <exception cref="ArgumentNullException">Khi user là null</exception>
        public string GenerateToken(UserAccount user)
        {
            // Validation đầu vào
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Khởi tạo token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            // Tạo claims cho token
            var claims = CreateUserClaims(user);

            // Tạo token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            // Tạo và trả về token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Tạo danh sách claims cho user
        /// </summary>
        /// <param name="user">Thông tin user</param>
        /// <returns>Danh sách claims</returns>
        private static List<Claim> CreateUserClaims(UserAccount user)
        {
            return new List<Claim>
            {
                // Thông tin cơ bản
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserLoginDatum?.Username ?? string.Empty),
                
                // Thông tin bổ sung
                new Claim("LastName", user.LastName ?? string.Empty),
                new Claim("Gender", user.Gender.ToString()),
                new Claim("DateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd")),
                new Claim("AvatarUrl", user.AvatarUrl ?? string.Empty),
                
                // JWT standard claims
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };
        }
        #endregion
    }
}

