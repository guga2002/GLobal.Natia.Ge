using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jandag.DLL.Migrations
{
    /// <inheritdoc />
    public partial class migrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FrequencyId",
                table: "Sources",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SatteliteFrequencyId",
                table: "Chanells",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SatteliteFrequencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    SymbolRate = table.Column<int>(type: "int", nullable: false),
                    Polarisation = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    PortIn250 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatteliteFrequencies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sources_FrequencyId",
                table: "Sources",
                column: "FrequencyId");

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
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chanells_SatteliteFrequencies_SatteliteFrequencyId",
                table: "Chanells");

            migrationBuilder.DropForeignKey(
                name: "FK_Sources_SatteliteFrequencies_FrequencyId",
                table: "Sources");

            migrationBuilder.DropTable(
                name: "SatteliteFrequencies");

            migrationBuilder.DropIndex(
                name: "IX_Sources_FrequencyId",
                table: "Sources");

            migrationBuilder.DropIndex(
                name: "IX_Chanells_SatteliteFrequencyId",
                table: "Chanells");

            migrationBuilder.DropColumn(
                name: "FrequencyId",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "SatteliteFrequencyId",
                table: "Chanells");
        }
    }
}
