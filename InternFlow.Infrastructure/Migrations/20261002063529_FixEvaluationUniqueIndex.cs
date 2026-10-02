using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEvaluationUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Evaluations_EvaluationPeriodId_InternId_EvaluatorId",
                schema: "dbo",
                table: "Evaluations");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_EvaluationPeriodId_InternId",
                schema: "dbo",
                table: "Evaluations",
                columns: new[] { "EvaluationPeriodId", "InternId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Evaluations_EvaluationPeriodId_InternId",
                schema: "dbo",
                table: "Evaluations");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_EvaluationPeriodId_InternId_EvaluatorId",
                schema: "dbo",
                table: "Evaluations",
                columns: new[] { "EvaluationPeriodId", "InternId", "EvaluatorId" },
                unique: true);
        }
    }
}
