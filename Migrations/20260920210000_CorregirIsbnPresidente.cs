using BibliotecaMVC.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BibliotecaMVC.Migrations;

[DbContext(typeof(BibliotecaContext))]
[Migration("20260920210000_CorregirIsbnPresidente")]
public class CorregirIsbnPresidente : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        "UPDATE Libros SET ISBN = N'978-8420676630' WHERE ID = 4 AND Titulo = N'El señor Presidente' AND ISBN = N'978-8420674209';");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        "UPDATE Libros SET ISBN = N'978-8420674209' WHERE ID = 4 AND Titulo = N'El señor Presidente' AND ISBN = N'978-8420676630';");
}
