using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Repositories;

public sealed class RepositorioLibrosEf(BibliotecaContext context) : IRepositorioLibro
{
    public IReadOnlyList<Libro> ObtenerTodos() =>
        context.Libros.AsNoTracking().OrderBy(item => item.Titulo).ToList();

    public Libro? ObtenerPorId(int id) => context.Libros.Find(id);

    public int Agregar(Libro libro)
    {
        context.Libros.Add(libro);
        context.SaveChanges();
        return libro.ID;
    }

    public bool Actualizar(Libro libro)
    {
        var existente = context.Libros.Find(libro.ID);
        if (existente is null) return false;
        context.Entry(existente).CurrentValues.SetValues(libro);
        context.Libros.Update(existente);
        try { context.SaveChanges(); }
        catch (DbUpdateConcurrencyException) { return false; }
        return true;
    }

    public bool Eliminar(int id)
    {
        var libro = context.Libros.Find(id);
        if (libro is null) return false;
        context.Libros.Remove(libro);
        try { context.SaveChanges(); }
        catch (DbUpdateConcurrencyException) { return false; }
        return true;
    }
}
