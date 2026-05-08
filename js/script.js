// Tabela de alíquotas de imposto por tipo de produto
const ALIQUOTAS = {
    1: 0,      // Tipo 1: Isento
    2: 0.08,   // Tipo 2: 8%
    3: 0.10,   // Tipo 3: 10%
    4: 0.12,   // Tipo 4: 12%
    5: 0.17    // Tipo 5: 17%
};

// Array para armazenar os produtos
let produtos = [];
let idProduto = 0;

// Elementos do DOM
const form = document.getElementById('produtoForm');
const listaProdutos = document.getElementById('listaProdutos');

// Event listener para o formulário
form.addEventListener('submit', function(e) {
    e.preventDefault();
    adicionarProduto();
});

// Função para adicionar um novo produto
function adicionarProduto() {
    // Capturar dados do formulário
    const produto = document.getElementById('produto').value.trim();
    const caracteristicas = document.getElementById('caracteristicas').value.trim();
    const valorUnitario = parseFloat(document.getElementById('valorUnitario').value);
    const unidade = document.getElementById('unidade').value;
    const tipo = parseInt(document.querySelector('input[name="tipo"]:checked').value);

    // Validar campos vazios
    if (!produto || !caracteristicas || !valorUnitario || !unidade || !tipo) {
        alert('Por favor, preencha todos os campos!');
        return;
    }

    // Validar valor positivo
    if (valorUnitario <= 0) {
        alert('O valor unitário deve ser maior que zero!');
        return;
    }

    // Criar objeto do produto
    const novoProduto = {
        id: idProduto++,
        produto: produto,
        caracteristicas: caracteristicas,
        valorUnitario: valorUnitario,
        unidade: unidade,
        tipo: tipo,
        quantidade: 1
    };

    // Adicionar ao array
    produtos.push(novoProduto);

    // Limpar formulário
    form.reset();

    // Renderizar lista
    renderizarProdutos();
}

// Função para renderizar a lista de produtos
function renderizarProdutos() {
    // Limpar lista
    listaProdutos.innerHTML = '';

    // Se não há produtos, mostrar mensagem
    if (produtos.length === 0) {
        listaProdutos.innerHTML = '<p class="vazio">Nenhum produto cadastrado ainda</p>';
        return;
    }

    // Renderizar cada produto
    produtos.forEach(produto => {
        const item = criarItemProduto(produto);
        listaProdutos.appendChild(item);
    });
}

// Função para criar um item de produto
function criarItemProduto(produto) {
    const div = document.createElement('div');
    div.className = 'produto-item';
    
    // Adicionar classe especial para produtos isentos
    if (produto.tipo === 1) {
        div.classList.add('isento');
    }

    // Calcular valores
    const valorTotal = produto.quantidade * produto.valorUnitario;
    const aliquota = ALIQUOTAS[produto.tipo];
    const valorImposto = valorTotal * aliquota;
    const valorFinal = valorTotal + valorImposto;

    // Criar HTML do item
    div.innerHTML = `
        <div class="produto-nome">${escapeHtml(produto.produto)}</div>
        <span class="produto-tipo ${produto.tipo === 1 ? 'isento' : ''}">
            Tipo ${produto.tipo} - ${getDescricaoTipo(produto.tipo)} (${(aliquota * 100).toFixed(0)}%)
        </span>
        <div class="produto-caracteristicas">${escapeHtml(produto.caracteristicas)}</div>
        
        <div class="produto-info">
            <div class="info-item">
                <span class="info-label">Valor Unitário:</span>
                <span class="info-valor">R$ ${produto.valorUnitario.toFixed(2).replace('.', ',')}</span>
            </div>
            <div class="info-item">
                <span class="info-label">Unidade:</span>
                <span class="info-valor">${escapeHtml(produto.unidade)}</span>
            </div>
        </div>

        <div class="quantidade-container">
            <label for="qtd-${produto.id}">Quantidade:</label>
            <input type="number" id="qtd-${produto.id}" value="${produto.quantidade}" min="1" 
                   onchange="atualizarQuantidade(${produto.id}, this.value)">
        </div>

        <div class="calculos">
            <div class="calculo-linha">
                <span>Valor Total do Item:</span>
                <span>R$ ${valorTotal.toFixed(2).replace('.', ',')}</span>
            </div>
            <div class="calculo-linha">
                <span>Valor Imposto:</span>
                <span>R$ ${valorImposto.toFixed(2).replace('.', ',')}</span>
            </div>
            <div class="calculo-linha">
                <span>Valor Final:</span>
                <span>R$ ${valorFinal.toFixed(2).replace('.', ',')}</span>
            </div>
        </div>

        <button class="btn-remover" onclick="removerProduto(${produto.id})">Remover Produto</button>
    `;

    return div;
}

// Função para atualizar a quantidade de um produto
function atualizarQuantidade(id, novaQuantidade) {
    const quantidade = parseInt(novaQuantidade);
    
    if (quantidade < 1) {
        alert('A quantidade deve ser no mínimo 1!');
        renderizarProdutos();
        return;
    }

    const produto = produtos.find(p => p.id === id);
    if (produto) {
        produto.quantidade = quantidade;
        renderizarProdutos();
    }
}

// Função para remover um produto
function removerProduto(id) {
    if (confirm('Tem certeza que deseja remover este produto?')) {
        // Encontrar o índice do produto
        const index = produtos.findIndex(p => p.id === id);
        
        if (index > -1) {
            // Animar remoção
            const items = document.querySelectorAll('.produto-item');
            items.forEach((item, idx) => {
                const produtoAtual = produtos[idx];
                if (produtoAtual && produtoAtual.id === id) {
                    item.classList.add('removendo');
                }
            });

            // Remover após animação
            setTimeout(() => {
                produtos.splice(index, 1);
                renderizarProdutos();
            }, 300);
        }
    }
}

// Função para escapar HTML (prevenir XSS)
function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

// Função para obter descrição do tipo
function getDescricaoTipo(tipo) {
    const descricoes = {
        1: 'Isento',
        2: 'Normal',
        3: 'Intermediário',
        4: 'Especial',
        5: 'Premium'
    };
    return descricoes[tipo] || 'Desconhecido';
}

// Inicializar a aplicação
document.addEventListener('DOMContentLoaded', function() {
    renderizarProdutos();
});
