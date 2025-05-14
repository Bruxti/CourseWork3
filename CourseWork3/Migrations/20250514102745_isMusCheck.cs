using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseWork3.Migrations
{
    /// <inheritdoc />
    public partial class isMusCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMusicOnly",
                table: "AudioFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMusicOnly",
                table: "AudioFiles");
        }
    }
}
