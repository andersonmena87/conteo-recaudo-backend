using ConteoRecaudo.DAL;
using ConteoRecaudo.Entities;
using ConteoRecaudo.Helpers.Converts;
using ConteoRecaudo.Infraestructure.Interfaces;
using ConteoRecaudo.Models;
using Microsoft.EntityFrameworkCore;

namespace ConteoRecaudo.Infraestructure
{
    public class RecaudoRepository : IRecaudoRepository
    {
        private readonly AppDbContext _context;

        public RecaudoRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<int> GuardarRecaudo(RecaudoEntity recaudo)
        {
            try
            {
                _context.Add(recaudo);
                await _context.SaveChangesAsync();
                return recaudo.Id;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResponseReacudoModel> GetRecaudos(int pagina = 1, int cantidadRegistros = 100)
        {
            try
            {
                ResponseReacudoModel model = new();
                model.PaginaActual = pagina;

                var recaudos = from r in _context.Recaudos
                               select r;

                model.TotalRegistros = recaudos.Count();
                model.RegistrosPorPagina = cantidadRegistros;
                model.ConteoRecaudoList = await (from recaudo in recaudos
                                                 select new ConteoRecaudoModel
                                                 {
                                                     Id = recaudo.Id,
                                                     Estacion = recaudo.Estacion,
                                                     Sentido = recaudo.Sentido,
                                                     Hora = recaudo.Hora,
                                                     Categoria = recaudo.Categoria,
                                                     ValorTabulado = recaudo.ValorTabulado,
                                                     Cantidad = recaudo.Cantidad,
                                                     FechaRecaudo = recaudo.FechaRecaudo
                                                 }).OrderBy(n => n.FechaRecaudo)
                                                           .Skip((pagina - 1) * cantidadRegistros)
                                                           .Take(cantidadRegistros)
                                                           .ToListAsync(); ;

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ReporteRecaudoExcel>> ObtenerRecaudosxFechas(DateTime fechaInicial, DateTime fechaFinal)
        {
            try
            {
                var query = from r in _context.Recaudos
                            where r.FechaRecaudo >= fechaInicial && r.FechaRecaudo <= fechaFinal
                            select r;

                var recaudos = await query.ToListAsync();

                var grouped = recaudos.GroupBy(r => new { r.FechaRecaudo, r.Estacion })
                                      .Select(g => new
                                      {
                                          FechaRecaudo = g.Key.FechaRecaudo,
                                          Estacion = g.Key.Estacion,
                                          TotalCantidad = g.Sum(r => r.Cantidad),
                                          TotalValorTabulado = g.Sum(r => r.ValorTabulado)
                                      })
                                      .ToList();

                var groupedByFecha = grouped.GroupBy(g => g.FechaRecaudo)
                                            .Select(g => new ReporteRecaudoExcel
                                            {
                                                FechaRecaudo = g.Key,
                                                Estaciones = g.Select(e => new EstacionReporteModel
                                                {
                                                    Estacion = e.Estacion,
                                                    TotalCantidad = e.TotalCantidad,
                                                    TotalValorTabulado = e.TotalValorTabulado
                                                }).ToList()
                                            })
                                            .ToList();

                return groupedByFecha;

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
