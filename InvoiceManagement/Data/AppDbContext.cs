using InvoiceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasIndex(invoice => invoice.Number)
                     .IsUnique();

                entity.HasMany(invoice => invoice.Items)
                     .WithOne()
                     .HasForeignKey(item => item.InvoiceId)
                     .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}