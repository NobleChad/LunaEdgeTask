using AutoMapper;
using LunaEdgeTask.DTOS;
using LunaEdgeTask.Models;

namespace LunaEdgeTask.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<(bool Success, string Message)> RegisterUserAsync(RegisterDto dto)
        {
            if (await _userRepository.ExistsByUsernameOrEmailAsync(dto.Username, dto.Email))
                return (false, "Username or Email already exists.");

            if (!PasswordValidator.IsValid(dto.Password))
                return (false, "Password does not meet complexity requirements.");

            // Map DTO → User model and hash the password securely
            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return (true, "User registered successfully");
        }

        public async Task<User?> AuthenticateUserAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(dto.UsernameOrEmail);
            if (user == null) return null;

            // Verify provided password against stored hash
            return BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash) ? user : null;
        }

        public static class PasswordValidator
        {
            public static bool IsValid(string password)
            {
                // Password must be at least 8 chars, include upper, lower, digit, and special character
                if (string.IsNullOrWhiteSpace(password) || password.Length < 8) return false;
                return password.Any(char.IsUpper)
                    && password.Any(char.IsLower)
                    && password.Any(char.IsDigit)
                    && password.Any(ch => !char.IsLetterOrDigit(ch));
            }
        }
    }
}
