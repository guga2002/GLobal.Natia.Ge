using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jandag.DLL.Migrations
{
    /// <inheritdoc />
    public partial class migrateh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sources_SatteliteFrequencies_FrequencyId",
                table: "Sources");

            migrationBuilder.AlterColumn<int>(
                name: "FrequencyId",
                table: "Sources",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Sources_SatteliteFrequencies_FrequencyId",
                table: "Sources",
                column: "FrequencyId",
                principalTable: "SatteliteFrequencies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sources_SatteliteFrequencies_FrequencyId",
                table: "Sources");

            migrationBuilder.AlterColumn<int>(
                name: "FrequencyId",
                table: "Sources",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sources_SatteliteFrequencies_FrequencyId",
                table: "Sources",
                column: "FrequencyId",
                principalTable: "SatteliteFrequencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
