using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes; 
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;

namespace Paneles_CFT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SolarProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        // Constructor: Inyecta IHttpClientFactory para crear HttpClient
        public SolarProxyController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // Método API que responde a GET en /SolarProxy/SolarData
        [HttpGet("SolarData")]
        public async Task<IActionResult> GetSolarData(double lat, double lng)
        {
            // 1. URL base de la API de PVGIS (la v5_2 es más estable, la v5_3 es más nueva)
            string pvgisBaseUrl = "https://re.jrc.ec.europa.eu/api/v5_2/";

            // 2. Elegir herramientas PVGIS.
            //    - "MRcalc": Para radiación mensual promedio. (Datos solares generales)
            //    - "seriescalc": Para series de tiempo horarias de radicación. (Más detallado pero más grande)
            //    - "PVcalc": Para estimar la producción de un sistema fotovoltaico. (Requiere más parámetros)

            // Empezar con radiación mensual para simplificar.
            string toolName = "MRcalc";

            // 3. Definir los parámetros de la consulta para PVGIS.
            //    - Formatear lar y lng usando InvariantCulture para asegurar el punto decimal
            //    - lat y lon: Las coordenadas entregadas por la API Google Maps.
            //    -raddatabase: "PVGIS-ERA5" para cubrir Chile y Sudamérica.
            //    - horirrad=1: Solicita la irradiación global en la superficie horizontal.
            //    - outputformat=json: Para respuesta en formato JSON.
            string formattedLat = lat.ToString(CultureInfo.InvariantCulture);
            string formattedLng = lng.ToString(CultureInfo.InvariantCulture);

            // Usar PVGIS-ERA5 para Chile
            string raddatabase = "PVGIS-ERA5";

            string pvgisEndpoint = $"{pvgisBaseUrl}{toolName}?" +
                                   $"lat={formattedLat}&lon={formattedLng}" +
                                   $"&raddatabase={raddatabase}" +
                                   $"&horirrad=1" +
                                   $"&outputformat=json";

            // Para depuración, se puede imprimir la URL que se enviará a PVGIS.
            Console.WriteLine($"Llamando a PVGIS: {pvgisEndpoint}");


            try
            {
                // 4. Realizar la solicitud HTTP GET a la API de PVGIS.
                HttpResponseMessage response = await _httpClient.GetAsync(pvgisEndpoint);

                // 5. Verificar el código de estado HTTP de la respuesta de PVGIS.
                //    EnsureSuccessStatusCode() lanza una excepción si el código no es 2xx. (!success)
                //    Se deben manejar los códigos 429 y 529 específicos de PVGIS.
                if (!response.IsSuccessStatusCode)
                {
                    // Lee el contenido del error para reenviarlo o registrarlo.
                    string errorContent = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return StatusCode((int)response.StatusCode,
                            "Demasiadas solicitudes a la API de PVGIS. Por favor, inténtelo de nuevo más tarde.");
                    }

                    // Código personalizado de PVGIS de sobrecarga.
                    if ((int)response.StatusCode == 529)
                    {
                        return StatusCode((int)response.StatusCode,
                            "La API de PVGIS está sobrecargada. Por favor, inténtelo de nuevo en unos segundos. " +
                            errorContent);
                    }

                    // Para cualquier otro error no exitoso de PVGIS
                    return StatusCode((int)response.StatusCode,
                        $"Error de la API de PVGIS: {response.ReasonPhrase}. Detalle: {errorContent}");
                }

                // 6. Leer la respuesta JSON de PVGIS.
                string jsonResponse = await response.Content.ReadAsStringAsync();
                // return Ok(jsonResponse); // Devuelve la respuesta de la API de Google al cliente

                // 7. Procesar la respuesta JSON de PVGIS.
                // Se deben extraer solo los datos necesarios.
                // Aquí se asume que la respuesta es un objeto JSON con una propiedad "outputs" que contiene los datos de radiación.
                JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                // Extraer total anual
                double? annualEnergy = null;
                var monthlyData = new List<object>();
                // Nueva variable para controlar si hay datos
                bool solarDataAvailable = false;

                // 1. Extraer los datos de la tabla mensual
                if (root.TryGetProperty("outputs", out JsonElement outputsElement) &&
                    outputsElement.TryGetProperty("monthly", out JsonElement monthlyTableElement) &&
                    monthlyTableElement.ValueKind == JsonValueKind.Array)
                {
                    // Crear un diccionario para sumar la irradiación por mes y contar ocurrencias
                    var monthlySums = new Dictionary<int, double>();
                    var monthlyCounts = new Dictionary<int, int>();

                    foreach (var monthEntry in monthlyTableElement.EnumerateArray())
                    {
                        if (monthEntry.TryGetProperty("month", out JsonElement monthNumElement) &&
                            monthEntry.TryGetProperty("H(h)_m", out JsonElement irradiationElement) &&
                            irradiationElement.ValueKind == JsonValueKind.Number)
                        {
                            int monthNum = monthNumElement.GetInt32();
                            double irradiationValue = irradiationElement.GetDouble();

                            if (monthlySums.ContainsKey(monthNum))
                            {
                                monthlySums[monthNum] += irradiationValue;
                                monthlyCounts[monthNum]++;
                            }
                            else
                            {
                                monthlySums.Add(monthNum, irradiationValue);
                                monthlyCounts.Add(monthNum, 1);
                            }
                        }
                    }

                    // 2. Calcular los promedios mensuales y la suma total anual
                    double totalAnnualSum = 0;
                    // Asegurar orden de los meses
                    var orderedMonths = monthlySums.Keys.OrderBy(m => m);

                    foreach (var monthNum in orderedMonths)
                    {
                        double averageMonthlyIrradiance = monthlySums[monthNum] / monthlyCounts[monthNum];
                        totalAnnualSum += averageMonthlyIrradiance; // Suma para el total anual

                        // Mapear número de mes a nombre del mes
                        string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNum);

                        monthlyData.Add(new
                        {
                            Month = monthName,
                            Irradiance_kWh_m2 = averageMonthlyIrradiance
                            // No hay H_m directamente en este formato, hay que buscarlo
                        });
                    }

                    // 3. Asignar la irradiación anual promedio (suma de los promedios mensuales)
                    if (monthlyData.Any()) // Si hay datos mensuales, entonces hay datos solares
                    {
                        annualEnergy = totalAnnualSum;
                        solarDataAvailable = true;
                    }
                }

                var customResponse = new
                {
                    latitude = lat,
                    longitude = lng,
                    solarDataAvailable = solarDataAvailable, // Usa la nueva variable
                    annualIrradiance_kWh_m2 = annualEnergy,
                    monthlyIrradiance = monthlyData,
                    rawPvgisResponse = jsonResponse
                };

                return Ok(customResponse);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en SolarProxyController: {ex.Message}");
                return StatusCode(500, $"Error interno del servidor al procesar datos solares: {ex.Message}");
            }
        }
    }
}