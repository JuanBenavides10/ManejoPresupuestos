using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);


//Creamos una pol�tica de autorizaci�n
//Con esto, todas las rutas de la aplicaci�n van a requerir autenticaci�n por defecto, aunque no pongamos [Authorize] en cada controlador.
var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados)); //pasamos la pol�tica para que se aplique globalmente a todos los controladores y m�todos.
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
builder.Services.AddHttpContextAccessor(); //permite acceder al HttpContext actual (la petici�n en curso) desde clases donde normalmente no lo tendr�as disponible como repositorios,servicios,etc , en cambio en el controlador si tenemos acceso porque hereda de ControllerBase 
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

    //MensajesDeErrorIdentity es una clase que se encuentra en la carpeta de servicios, con esta clase traducimos al espa�ol los mensajes de error
    //AddDefaultTokenProviders Genera tokens temporales, utiles para reestablecer contrase�a
}).AddErrorDescriber<MensajesDeErrorIdentity>().AddDefaultTokenProviders(); 


//Usamos Cookie
builder.Services.AddTransient<SignInManager<Usuario>>();

//Aplicamos Cookie, este codigo permite que nuestra aplicacion entienda el uso de cookies para autenticacion
/* En resumen hace lo siguiente: 
        Voy a usar autenticaci�n basada en cookies para toda la aplicaci�n.
        Cuando un usuario se loguee, guardar� un ticket de autenticaci�n en una cookie (Identity.Application).
        En cada request, leer� esa cookie para saber qui�n es el usuario.
        Si no tiene cookie y entra a una p�gina protegida, lo reto (Challenge) mand�ndolo al login.
        Y cuando cierre sesi�n, borrar� esa cookie.
  */
builder.Services.AddAuthentication(options =>
{ 
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme; //c�mo autenticar al usuario en cada request (aqu�: usando la cookie de Identity).
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme; //qu� hacer si un usuario no est� autenticado y entra a una p�gina [Authorize] (Identity lo redirige al login).
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme; //qu� esquema usar para cerrar sesi�n (tambi�n cookies).
}).AddCookie(IdentityConstants.ApplicationScheme, opciones =>
{
    opciones.LoginPath = "/usuarios/login"; // Si alguien intenta acceder a una acci�n protegida con [Authorize] y no est� autenticado, lo redirigimos a esta URL�.
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

app.UseAuthentication(); //Cookie, es el middleware que lee la cookie de autenticaci�n en cada request y reconstruye el usuario para que puedas usarlo en controladores, vistas o [Authorize]

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transacciones}/{action=Index}/{id?}") //escogemos el controlador por defecto
    .WithStaticAssets();


app.Run();
