using BMPTec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace BMPTec.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IConfiguration configuration) 
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Conta> Conta { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Transferencia> Transferencia { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Para desenvolvimento local
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
            modelBuilder.Entity<Conta>()
                .HasKey(c => c.Id);
            
            modelBuilder.Entity<Conta>()
                .HasIndex(c => c.NumeroConta)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasKey(c => c.Id);
                
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.CPF)
                .IsUnique();
                
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();
                
            modelBuilder.Entity<Conta>()
                .HasOne(c => c.Cliente)
                .WithMany(c => c.Contas)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Transferencia>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Transferencia>()
                .HasOne(t => t.ContaDestino)
                .WithMany() // Não há coleção de transferências recebidas
                .HasForeignKey(t => t.ContaDestinoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}