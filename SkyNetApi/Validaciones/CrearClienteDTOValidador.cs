﻿﻿using FluentValidation;
using SkyNetApi.DTOs;

namespace SkyNetApi.Validaciones
{
    public class CrearClienteDTOValidador: AbstractValidator<CrearClienteDTO>
    {
        public CrearClienteDTOValidador()
        {
            RuleFor(x => x.PrimerNombre)
                .NotEmpty().WithMessage("El primer nombre es requerido")
                .MaximumLength(45).WithMessage("El primer nombre no debe exceder los 45 caracteres");

            RuleFor(x => x.SegundoNombre)
                .MaximumLength(45).WithMessage("El segundo nombre no debe exceder los 45 caracteres")
                .When(x => !string.IsNullOrEmpty(x.SegundoNombre));

            RuleFor(x => x.TercerNombre)
                .MaximumLength(45).WithMessage("El tercer nombre no debe exceder los 45 caracteres")
                .When(x => !string.IsNullOrEmpty(x.TercerNombre));

            RuleFor(x => x.PrimerApellido)
                .NotEmpty().WithMessage("El primer apellido es requerido")
                .MaximumLength(45).WithMessage("El primer apellido no debe exceder los 45 caracteres");

            RuleFor(x => x.SegundoApellido)
                .MaximumLength(45).WithMessage("El segundo apellido no debe exceder los 45 caracteres")
                .When(x => !string.IsNullOrEmpty(x.SegundoApellido));

            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .Matches(@"^\d{8}$").WithMessage("El teléfono debe tener exactamente 8 dígitos");

            RuleFor(x => x.CorreoElectronico)
                .NotEmpty().WithMessage("El correo electrónico es requerido")
                .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido")
                .MaximumLength(100).WithMessage("El correo electrónico no debe exceder los 100 caracteres");

            RuleFor(x => x.Latitud)
                .InclusiveBetween(-90, 90).WithMessage("La latitud debe estar entre -90 y 90");

            RuleFor(x => x.Longitud)
                .InclusiveBetween(-180, 180).WithMessage("La longitud debe estar entre -180 y 180");

            RuleFor(x => x.Direccion)
                .NotEmpty().WithMessage("La dirección es requerida")
                .MaximumLength(250).WithMessage("La dirección no debe exceder los 250 caracteres");
        }
    }
}
