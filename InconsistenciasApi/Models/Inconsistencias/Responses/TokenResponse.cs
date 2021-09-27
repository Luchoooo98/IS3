using System;

namespace InconsistenciasApi.Models.Inconsistencias.Responses
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Error { get; set; }
    }
}
