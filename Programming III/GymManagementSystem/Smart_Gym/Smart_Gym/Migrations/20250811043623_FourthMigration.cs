using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Gym.Migrations
{
    /// <inheritdoc />
    public partial class FourthMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Membresia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaExpiracion = table.Column<DateOnly>(type: "date", nullable: false),
                    EstaPagada = table.Column<bool>(type: "bit", nullable: false),
                    ClienteId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Membresia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Membresia_AspNetUsers_ClienteId1",
                        column: x => x.ClienteId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Factura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    MembresiaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Factura_Membresia_MembresiaId",
                        column: x => x.MembresiaId,
                        principalTable: "Membresia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaPago = table.Column<DateOnly>(type: "date", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CodigoTransaccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MembresiaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pago_Membresia_MembresiaId",
                        column: x => x.MembresiaId,
                        principalTable: "Membresia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Factura_MembresiaId",
                table: "Factura",
                column: "MembresiaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Membresia_ClienteId1",
                table: "Membresia",
                column: "ClienteId1");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_MembresiaId",
                table: "Pago",
                column: "MembresiaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Factura");

            migrationBuilder.DropTable(
                name: "Pago");

            migrationBuilder.DropTable(
                name: "Membresia");
        }
    }
}
