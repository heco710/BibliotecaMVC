using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaMVC.Migrations
{
    /// <inheritdoc />
    public partial class CrearTablasAutoresYLibros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Autores",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nacionalidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autores", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Libros",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Autor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    AnioPublicacion = table.Column<int>(type: "int", nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Imagen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Disponible = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libros", x => x.ID);
                });

            // Conservo una sola vez los registros del catálogo anterior.
            migrationBuilder.InsertData(
                table: "Autores",
                columns: new[] { "ID", "Nombre", "Apellido", "Nacionalidad", "FechaNacimiento", "Activo" },
                values: new object[,]
                {
                    { 1, "Gabriel", "García Márquez", "Colombiana", new DateTime(1927, 3, 6), false },
                    { 2, "Isabel", "Allende", "Chilena", new DateTime(1942, 8, 2), true },
                    { 3, "Jorge Luis", "Borges", "Argentina", new DateTime(1899, 8, 24), false },
                    { 4, "Laura", "Esquivel", "Mexicana", new DateTime(1950, 9, 30), true },
                    { 5, "Miguel Ángel", "Asturias", "Guatemalteca", new DateTime(1899, 10, 19), false }
                });

            migrationBuilder.InsertData(
                table: "Libros",
                columns: new[] { "ID", "Titulo", "Autor", "Categoria", "AnioPublicacion", "ISBN", "Descripcion", "Imagen", "Disponible" },
                values: new object[,]
                {
                    { 1, "Cien años de soledad", "Gabriel García Márquez", "Realismo mágico", 1967, "978-0307474728", "La historia de la familia Buendía y del inolvidable pueblo de Macondo.", "cien-anos-soledad.png", true },
                    { 2, "La casa de los espíritus", "Isabel Allende", "Narrativa", 1982, "978-0525433477", "Una saga familiar atravesada por la memoria, el amor y los cambios históricos.", "casa-espiritus.png", true },
                    { 3, "Ficciones", "Jorge Luis Borges", "Cuentos", 1944, "978-0802130303", "Laberintos, bibliotecas y mundos posibles en una colección esencial.", "ficciones.png", false },
                    { 4, "El señor Presidente", "Miguel Ángel Asturias", "Novela", 1946, "978-8420674209", "Una obra fundamental de la literatura guatemalteca sobre el poder y sus sombras.", "senor-presidente.png", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Autores");

            migrationBuilder.DropTable(
                name: "Libros");
        }
    }
}
