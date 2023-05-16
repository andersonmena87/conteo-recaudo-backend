using System.Numerics;

namespace ConteoRecaudo.Models
{
    public class EstacionReporteModel
    {
        public string? Estacion { get; set; }
        public BigInteger TotalCantidad { get; set; }
        public BigInteger TotalValorTabulado { get; set; }
    }
}
