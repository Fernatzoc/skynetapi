namespace SkyNetApi.DTOs
{
    public class RestablecerContraseniaDTO
    {
        public string Email { get; set; } = null!;
        public string NuevaContrasenia { get; set; } = null!;
        public string ConfirmarContrasenia { get; set; } = null!;
    }
}

