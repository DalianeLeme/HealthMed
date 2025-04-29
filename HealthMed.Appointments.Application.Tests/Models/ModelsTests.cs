using FluentAssertions;
using HealthMed.Appointments.Application.Models;
using HealthMed.Appointments.Domain.Enums;

namespace HealthMed.Appointments.Application.Tests.Models
{
    public class ModelsTests
    {
        [Fact]
        public void CancelAppointmentRequest_ShouldDefaultToEmptyJustification()
        {
            var model = new CancelAppointmentRequest();
            model.Justification.Should().BeEmpty();
        }

        [Fact]
        public void CancelAppointmentRequest_ShouldAcceptJustification()
        {
            var model = new CancelAppointmentRequest { Justification = "I'm sick" };
            model.Justification.Should().Be("I'm sick");
        }

        [Fact]
        public void RescheduleRequest_ShouldStoreAppointmentIdAndSlotId()
        {
            var appointmentId = Guid.NewGuid();
            var newSlotId = Guid.NewGuid();

            var model = new RescheduleRequest
            {
                AppointmentId = appointmentId,
                NewSlotId = newSlotId
            };

            model.AppointmentId.Should().Be(appointmentId);
            model.NewSlotId.Should().Be(newSlotId);
        }

        [Fact]
        public void ScheduleRequest_ShouldAcceptSlotId()
        {
            var slotId = Guid.NewGuid();

            var model = new ScheduleRequest
            {
                SlotId = slotId
            };

            model.SlotId.Should().Be(slotId);
        }

        [Fact]
        public void UpdateStatusRequest_ShouldDefaultToAccepted()
        {
            var model = new UpdateStatusRequest();
            model.NewStatus.Should().Be(AppointmentStatus.Accepted);
        }

        [Fact]
        public void UpdateStatusRequest_ShouldAcceptCustomValues()
        {
            var id = Guid.NewGuid();

            var model = new UpdateStatusRequest
            {
                AppointmentId = id,
                NewStatus = AppointmentStatus.Rejected
            };

            model.AppointmentId.Should().Be(id);
            model.NewStatus.Should().Be(AppointmentStatus.Rejected);
        }
    }
}
