using Banco.Domain.Common;

namespace Banco.Domain.Transactions;

public static class TransactionTypes
{
    private static readonly HashSet<string> Credits = new(StringComparer.OrdinalIgnoreCase)
    {
        "HABERES / SUELDO ACREDITADO",
        "TRANSFERENCIA RECIBIDA",
        "DEPOSITO EN EFECTIVO",
        "DEPOSITO POR CAJERO AUTOMATICO",
        "DEVOLUCION DE GASTOS",
        "REINTEGRO TARJETA DE DEBITO",
        "REINTEGRO IMPUESTO",
        "ACREDITACION DE PLAZO FIJO",
        "ACREDITACION DE PRESTAMO"
    };

    private static readonly HashSet<string> Debits = new(StringComparer.OrdinalIgnoreCase)
    {
        "EXTRACCION EN EFECTIVO (CAJERO)",
        "TRANSFERENCIA EMITIDA",
        "COMPRA TARJETA DE DEBITO",
        "PAGO DE SERVICIOS (DEBITO AUTOMATICO)",
        "DEBITO AUTOMATICO TARJETA DE CREDITO",
        "PAGO DE PRESTAMO (CUOTA)",
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