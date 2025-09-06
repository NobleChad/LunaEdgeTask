using AutoMapper;
using LunaEdgeTask.DTOS;
using LunaEdgeTask.Models;
using LunaEdgeTask.Services;
using LunaEdgeTask.Validators;
using Moq;

namespace LunaEdgeTask.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly IMapper _mapper;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<User>(It.IsAny<RegisterDto>()))
                      .Returns((RegisterDto dto) => new User
                      {
                          Username = dto.Username,
                          Email = dto.Email
                      });

            _mapper = mapperMock.Object;

            _userService = new UserService(_userRepoMock.Object, _mapper);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnSuccess_WhenValid()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "StrongPass1!"
            };

            _userRepoMock.Setup(r => r.ExistsByUsernameOrEmailAsync(dto.Username, dto.Email))
                .ReturnsAsync(false);

            // Act
            var result = await _userService.RegisterUserAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User registered successfully", result.Message);
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
            _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldFail_WhenUserExists()
        {
            var dto = new RegisterDto
            {
                Username = "existing",
                Email = "exist@example.com",
                Password = "StrongPass1!"
            };

            _userRepoMock.Setup(r => r.ExistsByUsernameOrEmailAsync(dto.Username, dto.Email))
                .ReturnsAsync(true);

            var result = await _userService.RegisterUserAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Username or Email already exists.", result.Message);
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldFail_WhenPasswordInvalid()
        {
            var dto = new RegisterDto
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = "weak"
            };

            _userRepoMock.Setup(r => r.ExistsByUsernameOrEmailAsync(dto.Username, dto.Email))
                .ReturnsAsync(false);

            var result = await _userService.RegisterUserAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Password does not meet complexity requirements.", result.Message);
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnUser_WhenCredentialsValid()
        {
            var dto = new LoginDto
            {
                UsernameOrEmail = "testuser",
                Password = "StrongPass1!"
            };

            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User { Username = "testuser", PasswordHash = hashed };

            _userRepoMock.Setup(r => r.GetByUsernameOrEmailAsync(dto.UsernameOrEmail))
                .ReturnsAsync(user);

            var result = await _userService.AuthenticateUserAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenPasswordWrong()
        {
            var dto = new LoginDto
            {
                UsernameOrEmail = "testuser",
                Password = "WrongPass!"
            };

            var user = new User
            {
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1!")
            };

            _userRepoMock.Setup(r => r.GetByUsernameOrEmailAsync(dto.UsernameOrEmail))
                .ReturnsAsync(user);

            var result = await _userService.AuthenticateUserAsync(dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenUserNotFound()
        {
            var dto = new LoginDto
            {
                UsernameOrEmail = "notfound",
                Password = "SomePass1!"
            };

            _userRepoMock.Setup(r => r.GetByUsernameOrEmailAsync(dto.UsernameOrEmail))
                .ReturnsAsync((User?)null);

            var result = await _userService.AuthenticateUserAsync(dto);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("short", false)]
        [InlineData("NoDigits!", false)]
        [InlineData("nouppercase1!", false)]
        [InlineData("NOLOWERCASE1!", false)]
        [InlineData("Valid1Password!", true)]
        public void PasswordValidator_ShouldValidateCorrectly(string password, bool expected)
        {
            var result = PasswordValidator.IsValid(password);
            Assert.Equal(expected, result);
        }
    }
}