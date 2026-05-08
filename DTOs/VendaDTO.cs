using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ImpostoLula.DTOs;

/// <summary>
/// DTO para criação de venda
/// </summary>
public class CreateVendaDTO
{
    [Required(ErrorMessage = "Os itens da venda são obrigatórios")]
    [MinLength(1, ErrorMessage = "A venda deve ter pelo menos um item")]
    public List<CreateItensVendaDTO> Itens { get; set; } = new();

    [StringLength(500, ErrorMessage = "As observações não podem ter mais de 500 caracteres")]
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para item de venda na criação
/// </summary>
public class CreateItensVendaDTO
{
    [Required(ErrorMessage = "O ID do produto é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "ID do produto inválido")]
    public int IdProduto { get; set; }

    [Required(ErrorMessage = "A quantidade é obrigatória")]
    [Range(1, 999999, ErrorMessage = "A quantidade deve ser maior que 0")]
    public int Quantidade { get; set; }
}

/// <summary>
/// DTO para resposta de venda (leitura)
/// </summary>
public class VendaDTO
{
    public int Id { get; set; }

    public DateTime DataVenda { get; set; }

    public decimal ValorTotalItens { get; set; }

    public decimal ValorTotalImpostos { get; set; }

    public decimal ValorFinal { get; set; }

    public string? Observacoes { get; set; }

    public DateTime DataCriacao { get; set; }

    public List<ItensVendaDTO> Itens { get; set; } = new();
}

/// <summary>
/// DTO para listagem de vendas
/// </summary>
public class VendaListaDTO
{
    public int Id { get; set; }

    public DateTime DataVenda { get; set; }

    public int TotalItens { get; set; }

    public decimal ValorTotalItens { get; set; }

    public decimal ValorTotalImpostos { get; set; }

    public decimal ValorFinal { get; set; }

    public decimal PercentualImposto { get; set; }

    public DateTime DataCriacao { get; set; }
}

/// <summary>
/// DTO para item de venda (resposta)
/// </summary>
public class ItensVendaDTO
{
    public int Id { get; set; }

    public int IdVenda { get; set; }

    public int IdProduto { get; set; }

    public string? NomeProduto { get; set; }

    public int Quantidade { get; set; }

    public decimal ValorUnitario { get; set; }

    public decimal ValorTotalItem { get; set; }

    public decimal AliquotaImposto { get; set; }

    public decimal ValorImposto { get; set; }

    public decimal ValorFinalItem { get; set; }

    public DateTime DataCriacao { get; set; }
}

/// <summary>
/// DTO para relatório de vendas
/// </summary>
public class RelatorioVendasDTO
{
    public DateTime Data { get; set; }

    public int TotalVendas { get; set; }

    public decimal ValorItens { get; set; }

    public decimal ValorImpostos { get; set; }

    public decimal ValorFinal { get; set; }

    public decimal TicketMedio { get; set; }
}
