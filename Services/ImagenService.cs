using BibliotecaMVC.Services.Interfaces;

namespace BibliotecaMVC.Services;

public sealed class ImagenService(IWebHostEnvironment entorno) : IImagenService
{
    private const string Carpeta = "libros";
    private const long TamanoMaximo = 2 * 1024 * 1024;
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47];
    private static readonly byte[] Jpg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Riff = "RIFF"u8.ToArray();
    private static readonly byte[] Webp = "WEBP"u8.ToArray();

    public string? Validar(IFormFile archivo)
    {
        if (archivo.Length == 0) return "El archivo de imagen está vacío.";
        if (archivo.Length > TamanoMaximo) return "La imagen no debe superar 2 MB.";

        // Se comprueban extensión, tipo declarado y firma real del archivo.
        var cabecera = new byte[12];
        using (var lectura = archivo.OpenReadStream()) lectura.ReadAtLeast(cabecera, cabecera.Length, throwOnEndOfStream: false);
        var valido = Path.GetExtension(archivo.FileName).ToLowerInvariant() switch
        {
            ".png" => cabecera.AsSpan().StartsWith(Png),
            ".jpg" or ".jpeg" => cabecera.AsSpan().StartsWith(Jpg),
            ".webp" => cabecera.AsSpan().StartsWith(Riff) && cabecera.AsSpan(8).StartsWith(Webp),
            _ => false
        };
        return valido && archivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            ? null
            : "Formato no permitido. Usa una imagen JPG, PNG o WEBP.";
    }

    public async Task<string> GuardarAsync(IFormFile archivo)
    {
        // El nombre lo genera el servidor; nunca se usa el nombre enviado por el usuario.
        var nombre = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName).ToLowerInvariant()}";
        var carpeta = Path.Combine(entorno.WebRootPath, "images", Carpeta);
        Directory.CreateDirectory(carpeta);

        await using var destino = new FileStream(Path.Combine(carpeta, nombre), FileMode.CreateNew);
        await archivo.CopyToAsync(destino);
        return $"{Carpeta}/{nombre}";
    }

    public void Eliminar(string? ruta)
    {
        if (ruta is null || !ruta.StartsWith(Carpeta + "/")) return;
        File.Delete(Path.Combine(entorno.WebRootPath, "images", Carpeta, Path.GetFileName(ruta)));
    }
}
