using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseWork3.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorToAudioFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "AudioFiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "AudioFiles");
        }
    }
}
