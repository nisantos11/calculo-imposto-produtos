// =====================================================
// Configuração da API
// =====================================================

// URL base da API (ajuste conforme necessário)
const API_BASE_URL = 'https://localhost:7000/api'; // Altere para a URL do seu servidor

// Função para fazer requisições à API
async function fazerRequisicao(endpoint, opcoes = {}) {
    const url = `${API_BASE_URL}${endpoint}`;
    
    const configPadrao = {
        headers: {
            'Content-Type': 'application/json'
        }
    };

    const config = { ...configPadrao, ...opcoes };

    try {
        const resposta = await fetch(url, config);

        // Se a resposta não for ok, lançar erro
        if (!resposta.ok) {
            const erro = await resposta.json();
            throw new Error(erro.mensagem || `Erro ${resposta.status}: ${resposta.statusText}`);
        }

        // Retornar os dados da resposta
        return await resposta.json();
    } catch (erro) {
        console.error('Erro na requisição:', erro);
        throw erro;
    }
}

// Função para exibir notificação
function exibirNotificacao(mensagem, tipo = 'sucesso') {
    const notificacao = document.createElement('div');
    notificacao.className = `notificacao notificacao-${tipo}`;
    notificacao.textContent = mensagem;
    
    document.body.appendChild(notificacao);
    
    // Remover após 3 segundos
    setTimeout(() => {
        notificacao.remove();
    }, 3000);
}

// Função para formatar moeda
function formatarMoeda(valor) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(valor);
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
