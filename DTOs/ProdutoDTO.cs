using System;
using System.ComponentModel.DataAnnotations;

namespace ImpostoLula.DTOs;

/// <summary>
/// DTO para criação e atualização de produtos
/// </summary>
public class CreateProdutoDTO
{
    [Required(ErrorMessage = "O nome do produto é obrigatório")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres")]
    public string Nome { get; set; } = null!;

    [Required(ErrorMessage = "As características do produto são obrigatórias")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "As características devem ter entre 10 e 1000 caracteres")]
    public string Caracteristicas { get; set; } = null!;

    [Required(ErrorMessage = "O valor unitário é obrigatório")]
    [Range(0.01, 999999.99, ErrorMessage = "O valor unitário deve ser maior que 0")]
    public decimal ValorUnitario { get; set; }

    [Required(ErrorMessage = "A unidade é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma unidade válida")]
    public int IdUnidade { get; set; }

    [Required(ErrorMessage = "O tipo de produto é obrigatório")]
    [Range(1, 5, ErrorMessage = "Selecione um tipo de produto válido")]
    public int IdTipo { get; set; }
}

/// <summary>
/// DTO para atualização de produtos
/// </summary>
public class UpdateProdutoDTO : CreateProdutoDTO
{
    [Required(ErrorMessage = "O ID do produto é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "ID inválido")]
    public int Id { get; set; }
}

/// <summary>
/// DTO para resposta de produto (leitura)
/// </summary>
public class ProdutoDTO
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Caracteristicas { get; set; } = null!;

    public decimal ValorUnitario { get; set; }

    public int IdUnidade { get; set; }

    public string? SiglaUnidade { get; set; }

    public string? DescricaoUnidade { get; set; }

    public int IdTipo { get; set; }

    public string? DescricaoTipo { get; set; }

    public decimal? AliquotaTipo { get; set; }

    public bool? Ativo { get; set; }

    public DateTime DataCriacao { get; set; }

    public DateTime DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para listagem de produtos com paginação
/// </summary>
public class ProdutoListaDTO
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public decimal ValorUnitario { get; set; }

    public string? SiglaUnidade { get; set; }

    public string? DescricaoTipo { get; set; }

    public decimal? AliquotaTipo { get; set; }

    public bool? Ativo { get; set; }

    public DateTime DataCriacao { get; set; }
}
