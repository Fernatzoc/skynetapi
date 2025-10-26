﻿﻿using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace SkyNetApi.Repositorios
{
    public interface IRepositorioUsuarios
    {
        Task AsignarClaims(IdentityUser user, IEnumerable<Claim> claims);
        Task<IdentityUser?> BuscarUsuarioPorEmail(string normalizedEmail);
        Task<IdentityUser?> BuscarUsuarioPorId(string userId);
        Task<string> CrearUsuario(IdentityUser usuario, string firstName, string? middleName, string lastName, string? secondSurname, string phone);
        Task<List<Claim>> ObtenerClaims(IdentityUser user);
        Task RemoverClaims(IdentityUser user, IEnumerable<Claim> claims);
        
        // Métodos para roles
        Task CrearRol(IdentityRole role);
        Task<IdentityRole?> ObtenerRolPorId(string roleId);
        Task<IdentityRole?> ObtenerRolPorNombre(string normalizedRoleName);
        Task ActualizarRol(IdentityRole role);
        Task EliminarRol(string roleId);
        Task AsignarRolAUsuario(string userId, string roleId);
        Task RemoverRolDeUsuario(string userId, string roleId);
        Task<List<string>> ObtenerRolesDeUsuario(string userId);
        Task<bool> UsuarioTieneRol(string userId, string normalizedRoleName);
        
        // Métodos para listar usuarios
        Task<List<IdentityUser>> ObtenerTodosLosUsuarios();
        
        // Métodos para perfil de usuario
        Task ActualizarPerfilUsuario(string userId, string firstName, string? middleName, string lastName, string secondSurname, string phone, bool status);
        Task<(string? FirstName, string? MiddleName, string? LastName, string? SecondSurname, string? Phone, bool Status)> ObtenerPerfilUsuario(string userId);
        Task ActualizarStatusUsuario(string userId, bool status);
        
        // Método para obtener técnicos asignados a un supervisor
        Task<List<(string Id, string Email, string FirstName, string? MiddleName, string LastName, string? SecondSurname, string Phone)>> ObtenerTecnicosAsignados(string idSupervisor);
        
        // Método para actualizar usuario (contraseña, etc)
        Task ActualizarUsuario(IdentityUser usuario);
    }
}