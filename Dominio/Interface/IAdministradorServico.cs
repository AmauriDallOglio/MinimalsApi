using MinimalsApi.Dominio.DTOs;
using MinimalsApi.Dominio.Entidades;

namespace MinimalsApi.Dominio.Interface
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);
        Administrador Incluir(Administrador administrador);
        Administrador? BuscaPorId(int id);
        List<Administrador> Todos(int? pagina);
    }
}
