using Banco.Domain.Common;
using Banco.Domain.ValueObjects;

namespace Banco.Domain.Entities;

public class Customer
{
    public const int NameMaxLength = 255;

    private readonly List<Account> _accounts = new();

    public int Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Cuit Cuit { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<Account> Accounts
    {
        get
        {
            return _accounts.AsReadOnly();
        }
    }

    // Required by EF Core to materialize entities from the database.
    private Customer() { }

    public Customer(string firstName, string lastName, Cuit cuit)
    {
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > NameMaxLength)
        {
            throw new DomainException($"First name is required and cannot exceed {NameMaxLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length > NameMaxLength)
        {
            throw new DomainException($"Last name is required and cannot exceed {NameMaxLength} characters.");
        }

        if (cuit is null)
        {
            throw new DomainException("CUIT is required.");
        }

        FirstName = firstName;
        LastName = lastName;
        Cuit = cuit;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}