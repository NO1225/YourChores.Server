using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YourChores.Relational.MSSQL.Migrations
{
    public partial class Addingappversiontable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppVersions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Version = table.Column<int>(nullable: false),
                    LowestAllowedVersion = table.Column<int>(nullable: false),
                    Message = table.Column<string>(maxLength: 700, nullable: false),
                    DownloadURL = table.Column<string>(maxLength: 700, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVersions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppVersions_Version",
                table: "AppVersions",
                column: "Version",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppVersions");
        }
    }
}
