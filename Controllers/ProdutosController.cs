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
/// Controller para gerenciar produtos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly CalculoImpostoProdutosContext _context;
    private const int ITENS_POR_PAGINA = 10;

    public ProdutosController(CalculoImpostoProdutosContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todos os produtos com paginação
    /// </summary>
    /// <param name="pagina">Número da página (padrão: 1)</param>
    /// <param name="ativo">Filtrar por status ativo (opcional)</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDTO<PaginatedResponseDTO<ProdutoListaDTO>>>> GetProdutos(
        [FromQuery] int pagina = 1,
        [FromQuery] bool? ativo = null)
    {
        try
        {
            if (pagina < 1)
                return BadRequest(ApiResponseDTO<object>.Erro("O número da página deve ser maior que 0"));

            var query = _context.Produtos
                .Include(p => p.IdTipoNavigation)
                .Include(p => p.IdUnidadeNavigation)
                .AsQueryable();

            if (ativo.HasValue)
                query = query.Where(p => p.Ativo == ativo.Value);

            var totalRegistros = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)ITENS_POR_PAGINA);

            var produtos = await query
                .OrderByDescending(p => p.DataCriacao)
                .Skip((pagina - 1) * ITENS_POR_PAGINA)
                .Take(ITENS_POR_PAGINA)
                .Select(p => new ProdutoListaDTO
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    ValorUnitario = p.ValorUnitario,
                    SiglaUnidade = p.IdUnidadeNavigation.Sigla,
                    DescricaoTipo = p.IdTipoNavigation.Descricao,
                    AliquotaTipo = p.IdTipoNavigation.Aliquota,
                    Ativo = p.Ativo,
                    DataCriacao = p.DataCriacao
                })
                .ToListAsync();

            var resposta = new PaginatedResponseDTO<ProdutoListaDTO>
            {
                Dados = produtos,
                TotalRegistros = totalRegistros,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas,
                ItensPorPagina = ITENS_POR_PAGINA
            };

            return Ok(ApiResponseDTO<PaginatedResponseDTO<ProdutoListaDTO>>.Sucesso(
                resposta, 
                $"Total de {totalRegistros} produtos encontrados"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao buscar produtos", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Obtém um produto específico pelo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDTO<ProdutoDTO>>> GetProduto(int id)
    {
        try
        {
            var produto = await _context.Produtos
                .Include(p => p.IdTipoNavigation)
                .Include(p => p.IdUnidadeNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                return NotFound(ApiResponseDTO<object>.Erro("Produto não encontrado"));

            var dto = new ProdutoDTO
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Caracteristicas = produto.Caracteristicas,
                ValorUnitario = produto.ValorUnitario,
                IdUnidade = produto.IdUnidade,
                SiglaUnidade = produto.IdUnidadeNavigation.Sigla,
                DescricaoUnidade = produto.IdUnidadeNavigation.Descricao,
                IdTipo = produto.IdTipo,
                DescricaoTipo = produto.IdTipoNavigation.Descricao,
                AliquotaTipo = produto.IdTipoNavigation.Aliquota,
                Ativo = produto.Ativo,
                DataCriacao = produto.DataCriacao,
                DataAtualizacao = produto.DataAtualizacao
            };

            return Ok(ApiResponseDTO<ProdutoDTO>.Sucesso(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao buscar produto", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Cria um novo produto
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDTO<ProdutoDTO>>> CreateProduto([FromBody] CreateProdutoDTO createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(ApiResponseDTO<object>.Erro("Dados inválidos", erros));
            }

            // Validar se unidade existe
            var unidade = await _context.Unidades.FindAsync(createDto.IdUnidade);
            if (unidade == null)
                return BadRequest(ApiResponseDTO<object>.Erro("Unidade não encontrada"));

            // Validar se tipo existe
            var tipo = await _context.TiposProdutos.FindAsync(createDto.IdTipo);
            if (tipo == null)
                return BadRequest(ApiResponseDTO<object>.Erro("Tipo de produto não encontrado"));

            var produto = new Produto
            {
                Nome = createDto.Nome,
                Caracteristicas = createDto.Caracteristicas,
                ValorUnitario = createDto.ValorUnitario,
                IdUnidade = createDto.IdUnidade,
                IdTipo = createDto.IdTipo,
                Ativo = true,
                DataCriacao = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            var dto = new ProdutoDTO
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Caracteristicas = produto.Caracteristicas,
                ValorUnitario = produto.ValorUnitario,
                IdUnidade = produto.IdUnidade,
                SiglaUnidade = unidade.Sigla,
                DescricaoUnidade = unidade.Descricao,
                IdTipo = produto.IdTipo,
                DescricaoTipo = tipo.Descricao,
                AliquotaTipo = tipo.Aliquota,
                Ativo = produto.Ativo,
                DataCriacao = produto.DataCriacao,
                DataAtualizacao = produto.DataAtualizacao
            };

            return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, 
                ApiResponseDTO<ProdutoDTO>.Sucesso(dto, "Produto criado com sucesso"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao criar produto", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Atualiza um produto existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDTO<ProdutoDTO>>> UpdateProduto(int id, [FromBody] UpdateProdutoDTO updateDto)
    {
        try
        {
            if (id != updateDto.Id)
                return BadRequest(ApiResponseDTO<object>.Erro("ID da URL não corresponde ao ID do corpo"));

            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(ApiResponseDTO<object>.Erro("Dados inválidos", erros));
            }

            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound(ApiResponseDTO<object>.Erro("Produto não encontrado"));

            // Validar se unidade existe
            var unidade = await _context.Unidades.FindAsync(updateDto.IdUnidade);
            if (unidade == null)
                return BadRequest(ApiResponseDTO<object>.Erro("Unidade não encontrada"));

            // Validar se tipo existe
            var tipo = await _context.TiposProdutos.FindAsync(updateDto.IdTipo);
            if (tipo == null)
                return BadRequest(ApiResponseDTO<object>.Erro("Tipo de produto não encontrado"));

            produto.Nome = updateDto.Nome;
            produto.Caracteristicas = updateDto.Caracteristicas;
            produto.ValorUnitario = updateDto.ValorUnitario;
            produto.IdUnidade = updateDto.IdUnidade;
            produto.IdTipo = updateDto.IdTipo;
            produto.DataAtualizacao = DateTime.Now;

            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();

            var dto = new ProdutoDTO
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Caracteristicas = produto.Caracteristicas,
                ValorUnitario = produto.ValorUnitario,
                IdUnidade = produto.IdUnidade,
                SiglaUnidade = unidade.Sigla,
                DescricaoUnidade = unidade.Descricao,
                IdTipo = produto.IdTipo,
                DescricaoTipo = tipo.Descricao,
                AliquotaTipo = tipo.Aliquota,
                Ativo = produto.Ativo,
                DataCriacao = produto.DataCriacao,
                DataAtualizacao = produto.DataAtualizacao
            };

            return Ok(ApiResponseDTO<ProdutoDTO>.Sucesso(dto, "Produto atualizado com sucesso"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao atualizar produto", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Deleta um produto (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDTO<object>>> DeleteProduto(int id)
    {
        try
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound(ApiResponseDTO<object>.Erro("Produto não encontrado"));

            produto.Ativo = false;
            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();

            return Ok(ApiResponseDTO<object>.Sucesso(null, "Produto deletado com sucesso"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao deletar produto", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Busca produtos por nome
    /// </summary>
    [HttpGet("buscar/{termo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDTO<List<ProdutoListaDTO>>>> BuscarPorNome(string termo)
    {
        try
        {
            var produtos = await _context.Produtos
                .Include(p => p.IdTipoNavigation)
                .Include(p => p.IdUnidadeNavigation)
                .Where(p => p.Nome.Contains(termo) && p.Ativo == true)
                .Select(p => new ProdutoListaDTO
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    ValorUnitario = p.ValorUnitario,
                    SiglaUnidade = p.IdUnidadeNavigation.Sigla,
                    DescricaoTipo = p.IdTipoNavigation.Descricao,
                    AliquotaTipo = p.IdTipoNavigation.Aliquota,
                    Ativo = p.Ativo,
                    DataCriacao = p.DataCriacao
                })
                .ToListAsync();

            return Ok(ApiResponseDTO<List<ProdutoListaDTO>>.Sucesso(
                produtos, 
                $"{produtos.Count} produtos encontrados"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.Erro(
                "Erro ao buscar produtos", 
                new List<string> { ex.Message }));
        }
    }
}
