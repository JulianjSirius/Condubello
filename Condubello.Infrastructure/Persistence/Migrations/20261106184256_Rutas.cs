using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condubello.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Rutas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ruta_id",
                table: "sesiones_practicas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "rutas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    geo_json = table.Column<string>(type: "jsonb", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rutas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sesiones_practicas_ruta_id",
                table: "sesiones_practicas",
                column: "ruta_id");

            migrationBuilder.CreateIndex(
                name: "ix_rutas_nombre",
                table: "rutas",
                column: "nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_sesiones_practicas_rutas_ruta_id",
                table: "sesiones_practicas",
                column: "ruta_id",
                principalTable: "rutas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sesiones_practicas_rutas_ruta_id",
                table: "sesiones_practicas");

            migrationBuilder.DropTable(
                name: "rutas");

            migrationBuilder.DropIndex(
                name: "ix_sesiones_practicas_ruta_id",
                table: "sesiones_practicas");

            migrationBuilder.DropColumn(
                name: "ruta_id",
                table: "sesiones_practicas");
        }
    }
}
