using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Paneles_CFT.Models;

namespace Paneles_CFT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SolarProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache; // Inyección de IMemoryCache para controlar consultas
        private readonly string _pvgisBaseUrl;

        // Constructor: Inyecta IHttpClientFactory para crear HttpClient y IMemoryCache para el cache
        public SolarProxyController(IHttpClientFactory httpClientFactory, IMemoryCache cache, IOptions<PvgisApiSettings> pvgisApiSettings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _cache = cache;
            _pvgisBaseUrl = pvgisApiSettings.Value.BaseUrl;
        }

        // ------------------------------------------------------------------------
        // 1. Método para obtener datos de irradiación horizontal promedio (MRcalc)
        //  Endpoint: GET /SolarProxy/SolarData?lat=...&lng=...
        //  Retorna SolarDataResponse, que consolida datos anuales y mensuales
        // ------------------------------------------------------------------------
        [HttpGet("SolarData")]
        public async Task<IActionResult> GetSolarData(double lat, double lng)
        {
            // Clave de caché única para esta combinación de latitud y longitud
            var cacheKey = $"MRcalc_{lat}_{lng}";

            // Intentar obtener los datos de la caché primero
            if (_cache.TryGetValue(cacheKey, out SolarDataResponse cachedData))
            {
                // Si están en caché, devolver inmediatamente.
                return Ok(cachedData);
            }

            // URL base de la API MRcalc de PVGIS
            string pvgisApiUrl = $"{_pvgisBaseUrl}MRcalc?";
            pvgisApiUrl += $"lat={lat}&lon={lng}";
            pvgisApiUrl += $"outputformat=json";
            pvgisApiUrl += $"&raddatabase=PVGIS-ERA5";

            try
            {
                var response = await _httpClient.GetAsync(pvgisApiUrl);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var pvgisMRcalcResponse = JsonSerializer.Deserialize<SolarDataResponse>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var solarDataResponse = new SolarDataResponse
                {
                    Latitude = lat,
                    Longitude = lng,
                    RawPvgisResponse = jsonString
                };

                if (pvgisMRcalcResponse?.Outputs?.Monthly != null && pvgisMRcalcResponse.Outputs.Monthly.Any())
                {
                    solarDataResponse.SolarDataAvailable = true;
                    solarDataResponse.AnnualIrradiance_kWh_m2 = pvgisMRcalcResponse.Outputs.Monthly.Sum(m => m.H_h_m);
                    solarDataResponse.MonthlyIrradiance = pvgisMRcalcResponse.Outputs.Monthly.Select(m =>
                        new MonthlyIrradianceData
                        {
                            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Month),
                            Irradiance_kWh_m2 = m.H_h_m
                        }).ToList();
                }
                else
                {
                    solarDataResponse.SolarDataAvailable = false;
                    solarDataResponse.AnnualIrradiance_kWh_m2 = 0;
                    solarDataResponse.MonthlyIrradiance = new List<MonthlyIrradianceData>();
                }

                // Caché por 24 horas
                _cache.Set(cacheKey, solarDataResponse, TimeSpan.FromHours(24));

                return Ok(solarDataResponse);
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Error al conectar con PVGIS (MRcalc): {httpEx.Message}");
                return StatusCode((int)httpEx.StatusCode, $"Error externo al obtener datos solares: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error al deserializar respuesta de PVGIS (MRcalc): {jsonEx.Message}");
                return StatusCode(500, $"Error al procesar los datos solares recibidos: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado en SolarProxyController.GetSolarData: {ex.Message}");
                return StatusCode(500, $"Ocurrió un error inesperado: {ex.Message}");
            }
        }
        // ------------------------------------------------------------------------
        // 2. Método para estimar la producción de un sistema FV (PVcalc)
        //  Endpoint: POST /SolarProxy/PvSystemProduction
        //  Recibe PvSystemParameters como entrada y retorna PvgisPvCalcResponse
        // ------------------------------------------------------------------------
        [HttpPost("PvSystemProduction")]
        public async Task<IActionResult> GetPvSystemProduction([FromBody] PvSystemParameters parameters)
        {
            if (parameters == null)
            {
                return BadRequest("Parámetros de entrada inválidos.");
            }

            var cacheKey = $"PVcalc_{parameters.Latitude}_{parameters.Longitude}_" +
                           $"{parameters.PeakPower}_{parameters.SystemLosses}_" +
                           // Usar InvariantCulture para dobles en clave
                           $"{parameters.InclinationAngle?.ToString(CultureInfo.InvariantCulture) ?? "null"}_" +
                           $"{parameters.AzimuthAngle?.ToString(CultureInfo.InvariantCulture) ?? "null"}_" +
                           $"{parameters.GetOptimalAngles}";

            if (_cache.TryGetValue(cacheKey, out PvgisPvCalcResponse cachedData))
            {
                return Ok(cachedData);
            }

            string pvgisApiUrl = $"{_pvgisBaseUrl}PVcalc?";
            pvgisApiUrl += $"lat={parameters.Latitude}&lon={parameters.Longitude}";
            pvgisApiUrl += $"&peakpower={parameters.PeakPower}";
            pvgisApiUrl += $"&systemloss={parameters.SystemLosses}";
            pvgisApiUrl += $"&outputformat=json";
            pvgisApiUrl += $"&raddatabase=PVGIS-ERA5";

            if (parameters.GetOptimalAngles)
            {
                pvgisApiUrl += $"&optimalangles=1";
                pvgisApiUrl += $"&optimalinclination=1";
            }
            else
            {
                if (parameters.InclinationAngle.HasValue)
                {
                    pvgisApiUrl += $"&angle={parameters.InclinationAngle.Value}";
                }

                if (parameters.AzimuthAngle.HasValue)
                {
                    pvgisApiUrl += $"&aspect={parameters.AzimuthAngle.Value}";
                }
            }

            pvgisApiUrl += $"&mountingplace=free";

            string jsonString = string.Empty;

            try
            {
                var response = await _httpClient.GetAsync(pvgisApiUrl);
                response.EnsureSuccessStatusCode();

                jsonString = await response.Content.ReadAsStringAsync();
                var pvgisPvCalcResponse = JsonSerializer.Deserialize<PvgisPvCalcResponse>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _cache.Set(cacheKey, pvgisPvCalcResponse, TimeSpan.FromHours(24));

                return Ok(pvgisPvCalcResponse);
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Error al conectar con PVGIS (PVcalc): {httpEx.Message}");
                return StatusCode((int)httpEx.StatusCode,
                    $"Error externo al obtener la producción FV: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine(
                    $"Error al deserializar respuesta de PVGIS (PVcalc): {jsonEx.Message}. JSON crudo: {jsonString}");
                return StatusCode(500, $"Error al procesar los datos de producción FV recibidos: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado en SolarProxyController.GetPvSystemProduction: {ex.Message}");
                return StatusCode(500, $"Ocurrió un error inesperado: {ex.Message}");
            }
        }

        // ------------------------------------------------------------------------
        // 3. Método para obtener datos horarios detallados (seriescalc) y calcular la energía FV
        //  Endpoint: GET /SolarProxy/HourlySolarData?lat=...&lng=...
        //  Retorna los datos horarios crudos de PVGIS, más la energía anual y mensual calculada por SolarCalculator
        // ------------------------------------------------------------------------
        [HttpGet("HourlySolarData")]
        public async Task<IActionResult> GetHourlySolarData(
            double lat,
            double lng,
            double angle = 30, // Inclinación por defecto
            double aspect = 0, // Azimut por defecto (Sur para PVGIS)
            double panelEfficiency = 0.20, // Eficiencia del panel por defecto
            double panelArea = 1.6, // Área del panel por defecto (m²)
            int numberOfPanels = 10, // Número de paneles por defecto
            double inverterEfficiency = 0.95, // Eficiencia del inversor por defecto
            double temperatureCoefficient = -0.004, // Coeficiente de temperatura por defecto
            double noct = 45, // NOCT por defecto
            string startyear = "2005",
            string endyear = "2020",
            bool useoptimalangles = false
        )
        {
            // Clave de caché incluyendo todos los parámetros que afectan al cálculo
            var cacheKey =
                $"SeriesCalcFull_{lat}_{lng}_{angle}_{panelEfficiency}_{panelArea}_{numberOfPanels}_{inverterEfficiency}_{temperatureCoefficient}_{noct}_{startyear}_{endyear}_{useoptimalangles}";

            if (_cache.TryGetValue(cacheKey, out HourlySolarDataResponse cachedResponse))
            {
                return Ok(cachedResponse);
            }

            string pvgisApiUrl = $"{_pvgisBaseUrl}seriescalc?";
            pvgisApiUrl += $"lat={lat}&lon={lng}";
            pvgisApiUrl += $"&outputformat=json";
            pvgisApiUrl += $"&startyear={startyear}&endyear{endyear}";
            pvgisApiUrl += $"&raddatabase=PVGIS-ERA5";
            pvgisApiUrl += $"&meteo_data=temp,wind"; // Temperatura del aire para SolarCalculator

            if (useoptimalangles)
            {
                pvgisApiUrl += $"&optimalangles=1";
                pvgisApiUrl += $"&optimalinclination=1";
            }
            else
            {
                pvgisApiUrl += $"&angle={angle}";
                pvgisApiUrl += $"&aspect={aspect}";
            }

            pvgisApiUrl += $"&mountingplace=free";
            pvgisApiUrl += $"&pvcalculation=0"; // Para pedir solo datos metereológicos, no el cálculo de PVGIS
            pvgisApiUrl += $"&components=1";

            string jsonString = string.Empty;

            try
            {
                var response = await _httpClient.GetAsync(pvgisApiUrl);
                response.EnsureSuccessStatusCode();

                jsonString = await response.Content.ReadAsStringAsync();
                var pvgisData = JsonSerializer.Deserialize<PvgisSeriesCalcResponse>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // -- Integración con SolarCalculator --
                var solarCalculator = new SolarCalculator
                {
                    Latitude = lat,
                    Longitude = lng,
                    Azimuth = aspect, // Usar el azimut pasado a seriescalc
                    TiltAngle = angle, // Lo mismo con el ángulo
                    PanelEfficiency = panelEfficiency,
                    PanelArea = panelArea,
                    NumberOfPanels = numberOfPanels,
                    InverterEfficiency = inverterEfficiency,
                    TemperatureCoefficient = temperatureCoefficient,
                    NOCT = noct
                };

                // Calcular la energía anual y mensual utilizando los datos horarios de PVGIS
                if (pvgisData?.Outputs?.Tmy != null && pvgisData.Outputs.Tmy.Any())
                {
                    solarCalculator.CalculateDailyOrAnnualOutput(pvgisData.Outputs.Tmy);
                }

                // Crear una respuesta combinada que incluye datos horarios crudos de PVGIS y los resultados procesador en SolarCalculator
                var combinedResponse = new HourlySolarDataResponse
                {
                    RawHourlyData = pvgisData, // Para datos crudos
                    AnnualEnergy_kWh = solarCalculator.AnnualACOutput_kWh,
                    MonthlyEnergy_kWh = solarCalculator.MonthlyACOutput_kWh
                };

                // Si se usaron ángulos óptimos, intentar extracción de metadatos de PVGIS
                if (useoptimalangles && pvgisData?.Outputs?.Optimal_Angles != null)
                {
                    combinedResponse.OptimalInclinationAngle = pvgisData.Outputs.Optimal_Angles.Angle.Value;
                    combinedResponse.OptimalAzimuthAngle = pvgisData.Outputs.Optimal_Angles.Aspect.Value;
                }

                _cache.Set(cacheKey, combinedResponse, TimeSpan.FromHours(24)); // Caché por 24 horas

                return Ok(combinedResponse);
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Error al conectar con PVGIS (seriescalc): {httpEx.Message}");
                return StatusCode((int)httpEx.StatusCode, $"Error externo al obtener datos horarios: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error al deserializar respuesta de PVGIS (seriescalc: {jsonEx.Message}");
                return StatusCode(500, $"Error al procesar los datos horarios recibidos: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado en SolarProxyController.GetHourlySolarData: {ex.Message}");
                return StatusCode(500, $"Ocurrió un error inesperado al obtener datos seriescalc: {ex.Message}");
            }
        }
    }
}