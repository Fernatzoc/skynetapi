using FluentValidation;
using SkyNetApi.DTOs;

namespace SkyNetApi.Validaciones
{
    public class ActualizarStatusUsuarioDTOValidador : AbstractValidator<ActualizarStatusUsuarioDTO>
    {
        public ActualizarStatusUsuarioDTOValidador()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("Email no válido");
        }
    }
}

