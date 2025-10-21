using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SkyNetApi.Endpoints;
using SkyNetApi.Entidades;
using SkyNetApi.Repositorios;
using SkyNetApi.Servicios;
using SkyNetApi.Utilidades;

var builder = WebApplication.CreateBuilder(args);

// SERVICIOS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(configure =>
    {
        configure.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    }));

builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false;

    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = Llaves.ObtenerLlave(builder.Configuration).First(),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IRepositorioClientes, RepositorioClientes>();
builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();

builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 1. Registra IHttpContextAccessor (requerido por SignInManager y otros servicios de Identity)
builder.Services.AddHttpContextAccessor();

// 2. Registra tu user store personalizado
builder.Services.AddTransient<IUserStore<IdentityUser>, UsuarioStore>();

// 3. Registra Identity Core y SignInManager correctamente (esto registra todas las dependencias internas)
builder.Services.AddIdentityCore<IdentityUser>()
    .AddSignInManager();

var app = builder.Build();

// MIDDLEWARE
app.UseCors();

// 4. Asegúrate de llamar UseAuthentication() antes de UseAuthorization()
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.MapGroup("/clientes").MapClientes();
app.MapGroup("/usuarios").MapUsuarios();

app.Run();