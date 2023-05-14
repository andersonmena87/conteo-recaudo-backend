
using ConteoRecaudo.Entities;
using ConteoRecaudo.Models;

namespace ConteoRecaudo.Helpers.Converts
{
    public class ConvertRecaudo
    {
        public static ConteoRecaudoModel ToModel(RecaudoEntity recaudo) {
            return new()
            {
                Id = recaudo.Id,
                Estacion = recaudo.Estacion,
                Sentido = recaudo.Sentido,
                Hora = recaudo.Hora,
                Categoria = recaudo.Categoria,
                ValorTabulado = recaudo.ValorTabulado,
                Cantidad = recaudo.Cantidad,
                FechaRecaudo = recaudo.FechaRecaudo
            };
        }

        public static RecaudoEntity ToEntity(ConteoRecaudoModel recaudo)
        {
            return new()
            {
                Id = recaudo.Id,
                Estacion = recaudo.Estacion,
                Sentido = recaudo.Sentido,
                Hora = recaudo.Hora,
                Categoria = recaudo.Categoria,
                ValorTabulado = recaudo.ValorTabulado,
                Cantidad = recaudo.Cantidad,
                FechaRecaudo = recaudo.FechaRecaudo
            };
        }
    }
}
