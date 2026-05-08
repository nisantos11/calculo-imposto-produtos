// =====================================================
// Script para página de Produtos
// =====================================================

let produtoEmEdicao = null;
let unidades = [];
let tipos = [];

// Inicializar página
document.addEventListener('DOMContentLoaded', async function() {
    await carregarUnidades();
    await carregarTipos();
    await carregarProdutos();

    // Event listener do formulário
    document.getElementById('formProduto').addEventListener('submit', salvarProduto);
});

// Carregar unidades
async function carregarUnidades() {
    try {
        unidades = await ApiUnidades.obterTodas();
        preencherSelectUnidades();
    } catch (erro) {
        console.error('Erro ao carregar unidades:', erro);
    }
}

// Carregar tipos
async function carregarTipos() {
    try {
        tipos = await ApiTipos.obterTodos();
        preencherSelectTipos();
    } catch (erro) {
        console.error('Erro ao carregar tipos:', erro);
    }
}

// Preencher select de unidades
function preencherSelectUnidades() {
    const select = document.getElementById('idUnidade');
    select.innerHTML = '<option value="">Selecione uma unidade</option>';
    
    unidades.forEach(unidade => {
        const option = document.createElement('option');
        option.value = unidade.id;
        option.textContent = `${unidade.sigla} - ${unidade.descricao}`;
        select.appendChild(option);
    });
}

// Preencher select de tipos
function preencherSelectTipos() {
    const select = document.getElementById('idTipo');
    select.innerHTML = '<option value="">Selecione um tipo</option>';
    
    tipos.forEach(tipo => {
        const option = document.createElement('option');
        option.value = tipo.id;
        option.textContent = `${tipo.descricao} (${tipo.aliquota}%)`;
        select.appendChild(option);
    });
}

// Carregar produtos
async function carregarProdutos(pagina = 1) {
    try {
        document.getElementById('carregando').style.display = 'block';
        document.getElementById('tabelaProdutos').style.display = 'none';
        document.getElementById('vazio').style.display = 'none';

        const dados = await ApiProdutos.obterTodos(pagina);

        if (dados.dados.length === 0) {
            document.getElementById('vazio').style.display = 'block';
        } else {
            preencherTabela(dados.dados);
            document.getElementById('tabelaProdutos').style.display = 'table';
        }

        document.getElementById('carregando').style.display = 'none';
    } catch (erro) {
        console.error('Erro ao carregar produtos:', erro);
        document.getElementById('carregando').style.display = 'none';
    }
}

// Preencher tabela
function preencherTabela(produtos) {
    const tbody = document.getElementById('corpoTabela');
    tbody.innerHTML = '';

    produtos.forEach(produto => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${produto.id}</td>
            <td>${escapeHtml(produto.nome)}</td>
            <td>${formatarMoeda(produto.valorUnitario)}</td>
            <td>${escapeHtml(produto.siglaUnidade)}</td>
            <td>${escapeHtml(produto.descricaoTipo)} (${produto.aliquotaTipo}%)</td>
            <td>
                <button class="btn-editar" onclick="editarProduto(${produto.id})">Editar</button>
                <button class="btn-deletar" onclick="deletarProduto(${produto.id})">Deletar</button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

// Abrir modal para novo produto
function abrirModalNovoProduto() {
    produtoEmEdicao = null;
    document.getElementById('tituloModal').textContent = 'Novo Produto';
    document.getElementById('formProduto').reset();
    document.getElementById('modalProduto').style.display = 'block';
}

// Editar produto
async function editarProduto(id) {
    try {
        produtoEmEdicao = await ApiProdutos.obterPorId(id);
        
        document.getElementById('tituloModal').textContent = 'Editar Produto';
        document.getElementById('nome').value = produtoEmEdicao.nome;
        document.getElementById('caracteristicas').value = produtoEmEdicao.caracteristicas;
        document.getElementById('valorUnitario').value = produtoEmEdicao.valorUnitario;
        document.getElementById('idUnidade').value = produtoEmEdicao.idUnidade;
        document.getElementById('idTipo').value = produtoEmEdicao.idTipo;
        
        document.getElementById('modalProduto').style.display = 'block';
    } catch (erro) {
        console.error('Erro ao editar produto:', erro);
    }
}

// Deletar produto
async function deletarProduto(id) {
    try {
        await ApiProdutos.deletar(id);
        await carregarProdutos();
    } catch (erro) {
        console.error('Erro ao deletar produto:', erro);
    }
}

// Salvar produto
async function salvarProduto(e) {
    e.preventDefault();

    const dados = {
        nome: document.getElementById('nome').value,
        caracteristicas: document.getElementById('caracteristicas').value,
        valorUnitario: parseFloat(document.getElementById('valorUnitario').value),
        idUnidade: parseInt(document.getElementById('idUnidade').value),
        idTipo: parseInt(document.getElementById('idTipo').value)
    };

    try {
        if (produtoEmEdicao) {
            dados.id = produtoEmEdicao.id;
            await ApiProdutos.atualizar(produtoEmEdicao.id, dados);
        } else {
            await ApiProdutos.criar(dados);
        }

        fecharModal();
        await carregarProdutos();
    } catch (erro) {
        console.error('Erro ao salvar produto:', erro);
    }
}

// Fechar modal
function fecharModal() {
    document.getElementById('modalProduto').style.display = 'none';
    produtoEmEdicao = null;
}

// Fechar modal ao clicar fora
window.onclick = function(event) {
    const modal = document.getElementById('modalProduto');
    if (event.target === modal) {
        fecharModal();
    }
}
