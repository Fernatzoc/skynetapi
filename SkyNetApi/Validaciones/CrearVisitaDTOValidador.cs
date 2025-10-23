using FluentValidation;
using SkyNetApi.DTOs;

namespace SkyNetApi.Validaciones
{
    public class CrearVisitaDTOValidador : AbstractValidator<CrearVisitaDTO>
    {
        public CrearVisitaDTOValidador()
        {
            RuleFor(x => x.IdCliente)
                .GreaterThan(0).WithMessage("El cliente es requerido");

            RuleFor(x => x.IdTecnico)
                .NotEmpty().WithMessage("El técnico es requerido")
                .MaximumLength(255).WithMessage("El ID del técnico no puede exceder los 255 caracteres");

            RuleFor(x => x.IdSupervisor)
                .NotEmpty().WithMessage("El supervisor es requerido")
                .MaximumLength(255).WithMessage("El ID del supervisor no puede exceder los 255 caracteres");

            RuleFor(x => x.FechaHoraProgramada)
                .NotEmpty().WithMessage("La fecha y hora programada es requerida")
                .GreaterThanOrEqualTo(DateTime.Now.Date).WithMessage("La fecha programada no puede ser en el pasado");

            RuleFor(x => x.IdEstadoVisita)
                .GreaterThan(0).WithMessage("El estado de la visita es requerido");

            RuleFor(x => x.IdTipoVisita)
                .GreaterThan(0).WithMessage("El tipo de visita es requerido");

            RuleFor(x => x.Descripcion)
                .MaximumLength(300).WithMessage("La descripción no puede exceder los 300 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Descripcion));
        }
    }
}

