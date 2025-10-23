using Microsoft.AspNetCore.Identity;
using SkyNetApi.Utilidades;

namespace SkyNetApi.Servicios
{
    public class InicializadorRoles
    {
        public static async Task Inicializar(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var rol in Roles.TodosLosRoles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }
        }
    }
}

