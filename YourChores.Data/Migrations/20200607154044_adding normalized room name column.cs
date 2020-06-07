using Microsoft.EntityFrameworkCore.Migrations;

namespace YourChores.Data.Migrations
{
    public partial class addingnormalizedroomnamecolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedRoomName",
                table: "Rooms",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_NormalizedRoomName",
                table: "Rooms",
                column: "NormalizedRoomName",
                unique: true,
                filter: "[NormalizedRoomName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_NormalizedRoomName",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "NormalizedRoomName",
                table: "Rooms");
        }
    }
}
