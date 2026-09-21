using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Repositories;

public sealed class RepositorioAutoresEf(BibliotecaContext context) : IRepositorioAutor
{
    public async Task<IReadOnlyList<Autor>> ObtenerTodosAsync() =>
        await context.Autores.AsNoTracking().OrderBy(item => item.Apellido).ToListAsync();

    public async Task<Autor?> ObtenerPorIdAsync(int id) => await context.Autores.FindAsync(id);

    public async Task<int> AgregarAsync(Autor autor)
    {
        context.Autores.Add(autor);
        await context.SaveChangesAsync();
        return autor.ID;
    }

    public async Task<bool> ActualizarAsync(Autor autor)
    {
        if (!await context.Autores.AnyAsync(item => item.ID == autor.ID)) return false;
        context.Autores.Update(autor);
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return false; }
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var autor = await context.Autores.FindAsync(id);
        if (autor is null) return false;
        context.Autores.Remove(autor);
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return false; }
        return true;
    }
}
