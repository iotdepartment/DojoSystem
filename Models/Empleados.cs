namespace TrainingsDashboard.Models
{
    public class Empleados
    {
        public int ID { get; set; }
        public string? NombreEmpleado { get; set; }
        public int? NumeroEmpleado { get; set; }

        public int? AreaID { get; set; }

        public Areas? Area { get; set; }
    }
}
