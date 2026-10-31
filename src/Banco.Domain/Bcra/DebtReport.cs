namespace Banco.Domain.Bcra;

public sealed class DebtReport
{
    public string Period { get; }
    public IReadOnlyList<DebtEntry> Entries { get; }

    public DebtReport(string period, IReadOnlyList<DebtEntry> entries)
    {
        Period = period;
        Entries = entries;
    }

    public int WorstSituation
    {
        get
        {
            if (Entries.Count == 0)
            {
                return 0;
            }

            return Entries.Max(e => e.Situation);
        }
    }

    public decimal TotalAmount
    {
        get
        {
            return Entries.Sum(e => e.Amount);
        }
    }
}