using FluentValidation;
using SkyNetApi.DTOs;

namespace SkyNetApi.Validaciones
{
    public class RegistrarVisitaTecnicoDTOValidador : AbstractValidator<RegistrarVisitaTecnicoDTO>
    {
        public RegistrarVisitaTecnicoDTOValidador()
        {
            RuleFor(x => x.FechaHoraInicioReal)
                .NotEmpty().WithMessage("La fecha y hora de inicio es requerida");

            RuleFor(x => x.FechaHoraFinReal)
                .NotEmpty().WithMessage("La fecha y hora de fin es requerida")
                .GreaterThanOrEqualTo(x => x.FechaHoraInicioReal)
                .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio");

            RuleFor(x => x.Observaciones)
                .NotEmpty().WithMessage("Las observaciones son requeridas")
                .MaximumLength(1000).WithMessage("Las observaciones no pueden exceder los 1000 caracteres");
        }
    }
}

