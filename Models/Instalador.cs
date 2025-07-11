using System;
using System.Collections.Generic;

namespace Paneles_CFT.Models;

public partial class Instalador
{
    public string RunInstalador { get; set; } = null!;

    public string? UsernameInstalador { get; set; }

    public string? NombresInstalador { get; set; }

    public string? ApellidosInstalador { get; set; }

    public string? DireccionInstalador { get; set; }

    public string? TelefonoInstalador { get; set; }

    public int? SueldoInstalador { get; set; }

    public string? ContrasenaInstalador { get; set; }

    public int? IdRolInstalador { get; set; }

    public virtual Rol? IdRolInstaladorNavigation { get; set; }

    public virtual ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}
