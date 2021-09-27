using System.ComponentModel.DataAnnotations;

namespace InconsistenciasApi.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Usuario { get; set; }
        [Required]
        public string Contraseña { get; set; }
        public string Mail { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
    }
}
