namespace Paneles_Fotovoltaicos.Models
{
    public class SolarCalculator
    {
        // Parámetros de entrada
        public double Latitude { get; set; } // Latitud
        public double Longitude { get; set; } // Longitud
        public double Azimuth { get; set; } // Azimut
        public double TiltAngle { get; set; } // Ángulo de inclicación
        public double SolarRadiation { get; set; } // Radiación incidente (Ipoa)
        public double CellTemperatue { get; set; } // Temperatura de la cela (Tc)
        public double PanelEfficiency { get; set; } // Eficiencia del panel
        public double PanelArea { get; set; } // Área total del panel (m)
        public int NumberOfPanels { get; set; } // Número de paneles
        public double InverterEfficiency { get; set; } // Eficiencia del inversor

        // Resultados del Cálculo
        public double DCOutput { get; set; }
        public double ACOutput { get; set; }

        // Cálculos para la radiación incidente sobre el panel
        public double CalculateIncidentRadiation()
        {
            // Fórmulas para radiación directa, difussa y suelo
            double dirInc = SolarRadiation * Math.Cos(Math.PI / 180 * Latitude); // Radiación directa
            double difInc = SolarRadiation * 0.5 * (1 + Math.Cos(Math.PI / 180 * TiltAngle)); // Radiación difusa
            double difSuelo = SolarRadiation * 0.5 * 0.24 * (1 + Math.Cos(Math.PI / 180 * TiltAngle)); // Radiación reflejada del suelo

            return dirInc + difInc + difSuelo;
        }

        // Calcular la potencia DC
        public double CalculateDCOutput()
        {
            double incidentRadiation = CalculateIncidentRadiation();
            double nominalPower = PanelArea * NumberOfPanels * PanelEfficiency;

            return (incidentRadiation / 1000) * nominalPower;
        }

        // Calcular la potencia AC
        public double CalculateACOutput()
        {
            double dcPower = CalculateDCOutput();
            return dcPower * InverterEfficiency;
        }
    }
}
