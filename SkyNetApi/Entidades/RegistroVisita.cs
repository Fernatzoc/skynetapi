namespace SkyNetApi.Entidades
{
    public class RegistroVisita
    {
        public int IdRegistroVisita { get; set; }
        public int IdVisita { get; set; }
        public DateTime FechaHoraInicioReal { get; set; }
        public DateTime FechaHoraFinReal { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
}

