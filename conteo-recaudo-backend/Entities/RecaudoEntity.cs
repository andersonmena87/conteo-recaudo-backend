using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ConteoRecaudo.Entities
{
    public class RecaudoEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("estacion")]
        public string? Estacion { get; set; }

        [Column("sentido")]
        public string? Sentido { get; set; }

        [Column("hora")]
        public int Hora { get; set; }

        [Column("categoria")]
        public string? Categoria { get; set; }

        [Column("valorTabulado")]
        public int ValorTabulado { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("fechaRecaudo")]
        public DateTime FechaRecaudo { get; set; }
    }
}
