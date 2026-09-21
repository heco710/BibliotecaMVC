using BibliotecaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Data;

public class BibliotecaContext(DbContextOptions<BibliotecaContext> options) : DbContext(options)
{
    public DbSet<Autor> Autores { get; set; }
    public DbSet<Libro> Libros { get; set; }
}
