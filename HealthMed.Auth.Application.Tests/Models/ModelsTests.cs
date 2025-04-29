using FluentAssertions;
using HealthMed.Auth.Application.Models;

namespace HealthMed.Auth.Application.Tests.Models
{
    public class ModelsTests
    {
        [Fact]
        public void LoginRequest_ShouldInitializeWithDefaultValues()
        {
            // Arrange
            var model = new LoginRequest();

            // Act & Assert
            model.Identifier.Should().BeEmpty();
            model.Password.Should().BeEmpty();
        }

        [Fact]
        public void LoginRequest_ShouldSetValuesCorrectly()
        {
            // Arrange
            var model = new LoginRequest
            {
                Identifier = "doctor123",
                Password = "securepassword"
            };

            // Act & Assert
            model.Identifier.Should().Be("doctor123");
            model.Password.Should().Be("securepassword");
        }

        [Fact]
        public void RegisterRequest_ShouldInitializeCorrectly()
        {
            // Arrange
            var model = new RegisterRequest
            {
                Name = "Dr. Strange",
                Email = "strange@med.com",
                Password = "safepassword",
                CPF = "12345678900",
                CRM = "CRM12345",
                Specialty = "Surgery",
                ConsultationValor = 350.00m
            };

            // Act & Assert
            model.Name.Should().Be("Dr. Strange");
            model.Email.Should().Be("strange@med.com");
            model.Password.Should().Be("safepassword");
            model.CPF.Should().Be("12345678900");
            model.CRM.Should().Be("CRM12345");
            model.Specialty.Should().Be("Surgery");
            model.ConsultationValor.Should().Be(350.00m);
        }

        [Fact]
        public void RegisterRequest_ShouldAllowNullOptionalFields()
        {
            // Arrange
            var model = new RegisterRequest
            {
                Name = "Patient Name",
                Email = "patient@email.com",
                Password = "patientpassword"
                // CPF, CRM, Specialty, ConsultationValor são opcionais
            };

            // Act & Assert
            model.CPF.Should().BeNull();
            model.CRM.Should().BeNull();
            model.Specialty.Should().BeNull();
            model.ConsultationValor.Should().BeNull();
        }
    }
}
