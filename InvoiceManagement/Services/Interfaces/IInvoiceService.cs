using InvoiceManagement.Models;
namespace InvoiceManagement.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<List<Invoice>> GetAllAsync();
        Task<int> GetNextInvoiceNumberAsync();
        Task<Invoice?> FindAsync(int id);
        Task<Invoice> SaveAsync(Invoice invoice);
        Task UpdateAsync(Invoice invoice);
        Task DeleteAsync(int id);
    }
}
