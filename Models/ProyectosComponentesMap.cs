using System;
using System.Collections.Generic;

namespace Paneles_CFT.Models;

public partial class ProyectosComponentesMap
{
    public int IdProyectosComponentes { get; set; }

    public int? IdProyectoPsCs { get; set; }

    public string? CodigoComponentePsCs { get; set; }

    public int? Cantidad { get; set; }

    public DateOnly? FechaInstalacionComponente { get; set; }

    public string? EstadoUsoComponente { get; set; }

    public virtual Componente? CodigoComponentePsCsNavigation { get; set; }

    public virtual Proyecto? IdProyectoPsCsNavigation { get; set; }
}
