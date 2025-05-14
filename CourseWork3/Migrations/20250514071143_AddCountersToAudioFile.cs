using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseWork3.Migrations
{
    /// <inheritdoc />
    public partial class AddCountersToAudioFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DownloadCount",
                table: "AudioFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlayCount",
                table: "AudioFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadCount",
                table: "AudioFiles");

            migrationBuilder.DropColumn(
                name: "PlayCount",
                table: "AudioFiles");
        }
    }
}
