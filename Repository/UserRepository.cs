using Microsoft.EntityFrameworkCore;
using Plantpedia.Models;
using MyRazorApp.Services;

namespace Plantpedia.Repository
{
    public class UserRepository
    {
        private readonly Plantpedia2Context _context;

        public UserRepository(Plantpedia2Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Đăng nhập người dùng bằng username và password
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu</param>
        /// <returns>UserAccount nếu đăng nhập thành công, null nếu thất bại</returns>
        public async Task<UserAccount?> LoginAsync(string username, string password)
        {
            try
            {
                // Tìm user login data theo username
                var userLoginData = await _context.UserLoginData
                    .Include(uld => uld.User)
                    .FirstOrDefaultAsync(uld => uld.Username == username);

                if (userLoginData == null)
                {
                    return null; // Username không tồn tại
                }

                // Kiểm tra password
                bool isPasswordValid = PasswordHelper.VerifyPassword(
                    password, 
                    userLoginData.PasswordHash, 
                    userLoginData.PasswordSalt
                );

                if (!isPasswordValid)
                {
                    return null; // Password không đúng
                }

                // Cập nhật thời gian đăng nhập cuối
                userLoginData.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return userLoginData.User;
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                throw new Exception($"Lỗi khi đăng nhập: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Đăng ký người dùng mới
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu</param>
        /// <param name="lastName">Họ tên</param>
        /// <param name="gender">Giới tính</param>
        /// <param name="dateOfBirth">Ngày sinh</param>
        /// <param name="avatarUrl">URL avatar</param>
        /// <returns>UserAccount mới được tạo</returns>
        public async Task<UserAccount> RegisterAsync(string username, string password, 
            string lastName, char gender, DateOnly dateOfBirth, string avatarUrl = "")
        {
            try
            {
                // Kiểm tra username đã tồn tại chưa
                bool usernameExists = await _context.UserLoginData
                    .AnyAsync(uld => uld.Username == username);

                if (usernameExists)
                {
                    throw new InvalidOperationException("Tên đăng nhập đã tồn tại");
                }

                // Tạo hash và salt cho password
                var (hash, salt) = PasswordHelper.HashPassword(password);

                // Tạo UserAccount mới
                var newUser = new UserAccount
                {
                    LastName = lastName,
                    Gender = gender,
                    DateOfBirth = dateOfBirth,
                    AvatarUrl = avatarUrl
                };

                // Thêm UserAccount vào database
                _context.UserAccounts.Add(newUser);
                await _context.SaveChangesAsync(); // Lưu để lấy UserId

                // Tạo UserLoginDatum
                var userLoginData = new UserLoginDatum
                {
                    UserId = newUser.UserId,
                    Username = username,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserLoginData.Add(userLoginData);
                await _context.SaveChangesAsync();

                return newUser;
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                throw new Exception($"Lỗi khi đăng ký: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Kiểm tra username có tồn tại không
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <returns>true nếu username tồn tại</returns>
        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.UserLoginData
                .AnyAsync(uld => uld.Username == username);
        }

        /// <summary>
        /// Lấy thông tin user theo UserId
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>UserAccount hoặc null nếu không tìm thấy</returns>
        public async Task<UserAccount?> GetUserByIdAsync(int userId)
        {
            return await _context.UserAccounts
                .Include(ua => ua.UserLoginDatum)
                .FirstOrDefaultAsync(ua => ua.UserId == userId);
        }

        /// <summary>
        /// Lấy thông tin user theo username
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <returns>UserAccount hoặc null nếu không tìm thấy</returns>
        public async Task<UserAccount?> GetUserByUsernameAsync(string username)
        {
            var userLoginData = await _context.UserLoginData
                .Include(uld => uld.User)
                .FirstOrDefaultAsync(uld => uld.Username == username);

            return userLoginData?.User;
        }

        /// <summary>
        /// Cập nhật mật khẩu cho user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="oldPassword">Mật khẩu cũ</param>
        /// <param name="newPassword">Mật khẩu mới</param>
        /// <returns>true nếu cập nhật thành công</returns>
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            try
            {
                var userLoginData = await _context.UserLoginData
                    .FirstOrDefaultAsync(uld => uld.UserId == userId);

                if (userLoginData == null)
                {
                    return false;
                }

                // Kiểm tra mật khẩu cũ
                bool isOldPasswordValid = PasswordHelper.VerifyPassword(
                    oldPassword, 
                    userLoginData.PasswordHash, 
                    userLoginData.PasswordSalt
                );

                if (!isOldPasswordValid)
                {
                    return false;
                }

                // Tạo hash và salt mới cho mật khẩu mới
                var (newHash, newSalt) = PasswordHelper.HashPassword(newPassword);

                // Cập nhật mật khẩu
                userLoginData.PasswordHash = newHash;
                userLoginData.PasswordSalt = newSalt;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                throw new Exception($"Lỗi khi đổi mật khẩu: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật thông tin profile của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="lastName">Họ tên mới</param>
        /// <param name="gender">Giới tính mới</param>
        /// <param name="dateOfBirth">Ngày sinh mới</param>
        /// <param name="avatarUrl">URL avatar mới</param>
        /// <returns>true nếu cập nhật thành công</returns>
        public async Task<bool> UpdateProfileAsync(int userId, string lastName, 
            char gender, DateOnly dateOfBirth, string avatarUrl)
        {
            try
            {
                var user = await _context.UserAccounts
                    .FirstOrDefaultAsync(ua => ua.UserId == userId);

                if (user == null)
                {
                    return false;
                }

                user.LastName = lastName;
                user.Gender = gender;
                user.DateOfBirth = dateOfBirth;
                user.AvatarUrl = avatarUrl;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                throw new Exception($"Lỗi khi cập nhật profile: {ex.Message}", ex);
            }
        }
    }
}
