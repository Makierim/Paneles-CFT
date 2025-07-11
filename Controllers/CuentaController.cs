using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Paneles_CFT.ViewModels;
using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace Paneles_CFT.Controllers
{
    public class CuentaController : Controller
    {
        private readonly IConfiguration _config;

        public CuentaController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var cadena = _config.GetConnectionString("conexion");

            using SqlConnection con = new(cadena);
            using SqlCommand cmd = new("sp_ValidarUsuario", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@RunInstalador", model.LoginInput);
            cmd.Parameters.AddWithValue("@UsernameInstalador", model.LoginInput);
            cmd.Parameters.AddWithValue("@ContrasenaInstalador", model.Contrasena);

            await con.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();

            if (rd.Read())
            {
                string run = rd["RunInstalador"].ToString();
                string username = rd["UsernameInstalador"].ToString();
                string rol = rd["NombreRol"]?.ToString() ?? "Publico";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, run),
                    new Claim("RunInstalador", run),
                    new Claim("UsernameInstalador", username),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, rol)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var props = new AuthenticationProperties
                {
                    IsPersistent = model.MantenerSesion,
                    ExpiresUtc = model.MantenerSesion 
                        ? DateTimeOffset.UtcNow.AddDays(7)
                        : DateTimeOffset.UtcNow.AddMinutes(2)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Mensaje = "Credenciales inválidas. Por favor, intente de nuevo.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
