using ConteoRecaudo.Models;

namespace ConteoRecaudo.BLL.Interfaces
{
    public interface IRecaudoBL
    {
        Task<ResponseReacudoModel> GetRecaudos(int pagina);

        Task<bool> GuardarRecaudos(string token, DateTime fechaIncio, DateTime fechaFin);

        Task<ArchivoRecaudoExcel> ExportarExcel(DateTime fechaInicial, DateTime fechaFinal);
    }
}
