using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condubello.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GeocodificacionYPuntosGps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "geocodificacion_cache",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clave = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    resultado = table.Column<string>(type: "jsonb", nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_geocodificacion_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "puntos_gps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sesion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    latitud = table.Column<double>(type: "double precision", nullable: false),
                    longitud = table.Column<double>(type: "double precision", nullable: false),
                    marca = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    velocidad_kmh = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_puntos_gps", x => x.id);
                    table.ForeignKey(
                        name: "fk_puntos_gps_sesiones_practicas_sesion_id",
                        column: x => x.sesion_id,
                        principalTable: "sesiones_practicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_geocodificacion_cache_clave",
                table: "geocodificacion_cache",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_puntos_gps_sesion_id_marca",
                table: "puntos_gps",
                columns: new[] { "sesion_id", "marca" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "geocodificacion_cache");

            migrationBuilder.DropTable(
                name: "puntos_gps");
        }
    }
}
