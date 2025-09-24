using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Identity; 

namespace ManejoPresupuesto.Servicios
{
    //video 172
    //Usamos Identity de ASP.NET Core
    //El UsuarioStore es la puerta de entrada que le dice a ASP.NET Identity cómo comunicarse con nuestra base de datos de usuarios.


        /*
            Flujo Completo 
            Controller → pide UserManager<Usuario> → este se dirige a Program.cs y pide IUserStore<Usuario> → le devuelve UsuarioStore → UsuarioStore usa tu repositorioUsuarios → guarda en SQL Server. ✅
        */
    public class UsuarioStore : IUserStore<Usuario>, IUserEmailStore<Usuario>, IUserPasswordStore<Usuario> //Ctrl + . por cada uno y seleccionamos "Implementar Interfaz"
    {
        private readonly IRepositorioUsuarios repositorioUsuarios;

        //Esto de aqui si agregamos nosotros
        public UsuarioStore(IRepositorioUsuarios repositorioUsuarios) //usamos nuestro IRepositorioUsuarios
        {
            this.repositorioUsuarios = repositorioUsuarios;
        }



        //Los metodos de aqui fue lo que se autocompleto cuando "Implementamos interfaz", pero el contenido de los metodos si lo fuimos modificando
        public async Task<IdentityResult> CreateAsync(Usuario user, CancellationToken cancellationToken)  //modificado
        {
            user.Id = await repositorioUsuarios.CrearUsuario(user);
            return IdentityResult.Success;
        }

        public Task<IdentityResult> DeleteAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
           // throw new NotImplementedException();
        }

        public async Task<Usuario> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) //modificado
        {
            return await repositorioUsuarios.BuscarUsuarioPorEmail(normalizedEmail);
        }

        public Task<Usuario> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Usuario> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) //modificado
        {
            return await repositorioUsuarios.BuscarUsuarioPorEmail(normalizedUserName);
        }

        public Task<string> GetEmailAsync(Usuario user, CancellationToken cancellationToken)  //modificado
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedEmailAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(Usuario user, CancellationToken cancellationToken) //modificado
        {
            //devuelve el hash cuando Identity lo necesita.
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(Usuario user, CancellationToken cancellationToken) //modificado
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(Usuario user, CancellationToken cancellationToken) //modificado
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> HasPasswordAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailAsync(Usuario user, string email, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(Usuario user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(Usuario user, string normalizedEmail, CancellationToken cancellationToken) //modificado
        {
            user.EmailNormalizado = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(Usuario user, string normalizedName, CancellationToken cancellationToken) //modificado
        {
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(Usuario user, string passwordHash, CancellationToken cancellationToken) //modificado
        {
            //asigna el hash que Identity genera al objeto Usuario

            user.PasswordHash= passwordHash;
           return Task.CompletedTask;
        }

        public Task SetUserNameAsync(Usuario user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> UpdateAsync(Usuario user, CancellationToken cancellationToken) //Modificado
        {
            await repositorioUsuarios.Actualizar(user);
            return IdentityResult.Success;
        }
    }
}
