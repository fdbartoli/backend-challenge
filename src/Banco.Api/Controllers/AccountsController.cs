using Banco.Application.Accounts;
using Banco.Application.Accounts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Banco.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AccountDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var account = await _accountService.GetByIdAsync(id, cancellationToken);
        return Ok(account);
    }
}