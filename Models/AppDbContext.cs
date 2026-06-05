using Microsoft.EntityFrameworkCore;

namespace TrainingsDashboard.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Areas> Areas { get; set; }
        public DbSet<Empleados> Empleados { get; set; }
        public DbSet<Entrenadores> Entrenadores { get; set; }
        public DbSet<Turnos> Turnos { get; set; }
        public DbSet<Entrenamientos> Entrenamientos { get; set; }
        public DbSet<EntrenamientosProgramados> EntrenamientosProgramados { get; set; }
        public DbSet<Supervisores> Supervisores { get; set; }

        public DbSet<Usuarios> Usuarios { get; set; }

    }
}
