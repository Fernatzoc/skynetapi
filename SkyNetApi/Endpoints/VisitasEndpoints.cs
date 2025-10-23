using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyNetApi.DTOs;
using SkyNetApi.Entidades;
using SkyNetApi.Repositorios;
using SkyNetApi.Utilidades;
using System.IdentityModel.Tokens.Jwt;

namespace SkyNetApi.Endpoints
{
    public static class VisitasEndpoints
    {
        public static RouteGroupBuilder MapVisitas(this RouteGroupBuilder group)
        {
            group.MapPost("/", CrearVisita)
                .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador, Roles.Supervisor));

            group.MapGet("/", ObtenerTodasLasVisitas)
                .RequireAuthorization();

            group.MapGet("/{id:int}", ObtenerVisitaPorId)
                .RequireAuthorization();

            group.MapGet("/{id:int}/detalle", ObtenerDetalleVisita)
                .RequireAuthorization();

            group.MapGet("/cliente/{idCliente:int}", ObtenerVisitasPorCliente)
                .RequireAuthorization();

            group.MapGet("/tecnico/{idTecnico}", ObtenerVisitasPorTecnico)
                .RequireAuthorization();

            group.MapGet("/supervisor/{idSupervisor}", ObtenerVisitasPorSupervisor)
                .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador, Roles.Supervisor));

            group.MapPut("/{id:int}", ActualizarVisita)
                .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador, Roles.Supervisor));

            group.MapPatch("/{id:int}/estado/{idEstado:int}", ActualizarEstadoVisita)
                .RequireAuthorization();

            group.MapPost("/{id:int}/registrar", RegistrarVisitaTecnico)
                .RequireAuthorization(policy => policy.RequireRole(Roles.Tecnico, Roles.Administrador));

            group.MapDelete("/{id:int}", EliminarVisita)
                .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador));

            return group;
        }

        static async Task<Results<Created<VisitaDTO>, ValidationProblem, NotFound>> CrearVisita(
            CrearVisitaDTO crearVisitaDTO,
            IRepositorioVisitas repositorioVisitas,
            IRepositorioClientes repositorioClientes,
            IRepositorioUsuarios repositorioUsuarios,
            IMapper mapper,
            IValidator<CrearVisitaDTO> validator)
        {
            var resultadoValidacion = await validator.ValidateAsync(crearVisitaDTO);

            if (!resultadoValidacion.IsValid)
            {
                return TypedResults.ValidationProblem(resultadoValidacion.ToDictionary());
            }

            // Verificar que el cliente existe
            var clienteExiste = await repositorioClientes.Existe(crearVisitaDTO.IdCliente);
            if (!clienteExiste)
            {
                return TypedResults.NotFound();
            }

            var visita = mapper.Map<Visita>(crearVisitaDTO);
            var idVisita = await repositorioVisitas.Crear(visita);

            var visitaCreada = await repositorioVisitas.ObtenerPorId(idVisita);
            var visitaDTO = await MapearVisitaADTO(visitaCreada!, repositorioClientes, repositorioUsuarios, repositorioVisitas);

            return TypedResults.Created($"/visitas/{idVisita}", visitaDTO);
        }

        static async Task<Ok<List<VisitaDTO>>> ObtenerTodasLasVisitas(
            IRepositorioVisitas repositorio,
            IRepositorioClientes repositorioClientes,
            IRepositorioUsuarios repositorioUsuarios)
        {
            var visitas = await repositorio.ObtenerTodas();
            var visitasDTO = new List<VisitaDTO>();

            foreach (var visita in visitas)
            {
                var visitaDTO = await MapearVisitaADTO(visita, repositorioClientes, repositorioUsuarios, repositorio);
                visitasDTO.Add(visitaDTO);
            }

            return TypedResults.Ok(visitasDTO);
        }

        static async Task<Results<Ok<VisitaDTO>, NotFound>> ObtenerVisitaPorId(
            int id,
            IRepositorioVisitas repositorio,
            IRepositorioClientes repositorioClientes,
            IRepositorioUsuarios repositorioUsuarios)
        {
            var visita = await repositorio.ObtenerPorId(id);

            if (visita is null)
            {
                return TypedResults.NotFound();
            }

            var visitaDTO = await MapearVisitaADTO(visita, repositorioClientes, repositorioUsuarios, repositorio);

            return TypedResults.Ok(visitaDTO);
        }

        static async Task<Results<Ok<VisitaDetalleDTO>, NotFound, UnauthorizedHttpResult, ProblemHttpResult>> ObtenerDetalleVisita(
            int id,
            HttpContext httpContext,
            IRepositorioVisitas repositorioVisitas,
            IRepositorioClientes repositorioClientes,
            IRepositorioUsuarios repositorioUsuarios,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            // Obtener la visita
            var visita = await repositorioVisitas.ObtenerPorId(id);

            if (visita is null)
            {
                return TypedResults.NotFound();
            }

            // Obtener el ID del usuario autenticado desde el token
            var userId = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return TypedResults.Unauthorized();
            }

            // Obtener los roles del usuario autenticado
            var usuario = await userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                return TypedResults.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(usuario);
            var esAdministrador = roles.Contains(Roles.Administrador);
            var esSupervisor = roles.Contains(Roles.Supervisor);
            var esTecnico = roles.Contains(Roles.Tecnico);

            // Validar permisos según el rol
            if (!esAdministrador)
            {
                if (esTecnico && visita.IdTecnico != userId)
                {
                    // El técnico solo puede ver sus propias visitas
                    return TypedResults.Unauthorized();
                }

                if (esSupervisor && !esTecnico && visita.IdSupervisor != userId)
                {
                    // El supervisor solo puede ver visitas que supervisa
                    return TypedResults.Unauthorized();
                }
            }

            // Obtener el cliente asociado a la visita
            var cliente = await repositorioClientes.ObtenerCliente(visita.IdCliente);

            if (cliente == null)
            {
                return TypedResults.Problem(
                    statusCode: 500,
                    detail: "El cliente asociado a esta visita no existe. Por favor contacte al administrador.");
            }

            // Mapear la visita a DTO
            var visitaDTO = await MapearVisitaADTO(visita, repositorioClientes, repositorioUsuarios, repositorioVisitas);

            // Crear el DTO del cliente con todos los detalles
            var clienteDetalleDTO = new ClienteDetalleDTO
            {
                Id = cliente.IdCliente,
                PrimerNombre = cliente.PrimerNombre,
                SegundoNombre = cliente.SegundoNombre,
                TercerNombre = cliente.TercerNombre,
                PrimerApellido = cliente.PrimerApellido,
                SegundoApellido = cliente.SegundoApellido,
                Telefono = cliente.Telefono,
                CorreoElectronico = cliente.CorreoElectronico,
                Latitud = (decimal)cliente.Latitud,
                Longitud = (decimal)cliente.Longitud,
                Direccion = cliente.Direccion,
                Estado = cliente.Estado
            };

            // Crear el DTO de respuesta combinado
            var visitaDetalleDTO = new VisitaDetalleDTO
            {
                Visita = visitaDTO,
                Cliente = clienteDetalleDTO
            };

            return TypedResults.Ok(visitaDetalleDTO);
        }

        static async Task<Ok<List<VisitaDTO>>> ObtenerVisitasPorCliente(
            int idCliente,
            IRepositorioVisitas repositorio,
            IRepositorioClientes repositorioClientes,
            IRepositorioUsuarios repositorioUsuarios)
        {
            var visitas = await repositorio.ObtenerPorCliente(idCliente);
            var visitasDTO = new List<VisitaDTO>();

            foreach (var visita in visitas)
            {
                var visitaDTO = await MapearVisitaADTO(visita, repositorioClientes, repositorioUsuarios, repositorio);
                visitasDTO.Add(visitaDTO);
            }

            return TypedResults.Ok(visitasDTO);
        }

        static async Task<Ok<List<VisitaDTO>>> ObtenerVisitasPorTecnico(
            string idTecnico,
            IRepositorioVisitas repositorio,
            IRepositorioClientes repositorioClientes,
            IRepositorioUsuarios repositorioUsuarios)
        {
            var visitas = await repositorio.ObtenerPorTecnico(idTecnico);
            var visitasDTO = new List<VisitaDTO>();

            foreach (var visita in visitas)
            {
                var visitaDTO = await MapearVisitaADTO(visita, repositorioClientes, repositorioUsuarios, repositorio);
                visitasDTO.Add(visitaDTO);
            }

            return TypedResults.Ok(visitasDTO);
        }

        static async Task<Ok<List<VisitaDTO>>> ObtenerVisitasPorSupervisor(
            string idSupervisor,
            IRepositorioVisitas repositorio,
            IRepositorioClientes repositorioClientes,
            IRepositorioUsuarios repositorioUsuarios)
        {
            var visitas = await repositorio.ObtenerPorSupervisor(idSupervisor);
            var visitasDTO = new List<VisitaDTO>();

            foreach (var visita in visitas)
            {
                var visitaDTO = await MapearVisitaADTO(visita, repositorioClientes, repositorioUsuarios, repositorio);
                visitasDTO.Add(visitaDTO);
            }

            return TypedResults.Ok(visitasDTO);
        }

        static async Task<Results<NoContent, NotFound, ValidationProblem>> ActualizarVisita(
            int id,
            CrearVisitaDTO crearVisitaDTO,
            IRepositorioVisitas repositorio,
            IRepositorioClientes repositorioClientes,
            IMapper mapper,
            IValidator<CrearVisitaDTO> validator)
        {
            var resultadoValidacion = await validator.ValidateAsync(crearVisitaDTO);

            if (!resultadoValidacion.IsValid)
            {
                return TypedResults.ValidationProblem(resultadoValidacion.ToDictionary());
            }

            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            // Verificar que el cliente existe
            var clienteExiste = await repositorioClientes.Existe(crearVisitaDTO.IdCliente);
            if (!clienteExiste)
            {
                return TypedResults.NotFound();
            }

            var visita = mapper.Map<Visita>(crearVisitaDTO);
            visita.IdVisita = id;

            await repositorio.Actualizar(visita);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> ActualizarEstadoVisita(
            int id,
            int idEstado,
            IRepositorioVisitas repositorio)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repositorio.ActualizarEstado(id, idEstado);
            return TypedResults.NoContent();
        }

        static async Task<Results<Created<int>, NotFound, ValidationProblem, BadRequest<string>>> RegistrarVisitaTecnico(
            int id,
            RegistrarVisitaTecnicoDTO registroDTO,
            IRepositorioVisitas repositorio,
            IValidator<RegistrarVisitaTecnicoDTO> validator)
        {
            var resultadoValidacion = await validator.ValidateAsync(registroDTO);

            if (!resultadoValidacion.IsValid)
            {
                return TypedResults.ValidationProblem(resultadoValidacion.ToDictionary());
            }

            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            // Verificar si ya existe un registro para esta visita
            var registroExistente = await repositorio.ObtenerRegistroVisita(id);
            if (registroExistente != null)
            {
                return TypedResults.BadRequest("Esta visita ya ha sido registrada");
            }

            var registroVisita = new RegistroVisita
            {
                IdVisita = id,
                FechaHoraInicioReal = registroDTO.FechaHoraInicioReal,
                FechaHoraFinReal = registroDTO.FechaHoraFinReal,
                Observaciones = registroDTO.Observaciones
            };

            var idRegistro = await repositorio.CrearRegistroVisita(registroVisita);

            return TypedResults.Created($"/visitas/{id}/registro", idRegistro);
        }

        static async Task<Results<NoContent, NotFound>> EliminarVisita(
            int id,
            IRepositorioVisitas repositorio)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repositorio.Eliminar(id);
            return TypedResults.NoContent();
        }

        // Método auxiliar para mapear visita a DTO con información relacionada
        private static async Task<VisitaDTO> MapearVisitaADTO(
            Visita visita,
            IRepositorioClientes repositorioClientes,
            IRepositorioUsuarios repositorioUsuarios,
            IRepositorioVisitas repositorioVisitas)
        {
            var cliente = await repositorioClientes.ObtenerCliente(visita.IdCliente);
            var tecnico = await repositorioUsuarios.ObtenerPerfilUsuario(visita.IdTecnico);
            var supervisor = await repositorioUsuarios.ObtenerPerfilUsuario(visita.IdSupervisor);
            
            // Obtener el registro de visita si existe
            var registroVisita = await repositorioVisitas.ObtenerRegistroVisita(visita.IdVisita);

            return new VisitaDTO
            {
                Id = visita.IdVisita,
                IdCliente = visita.IdCliente,
                NombreCliente = cliente != null 
                    ? $"{cliente.PrimerNombre} {cliente.PrimerApellido}".Trim() 
                    : "Cliente no encontrado",
                IdTecnico = visita.IdTecnico,
                NombreTecnico = $"{tecnico.FirstName} {tecnico.LastName}".Trim(),
                IdSupervisor = visita.IdSupervisor,
                NombreSupervisor = $"{supervisor.FirstName} {supervisor.LastName}".Trim(),
                IdEstadoVisita = visita.IdEstadoVisita,
                EstadoVisita = ObtenerNombreEstado(visita.IdEstadoVisita),
                IdTipoVisita = visita.IdTipoVisita,
                TipoVisita = ObtenerNombreTipo(visita.IdTipoVisita),
                FechaHoraProgramada = visita.FechaHoraProgramada,
                Descripcion = visita.Descripcion,
                // Información del registro (si existe)
                IdRegistroVisita = registroVisita?.IdRegistroVisita,
                FechaHoraInicioReal = registroVisita?.FechaHoraInicioReal,
                FechaHoraFinReal = registroVisita?.FechaHoraFinReal,
                Observaciones = registroVisita?.Observaciones
            };
        }

        // Métodos auxiliares para obtener nombres (deberían venir de tablas)
        private static string ObtenerNombreEstado(int idEstado)
        {
            return idEstado switch
            {
                1 => "Pendiente",
                2 => "En Progreso",
                3 => "Completada",
                4 => "Cancelada",
                _ => "Desconocido"
            };
        }

        private static string ObtenerNombreTipo(int idTipo)
        {
            return idTipo switch
            {
                1 => "Instalación",
                2 => "Mantenimiento",
                3 => "Reparación",
                4 => "Inspección",
                _ => "Desconocido"
            };
        }
    }
}

