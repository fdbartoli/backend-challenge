using Banco.Domain.Common;

namespace Banco.Domain.Entities;

public class Account
{
    public const int NumberMaxLength = 12;
    public const int BranchCodeLength = 4;

    private readonly List<Transaction> _transactions = new();

    public int Id { get; private set; }
    public int CustomerId { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public string BranchCode { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions
    {
        get
        {
            return _transactions.AsReadOnly();
        }
    }

    // Required by EF Core to materialize entities from the database.
    private Account() { }

    public Account(int customerId, string number, string branchCode, decimal balance)
    {
        if (string.IsNullOrWhiteSpace(number) || number.Length > NumberMaxLength)
        {
            throw new DomainException($"Account number is required and cannot exceed {NumberMaxLength} characters.");
        }

        if (branchCode is null || branchCode.Length != BranchCodeLength || !branchCode.All(char.IsDigit))
        {
            throw new DomainException($"Branch code must be exactly {BranchCodeLength} digits.");
        }

        CustomerId = customerId;
        Number = number;
        BranchCode = branchCode;
        Balance = balance;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}
