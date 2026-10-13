using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Data;

// IdentityDbContext incluye las tablas de roles (AspNetRoles, AspNetUserRoles, AspNetRoleClaims).
public class BibliotecaContext(DbContextOptions<BibliotecaContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Autor> Autores { get; set; }
    public DbSet<Libro> Libros { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Libro>().HasIndex(libro => libro.ISBN).IsUnique();
    }
}
