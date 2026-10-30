using Banco.Domain.Entities;
using Banco.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banco.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.FirstName).HasColumnName("nombre").HasMaxLength(Customer.NameMaxLength);
        builder.Property(c => c.LastName).HasColumnName("apellido").HasMaxLength(Customer.NameMaxLength);
        builder.Property(c => c.Cuit)
            .HasColumnName("cuit")
            .HasConversion(cuit => cuit.Value, value => new Cuit(value));
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(c => c.Accounts)
            .WithOne()
            .HasForeignKey(a => a.CustomerId);

        builder.Navigation(c => c.Accounts)
            .HasField("_accounts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}