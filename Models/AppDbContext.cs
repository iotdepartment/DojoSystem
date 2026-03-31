using TrainingsDashboard.Models;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;

namespace TrainingsDashboard.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Areas> Areas { get; set; }
        public DbSet<Empleados> Empleados { get; set; }
        public DbSet<Entrenadores> Entrenadores { get; set; }

    }
}
