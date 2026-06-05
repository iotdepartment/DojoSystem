namespace TrainingsDashboard.Models
{
    public class Entrenamientos
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? AreaID { get; set; }
        public int? EntrenadorID { get; set; }
        public int? Limite { get; set; }

    }
}
