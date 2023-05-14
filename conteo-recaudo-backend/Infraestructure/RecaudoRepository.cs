using ConteoRecaudo.DAL;
using ConteoRecaudo.Entities;
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

        public async Task<int> GuardarRecaudo(RecaudoEntity recaudo) {
            try {
                _context.Add(recaudo);
                await _context.SaveChangesAsync();
                return recaudo.Id;
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ConteoRecaudoModel>> GetRecaudos()
        {
            try
            {
                List<ConteoRecaudoModel> recaudos = await (from recaudo in _context.Recaudos
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
