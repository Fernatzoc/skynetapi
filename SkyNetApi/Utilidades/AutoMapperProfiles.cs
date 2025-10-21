using AutoMapper;
using SkyNetApi.DTOs;
using SkyNetApi.Entidades;

namespace SkyNetApi.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<CrearClienteDTO, Cliente>();
            CreateMap<Cliente, ClienteDTO>();
        }
    }
}
