using Microsoft.EntityFrameworkCore;

namespace InconsistenciasApi.Models.Entities
{
    public class MyContextDatabase : DbContext
    {
        public MyContextDatabase()
        {
        }

        public MyContextDatabase(DbContextOptions<MyContextDatabase> options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Archivo> Archivo { get; set; }
        public DbSet<ReglasArchivo> ReglasArchivo { get; set; }
        public DbSet<ResultadoArchivo> ResultadoArchivo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Archivo>()
            .HasIndex(p => new { p.HashArchivo, p.Usuario })
            .HasDatabaseName("UQ_HashArchivo")
            .IsUnique(true);
        }
    }
}
