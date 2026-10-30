using Banco.Application.Interfaces.Persistence;
using Banco.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Banco.Infrastructure.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly BancoDbContext _context;

    public AccountRepository(BancoDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdWithTransactionsAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Transactions.OrderByDescending(t => t.Date))
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}