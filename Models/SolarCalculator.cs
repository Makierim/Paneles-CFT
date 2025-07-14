using System;
using System.Collections.Generic;
using System.Linq;

namespace Paneles_CFT.Models
{
    // Clase que encapsula la lógica y los parámetros para cálculos propios de rendimiento de un sistema fotovoltaico
    // usando datos instantáneos
    public class SolarCalculator
    {
        // -- Parámetros de entrada del sitio y configuración del panel --
        public double Latitude { get; set; } // Latitud
        public double Longitude { get; set; } // Longitud

        // Azimut de los paneles. Si es que se escoge definir por usuario
        // PVGIS puede sugerir el óptimo
        public double Azimuth { get; set; } // Azimut
        
        // Ángulo de inclinación similar a azimut
        public double TiltAngle { get; set; } // Ángulo de inclicación

        // Eficiencia nominal del panel fotovoltaico (ej 0.20 para 20%)
        public double PanelEfficiency { get; set; } // Eficiencia del panel
        // Área de un solo panel en metros cuadrados
        public double PanelArea { get; set; } // Área total del panel (m2)
        // Número total de paneles en el sistema
        public int NumberOfPanels { get; set; } // Número de paneles
        // Eficiencia del inversor (ej. 0.95 para 95%)
        public double InverterEfficiency { get; set; } // Eficiencia del inversor
        // Coeficiente de potencia por temperatura (%/°C).
        // Típico: -0.4% por °C, así que -0.004
        // Indica cuánto disminuye la potencia por cada grado Celsius que la celda se calienta por encima de STC
        public double TemperatureCoefficient { get; set; } = -0.004; // -0.4% por °C valor por defecto

        // Temperatura nominal de operación de la celda (NOCT) en °C.
        // Típico: 45°C
        // Temperatura de la celda bajo 800 W/m² de irradiancia y 20°C de temperatura ambiente
        public double NOCT { get; set; } = 45; // 45° valor por defecto

        // - Propiedades que contendrán los resultados de los cálculos
        // Potencia de salida en corriente continua (DC) en kW
        public double DCOutput_kW { get; set; }
        // Potencia de salida en corriente alterna (AC) en kW
        public double ACOutput_kW { get; set; }
        // Energía de salida anual en corriente alterna (AC) en kWh
        public double AnnualACOutput_kWh { get; set; }
        // Energía de salida mensual en corriente alterna (AC) en kWh
        // Lista para cada mes
        public List<MonthlyEnergyOutput> MonthlyACOutput_kWh { get; set; } = new List<MonthlyEnergyOutput>();

        // -- Método de Cálculo --

        // Método para calcular la temperatura de la celda (Tc)
        // Utiliza un modelo simplificado basado en la irradiancia y la temperatura del aire
        // Asume que la irradiancia es en W/m², adaptado de modelos comunes
        // G_tilt: Irradiancia global en el plano del panel (W/m²)
        // T_air: Temperatura del aire (grado Celsius)
        public double CalculateCellTemperature(double G_tilt, double T_air)
        {
            const double STC_Irradiance = 1000; // W/m² (Irradiancia en STC)
            const double STC_Temperature = 25; // °C (Temperatura en celca en STC)
            const double NOCT_Irradiance = 800; // W/m² (Irradiancia en NOCT)
            const double NOCT_AirTemperature = 20; // °C (Temperatura de aire en NOCT)

                // --- Simplificación del modelo de temperatura de la celda: ---
                //      Delta_T = (NOCT - STC_Temperature) / ((NOCT_Irradiance / 1000) * (1 - NOCT_AirTemperature / 25))
                //      Un modelo más común y simple es: Tc = T_air + (NOCT - 20) * (G_tilt / 800)
                //      Otro modelo: Tc = T_air + (G_tilt / STC_Irradiance) * (NOCT - NOCT_AirTemperature) * (1 - PanelEfficiency / 0.1) // Más complejo y menos preciso sin coef. de transferencia.

            // Se usa un modelo lineal simple y ampliamente utilizado para fines de demostración:
            // Tc = T_air + (NOCT - 20) * (G_tilt / 800)
            // Asegurar que la temperatura de la celda no caiga por debajo de la temperatura del aire
            if (G_tilt <= 0) return T_air; // Si no hay irradiancia, la celda está a la temperatura del aire
            return T_air + ((NOCT - NOCT_AirTemperature) * (G_tilt / NOCT_Irradiance));
        }

        // Calcular la potencia de salida en corriente continua (DC) de un solo panel en kW
        // G_tilt: Irradiancia global en el plano inclinado (W/m²)
        // T_cell: Temperatura de la celda (grados Celsius)
        public double CalculateSinglePanelDCOutput_kW(double G_tilt, double T_cell)
        {
            // Normalizar la irradiancia a STC (STC = 1000 W/m²)
            // Convertir a kW/m² porque la eficiencia y el área darán W, luego kW
            double irradiance_kW_m2 = G_tilt / 1000.0;

            // Potencia nominal del panel bajo STC (P_nominal = Área * Eficiencia * 1000 W/m²)
            // == Potencia nominal en kW
            double nominalPanelPower_kW = PanelArea * PanelEfficiency; // PanelEfficiency ya es un factor (0.20)

            // Factor de corrección de temperatura: La potencia de los paneles disminuye con la temperatura
            // (T_cell - 25) porque 25°C es la temperatura de la celda en STC
            double temperatureCorrectionFactor = 1 + (TemperatureCoefficient * (T_cell - 25));

            // Potencia DC de un solo panel: (Potencia Nominal * Factor de Radiación * Factor de Temperatura)
            double dcPowerSinglePanel_kW = nominalPanelPower_kW * irradiance_kW_m2 * temperatureCorrectionFactor;

            // Asegurar que la potencia no sea negativa (ej. en la noche)
            return Math.Max(0, dcPowerSinglePanel_kW);
        }

        // Calcula la potencia total de salida en corriente continua (DC) del sistema en kW
        // Se llama para cada punto de tiempo
        public double CalculateSystemDCOutput_kW(double G_tilt, double T_air)
        {
            double T_cell = CalculateCellTemperature(G_tilt, T_air);
            double singlePanelDC = CalculateSinglePanelDCOutput_kW(G_tilt, T_cell);
            return singlePanelDC * NumberOfPanels;
        }

        // Calcula la potencia total de salida en corriente alterna (AC) del sistema en kW
        // Se llama para cada punto del tiempo
        public double CalculateSystemACOutput_kW(double G_tilt, double T_air)
        {
            double dcPower = CalculateSystemDCOutput_kW(G_tilt, T_air);
            // La potencia AC es la potencia DC multiplicada por la eficiencia del inversor
            return dcPower * InverterEfficiency;
        }

        // Método para calcular la producción total diaria o anual a partir de datos horarios
        // PVGIS 'seriescalc' devuelve datos horarios
        // Los datos de G_tilt y Temp ya vienen en la clase PvgisDataPoint
        public void CalculateDailyOrAnnualOutput(List<PvgisDataPoint> hourlyData)
        {
            if (hourlyData == null || !hourlyData.Any())
            {
                AnnualACOutput_kWh = 0;
                MonthlyACOutput_kWh.Clear();
                return;
            }

            double totalAnnualAC_kWh = 0;
            // Inicializar la lista de resultados mensuales
            MonthlyACOutput_kWh = Enumerable.Range(1, 12)
                .Select(m => new MonthlyEnergyOutput { Month = m, Energy_kWh = 0 })
                .ToList();

            foreach (var dataPoint in hourlyData)
            {
                // Calcular la potencia AC para cada hora
                // Convertir la irradiancia de W/m² a kW/m² para que el resultado sea kW
                double currentACOutput_kW = CalculateSystemACOutput_kW(dataPoint.G_tilt, dataPoint.Temp);

                // La potencia es en kW, el intervalo es de 1 hora, por lo que kWh = kW * 1 hora
                double energy_kWh_this_hour = currentACOutput_kW; // kWh

                totalAnnualAC_kWh += energy_kWh_this_hour;

                // Obtener el mes del punto de datos. El formato de Time es YYYYMMDD:HHMM
                int month = int.Parse(dataPoint.Time.Substring(4, 2));
                // Sumar a la producción mensual correspondiente
                MonthlyACOutput_kWh[month - 1].Energy_kWh += energy_kWh_this_hour;
            }

            AnnualACOutput_kWh = totalAnnualAC_kWh;
        }

        // Clase auxiliar para almacenar la producción de energía mensual
        public class MonthlyEnergyOutput
        {
            // Número del mes
            public int Month { get; set; }
            // Energía producia en ese mes en kWh
            public double Energy_kWh { get; set; }
        }
    }
}