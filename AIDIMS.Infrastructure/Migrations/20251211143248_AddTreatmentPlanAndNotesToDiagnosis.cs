using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDIMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatmentPlanAndNotesToDiagnosis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Diagnoses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TreatmentPlan",
                table: "Diagnoses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Diagnoses");

            migrationBuilder.DropColumn(
                name: "TreatmentPlan",
                table: "Diagnoses");
        }
    }
}
