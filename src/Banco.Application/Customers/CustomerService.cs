using Banco.Application.Common.Exceptions;
using Banco.Application.Customers.Dtos;
using Banco.Application.Interfaces.Persistence;
using Banco.Domain.Entities;
using Banco.Domain.ValueObjects;

namespace Banco.Application.Customers;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
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

        return ToDetailDto(customer);
    }

    private static CustomerDetailDto ToDetailDto(Customer customer)
    {
        var accounts = customer.Accounts
            .Select(a => new AccountSummaryDto(a.Id, a.Number, a.BranchCode, a.Balance))
            .ToList();

        return new CustomerDetailDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Cuit.Value,
            accounts);
    }
}