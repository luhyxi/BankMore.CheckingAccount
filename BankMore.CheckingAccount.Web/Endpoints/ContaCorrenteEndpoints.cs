using BankMore.CheckingAccount.Application.ContaCorrente.Command.CreateContaCorrente;
using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

using Mediator;

using Microsoft.AspNetCore.Mvc;

namespace BankMore.CheckingAccount.Web.Endpoints;

public static class ContaCorrenteEndpoints
{
    public static void MapContaCorrenteEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("conta",
                async ([FromBody] CreateContaCorrenteRequest request, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var command = new CreateContaCorrenteCommand(
                            new ContaCorrenteNumero(request.Numero), 
                            new ContaCorrenteNome(request.Nome), 
                            new ContaCorrenteSenha(request.Senha));
                        var id = await mediator.Send(command, cancellationToken);
                        return Results.Created($"/conta/{id}", new { id });
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
    }
}

public sealed record CreateContaCorrenteRequest(string Numero, string Nome, string Senha);
