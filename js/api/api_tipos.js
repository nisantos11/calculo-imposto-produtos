// =====================================================
// API de Tipos de Produto
// =====================================================

class ApiTipos {
    // Obter todos os tipos
    static async obterTodos() {
        try {
            const resposta = await fazerRequisicao('/tipos-produto');
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao obter tipos:', erro);
            exibirNotificacao('Erro ao carregar tipos de produto', 'erro');
            throw erro;
        }
    }

    // Obter um tipo específico
    static async obterPorId(id) {
        try {
            const resposta = await fazerRequisicao(`/tipos-produto/${id}`);
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao obter tipo:', erro);
            exibirNotificacao('Erro ao carregar tipo de produto', 'erro');
            throw erro;
        }
    }

    // Criar novo tipo
    static async criar(dados) {
        try {
            const resposta = await fazerRequisicao('/tipos-produto', {
                method: 'POST',
                body: JSON.stringify(dados)
            });

            exibirNotificacao('Tipo de produto criado com sucesso!', 'sucesso');
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao criar tipo:', erro);
            exibirNotificacao('Erro ao criar tipo de produto', 'erro');
            throw erro;
        }
    }

    // Atualizar tipo
    static async atualizar(id, dados) {
        try {
            const resposta = await fazerRequisicao(`/tipos-produto/${id}`, {
                method: 'PUT',
                body: JSON.stringify(dados)
            });

            exibirNotificacao('Tipo de produto atualizado com sucesso!', 'sucesso');
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao atualizar tipo:', erro);
            exibirNotificacao('Erro ao atualizar tipo de produto', 'erro');
            throw erro;
        }
    }

    // Deletar tipo
    static async deletar(id) {
        try {
            if (!confirm('Tem certeza que deseja deletar este tipo de produto?')) {
                return;
            }

            await fazerRequisicao(`/tipos-produto/${id}`, {
                method: 'DELETE'
            });

            exibirNotificacao('Tipo de produto deletado com sucesso!', 'sucesso');
        } catch (erro) {
            console.error('Erro ao deletar tipo:', erro);
            exibirNotificacao('Erro ao deletar tipo de produto', 'erro');
            throw erro;
        }
    }
}
