namespace TrainingsDashboard.Models
{
    public class Empleados
    {
        public int ID { get; set; }
        public string? NombreEmpleado { get; set; }
        public int? NumeroEmpleado { get; set; }
        public int? AreaID { get; set; }
        public int? SupervisorID { get; set; }
        public int? TurnoID { get; set; }
        public int? Activo { get; set; }
        public Areas? Area { get; set; }
    }
}
