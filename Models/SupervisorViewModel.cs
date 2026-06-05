namespace TrainingsDashboard.Models
{
    public class SupervisorViewModel
    {
        public int SupervisorID { get; set; }
        public int EmpleadoID { get; set; }
        public string? NombreEmpleado { get; set; }
        public int? NumeroEmpleado { get; set; }
        public string? AreaNombre { get; set; }
        public string? TurnoNombre { get; set; }
        public int? SupervisorPadreID { get; set; }
        public string? SupervisorNombre { get; set; }
        public string? Foto { get; set; }
    }
}
