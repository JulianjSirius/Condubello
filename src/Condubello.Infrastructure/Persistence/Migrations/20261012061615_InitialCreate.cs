using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condubello.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "administrativos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cedula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrativos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estudiantes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cedula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estudiantes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "instructores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cedula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instructores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehiculos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    placa = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estado_actual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehiculos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "control_horas_estudiante",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estudiante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_licencia = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    horas_requeridas = table.Column<int>(type: "integer", nullable: false),
                    horas_realizadas = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_control_horas_estudiante", x => x.id);
                    table.ForeignKey(
                        name: "FK_control_horas_estudiante_estudiantes_estudiante_id",
                        column: x => x.estudiante_id,
                        principalTable: "estudiantes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sesiones_practicas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estudiante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instructor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_hora_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_hora_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duracion_horas = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    estado_prueba = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    calificacion_instructor = table.Column<int>(type: "integer", nullable: true),
                    calificacion_alumno_a_instructor = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesiones_practicas", x => x.id);
                    table.ForeignKey(
                        name: "FK_sesiones_practicas_estudiantes_estudiante_id",
                        column: x => x.estudiante_id,
                        principalTable: "estudiantes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sesiones_practicas_instructores_instructor_id",
                        column: x => x.instructor_id,
                        principalTable: "instructores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "telemetria_pruebas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sesion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resumen_telemetria = table.Column<string>(type: "jsonb", nullable: true),
                    ruta_gps = table.Column<string>(type: "jsonb", nullable: true),
                    alerta_desviacion = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telemetria_pruebas", x => x.id);
                    table.ForeignKey(
                        name: "FK_telemetria_pruebas_sesiones_practicas_sesion_id",
                        column: x => x.sesion_id,
                        principalTable: "sesiones_practicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "validaciones_antifraude",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sesion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    url_foto_capturada = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    estado_validacion = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validaciones_antifraude", x => x.id);
                    table.ForeignKey(
                        name: "FK_validaciones_antifraude_sesiones_practicas_sesion_id",
                        column: x => x.sesion_id,
                        principalTable: "sesiones_practicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_administrativos_cedula",
                table: "administrativos",
                column: "cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_administrativos_email",
                table: "administrativos",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_control_horas_estudiante_estudiante_id_tipo_licencia",
                table: "control_horas_estudiante",
                columns: new[] { "estudiante_id", "tipo_licencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estudiantes_cedula",
                table: "estudiantes",
                column: "cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estudiantes_email",
                table: "estudiantes",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_instructores_cedula",
                table: "instructores",
                column: "cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_instructores_email",
                table: "instructores",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_practicas_estudiante_id",
                table: "sesiones_practicas",
                column: "estudiante_id");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_practicas_instructor_id",
                table: "sesiones_practicas",
                column: "instructor_id");

            migrationBuilder.CreateIndex(
                name: "IX_telemetria_pruebas_sesion_id",
                table: "telemetria_pruebas",
                column: "sesion_id");

            migrationBuilder.CreateIndex(
                name: "IX_validaciones_antifraude_sesion_id",
                table: "validaciones_antifraude",
                column: "sesion_id");

            migrationBuilder.CreateIndex(
                name: "IX_vehiculos_placa",
                table: "vehiculos",
                column: "placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administrativos");

            migrationBuilder.DropTable(
                name: "control_horas_estudiante");

            migrationBuilder.DropTable(
                name: "telemetria_pruebas");

            migrationBuilder.DropTable(
                name: "validaciones_antifraude");

            migrationBuilder.DropTable(
                name: "vehiculos");

            migrationBuilder.DropTable(
                name: "sesiones_practicas");

            migrationBuilder.DropTable(
                name: "estudiantes");

            migrationBuilder.DropTable(
                name: "instructores");
        }
    }
}
