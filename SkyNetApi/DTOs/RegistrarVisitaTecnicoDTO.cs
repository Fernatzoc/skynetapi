namespace SkyNetApi.DTOs
{
    public class RegistrarVisitaTecnicoDTO
    {
        public DateTime FechaHoraInicioReal { get; set; }
        public DateTime FechaHoraFinReal { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
}

