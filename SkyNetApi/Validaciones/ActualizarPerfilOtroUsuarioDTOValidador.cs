using FluentValidation;
using SkyNetApi.DTOs;

namespace SkyNetApi.Validaciones
{
    public class ActualizarPerfilOtroUsuarioDTOValidador : AbstractValidator<ActualizarPerfilOtroUsuarioDTO>
    {
        public ActualizarPerfilOtroUsuarioDTOValidador()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El formato del email no es válido");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El primer nombre es requerido")
                .MaximumLength(100).WithMessage("El primer nombre no puede exceder los 100 caracteres");

            RuleFor(x => x.MiddleName)
                .MaximumLength(100).WithMessage("El segundo nombre no puede exceder los 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.MiddleName));

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido paterno es requerido")
                .MaximumLength(100).WithMessage("El apellido paterno no puede exceder los 100 caracteres");

            RuleFor(x => x.SecondSurname)
                .MaximumLength(100).WithMessage("El apellido materno no puede exceder los 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.SecondSurname));

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .Matches(@"^\d{8}$").WithMessage("El teléfono debe tener 8 dígitos");
        }
    }
}

