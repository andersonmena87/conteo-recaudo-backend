using ConteoRecaudo.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConteoRecaudo.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RecaudoEntity> Recaudos { get; set; }
    }
}
