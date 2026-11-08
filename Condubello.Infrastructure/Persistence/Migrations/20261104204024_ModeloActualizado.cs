using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condubello.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModeloActualizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_control_horas_estudiante_estudiantes_estudiante_id",
                table: "control_horas_estudiante");

            migrationBuilder.DropForeignKey(
                name: "FK_sesiones_practicas_estudiantes_estudiante_id",
                table: "sesiones_practicas");

            migrationBuilder.DropForeignKey(
                name: "FK_sesiones_practicas_instructores_instructor_id",
                table: "sesiones_practicas");

            migrationBuilder.DropForeignKey(
                name: "FK_telemetria_pruebas_sesiones_practicas_sesion_id",
                table: "telemetria_pruebas");

            migrationBuilder.DropForeignKey(
                name: "FK_validaciones_antifraude_sesiones_practicas_sesion_id",
                table: "validaciones_antifraude");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehiculos",
                table: "vehiculos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_validaciones_antifraude",
                table: "validaciones_antifraude");

            migrationBuilder.DropPrimaryKey(
                name: "PK_telemetria_pruebas",
                table: "telemetria_pruebas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sesiones_practicas",
                table: "sesiones_practicas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_instructores",
                table: "instructores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estudiantes",
                table: "estudiantes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_control_horas_estudiante",
                table: "control_horas_estudiante");

            migrationBuilder.DropPrimaryKey(
                name: "PK_administrativos",
                table: "administrativos");

            migrationBuilder.RenameIndex(
                name: "IX_vehiculos_placa",
                table: "vehiculos",
                newName: "ix_vehiculos_placa");

            migrationBuilder.RenameIndex(
                name: "IX_validaciones_antifraude_sesion_id",
                table: "validaciones_antifraude",
                newName: "ix_validaciones_antifraude_sesion_id");

            migrationBuilder.RenameIndex(
                name: "IX_telemetria_pruebas_sesion_id",
                table: "telemetria_pruebas",
                newName: "ix_telemetria_pruebas_sesion_id");

            migrationBuilder.RenameIndex(
                name: "IX_sesiones_practicas_instructor_id",
                table: "sesiones_practicas",
                newName: "ix_sesiones_practicas_instructor_id");

            migrationBuilder.RenameIndex(
                name: "IX_sesiones_practicas_estudiante_id",
                table: "sesiones_practicas",
                newName: "ix_sesiones_practicas_estudiante_id");

            migrationBuilder.RenameIndex(
                name: "IX_instructores_email",
                table: "instructores",
                newName: "ix_instructores_email");

            migrationBuilder.RenameIndex(
                name: "IX_instructores_cedula",
                table: "instructores",
                newName: "ix_instructores_cedula");

            migrationBuilder.RenameIndex(
                name: "IX_estudiantes_email",
                table: "estudiantes",
                newName: "ix_estudiantes_email");

            migrationBuilder.RenameIndex(
                name: "IX_estudiantes_cedula",
                table: "estudiantes",
                newName: "ix_estudiantes_cedula");

            migrationBuilder.RenameIndex(
                name: "IX_control_horas_estudiante_estudiante_id_tipo_licencia",
                table: "control_horas_estudiante",
                newName: "ix_control_horas_estudiante_estudiante_id_tipo_licencia");

            migrationBuilder.RenameIndex(
                name: "IX_administrativos_email",
                table: "administrativos",
                newName: "ix_administrativos_email");

            migrationBuilder.RenameIndex(
                name: "IX_administrativos_cedula",
                table: "administrativos",
                newName: "ix_administrativos_cedula");

            migrationBuilder.AddPrimaryKey(
                name: "pk_vehiculos",
                table: "vehiculos",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_validaciones_antifraude",
                table: "validaciones_antifraude",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_telemetria_pruebas",
                table: "telemetria_pruebas",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sesiones_practicas",
                table: "sesiones_practicas",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_instructores",
                table: "instructores",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_estudiantes",
                table: "estudiantes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_control_horas_estudiante",
                table: "control_horas_estudiante",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_administrativos",
                table: "administrativos",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_control_horas_estudiante_estudiantes_estudiante_id",
                table: "control_horas_estudiante",
                column: "estudiante_id",
                principalTable: "estudiantes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sesiones_practicas_estudiantes_estudiante_id",
                table: "sesiones_practicas",
                column: "estudiante_id",
                principalTable: "estudiantes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_sesiones_practicas_instructores_instructor_id",
                table: "sesiones_practicas",
                column: "instructor_id",
                principalTable: "instructores",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_telemetria_pruebas_sesiones_practicas_sesion_id",
                table: "telemetria_pruebas",
                column: "sesion_id",
                principalTable: "sesiones_practicas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_validaciones_antifraude_sesiones_practicas_sesion_id",
                table: "validaciones_antifraude",
                column: "sesion_id",
                principalTable: "sesiones_practicas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_control_horas_estudiante_estudiantes_estudiante_id",
                table: "control_horas_estudiante");

            migrationBuilder.DropForeignKey(
                name: "fk_sesiones_practicas_estudiantes_estudiante_id",
                table: "sesiones_practicas");

            migrationBuilder.DropForeignKey(
                name: "fk_sesiones_practicas_instructores_instructor_id",
                table: "sesiones_practicas");

            migrationBuilder.DropForeignKey(
                name: "fk_telemetria_pruebas_sesiones_practicas_sesion_id",
                table: "telemetria_pruebas");

            migrationBuilder.DropForeignKey(
                name: "fk_validaciones_antifraude_sesiones_practicas_sesion_id",
                table: "validaciones_antifraude");

            migrationBuilder.DropPrimaryKey(
                name: "pk_vehiculos",
                table: "vehiculos");

            migrationBuilder.DropPrimaryKey(
                name: "pk_validaciones_antifraude",
                table: "validaciones_antifraude");

            migrationBuilder.DropPrimaryKey(
                name: "pk_telemetria_pruebas",
                table: "telemetria_pruebas");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sesiones_practicas",
                table: "sesiones_practicas");

            migrationBuilder.DropPrimaryKey(
                name: "pk_instructores",
                table: "instructores");

            migrationBuilder.DropPrimaryKey(
                name: "pk_estudiantes",
                table: "estudiantes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_control_horas_estudiante",
                table: "control_horas_estudiante");

            migrationBuilder.DropPrimaryKey(
                name: "pk_administrativos",
                table: "administrativos");

            migrationBuilder.RenameIndex(
                name: "ix_vehiculos_placa",
                table: "vehiculos",
                newName: "IX_vehiculos_placa");

            migrationBuilder.RenameIndex(
                name: "ix_validaciones_antifraude_sesion_id",
                table: "validaciones_antifraude",
                newName: "IX_validaciones_antifraude_sesion_id");

            migrationBuilder.RenameIndex(
                name: "ix_telemetria_pruebas_sesion_id",
                table: "telemetria_pruebas",
                newName: "IX_telemetria_pruebas_sesion_id");

            migrationBuilder.RenameIndex(
                name: "ix_sesiones_practicas_instructor_id",
                table: "sesiones_practicas",
                newName: "IX_sesiones_practicas_instructor_id");

            migrationBuilder.RenameIndex(
                name: "ix_sesiones_practicas_estudiante_id",
                table: "sesiones_practicas",
                newName: "IX_sesiones_practicas_estudiante_id");

            migrationBuilder.RenameIndex(
                name: "ix_instructores_email",
                table: "instructores",
                newName: "IX_instructores_email");

            migrationBuilder.RenameIndex(
                name: "ix_instructores_cedula",
                table: "instructores",
                newName: "IX_instructores_cedula");

            migrationBuilder.RenameIndex(
                name: "ix_estudiantes_email",
                table: "estudiantes",
                newName: "IX_estudiantes_email");

            migrationBuilder.RenameIndex(
                name: "ix_estudiantes_cedula",
                table: "estudiantes",
                newName: "IX_estudiantes_cedula");

            migrationBuilder.RenameIndex(
                name: "ix_control_horas_estudiante_estudiante_id_tipo_licencia",
                table: "control_horas_estudiante",
                newName: "IX_control_horas_estudiante_estudiante_id_tipo_licencia");

            migrationBuilder.RenameIndex(
                name: "ix_administrativos_email",
                table: "administrativos",
                newName: "IX_administrativos_email");

            migrationBuilder.RenameIndex(
                name: "ix_administrativos_cedula",
                table: "administrativos",
                newName: "IX_administrativos_cedula");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehiculos",
                table: "vehiculos",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_validaciones_antifraude",
                table: "validaciones_antifraude",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_telemetria_pruebas",
                table: "telemetria_pruebas",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sesiones_practicas",
                table: "sesiones_practicas",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_instructores",
                table: "instructores",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_estudiantes",
                table: "estudiantes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_control_horas_estudiante",
                table: "control_horas_estudiante",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_administrativos",
                table: "administrativos",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_control_horas_estudiante_estudiantes_estudiante_id",
                table: "control_horas_estudiante",
                column: "estudiante_id",
                principalTable: "estudiantes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sesiones_practicas_estudiantes_estudiante_id",
                table: "sesiones_practicas",
                column: "estudiante_id",
                principalTable: "estudiantes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sesiones_practicas_instructores_instructor_id",
                table: "sesiones_practicas",
                column: "instructor_id",
                principalTable: "instructores",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_telemetria_pruebas_sesiones_practicas_sesion_id",
                table: "telemetria_pruebas",
                column: "sesion_id",
                principalTable: "sesiones_practicas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_validaciones_antifraude_sesiones_practicas_sesion_id",
                table: "validaciones_antifraude",
                column: "sesion_id",
                principalTable: "sesiones_practicas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
