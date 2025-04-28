using HealthMed.Appointments.Domain.Entities;
using HealthMed.Appointments.Domain.Interfaces;
using HealthMed.Appointments.Infrastructure.Data;
using HealthMed.Auth.Domain;
using HealthMed.Auth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthMed.Appointments.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppointmentsDbContext _context;

        public AppointmentRepository(AppointmentsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctor(Guid doctorId)
        {
            return await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByPatient(Guid patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .ToListAsync();
        }

        public async Task AddAppointment(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAppointment(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<Appointment?> FindByIdAsync(Guid appointmentId)
        {
            return await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        public Task<bool> ExistsByDoctorAndTimeAsync(Guid doctorId, DateTime scheduledTime)
            => _context.Appointments
                  .AsNoTracking()
                  .AnyAsync(a =>
                      a.DoctorId == doctorId &&
                      a.ScheduledTime == scheduledTime
                  );
    }
}
