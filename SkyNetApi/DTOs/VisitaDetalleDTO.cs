namespace SkyNetApi.DTOs
{
    public class VisitaDetalleDTO
    {
        public VisitaDTO Visita { get; set; } = null!;
        public ClienteDetalleDTO Cliente { get; set; } = null!;
    }

    public class ClienteDetalleDTO
    {
        public int Id { get; set; }
        public string PrimerNombre { get; set; } = null!;
        public string? SegundoNombre { get; set; }
        public string? TercerNombre { get; set; }
        public string PrimerApellido { get; set; } = null!;
        public string? SegundoApellido { get; set; }
        public string Telefono { get; set; } = null!;
        public string CorreoElectronico { get; set; } = null!;
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public string Direccion { get; set; } = null!;
        public bool Estado { get; set; }
    }
}

