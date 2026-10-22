namespace BibliotecaMVC.Models;

// Niveles de acceso: Usuario consulta; Bibliotecario agrega, edita y elimina libros y categorías, y agrega y edita autores;
// Admin puede todo, incluidos eliminar autores y asignar roles.
public static class Roles
{
    public const string Admin = "Admin";
    public const string Bibliotecario = "Bibliotecario";
    public const string Usuario = "Usuario";

    // Valores para [Authorize(Roles = ...)]: una coma significa "cualquiera de estos roles".
    public const string Gestores = Admin + "," + Bibliotecario;
    public const string Todos = Gestores + "," + Usuario;

    public static readonly string[] Lista = [Admin, Bibliotecario, Usuario];

    // Nombres usados hasta la semana 12; al iniciar se renombran conservando las asignaciones.
    public static readonly (string Anterior, string Actual)[] Renombrados = [("Administrador", Admin), ("Lector", Usuario)];

    public static string Descripcion(string rol) => rol switch
    {
        Admin => "Consulta, agrega, edita y elimina libros, autores y categorías; asigna roles.",
        Bibliotecario => "Consulta, agrega, edita y elimina libros y categorías; agrega y edita autores.",
        Usuario => "Consulta libros, autores y categorías.",
        _ => "Sin permisos asignados."
    };
}
