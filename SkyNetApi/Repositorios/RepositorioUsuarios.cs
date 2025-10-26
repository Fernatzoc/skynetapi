﻿using Dapper;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using System.Security.Claims;

namespace SkyNetApi.Repositorios
{
    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string connectionString;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task<IdentityUser?> BuscarUsuarioPorEmail(string email)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                return await conexion.QuerySingleOrDefaultAsync<IdentityUser>(
                @"SELECT 
                id AS Id,
                email AS Email,
                normalized_email AS NormalizedEmail,
                user_name AS UserName,
                normalized_user_name AS NormalizedUserName,
                password_hash AS PasswordHash
              FROM db_skynet.usuarios 
              WHERE normalized_email = @Email",
                    new { Email = email });
            }
        }

        public async Task<IdentityUser?> BuscarUsuarioPorId(string userId)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                return await conexion.QuerySingleOrDefaultAsync<IdentityUser>(
                @"SELECT 
                id AS Id,
                email AS Email,
                normalized_email AS NormalizedEmail,
                user_name AS UserName,
                normalized_user_name AS NormalizedUserName,
                password_hash AS PasswordHash
              FROM db_skynet.usuarios 
              WHERE id = @UserId",
                    new { UserId = userId });
            }
        }

        public async Task<string> CrearUsuario(IdentityUser usuario, string firstName, string? middleName, string lastName, string? secondSurname, string phone)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                usuario.Id = Guid.NewGuid().ToString();

                await conexion.ExecuteAsync(
                    @"INSERT INTO db_skynet.usuarios (id, email, normalized_email, user_name, normalized_user_name, password_hash, status, phone, first_name, middle_name, last_name, second_surname) 
                      VALUES (@Id, @Email, @NormalizedEmail, @UserName, @NormalizedUserName, @PasswordHash, @Status, @Phone, @FirstName, @MiddleName, @LastName, @SecondSurname)",
                    new
                    {
                        usuario.Id,
                        usuario.Email,
                        usuario.NormalizedEmail,
                        usuario.UserName,
                        usuario.NormalizedUserName,
                        usuario.PasswordHash,
                        Status = true,
                        Phone = phone,
                        FirstName = firstName,
                        MiddleName = middleName ?? string.Empty,
                        LastName = lastName,
                        SecondSurname = secondSurname ?? string.Empty
                    });
                return usuario.Id;
            }
        }

        public async Task<List<Claim>> ObtenerClaims(IdentityUser user)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                var claims = await conexion.QueryAsync<Claim>(@"
            SELECT claim_type AS Type, claim_value AS Value
            FROM db_skynet.usuarios_claims
            WHERE user_id = @Id;",
                    new { user.Id });
                return claims.ToList();
            }
        }


        public async Task AsignarClaims(IdentityUser user, IEnumerable<Claim> claims)
        {
            var sql = @"INSERT INTO usuarios_claims (user_id, claim_type, claim_value)
                        VALUES (@Id, @Type, @Value)";

            var parametros = claims.Select(x => new { user.Id, x.Type, x.Value });

            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(sql, parametros);
            }
        }

        public async Task RemoverClaims(IdentityUser user, IEnumerable<Claim> claims)
        {
            var sql = @"DELETE UsuariosClaims WHERE UserId = @Id AND ClaimType = @Type";
            var parametros = claims.Select(x => new { user.Id, x.Type });

            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(sql, parametros);
            }
        }


        // Implementación de métodos para roles
        public async Task CrearRol(IdentityRole role)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                role.Id = Guid.NewGuid().ToString();
                await conexion.ExecuteAsync(
                    @"INSERT INTO db_skynet.roles (id, name, normalized_name, concurrency_stamp) 
                      VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp)",
                    new { role.Id, role.Name, role.NormalizedName, role.ConcurrencyStamp });
            }
        }

        public async Task<IdentityRole?> ObtenerRolPorId(string roleId)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                return await conexion.QuerySingleOrDefaultAsync<IdentityRole>(
                    @"SELECT id AS Id, name AS Name, normalized_name AS NormalizedName, concurrency_stamp AS ConcurrencyStamp
                      FROM db_skynet.roles WHERE id = @RoleId",
                    new { RoleId = roleId });
            }
        }

        public async Task<IdentityRole?> ObtenerRolPorNombre(string normalizedRoleName)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                return await conexion.QuerySingleOrDefaultAsync<IdentityRole>(
                    @"SELECT id AS Id, name AS Name, normalized_name AS NormalizedName, concurrency_stamp AS ConcurrencyStamp
                      FROM db_skynet.roles WHERE normalized_name = @NormalizedRoleName",
                    new { NormalizedRoleName = normalizedRoleName });
            }
        }

        public async Task ActualizarRol(IdentityRole role)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(
                    @"UPDATE db_skynet.roles 
                      SET name = @Name, normalized_name = @NormalizedName, concurrency_stamp = @ConcurrencyStamp
                      WHERE id = @Id",
                    new { role.Id, role.Name, role.NormalizedName, role.ConcurrencyStamp });
            }
        }

        public async Task EliminarRol(string roleId)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(
                    @"DELETE FROM db_skynet.roles WHERE id = @RoleId",
                    new { RoleId = roleId });
            }
        }

        public async Task AsignarRolAUsuario(string userId, string roleId)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(
                    @"INSERT INTO db_skynet.usuarios_roles (user_id, role_id)
                      VALUES (@UserId, @RoleId)
                      ON DUPLICATE KEY UPDATE user_id = user_id",
                    new { UserId = userId, RoleId = roleId });
            }
        }

        public async Task RemoverRolDeUsuario(string userId, string roleId)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(
                    @"DELETE FROM db_skynet.usuarios_roles 
                      WHERE user_id = @UserId AND role_id = @RoleId",
                    new { UserId = userId, RoleId = roleId });
            }
        }

        public async Task<List<string>> ObtenerRolesDeUsuario(string userId)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                var roles = await conexion.QueryAsync<string>(
                    @"SELECT r.name 
                      FROM db_skynet.roles r
                      INNER JOIN db_skynet.usuarios_roles ur ON r.id = ur.role_id
                      WHERE ur.user_id = @UserId",
                    new { UserId = userId });
                return roles.ToList();
            }
        }

        public async Task<bool> UsuarioTieneRol(string userId, string normalizedRoleName)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                var count = await conexion.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*) 
                      FROM db_skynet.usuarios_roles ur
                      INNER JOIN db_skynet.roles r ON ur.role_id = r.id
                      WHERE ur.user_id = @UserId AND r.normalized_name = @NormalizedRoleName",
                    new { UserId = userId, NormalizedRoleName = normalizedRoleName });
                return count > 0;
            }
        }

        public async Task<List<IdentityUser>> ObtenerTodosLosUsuarios()
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                var usuarios = await conexion.QueryAsync<IdentityUser>(
                    @"SELECT 
                        id AS Id,
                        email AS Email,
                        normalized_email AS NormalizedEmail,
                        user_name AS UserName,
                        normalized_user_name AS NormalizedUserName
                      FROM db_skynet.usuarios
                      ORDER BY email");
                return usuarios.ToList();
            }
        }

        public async Task ActualizarPerfilUsuario(string userId, string firstName, string? middleName, string lastName, string secondSurname, string phone, bool status)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(
                    @"UPDATE db_skynet.usuarios 
                      SET first_name = @FirstName, 
                          middle_name = @MiddleName,
                          last_name = @LastName, 
                          second_surname = @SecondSurname, 
                          phone = @Phone,
                          status = @Status
                      WHERE id = @UserId",
                    new { UserId = userId, FirstName = firstName, MiddleName = middleName ?? string.Empty, LastName = lastName, SecondSurname = secondSurname ?? string.Empty, Phone = phone, Status = status });
            }
        }

        public async Task<(string? FirstName, string? MiddleName, string? LastName, string? SecondSurname, string? Phone, bool Status)> ObtenerPerfilUsuario(string userId)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                var resultado = await conexion.QuerySingleOrDefaultAsync<dynamic>(
                    @"SELECT first_name AS FirstName, middle_name AS MiddleName, last_name AS LastName,
                             second_surname AS SecondSurname, phone AS Phone, CAST(status AS SIGNED) AS Status
                      FROM db_skynet.usuarios 
                      WHERE id = @UserId",
                    new { UserId = userId });
                
                if (resultado == null)
                    return (null, null, null, null, null, true);
                
                // Convertir el valor numérico de Status a bool (0 = false, cualquier otro = true)
                // MySQL TINYINT retorna como long en dynamic
                long statusValue = resultado.Status ?? 1L;
                bool status = statusValue != 0;
                
                return ((string?)resultado.FirstName, (string?)resultado.MiddleName, (string?)resultado.LastName,
                        (string?)resultado.SecondSurname, (string?)resultado.Phone, status);
            }
        }

        public async Task ActualizarStatusUsuario(string userId, bool status)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(
                    @"UPDATE db_skynet.usuarios 
                      SET status = @Status
                      WHERE id = @UserId",
                    new { UserId = userId, Status = status });
            }
        }

        public async Task<List<(string Id, string Email, string FirstName, string? MiddleName, string LastName, string? SecondSurname, string Phone)>> ObtenerTecnicosAsignados(string idSupervisor)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                var tecnicos = await conexion.QueryAsync<dynamic>(
                    @"SELECT DISTINCT u.id AS Id, u.email AS Email, u.first_name AS FirstName, 
                             u.middle_name AS MiddleName, u.last_name AS LastName, 
                             u.second_surname AS SecondSurname, u.phone AS Phone
                      FROM db_skynet.usuarios u
                      INNER JOIN db_skynet.visita v ON u.id = v.idTecnico
                      INNER JOIN db_skynet.usuarios_roles ur ON u.id = ur.user_id
                      INNER JOIN db_skynet.roles r ON ur.role_id = r.id
                      WHERE v.idSupervisor = @IdSupervisor 
                        AND r.normalized_name = 'TECNICO'
                        AND u.status = 1
                      ORDER BY u.first_name, u.last_name",
                    new { IdSupervisor = idSupervisor });

                var resultado = new List<(string, string, string, string?, string, string?, string)>();
                
                foreach (var tecnico in tecnicos)
                {
                    resultado.Add((
                        (string)tecnico.Id,
                        (string)tecnico.Email,
                        (string)tecnico.FirstName,
                        (string?)tecnico.MiddleName,
                        (string)tecnico.LastName,
                        (string?)tecnico.SecondSurname,
                        (string)tecnico.Phone
                    ));
                }

                return resultado;
            }
        }

        public async Task ActualizarUsuario(IdentityUser usuario)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(
                    @"UPDATE db_skynet.usuarios 
                      SET email = @Email,
                          normalized_email = @NormalizedEmail,
                          user_name = @UserName,
                          normalized_user_name = @NormalizedUserName,
                          password_hash = @PasswordHash
                      WHERE id = @Id",
                    new
                    {
                        usuario.Id,
                        usuario.Email,
                        usuario.NormalizedEmail,
                        usuario.UserName,
                        usuario.NormalizedUserName,
                        usuario.PasswordHash
                    });
            }
        }
    }
}
