using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Paneles_CFT.Models;

namespace Paneles_CFT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolarProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _pvgisBaseUrl;

        public SolarProxyController(IHttpClientFactory httpClientFactory, IOptions<PvgisApiSettings> pvgisApiSettings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _pvgisBaseUrl = pvgisApiSettings.Value.BaseUrl;
        }

        [HttpGet("SolarData")]
        public async Task<IActionResult> GetSolarData(double lat, double lng)
        {
            string pvgisApiUrl = $"{_pvgisBaseUrl}PVcalc?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lng.ToString(CultureInfo.InvariantCulture)}&outputformat=json&raddatabase=PVGIS-ERA5&angle=0&aspect=0&peakpower=1&loss=0&pvtech=csi&mountingplace=free";

            try
            {
                var jsonString = await _httpClient.GetStringAsync(pvgisApiUrl);
                var pvgisResponse = JsonSerializer.Deserialize<PvgisPvCalcResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var solarDataResponse = new SolarDataResponse();

                if (pvgisResponse?.Outputs?.Monthly?.Fixed != null)
                {
                    solarDataResponse.SolarDataAvailable = true;
                    solarDataResponse.AnnualIrradiance_kWh_m2 = pvgisResponse.Outputs.Monthly.Fixed.Sum(m => m.IrradiationMonthly);
                    solarDataResponse.MonthlyIrradiance = pvgisResponse.Outputs.Monthly.Fixed.Select(m => new MonthlyIrradianceData
                    {
                        Month = new DateTime(2000, m.Month, 1).ToString("MMMM", CultureInfo.CurrentCulture),
                        Irradiance_kWh_m2 = m.IrradiationMonthly
                    }).ToList();
                }
                return Ok(solarDataResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error en GetSolarData: {ex.Message}");
            }
        }

        [HttpPost("PvSystemProduction")]
        public async Task<IActionResult> GetPvSystemProduction([FromBody] PvSystemParameters parameters)
        {
            if (parameters == null) return BadRequest();

            string pvgisApiUrl = $"{_pvgisBaseUrl}PVcalc?lat={parameters.Latitude.ToString(CultureInfo.InvariantCulture)}&lon={parameters.Longitude.ToString(CultureInfo.InvariantCulture)}&peakpower={parameters.PeakPower.ToString(CultureInfo.InvariantCulture)}&loss={parameters.SystemLosses.ToString(CultureInfo.InvariantCulture)}&pvtech=csi&outputformat=json&raddatabase=PVGIS-ERA5&mountingplace=free";

            if (parameters.GetOptimalAngles)
            {
                pvgisApiUrl += "&optimalinclination=1&optimalangle=1";
            }
            else
            {
                if (parameters.InclinationAngle.HasValue) pvgisApiUrl += $"&angle={parameters.InclinationAngle.Value.ToString(CultureInfo.InvariantCulture)}";
                if (parameters.AzimuthAngle.HasValue) pvgisApiUrl += $"&aspect={parameters.AzimuthAngle.Value.ToString(CultureInfo.InvariantCulture)}";
            }

            try
            {
                var jsonString = await _httpClient.GetStringAsync(pvgisApiUrl);
                var pvgisResponse = JsonSerializer.Deserialize<PvgisPvCalcResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Ok(pvgisResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error en GetPvSystemProduction: {ex.Message}");
            }
        }

        [HttpGet("HourlySolarData")]
        public async Task<IActionResult> GetHourlySolarData(double lat, double lng, double angle, double aspect, double panelEfficiency, double panelArea, int numberOfPanels, double inverterEfficiency, double temperatureCoefficient, double noct, string startyear = "2005", string endyear = "2019", bool useoptimalangles = false)
        {
            string pvgisApiUrl = $"{_pvgisBaseUrl}seriescalc?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lng.ToString(CultureInfo.InvariantCulture)}&outputformat=json&startyear={startyear}&endyear={endyear}&raddatabase=PVGIS-ERA5&pvcalculation=0&meteo_data=T2m,WS10m&mountingplace=free";

            if (useoptimalangles)
            {
                pvgisApiUrl += "&optimalinclination=1&optimalangle=1";
            }
            else
            {
                pvgisApiUrl += $"&angle={angle.ToString(CultureInfo.InvariantCulture)}&aspect={aspect.ToString(CultureInfo.InvariantCulture)}";
            }

            // --- INICIO DEPURACIÓN BACKEND ---
            Console.WriteLine("\n--- DEPURACIÓN BOTÓN 3 (BACKEND) ---");
            Console.WriteLine("1. URL enviada a PVGIS (seriescalc):");
            Console.WriteLine(pvgisApiUrl);
            // --- FIN DEPURACIÓN ---

            try
            {
                var jsonString = await _httpClient.GetStringAsync(pvgisApiUrl);

                // --- INICIO DEPURACIÓN BACKEND ---
                Console.WriteLine("\n2. JSON crudo recibido de PVGIS (seriescalc):");
                Console.WriteLine(jsonString);
                // --- FIN DEPURACIÓN ---

                var pvgisData = JsonSerializer.Deserialize<PvgisSeriesCalcResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Se crea una instancia de la calculadora y se le pasan los parámetros
                var solarCalculator = new SolarCalculator
                {
                    PanelEfficiency = panelEfficiency,
                    PanelArea = panelArea,
                    NumberOfPanels = numberOfPanels,
                    InverterEfficiency = inverterEfficiency,
                    TemperatureCoefficient = temperatureCoefficient,
                    NOCT = noct
                };

                // Se llama al método de cálculo con los datos de la API
                if (pvgisData?.Outputs?.Tmy != null)
                {
                    solarCalculator.CalculateAnnualOutput(pvgisData.Outputs.Tmy);
                }

                // Se crea y devuelve la respuesta
                var combinedResponse = new
                {
                    annualEnergy_kWh = solarCalculator.AnnualACOutput_kWh,
                    monthlyEnergy_kWh = solarCalculator.MonthlyACOutput_kWh,
                    optimalInclinationAngle = pvgisData?.Outputs?.OptimalAngles?.Angle.Value,
                    optimalAzimuthAngle = pvgisData?.Outputs?.OptimalAngles?.Aspect.Value
                };

                return Ok(combinedResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] en GetHourlySolarData: {ex.Message}");
                return StatusCode(500, $"Ocurrió un error inesperado: {ex.Message}");
            }
        }
    }
}