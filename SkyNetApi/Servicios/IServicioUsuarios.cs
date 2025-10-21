using Microsoft.AspNetCore.Identity;

namespace SkyNetApi.Servicios
{
    public interface IServicioUsuarios
    {
        Task<IdentityUser?> ObtenerUsuario();
    }
}