namespace BibliotecaMVC.Services.Interfaces;

public interface IImagenService
{
    /// <summary>Devuelve un mensaje de error si el archivo no es una imagen aceptada; null si es válido.</summary>
    string? Validar(IFormFile archivo);

    /// <summary>Guarda la imagen y devuelve su ruta relativa a wwwroot/images.</summary>
    Task<string> GuardarAsync(IFormFile archivo);

    /// <summary>Borra una imagen subida (libros/...); las portadas del catálogo inicial no se tocan.</summary>
    void Eliminar(string? ruta);
}
