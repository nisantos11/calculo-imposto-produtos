using System;
using System.ComponentModel.DataAnnotations;

namespace ImpostoLula.DTOs;

/// <summary>
/// DTO para criação e atualização de tipos de produto
/// </summary>
public class CreateTiposProdutoDTO
{
    [Required(ErrorMessage = "A descrição é obrigatória")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 50 caracteres")]
    public string Descricao { get; set; } = null!;

    [Required(ErrorMessage = "A alíquota é obrigatória")]
    [Range(0, 100, ErrorMessage = "A alíquota deve estar entre 0 e 100")]
    public decimal Aliquota { get; set; }
}

/// <summary>
/// DTO para atualização de tipos de produto
/// </summary>
public class UpdateTiposProdutoDTO : CreateTiposProdutoDTO
{
    [Required(ErrorMessage = "O ID é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "ID inválido")]
    public int Id { get; set; }
}

/// <summary>
/// DTO para resposta de tipo de produto
/// </summary>
public class TiposProdutoDTO
{
    public int Id { get; set; }

    public string Descricao { get; set; } = null!;

    public decimal Aliquota { get; set; }

    public bool? Ativo { get; set; }

    public DateTime DataCriacao { get; set; }
}
