using System.Security.Claims;

namespace ManejoPresupuesto.Servicios
{


    public interface IServicioUsuarios
    {
        int ObtenerUsuarioId();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly HttpContext httpContext;
        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }
        public int ObtenerUsuarioId()
        {
            //Cuando el usuario hace login (SignInAsync o PasswordSignInAsync), Identity construye un ClaimsPrincipal con esa información, y la guarda en la cookie.

            /*
            1. Verifica si el usuario ya está autenticado (IsAuthenticated), Esto significa que la cookie fue validada y el ClaimsPrincipal ya existe.
            2. Busca dentro de los claims uno de tipo ClaimTypes.NameIdentifier, Ese claim es el ID del usuario (por convención en Identity, suele ser el UserId).
            3. Obtiene el valor (idClaim.Value) y lo convierte a int, Ahora ya tienes el ID del usuario logueado en tu aplicación. 
             */

            if (httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                var id = int.Parse(idClaim.Value);

                return id;
            }
            else
            {
                throw new ApplicationException("El usuario no esta autenticado");
            }

            //return 1;
        }

    }
}
