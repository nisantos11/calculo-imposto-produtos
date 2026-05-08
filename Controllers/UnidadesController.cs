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
/// Controller para gerenciar unidades de medida
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UnidadesController : ControllerBase
{
    private readonly CalculoImpostoProdutosContext _context;

    public UnidadesController(CalculoImpostoProdutosContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todas as unidades ativas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDTO<List<UnidadeDTO>>>> GetUnidades()
    {
        try
        {
            var unidades = await _context.Unidades
                .Where(u => u.Ativo == true)
                .Select(u => new UnidadeDTO
                {
                    Id = u.Id,
                    Sigla = u.Sigla,
                    Descricao = u.Descricao,
                    Ativo = u.Ativo,
                    DataCriacao = u.DataCriacao
                })
                .OrderBy(u => u.Sigla)
                .ToListAsync();

            return Ok(ApiResponseDTO<List<UnidadeDTO>>.CriarSucesso(
                unidades, 
                $"Total de {unidades.Count} unidades encontradas"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.CriarErro(
                "Erro ao buscar unidades", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Obtém uma unidade específica
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDTO<UnidadeDTO>>> GetUnidade(int id)
    {
        try
        {
            var unidade = await _context.Unidades.FindAsync(id);

            if (unidade == null)
                return NotFound(ApiResponseDTO<object>.CriarErro("Unidade não encontrada"));

            var dto = new UnidadeDTO
            {
                Id = unidade.Id,
                Sigla = unidade.Sigla,
                Descricao = unidade.Descricao,
                Ativo = unidade.Ativo,
                DataCriacao = unidade.DataCriacao
            };

            return Ok(ApiResponseDTO<UnidadeDTO>.CriarSucesso(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.CriarErro(
                "Erro ao buscar unidade", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Cria uma nova unidade
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDTO<UnidadeDTO>>> CreateUnidade([FromBody] CreateUnidadeDTO createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(ApiResponseDTO<object>.CriarErro("Dados inválidos", erros));
            }

            // Verificar se já existe unidade com mesma sigla
            var existe = await _context.Unidades
                .AnyAsync(u => u.Sigla.ToUpper() == createDto.Sigla.ToUpper());

            if (existe)
                return BadRequest(ApiResponseDTO<object>.CriarErro("Já existe uma unidade com esta sigla"));

            var unidade = new Unidade
            {
                Sigla = createDto.Sigla.ToUpper(),
                Descricao = createDto.Descricao,
                Ativo = true,
                DataCriacao = DateTime.Now
            };

            _context.Unidades.Add(unidade);
            await _context.SaveChangesAsync();

            var dto = new UnidadeDTO
            {
                Id = unidade.Id,
                Sigla = unidade.Sigla,
                Descricao = unidade.Descricao,
                Ativo = unidade.Ativo,
                DataCriacao = unidade.DataCriacao
            };

            return CreatedAtAction(nameof(GetUnidade), new { id = unidade.Id }, 
                ApiResponseDTO<UnidadeDTO>.CriarSucesso(dto, "Unidade criada com sucesso"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.CriarErro(
                "Erro ao criar unidade", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Atualiza uma unidade
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDTO<UnidadeDTO>>> UpdateUnidade(int id, [FromBody] UpdateUnidadeDTO updateDto)
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

            var unidade = await _context.Unidades.FindAsync(id);
            if (unidade == null)
                return NotFound(ApiResponseDTO<object>.CriarErro("Unidade não encontrada"));

            // Verificar se já existe outra unidade com mesma sigla
            var existe = await _context.Unidades
                .AnyAsync(u => u.Sigla.ToUpper() == updateDto.Sigla.ToUpper() && u.Id != id);

            if (existe)
                return BadRequest(ApiResponseDTO<object>.CriarErro("Já existe outra unidade com esta sigla"));

            unidade.Sigla = updateDto.Sigla.ToUpper();
            unidade.Descricao = updateDto.Descricao;

            _context.Unidades.Update(unidade);
            await _context.SaveChangesAsync();

            var dto = new UnidadeDTO
            {
                Id = unidade.Id,
                Sigla = unidade.Sigla,
                Descricao = unidade.Descricao,
                Ativo = unidade.Ativo,
                DataCriacao = unidade.DataCriacao
            };

            return Ok(ApiResponseDTO<UnidadeDTO>.CriarSucesso(dto, "Unidade atualizada com sucesso"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.CriarErro(
                "Erro ao atualizar unidade", 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Deleta uma unidade (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDTO<object>>> DeleteUnidade(int id)
    {
        try
        {
            var unidade = await _context.Unidades.FindAsync(id);
            if (unidade == null)
                return NotFound(ApiResponseDTO<object>.CriarErro("Unidade não encontrada"));

            // Verificar se existem produtos com esta unidade
            var temProdutos = await _context.Produtos.AnyAsync(p => p.IdUnidade == id);
            if (temProdutos)
                return BadRequest(ApiResponseDTO<object>.CriarErro("Não é possível deletar uma unidade que possui produtos associados"));

            unidade.Ativo = false;
            _context.Unidades.Update(unidade);
            await _context.SaveChangesAsync();

            return Ok(ApiResponseDTO<object>.CriarSucesso(null, "Unidade deletada com sucesso"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDTO<object>.CriarErro(
                "Erro ao deletar unidade", 
                new List<string> { ex.Message }));
        }
    }
}
