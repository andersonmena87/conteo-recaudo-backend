using conteo_recaudo_backend.DAL;
using conteo_recaudo_backend.Entities;
using ConteoRecaudo.Infraestructure.Interfaces;
using ConteoRecaudo.Models;
using Microsoft.EntityFrameworkCore;

namespace ConteoRecaudo.Infraestructure
{
    public class RecaudoRepository : IRecaudoRepository
    {
        private readonly AppDbContext _context;

        public RecaudoRepository(AppDbContext appDbContext) { 
            _context = appDbContext;
        }

        public async Task<int> GuardarRecaudo(RecaudoModel recaudo) {
            try {
                RecaudoEntity nuevoRecaudo = new RecaudoEntity {
                    Estacion = recaudo.Estacion,
                    Sentido = recaudo.Sentido,
                    Hora = recaudo.Hora,
                    Categoria = recaudo.Categoria,
                    ValorTabulado = recaudo.ValorTabulado,
                    Cantidad = recaudo.Cantidad,
                    FechaRecaudo =  recaudo.FechaRecaudo
                };

                _context.Add(nuevoRecaudo);
                await _context.SaveChangesAsync();
                return nuevoRecaudo.Id;
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<RecaudoModel>> GetRecaudos()
        {
            try
            {
                List<RecaudoModel> recaudos = await (from recaudo in _context.Recaudos
                                            select new RecaudoModel
                                            {
                                                Id = recaudo.Id,
                                                Estacion = recaudo.Estacion,
                                                Sentido = recaudo.Sentido,
                                                Hora = recaudo.Hora,
                                                Categoria = recaudo.Categoria,
                                                ValorTabulado = recaudo.ValorTabulado,
                                                Cantidad = recaudo.Cantidad,
                                                FechaRecaudo = recaudo.FechaRecaudo
                                            }).OrderBy(n => n.FechaRecaudo).ToListAsync();
                return recaudos;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
