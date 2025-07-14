using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Paneles_CFT.Models
{
    // Modelo principal para la respuesta de la herramienta "seriescalc" de PVGIS
    // Devuelve datos de series de tiempo (horarios o diarios)
    public class PvgisSeriesCalcResponse
    {
        public PvgisInputsSeriesCalc Inputs { get; set; }
        // Datos de la serie de tiempo
        public PvgisOutputsSeriesCalc Outputs { get; set; }
        public PvgisMetaSeriesCalc Meta { get; set; }
    }

    // - Clases para la sección inputs -
    public class PvgisInputsSeriesCalc
    {
        public PvgisLocationSeriesCalc Location { get; set; }
        public PvgisMeteoDataSeriesCalc Meteo_Data { get; set; }
        public PvgisMountingPlaceSeriesCalc Mounting_Place { get; set; }
        // seriescalc no tiene Pv_Module en sus inputs, ya que no simula un sistema completo
    }

    public class PvgisLocationSeriesCalc
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Elevation { get; set; }
    }

    public class PvgisMeteoDataSeriesCalc
    {
        public string Radiation_Db { get; set; }
        public string Meteo_Db { get; set; }
        public int Year_Min { get; set; }
        public int Year_Max { get; set; }
        public bool Use_Horizon { get; set; }
        public object Horizon_Db { get; set; }
        public string Horizon_Data { get; set; }
        public string Optimal_Angle { get; set; }
    }

    public class PvgisMountingPlaceSeriesCalc
    {
        public string Type { get; set; } // "free o "building"
        // Ángulo de inclinación del panel
        public double Slope { get; set; }
        // Azimut del panel
        public double Azimuth { get; set; }
        public string Building_Integrated { get; set; } // "NO" o "YES"
    }

    // - Clases para la sección outputs -
    public class PvgisOutputsSeriesCalc
    {
        // Propiedad clave: lista de objetos que contienen los datos de cada punto de tiempo
        public List<PvgisDataPoint> Tmy { get; set; } // 'Tmy' es un nombre común en PVGIS para datos de series de tiempo

        // Propiedad para capturar ángulos óptimos si se devuelven
        public PvgisOptimalAngles Optimal_Angles { get; set; }
    }

    // Clase para cada punto de datos en la series de tiempo (horario o diario)
    // Las propiedades reflejan los campos comunes de la repuesta 'seriescalc'
    public class PvgisDataPoint
    {
        // Fecha y Hora en formato ISO (YYYYMMDD:HHMM)
        public string Time { get; set; }
        // Irradiación global horizontal (W/m²)
        public double G_hor { get; set; }
        // Irradiación global en el plano inclinado (W/m²)
        public double G_tilt { get; set; }
        // Irradiación difusa horizontal(W/m²)
        public double Diff_hor { get; set; }
        // Irradiación difusa en el plano inclinado (W/m²)
        public double Diff_tilt { get; set; }
        // Temperatura del aire a 2 metros (Celsius)
        public double Temp { get; set; }
        // Velocidad del viento a 10 metros (m/s)
        public double Wsp { get; set; }
        // Espacio para más datos en caso de necesitarse
    }

    // Clase para representar los ángulo óptimos devueltos por PVGIS
    public class PvgisOptimalAngles
    {
        // Ángulo de iclinación óptimo
        public PvgisOptimalAngleItem Angle { get; set; }
        // Ázimut óptimo
        public PvgisOptimalAngleItem Aspect { get; set; }
    }

    // Clase auxiliar para un elemento de ángulo óptimo
    public class PvgisOptimalAngleItem
    {
        public double Value { get; set; }
        public string Description { get; set; }
        public string Units { get; set; }
    }

    // - Clases para la sección meta -
    public class PvgisMetaSeriesCalc
    {
        public PvgisMetaInputsSeriesCalc Inputs { get; set; }
        public PvgisMetaOutputsSeriesCalc Outputs { get; set; }
    }

    public class PvgisMetaInputsSeriesCalc
    {
        public PvgisMetaLocationSeriesCalc Location { get; set; }
        public PvgisMetaMeteoDataSeriesCalc Meteo_Data { get; set; }
        public PvgisMetaMountingPlaceSeriesCalc Mounting_Place { get; set; }
    }

    public class PvgisMetaLocationSeriesCalc
    {
        public string Description { get; set; }
        public PvgisMetaVariablesSeriesCalc Variables { get; set; }
    }

    public class PvgisMetaMeteoDataSeriesCalc
    {
        public string Description { get; set; }
        public PvgisMetaVariablesSeriesCalc Variables { get; set; }
    }

    public class PvgisMetaMountingPlaceSeriesCalc
    {
        public string Description { get; set; }
        public PvgisMetaFieldsSeriesCalc Fields { get; set; }
    }

    public class PvgisMetaVariablesSeriesCalc
    {
        public PvgisMetaVariableItemSeriesCalc Latitude { get; set; }
        public PvgisMetaVariableItemSeriesCalc Longitude { get; set; }
        public PvgisMetaVariableItemSeriesCalc Elevation { get; set; }
        public PvgisMetaVariableItemSeriesCalc Radiation_Db { get; set; }
        public PvgisMetaVariableItemSeriesCalc Meteo_Db { get; set; }
        public PvgisMetaVariableItemSeriesCalc Year_Min { get; set; }
        public PvgisMetaVariableItemSeriesCalc Year_Max { get; set; }
        public PvgisMetaVariableItemSeriesCalc Use_Horizon { get; set; }
        public PvgisMetaVariableItemSeriesCalc Horizon_Db { get; set; }
        public PvgisMetaVariableItemSeriesCalc Horizon_Data { get; set; }
        public PvgisMetaVariableItemSeriesCalc Optimal_Angle { get; set; }
    }

    public class PvgisMetaFieldsSeriesCalc
    {
        public PvgisMetaVariableItemSeriesCalc Type { get; set; }
        public PvgisMetaVariableItemSeriesCalc Slope { get; set; }
        public PvgisMetaVariableItemSeriesCalc Azimuth { get; set; }
        public PvgisMetaVariableItemSeriesCalc Building_Integrated { get; set; }
    }

    public class PvgisMetaVariableItemSeriesCalc
    {
        public string Description { get; set; }
        public string Units { get; set; }
    }

    public class PvgisMetaOutputsSeriesCalc
    {
        public PvgisMetaOutputItemSeriesCalc Tmy { get; set; }
    }

    public class PvgisMetaOutputItemSeriesCalc
    {
        public string Type { get; set; }
        public string Timestamp { get; set; }
        public PvgisMetaVariablesSeriesCalc_Output Variables { get; set; }
    }

    public class PvgisMetaVariablesSeriesCalc_Output
    {
        public PvgisMetaVariableItemSeriesCalc Time { get; set; }
        public PvgisMetaVariableItemSeriesCalc G_hor { get; set; }
        public PvgisMetaVariableItemSeriesCalc G_tilt { get; set; }
        public PvgisMetaVariableItemSeriesCalc Diff_hot { get; set; }
        public PvgisMetaVariableItemSeriesCalc Diff_tilt { get; set; }
        public PvgisMetaVariableItemSeriesCalc Temp { get; set; }
        public PvgisMetaVariableItemSeriesCalc Wsp { get; set; }
        // Agregar aquí variables extras mapeadas en PvgisDataPoint
    }
}
