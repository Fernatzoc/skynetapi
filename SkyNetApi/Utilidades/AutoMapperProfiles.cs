﻿using AutoMapper;
using SkyNetApi.DTOs;
using SkyNetApi.Entidades;

namespace SkyNetApi.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<CrearClienteDTO, Cliente>();
            CreateMap<Cliente, ClienteDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdCliente));
            
            CreateMap<CrearVisitaDTO, Visita>();
            // VisitaDTO se mapea manualmente en los endpoints porque incluye datos de registroVisita
        }
    }
}
