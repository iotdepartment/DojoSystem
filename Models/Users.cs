namespace TrainingsDashboard.Models
{
    public class Users
    {
        public int ID { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Role { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public string? Area { get; set; }
    }
}
