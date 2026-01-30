namespace BankMore.CheckingAccount.Domain.Idempotencia;

public sealed class Idempotencia
{
    public Guid  ChaveIdempotencia { get; set; }
    public string? Requisicao  { get; set; }
    public string? Resultado { get; set; }
}