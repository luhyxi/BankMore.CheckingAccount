using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

namespace BankMore.CheckingAccount.Domain.MovimentoAggregate;

public class Movimento
{
    public Movimento(
        MovimentoId movimentoId,
        ContaCorrenteId  contaCorrenteId,
        DateTime dataMovimento,
        TipoMovimento tipoMovimento)
    {
        MovimentoId = movimentoId;
        ContaCorrenteId = contaCorrenteId;
        DataMovimento = dataMovimento;
        TipoMovimento = tipoMovimento;
    }

    public MovimentoId MovimentoId{ get;}
    public ContaCorrenteId ContaCorrenteId{ get;}
    public DateTime DataMovimento { get; }
    public TipoMovimento TipoMovimento { get; }
    public decimal Valor { get; } // Remove primitive 
}