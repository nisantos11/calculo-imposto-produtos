using System;
using System.Collections.Generic;

namespace ImpostoLula.DTOs;

/// <summary>
/// DTO genérico para respostas da API
/// </summary>
public class ApiResponseDTO<T>
{
    public bool Sucesso { get; set; }

    public string? Mensagem { get; set; }

    public T? Dados { get; set; }

    public List<string>? Erros { get; set; }

    public DateTime DataResposta { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cria uma resposta de sucesso
    /// </summary>
    public static ApiResponseDTO<T> Sucesso(T dados, string mensagem = "Operação realizada com sucesso")
    {
        return new ApiResponseDTO<T>
        {
            Sucesso = true,
            Mensagem = mensagem,
            Dados = dados
        };
    }

    /// <summary>
    /// Cria uma resposta de erro
    /// </summary>
    public static ApiResponseDTO<T> Erro(string mensagem, List<string>? erros = null)
    {
        return new ApiResponseDTO<T>
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = erros ?? new List<string>()
        };
    }
}

/// <summary>
/// DTO para resposta paginada
/// </summary>
public class PaginatedResponseDTO<T>
{
    public List<T> Dados { get; set; } = new();

    public int TotalRegistros { get; set; }

    public int PaginaAtual { get; set; }

    public int TotalPaginas { get; set; }

    public int ItensPorPagina { get; set; }

    public bool TemProxima => PaginaAtual < TotalPaginas;

    public bool TemAnterior => PaginaAtual > 1;
}
