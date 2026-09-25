using InvoiceManagement.Data;
using InvoiceManagement.Models;
using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Services.Interfaces;

namespace InvoiceManagement.Services.Implementations
{
    public class InvoiceService(IDbContextFactory<AppDbContext> contextFactory) : IInvoiceService
    {

        public async Task<List<Invoice>> GetAllAsync()
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            
            return await context.Invoices
                .AsNoTracking()
                .Include(invoice => invoice.Items)
                .OrderByDescending(invoice => invoice.Number)
                .ToListAsync();
        }

        public async Task<int> GetNextInvoiceNumberAsync()
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await GetNextInvoiceNumberInternalAsync(context);
        }

        private static async Task<int> GetNextInvoiceNumberInternalAsync(AppDbContext context)
        {
            var maxNumber = await context.Invoices.MaxAsync(invoiceEntity => (int?)invoiceEntity.Number) ?? 100;
            return maxNumber + 1;
        }

        public async Task<Invoice?> FindAsync(int id)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Invoices
                .Include(invoiceEntity => invoiceEntity.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(invoiceEntity => invoiceEntity.Id == id);
        }

        public async Task<Invoice> SaveAsync(Invoice invoice)
        {
            if (invoice.Items == null || invoice.Items.Count == 0)
            {
                throw new InvalidOperationException("An invoice must contain at least one line item.");
            }

            await using var context = await contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                invoice.Number = await GetNextInvoiceNumberInternalAsync(context);
                invoice.TotalAmount = invoice.Items.Sum(item => item.Quantity * item.Price);

                context.Invoices.Add(invoice);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
                return invoice;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(Invoice invoice)
        {
            if (invoice.Items == null || invoice.Items.Count == 0)
            {
                throw new InvalidOperationException("An invoice must contain at least one line item.");
            }

            await using var context = await contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var existingInvoice = await context.Invoices
                .Include(invoiceEntity => invoiceEntity.Items)
                .FirstOrDefaultAsync(invoiceEntity => invoiceEntity.Id == invoice.Id);

            if (existingInvoice is null)
            {
                throw new InvalidOperationException("Invoice not found");
            }

            existingInvoice.CustomerName = invoice.CustomerName;
            existingInvoice.Date = invoice.Date;
            existingInvoice.TotalAmount = invoice.Items.Sum(item => item.Quantity * item.Price);

            context.InvoiceItems.RemoveRange(existingInvoice.Items);

            existingInvoice.Items = invoice.Items.Select(item => new InvoiceItem
            {
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity
            }).ToList();

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var existingInvoice = await context.Invoices.FindAsync(id);
         
            if (existingInvoice == null)
            {
                return;
            }

            context.Invoices.Remove(existingInvoice);
            await context.SaveChangesAsync();
        }
    }
}