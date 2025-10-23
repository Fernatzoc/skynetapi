﻿﻿using Dapper;
using MySqlConnector;
using SkyNetApi.Entidades;
using System.Data;

namespace SkyNetApi.Repositorios
{
    public class RepositorioClientes : IRepositorioClientes
    {
        private readonly string? connectionString;
        public RepositorioClientes(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Cliente>> ObtenerClientes()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var clientes = await connection.QueryAsync<Cliente>(
                    @"SELECT idCliente, primerNombre, segundoNombre, tercerNombre, 
                             primerApellido, segundoApellido, telefono, correoElectronico,
                             latitud, longitud, direccion, estado
                      FROM db_skynet.cliente 
                      WHERE estado = TRUE 
                      ORDER BY primerNombre");
                return clientes.ToList();
            }
        }

        public async Task<Cliente?> ObtenerCliente(int idCliente)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var cliente = await connection.QueryFirstOrDefaultAsync<Cliente>(
                    @"SELECT * FROM db_skynet.cliente 
                      WHERE idCliente = @idCliente AND estado = TRUE", 
                    new { idCliente });
            
                return cliente;
            }
        }

        public async Task<int> crearCliente(Cliente cliente)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var idCliente = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO db_skynet.cliente 
                      (primerNombre, segundoNombre, tercerNombre, primerApellido, 
                       segundoApellido, telefono, correoElectronico, latitud, longitud, direccion, estado)
                      VALUES 
                      (@PrimerNombre, @SegundoNombre, @TercerNombre, @PrimerApellido, 
                       @SegundoApellido, @Telefono, @CorreoElectronico, @Latitud, @Longitud, @Direccion, @Estado);
                      SELECT LAST_INSERT_ID();",
                    new
                    {
                        cliente.PrimerNombre,
                        SegundoNombre = cliente.SegundoNombre ?? string.Empty,
                        TercerNombre = cliente.TercerNombre ?? string.Empty,
                        cliente.PrimerApellido,
                        SegundoApellido = cliente.SegundoApellido ?? string.Empty,
                        cliente.Telefono,
                        cliente.CorreoElectronico,
                        cliente.Latitud,
                        cliente.Longitud,
                        cliente.Direccion,
                        Estado = true
                    });

                cliente.IdCliente = idCliente;
                return idCliente;
            }

        }

        public async Task<bool> Existe(int id)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                var existe = await conexion.QuerySingleAsync<bool>(
                    @"SELECT COUNT(*) > 0 AS existe 
                      FROM db_skynet.cliente 
                      WHERE idCliente = @id AND estado = TRUE;", 
                    new {id});

                return existe;
            }
        }

        public async Task Actualizar(Cliente cliente)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(@"UPDATE db_skynet.cliente
SET primerNombre = @PrimerNombre,
    segundoNombre = @SegundoNombre,
    tercerNombre = @TercerNombre,
    primerApellido = @PrimerApellido,
    segundoApellido = @SegundoApellido,
    correoElectronico = @CorreoElectronico,
    telefono = @Telefono,
    latitud = @Latitud,
    longitud = @Longitud,
    direccion = @Direccion,
    fechaActualizacion = NOW()
WHERE idCliente = @IdCliente;", new
                {
                    cliente.IdCliente,
                    cliente.PrimerNombre,
                    SegundoNombre = cliente.SegundoNombre ?? string.Empty,
                    TercerNombre = cliente.TercerNombre ?? string.Empty,
                    cliente.PrimerApellido,
                    SegundoApellido = cliente.SegundoApellido ?? string.Empty,
                    cliente.CorreoElectronico,
                    cliente.Telefono,
                    cliente.Latitud,
                    cliente.Longitud,
                    cliente.Direccion
                });
            }
        }

        public async Task Eliminar(int idCliente)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(
                    @"UPDATE db_skynet.cliente 
                      SET estado = FALSE, 
                          fechaActualizacion = NOW() 
                      WHERE idCliente = @idCliente;", 
                    new { idCliente });
            }
        }
    }
}
