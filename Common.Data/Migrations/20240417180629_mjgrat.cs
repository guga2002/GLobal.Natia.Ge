using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jandag.DLL.Migrations
{
    /// <inheritdoc />
    public partial class mjgrat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chanells_SatteliteFrequencies_SatteliteFrequencyId",
                table: "Chanells");

            migrationBuilder.DropForeignKey(
                name: "FK_Sources_SatteliteFrequencies_FrequencyId",
                table: "Sources");

            migrationBuilder.DropIndex(
                name: "IX_Chanells_SatteliteFrequencyId",
                table: "Chanells");

            migrationBuilder.DropColumn(
                name: "SatteliteFrequencyId",
                table: "Chanells");

            migrationBuilder.RenameColumn(
                name: "FrequencyId",
                table: "Sources",
                newName: "SatteliteId");

            migrationBuilder.RenameIndex(
                name: "IX_Sources_FrequencyId",
                table: "Sources",
                newName: "IX_Sources_SatteliteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sources_SatteliteFrequencies_SatteliteId",
                table: "Sources",
                column: "SatteliteId",
                principalTable: "SatteliteFrequencies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sources_SatteliteFrequencies_SatteliteId",
                table: "Sources");

            migrationBuilder.RenameColumn(
                name: "SatteliteId",
                table: "Sources",
                newName: "FrequencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Sources_SatteliteId",
                table: "Sources",
                newName: "IX_Sources_FrequencyId");

            migrationBuilder.AddColumn<int>(
                name: "SatteliteFrequencyId",
                table: "Chanells",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chanells_SatteliteFrequencyId",
                table: "Chanells",
                column: "SatteliteFrequencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chanells_SatteliteFrequencies_SatteliteFrequencyId",
                table: "Chanells",
                column: "SatteliteFrequencyId",
                principalTable: "SatteliteFrequencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sources_SatteliteFrequencies_FrequencyId",
                table: "Sources",
                column: "FrequencyId",
                principalTable: "SatteliteFrequencies",
                principalColumn: "Id");
        }
    }
}
