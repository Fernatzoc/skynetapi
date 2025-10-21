using FluentValidation;
using SkyNetApi.DTOs;

namespace SkyNetApi.Validaciones
{
    public class CredencialesUsuarioDTOValidador: AbstractValidator<CredencialesUsuarioDTO>
    {
        public CredencialesUsuarioDTOValidador()
        {
            RuleFor(x => x.Email).NotEmpty()
                .MaximumLength(256)
                .EmailAddress();


            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
