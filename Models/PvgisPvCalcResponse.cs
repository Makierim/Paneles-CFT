using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Paneles_CFT.Models
{
    public class PvgisPvCalcResponse
    {
        [JsonPropertyName("outputs")]
        public PvgisOutputsPvCalc Outputs { get; set; }
    }

    public class PvgisOutputsPvCalc
    {
        [JsonPropertyName("totals")]
        public PvgisTotalsWrapper Totals { get; set; }

        [JsonPropertyName("monthly")]
        public PvgisMonthlyWrapper Monthly { get; set; }
    }

    public class PvgisTotalsWrapper
    {
        [JsonPropertyName("fixed")]
        public PvgisTotalsPvCalc Fixed { get; set; }
    }

    public class PvgisTotalsPvCalc
    {
        [JsonPropertyName("E_y")]
        public double EnergyYearly { get; set; }

        [JsonPropertyName("l_aoi")]
        public double LossAoi { get; set; }

        [JsonPropertyName("l_tg")]
        public double LossTemperature { get; set; }

        [JsonPropertyName("l_total")]
        public double LossTotal { get; set; }
    }

    public class PvgisMonthlyWrapper
    {
        [JsonPropertyName("fixed")]
        public List<PvgisMonthlyPvCalc> Fixed { get; set; }
    }

    public class PvgisMonthlyPvCalc
    {
        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("E_m")]
        public double EnergyMonthly { get; set; }

        [JsonPropertyName("H(i)_m")]
        public double IrradiationMonthly { get; set; }
    }
}