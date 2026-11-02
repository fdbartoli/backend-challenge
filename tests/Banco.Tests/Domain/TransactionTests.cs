using Banco.Domain.Common;
using Banco.Domain.Entities;

namespace Banco.Tests.Domain;

public class TransactionTests
{
    private static readonly DateTime Date = new(2026, 9, 1);

    [Fact]
    public void SignedAmount_WithCreditType_ReturnsPositiveAmount()
    {
        var transaction = new Transaction(1, Date, "DEPOSITO EN EFECTIVO", null, 250000m);

        Assert.Equal(250000m, transaction.SignedAmount);
    }

    [Fact]
    public void SignedAmount_WithDebitType_ReturnsNegativeAmount()
    {
        var transaction = new Transaction(1, Date, "TRANSFERENCIA EMITIDA", null, 420000m);

        Assert.Equal(-420000m, transaction.SignedAmount);
    }

    [Fact]
    public void SignedAmount_SumOfCreditsAndDebits_EqualsAccountBalance()
    {
        var transactions = new List<Transaction>
        {
            new(1, Date, "TRANSFERENCIA RECIBIDA", "Cobro a proveedor", 1500000m),
            new(1, Date, "PAGO DE SERVICIOS (DEBITO AUTOMATICO)", "Energía eléctrica", 85000.50m),
            new(1, Date, "TRANSFERENCIA EMITIDA", "Pago a proveedor", 420000m),
            new(1, Date, "DEPOSITO EN EFECTIVO", "Recaudación local", 250000m)
        };

        var balance = transactions.Sum(t => t.SignedAmount);

        Assert.Equal(1244999.50m, balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_WithNonPositiveAmount_ThrowsDomainException(decimal amount)
    {
        Assert.Throws<DomainException>(() => new Transaction(1, Date, "DEPOSITO EN EFECTIVO", null, amount));
    }

    [Fact]
    public void Constructor_WithUnknownType_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Transaction(1, Date, "PAGO EN CRIPTO", null, 100m));
    }

    [Fact]
    public void Constructor_WithDescriptionTooLong_ThrowsDomainException()
    {
        var description = new string('x', Transaction.DescriptionMaxLength + 1);

        Assert.Throws<DomainException>(() => new Transaction(1, Date, "DEPOSITO EN EFECTIVO", description, 100m));
    }
}