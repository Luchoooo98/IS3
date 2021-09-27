namespace InconsistenciasApi.Models.Inconsistencias.Classes
{
    public class Rule
    {

        public Rule() { }

        public int Id { get; set; }
        public string Antecedent { get; set; }
        public string Consequent { get; set; }
    }
}
