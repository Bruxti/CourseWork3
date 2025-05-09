using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseWork3.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AudioFiles_AspNetUsers_UserId",
                table: "AudioFiles");

            migrationBuilder.DropIndex(
                name: "IX_AudioFiles_UserId",
                table: "AudioFiles");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AudioFiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AudioFiles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_AudioFiles_UserId",
                table: "AudioFiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AudioFiles_AspNetUsers_UserId",
                table: "AudioFiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
