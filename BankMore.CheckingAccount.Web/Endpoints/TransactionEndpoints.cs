using BankMore.CheckingAccount.Application.Movimentacao.Command.Transaction;
using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using BankMore.CheckingAccount.Domain.IdempotenciaAggregate;
using BankMore.CheckingAccount.Domain.MovimentoAggregate;

using Mediator;

using Microsoft.AspNetCore.Mvc;

namespace BankMore.CheckingAccount.Web.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this IEndpointRouteBuilder routes)
    {
        
        routes.MapPost("transaction",
                async (
                    [FromBody] TransactionRequest request,
                    HttpContext httpContext,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {

                        var contaNumero = request.Numero ??
                                          httpContext.User.FindFirst("numero")?.Value;

                        if (string.IsNullOrWhiteSpace(contaNumero))
                        {
                            return Results.BadRequest(new { error = "Account number must be provided." });
                        }

                        var tipoMovimentoDomain = request.TipoMovimento switch
                        {
                            "C" or "c" => TipoMovimento.Credito,
                            "D" or "d" => TipoMovimento.Debito,
                            _ => TipoMovimento.None
                        };

                        if (tipoMovimentoDomain == TipoMovimento.None)
                        {
                            return Results.BadRequest(new { error = "Invalid transaction type." });
                        }

                        var requestHash = IdempotenciaHashedRequest.FromPlainText(
                            request.GetHashCode().ToString());

                        var command = new TransactionCommand(
                            Idempotencia.Create(requestHash.Value),
                            new ContaCorrenteNumero(contaNumero),
                            request.Valor,
                            tipoMovimentoDomain);

                        var result = await mediator.Send(command, cancellationToken);

                        if (!result.IsSuccess)
                        {
                            return Results.BadRequest(new { error = result.Error });
                        }

                        return Results.NoContent();
                        
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                })
            .WithName("Transaction")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();
    }
}

public sealed record TransactionRequest(string? Numero, decimal Valor, string TipoMovimento);
