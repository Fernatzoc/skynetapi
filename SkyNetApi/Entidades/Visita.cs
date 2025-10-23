namespace SkyNetApi.Entidades
{
    public class Visita
    {
        public int IdVisita { get; set; }
        public int IdCliente { get; set; }
        public string IdSupervisor { get; set; } = null!;
        public string IdTecnico { get; set; } = null!;
        public int IdEstadoVisita { get; set; }
        public int IdTipoVisita { get; set; }
        public DateTime FechaHoraProgramada { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}

