using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Repositories;

public sealed class RepositorioLibrosEf(BibliotecaContext context) : IRepositorioLibro
{
    public async Task<IReadOnlyList<Libro>> ObtenerTodosAsync() =>
        await context.Libros.AsNoTracking().OrderBy(item => item.Titulo).ToListAsync();

    public async Task<Libro?> ObtenerPorIdAsync(int id) => await context.Libros.FindAsync(id);

    public Task<bool> ExisteIsbnAsync(string isbn, int excluirId) =>
        context.Libros.AnyAsync(item => item.ISBN == isbn && item.ID != excluirId);

    public async Task<int> AgregarAsync(Libro libro)
    {
        context.Libros.Add(libro);
        await context.SaveChangesAsync();
        return libro.ID;
    }

    public async Task<bool> ActualizarAsync(Libro libro)
    {
        var existente = await context.Libros.FindAsync(libro.ID);
        if (existente is null) return false;
        context.Entry(existente).CurrentValues.SetValues(libro);
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return false; }
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var libro = await context.Libros.FindAsync(id);
        if (libro is null) return false;
        context.Libros.Remove(libro);
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return false; }
        return true;
    }
}
