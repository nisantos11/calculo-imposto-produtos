// =====================================================
// Script para página de Relatórios
// =====================================================

let dadosPeriodo = [];
let dadosTipo = [];

// Inicializar página
document.addEventListener('DOMContentLoaded', function() {
    // Definir datas padrão (últimos 30 dias)
    const dataFim = new Date();
    const dataInicio = new Date(dataFim);
    dataInicio.setDate(dataInicio.getDate() - 30);

    document.getElementById('dataInicio').valueAsDate = dataInicio;
    document.getElementById('dataFim').valueAsDate = dataFim;
});

// Carregar relatório por período
async function carregarRelatorioPeriodo() {
    try {
        const dataInicio = document.getElementById('dataInicio').value;
        const dataFim = document.getElementById('dataFim').value;

        if (!dataInicio || !dataFim) {
            alert('Selecione data de início e fim');
            return;
        }

        document.getElementById('carregandoPeriodo').style.display = 'block';
        document.getElementById('tabelaPeriodo').style.display = 'none';
        document.getElementById('resumoPeriodo').style.display = 'none';
        document.getElementById('vazioPeríodo').style.display = 'none';

        dadosPeriodo = await ApiVendas.obterRelatorioPeriodo(dataInicio, dataFim);

        if (dadosPeriodo.length === 0) {
            document.getElementById('vazioPeríodo').style.display = 'block';
        } else {
            preencherTabelaPeriodo(dadosPeriodo);
            preencherResumoPeriodo(dadosPeriodo);
            document.getElementById('tabelaPeriodo').style.display = 'table';
            document.getElementById('resumoPeriodo').style.display = 'grid';
        }

        document.getElementById('carregandoPeriodo').style.display = 'none';
    } catch (erro) {
        console.error('Erro ao carregar relatório:', erro);
        document.getElementById('carregandoPeriodo').style.display = 'none';
    }
}

// Preencher tabela de período
function preencherTabelaPeriodo(dados) {
    const tbody = document.querySelector('#tabelaPeriodo tbody');
    tbody.innerHTML = '';

    dados.forEach(linha => {
        const tr = document.createElement('tr');
        const data = new Date(linha.data).toLocaleDateString('pt-BR');
        
        tr.innerHTML = `
            <td>${data}</td>
            <td>${linha.totalVendas}</td>
            <td>${formatarMoeda(linha.valorItens)}</td>
            <td>${formatarMoeda(linha.valorImpostos)}</td>
            <td>${formatarMoeda(linha.valorFinal)}</td>
            <td>${formatarMoeda(linha.ticketMedio)}</td>
        `;
        tbody.appendChild(tr);
    });
}

// Preencher resumo de período
function preencherResumoPeriodo(dados) {
    const totalVendas = dados.reduce((acc, d) => acc + d.totalVendas, 0);
    const totalItens = dados.reduce((acc, d) => acc + d.valorItens, 0);
    const totalImpostos = dados.reduce((acc, d) => acc + d.valorImpostos, 0);
    const totalFinal = dados.reduce((acc, d) => acc + d.valorFinal, 0);
    const ticketMedio = totalFinal / totalVendas;

    const div = document.getElementById('resumoPeriodo');
    div.innerHTML = `
        <div class="card-valor">
            <h3>Total de Vendas</h3>
            <div class="valor">${totalVendas}</div>
        </div>
        <div class="card-valor itens">
            <h3>Valor Total Itens</h3>
            <div class="valor">${formatarMoeda(totalItens)}</div>
        </div>
        <div class="card-valor impostos">
            <h3>Valor Total Impostos</h3>
            <div class="valor">${formatarMoeda(totalImpostos)}</div>
        </div>
        <div class="card-valor final">
            <h3>Valor Final Total</h3>
            <div class="valor">${formatarMoeda(totalFinal)}</div>
        </div>
        <div class="card-valor">
            <h3>Ticket Médio</h3>
            <div class="valor">${formatarMoeda(ticketMedio)}</div>
        </div>
    `;
}

// Carregar relatório por tipo
async function carregarRelatorioTipo() {
    try {
        document.getElementById('carregandoTipo').style.display = 'block';
        document.getElementById('tabelaTipo').style.display = 'none';
        document.getElementById('vazioTipo').style.display = 'none';

        dadosTipo = await ApiVendas.obterRelatorioPorTipo();

        if (!dadosTipo || dadosTipo.length === 0) {
            document.getElementById('vazioTipo').style.display = 'block';
        } else {
            preencherTabelaTipo(dadosTipo);
            document.getElementById('tabelaTipo').style.display = 'table';
        }

        document.getElementById('carregandoTipo').style.display = 'none';
    } catch (erro) {
        console.error('Erro ao carregar relatório por tipo:', erro);
        document.getElementById('carregandoTipo').style.display = 'none';
    }
}

// Preencher tabela de tipo
function preencherTabelaTipo(dados) {
    const tbody = document.querySelector('#tabelaTipo tbody');
    tbody.innerHTML = '';

    dados.forEach(linha => {
        const tr = document.createElement('tr');
        
        tr.innerHTML = `
            <td>${escapeHtml(linha.tipoProduto)}</td>
            <td>${linha.totalItens}</td>
            <td>${linha.quantidadeTotal}</td>
            <td>${formatarMoeda(linha.valorTotalItens)}</td>
            <td>${formatarMoeda(linha.valorTotalImpostos)}</td>
            <td>${formatarMoeda(linha.valorFinalTotal)}</td>
            <td>${formatarMoeda(linha.valorFinalTotal / linha.totalItens)}</td>
        `;
        tbody.appendChild(tr);
    });
}

// Exportar período para CSV
function exportarPeriodoCSV() {
    if (dadosPeriodo.length === 0) {
        alert('Nenhum dado para exportar');
        return;
    }

    let csv = 'Data,Total de Vendas,Valor Itens,Valor Impostos,Valor Final,Ticket Médio\n';
    
    dadosPeriodo.forEach(linha => {
        const data = new Date(linha.data).toLocaleDateString('pt-BR');
        csv += `${data},${linha.totalVendas},${linha.valorItens.toFixed(2)},${linha.valorImpostos.toFixed(2)},${linha.valorFinal.toFixed(2)},${linha.ticketMedio.toFixed(2)}\n`;
    });

    baixarCSV(csv, 'relatorio_periodo.csv');
}

// Exportar tipo para CSV
function exportarTipoCSV() {
    if (dadosTipo.length === 0) {
        alert('Nenhum dado para exportar');
        return;
    }

    let csv = 'Tipo de Produto,Total de Itens,Quantidade Total,Valor Itens,Valor Impostos,Valor Final,Valor Médio/Item\n';
    
    dadosTipo.forEach(linha => {
        csv += `"${linha.tipoProduto}",${linha.totalItens},${linha.quantidadeTotal},${linha.valorTotalItens.toFixed(2)},${linha.valorTotalImpostos.toFixed(2)},${linha.valorFinalTotal.toFixed(2)},${(linha.valorFinalTotal / linha.totalItens).toFixed(2)}\n`;
    });

    baixarCSV(csv, 'relatorio_tipo.csv');
}

// Função auxiliar para baixar CSV
function baixarCSV(csv, nomeArquivo) {
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    
    link.setAttribute('href', url);
    link.setAttribute('download', nomeArquivo);
    link.style.visibility = 'hidden';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
