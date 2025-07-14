using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Paneles_CFT.Controllers
{
    [Authorize]
    public class SolarController : Controller
    {
        // Acción para mostrar la vista del mapa
        public IActionResult Index()
        {
            return View(); // Vista para interactuar con el mapa
        }
    }
}