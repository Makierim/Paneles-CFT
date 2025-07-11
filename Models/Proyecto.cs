using System;
using System.Collections.Generic;

namespace Paneles_CFT.Models;

public partial class Proyecto
{
    public int IdProyecto { get; set; }

    public string? RunInstaladorProyecto { get; set; }

    public string? RunClienteProyecto { get; set; }

    public string? NombreProyecto { get; set; }

    public DateOnly? FechaInicioProyecto { get; set; }

    public DateOnly? FechaTerminoProyecto { get; set; }

    public string? DireccionProyecto { get; set; }

    public string? ResumenDatosProyecto { get; set; }

    public virtual ICollection<ProyectosComponentesMap> ProyectosComponentesMaps { get; set; } = new List<ProyectosComponentesMap>();

    public virtual ICollection<ProyectosNormativasMap> ProyectosNormativasMaps { get; set; } = new List<ProyectosNormativasMap>();

    public virtual Cliente? RunClienteProyectoNavigation { get; set; }

    public virtual Instalador? RunInstaladorProyectoNavigation { get; set; }
}
