using System.ComponentModel.DataAnnotations.Schema;
using SharedKernel;

namespace BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

public class ContaCorrente : IAggregateRoot
{
    public ContaCorrenteId Id { get; private set; } = default!;
    
    public ContaCorrenteCpf Cpf{ get; private set; } = default!;

    public ContaCorrenteNumero Numero { get; private set; } = default!;

    public ContaCorrenteNome Nome { get; private set; } = default!;
    
    public ContaCorrenteSenha Senha { get; private set; } = default!;

    public string Salt { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public ContaCorrente(
        ContaCorrenteId id,
        ContaCorrenteNumero numero,
        ContaCorrenteNome nome,
        ContaCorrenteCpf cpf,
        ContaCorrenteSenha senha,
        string salt,
        bool ativo = true)
    {
        Id = id;
        Numero = numero;
        Nome = nome;
        Cpf = cpf;
        Senha = senha;
        Salt = salt;
        Ativo = ativo;
    }

    private ContaCorrente()
    {
    }
}
