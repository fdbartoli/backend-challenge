using Banco.Domain.Common;

namespace Banco.Domain.Transactions;

public static class TransactionTypes
{
    private static readonly HashSet<string> Credits = new(StringComparer.OrdinalIgnoreCase)
    {
        "HABERES / SUELDO ACREDITADO",
        "TRANSFERENCIA RECIBIDA",
        "DEPÓSITO EN EFECTIVO",
        "DEPÓSITO POR CAJERO AUTOMÁTICO",
        "DEVOLUCIÓN DE GASTOS",
        "REINTEGRO TARJETA DE DÉBITO",
        "REINTEGRO IMPUESTO",
        "ACREDITACIÓN DE PLAZO FIJO",
        "ACREDITACIÓN DE PRÉSTAMO"
    };

    private static readonly HashSet<string> Debits = new(StringComparer.OrdinalIgnoreCase)
    {
        "EXTRACCIÓN EN EFECTIVO (CAJERO)",
        "TRANSFERENCIA EMITIDA",
        "COMPRA TARJETA DE DÉBITO",
        "PAGO DE SERVICIOS (DEBITO AUTOMATICO)",
        "DÉBITO AUTOMÁTICO TARJETA DE CRÉDITO",
        "PAGO DE PRÉSTAMO (CUOTA)",
        "TRANSFERENCIA INMEDIATA SALIENTE (CI)"
    };

    public static bool IsKnown(string type) => Credits.Contains(type) || Debits.Contains(type);

    public static bool IsCredit(string type)
    {
        if (Credits.Contains(type))
        {
            return true;
        }

        if (Debits.Contains(type))
        {
            return false;
        }

        throw new DomainException($"Unknown transaction type: '{type}'.");
    }
}