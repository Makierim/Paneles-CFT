using System;
using System.Collections.Generic;

namespace Paneles_CFT.Models;

public partial class Normativa
{
    public string CodigoNormativa { get; set; } = null!;

    public string? TituloNormativa { get; set; }

    public string? DescripcionUrlNormativa { get; set; }

    public string? TipoNormativa { get; set; }

    public string? RegionNormativa { get; set; }

    public string? ComunaNormativa { get; set; }

    public string? InstitucionNormativa { get; set; }

    public DateOnly? FechaPublicacionNormativa { get; set; }

    public DateOnly? FechaInicioVigencia { get; set; }

    public DateOnly? FechaFinVigencia { get; set; }

    public virtual ICollection<ProyectosNormativasMap> ProyectosNormativasMaps { get; set; } = new List<ProyectosNormativasMap>();
}
