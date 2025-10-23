using FluentValidation;
using SkyNetApi.DTOs;
using SkyNetApi.Utilidades;

namespace SkyNetApi.Validaciones
{
    public class AsignarRolDTOValidador : AbstractValidator<AsignarRolDTO>
    {
        public AsignarRolDTOValidador()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("Email no válido");

            RuleFor(x => x.Rol)
                .NotEmpty().WithMessage("El rol es requerido")
                .Must(rol => Roles.TodosLosRoles.Contains(rol))
                .WithMessage($"El rol debe ser uno de: {string.Join(", ", Roles.TodosLosRoles)}");
        }
    }
}
namespace SkyNetApi.DTOs
{
    public class AsignarRolDTO
    {
        public string Email { get; set; } = null!;
        public string Rol { get; set; } = null!;
    }
}

