namespace BibliotecaMVC.Models
{
    public class Autor
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Nacionalidad { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public bool Activo { get; set; }
    }
}
