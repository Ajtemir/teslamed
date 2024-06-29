using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeslaMed.Migrations
{
    public partial class nav_prop_diagnostics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DicomPathAndImagesPaths_Diagnostics_DiagnosticsId",
                table: "DicomPathAndImagesPaths");

            migrationBuilder.AlterColumn<int>(
                name: "DiagnosticsId",
                table: "DicomPathAndImagesPaths",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DicomPathAndImagesPaths_Diagnostics_DiagnosticsId",
                table: "DicomPathAndImagesPaths",
                column: "DiagnosticsId",
                principalTable: "Diagnostics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DicomPathAndImagesPaths_Diagnostics_DiagnosticsId",
                table: "DicomPathAndImagesPaths");

            migrationBuilder.AlterColumn<int>(
                name: "DiagnosticsId",
                table: "DicomPathAndImagesPaths",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_DicomPathAndImagesPaths_Diagnostics_DiagnosticsId",
                table: "DicomPathAndImagesPaths",
                column: "DiagnosticsId",
                principalTable: "Diagnostics",
                principalColumn: "Id");
        }
    }
}
