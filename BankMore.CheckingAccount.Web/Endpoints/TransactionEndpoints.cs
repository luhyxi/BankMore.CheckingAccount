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
                        // Check authentication
                        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
                        if (!isAuthenticated)
                        {
                            return Results.Json(
                                new TransactionErrorResponse("Invalid or expired token", "UNAUTHORIZED"),
                                statusCode: StatusCodes.Status403Forbidden);
                        }

                        var loggedInAccountNumber = httpContext.User.FindFirst("numero")?.Value;
                        var contaNumero = request.Numero ?? loggedInAccountNumber;

                        if (string.IsNullOrWhiteSpace(contaNumero))
                        {
                            return Results.BadRequest(
                                new TransactionErrorResponse("Account number must be provided.", "INVALID_ACCOUNT"));
                        }

                        // Validate transaction type
                        var tipoMovimentoDomain = request.TipoMovimento switch
                        {
                            "C" or "c" => TipoMovimento.Credito,
                            "D" or "d" => TipoMovimento.Debito,
                            _ => TipoMovimento.None
                        };

                        if (tipoMovimentoDomain == TipoMovimento.None)
                        {
                            return Results.BadRequest(
                                new TransactionErrorResponse("Only 'C' (Credit) or 'D' (Debit) types are accepted.", "INVALID_TYPE"));
                        }

                        // Validate that only credit type is accepted if account number is different from logged-in user
                        if (contaNumero != loggedInAccountNumber && tipoMovimentoDomain != TipoMovimento.Credito)
                        {
                            return Results.BadRequest(
                                new TransactionErrorResponse("Only credit transactions are allowed for other accounts.", "INVALID_TYPE"));
                        }

                        // Validate positive value
                        if (request.Valor <= 0)
                        {
                            return Results.BadRequest(
                                new TransactionErrorResponse("Only positive values are accepted.", "INVALID_VALUE"));
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
                            var error = result.Error ?? "Transaction failed.";
                            var errorType = error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                                ? "INVALID_ACCOUNT"
                                : error.Contains("inactive", StringComparison.OrdinalIgnoreCase)
                                    ? "INACTIVE_ACCOUNT"
                                    : "INVALID_DATA";

                            return Results.BadRequest(new TransactionErrorResponse(error, errorType));
                        }

                        return Results.NoContent();

                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new TransactionErrorResponse(ex.Message, "INVALID_DATA"));
                    }
                })
            .WithName("CreateTransaction")
            .WithSummary("Create account transaction")
            .WithDescription("Creates a credit or debit transaction for a checking account")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<TransactionErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<TransactionErrorResponse>(StatusCodes.Status403Forbidden)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create checking account transaction";
                operation.Description = @"Creates a transaction (credit or debit) for a checking account.
                - Validates that only registered and active accounts can receive transactions
                - Only positive values are accepted
                - Only 'C' (Credit) or 'D' (Debit) types are allowed
                - Credit transactions can be made to other accounts, but debit transactions can only be made to the authenticated user's account
                - Requires authentication token in the request header";
                return operation;
            });
    }
}

public sealed record TransactionRequest(string? Numero, decimal Valor, string TipoMovimento);
public sealed record TransactionErrorResponse(string Message, string Type);
