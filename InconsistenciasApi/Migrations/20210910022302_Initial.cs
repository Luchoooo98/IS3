using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InconsistenciasApi.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Archivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Alta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HashArchivo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ContenidoArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tamanio = table.Column<double>(type: "float", nullable: false),
                    Bytes = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archivo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReglasArchivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Regla = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArchivoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasArchivo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasArchivo_Archivo_ArchivoId",
                        column: x => x.ArchivoId,
                        principalTable: "Archivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResultadoArchivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reglas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoProcesamiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArchivoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadoArchivo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultadoArchivo_Archivo_ArchivoId",
                        column: x => x.ArchivoId,
                        principalTable: "Archivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_HashArchivo",
                table: "Archivo",
                columns: new[] { "HashArchivo", "Usuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReglasArchivo_ArchivoId",
                table: "ReglasArchivo",
                column: "ArchivoId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadoArchivo_ArchivoId",
                table: "ResultadoArchivo",
                column: "ArchivoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReglasArchivo");

            migrationBuilder.DropTable(
                name: "ResultadoArchivo");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Archivo");
        }
    }
}
