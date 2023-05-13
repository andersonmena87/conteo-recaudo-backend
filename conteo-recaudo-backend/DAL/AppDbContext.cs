using conteo_recaudo_backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace conteo_recaudo_backend.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RecaudoEntity> Recaudos { get; set; }
    }
}
