namespace SkyNetApi.DTOs
{
    public class CambiarContraseniaDTO
    {
        public string ContraseniaActual { get; set; } = null!;
        public string NuevaContrasenia { get; set; } = null!;
        public string ConfirmarContrasenia { get; set; } = null!;
    }
}

