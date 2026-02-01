using System.ComponentModel.DataAnnotations.Schema;
using SharedKernel;

namespace BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

// Anotations are a temporary workaround before adding a mapper
public class ContaCorrente : IAggregateRoot
{
    public ContaCorrenteId Id { get; private set; } = default!;
    
    public ContaCorrenteCpf Cpf{ get; private set; } = default!;

    public ContaCorrenteNumero Numero { get; private set; } = default!;

    public ContaCorrenteNome Nome { get; private set; } = default!;
    
    public decimal Saldo { get; private set; } = default!;

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
        bool ativo = true,
        decimal saldo = 0.0m)
    {
        Id = id;
        Numero = numero;
        Nome = nome;
        Cpf = cpf;
        Senha = senha;
        Salt = salt;
        Saldo = saldo;
        Ativo = ativo;
    }

    private ContaCorrente()
    {
    }
}
