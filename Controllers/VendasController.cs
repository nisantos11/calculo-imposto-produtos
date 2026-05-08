using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImpostoLula.Models;
using ImpostoLula.DTOs;

namespace ImpostoLula.Controllers;

/// <summary>
/// Controller para gerenciar vendas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VendasController : ControllerBase
{
    private readonly CalculoImpostoProdutosContext _context;
    private const int ITENS_POR_PAGINA = 10;

    public VendasController(CalculoImpostoProdutosContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todas as vendas com paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDTO<PaginatedResponseDTO<VendaListaDTO>>>> GetVendas(
        [FromQuery] int pagina = 1,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            if (pagina < 1)
                return BadRequest(ApiResponseDTO<object>.Erro("O número da página deve ser maior que 0"));

            var query = _context.Vendas
                .Include(v => v.ItensVenda)
                .AsQueryable();

            if (dataInicio.HasValue)
                query = query.Where(v => v.DataVenda >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(v => v.DataVenda <= dataFim.Value);

            var totalRegistros = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)ITENS_POR_PAGINA);

            var vendas = await query
                .OrderByDescending(v => v.DataVenda)
                .Skip((pagina - 1) * ITENS_POR_PAGINA)
                .Take(ITENS_POR_PAGINA)
                .Select(v => new VendaListaDTO
                {
                    Id = v.Id,
                    DataVenda = v.DataVenda,
                    TotalItens = v.ItensVenda.Count,
                    ValorTotalItens = v.ValorTotalItens,
                    ValorTotalImpostos = v.ValorTotalImpostos,
                    ValorFinal = v.ValorFinal,
                    PercentualImposto = v.ValorTotalItens > 0 
                        ? Math.Round((v.ValorTotalImpostos / v.ValorTotalItens) * 100, 2) 
                        : 0,
                    DataCriacao = v.DataCriacao
                })
                .ToListAsync();

            var resposta = new PaginatedResponseDTO<VendaListaDTO>
            {
                Dados = vendas,
                TotalRegistros = totalRegistros,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas,
                ItensPorPagina = ITENS_POR_PAGINA
            };

            return Ok(ApiResponseDTO<PaginatedResponseDTO<VendaListaDTO>>.Sucesso(
                resposta, 
                $"Total de {totalRegistros} vendas encontradas"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao buscar vendas", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Obtém uma venda específica pelo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDTO<VendaDTO>>> GetVenda(int id)
    {
        try
        {
            var venda = await _context.Vendas
                .Include(v => v.ItensVenda)
                .ThenInclude(iv => iv.IdProdutoNavigation)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
                return NotFound(ApiResponseDTO<object>.Erro("Venda não encontrada"));

            var dto = new VendaDTO
            {
                Id = venda.Id,
                DataVenda = venda.DataVenda,
                ValorTotalItens = venda.ValorTotalItens,
                ValorTotalImpostos = venda.ValorTotalImpostos,
                ValorFinal = venda.ValorFinal,
                Observacoes = venda.Observacoes,
                DataCriacao = venda.DataCriacao,
                Itens = venda.ItensVenda.Select(iv => new ItensVendaDTO
                {
                    Id = iv.Id,
                    IdVenda = iv.IdVenda,
                    IdProduto = iv.IdProduto,
                    NomeProduto = iv.IdProdutoNavigation.Nome,
                    Quantidade = iv.Quantidade,
                    ValorUnitario = iv.ValorUnitario,
                    ValorTotalItem = iv.ValorTotalItem,
                    AliquotaImposto = iv.AliquotaImposto,
                    ValorImposto = iv.ValorImposto,
                    ValorFinalItem = iv.ValorFinalItem,
                    DataCriacao = iv.DataCriacao
                }).ToList()
            };

            return Ok(ApiResponseDTO<VendaDTO>.Sucesso(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao buscar venda", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Cria uma nova venda com seus itens
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDTO<VendaDTO>>> CreateVenda([FromBody] CreateVendaDTO createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(ApiResponseDTO<object>.Erro("Dados inválidos", erros));
            }

            if (!createDto.Itens.Any())
                return BadRequest(ApiResponseDTO<object>.Erro("A venda deve conter pelo menos um item"));

            // Buscar todos os produtos da venda
            var produtoIds = createDto.Itens.Select(i => i.IdProduto).Distinct().ToList();
            var produtos = await _context.Produtos
                .Include(p => p.IdTipoNavigation)
                .Where(p => produtoIds.Contains(p.Id))
                .ToListAsync();

            if (produtos.Count != produtoIds.Count)
                return BadRequest(ApiResponseDTO<object>.Erro("Um ou mais produtos não foram encontrados"));

            // Calcular totais
            decimal valorTotalItens = 0;
            decimal valorTotalImpostos = 0;
            var itensVenda = new List<ItensVendum>();

            foreach (var item in createDto.Itens)
            {
                var produto = produtos.FirstOrDefault(p => p.Id == item.IdProduto);
                if (produto == null)
                    return BadRequest(ApiResponseDTO<object>.Erro($"Produto {item.IdProduto} não encontrado"));

                decimal valorTotalItem = item.Quantidade * produto.ValorUnitario;
                decimal aliquota = produto.IdTipoNavigation.Aliquota / 100;
                decimal valorImposto = valorTotalItem * aliquota;
                decimal valorFinalItem = valorTotalItem + valorImposto;

                valorTotalItens += valorTotalItem;
                valorTotalImpostos += valorImposto;

                itensVenda.Add(new ItensVendum
                {
                    IdProduto = item.IdProduto,
                    Quantidade = item.Quantidade,
                    ValorUnitario = produto.ValorUnitario,
                    ValorTotalItem = valorTotalItem,
                    AliquotaImposto = produto.IdTipoNavigation.Aliquota,
                    ValorImposto = valorImposto,
                    ValorFinalItem = valorFinalItem,
                    DataCriacao = DateTime.Now
                });
            }

            var venda = new Venda
            {
                DataVenda = DateTime.Now,
                ValorTotalItens = valorTotalItens,
                ValorTotalImpostos = valorTotalImpostos,
                ValorFinal = valorTotalItens + valorTotalImpostos,
                Observacoes = createDto.Observacoes,
                ItensVenda = itensVenda,
                DataCriacao = DateTime.Now
            };

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();

            var dto = new VendaDTO
            {
                Id = venda.Id,
                DataVenda = venda.DataVenda,
                ValorTotalItens = venda.ValorTotalItens,
                ValorTotalImpostos = venda.ValorTotalImpostos,
                ValorFinal = venda.ValorFinal,
                Observacoes = venda.Observacoes,
                DataCriacao = venda.DataCriacao,
                Itens = venda.ItensVenda.Select(iv => new ItensVendaDTO
                {
                    Id = iv.Id,
                    IdVenda = iv.IdVenda,
                    IdProduto = iv.IdProduto,
                    NomeProduto = produtos.FirstOrDefault(p => p.Id == iv.IdProduto)?.Nome,
                    Quantidade = iv.Quantidade,
                    ValorUnitario = iv.ValorUnitario,
                    ValorTotalItem = iv.ValorTotalItem,
                    AliquotaImposto = iv.AliquotaImposto,
                    ValorImposto = iv.ValorImposto,
                    ValorFinalItem = iv.ValorFinalItem,
                    DataCriacao = iv.DataCriacao
                }).ToList()
            };

            return CreatedAtAction(nameof(GetVenda), new { id = venda.Id }, 
                ApiResponseDTO<VendaDTO>.Sucesso(dto, "Venda criada com sucesso"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao criar venda", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Obtém relatório de vendas por período
    /// </summary>
    [HttpGet("relatorio/periodo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDTO<List<RelatorioVendasDTO>>>> GetRelatorioPeriodo(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim)
    {
        try
        {
            var relatorio = await _context.Vendas
                .Where(v => v.DataVenda >= dataInicio && v.DataVenda <= dataFim)
                .GroupBy(v => v.DataVenda.Date)
                .Select(g => new RelatorioVendasDTO
                {
                    Data = g.Key,
                    TotalVendas = g.Count(),
                    ValorItens = g.Sum(v => v.ValorTotalItens),
                    ValorImpostos = g.Sum(v => v.ValorTotalImpostos),
                    ValorFinal = g.Sum(v => v.ValorFinal),
                    TicketMedio = g.Average(v => v.ValorFinal)
                })
                .OrderByDescending(r => r.Data)
                .ToListAsync();

            return Ok(ApiResponseDTO<List<RelatorioVendasDTO>>.Sucesso(
                relatorio, 
                $"Relatório com {relatorio.Count} dias"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao gerar relatório", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Obtém resumo de vendas por tipo de produto
    /// </summary>
    [HttpGet("relatorio/por-tipo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDTO<object>>> GetRelatorioPorTipo()
    {
        try
        {
            var relatorio = await _context.ItensVenda
                .Include(iv => iv.IdProdutoNavigation)
                .ThenInclude(p => p.IdTipoNavigation)
                .GroupBy(iv => iv.IdProdutoNavigation.IdTipoNavigation.Descricao)
                .Select(g => new
                {
                    TipoProduto = g.Key,
                    TotalItens = g.Count(),
                    QuantidadeTotal = g.Sum(iv => iv.Quantidade),
                    ValorTotalItens = g.Sum(iv => iv.ValorTotalItem),
                    ValorTotalImpostos = g.Sum(iv => iv.ValorImposto),
                    ValorFinalTotal = g.Sum(iv => iv.ValorFinalItem)
                })
                .OrderByDescending(r => r.ValorFinalTotal)
                .ToListAsync();

            return Ok(ApiResponseDTO<object>.Sucesso(relatorio, "Relatório por tipo de produto"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao gerar relatório", 
                new List<string> { ex.Message }));
        }
    }
}
