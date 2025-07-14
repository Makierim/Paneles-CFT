using System.Collections.Generic;

namespace Paneles_CFT.Models
{
    // Clase principal que mapea la respuesta JSON de la herramienta "MRcalc"
    // Se usa también para consolidar y simplificar los datos enviados al frontend
    public class SolarDataResponse
    {
        // Propiedades para lat y lng, parte de la respuesta PVGIS y parámetros de entrada
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Propiedad que contendrá el valor del promedio anual de la irradiación horizontal global
        // que se calculan sumando los valores mensuales de la API de PVGIS
        public double AnnualIrradiance_kWh_m2 { get; set; }

        // Lista que contendrá los promedios de irradiación horizontal global por cada mes
        // Uso de la clase auxiliar 'MonthlyIrradianceData' para estructurar cada elemento
        public List<MonthlyIrradianceData> MonthlyIrradiance { get; set; }

        // Un flag para indicar si se pudieron obtener datos solares o si hubo un problema
        public bool SolarDataAvailable { get; set; }

        // Guardar la respuesta JSON cruda de PVGIS para depuración
        public string RawPvgisResponse { get; set; }

        // - Clases internas para mapear la estructura de la respuesta cruda de MRcalc -
        // Clases anidadas que reflejan la estructura JSON de la respuesta de la API MRcalc
        // Necesarias para que el deserializador JSON (System.Text.Json) sepa cómo convertir el JSON a objeto C#
        public PvgisOutputsMRcalc Outputs { get; set; }
        public PvgisMetaMRcalc Meta { get; set; }
    }

    // Clase para sección outputs de la respuesta de MRcalc
    public class PvgisOutputsMRcalc
    {
        // Lista de objetos que representan los datos mensuales
        public List<PvgisMonthlyMRcalc> Monthly { get; set; }
    }

    // Clase para cada elemento mensual dentro de la sección outputs de MRcalc
    public class PvgisMonthlyMRcalc
    {
        // Número del mes
        public int Month { get; set; }
        // Irradiación horizontal global promedio para ese mes en KWh/m²
        // El nombre 'H_h_m' viene directo del JSON de PVGIS
        public double H_h_m { get; set; }
    }

    // Clase para la sección meta de la respuesta MRcalc
    // Proporciona descripciones y unidades de los datos
    public class PvgisMetaMRcalc
    {
        public PvgisMetaOutputsMRcalc Outputs { get; set; }
    }

    // Clase para la meta-información sobre las salidas en MRcalc
    public class PvgisMetaOutputsMRcalc
    {
        public PvgisMetaMonthlyMRcalc Monthly { get; set; }
    }

    // Clase para la meta-información sobre los datos mensuales en MRcalc
    public class PvgisMetaMonthlyMRcalc
    {
        public PvgisMetaVariablesMRcalc Variables { get; set; }
    }

    // Clase que describe las variables disponibles en la respuesta mensual de MRcalc
    public class PvgisMetaVariablesMRcalc
    {
        // Metadatos para H_h_m
        public PvgisMetaVariablesItemCalc H_h_m { get; set; }
    }

    // Clase genérica para describir una variable, incluyendo su descripción y unidades
    public class PvgisMetaVariablesItemCalc
    {
        public string Description { get; set; }
        public string Units { get; set; }
    }

    // Clase auxiliar para simplificar la lista mensual enviada al frontend (Más amigable que PvgisMonthlyMRcalc)
    public class MonthlyIrradianceData
    {
        // Nombre del mes (ej. "Enero", "Febrero")
        public string Month { get; set; }
        // La irradiación correspondiente para ese mes
        public double Irradiance_kWh_m2 { get; set; }
    }

    // Clase que encapsula la respuesta combinada para los datos horarios de PVGIS y
    // cálculos de energía por SolarCalculator en backend
    public class HourlySolarDataResponse
    {
        // Convierte respuesta JSON cruda de la API seriescacl de PVGIS
        public PvgisSeriesCalcResponse RawHourlyData { get; set; }

        // Energía anual total en AC calculada por SolarCalculator (kWH)
        public double AnnualEnergy_kWh { get; set; }

        // Lista de la energía mensual en AC calculada por el SolarCalculator (kWh)
        public List<SolarCalculator.MonthlyEnergyOutput> MonthlyEnergy_kWh { get; set; }

        // Añadir otras propiedades en caso de usarlas en frontend
        // Como ángulos óptimos calculados por PVGIS si useoptimalangles es true
        public double? OptimalInclinationAngle { get; set; }
        public double? OptimalAzimuthAngle { get; set; }
    }
}