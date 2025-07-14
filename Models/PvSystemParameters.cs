namespace Paneles_CFT.Models
{

    // Clase que representa los parámetros que ingresa el usuario para calcular la producción
    // de un sistema fotovoltaico con la API de PVGIS (PVcalc)
    public class PvSystemParameters
    {
        // Latitud de la ubicación seleccionada en el mapa
        public double Latitude { get; set; }
        // Longitud de la ubicación seleccionada en el mapa
        public double Longitude { get; set; }

        // Potencia pico nominal del sistema fotovoltaico en kWp (kilowatts-pico)
        // Un valor de 3.0 significaria un sistema de 3 kWp
        public double PeakPower { get; set; }

        // Porcentaje de pérdidas totales del sistema (cableado, sombreado, temperatua, etc)
        // Un valor de 14.0 significaría un 14% de pérdidas
        public double SystemLosses { get; set; }

        // Ángulo de inclinación de los paneles fotovoltaivos repecto a la horizontal, en grados
        // En 'nullable' porque el usuario puede elegir que PVGIS calcule el ángulo óptimo
        public double? InclinationAngle { get; set; }

        // Ángulo de aizmut de los paneles fotovoltaicos en grados
        // Es 'nullable' también
        // 0 grados normalmente es Sut en el Hemisferio Norte, o Norte en el Hemisferio Sur
        // PVGIS usa 0 para el Sur
        public double? AzimuthAngle { get; set; }

        // Flag booleana que indica si se le debe pedir a PVGIS que calcule los ángulos óptimos
        // de inclinación y azimut para la ubicación
        // Si es true, InclinationAngle y AzimuthAngle pueden ser ignorados por PVGIS
        public bool GetOptimalAngles { get; set; } = false; // Valor por defecto
    }
}
