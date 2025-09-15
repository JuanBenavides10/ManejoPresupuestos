using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class CuentaCreacionViewModel : Cuenta //Hereda de la clase Cuenta
    {
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }  

    }
}
