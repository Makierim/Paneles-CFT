using System;
using System.Collections.Generic;

namespace Paneles_CFT.Models;

public partial class ProyectosNormativasMap
{
    public int IdProyectosNormativas { get; set; }

    public int? IdProyectoPsNs { get; set; }

    public string? CodigoNormativaPsNs { get; set; }

    public bool? EsObligatoria { get; set; }

    public virtual Normativa? CodigoNormativaPsNsNavigation { get; set; }

    public virtual Proyecto? IdProyectoPsNsNavigation { get; set; }
}
