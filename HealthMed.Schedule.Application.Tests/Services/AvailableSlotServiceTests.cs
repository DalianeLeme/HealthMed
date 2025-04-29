using FluentAssertions;
using HealthMed.Schedule.Application.Services;
using HealthMed.Schedule.Domain.Entities;
using HealthMed.Schedule.Domain.Interfaces;
using HealthMed.Shared.Events;
using HealthMed.Shared.Messaging;
using Moq;

namespace HealthMed.Schedule.Application.Tests.Services
{
    public class AvailableSlotServiceTests
    {
        private readonly Mock<IAvailableSlotRepository> _repositoryMock;
        private readonly Mock<IEventPublisher> _publisherMock;
        private readonly AvailableSlotService _service;

        public AvailableSlotServiceTests()
        {
            _repositoryMock = new Mock<IAvailableSlotRepository>();
            _publisherMock = new Mock<IEventPublisher>();
            _service = new AvailableSlotService(_repositoryMock.Object, _publisherMock.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnFalse_WhenConflictExists()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var existingSlots = new List<AvailableSlot>
            {
                new AvailableSlot
                {
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    DoctorId = doctorId
                }
            };

            var newSlot = new AvailableSlot
            {
                StartTime = DateTime.UtcNow.AddMinutes(30),
                EndTime = DateTime.UtcNow.AddHours(1).AddMinutes(30),
                DoctorId = doctorId
            };

            _repositoryMock.Setup(r => r.GetByDoctorAsync(doctorId)).ReturnsAsync(existingSlots);

            // Act
            var result = await _service.AddAsync(newSlot);

            // Assert
            result.Should().BeFalse();
            _publisherMock.Verify(p => p.Publish(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_ShouldAddSlotAndPublishEvent_WhenNoConflict()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var existingSlots = new List<AvailableSlot>(); // vazio

            var newSlot = new AvailableSlot
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                DoctorId = doctorId
            };

            _repositoryMock.Setup(r => r.GetByDoctorAsync(doctorId)).ReturnsAsync(existingSlots);

            // Act
            var result = await _service.AddAsync(newSlot);

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.AddAsync(newSlot), Times.Once);
            _publisherMock.Verify(p => p.Publish(nameof(SlotCreated), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDoNothing_WhenSlotNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AvailableSlot)null);

            // Act
            await _service.DeleteAsync(Guid.NewGuid());

            // Assert
            _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<AvailableSlot>()), Times.Never);
            _publisherMock.Verify(p => p.Publish(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteSlotAndPublishEvent_WhenSlotExists()
        {
            // Arrange
            var slot = new AvailableSlot
            {
                Id = Guid.NewGuid(),
                DoctorId = Guid.NewGuid()
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);

            // Act
            await _service.DeleteAsync(slot.Id);

            // Assert
            _repositoryMock.Verify(r => r.DeleteAsync(slot), Times.Once);
            _publisherMock.Verify(p => p.Publish(nameof(SlotDeleted), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task RemoveSlotByTimeAsync_ShouldReturnFalse_WhenSlotNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByTimeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
                .ReturnsAsync((AvailableSlot)null);

            // Act
            var result = await _service.RemoveSlotByTimeAsync(Guid.NewGuid(), DateTime.UtcNow);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveSlotByTimeAsync_ShouldDeleteSlot_WhenSlotFound()
        {
            // Arrange
            var slot = new AvailableSlot
            {
                Id = Guid.NewGuid(),
                DoctorId = Guid.NewGuid()
            };

            _repositoryMock.Setup(r => r.GetByTimeAsync(slot.DoctorId, slot.StartTime))
                .ReturnsAsync(slot);

            _repositoryMock.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);

            // Act
            var result = await _service.RemoveSlotByTimeAsync(slot.DoctorId, slot.StartTime);

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.DeleteAsync(slot), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenSlotNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((AvailableSlot)null);

            // Act
            var result = await _service.UpdateAsync(new AvailableSlot { Id = Guid.NewGuid() });

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateSlotAndPublishEvents_WhenSlotExists()
        {
            // Arrange
            var oldSlot = new AvailableSlot
            {
                Id = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };

            var updatedSlot = new AvailableSlot
            {
                Id = oldSlot.Id,
                DoctorId = oldSlot.DoctorId,
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(3)
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(oldSlot.Id))
                .ReturnsAsync(oldSlot);

            // Act
            var result = await _service.UpdateAsync(updatedSlot);

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.UpdateAsync(updatedSlot), Times.Once);
            _publisherMock.Verify(p => p.Publish(nameof(SlotDeleted), It.IsAny<object>()), Times.Once);
            _publisherMock.Verify(p => p.Publish(nameof(SlotCreated), It.IsAny<object>()), Times.Once);
        }
    }
}
