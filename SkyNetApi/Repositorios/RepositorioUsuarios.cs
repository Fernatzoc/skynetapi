using Dapper;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using System.Data;
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


        public async Task<string> CrearUsuario(IdentityUser usuario)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                usuario.Id = Guid.NewGuid().ToString();

                await conexion.ExecuteAsync(
                    @"INSERT INTO db_skynet.usuarios (id, email, normalized_email, user_name, normalized_user_name, password_hash) 
                      VALUES (@Id, @Email, @NormalizedEmail, @UserName, @NormalizedUserName, @PasswordHash)",
                    new
                    {
                        usuario.Id,
                        usuario.Email,
                        usuario.NormalizedEmail,
                        usuario.UserName,
                        usuario.NormalizedUserName,
                        usuario.PasswordHash
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
                    new { Id = user.Id });

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
    }
}
