using Banco.Domain.Bcra;
using Banco.Domain.ValueObjects;

namespace Banco.Application.Interfaces.External;

public interface IBcraClient
{
    // Returns null when the BCRA has no records for the CUIT (HTTP 404)
    Task<DebtReport?> GetDebtReportAsync(Cuit cuit, CancellationToken cancellationToken);
}