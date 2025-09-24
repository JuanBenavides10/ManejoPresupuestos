using System.ComponentModel.DataAnnotations;
using ManejoPresupuesto.Validaciones;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Models
{
    public class TipoCuenta
    {

        /*
         {0} → siempre representa el nombre de la propiedad (Nombre en tu caso).
         {1} → depende del atributo, normalmente es el valor máximo permitido.
         {2} → depende del atributo, normalmente es el valor mínimo permitido. 
         */

        /*
         ALGUNAS VALIDACIONES SON: 

         [StringLength(maximumLength:50,MinimumLength =3,ErrorMessage = "La longitud del campo {0} debe estar entre {2} y {1}")] // atributo: valida la longitud del texto
         [Display(Name ="Nombre del tipo cuenta")] // atributo (cambia el label en la vista)
         
         [Required(ErrorMessage = "El Campo {0} es requerido")]
         [EmailAddress(ErrorMessage ="El Campo debe ser un correo electronico valido")]
         [Range(minimum:18,maximum:130,ErrorMessage ="El valor debe estar entre {1} y {2}")]
         [Url(ErrorMessage = "El campo debe ser una URL valida")]
         */
        public int Id { get; set; }

        [Required(ErrorMessage = "El Campo {0} es requerido")]   // atributo: valida que el campo sea obligatorio
        [PrimeraLetraMayuscula]
        [Remote(action:"VerificarExisteTipoCuenta",controller:"TiposCuentas", AdditionalFields = nameof(Id))] //Remote hace una llamada al servidor(controlador) con el metodo VerificarExisteTipoCuenta validando si el nombre ya existe

        public string Nombre { get; set; } // propiedad
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

    }
}
