using System.ComponentModel.DataAnnotations;

namespace InconsistenciasApi.Models.Entities
{
    public class ReglasArchivo
    {
        public int Id { get; set; }

        [Required]
        public string Regla { get; set; }

        public int ArchivoId { get; set; }
        //public Archivo Archivo { get; set; }
    }
}
