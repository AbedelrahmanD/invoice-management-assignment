using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManagement.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Must be at least 1")]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Must be greater than 0")]
        public decimal Price { get; set; }
        public int InvoiceId { get; set; }

        [NotMapped]
        public decimal LineTotal => Quantity * Price;

    }
}
