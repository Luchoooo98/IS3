using InconsistenciasApi.Models.Inconsistencias.Classes;
using System;
using System.Collections.Generic;

namespace InconsistenciasApi.Models.Inconsistencias.Responses
{
    public class ArchivoGuardadoResponse
    {
        public string NombreArchivo { get; set; }
        public double Tamanio { get; set; }
        public int IdInsercion { get; set; }
    }

    public class ArchivoDetalleResponse
    {
        public string NombreArchivo { get; set; }
        public string Usuario { get; set; }
        public int IdInsercion { get; set; }
        public DateTime FechaAlta { get; set; }
        public double Tamanio { get; set; }
        public List<Rule> Reglas { get; set; }
        public ReglasProcesadoResponse ReglasArchivo { get; set; } 
        public List<ArchivoProcesadoResponse> ResultadoArchivo { get; set; } = new List<ArchivoProcesadoResponse>();
    }
}
