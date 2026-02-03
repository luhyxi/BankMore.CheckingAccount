using BankMore.CheckingAccount.Application.ContaCorrente.Command.Create;
using BankMore.CheckingAccount.Application.ContaCorrente.Command.Inactivate;
using BankMore.CheckingAccount.Application.ContaCorrente.Command.Login;
using BankMore.CheckingAccount.Application.ContaCorrente.Query.GetSaldo;
using BankMore.CheckingAccount.Application.ContaCorrente.Query.GetIdByNumero;
using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

using Mediator;

using Microsoft.AspNetCore.Mvc;

namespace BankMore.CheckingAccount.Web.Endpoints;

public static class ContaCorrenteEndpoints
{
    public static void MapContaCorrenteEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("conta",
                async (
                    [FromBody] CreateContaCorrenteRequest request,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var command = new CreateContaCorrenteCommand(
                            new ContaCorrenteCpf(request.Cpf),
                            new ContaCorrenteNome(request.Nome),
                            request.Senha);
                        var result = await mediator.Send(command, cancellationToken);

                        if (!result.IsSuccess)
                        {
                            var errorType = result.Error?.Contains("CPF", StringComparison.OrdinalIgnoreCase) == true ||
                                          result.Error?.Contains("invalid", StringComparison.OrdinalIgnoreCase) == true
                                ? "INVALID_DOCUMENT"
                                : "INVALID_DATA";
                            return Results.BadRequest(new ErrorResponse(result.Error ?? "Invalid request", errorType));
                        }

                        // Assuming result.Value contains the account number
                        return Results.Created($"/conta/{result.Value}", new CreateAccountResponse(result.Value.ToString()));
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new ErrorResponse(ex.Message, "INVALID_DOCUMENT"));
                    }
                })
            .WithName("CreateCheckingAccount")
            .WithDescription("Creates a new checking account")
            .WithSummary("Create checking account")
            .Produces<CreateAccountResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create checking account";
                operation.Description = "Creates a new checking account with CPF validation. Returns the account number on success.";
                return operation;
            });

        routes.MapPost("conta/login",
            async (
                [FromBody] LoginContaCorrenteRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    LoginContaCorrenteCommand command;
                    if (request.isCpf)
                    {
                        command = new LoginContaCorrenteCommand(
                            request.isCpf,
                            new ContaCorrenteSenha(request.Senha),
                            null,
                            new ContaCorrenteCpf(request.Cpf));
                    }
                    else
                    {
                        command = new LoginContaCorrenteCommand(
                            request.isCpf,
                            new ContaCorrenteSenha(request.Senha),
                            new ContaCorrenteNumero(request.Numero),
                            null);
                    }
                    var result = await mediator.Send(command, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        return Results.Unauthorized();
                    }

                    return Results.Ok(new LoginResponse(result.Value));
                }
                catch (ArgumentException ex)
                {
                    return Results.Json(
                        new ErrorResponse(ex.Message, "USER_UNAUTHORIZED"),
                        statusCode: StatusCodes.Status401Unauthorized);
                }
            })
            .WithName("Login")
            .WithSummary("Authenticate user")
            .WithDescription("Authenticates a user with account number or CPF and password. Returns a JWT token.")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Login to checking account";
                operation.Description = "Authenticates using account number or CPF with password. Returns a JWT token containing the checking account identification.";
                return operation;
            });

        routes.MapPost("conta/inactivate",
                async (
                    [FromBody] InactivateContaCorrenteRequest request,
                    HttpContext httpContext,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var subject = httpContext.User.FindFirst("id")?.Value;
                        if (string.IsNullOrWhiteSpace(subject) || !Guid.TryParse(subject, out var contaId))
                        {
                            return Results.Json(
                                new ErrorResponse("Invalid or expired token", "UNAUTHORIZED"),
                                statusCode: StatusCodes.Status403Forbidden);
                        }

                        var command = new InactivateContaCorrenteCommand(
                            new ContaCorrenteId(contaId),
                            new ContaCorrenteSenha(request.Senha));
                        var result = await mediator.Send(command, cancellationToken);

                        if (!result.IsSuccess)
                        {
                            var errorType = result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                                ? "INVALID_ACCOUNT"
                                : result.Error?.Contains("inactive", StringComparison.OrdinalIgnoreCase) == true
                                    ? "INACTIVE_ACCOUNT"
                                    : result.Error?.Contains("password", StringComparison.OrdinalIgnoreCase) == true
                                        ? "INVALID_PASSWORD"
                                        : "INVALID_DATA";
                            return Results.BadRequest(new ErrorResponse(result.Error ?? "Invalid request", errorType));
                        }

                        return Results.NoContent();
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new ErrorResponse(ex.Message, "INVALID_DATA"));
                    }
                })
            .WithName("DeactivateCheckingAccount")
            .WithSummary("Deactivate checking account")
            .WithDescription("Deactivates a checking account after password validation")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Deactivate checking account";
                operation.Description = "Deactivates the authenticated user's checking account. Requires valid authentication token and password confirmation.";
                return operation;
            });
        
        routes.MapGet("conta/saldo",
            async (
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
                if (!isAuthenticated)
                {
                    return Results.Json(
                        new ErrorResponse("Invalid or expired token", "UNAUTHORIZED"),
                        statusCode: StatusCodes.Status403Forbidden);
                }

                var subject = httpContext.User.FindFirst("id")?.Value;
                if (string.IsNullOrWhiteSpace(subject) || !Guid.TryParse(subject, out var contaId))
                {
                    return Results.Json(
                        new ErrorResponse("Invalid or expired token", "UNAUTHORIZED"),
                        statusCode: StatusCodes.Status403Forbidden);
                }

                var query = new GetSaldoQuery(new ContaCorrenteId(contaId));
                var result = await mediator.Send(query, cancellationToken);

                if (!result.IsSuccess)
                {
                    var error = result.Error ?? "Failed to retrieve balance.";
                    var errorType = error.Contains("inactive", StringComparison.OrdinalIgnoreCase)
                        ? "INACTIVE_ACCOUNT"
                        : error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                            ? "INVALID_ACCOUNT"
                            : "INVALID_DATA";

                    return Results.BadRequest(new ErrorResponse(error, errorType));
                }

                // Map from SaldoResult to BalanceResponse
                var saldoResult = result.Value;
                var balanceResponse = new BalanceResponse(
                    saldoResult.NumeroConta,
                    saldoResult.NomeTitular,
                    saldoResult.DataConsulta,
                    saldoResult.SaldoAtual);

                return Results.Ok(balanceResponse);
            })
            .WithName("GetBalance")
            .WithSummary("Get account balance")
            .WithDescription("Retrieves the current balance and account information")
            .RequireAuthorization()
            .Produces<BalanceResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get checking account balance";
                operation.Description = "Retrieves the current balance, account holder name, and balance inquiry timestamp for the authenticated user's checking account.";
                return operation;
            });

        routes.MapGet("conta/id/{accountNumber}",
            async (
                [FromRoute] string accountNumber,
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var subject = httpContext.User.FindFirst("id")?.Value;
                    if (string.IsNullOrWhiteSpace(subject) || !Guid.TryParse(subject, out var contaId))
                    {
                        return Results.Json(
                            new ErrorResponse("Invalid or expired token", "UNAUTHORIZED"),
                            statusCode: StatusCodes.Status403Forbidden);
                    }
                    
                    var query = new GetIdByNumeroQuery(new ContaCorrenteNumero(accountNumber));
                    var result = await mediator.Send(query, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        var error = result.Error ?? "Failed to retrieve account ID.";
                        var errorType = error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                            ? "ACCOUNT_NOT_FOUND"
                            : "INVALID_DATA";

                        return Results.BadRequest(new ErrorResponse(error, errorType));
                    }

                    return Results.Ok(new AccountIdResponse(result.Value));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new ErrorResponse(ex.Message, "INVALID_ACCOUNT_NUMBER"));
                }
            })
            .WithName("GetAccountId")
            .WithSummary("Get account UUID by account number")
            .WithDescription("Retrieves the UUID of an account by its account number")
            .RequireAuthorization()
            .Produces<AccountIdResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get account UUID";
                operation.Description = "Retrieves the UUID identifier for a checking account using its account number.";
                return operation;
            });
    }
}

public sealed record CreateContaCorrenteRequest(string Cpf, string Nome, string Senha);

public sealed record LoginContaCorrenteRequest(bool isCpf, string Cpf, string Numero, string Senha); // need to make CPF and Numero nullable

public sealed record InactivateContaCorrenteRequest(string Senha);

public sealed record CreateAccountResponse(string AccountNumber);
public sealed record LoginResponse(string Token);
public sealed record ErrorResponse(string Message, string Type);
public sealed record BalanceResponse(
    string AccountNumber,
    string AccountHolderName,
    DateTime BalanceDateTime,
    string CurrentBalance);
public sealed record AccountIdResponse(Guid Id);
