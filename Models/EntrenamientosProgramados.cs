namespace TrainingsDashboard.Models
{
    public class EntrenamientosProgramados
    {
        public int ID { get; set; }
        public int? EntrenamientoID { get; set; }
        public int? NumeroEmpleadoID { get; set; }
        public int? AreaID { get; set; }
        public int? Status { get; set; }
        public DateTime? FechaProgramacion { get; set; }
        public DateTime? FechaAsistencia { get; set; }
        public int? SupervisorID { get; set; }
        public int? TurnoID { get; set; }
    }
}
