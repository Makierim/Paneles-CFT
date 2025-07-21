using System;
using System.Collections.Generic;
using System.Linq;

namespace Paneles_CFT.Models
{
    public class SolarCalculator
    {
        public double PanelEfficiency { get; set; }
        public double PanelArea { get; set; }
        public int NumberOfPanels { get; set; }
        public double InverterEfficiency { get; set; }
        public double TemperatureCoefficient { get; set; }
        public double NOCT { get; set; }

        public double AnnualACOutput_kWh { get; private set; }
        public List<MonthlyEnergyOutput> MonthlyACOutput_kWh { get; private set; }

        public SolarCalculator()
        {
            MonthlyACOutput_kWh = new List<MonthlyEnergyOutput>();
        }

        public void CalculateAnnualOutput(List<PvgisDataPoint> hourlyData)
        {
            if (hourlyData == null || !hourlyData.Any())
            {
                AnnualACOutput_kWh = 0;
                MonthlyACOutput_kWh.Clear();
                return;
            }

            // --- INICIO DE LA CORRECCIÓN ---
            // Determina el número de años únicos en el conjunto de datos para promediar.
            // La API de PVGIS devuelve datos para un rango de años (ej. 2005-2019).
            var years = hourlyData.Select(d => int.Parse(d.Time.Substring(0, 4))).Distinct();
            int numberOfYears = years.Count();
            // Evita la división por cero si solo hay datos de un año o si algo falla.
            if (numberOfYears == 0)
            {
                numberOfYears = 1;
            }
            // --- FIN DE LA CORRECCIÓN ---

            double totalEnergySum = 0; // Suma total de todos los años
            var monthlyTotalsSum = new double[12]; // Suma de cada mes de todos los años
            double totalSystemArea = PanelArea * NumberOfPanels;

            foreach (var dataPoint in hourlyData)
            {
                double gTilt = dataPoint.GlobalIrradiation; // en W/m²
                double tAir = dataPoint.Temperature;      // en °C

                if (gTilt <= 0) continue;

                // La lógica de cálculo por hora es correcta.
                double tCell = tAir + ((NOCT - 20.0) * (gTilt / 800.0));
                double tempFactor = 1.0 + (TemperatureCoefficient * (tCell - 25.0));
                double systemDcPowerW = gTilt * totalSystemArea * PanelEfficiency * tempFactor;
                double systemAcPowerW = systemDcPowerW * InverterEfficiency;
                double energyKwhThisHour = Math.Max(0, systemAcPowerW) / 1000.0;

                totalEnergySum += energyKwhThisHour;
                int monthIndex = int.Parse(dataPoint.Time.Substring(4, 2)) - 1;
                if (monthIndex >= 0 && monthIndex < 12)
                {
                    monthlyTotalsSum[monthIndex] += energyKwhThisHour;
                }
            }

            // --- INICIO DE LA CORRECCIÓN ---
            // Divide la suma total entre el número de años para obtener el promedio anual.
            AnnualACOutput_kWh = totalEnergySum / numberOfYears;

            // Crea la lista de producción mensual promediada.
            MonthlyACOutput_kWh = Enumerable.Range(0, 12)
                .Select(i => new MonthlyEnergyOutput
                {
                    Month = i + 1,
                    Energy_kWh = monthlyTotalsSum[i] / numberOfYears // Promedia también cada mes
                })
                .ToList();
            // --- FIN DE LA CORRECCIÓN ---
        }

        public class MonthlyEnergyOutput
        {
            public int Month { get; set; }
            public double Energy_kWh { get; set; }
        }
    }
}