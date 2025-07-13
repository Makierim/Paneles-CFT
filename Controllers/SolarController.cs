using Microsoft.AspNetCore.Mvc;
using Paneles_Fotovoltaicos.Models;
using Microsoft.AspNetCore.Authorization;

namespace Paneles_Fotovoltaicos.Controllers
{
    [Authorize]
    public class SolarController : Controller
    {
        public IActionResult Map()
        {
            return View(); // Vista para interactuar con el mapa
        }

        // Acción POST para manejar la selección del usuario
        [HttpPost]
        public IActionResult Calculate(double latitude, double longitude)
        {
            // Crear una instancia del modelo SolarCalculator
            var model = new SolarCalculator
            {
                Latitude = latitude,
                Longitude = longitude
            };

            // Realizar los cálculos solares con las coordenadas seleccionadas
            model.CalculateIncidentRadiation();
            model.CalculateDCOutput();
            model.CalculateACOutput();

            // Devolver los resultados a la vista de resultados
            return View("Calculate", model); // Redirige a la vista Calculate con los resultados
        }
    }
}