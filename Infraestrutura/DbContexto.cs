using Microsoft.EntityFrameworkCore;
using MinimalsApi.Dominio.Entidades;

namespace MinimalsApi.Infraestrutura
{
    public class DbContexto : DbContext
    {

        public DbSet<Administrador> Administradores { get; set; } = default!;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var stringConexao = _configuracaoAppSettings.GetConnectionString("MySql")?.ToString();
                if (!string.IsNullOrEmpty(stringConexao))
                {
                    optionsBuilder.UseMySql(
                        stringConexao,
                        ServerVersion.AutoDetect(stringConexao)
                    );
                }
            }
        }

    }
}
