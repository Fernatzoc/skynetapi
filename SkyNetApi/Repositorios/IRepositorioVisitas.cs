using SkyNetApi.Entidades;

namespace SkyNetApi.Repositorios
{
    public interface IRepositorioVisitas
    {
        Task<int> Crear(Visita visita);
        Task<Visita?> ObtenerPorId(int idVisita);
        Task<List<Visita>> ObtenerTodas();
        Task<List<Visita>> ObtenerPorCliente(int idCliente);
        Task<List<Visita>> ObtenerPorTecnico(string idTecnico);
        Task<List<Visita>> ObtenerPorSupervisor(string idSupervisor);
        Task<bool> Existe(int idVisita);
        Task Actualizar(Visita visita);
        Task ActualizarEstado(int idVisita, int idEstadoVisita);
        Task<int> CrearRegistroVisita(RegistroVisita registroVisita);
        Task<RegistroVisita?> ObtenerRegistroVisita(int idVisita);
        Task Eliminar(int idVisita);
    }
}

