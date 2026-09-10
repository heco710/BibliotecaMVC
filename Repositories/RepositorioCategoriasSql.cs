using System.Data;
using BibliotecaMVC.Models;
using Microsoft.Data.SqlClient;

namespace BibliotecaMVC.Repositories;

public sealed class RepositorioCategoriasSql(IConfiguration configuration) : IRepositorioCategoria
{
    private SqlConnection CrearConexion()
    {
        var cadena = configuration.GetConnectionString("BibliotecaDB");
        if (string.IsNullOrWhiteSpace(cadena))
        {
            throw new InvalidOperationException("Falta configurar ConnectionStrings:BibliotecaDB.");
        }
        return new SqlConnection(cadena);
    }

    public async Task<IReadOnlyList<Categoria>> ObtenerTodosAsync()
    {
        var categorias = new List<Categoria>();
        using var conexion = CrearConexion();
        using var comando = new SqlCommand("SELECT ID, Nombre, Descripcion FROM dbo.Categorias ORDER BY Nombre, ID", conexion);
        await conexion.OpenAsync();
        using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
        {
            categorias.Add(Leer(lector));
        }
        return categorias;
    }

    public async Task<Categoria?> ObtenerPorIdAsync(int id)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand("SELECT ID, Nombre, Descripcion FROM dbo.Categorias WHERE ID = @ID", conexion);
        comando.Parameters.Add("@ID", SqlDbType.Int).Value = id;
        await conexion.OpenAsync();
        using var lector = await comando.ExecuteReaderAsync();
        return await lector.ReadAsync() ? Leer(lector) : null;
    }

    public async Task<int> AgregarAsync(Categoria categoria)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand("INSERT INTO dbo.Categorias (Nombre, Descripcion) OUTPUT INSERTED.ID VALUES (@Nombre, @Descripcion)", conexion);
        AgregarParametros(comando, categoria);
        await conexion.OpenAsync();
        return (int)(await comando.ExecuteScalarAsync())!;
    }

    public async Task<bool> ActualizarAsync(Categoria categoria)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand("UPDATE dbo.Categorias SET Nombre = @Nombre, Descripcion = @Descripcion WHERE ID = @ID", conexion);
        AgregarParametros(comando, categoria);
        comando.Parameters.Add("@ID", SqlDbType.Int).Value = categoria.ID;
        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand("DELETE FROM dbo.Categorias WHERE ID = @ID", conexion);
        comando.Parameters.Add("@ID", SqlDbType.Int).Value = id;
        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() == 1;
    }

    private static Categoria Leer(SqlDataReader lector) => new()
    {
        ID = lector.GetInt32(0),
        Nombre = lector.GetString(1),
        Descripcion = lector.IsDBNull(2) ? null : lector.GetString(2)
    };

    private static void AgregarParametros(SqlCommand comando, Categoria categoria)
    {
        comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = categoria.Nombre;
        comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 250).Value = (object?)categoria.Descripcion ?? DBNull.Value;
    }
}
