using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using SkyNetApi.DTOs;
using SkyNetApi.Entidades;
using SkyNetApi.Repositorios;

namespace SkyNetApi.Endpoints
{
    public static class ClientesEndpoints
    {
        public static RouteGroupBuilder MapClientes(this RouteGroupBuilder group)
        {

            group.MapPost("/", CrearCliente);
            group.MapGet("/{id:int}", ObtenerClientePorId);
            group.MapGet("/", ObtenerClientes).RequireAuthorization();
            group.MapPut("/{id:int}", ActualizarCliente);
            group.MapDelete("/clientes/{id:int}", EliminarCliente);


            return group;
        }

        static async Task<Results<Created<ClienteDTO>, ValidationProblem>> CrearCliente(CrearClienteDTO crearClienteDTO, IRepositorioClientes repositorioClientes, IMapper mapper,
            IValidator<CrearClienteDTO> validator)
        {
            var resultadoValidacion = await validator.ValidateAsync(crearClienteDTO);

            if (!resultadoValidacion.IsValid)
            {
                return TypedResults.ValidationProblem(resultadoValidacion.ToDictionary());
            }

            var cliente = mapper.Map<Cliente>(crearClienteDTO);
            var idCliente = await repositorioClientes.crearCliente(cliente);

            var clienteDTO = mapper.Map<ClienteDTO>(cliente);

            return TypedResults.Created($"/clientes/{idCliente}", clienteDTO);
        }

        static async Task<Ok<List<ClienteDTO>>> ObtenerClientes(IRepositorioClientes repositorio, IMapper mapper)
        {

            var clientes = await repositorio.ObtenerClientes();
            var clientesDTO = mapper.Map<List<ClienteDTO>>(clientes);

            return TypedResults.Ok(clientesDTO);
        }

        static async Task<Results<Ok<ClienteDTO>, NotFound>> ObtenerClientePorId(int id, IRepositorioClientes repositorio, IMapper mapper)
        {
            var cliente = await repositorio.ObtenerCliente(id);

            if (cliente is null)
            {
                return TypedResults.NotFound();
            }

            var clienteDTO = mapper.Map<ClienteDTO>(cliente);

            return TypedResults.Ok(clienteDTO);

        }

        static async Task<Results<NoContent, NotFound>> ActualizarCliente(int id, CrearClienteDTO crearClienteDTO, IRepositorioClientes repositorio, IMapper mapper)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            var cliente = mapper.Map<Cliente>(crearClienteDTO);
            cliente.IdCliente = id;

            await repositorio.Actualizar(cliente);
            return TypedResults.NoContent();

        }

        static async Task<Results<NoContent, NotFound>> EliminarCliente(int id, IRepositorioClientes repositorio)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repositorio.Eliminar(id);
            return TypedResults.NoContent();

        }
    }
}
