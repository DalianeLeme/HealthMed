using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthMed.Appointments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FilterIndexToActiveAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId_ScheduledTime",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId_ScheduledTime",
                table: "Appointments",
                columns: new[] { "DoctorId", "ScheduledTime" },
                unique: true,
                filter: "[Status] IN ('Pending','Accepted')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId_ScheduledTime",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId_ScheduledTime",
                table: "Appointments",
                columns: new[] { "DoctorId", "ScheduledTime" },
                unique: true);
        }
    }
}
