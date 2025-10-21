using SkyNetApi.Entidades;

namespace SkyNetApi.Repositorios
{
    public interface IRepositorioClientes
    {
        Task<int> crearCliente(Cliente cliente);
        Task<Cliente?> ObtenerCliente(int idCliente);
        Task<List<Cliente>> ObtenerClientes();
        Task<bool> Existe(int idCliente);
        Task Actualizar(Cliente cliente);
        Task Eliminar(int idCliente);
    }
}