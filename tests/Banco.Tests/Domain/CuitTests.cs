using Banco.Domain.Common;
using Banco.Domain.ValueObjects;

namespace Banco.Tests.Domain;

public class CuitTests
{
    [Theory]
    [InlineData("30610252334")]
    [InlineData("20990000019")]
    [InlineData("27990000021")]
    public void Constructor_WithValidCuit_KeepsValue(string value)
    {
        var cuit = new Cuit(value);

        Assert.Equal(value, cuit.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("3061025233")]
    [InlineData("306102523345")]
    [InlineData("3061025233A")]
    [InlineData(" 30610252334")]
    [InlineData("30610252335")]
    public void Constructor_WithInvalidCuit_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => new Cuit(value));
    }

    [Fact]
    public void Constructor_WhenCalculationGives11_AcceptsZeroAsCheckDigit()
    {
        var cuit = new Cuit("20200000030");

        Assert.Equal("20200000030", cuit.Value);
    }

    [Theory]
    [InlineData("20200000090")]
    [InlineData("20200000099")]
    public void Constructor_WhenCalculationGives10_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => new Cuit(value));
    }

    [Fact]
    public void Equality_WithSameValue_IsEqual()
    {
        var first = new Cuit("30610252334");
        var second = new Cuit("30610252334");

        Assert.Equal(first, second);
    }
}