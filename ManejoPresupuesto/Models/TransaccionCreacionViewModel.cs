using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class TransaccionCreacionViewModel : Transaccion
    {
        /*
         Un ViewModel es una clase que sirve como "puente" entre modelo y vista
        contiene datos adicionales que la vista necesita mostrar, datos que no existen
        en el modelo (Transaccion), en esta caso tenemos listas para llenar una etiqueta <Select>
         */
        public IEnumerable<SelectListItem> Cuentas { get; set; }
        public IEnumerable<SelectListItem> Categorias { get; set; }

       
       
    }
}
