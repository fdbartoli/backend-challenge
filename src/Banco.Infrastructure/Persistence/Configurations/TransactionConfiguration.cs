using Banco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banco.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Movimientos");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.AccountId).HasColumnName("idCuenta");
        builder.Property(t => t.Date).HasColumnName("fecha");
        builder.Property(t => t.Type).HasColumnName("tipo").HasMaxLength(50);
        builder.Property(t => t.Description).HasColumnName("descripcion").HasMaxLength(Transaction.DescriptionMaxLength);
        builder.Property(t => t.Amount).HasColumnName("importe");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        builder.Ignore(t => t.SignedAmount);
    }
}