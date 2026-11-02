using Banco.Domain.Bcra;

namespace Banco.Tests.Domain;

public class DebtReportTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void WorstSituation_WithOneEntryWorseThanNormal_ReturnsThatSituation(int worst)
    {
        var report = new DebtReport("202609", new List<DebtEntry>
    {
        new("BANCO A", 1, 1500m, 0),
        new("BANCO B", 1, 300m, 0),
        new("BANCO C", worst, 800m, 120)
    });

        Assert.Equal(worst, report.WorstSituation);
    }

    [Fact]
    public void WorstSituation_WithAllNormal_ReturnsOne()
    {
        var report = new DebtReport("202609", new List<DebtEntry>
        {
            new("BANCO A", 1, 1500m, 0),
            new("BANCO B", 1, 300m, 0)
        });

        Assert.Equal(1, report.WorstSituation);
    }

    [Fact]
    public void WorstSituation_WithNoEntries_ReturnsZero()
    {
        var report = new DebtReport("202609", new List<DebtEntry>());

        Assert.Equal(0, report.WorstSituation);
    }

    [Fact]
    public void TotalAmount_WithSeveralEntries_ReturnsSum()
    {
        var report = new DebtReport("202609", new List<DebtEntry>
        {
            new("BANCO A", 1, 1500m, 0),
            new("BANCO B", 2, 300m, 15)
        });

        Assert.Equal(1800m, report.TotalAmount);
    }
}