namespace BibliotecaMVC.Models;

// Niveles de acceso del módulo Libros:
// Lector consulta; Bibliotecario además agrega y edita; Administrador además elimina y asigna roles.
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Bibliotecario = "Bibliotecario";
    public const string Lector = "Lector";

    // Valores para [Authorize(Roles = ...)]: una coma significa "cualquiera de estos roles".
    public const string Gestores = Administrador + "," + Bibliotecario;
    public const string Todos = Gestores + "," + Lector;

    public static readonly string[] Lista = [Administrador, Bibliotecario, Lector];

    public static string Descripcion(string rol) => rol switch
    {
        Administrador => "Consulta, agrega, edita y elimina libros; asigna roles.",
        Bibliotecario => "Consulta, agrega y edita libros.",
        Lector => "Consulta el catálogo de libros.",
        _ => "Sin permisos sobre el módulo Libros."
    };
}
