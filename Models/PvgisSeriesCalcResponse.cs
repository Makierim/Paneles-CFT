using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Paneles_CFT.Models
{
    public class PvgisSeriesCalcResponse
    {
        [JsonPropertyName("outputs")]
        public PvgisOutputsSeriesCalc Outputs { get; set; }
    }

    public class PvgisOutputsSeriesCalc
    {
        // CORRECCIÓN FINAL: La clave en el JSON es "hourly", no "tmy".
        [JsonPropertyName("hourly")]
        public List<PvgisDataPoint> Tmy { get; set; }

        [JsonPropertyName("optimal_angles")]
        public PvgisOptimalAngles OptimalAngles { get; set; }
    }

    public class PvgisDataPoint
    {
        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("G(i)")]
        public double GlobalIrradiation { get; set; }

        [JsonPropertyName("T2m")]
        public double Temperature { get; set; }
    }

    public class PvgisOptimalAngles
    {
        public PvgisOptimalAngleItem Angle { get; set; }
        public PvgisOptimalAngleItem Aspect { get; set; }
    }

    public class PvgisOptimalAngleItem
    {
        [JsonPropertyName("value")]
        public double Value { get; set; }
    }
}