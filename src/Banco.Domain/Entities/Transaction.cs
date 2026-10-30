using Banco.Domain.Common;
using Banco.Domain.Transactions;

namespace Banco.Domain.Entities;

public class Transaction
{
    public const int DescriptionMaxLength = 50;

    public int Id { get; private set; }
    public int AccountId { get; private set; }
    public DateTime Date { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public decimal SignedAmount
    {
        get
        {
            return TransactionTypes.IsCredit(Type) ? Amount : -Amount;
        }
    }

    private Transaction() { }

    public Transaction(int accountId, DateTime date, string type, string? description, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(type) || !TransactionTypes.IsKnown(type))
        {
            throw new DomainException($"Unknown transaction type: '{type}'.");
        }

        if (description is not null && description.Length > DescriptionMaxLength)
        {
            throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters.");
        }

        if (amount <= 0)
        {
            throw new DomainException("Amount must be greater than zero.");
        }

        AccountId = accountId;
        Date = date;
        Type = type;
        Description = description;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}
