using System;
using System.Collections.Generic;

namespace Paneles_CFT.Models;

public partial class Rol
{
    public int IdRol { get; set; }

    public string? NombreRol { get; set; }

    public virtual ICollection<Instalador> Instaladores { get; set; } = new List<Instalador>();
}
