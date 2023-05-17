namespace ConteoRecaudo.Models
{
    public class ResponseReacudoModel
    {
        public int PaginaActual { get; set; }
        public int TotalRegistros { get; set; }
        public int RegistrosPorPagina { get; set; }
        public List<ConteoRecaudoModel>? ConteoRecaudoList { get; set; }
    }
}
