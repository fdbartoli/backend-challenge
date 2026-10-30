using Banco.Domain.Common;

namespace Banco.Domain.ValueObjects;

public sealed record Cuit
{
    private static readonly int[] Weights = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

    public string Value { get; }

    public Cuit(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("CUIT cannot be null or empty.");
        }

        if (value.Length != 11 || !value.All(char.IsDigit))
        {
            throw new DomainException("CUIT must be exactly 11 digits.");
        }

        if (!HasValidCheckDigit(value))
        {
            throw new DomainException("CUIT has an invalid check digit.");
        }

        Value = value;
    }

    private static bool HasValidCheckDigit(string cuit)
    {
        var sum = 0;
        for (var i = 0; i < Weights.Length; i++)
        {
            sum += (cuit[i] - '0') * Weights[i];
        }

        var checkDigit = 11 - (sum % 11);

        if (checkDigit == 11)
        {
            checkDigit = 0;
        }
        else if (checkDigit == 10)
        {
            return false;
        }

        return checkDigit == cuit[10] - '0';
    }

    public override string ToString() => Value;
}
