namespace ConteoRecaudo.Models
{
    public class RecaudoModel
    {
        public int Id { get; set; }

        public string? Estacion { get; set; }

        public string? Sentido { get; set; }

        public int Hora { get; set; }

        public string? Categoria { get; set; }

        public int ValorTabulado { get; set; }

        public int Cantidad { get; set; }

        public DateTime FechaRecaudo { get; set; }
    }
}
