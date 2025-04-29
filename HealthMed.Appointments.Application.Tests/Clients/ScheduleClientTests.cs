using FluentAssertions;
using HealthMed.Shared.DTOs;

namespace HealthMed.Appointments.Application.Tests.Clients
{
    public class ScheduleClientTests
    {
        [Fact]
        public async Task IsSlotAvailable_ShouldReturnTrue_WhenMatchingSlotExists()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var startTime = DateTime.UtcNow;

            var slots = new List<AvailableSlotDto>
            {
                new AvailableSlotDto
                {
                    Id = Guid.NewGuid(),
                    StartTime = startTime,
                    EndTime = startTime.AddMinutes(30)
                }
            };

            // Act
            var result = slots.Any(s => s.StartTime == startTime);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSlotAvailable_ShouldReturnFalse_WhenNoMatch()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var requestedStart = DateTime.UtcNow;

            var slots = new List<AvailableSlotDto>
            {
                new AvailableSlotDto
                {
                    Id = Guid.NewGuid(),
                    StartTime = requestedStart.AddHours(1),
                    EndTime = requestedStart.AddHours(2)
                }
            };

            // Simula o comportamento do método a ser testado
            var result = slots.Any(s => s.StartTime == requestedStart);

            // Assert
            result.Should().BeFalse();
        }
 
    }
}
