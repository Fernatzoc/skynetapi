﻿namespace SkyNetApi.DTOs
{
    public class ClienteDTO
    {
        public int Id { get; set; }
        public string PrimerNombre { get; set; } = string.Empty;
        public string? SegundoNombre { get; set; }
        public string? TercerNombre { get; set; }
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public bool Estado { get; set; }
    }
}
