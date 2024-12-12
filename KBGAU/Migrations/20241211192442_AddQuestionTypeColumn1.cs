using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KBGAU.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionTypeColumn1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatchText",
                table: "Answers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchText",
                table: "Answers");
        }
    }
}
