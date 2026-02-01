using BankMore.CheckingAccount.Domain.IdempotenciaAggregate;

namespace BankMore.CheckingAccount.Domain.Interfaces;

public interface IIdempotenciaService
{
  public ValueTask<IResult<bool>> SaveIdempotencia(Idempotencia idempotencia);
  public ValueTask<IResult<bool>> RequestHashExists(string hash);
  public ValueTask<IResult<bool>> ChangeIdempotenciaStatus(string id, IdempotenciaResult idempotenciaResult);
}