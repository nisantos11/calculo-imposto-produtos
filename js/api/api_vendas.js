// =====================================================
// API de Vendas
// =====================================================

class ApiVendas {
    // Obter todas as vendas com paginação
    static async obterTodas(pagina = 1, dataInicio = null, dataFim = null) {
        try {
            let endpoint = `/vendas?pagina=${pagina}`;
            
            if (dataInicio) {
                endpoint += `&dataInicio=${dataInicio}`;
            }
            
            if (dataFim) {
                endpoint += `&dataFim=${dataFim}`;
            }

            const resposta = await fazerRequisicao(endpoint);
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao obter vendas:', erro);
            exibirNotificacao('Erro ao carregar vendas', 'erro');
            throw erro;
        }
    }

    // Obter uma venda específica
    static async obterPorId(id) {
        try {
            const resposta = await fazerRequisicao(`/vendas/${id}`);
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao obter venda:', erro);
            exibirNotificacao('Erro ao carregar venda', 'erro');
            throw erro;
        }
    }

    // Criar nova venda
    static async criar(dados) {
        try {
            const resposta = await fazerRequisicao('/vendas', {
                method: 'POST',
                body: JSON.stringify(dados)
            });

            exibirNotificacao('Venda criada com sucesso!', 'sucesso');
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao criar venda:', erro);
            exibirNotificacao('Erro ao criar venda', 'erro');
            throw erro;
        }
    }

    // Obter relatório por período
    static async obterRelatorioPeriodo(dataInicio, dataFim) {
        try {
            const resposta = await fazerRequisicao(
                `/vendas/relatorio/periodo?dataInicio=${dataInicio}&dataFim=${dataFim}`
            );
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao obter relatório:', erro);
            exibirNotificacao('Erro ao carregar relatório', 'erro');
            throw erro;
        }
    }

    // Obter relatório por tipo de produto
    static async obterRelatorioPorTipo() {
        try {
            const resposta = await fazerRequisicao('/vendas/relatorio/por-tipo');
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao obter relatório por tipo:', erro);
            exibirNotificacao('Erro ao carregar relatório', 'erro');
            throw erro;
        }
    }
}
