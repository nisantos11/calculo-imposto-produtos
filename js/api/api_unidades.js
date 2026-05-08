// =====================================================
// API de Unidades
// =====================================================

class ApiUnidades {
    // Obter todas as unidades
    static async obterTodas() {
        try {
            const resposta = await fazerRequisicao('/unidades');
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao obter unidades:', erro);
            exibirNotificacao('Erro ao carregar unidades', 'erro');
            throw erro;
        }
    }

    // Obter uma unidade específica
    static async obterPorId(id) {
        try {
            const resposta = await fazerRequisicao(`/unidades/${id}`);
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao obter unidade:', erro);
            exibirNotificacao('Erro ao carregar unidade', 'erro');
            throw erro;
        }
    }

    // Criar nova unidade
    static async criar(dados) {
        try {
            const resposta = await fazerRequisicao('/unidades', {
                method: 'POST',
                body: JSON.stringify(dados)
            });

            exibirNotificacao('Unidade criada com sucesso!', 'sucesso');
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao criar unidade:', erro);
            exibirNotificacao('Erro ao criar unidade', 'erro');
            throw erro;
        }
    }

    // Atualizar unidade
    static async atualizar(id, dados) {
        try {
            const resposta = await fazerRequisicao(`/unidades/${id}`, {
                method: 'PUT',
                body: JSON.stringify(dados)
            });

            exibirNotificacao('Unidade atualizada com sucesso!', 'sucesso');
            return resposta.dados;
        } catch (erro) {
            console.error('Erro ao atualizar unidade:', erro);
            exibirNotificacao('Erro ao atualizar unidade', 'erro');
            throw erro;
        }
    }

    // Deletar unidade
    static async deletar(id) {
        try {
            if (!confirm('Tem certeza que deseja deletar esta unidade?')) {
                return;
            }

            await fazerRequisicao(`/unidades/${id}`, {
                method: 'DELETE'
            });

            exibirNotificacao('Unidade deletada com sucesso!', 'sucesso');
        } catch (erro) {
            console.error('Erro ao deletar unidade:', erro);
            exibirNotificacao('Erro ao deletar unidade', 'erro');
            throw erro;
        }
    }
}
