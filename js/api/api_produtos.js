// =====================================================
// API de Produtos
// =====================================================

class ApiProdutos {
    // Obter todos os produtos com paginação
    static async obterTodos(pagina = 1, ativo = null) {
        try {
            let endpoint = `/produtos?pagina=${pagina}`;
            
            if (ativo !== null) {
                endpoint += `&ativo=${ativo}`;
            }

            const resposta = await fazerRequisicao(endpoint);
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao obter produtos:', erro);
            exibirNotificacao('Erro ao carregar produtos', 'erro');
            throw erro;
        }
    }

    // Obter um produto específico
    static async obterPorId(id) {
        try {
            const resposta = await fazerRequisicao(`/produtos/${id}`);
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao obter produto:', erro);
            exibirNotificacao('Erro ao carregar produto', 'erro');
            throw erro;
        }
    }

    // Criar novo produto
    static async criar(dados) {
        try {
            const resposta = await fazerRequisicao('/produtos', {
                method: 'POST',
                body: JSON.stringify(dados)
            });

            exibirNotificacao('Produto criado com sucesso!', 'sucesso');
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao criar produto:', erro);
            exibirNotificacao('Erro ao criar produto', 'erro');
            throw erro;
        }
    }

    // Atualizar produto
    static async atualizar(id, dados) {
        try {
            const resposta = await fazerRequisicao(`/produtos/${id}`, {
                method: 'PUT',
                body: JSON.stringify(dados)
            });

            exibirNotificacao('Produto atualizado com sucesso!', 'sucesso');
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao atualizar produto:', erro);
            exibirNotificacao('Erro ao atualizar produto', 'erro');
            throw erro;
        }
    }

    // Deletar produto
    static async deletar(id) {
        try {
            if (!confirm('Tem certeza que deseja deletar este produto?')) {
                return;
            }

            await fazerRequisicao(`/produtos/${id}`, {
                method: 'DELETE'
            });

            exibirNotificacao('Produto deletado com sucesso!', 'sucesso');
        } catch (erro) {
            console.error('Erro ao deletar produto:', erro);
            exibirNotificacao('Erro ao deletar produto', 'erro');
            throw erro;
        }
    }

    // Buscar produtos por nome
    static async buscar(termo) {
        try {
            const resposta = await fazerRequisicao(`/produtos/buscar/${termo}`);
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao buscar produtos:', erro);
            exibirNotificacao('Erro ao buscar produtos', 'erro');
            throw erro;
        }
    }
}
