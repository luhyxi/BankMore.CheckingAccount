using System.IdentityModel.Tokens.Jwt;
using BankMore.CheckingAccount.Application.ContaCorrente.Command.Create;
using BankMore.CheckingAccount.Application.ContaCorrente.Command.Inactivate;
using BankMore.CheckingAccount.Application.ContaCorrente.Command.Login;
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

                        if (!result.IsSuccess) return Results.BadRequest(new { error = result.Error });

                        return Results.Created($"/conta/{result.Value}", new { id = result.Value });
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                })
            .WithName("CreateContaCorrente")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        routes.MapPost("login",
            async (
                [FromBody] LoginContaCorrenteRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var command = new LoginContaCorrenteCommand(
                        request.isCpf,
                        new ContaCorrenteCpf(request.Cpf),
                        new ContaCorrenteNumero(request.Numero),
                        new ContaCorrenteSenha(request.Senha));
                    var result = await mediator.Send(command, cancellationToken);

                    if (!result.IsSuccess) return Results.BadRequest(new { error = result.Error });

                    return Results.Accepted($"/login/{result.Value}", result.Value);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            })
            .WithName("Login")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

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
                            return Results.StatusCode(StatusCodes.Status403Forbidden);
                        }

                        var command = new InactivateContaCorrenteCommand(
                            new ContaCorrenteId(contaId),
                            new ContaCorrenteSenha(request.Senha));
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
            .WithName("InactivateContaCorrente")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();
    }
}

public sealed record CreateContaCorrenteRequest(string Cpf, string Nome, string Senha);

public sealed record LoginContaCorrenteRequest(bool isCpf, string Cpf, string Numero, string Senha); // need to make CPF and Numero nullable
public sealed record InactivateContaCorrenteRequest(string Senha);
