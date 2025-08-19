using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Gym.Migrations
{
    /// <inheritdoc />
    public partial class FithMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Membresia_AspNetUsers_ClienteId1",
                table: "Membresia");

            migrationBuilder.DropIndex(
                name: "IX_Membresia_ClienteId1",
                table: "Membresia");

            migrationBuilder.DropColumn(
                name: "ClienteId1",
                table: "Membresia");

            migrationBuilder.AlterColumn<string>(
                name: "ClienteId",
                table: "Membresia",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Membresia_ClienteId",
                table: "Membresia",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Membresia_AspNetUsers_ClienteId",
                table: "Membresia",
                column: "ClienteId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Membresia_AspNetUsers_ClienteId",
                table: "Membresia");

            migrationBuilder.DropIndex(
                name: "IX_Membresia_ClienteId",
                table: "Membresia");

            migrationBuilder.AlterColumn<int>(
                name: "ClienteId",
                table: "Membresia",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ClienteId1",
                table: "Membresia",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Membresia_ClienteId1",
                table: "Membresia",
                column: "ClienteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Membresia_AspNetUsers_ClienteId1",
                table: "Membresia",
                column: "ClienteId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
