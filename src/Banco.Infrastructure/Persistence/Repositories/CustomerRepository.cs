using Banco.Application.Interfaces.Persistence;
using Banco.Domain.Entities;
using Banco.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Banco.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly BancoDbContext _context;

    public CustomerRepository(BancoDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByCuitWithAccountsAsync(Cuit cuit, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Cuit == cuit, cancellationToken);
    }
}