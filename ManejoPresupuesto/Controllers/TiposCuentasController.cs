using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
//using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller //Heredamos de Controller para que mi clase se convierta en un controlador dando acceso a herramientas que hacen que MVC funcione.
    {

        /************************ IMPORTANTE *********************
         * 
         * return View();	Vista con el mismo nombre que la acción (metodo)
           return View(modelo);	Vista con el mismo nombre que la acción (metodo) y pasa modelo
           return View("OtraVista");	Vista llamada OtraVista.cshtml
           return View("OtraVista", modelo);	Vista OtraVista.cshtml con modelo
         
         
        el return busca la vista segun el nombre del controlador por ejemplo TiposCuentasController
        entonces busca la vista /Views/TiposCuentas/MiVista.cshtml
        el nombre que comparte la vista con el controlador es "TiposCuentas" asi se relacionan
         
         */


        //IActionResult  puede retornar diferentes tipos de respuestas desde una misma acción, ejemplo de tipos de respuestas(View,RedirectToAction,Json,etc)

        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,IServicioUsuarios servicioUsuarios) //Llamamos la inyeccion de dependencia definida en el archivo Program.cs
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return View(tiposCuentas); //Retorna vista con el mismo nombre que el metodo
        }

        public IActionResult Crear()
        {
          return View();  //Retorna vista con el mismo nombre que el metodo
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {

            if (!ModelState.IsValid) //devuelve true solo si todos los valores enviados desde el formulario cumplen las validaciones del modelo.
            {
                return View(tipoCuenta);  
            }

            tipoCuenta.UsuarioId = servicioUsuarios.ObtenerUsuarioId();

            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), 
                    $"El nombre {tipoCuenta.Nombre} ya existe."); //este error los mostramos gracias a la propiedad ModelOnly en la vista

                return View(tipoCuenta);
            }


            await repositorioTiposCuentas.Crear(tipoCuenta);

            //return View();
            return RedirectToAction("Index"); //hacemos una nueva peticion a otra accion(metodo) dentro de mi controlador
        }

        /* 
         RedirectToAction(Accion,Controlador)
            Un parámetro → acción del mismo controlador ejemplo -> RedirectToAction("Index")
            Dos parámetros → acción de otro controlador ejemplo -> RedirectToAction("NoEncontrado","Home");
         */

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<ActionResult>Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);
            if (tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
       
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre) //Hacemos la validacion consultando en la BD y retornamos un Json para que el navegador lo interprete y muestre el mensaje
        { //en el modelo usamos [Remote]
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioId);
            
            if(yaExisteTipoCuenta){
                return Json($"El nombre {nombre} ya existe."); //Json formato ligero y adecuado para comunicar datos entre navegador y servidor
            }
            return Json(true); 
        }

        [HttpPost] //[FromBody] espera que los datos (un array de int) vengan en el cuerpo del request(solicitud), probablemente como JSON 
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);

            var idsTiposCuentas = tiposCuentas.Select(x => x.Id); //Toma cada x (TipoCuenta) y devuelve solo su Id

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList(); //obtiene los elementos de ids que NO están en idsTiposCuentas

            if (idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();  //HTTP 403
            }
            var tiposCuentasOrdenados = ids.Select((valor, indice) =>
            new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();

            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);

            return Ok(); //HTTP 200
        }


    }
}
