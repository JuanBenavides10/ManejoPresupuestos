using System.Security.Claims;
using System.Text;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace ManejoPresupuesto.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IServicioEmail servicioEmail;

        /*
           CONSTRUCTOR
         1. al crear UserManager<Usuario> usamos UsuarioStore, que lo configuramos en Program.cs , UserManager sirve para el CRUD de usuarios.
         Controller → pide UserManager<Usuario> → este se dirige a Program.cs y pide IUserStore<Usuario> → le devuelve UsuarioStore → En UsuarioStore usa repositorioUsuarios → guarda en SQL Server. ✅
       
         2. al crear SignInManager<Usuario> Crea la cookie de autenticación cuando el usuario inicia sesión y la borra cuando cerramos sesion, esto tambien configuramos en Program.cs
         */
        public UsuariosController(UserManager<Usuario> userManager,SignInManager<Usuario> signInManager, IServicioEmail servicioEmail ) 
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.servicioEmail = servicioEmail;
        }

        [AllowAnonymous] //En este metodo Excluimos la politica de autenticacion creada en Program.cs
        public IActionResult Registro()
        {
            return View();  
        }


        /* 
        RedirectToAction(Accion,Controlador)
           Un parámetro → acción del mismo controlador ejemplo -> RedirectToAction("Index")
           Dos parámetros → acción de otro controlador ejemplo -> RedirectToAction("NoEncontrado","Home");
        */

        [HttpPost]
        [AllowAnonymous] //En este metodo Excluimos la politica de autenticacion creada en Program.cs
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid) //devuelve true solo si todos los valores enviados desde el formulario cumplen las validaciones del modelo.
            {
                return View(modelo);
            }

            var usuario = new Usuario()
            {
                Email = modelo.Email
            };

            //cuando llamamos userManager.CreateAsync, el flujo pasa por la clase que creamos UsuarioStore en el metodo CreateAsync
            var resultado = await userManager.CreateAsync(usuario, password: modelo.Password);

            if (resultado.Succeeded)
            {
                /*
                 1. SignInAsync ->  Inicia sesión al usuario creando la cookie de autenticación (Identity.Application)
                 2. No valida nada en la base de datos, ya que le pasamos un usuario que sabemos que es valido, osea el usuario que se acaba de crear

                 El parámetro isPersistent: true significa:
                 true → la cookie será persistente (se guarda aunque cierres el navegador → “recordar sesión”).
                 false → la cookie dura solo mientras la sesión del navegador esté abierta. */
                await signInManager.SignInAsync(usuario,isPersistent: true); 

                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                foreach(var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(modelo);
            }

           
        }

        [HttpGet]
        [AllowAnonymous] //En este metodo Excluimos la politica de autenticacion creada en Program.cs
        public IActionResult Login()
        {
           

            return View();  
        }

        [HttpPost]
        [AllowAnonymous] //En este metodo Excluimos la politica de autenticacion creada en Program.cs
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            /*
            1.SignInManager.PasswordSignInAsync → pide el usuario al UserManager
            2.UserManager → usa IUserStore<Usuario> para encontrar al usuario (tu clase UsuarioStore).
            3.Despues en UsuarioStore en FindByNameAsync → hace el SELECT en tu SQL Server para verificar el usuario
            4. Si lo encuentra, compara la contraseña con el PasswordHasher
            5. Si coincide, crea la cookie Identity.Application para mantener al usuario logueado.
            6. sino coincide incrementa el contador de intentos fallidos y puede bloquear la cuenta, pero en este caso el atributo lockoutOnFailure ponemos en false para que no bloquee la cuenta en caso fallemos varias veces 
             
             En pocas palabras PasswordSignInAsync verifica y luego loguea.
             */
            var resultado = await signInManager.PasswordSignInAsync(modelo.Email, modelo.Password, modelo.Recuerdame, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario o password incorrecto.");
                return View(modelo);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); //Este código cierra la sesión del usuario actual eliminando su cookie de autenticación de Identity

            return RedirectToAction("Index","Transacciones");
        }


        [HttpGet]
        [AllowAnonymous] //En este metodo Excluimos la politica de autenticacion creada en Program.cs
        public IActionResult OlvideMiPassword(string mensaje="")
        {
            ViewBag.Mensaje= mensaje;
            return View();  
        }

        [HttpPost]
        [AllowAnonymous] //En este metodo Excluimos la politica de autenticacion creada en Program.cs
        public async Task<IActionResult> OlvideMiPassword(OlvideMiPasswordViewModel modelo)
        {
            var mensaje = "Proceso concluido, Si el email dado se corresponde con uno de nuestros usuarios, en su bandeja de entrada podra encontrar las instrucciones para recuperar su contraseña.";
            ViewBag.Mensaje = mensaje;

            ModelState.Clear(); //Limpiamos los valores enviados y los posibles errores del modelo

            //FindByEmailAsync verifica en el servicio UsuarioStore si el email existe
            var usuario = await userManager.FindByEmailAsync(modelo.Email);
            if (usuario is null)
            {
                return View();
            }

            var codigo = await userManager.GeneratePasswordResetTokenAsync(usuario); //GeneratePasswordResetTokenAsync -> genera el token de seguridad para restablecer la contraseña.

            var codigoBase64 = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codigo)); // convierte el token generado en una cadena codificada en Base64 para que sea seguro enviar a la URL

            var enlace = Url.Action(
             "RecuperarPassword",    // Nombre de la acción (método) del controlador
             "Usuarios",             // Nombre del controlador
             new { codigo = codigoBase64 }, // Parámetros de la URL (query string)
             protocol: Request.Scheme); // Esquema: http o https


            await servicioEmail.EnviarEmailCambioPassword(modelo.Email, enlace);

            return View();
        }

        [HttpGet]
        [AllowAnonymous] //En este metodo Excluimos la politica de autenticacion creada en Program.cs
        public IActionResult RecuperarPassword(string codigo= null)
        {
            if (codigo is null)
            {
                var mensaje = "Codigo no encontrado";
                return RedirectToAction("OlvideMiPassword", new { mensaje });
            }

            var modelo = new RecuperarPasswordViewModel();
            modelo.CodigoReseteo = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(codigo)); //decodifica el token que anteriormente se codificó para enviarlo por URL

            return View(modelo);    
        }

        [HttpPost]
        [AllowAnonymous] //En este metodo Excluimos la politica de autenticacion creada en Program.cs
        public async Task<IActionResult> RecuperarPassword(RecuperarPasswordViewModel modelo)
        {
            //FindByEmailAsync verifica en el servicio UsuarioStore si el email existe
            var usuario = await userManager.FindByEmailAsync(modelo.Email);

            if (usuario is null)
            {
                return RedirectToAction("PasswordCambiado");
            }

            //ResetPasswordAsync -> valida token y aplica la nueva contraseña  pasando (El usuario, el Token, El nuevo Password)
            //Para despues dirigirse a nuestro servicio UsuarioStore al metodo UpdateAsync que es donde se hace el UPDATE a la Base de datos
            var resultados = await userManager.ResetPasswordAsync(usuario, modelo.CodigoReseteo, modelo.Password);
            
            return RedirectToAction("PasswordCambiado");

        }

        [HttpGet]
        [AllowAnonymous] //En este metodo Excluimos la politica de autenticacion creada en Program.cs
        public IActionResult PasswordCambiado()
        {
            return View();
        }

    }
}
