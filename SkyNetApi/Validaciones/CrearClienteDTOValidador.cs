using FluentValidation;
using SkyNetApi.DTOs;

namespace SkyNetApi.Validaciones
{
    public class CrearClienteDTOValidador: AbstractValidator<CrearClienteDTO>
    {
        public CrearClienteDTOValidador()
        {
            RuleFor(x => x.PrimerNombre).NotEmpty().WithMessage("El {PropertyName} es requerido")
            .MaximumLength(50).WithMessage("El {PropertyName} no debe exceder los {MaxLength} caracteres");
        }
    }
}
