using System;
using System.Collections.Generic;

namespace Paneles_CFT.Models;

public partial class Componente
{
    public string CodigoComponente { get; set; } = null!;

    public string? NombreComponenbte { get; set; }

    public string? TipoComponente { get; set; }

    public string? MarcaComponente { get; set; }

    public string? ModeloComponente { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<ProyectosComponentesMap> ProyectosComponentesMaps { get; set; } = new List<ProyectosComponentesMap>();
}
