// =====================================================
// Script para página de Vendas
// =====================================================

let vendaEmEdicao = null;
let produtos = [];
let itensVendaTemp = [];

// Inicializar página
document.addEventListener('DOMContentLoaded', async function() {
    await carregarProdutos();
    await carregarVendas();

    // Event listener do formulário
    document.getElementById('formVenda').addEventListener('submit', salvarVenda);
});

// Carregar produtos
async function carregarProdutos() {
    try {
        const dados = await ApiProdutos.obterTodos();
        produtos = dados.dados;
        preencherSelectProdutos();
    } catch (erro) {
        console.error('Erro ao carregar produtos:', erro);
    }
}

// Preencher select de produtos
function preencherSelectProdutos() {
    const select = document.getElementById('idProduto');
    select.innerHTML = '<option value="">Selecione um produto</option>';
    
    produtos.forEach(produto => {
        const option = document.createElement('option');
        option.value = produto.id;
        option.textContent = `${produto.nome} - ${formatarMoeda(produto.valorUnitario)}`;
        select.appendChild(option);
    });
}

// Carregar vendas
async function carregarVendas(pagina = 1) {
    try {
        document.getElementById('carregando').style.display = 'block';
        document.getElementById('tabelaVendas').style.display = 'none';
        document.getElementById('vazio').style.display = 'none';

        const dados = await ApiVendas.obterTodas(pagina);

        if (dados.dados.length === 0) {
            document.getElementById('vazio').style.display = 'block';
        } else {
            preencherTabela(dados.dados);
            document.getElementById('tabelaVendas').style.display = 'table';
        }

        document.getElementById('carregando').style.display = 'none';
    } catch (erro) {
        console.error('Erro ao carregar vendas:', erro);
        document.getElementById('carregando').style.display = 'none';
    }
}

// Preencher tabela
function preencherTabela(vendas) {
    const tbody = document.getElementById('corpoTabela');
    tbody.innerHTML = '';

    vendas.forEach(venda => {
        const tr = document.createElement('tr');
        const data = new Date(venda.dataVenda).toLocaleDateString('pt-BR');
        
        tr.innerHTML = `
            <td>${venda.id}</td>
            <td>${data}</td>
            <td>${formatarMoeda(venda.valorTotalItens)}</td>
            <td>${formatarMoeda(venda.valorTotalImpostos)}</td>
            <td>${formatarMoeda(venda.valorFinal)}</td>
            <td>
                <button class="btn-visualizar" onclick="visualizarVenda(${venda.id})">Visualizar</button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

// Abrir modal para nova venda
function abrirModalNovaVenda() {
    vendaEmEdicao = null;
    itensVendaTemp = [];
    document.getElementById('formVenda').reset();
    document.getElementById('itensVenda').innerHTML = '';
    document.getElementById('modalVenda').style.display = 'block';
}

// Adicionar item à venda
function adicionarItemVenda() {
    const idProduto = parseInt(document.getElementById('idProduto').value);
    const quantidade = parseInt(document.getElementById('quantidade').value);

    if (!idProduto || !quantidade) {
        alert('Selecione um produto e informe a quantidade');
        return;
    }

    const produto = produtos.find(p => p.id === idProduto);
    if (!produto) {
        alert('Produto não encontrado');
        return;
    }

    const item = {
        idProduto: idProduto,
        quantidade: quantidade,
        produto: produto
    };

    itensVendaTemp.push(item);
    renderizarItensVenda();
    document.getElementById('idProduto').value = '';
    document.getElementById('quantidade').value = '1';
}

// Renderizar itens da venda
function renderizarItensVenda() {
    const div = document.getElementById('itensVenda');
    div.innerHTML = '';

    if (itensVendaTemp.length === 0) {
        div.innerHTML = '<p style="color: #999;">Nenhum item adicionado</p>';
        return;
    }

    itensVendaTemp.forEach((item, index) => {
        const valorTotal = item.quantidade * item.produto.valorUnitario;
        const aliquota = item.produto.aliquotaTipo / 100;
        const valorImposto = valorTotal * aliquota;
        const valorFinal = valorTotal + valorImposto;

        const div2 = document.createElement('div');
        div2.style.cssText = 'background-color: #f9f9f9; padding: 10px; margin-bottom: 10px; border-radius: 4px;';
        div2.innerHTML = `
            <div style="display: flex; justify-content: space-between; align-items: center;">
                <div>
                    <strong>${escapeHtml(item.produto.nome)}</strong><br>
                    Quantidade: ${item.quantidade} x ${formatarMoeda(item.produto.valorUnitario)}<br>
                    Subtotal: ${formatarMoeda(valorTotal)}<br>
                    Imposto (${item.produto.aliquotaTipo}%): ${formatarMoeda(valorImposto)}<br>
                    Total: ${formatarMoeda(valorFinal)}
                </div>
                <button type="button" class="btn-deletar" onclick="removerItemVenda(${index})">Remover</button>
            </div>
        `;
        div.appendChild(div2);
    });
}

// Remover item da venda
function removerItemVenda(index) {
    itensVendaTemp.splice(index, 1);
    renderizarItensVenda();
}

// Salvar venda
async function salvarVenda(e) {
    e.preventDefault();

    if (itensVendaTemp.length === 0) {
        alert('Adicione pelo menos um item à venda');
        return;
    }

    const dados = {
        itens: itensVendaTemp.map(item => ({
            idProduto: item.idProduto,
            quantidade: item.quantidade
        })),
        observacoes: document.getElementById('observacoes').value
    };

    try {
        await ApiVendas.criar(dados);
        fecharModal();
        await carregarVendas();
    } catch (erro) {
        console.error('Erro ao salvar venda:', erro);
    }
}

// Visualizar venda
async function visualizarVenda(id) {
    try {
        const venda = await ApiVendas.obterPorId(id);
        
        let html = `
            <div class="detalhes-venda">
                <h3>Informações da Venda</h3>
                <p><strong>ID:</strong> ${venda.id}</p>
                <p><strong>Data:</strong> ${new Date(venda.dataVenda).toLocaleDateString('pt-BR')}</p>
                <p><strong>Observações:</strong> ${escapeHtml(venda.observacoes || 'Nenhuma')}</p>
            </div>

            <div class="detalhes-venda">
                <h3>Itens da Venda</h3>
        `;

        venda.itens.forEach(item => {
            html += `
                <div class="item-venda">
                    <strong>${escapeHtml(item.nomeProduto)}</strong><br>
                    <div class="item-venda-info">
                        <div>Quantidade: ${item.quantidade}</div>
                        <div>Valor Unitário: ${formatarMoeda(item.valorUnitario)}</div>
                        <div>Subtotal: ${formatarMoeda(item.valorTotalItem)}</div>
                        <div>Alíquota: ${item.aliquotaImposto}%</div>
                        <div>Imposto: ${formatarMoeda(item.valorImposto)}</div>
                        <div><strong>Total: ${formatarMoeda(item.valorFinalItem)}</strong></div>
                    </div>
                </div>
            `;
        });

        html += `
            </div>

            <div class="detalhes-venda">
                <h3>Resumo Financeiro</h3>
                <p><strong>Valor Total Itens:</strong> ${formatarMoeda(venda.valorTotalItens)}</p>
                <p><strong>Valor Total Impostos:</strong> ${formatarMoeda(venda.valorTotalImpostos)}</p>
                <p><strong>Valor Final:</strong> ${formatarMoeda(venda.valorFinal)}</p>
            </div>
        `;

        document.getElementById('detalhesVenda').innerHTML = html;
        document.getElementById('modalVisualizarVenda').style.display = 'block';
    } catch (erro) {
        console.error('Erro ao visualizar venda:', erro);
    }
}

// Filtrar vendas
async function filtrarVendas() {
    const dataInicio = document.getElementById('dataInicio').value;
    const dataFim = document.getElementById('dataFim').value;

    if (!dataInicio || !dataFim) {
        alert('Selecione data de início e fim');
        return;
    }

    try {
        document.getElementById('carregando').style.display = 'block';
        const dados = await ApiVendas.obterTodas(1, dataInicio, dataFim);
        
        if (dados.dados.length === 0) {
            document.getElementById('vazio').style.display = 'block';
            document.getElementById('tabelaVendas').style.display = 'none';
        } else {
            preencherTabela(dados.dados);
            document.getElementById('tabelaVendas').style.display = 'table';
            document.getElementById('vazio').style.display = 'none';
        }

        document.getElementById('carregando').style.display = 'none';
    } catch (erro) {
        console.error('Erro ao filtrar vendas:', erro);
        document.getElementById('carregando').style.display = 'none';
    }
}

// Fechar modal
function fecharModal() {
    document.getElementById('modalVenda').style.display = 'none';
    vendaEmEdicao = null;
    itensVendaTemp = [];
}

// Fechar modal de visualização
function fecharModalVisualizacao() {
    document.getElementById('modalVisualizarVenda').style.display = 'none';
}

// Fechar modal ao clicar fora
window.onclick = function(event) {
    const modal = document.getElementById('modalVenda');
    const modal2 = document.getElementById('modalVisualizarVenda');
    
    if (event.target === modal) {
        fecharModal();
    }
    if (event.target === modal2) {
        fecharModalVisualizacao();
    }
}
