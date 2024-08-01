using Microsoft.EntityFrameworkCore;
using MinimalsApi.Dominio.Entidades;

namespace MinimalsApi.Infraestrutura
{
    public class DbContexto : DbContext
    {
        private readonly IConfiguration _configurationAppSettings;
        public DbContexto(IConfiguration configurationAppSettings)
        {
            _configurationAppSettings = configurationAppSettings;
        }
        public DbSet<Administrador> Administrador { get; set; } = default!;
        public DbSet<Veiculo> Veiculo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrador>().HasData(
               new Administrador
               {
                   Id = 1,
                   Email = "amauri@amauri",
                   Senha = "123",
                   Perfil = "Adm"
               }
           );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {



            if (!optionsBuilder.IsConfigured)
            {
                var stringConexao = _configurationAppSettings.GetConnectionString("ConexaoPadrao")?.ToString();
                if (!string.IsNullOrEmpty(stringConexao))
                {
                    optionsBuilder.UseSqlServer(stringConexao);
                }

   
            }
        }

    }
}
