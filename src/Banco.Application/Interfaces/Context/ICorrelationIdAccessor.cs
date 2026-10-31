namespace Banco.Application.Interfaces.Context;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
}