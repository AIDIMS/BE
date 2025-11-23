using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDIMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAiAnalysisAndFindings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiResults");

            migrationBuilder.CreateTable(
                name: "AiAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PrimaryFinding = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OverallConfidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiAnalyses_DicomStudies_StudyId",
                        column: x => x.StudyId,
                        principalTable: "DicomStudies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiFindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    XMin = table.Column<decimal>(type: "numeric(10,5)", precision: 10, scale: 5, nullable: true),
                    YMin = table.Column<decimal>(type: "numeric(10,5)", precision: 10, scale: 5, nullable: true),
                    XMax = table.Column<decimal>(type: "numeric(10,5)", precision: 10, scale: 5, nullable: true),
                    YMax = table.Column<decimal>(type: "numeric(10,5)", precision: 10, scale: 5, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiFindings_AiAnalyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "AiAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalyses_AnalysisDate",
                table: "AiAnalyses",
                column: "AnalysisDate");

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalyses_IsReviewed",
                table: "AiAnalyses",
                column: "IsReviewed");

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalyses_StudyId",
                table: "AiAnalyses",
                column: "StudyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiFindings_AnalysisId",
                table: "AiFindings",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_AiFindings_ConfidenceScore",
                table: "AiFindings",
                column: "ConfidenceScore");

            migrationBuilder.CreateIndex(
                name: "IX_AiFindings_Label",
                table: "AiFindings",
                column: "Label");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiFindings");

            migrationBuilder.DropTable(
                name: "AiAnalyses");

            migrationBuilder.CreateTable(
                name: "AiResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Classification = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailedOutput = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    ModelVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiResults_DicomStudies_StudyId",
                        column: x => x.StudyId,
                        principalTable: "DicomStudies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiResults_AnalysisDate",
                table: "AiResults",
                column: "AnalysisDate");

            migrationBuilder.CreateIndex(
                name: "IX_AiResults_IsReviewed",
                table: "AiResults",
                column: "IsReviewed");

            migrationBuilder.CreateIndex(
                name: "IX_AiResults_StudyId",
                table: "AiResults",
                column: "StudyId",
                unique: true);
        }
    }
}
