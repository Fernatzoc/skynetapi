namespace SkyNetApi.DTOs
{
    public class VisitaDTO
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } = null!;
        public string IdTecnico { get; set; } = null!;
        public string NombreTecnico { get; set; } = null!;
        public string IdSupervisor { get; set; } = null!;
        public string NombreSupervisor { get; set; } = null!;
        public int IdEstadoVisita { get; set; }
        public string EstadoVisita { get; set; } = null!;
        public int IdTipoVisita { get; set; }
        public string TipoVisita { get; set; } = null!;
        public DateTime FechaHoraProgramada { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        
        // Información del registro (si existe)
        public int? IdRegistroVisita { get; set; }
        public DateTime? FechaHoraInicioReal { get; set; }
        public DateTime? FechaHoraFinReal { get; set; }
        public string? Observaciones { get; set; }
    }
}

