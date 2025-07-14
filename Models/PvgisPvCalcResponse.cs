namespace Paneles_CFT.Models
{
    // Clase para manejar la respuesta JSON completa de la API de PVGIS al usar PVcalc
    // 3 secciones principales: inputs, outputs y meta
    public class PvgisPvCalcResponse
    {
        // inputs replica los parámetros que se enviarán a la API para confirmar qué configuración usó PVGIS
        public PvgisInputsPvCalc Inputs { get; set; }

        // La sección outputs contiene los resultados clave de la simulación, producción de energía y la irradiación
        public PvgisOutputsPvCalc Outputs { get; set; }

        // La sección meta proporciona metadatos sobre los campos como descripciones y unidades
        public PvgisMetaPvCalc Meta { get; set; }
    }

    // - Clases para la sección inputs -
    // Representar estructura de la sección inputs dentro de la respuesta PVcalc
    public class PvgisInputsPvCalc
    {
        // Contiene la latitud, longitud y elevación usadas
        public PvgisLocationPvCalc Location { get; set; }
        // Detalla la base de datos metereológica para la simulación
        public PvgisMeteoDataPvCalc Meteo_Data { get; set; }
        // Describe la configuración de montaje de los paneles (inclinación, azimut, tipo)
        public PvgisMountingPlacePvCalc Mounting_Place { get; set; }
        // Especificaciones del sistema fotovoltaico (potencia pico, pérdidad)
        public PvgisPVModulePvCalc Pv_Module { get; set; }
    }

    // Clase para los detalles de la ubicación dentro de inputs
    public class PvgisLocationPvCalc
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        // Elevación del sitio en metros
        public double Elevation { get; set; }
    }

    // Clase para los detalles metereológicos dentro de inputs
    public class PvgisMeteoDataPvCalc
    {
        // Base de datos para radiación solar 'PVGIS-ERA5' para Chile
        public string Radiation_Db { get; set; }
        // Base de datos metereológica 'ERA5'
        public string Meteo_Db { get; set; }
        // Primer año del rango de datos usados
        public int Year_Min { get; set; }
        // Último año del rango de datos usados
        public int Year_Max { get; set; }
        // Indica si se usó la información del horizonte para sombreado
        public bool Use_Horizon { get; set; }
        // Indica si se usó la información del horizonte para sombreado, 'object' porque a veces puede ser null o una cadena
        public object Horizon_Db { get; set; }
        // Fuente de los datos del horizonte 'DEM-calculated'
        public string Horizon_Data { get; set; }
    }

    // Clase para los detalles del montaje del sistema PV
    public class PvgisMountingPlacePvCalc
    {
        // Tipo de montaje "free" (sobre el suelo) o "building" (integrado en edificio)
        public string Type { get; set; }
        // Ángulo de inclinación (slope) usado para los paneles (grados)
        public double Slope { get; set; }
        // Ángulo de azimut usado para los paneles (grados)
        public double Azimuth { get; set; }
        // Indica si el sistema está integrado en un edificio ("NO" o "YES")
        public string Building_Integrated { get; set; }
        // "YES" si PVGIS calculó y usó el ángulo óptimo para la inclinación
        public string Angle_Opt { get; set; }
    }

    // Clase para los detalles del módulo FV dentro de inputs
    public class PvgisPVModulePvCalc
    {
        // Potencia pico del sistema kWp (kilowatts-pico)
        public double Peak_Power { get; set; }
        // Pérdidas estimadas del sistema en porcentaje
        public double System_Loss { get; set; }
    }

    // - Clases para la sección outputs -
    // Representar la estructura de la sección outputs de la respuesta PVGIS PVcalc
    public class PvgisOutputsPvCalc
    {
        // Contiene los resultados anuales consolidados
        public PvgisAnnualPvCalc Annual { get; set; }
        // Una lista de objetos, donde cada uno representa los resultados para un mes específico
        public List<PvgisMonthlyPvCalc> Monthly { get; set; }
    }

    // Clase para los resultados anuales dentro outputs
    public class PvgisAnnualPvCalc
    {
        // Producción de energía eléctrica anual total del sistema FV en kWh
        public double E_PV { get; set; }
        // Irradiación global anual en el plano del array FV en kWh/m²
        public double E_G { get; set; }

        // Se definen las diferentes categorías de pérdidas que PVGIS puede desglosar en el cálculo anual (en %)
        // Pérdidas por temperatura
        public double L_tg { get; set; }
        // Pérdidas por irradiancia (comportamiento de la celda a baja luz)
        public double L_gr { get; set; }
        // Pérdidas por ángulo de incidencia (rayos no perpendiculares)
        public double L_aoi { get; set; }
        // Pérdidas espectrales (efecto del espectro solar en la eficiencia)
        public double L_spec { get; set; }
        // Pérdidas por sombreado (obstáculos que bloquean el sol)
        public double L_sh { get; set; }
        // Pérdidas por suciedad (polvo, nieve, etc)
        public double L_soiling { get; set; }
        // Pérdidas por eficiencia del módulo
        public double L_eff { get; set; }
        // Pérdidas por desajuste entre módulos
        public double L_mismatch { get; set; }
        // Pérdidas en los cables
        public double L_cables { get; set; }
        // Pérdidas del inversor
        public double L_inv { get; set; }
        // Pérdidas totales acumuladas
        public double L_total { get; set; }
    }

    // Clase para los resultados mensuales dentro de outputs
    public class PvgisMonthlyPvCalc
    {
        // Número del mes (1-12)
        public int Month { get; set; }
        // Producción de energía eléctrica mensual del sistema FV en kWh
        public double E_PV { get; set; }
        // Irradiación global mensual en el plano del array FV en kWh/m²
        public double E_G { get; set; }
    }

    // - Clases para la seccion meta -
    // Representar la seccion meta que describe los datos
    public class PvgisMetaPvCalc
    {
        public PvgisMetaInputsPvCalc Inputs { get; set; }
        public PvgisMetaOutputsPvCalc Outputs { get; set; }
    }

    // Meta-información sobre la sección inputs
    public class PvgisMetaInputsPvCalc
    {
        public PvgisMetaLocationPvCalc Location { get; set; }
        public PvgisMetaMeteoDataPvCalc Meteo_Data { get; set; }
        public PvgisMetaMountingPlacePvCalc Mounting_Place { get; set; }
        public PvgisMetaPVModulePvCalc Pv_Module { get; set; }
    }

    // Meta-información sobre la ubicación
    public class PvgisMetaLocationPvCalc
    {
        public string Description { get; set; }
        public PvgisMetaVariablesPvCalc Variables { get; set; }
    }

    // Meta-información sobre los datos metereológicos
    public class PvgisMetaMeteoDataPvCalc
    {
        public string Description { get; set; }
        public PvgisMetaVariablesPvCalc Variables { get; set; }
    }

    // Meta-información sobre el montaje del sistema
    public class PvgisMetaMountingPlacePvCalc
    {
        public string Description { get; set; }
        // En PVcalc, 'fields' para montaje
        public PvgisMetaFieldsPvCalc Fields { get; set; }
    }

    // Meta-información sobre el módulo FV
    public class PvgisMetaPVModulePvCalc
    {
        public string Description { get; set; }
        // En PVcalc, 'fields' para módulo
        public PvgisMetaFieldsPvCalc Fields { get; set; }
    }

    // Reutilizable: describe una colección de variables y sus metadatos
    public class PvgisMetaVariablesPvCalc
    {
        public PvgisMetaVariableItemPvCalc Latitude { get; set; }
        public PvgisMetaVariableItemPvCalc Longitude { get; set; }
        public PvgisMetaVariableItemPvCalc Elevation { get; set; }
        public PvgisMetaVariableItemPvCalc Radiation_Db { get; set; }
        public PvgisMetaVariableItemPvCalc Meteo_Db { get; set; }
        public PvgisMetaVariableItemPvCalc Year_Min { get; set; }
        public PvgisMetaVariableItemPvCalc Year_Max { get; set; }
        public PvgisMetaVariableItemPvCalc Use_Horizon { get; set; }
        public PvgisMetaVariableItemPvCalc Horizon_Db { get; set; }
        public PvgisMetaVariableItemPvCalc Horizon_Data { get; set; }
    }

    // Reutilizable: describe una colección de campos y sus metadatos
    public class PvgisMetaFieldsPvCalc {
        public PvgisMetaVariableItemPvCalc Type { get; set; }
        public PvgisMetaVariableItemPvCalc Slope { get; set; }
        public PvgisMetaVariableItemPvCalc Azimuth { get; set; }
        public PvgisMetaVariableItemPvCalc Building_Integrated { get; set; }
        public PvgisMetaVariableItemPvCalc Peak_Power { get; set; }
        public PvgisMetaVariableItemPvCalc System_Loss { get; set; }
    }

    // Reutilizable: describe un único elemento de variable con su descripción y unidades
    public class PvgisMetaVariableItemPvCalc
    {
        public string Description { get; set; }
        public string Units { get; set; }
    }

    // Meta-información sobre la sección outputs
    public class PvgisMetaOutputsPvCalc
    {
        public PvgisMetaOutputItemPvCalc Annual { get; set; }
        public PvgisMetaOutputItemPvCalc Monthly { get; set; }
    }

    // Reutilizable: describe un elemento de salida (anual o mensual)
    public class PvgisMetaOutputItemPvCalc
    {
        // Tipo de serie de tiempo (ej "time series")
        public string Type { get; set; }
        // Frecuencia de los datos (ej "monthly averages")
        public string Timestamp { get; set; }
        // Variables de salida
        public PvgisMetaVariablesPvCalc_Output Variables { get; set; }
    }

    // Reutilizable: describe las variables de salida específicas (ej. E_PV, E_G, pérdidas)
    public class PvgisMetaVariablesPvCalc_Output
    {
        public PvgisMetaVariableItemPvCalc E_PV { get; set; }
        public PvgisMetaVariableItemPvCalc E_G { get; set; }
        // Descripciones para las propiedad de pérdidas
        public PvgisMetaVariableItemPvCalc L_tg { get; set; }
        public PvgisMetaVariableItemPvCalc L_gr { get; set; }
        public PvgisMetaVariableItemPvCalc L_aoi { get; set; }
        public PvgisMetaVariableItemPvCalc L_spec { get; set; }
        public PvgisMetaVariableItemPvCalc L_sh { get; set; }
        public PvgisMetaVariableItemPvCalc L_soiling { get; set; }
        public PvgisMetaVariableItemPvCalc L_eff { get; set; }
        public PvgisMetaVariableItemPvCalc L_mismatch { get; set; }
        public PvgisMetaVariableItemPvCalc L_cables { get; set; }
        public PvgisMetaVariableItemPvCalc L_inv { get; set; }
        public PvgisMetaVariableItemPvCalc L_total { get; set; }
    }
}