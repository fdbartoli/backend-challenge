using System.Net;
using System.Net.Http.Json;
using Banco.Application.Interfaces.External;
using Banco.Domain.Bcra;
using Banco.Domain.ValueObjects;

namespace Banco.Infrastructure.External.Bcra;

public class BcraClient : IBcraClient
{
    private readonly HttpClient _httpClient;

    public BcraClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DebtReport?> GetDebtReportAsync(Cuit cuit, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"deudas/{cuit.Value}", cancellationToken);

        // The BCRA answers 404 when it has no records for the CUIT: that is "no data", not a failure
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<BcraResponse>(cancellationToken);

        // Several periods can come back; the most recent one is used
        var latestPeriod = body?.Results?.Periods
            .OrderByDescending(p => p.Period)
            .FirstOrDefault();

        if (latestPeriod is null)
        {
            return null;
        }

        var entries = latestPeriod.Entities
            .Select(e => new DebtEntry(e.Institution, e.Situation, e.Amount, e.DaysOverdue))
            .ToList();

        return new DebtReport(latestPeriod.Period, entries);
    }
}