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
/// Controller para gerenciar tipos de produtos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TiposProdutoController : ControllerBase
{
    private readonly CalculoImpostoProdutosContext _context;

    public TiposProdutoController(CalculoImpostoProdutosContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todos os tipos de produtos ativos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDTO<List<TiposProdutoDTO>>>> GetTiposProduto()
    {
        try
        {
            var tipos = await _context.TiposProdutos
                .Where(t => t.Ativo == true)
                .Select(t => new TiposProdutoDTO
                {
                    Id = t.Id,
                    Descricao = t.Descricao,
                    Aliquota = t.Aliquota,
                    Ativo = t.Ativo,
                    DataCriacao = t.DataCriacao
                })
                .OrderBy(t => t.Id)
                .ToListAsync();

            return Ok(ApiResponseDTO<List<TiposProdutoDTO>>.CriarSucesso(
                tipos, 
                $"Total de {tipos.Count} tipos encontrados"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.CriarErro(
                "Erro ao buscar tipos de produtos", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Obtém um tipo de produto específico
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDTO<TiposProdutoDTO>>> GetTipoProduto(int id)
    {
        try
        {
            var tipo = await _context.TiposProdutos.FindAsync(id);

            if (tipo == null)
                return NotFound(ApiResponseDTO<object>.CriarErro("Tipo de produto não encontrado"));

            var dto = new TiposProdutoDTO
            {
                Id = tipo.Id,
                Descricao = tipo.Descricao,
                Aliquota = tipo.Aliquota,
                Ativo = tipo.Ativo,
                DataCriacao = tipo.DataCriacao
            };

            return Ok(ApiResponseDTO<TiposProdutoDTO>.CriarSucesso(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.CriarErro(
                "Erro ao buscar tipo de produto", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Cria um novo tipo de produto
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDTO<TiposProdutoDTO>>> CreateTipoProduto([FromBody] CreateTiposProdutoDTO createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(ApiResponseDTO<object>.CriarErro("Dados inválidos", erros));
            }

            // Verificar se já existe tipo com mesma descrição
            var existe = await _context.TiposProdutos
                .AnyAsync(t => t.Descricao.ToLower() == createDto.Descricao.ToLower());

            if (existe)
                return BadRequest(ApiResponseDTO<object>.CriarErro("Já existe um tipo com esta descrição"));

            var tipo = new TiposProduto
            {
                Descricao = createDto.Descricao,
                Aliquota = createDto.Aliquota,
                Ativo = true,
                DataCriacao = DateTime.Now
            };

            _context.TiposProdutos.Add(tipo);
            await _context.SaveChangesAsync();

            var dto = new TiposProdutoDTO
            {
                Id = tipo.Id,
                Descricao = tipo.Descricao,
                Aliquota = tipo.Aliquota,
                Ativo = tipo.Ativo,
                DataCriacao = tipo.DataCriacao
            };

            return CreatedAtAction(nameof(GetTipoProduto), new { id = tipo.Id }, 
                ApiResponseDTO<TiposProdutoDTO>.CriarSucesso(dto, "Tipo de produto criado com sucesso"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.CriarErro(
                "Erro ao criar tipo de produto", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Atualiza um tipo de produto
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDTO<TiposProdutoDTO>>> UpdateTipoProduto(int id, [FromBody] UpdateTiposProdutoDTO updateDto)
    {
        try
        {
            if (id != updateDto.Id)
                return BadRequest(ApiResponseDTO<object>.CriarErro("ID da URL não corresponde ao ID do corpo"));

            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(ApiResponseDTO<object>.CriarErro("Dados inválidos", erros));
            }

            var tipo = await _context.TiposProdutos.FindAsync(id);
            if (tipo == null)
                return NotFound(ApiResponseDTO<object>.CriarErro("Tipo de produto não encontrado"));

            // Verificar se já existe outro tipo com mesma descrição
            var existe = await _context.TiposProdutos
                .AnyAsync(t => t.Descricao.ToLower() == updateDto.Descricao.ToLower() && t.Id != id);

            if (existe)
                return BadRequest(ApiResponseDTO<object>.CriarErro("Já existe outro tipo com esta descrição"));

            tipo.Descricao = updateDto.Descricao;
            tipo.Aliquota = updateDto.Aliquota;

            _context.TiposProdutos.Update(tipo);
            await _context.SaveChangesAsync();

            var dto = new TiposProdutoDTO
            {
                Id = tipo.Id,
                Descricao = tipo.Descricao,
                Aliquota = tipo.Aliquota,
                Ativo = tipo.Ativo,
                DataCriacao = tipo.DataCriacao
            };

            return Ok(ApiResponseDTO<TiposProdutoDTO>.CriarSucesso(dto, "Tipo de produto atualizado com sucesso"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.CriarErro(
                "Erro ao atualizar tipo de produto", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Deleta um tipo de produto (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDTO<object>>> DeleteTipoProduto(int id)
    {
        try
        {
            var tipo = await _context.TiposProdutos.FindAsync(id);
            if (tipo == null)
                return NotFound(ApiResponseDTO<object>.CriarErro("Tipo de produto não encontrado"));

            // Verificar se existem produtos com este tipo
            var temProdutos = await _context.Produtos.AnyAsync(p => p.IdTipo == id);
            if (temProdutos)
                return BadRequest(ApiResponseDTO<object>.CriarErro("Não é possível deletar um tipo que possui produtos associados"));

            tipo.Ativo = false;
            _context.TiposProdutos.Update(tipo);
            await _context.SaveChangesAsync();

            return Ok(ApiResponseDTO<object>.CriarSucesso(null, "Tipo de produto deletado com sucesso"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.CriarErro(
                "Erro ao deletar tipo de produto", 
                new List<string> { ex.Message }));
        }
    }
}
