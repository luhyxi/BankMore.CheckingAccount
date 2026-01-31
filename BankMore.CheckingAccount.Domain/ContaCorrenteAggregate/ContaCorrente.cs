using System.ComponentModel.DataAnnotations.Schema;
using SharedKernel;

namespace BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

// Anotations are a temporary workaround before adding a mapper
[Table("contacorrente")]
public class ContaCorrente : IAggregateRoot
{
    [Column("idcontacorrente")]
    public ContaCorrenteId Id { get; private set; } = default!;
    
    [Column("cpf")]
    public ContaCorrenteCpf Cpf{ get; private set; } = default!;

    [Column("numero")]
    public ContaCorrenteNumero Numero { get; private set; } = default!;

    [Column("nome")]
    public ContaCorrenteNome Nome { get; private set; } = default!;

    [Column("senha")]
    public ContaCorrenteSenha Senha { get; private set; } = default!;

    [Column("salt")]
    public string Salt { get; private set; } = string.Empty;

    [Column("ativo")]
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
