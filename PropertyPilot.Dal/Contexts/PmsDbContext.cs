using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Dal.Contexts;

public class PmsDbContext(IConfiguration configuration) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // connects to postgres with the connection string from appsettings
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("pms"));
    }

    public DbSet<PropertyListing> PropertyListings { get; set; }
    public DbSet<SubUnit> SubUnits { get; set; }
    public DbSet<PropertyPilotUser> PropertyPilotUsers { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<Tenancy> Tenancies { get; set; }
    public DbSet<MonetaryAccount> MonetaryAccounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<RentPayment> RentPayments { get; set; }
}
