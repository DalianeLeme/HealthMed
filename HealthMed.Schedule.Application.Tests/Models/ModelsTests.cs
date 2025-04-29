using FluentAssertions;
using HealthMed.Schedule.Application.Models;

namespace HealthMed.Schedule.Application.Tests.Models
{
    public class ModelsTests
    {
        [Fact]
        public void GetAvailableSlotsRequest_ShouldSetAndGetDoctorIdCorrectly()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var request = new GetAvailableSlotsRequest
            {
                DoctorId = doctorId
            };

            // Act & Assert
            request.DoctorId.Should().Be(doctorId);
        }

        [Fact]
        public void UpdateStatusResult_ShouldContainExpectedEnumValues()
        {
            // Assert
            ((int)UpdateStatusResult.NotFound).Should().Be(0);
            ((int)UpdateStatusResult.Forbidden).Should().Be(1);
            ((int)UpdateStatusResult.Success).Should().Be(2);
        }
    }
}
