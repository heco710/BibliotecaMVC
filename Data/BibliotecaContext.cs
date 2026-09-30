using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Data;

public class BibliotecaContext(DbContextOptions<BibliotecaContext> options) : IdentityUserContext<IdentityUser>(options)
{
    public DbSet<Autor> Autores { get; set; }
    public DbSet<Libro> Libros { get; set; }
}
