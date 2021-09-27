using System.ComponentModel.DataAnnotations;

namespace InconsistenciasApi.Models.Entities
{
    public class ResultadoArchivo
    {
        public int Id { get; set; }

        [Required]
        public string Reglas { get; set; }
        [Required]
        public string Resultado { get; set; }
        [Required]
        public string TipoProcesamiento { get; set; }

        public int ArchivoId { get; set; }
        //public Archivo Archivo { get; set; }
    }
}
