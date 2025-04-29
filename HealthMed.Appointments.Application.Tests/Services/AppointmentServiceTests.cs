using FluentAssertions;
using HealthMed.Appointments.Application.Services;
using HealthMed.Appointments.Domain.Entities;
using HealthMed.Appointments.Domain.Enums;
using HealthMed.Appointments.Domain.Interfaces;
using HealthMed.Shared.Events;
using HealthMed.Shared.Messaging;
using Moq;


namespace HealthMed.Appointments.Application.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
        private readonly Mock<IAvailableSlotProjectionRepository> _slotRepoMock;
        private readonly Mock<IEventPublisher> _publisherMock;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _appointmentRepoMock = new Mock<IAppointmentRepository>();
            _slotRepoMock = new Mock<IAvailableSlotProjectionRepository>();
            _publisherMock = new Mock<IEventPublisher>();
            _service = new AppointmentService(_appointmentRepoMock.Object, _slotRepoMock.Object, _publisherMock.Object);
        }

        [Fact]
        public async Task ScheduleAppointmentAsync_ShouldReturnFalse_WhenSlotNotFound()
        {
            // Arrange
            _slotRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((AvailableSlotProjection)null);

            // Act
            var result = await _service.ScheduleAppointmentAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ScheduleAppointmentAsync_ShouldReturnFalse_WhenDoctorAlreadyBooked()
        {
            // Arrange
            var slot = new AvailableSlotProjection
            {
                Id = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow
            };

            _slotRepoMock.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _appointmentRepoMock.Setup(r => r.ExistsByDoctorAndTimeAsync(slot.DoctorId, slot.StartTime))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ScheduleAppointmentAsync(slot.Id, Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ScheduleAppointmentAsync_ShouldCreateAppointmentAndPublishEvent_WhenValid()
        {
            // Arrange
            var slot = new AvailableSlotProjection
            {
                Id = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(30)
            };

            _slotRepoMock.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _appointmentRepoMock.Setup(r => r.ExistsByDoctorAndTimeAsync(slot.DoctorId, slot.StartTime)).ReturnsAsync(false);

            // Act
            var result = await _service.ScheduleAppointmentAsync(slot.Id, Guid.NewGuid());

            // Assert
            result.Should().BeTrue();
            _appointmentRepoMock.Verify(r => r.AddAppointment(It.IsAny<Appointment>()), Times.Once);
            _publisherMock.Verify(p => p.Publish(nameof(ConsultationCreated), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAppointmentStatusAsync_ShouldReturnNotFound_WhenAppointmentDoesNotExist()
        {
            _appointmentRepoMock.Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Appointment)null);

            var result = await _service.UpdateAppointmentStatusAsync(Guid.NewGuid(), AppointmentStatus.Accepted, Guid.NewGuid());

            result.Should().Be(UpdateStatusResult.NotFound);
        }

        [Fact]
        public async Task UpdateAppointmentStatusAsync_ShouldReturnForbidden_WhenDoctorIsNotOwner()
        {
            var appt = new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                Status = AppointmentStatus.Pending
            };

            _appointmentRepoMock.Setup(r => r.FindByIdAsync(appt.Id)).ReturnsAsync(appt);

            var result = await _service.UpdateAppointmentStatusAsync(appt.Id, AppointmentStatus.Accepted, Guid.NewGuid()); 

            result.Should().Be(UpdateStatusResult.Forbidden);
        }

        [Fact]
        public async Task UpdateAppointmentStatusAsync_ShouldReturnSuccessAndPublishAccepted_WhenValid()
        {
            var appt = new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                ScheduledTime = DateTime.UtcNow,
                Status = AppointmentStatus.Pending
            };

            _appointmentRepoMock.Setup(r => r.FindByIdAsync(appt.Id)).ReturnsAsync(appt);

            var result = await _service.UpdateAppointmentStatusAsync(appt.Id, AppointmentStatus.Accepted, appt.DoctorId);

            result.Should().Be(UpdateStatusResult.Success);
            _publisherMock.Verify(p => p.Publish(nameof(ConsultationAccepted), It.IsAny<object>()), Times.Once);
        }
    }
}
