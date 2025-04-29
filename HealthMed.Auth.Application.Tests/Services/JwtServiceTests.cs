using FluentAssertions;
using HealthMed.Auth.Application.Services;
using HealthMed.Auth.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HealthMed.Auth.Application.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            _jwtService = new JwtService();
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Dr. Who",
                Role = "Doctor"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateToken_ShouldContainCorrectClaims()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Patient Zero",
                Role = "Patient"
            };

            // Act
            var token = _jwtService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            var claims = jwtToken.Claims.ToList();

            claims.Should().ContainSingle(c =>
                c.Type == "nameid" &&
                c.Value == user.Id.ToString());

            claims.Should().ContainSingle(c =>
                c.Type == "unique_name" &&
                c.Value == user.Name);

            claims.Should().ContainSingle(c =>
                c.Type == "role" &&
                c.Value == user.Role);
        }


        [Fact]
        public void GenerateToken_ShouldHaveExpirationInTwoHours()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Nurse Joy",
                Role = "Nurse"
            };

            // Act
            var token = _jwtService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(2), precision: TimeSpan.FromMinutes(1));
        }
    }
}
