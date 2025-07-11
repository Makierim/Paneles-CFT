using System;
using System.Collections.Generic;

namespace Paneles_CFT.Models;

public partial class Cliente
{
    public string RunCliente { get; set; } = null!;

    public string? UsernameCliente { get; set; }

    public string? NombresCliente { get; set; }

    public string? DireccionCliente { get; set; }

    public string? TelefonoCliente { get; set; }

    public virtual ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}
