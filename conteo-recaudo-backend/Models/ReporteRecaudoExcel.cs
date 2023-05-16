using System.Numerics;

namespace ConteoRecaudo.Models
{
    public class ReporteRecaudoExcel
    {
        public DateTime FechaRecaudo { get; set; }
        public List<EstacionReporteModel>? Estaciones { get; set; }
    }
}
