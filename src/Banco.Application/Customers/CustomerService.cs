using Banco.Application.Common.Exceptions;
using Banco.Application.Customers.Dtos;
using Banco.Application.Interfaces.Persistence;
using Banco.Domain.Entities;
using Banco.Domain.ValueObjects;
using Banco.Application.Interfaces.External;
using Banco.Domain.Bcra;
using Microsoft.Extensions.Logging;

namespace Banco.Application.Customers;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IBcraClient _bcraClient;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ICustomerRepository customerRepository, IBcraClient bcraClient, ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _bcraClient = bcraClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CustomerSummaryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);

        return customers
            .Select(c => new CustomerSummaryDto(c.Id, c.FirstName, c.LastName, c.Cuit.Value))
            .ToList();
    }

    public async Task<CustomerDetailDto> GetByCuitAsync(string cuit, CancellationToken cancellationToken)
    {
        var customerCuit = new Cuit(cuit);

        var customer = await _customerRepository.GetByCuitWithAccountsAsync(customerCuit, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException($"Customer with CUIT {cuit} was not found.");
        }

        var bcra = await GetBcraSituationAsync(customerCuit, cancellationToken);

        return ToDetailDto(customer, bcra);
    }

    private async Task<BcraSituationDto> GetBcraSituationAsync(Cuit cuit, CancellationToken cancellationToken)
    {
        try
        {
            var debtReport = await _bcraClient.GetDebtReportAsync(cuit, cancellationToken);
            return ToBcraDto(debtReport);
        }
        catch (ExternalServiceUnavailableException ex)
        {
            _logger.LogWarning(ex, "BCRA unavailable, returning customer without credit situation");
            return new BcraSituationDto(BcraStatus.Unavailable, null, null, null, new List<BcraEntityDto>());
        }
    }

    private static CustomerDetailDto ToDetailDto(Customer customer, BcraSituationDto bcra)
    {
        var accounts = customer.Accounts
            .Select(a => new AccountSummaryDto(a.Id, a.Number, a.BranchCode, a.Balance))
            .ToList();

        return new CustomerDetailDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Cuit.Value,
            accounts,
            bcra);
    }

    private static BcraSituationDto ToBcraDto(DebtReport? debtReport)
    {
        if (debtReport is null)
        {
            return new BcraSituationDto(BcraStatus.NoData, null, null, null, new List<BcraEntityDto>());
        }

        var entities = debtReport.Entries
            .Select(e => new BcraEntityDto(e.Institution, e.Situation, e.Amount, e.DaysOverdue))
            .ToList();

        return new BcraSituationDto(
            BcraStatus.Available,
            debtReport.Period,
            debtReport.WorstSituation,
            debtReport.TotalAmount,
            entities);
    }

}