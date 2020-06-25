using Microsoft.EntityFrameworkCore.Migrations;

namespace YourChores.Relational.MSSQL.Migrations
{
    public partial class Fixingatyponintheownercolumninroomusertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owener",
                table: "RoomUsers");

            migrationBuilder.AddColumn<bool>(
                name: "Owner",
                table: "RoomUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RoomUsers");

            migrationBuilder.AddColumn<bool>(
                name: "Owener",
                table: "RoomUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
