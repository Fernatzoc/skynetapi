using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SkyNetApi.DTOs;
using SkyNetApi.Filtros;
using SkyNetApi.Repositorios;
using SkyNetApi.Servicios;
using SkyNetApi.Utilidades;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkyNetApi.Endpoints
{
    public static class UsuariosEndpoints
    {
        public static RouteGroupBuilder MapUsuarios(this RouteGroupBuilder group)
        {
            group.MapPost("/registrar", Registrar)
                .AddEndpointFilter<FiltroValidaciones<CredencialesUsuarioDTO>>();

            group.MapPost("/login", Login)
               .AddEndpointFilter<FiltroValidaciones<LoginDTO>>();

            group.MapPost("/haceradmin", HacerAdmin)
                .AddEndpointFilter<FiltroValidaciones<EditarClaimDTO>>()
            .RequireAuthorization("esadmin");

            group.MapPost("/removeradmin", RemoverAdmin)
                .AddEndpointFilter<FiltroValidaciones<EditarClaimDTO>>()
            .RequireAuthorization("esadmin");

            group.MapPost("/asignarrol", AsignarRol)
                .AddEndpointFilter<FiltroValidaciones<AsignarRolDTO>>()
                .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador));

            group.MapPost("/removerrol", RemoverRol)
                .AddEndpointFilter<FiltroValidaciones<AsignarRolDTO>>()
                .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador));

            group.MapGet("/misroles", ObtenerMisRoles)
                .RequireAuthorization();

            group.MapGet("/todos", ObtenerTodosLosUsuarios)
                .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador));

            group.MapGet("/miperfil", ObtenerMiPerfil)
                .RequireAuthorization();

            group.MapPut("/miperfil", ActualizarMiPerfil)
                .AddEndpointFilter<FiltroValidaciones<ActualizarPerfilUsuarioDTO>>()
                .RequireAuthorization();

            group.MapPut("/perfil", ActualizarPerfilOtroUsuario)
                .AddEndpointFilter<FiltroValidaciones<ActualizarPerfilOtroUsuarioDTO>>()
                .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador));

            group.MapPut("/actualizarstatus", ActualizarStatusUsuario)
                .AddEndpointFilter<FiltroValidaciones<ActualizarStatusUsuarioDTO>>()
                .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador));

            group.MapGet("/tecnicos-asignados/{idSupervisor}", ObtenerTecnicosAsignados)
                .RequireAuthorization(policy => policy.RequireRole(Roles.Supervisor, Roles.Administrador));

            return group;
        }

        static async Task<Results<Ok<RespuestaAutenticacionDTO>, BadRequest<IEnumerable<IdentityError>>>>
            Registrar(CredencialesUsuarioDTO credencialesUsuarioDTO,
            [FromServices] UserManager<IdentityUser> userManager, 
            [FromServices] IRepositorioUsuarios repositorioUsuarios,
            IConfiguration configuration)
        {
            try
            {
                // Establecer el contexto con los datos del perfil antes de crear el usuario
                UsuarioCreacionContexto.Establecer(
                    credencialesUsuarioDTO.FirstName,
                    credencialesUsuarioDTO.MiddleName,
                    credencialesUsuarioDTO.LastName,
                    credencialesUsuarioDTO.SecondSurname,
                    credencialesUsuarioDTO.Phone
                );

                var usuario = new IdentityUser
                {
                    UserName = credencialesUsuarioDTO.Email,
                    Email = credencialesUsuarioDTO.Email,
                    PhoneNumber = credencialesUsuarioDTO.Phone
                };

                var resultado = await userManager.CreateAsync(usuario, credencialesUsuarioDTO.Password);

                if (resultado.Succeeded)
                {
                    var credencialesRespuesta =
                         await ConstruirToken(credencialesUsuarioDTO, configuration, userManager);
                    return TypedResults.Ok(credencialesRespuesta);
                }
                else
                {
                    return TypedResults.BadRequest(resultado.Errors);
                }
            }
            finally
            {
                // Limpiar el contexto después de crear el usuario
                UsuarioCreacionContexto.Limpiar();
            }
        }

        static async Task<Results<Ok<RespuestaAutenticacionDTO>, BadRequest<string>>> Login(
            LoginDTO loginDTO, [FromServices] SignInManager<IdentityUser> signInManager,
            [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            var usuario = await userManager.FindByEmailAsync(loginDTO.Email);

            if (usuario is null)
            {
                return TypedResults.BadRequest("Login incorrecto");
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario,
                loginDTO.Password, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var credencialesUsuarioDTO = new CredencialesUsuarioDTO
                {
                    Email = loginDTO.Email,
                    Password = loginDTO.Password,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Phone = string.Empty
                };
                
                var respuestaAutenticacion =
                     await ConstruirToken(credencialesUsuarioDTO, configuration, userManager);
                return TypedResults.Ok(respuestaAutenticacion);
            }
            else
            {
                return TypedResults.BadRequest("Login incorrecto");
            }
        }

        static async Task<Results<NoContent, NotFound>> HacerAdmin(EditarClaimDTO editarClaimDTO,
    [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.AddClaimAsync(usuario, new Claim("esadmin", "true"));
            return TypedResults.NoContent();
        }

        static async Task<IResult> RemoverAdmin(EditarClaimDTO editarClaimDTO, [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);
            if (usuario is null)
            {
                return Results.NotFound();
            }

            await userManager.RemoveClaimAsync(usuario, new Claim("esadmin", "true"));
            return Results.NoContent();
        }

        static async Task<Results<NoContent, NotFound, BadRequest<IEnumerable<IdentityError>>>> AsignarRol(
            AsignarRolDTO asignarRolDTO, [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByEmailAsync(asignarRolDTO.Email);
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            var resultado = await userManager.AddToRoleAsync(usuario, asignarRolDTO.Rol);
            if (resultado.Succeeded)
            {
                return TypedResults.NoContent();
            }
            else
            {
                return TypedResults.BadRequest(resultado.Errors);
            }
        }

        static async Task<Results<NoContent, NotFound, BadRequest<IEnumerable<IdentityError>>>> RemoverRol(
            AsignarRolDTO asignarRolDTO, [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByEmailAsync(asignarRolDTO.Email);
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            var resultado = await userManager.RemoveFromRoleAsync(usuario, asignarRolDTO.Rol);
            if (resultado.Succeeded)
            {
                return TypedResults.NoContent();
            }
            else
            {
                return TypedResults.BadRequest(resultado.Errors);
            }
        }

        static async Task<Results<Ok<List<string>>, NotFound>> ObtenerMisRoles(
            IServicioUsuarios servicioUsuarios, [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            var roles = await userManager.GetRolesAsync(usuario);
            return TypedResults.Ok(roles.ToList());
        }

        static async Task<Ok<List<UsuarioDTO>>> ObtenerTodosLosUsuarios(
            [FromServices] IRepositorioUsuarios repositorioUsuarios,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuarios = await repositorioUsuarios.ObtenerTodosLosUsuarios();
            var usuariosDTO = new List<UsuarioDTO>();

            foreach (var usuario in usuarios)
            {
                var roles = await userManager.GetRolesAsync(usuario);
                var perfil = await repositorioUsuarios.ObtenerPerfilUsuario(usuario.Id);
                
                usuariosDTO.Add(new UsuarioDTO
                {
                    Id = usuario.Id,
                    Email = usuario.Email!,
                    FirstName = perfil.FirstName,
                    MiddleName = perfil.MiddleName,
                    LastName = perfil.LastName,
                    SecondSurname = perfil.SecondSurname,
                    Phone = perfil.Phone,
                    Status = perfil.Status,
                    Roles = roles.ToList()
                });
            }

            return TypedResults.Ok(usuariosDTO);
        }

        static async Task<Results<Ok<UsuarioDTO>, NotFound>> ObtenerMiPerfil(
            IServicioUsuarios servicioUsuarios,
            [FromServices] IRepositorioUsuarios repositorioUsuarios,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            var roles = await userManager.GetRolesAsync(usuario);
            var perfil = await repositorioUsuarios.ObtenerPerfilUsuario(usuario.Id);

            var usuarioDTO = new UsuarioDTO
            {
                Id = usuario.Id,
                Email = usuario.Email!,
                FirstName = perfil.FirstName,
                MiddleName = perfil.MiddleName,
                LastName = perfil.LastName,
                SecondSurname = perfil.SecondSurname,
                Phone = perfil.Phone,
                Status = perfil.Status,
                Roles = roles.ToList()
            };

            return TypedResults.Ok(usuarioDTO);
        }

        static async Task<Results<NoContent, NotFound>> ActualizarMiPerfil(
            ActualizarPerfilUsuarioDTO perfilDTO,
            IServicioUsuarios servicioUsuarios,
            [FromServices] IRepositorioUsuarios repositorioUsuarios)
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            await repositorioUsuarios.ActualizarPerfilUsuario(
                usuario.Id, 
                perfilDTO.FirstName, 
                perfilDTO.MiddleName,
                perfilDTO.LastName, 
                perfilDTO.SecondSurname, 
                perfilDTO.Phone,
                perfilDTO.Status);

            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> ActualizarPerfilOtroUsuario(
            ActualizarPerfilOtroUsuarioDTO perfilDTO,
            [FromServices] UserManager<IdentityUser> userManager,
            [FromServices] IRepositorioUsuarios repositorioUsuarios)
        {
            var usuario = await userManager.FindByEmailAsync(perfilDTO.Email);
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            await repositorioUsuarios.ActualizarPerfilUsuario(
                usuario.Id, 
                perfilDTO.FirstName, 
                perfilDTO.MiddleName,
                perfilDTO.LastName, 
                perfilDTO.SecondSurname, 
                perfilDTO.Phone,
                perfilDTO.Status);

            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> ActualizarStatusUsuario(
            ActualizarStatusUsuarioDTO statusDTO,
            [FromServices] UserManager<IdentityUser> userManager,
            [FromServices] IRepositorioUsuarios repositorioUsuarios)
        {
            var usuario = await userManager.FindByEmailAsync(statusDTO.Email);
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            await repositorioUsuarios.ActualizarStatusUsuario(usuario.Id, statusDTO.Status);

            return TypedResults.NoContent();
        }

        static async Task<Results<Ok<List<TecnicoAsignadoDTO>>, NotFound<string>, UnauthorizedHttpResult>> ObtenerTecnicosAsignados(
            string idSupervisor,
            HttpContext httpContext,
            IRepositorioUsuarios repositorioUsuarios,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            // Validar que el GUID sea válido
            if (!Guid.TryParse(idSupervisor, out _))
            {
                return TypedResults.NotFound("Supervisor no encontrado");
            }

            // Obtener el ID del usuario autenticado
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

            // Si es supervisor, validar que solo consulte sus propios técnicos
            if (esSupervisor && !esAdministrador && userId != idSupervisor)
            {
                return TypedResults.Unauthorized();
            }

            // Verificar que el supervisor existe
            var supervisorExiste = await userManager.FindByIdAsync(idSupervisor);
            if (supervisorExiste == null)
            {
                return TypedResults.NotFound("Supervisor no encontrado");
            }

            // Obtener técnicos asignados
            var tecnicos = await repositorioUsuarios.ObtenerTecnicosAsignados(idSupervisor);

            var tecnicosDTO = tecnicos.Select(t => new TecnicoAsignadoDTO
            {
                Id = t.Id,
                Email = t.Email,
                FirstName = t.FirstName,
                MiddleName = t.MiddleName,
                LastName = t.LastName,
                SecondSurname = t.SecondSurname,
                Phone = t.Phone
            }).ToList();

            return TypedResults.Ok(tecnicosDTO);
        }

        static async Task<Results<NoContent, NotFound, BadRequest<string>>> CambiarContrasenia(
            CambiarContraseniaDTO cambiarDTO,
            IServicioUsuarios servicioUsuarios,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            // Verificar que la contraseña actual sea correcta
            var verificacion = await userManager.CheckPasswordAsync(usuario, cambiarDTO.ContraseniaActual);
            if (!verificacion)
            {
                return TypedResults.BadRequest("La contraseña actual es incorrecta");
            }

            // Cambiar la contraseña
            var resultado = await userManager.ChangePasswordAsync(usuario, cambiarDTO.ContraseniaActual, cambiarDTO.NuevaContrasenia);
            
            if (resultado.Succeeded)
            {
                return TypedResults.NoContent();
            }
            else
            {
                var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                return TypedResults.BadRequest($"Error al cambiar la contraseña: {errores}");
            }
        }

        static async Task<Results<NoContent, NotFound, BadRequest<string>>> RestablecerContrasenia(
            RestablecerContraseniaDTO restablecerDTO,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByEmailAsync(restablecerDTO.Email);
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            // Remover la contraseña actual y establecer la nueva (solo admins)
            var tokenReset = await userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultado = await userManager.ResetPasswordAsync(usuario, tokenReset, restablecerDTO.NuevaContrasenia);

            if (resultado.Succeeded)
            {
                return TypedResults.NoContent();
            }
            else
            {
                var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                return TypedResults.BadRequest($"Error al restablecer la contraseña: {errores}");
            }
        }


        public async static Task<Results<Ok<RespuestaAutenticacionDTO>, NotFound>>
            RenovarToken(
            IServicioUsuarios servicioUsuarios, IConfiguration configuration,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            var credencialesUsuarioDTO = new CredencialesUsuarioDTO { Email = usuario.Email! };

            var respuestaAutenticacionDTO = await ConstruirToken(credencialesUsuarioDTO, configuration,
                userManager);

            return TypedResults.Ok(respuestaAutenticacionDTO);
        }


        private async static Task<RespuestaAutenticacionDTO>
            ConstruirToken(CredencialesUsuarioDTO credencialesUsuarioDTO,
            IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByNameAsync(credencialesUsuarioDTO.Email);
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario!.Id),
                new Claim(JwtRegisteredClaimNames.Email, credencialesUsuarioDTO.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var claimsDB = await userManager.GetClaimsAsync(usuario!);
            claims.AddRange(claimsDB);

            // Agregar roles al token en formato simple "role"
            var roles = await userManager.GetRolesAsync(usuario!);
            foreach (var rol in roles)
            {
                claims.Add(new Claim("role", rol));
            }

            var llave = Llaves.ObtenerLlave(configuration);
            var creds = new SigningCredentials(llave.First(), SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var tokenDeSeguridad = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiracion, signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion
            };
        }
    }
}
