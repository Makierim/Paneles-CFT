using System.ComponentModel.DataAnnotations;

namespace Paneles_CFT.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Usuario o RUN")]
        [Required(ErrorMessage = "Este campo es obligatorio")]
        public string LoginInput { get; set; } = null!;

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña campo es obligatoria")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = null!;

        [Display(Name = "Recordarme")]
        public bool MantenerSesion { get; set; }
    }
}