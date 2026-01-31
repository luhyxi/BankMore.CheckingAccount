using BankMore.CheckingAccount.Domain.Interfaces;

using SharedKernel;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Command.Login;

public class LoginContaCorrenteHandler(
    IContaCorrenteRepository repository,
    IPasswordHashingService passwordHashingService,
    IJwtService jwtService)
    : ICommandHandler<LoginContaCorrenteCommand, IResult<string>>
{
    public async ValueTask<IResult<string>> Handle(LoginContaCorrenteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var conta = command.IsCpf
                ? await repository.GetByCpfAsync(command.cpf, cancellationToken)
                : await repository.GetByNumeroAsync(command.numero, cancellationToken);

            if (!conta.Ativo)
            {
                return Result<string>.Failure("Conta corrente is inactive.");
            }

            if (!passwordHashingService.Verify(command.senha.Value, conta.Salt, conta.Senha.Value))
            {
                return Result<string>.Failure("Invalid credentials.");
            }

            var token = jwtService.GenerateToken(conta);
            return Result<string>.Success(token);
        }
        catch (ArgumentException ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
