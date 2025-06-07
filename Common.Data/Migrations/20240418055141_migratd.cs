using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jandag.DLL.Migrations
{
    /// <inheritdoc />
    public partial class migratd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sources_Recievers_Reciever_Id",
                table: "Sources");

            migrationBuilder.DropTable(
                name: "Recievers");

            migrationBuilder.DropIndex(
                name: "IX_Sources_Reciever_Id",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "Reciever_Id",
                table: "Sources");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Reciever_Id",
                table: "Sources",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Recievers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Card_In_Reciever = table.Column<int>(type: "int", nullable: false),
                    Emr_Number = table.Column<int>(type: "int", nullable: false),
                    Frequency_Stream = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Is_From_Optic = table.Column<bool>(type: "bit", nullable: false),
                    Port_In_Reciever = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recievers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sources_Reciever_Id",
                table: "Sources",
                column: "Reciever_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sources_Recievers_Reciever_Id",
                table: "Sources",
                column: "Reciever_Id",
                principalTable: "Recievers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
