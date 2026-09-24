using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace InvoiceManagement.Models
{
    [ValidatableType]
    public class Invoice
    {
        public int Id { get; set; }
        public int Number { get; set; }

        [Required(ErrorMessage ="Required")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Required")]
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
      
         public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}
