using System.Collections.Generic;

namespace InconsistenciasApi.Models.Inconsistencias.Responses
{
    public class ArchivoProcesadoResponse
    {
        public List<string> Reglas { get; set; }
        public List<string> Resultado { get; set; }
        public string TipoProcesamiento { get; set; }
    }

    public class ReglasProcesadoResponse
    {
        public List<string> Reglas { get; set; } = new List<string>();
    }
}
