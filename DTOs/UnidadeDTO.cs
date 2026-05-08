using System;
using System.ComponentModel.DataAnnotations;

namespace ImpostoLula.DTOs;

/// <summary>
/// DTO para criação e atualização de unidades
/// </summary>
public class CreateUnidadeDTO
{
    [Required(ErrorMessage = "A sigla é obrigatória")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "A sigla deve ter entre 1 e 10 caracteres")]
    public string Sigla { get; set; } = null!;

    [Required(ErrorMessage = "A descrição é obrigatória")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 100 caracteres")]
    public string Descricao { get; set; } = null!;
}

/// <summary>
/// DTO para atualização de unidades
/// </summary>
public class UpdateUnidadeDTO : CreateUnidadeDTO
{
    [Required(ErrorMessage = "O ID é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "ID inválido")]
    public int Id { get; set; }
}

/// <summary>
/// DTO para resposta de unidade
/// </summary>
public class UnidadeDTO
{
    public int Id { get; set; }

    public string Sigla { get; set; } = null!;

    public string Descricao { get; set; } = null!;

    public bool? Ativo { get; set; }

    public DateTime DataCriacao { get; set; }
}
