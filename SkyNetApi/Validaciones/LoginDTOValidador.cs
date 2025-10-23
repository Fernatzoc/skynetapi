using FluentValidation;
using SkyNetApi.DTOs;

namespace SkyNetApi.Validaciones
{
    public class LoginDTOValidador : AbstractValidator<LoginDTO>
    {
        public LoginDTOValidador()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .MaximumLength(256)
                .EmailAddress().WithMessage("El email no es válido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida");
        }
    }
}
namespace SkyNetApi.DTOs
{
    public class LoginDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}

