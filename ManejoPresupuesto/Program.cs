using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);


//Creamos una política de autorización
//Con esto, todas las rutas de la aplicación van a requerir autenticación por defecto, aunque no pongamos [Authorize] en cada controlador.
var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados)); //pasamos la política para que se aplique globalmente a todos los controladores y métodos.
});

/* Inyeccion de dependencias : Tipos
 AddTransient -> Crea una nueva instancia cada vez que se solicita
 AddScoped -> Crea una instancia por cada solicitud HTTP
 AddSingleton -> Crea una sola instancia para toda la aplicacion */

/*Nos permite crear una instancia de RepositorioTiposCuentas cada vez que
un controlador o cualquier otra clase pida en el constructor un parametro de tipo IRepositorioTiposCuentas */
builder.Services.AddTransient<IRepositorioTiposCuentas, RepositorioTiposCuentas>();
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddTransient<IRepositorioCuentas, RepositorioCuentas>();
builder.Services.AddTransient<IRepositorioCategorias, RepositorioCategorias>();
builder.Services.AddTransient<IRepositorioTransacciones, RepositorioTransacciones>();
builder.Services.AddHttpContextAccessor(); //permite acceder al HttpContext actual (la petición en curso) desde clases donde normalmente no lo tendrías disponible como repositorios,servicios,etc , en cambio en el controlador si tenemos acceso porque hereda de ControllerBase 
builder.Services.AddTransient<IServicioReportes, ServicioReportes>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();

//Usamos Identity
builder.Services.AddTransient<IUserStore<Usuario>, UsuarioStore>(); //Cada vez que alguien pida un IUserStore<Usuario>, entregamos una instancia de nuestra clase UsuarioStore
builder.Services.AddIdentityCore<Usuario>(opciones =>
{
    //reglas de validacion de password

    opciones.Password.RequireDigit = false; //no requiere numeros
    opciones.Password.RequireLowercase = false; //no requiere minusculas
    opciones.Password.RequireUppercase = false; //no requiere mayusculas
    opciones.Password.RequireNonAlphanumeric = false; //no requiere alfanumerico

    //MensajesDeErrorIdentity es una clase que se encuentra en la carpeta de servicios, con esta clase traducimos al español los mensajes de error
    //AddDefaultTokenProviders Genera tokens temporales, utiles para reestablecer contraseña
}).AddErrorDescriber<MensajesDeErrorIdentity>().AddDefaultTokenProviders(); 


//Usamos Cookie
builder.Services.AddTransient<SignInManager<Usuario>>();

//Aplicamos Cookie, este codigo permite que nuestra aplicacion entienda el uso de cookies para autenticacion
/* En resumen hace lo siguiente: 
        Voy a usar autenticación basada en cookies para toda la aplicación.
        Cuando un usuario se loguee, guardaré un ticket de autenticación en una cookie (Identity.Application).
        En cada request, leeré esa cookie para saber quién es el usuario.
        Si no tiene cookie y entra a una página protegida, lo reto (Challenge) mandándolo al login.
        Y cuando cierre sesión, borraré esa cookie.
  */
builder.Services.AddAuthentication(options =>
{ 
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme; //cómo autenticar al usuario en cada request (aquí: usando la cookie de Identity).
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme; //qué hacer si un usuario no está autenticado y entra a una página [Authorize] (Identity lo redirige al login).
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme; //qué esquema usar para cerrar sesión (también cookies).
}).AddCookie(IdentityConstants.ApplicationScheme, opciones =>
{
    opciones.LoginPath = "/usuarios/login"; // Si alguien intenta acceder a una acción protegida con [Authorize] y no está autenticado, lo redirigimos a esta URL”.
});


builder.Services.AddTransient<IServicioEmail, ServicioEmail>();



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); //Cookie, es el middleware que lee la cookie de autenticación en cada request y reconstruye el usuario para que puedas usarlo en controladores, vistas o [Authorize]

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transacciones}/{action=Index}/{id?}") //escogemos el controlador por defecto
    .WithStaticAssets();


app.Run();
