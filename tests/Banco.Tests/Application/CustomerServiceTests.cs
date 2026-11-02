using Banco.Application.Common.Exceptions;
using Banco.Application.Customers;
using Banco.Application.Customers.Dtos;
using Banco.Application.Interfaces.External;
using Banco.Application.Interfaces.Persistence;
using Banco.Domain.Bcra;
using Banco.Domain.Common;
using Banco.Domain.Entities;
using Banco.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Banco.Tests.Application;

public class CustomerServiceTests
{
    private const string ValidCuit = "30610252334";

    private readonly Mock<ICustomerRepository> _repository = new();
    private readonly Mock<IBcraClient> _bcraClient = new();
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _service = new CustomerService(_repository.Object, _bcraClient.Object, NullLogger<CustomerService>.Instance);
    }

    private void CustomerExists()
    {
        var customer = new Customer("Arcos Dorados Argentina", "S.A.", new Cuit(ValidCuit));

        _repository
            .Setup(r => r.GetByCuitWithAccountsAsync(It.IsAny<Cuit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
    }

    [Fact]
    public async Task GetByCuitAsync_WhenBcraHasData_ReturnsAvailableSituation()
    {
        CustomerExists();
        var report = new DebtReport("202609", new List<DebtEntry>
        {
            new("BANCO A", 1, 20m, 0),
            new("BANCO B", 3, 5m, 95)
        });
        _bcraClient
            .Setup(c => c.GetDebtReportAsync(It.IsAny<Cuit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var result = await _service.GetByCuitAsync(ValidCuit, CancellationToken.None);

        Assert.Equal(BcraStatus.Available, result.Bcra.Status);
        Assert.Equal("202609", result.Bcra.Period);
        Assert.Equal(3, result.Bcra.WorstSituation);
        Assert.Equal(25m, result.Bcra.TotalAmount);
        Assert.Equal(2, result.Bcra.Entities.Count);
    }

    [Fact]
    public async Task GetByCuitAsync_WhenBcraHasNoData_ReturnsNoDataSituation()
    {
        CustomerExists();
        _bcraClient
            .Setup(c => c.GetDebtReportAsync(It.IsAny<Cuit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DebtReport?)null);

        var result = await _service.GetByCuitAsync(ValidCuit, CancellationToken.None);

        Assert.Equal(BcraStatus.NoData, result.Bcra.Status);
        Assert.Null(result.Bcra.WorstSituation);
        Assert.Empty(result.Bcra.Entities);
    }

    [Fact]
    public async Task GetByCuitAsync_WhenBcraIsUnavailable_ReturnsCustomerWithUnavailableSituation()
    {
        CustomerExists();
        _bcraClient
            .Setup(c => c.GetDebtReportAsync(It.IsAny<Cuit>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExternalServiceUnavailableException("BCRA service is unavailable.", new HttpRequestException()));

        var result = await _service.GetByCuitAsync(ValidCuit, CancellationToken.None);

        Assert.Equal(ValidCuit, result.Cuit);
        Assert.Equal("Arcos Dorados Argentina", result.FirstName);
        Assert.Equal(BcraStatus.Unavailable, result.Bcra.Status);
        Assert.Empty(result.Bcra.Entities);
    }

    [Fact]
    public async Task GetByCuitAsync_WhenCustomerDoesNotExist_ThrowsNotFoundAndSkipsBcra()
    {
        _repository
            .Setup(r => r.GetByCuitWithAccountsAsync(It.IsAny<Cuit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetByCuitAsync(ValidCuit, CancellationToken.None));

        _bcraClient.Verify(
            c => c.GetDebtReportAsync(It.IsAny<Cuit>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByCuitAsync_WithInvalidCuit_ThrowsDomainExceptionWithoutQueryingAnything()
    {
        await Assert.ThrowsAsync<DomainException>(
            () => _service.GetByCuitAsync("12345", CancellationToken.None));

        _repository.Verify(
            r => r.GetByCuitWithAccountsAsync(It.IsAny<Cuit>(), It.IsAny<CancellationToken>()), Times.Never);
        _bcraClient.Verify(
            c => c.GetDebtReportAsync(It.IsAny<Cuit>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}