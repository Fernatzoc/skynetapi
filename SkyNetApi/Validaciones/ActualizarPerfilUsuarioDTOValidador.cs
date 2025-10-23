using FluentValidation;
using SkyNetApi.DTOs;

namespace SkyNetApi.Validaciones
{
    public class ActualizarPerfilUsuarioDTOValidador : AbstractValidator<ActualizarPerfilUsuarioDTO>
    {
        public ActualizarPerfilUsuarioDTOValidador()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(45).WithMessage("El nombre no puede exceder 45 caracteres");

            RuleFor(x => x.MiddleName)
                .MaximumLength(45).WithMessage("El segundo nombre no puede exceder 45 caracteres")
                .When(x => !string.IsNullOrEmpty(x.MiddleName));

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido paterno es requerido")
                .MaximumLength(45).WithMessage("El apellido paterno no puede exceder 45 caracteres");

            RuleFor(x => x.SecondSurname)
                .MaximumLength(45).WithMessage("El apellido materno no puede exceder 45 caracteres")
                .When(x => !string.IsNullOrEmpty(x.SecondSurname));

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .Matches(@"^\d{8}$").WithMessage("El teléfono debe tener 8 dígitos");
        }
    }
}

