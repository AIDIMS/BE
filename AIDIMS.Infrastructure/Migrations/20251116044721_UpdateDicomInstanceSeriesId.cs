using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDIMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDicomInstanceSeriesId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DicomInstances_DicomSeries_SeriesUid",
                table: "DicomInstances");

            migrationBuilder.RenameColumn(
                name: "SeriesUid",
                table: "DicomInstances",
                newName: "SeriesId");

            migrationBuilder.RenameIndex(
                name: "IX_DicomInstances_SeriesUid",
                table: "DicomInstances",
                newName: "IX_DicomInstances_SeriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_DicomInstances_DicomSeries_SeriesId",
                table: "DicomInstances",
                column: "SeriesId",
                principalTable: "DicomSeries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DicomInstances_DicomSeries_SeriesId",
                table: "DicomInstances");

            migrationBuilder.RenameColumn(
                name: "SeriesId",
                table: "DicomInstances",
                newName: "SeriesUid");

            migrationBuilder.RenameIndex(
                name: "IX_DicomInstances_SeriesId",
                table: "DicomInstances",
                newName: "IX_DicomInstances_SeriesUid");

            migrationBuilder.AddForeignKey(
                name: "FK_DicomInstances_DicomSeries_SeriesUid",
                table: "DicomInstances",
                column: "SeriesUid",
                principalTable: "DicomSeries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
