using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InconsistenciasApi.Models.Entities
{
    public class Archivo
    {
        public int Id { get; set; }

        [Required]
        public DateTime Alta { get; set; }
        [Required]
        public string Usuario { get; set; }
        [Required]
        public string NombreArchivo { get; set; }
        [Required]
        public string HashArchivo { get; set; }
        [Required]
        public string ContenidoArchivo { get; set; }
        [Required]
        public double Tamanio { get; set; }
        [Required]
        public byte[] Bytes { get; set; }
        public List<ReglasArchivo> ReglasArchivo { get; set; } = new List<ReglasArchivo>();
        public List<ResultadoArchivo> ResultadoArchivo { get; set; } = new List<ResultadoArchivo>();
    }
}
