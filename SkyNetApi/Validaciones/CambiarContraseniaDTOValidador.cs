using FluentValidation;
using SkyNetApi.DTOs;

namespace SkyNetApi.Validaciones
{
    public class CambiarContraseniaDTOValidador : AbstractValidator<CambiarContraseniaDTO>
    {
        public CambiarContraseniaDTOValidador()
        {
            RuleFor(x => x.ContraseniaActual)
                .NotEmpty().WithMessage("La contraseña actual es requerida");

            RuleFor(x => x.NuevaContrasenia)
                .NotEmpty().WithMessage("La nueva contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula")
                .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número")
                .Matches(@"[\W_]").WithMessage("La contraseña debe contener al menos un carácter especial");

            RuleFor(x => x.ConfirmarContrasenia)
                .NotEmpty().WithMessage("Debe confirmar la nueva contraseña")
                .Equal(x => x.NuevaContrasenia).WithMessage("Las contraseñas no coinciden");
        }
    }
}

