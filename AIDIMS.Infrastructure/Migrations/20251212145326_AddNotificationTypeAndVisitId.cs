using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDIMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTypeAndVisitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedVisitId",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedVisitId",
                table: "Notifications",
                column: "RelatedVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_PatientVisits_RelatedVisitId",
                table: "Notifications",
                column: "RelatedVisitId",
                principalTable: "PatientVisits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_PatientVisits_RelatedVisitId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedVisitId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Type",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedVisitId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");
        }
    }
}
