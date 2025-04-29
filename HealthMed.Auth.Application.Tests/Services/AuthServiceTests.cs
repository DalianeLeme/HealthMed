using FluentAssertions;
using HealthMed.Auth.Application.Services;
using HealthMed.Auth.Domain.Entities;
using HealthMed.Auth.Domain.Interfaces;
using Moq;

namespace HealthMed.Auth.Application.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authService = new AuthService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnFalse_WhenUserAlreadyExists()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetByIdentifierAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());

            // Act
            var result = await _authService.RegisterUserAsync("Test", "test@test.com", "password", "Patient");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldThrowException_WhenDoctorFieldsAreMissing()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetByIdentifierAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var action = async () => await _authService.RegisterUserAsync("Doctor", "doc@test.com", "password", "Doctor");

            // Assert
            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*obrigatórios para médicos*");
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldRegisterDoctor_WhenAllFieldsAreProvided()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetByIdentifierAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.RegisterUserAsync(
                name: "Dr. House",
                email: "house@test.com",
                password: "password",
                role: "Doctor",
                crm: "12345",
                specialty: "Cardiology",
                consultationFee: 500
            );

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u =>
                u.Profile != null &&
                u.Profile.Specialty == "Cardiology"
            )), Times.Once);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.FindByCRMAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.FindByCPFAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var user = await _authService.AuthenticateUserAsync("nonexistent", "password");

            // Assert
            user.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenPasswordIsWrong()
        {
            // Arrange
            var user = new User
            {
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
            };

            _userRepositoryMock.Setup(r => r.FindByCRMAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.FindByCPFAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.AuthenticateUserAsync("email", "wrongpassword");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnUser_WhenCredentialsAreCorrect()
        {
            // Arrange
            var user = new User
            {
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
            };

            _userRepositoryMock.Setup(r => r.FindByCRMAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.FindByCPFAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.AuthenticateUserAsync("email", "password123");

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllDoctorsAsync_ShouldReturnAllDoctors()
        {
            // Arrange
            var doctors = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "Doc1", Email = "doc1@test.com", Role = "Doctor", Profile = new DoctorProfile { Specialty = "Cardiology" } },
                new User { Id = Guid.NewGuid(), Name = "Doc2", Email = "doc2@test.com", Role = "Doctor", Profile = new DoctorProfile { Specialty = "Dermatology" } }
            };

            _userRepositoryMock.Setup(r => r.GetAllDoctorsAsync())
                .ReturnsAsync(doctors);

            // Act
            var result = await _authService.GetAllDoctorsAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllDoctorsAsync_ShouldFilterBySpecialty()
        {
            // Arrange
            var doctors = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "Doc1", Email = "doc1@test.com", Role = "Doctor", Profile = new DoctorProfile { Specialty = "Cardiology" } },
                new User { Id = Guid.NewGuid(), Name = "Doc2", Email = "doc2@test.com", Role = "Doctor", Profile = new DoctorProfile { Specialty = "Dermatology" } }
            };

            _userRepositoryMock.Setup(r => r.GetAllDoctorsAsync())
                .ReturnsAsync(doctors);

            // Act
            var result = await _authService.GetAllDoctorsAsync("Cardiology");

            // Assert
            result.Should().HaveCount(1);
            result.First().Specialty.Should().Be("Cardiology");
        }
    }
}
