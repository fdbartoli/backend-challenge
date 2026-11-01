using Banco.Domain.Common;
using Banco.Domain.Transactions;

namespace Banco.Tests.Domain;

public class TransactionTypesTests
{
    [Theory]
    [InlineData("HABERES / SUELDO ACREDITADO")]
    [InlineData("DEPÓSITO EN EFECTIVO")]
    [InlineData("ACREDITACIÓN DE PRÉSTAMO")]
    public void IsCredit_WithCreditType_ReturnsTrue(string type)
    {
        Assert.True(TransactionTypes.IsCredit(type));
    }

    [Theory]
    [InlineData("TRANSFERENCIA EMITIDA")]
    [InlineData("PAGO DE SERVICIOS (DEBITO AUTOMATICO)")]
    [InlineData("PAGO DE PRÉSTAMO (CUOTA)")]
    public void IsCredit_WithDebitType_ReturnsFalse(string type)
    {
        Assert.False(TransactionTypes.IsCredit(type));
    }

    [Fact]
    public void IsCredit_WithLowercaseCreditType_ReturnsTrue()
    {
        Assert.True(TransactionTypes.IsCredit("transferencia recibida"));
    }

    [Fact]
    public void IsCredit_WithUnknownType_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => TransactionTypes.IsCredit("PAGO EN CRIPTO"));
    }

    [Theory]
    [InlineData("TRANSFERENCIA RECIBIDA")]
    [InlineData("TRANSFERENCIA EMITIDA")]
    public void IsKnown_WithExistingType_ReturnsTrue(string type)
    {
        Assert.True(TransactionTypes.IsKnown(type));
    }

    [Fact]
    public void IsKnown_WithUnknownType_ReturnsFalse()
    {
        Assert.False(TransactionTypes.IsKnown("PAGO EN CRIPTO"));
    }
}