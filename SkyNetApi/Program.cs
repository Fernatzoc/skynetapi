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
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "role",
        NameClaimType = "sub"
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IRepositorioClientes, RepositorioClientes>();
builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddScoped<IRepositorioVisitas, RepositorioVisitas>();

builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 1. Registra IHttpContextAccessor (requerido por SignInManager y otros servicios de Identity)
builder.Services.AddHttpContextAccessor();

// 2. Registra tu user store personalizado
builder.Services.AddTransient<IUserStore<IdentityUser>, UsuarioStore>();
builder.Services.AddTransient<IRoleStore<IdentityRole>, RoleStore>();

// 3. Registra Identity Core y SignInManager correctamente (esto registra todas las dependencias internas)
builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddSignInManager();

var app = builder.Build();

// Inicializar roles
using (var scope = app.Services.CreateScope())
{
    await InicializadorRoles.Inicializar(scope.ServiceProvider);
}

// MIDDLEWARE
app.UseCors();

// 4. Aseg�rate de llamar UseAuthentication() antes de UseAuthorization()
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World! CD/CI");

app.MapGroup("/clientes").MapClientes();
app.MapGroup("/visitas").MapVisitas();
app.MapGroup("/usuarios").MapUsuarios();

app.Run();