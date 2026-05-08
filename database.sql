-- =====================================================
-- Script SQL - Banco de Dados: Cálculo Imposto Produtos
-- Sistema de Cadastro de Produtos com Cálculo de Impostos
-- Data: Maio de 2026
-- =====================================================

-- Criar banco de dados
CREATE DATABASE IF NOT EXISTS calculo_imposto_produtos;
USE calculo_imposto_produtos;

-- =====================================================
-- Tabela: tipos_produto
-- Descrição: Armazena os tipos de produtos com suas alíquotas
-- =====================================================
CREATE TABLE tipos_produto (
    id INT PRIMARY KEY AUTO_INCREMENT,
    descricao VARCHAR(50) NOT NULL UNIQUE,
    aliquota DECIMAL(5, 2) NOT NULL,
    ativo BOOLEAN DEFAULT TRUE,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_aliquota CHECK (aliquota >= 0 AND aliquota <= 100)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabela: unidades
-- Descrição: Armazena as unidades de medida disponíveis
-- =====================================================
CREATE TABLE unidades (
    id INT PRIMARY KEY AUTO_INCREMENT,
    sigla VARCHAR(10) NOT NULL UNIQUE,
    descricao VARCHAR(100) NOT NULL,
    ativo BOOLEAN DEFAULT TRUE,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabela: produtos
-- Descrição: Armazena todos os produtos cadastrados
-- =====================================================
CREATE TABLE produtos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(150) NOT NULL,
    caracteristicas TEXT NOT NULL,
    valor_unitario DECIMAL(10, 2) NOT NULL,
    id_unidade INT NOT NULL,
    id_tipo INT NOT NULL,
    ativo BOOLEAN DEFAULT TRUE,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_produtos_unidade FOREIGN KEY (id_unidade) REFERENCES unidades(id),
    CONSTRAINT fk_produtos_tipo FOREIGN KEY (id_tipo) REFERENCES tipos_produto(id),
    CONSTRAINT chk_valor_unitario CHECK (valor_unitario > 0),
    INDEX idx_nome (nome),
    INDEX idx_tipo (id_tipo),
    INDEX idx_unidade (id_unidade)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabela: vendas
-- Descrição: Armazena o histórico de vendas/transações
-- =====================================================
CREATE TABLE vendas (
    id INT PRIMARY KEY AUTO_INCREMENT,
    data_venda TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    valor_total_itens DECIMAL(12, 2) NOT NULL,
    valor_total_impostos DECIMAL(12, 2) NOT NULL,
    valor_final DECIMAL(12, 2) NOT NULL,
    observacoes TEXT,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_valores_venda CHECK (
        valor_total_itens >= 0 AND 
        valor_total_impostos >= 0 AND 
        valor_final >= 0
    ),
    INDEX idx_data_venda (data_venda)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabela: itens_venda
-- Descrição: Armazena os itens de cada venda
-- =====================================================
CREATE TABLE itens_venda (
    id INT PRIMARY KEY AUTO_INCREMENT,
    id_venda INT NOT NULL,
    id_produto INT NOT NULL,
    quantidade INT NOT NULL,
    valor_unitario DECIMAL(10, 2) NOT NULL,
    valor_total_item DECIMAL(12, 2) NOT NULL,
    aliquota_imposto DECIMAL(5, 2) NOT NULL,
    valor_imposto DECIMAL(12, 2) NOT NULL,
    valor_final_item DECIMAL(12, 2) NOT NULL,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_itens_venda FOREIGN KEY (id_venda) REFERENCES vendas(id) ON DELETE CASCADE,
    CONSTRAINT fk_itens_produto FOREIGN KEY (id_produto) REFERENCES produtos(id),
    CONSTRAINT chk_quantidade CHECK (quantidade > 0),
    CONSTRAINT chk_valores_item CHECK (
        valor_unitario > 0 AND 
        valor_total_item >= 0 AND 
        valor_imposto >= 0 AND 
        valor_final_item >= 0
    ),
    INDEX idx_venda (id_venda),
    INDEX idx_produto (id_produto)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabela: auditoria
-- Descrição: Registra todas as operações para auditoria
-- =====================================================
CREATE TABLE auditoria (
    id INT PRIMARY KEY AUTO_INCREMENT,
    tabela VARCHAR(50) NOT NULL,
    operacao VARCHAR(20) NOT NULL,
    id_registro INT,
    dados_anteriores JSON,
    dados_novos JSON,
    data_operacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ip_usuario VARCHAR(45),
    INDEX idx_tabela (tabela),
    INDEX idx_operacao (operacao),
    INDEX idx_data (data_operacao)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- INSERIR DADOS INICIAIS
-- =====================================================

-- Inserir tipos de produtos
INSERT INTO tipos_produto (descricao, aliquota) VALUES
('Isento', 0.00),
('Normal', 8.00),
('Intermediário', 10.00),
('Especial', 12.00),
('Premium', 17.00);

-- Inserir unidades de medida
INSERT INTO unidades (sigla, descricao) VALUES
('UN', 'Unidade'),
('KG', 'Quilograma'),
('L', 'Litro'),
('M', 'Metro'),
('CX', 'Caixa');

-- =====================================================
-- CRIAR VIEWS ÚTEIS
-- =====================================================

-- View: Produtos com informações completas
CREATE VIEW vw_produtos_completo AS
SELECT 
    p.id,
    p.nome,
    p.caracteristicas,
    p.valor_unitario,
    u.sigla AS unidade,
    u.descricao AS descricao_unidade,
    tp.descricao AS tipo_produto,
    tp.aliquota,
    p.ativo,
    p.data_criacao,
    p.data_atualizacao
FROM produtos p
INNER JOIN unidades u ON p.id_unidade = u.id
INNER JOIN tipos_produto tp ON p.id_tipo = tp.id;

-- View: Resumo de vendas por tipo de produto
CREATE VIEW vw_vendas_por_tipo AS
SELECT 
    tp.descricao AS tipo_produto,
    COUNT(DISTINCT iv.id_venda) AS total_vendas,
    SUM(iv.quantidade) AS quantidade_total,
    SUM(iv.valor_total_item) AS valor_total_itens,
    SUM(iv.valor_imposto) AS valor_total_impostos,
    SUM(iv.valor_final_item) AS valor_final_total,
    AVG(iv.valor_final_item) AS valor_medio_por_item
FROM itens_venda iv
INNER JOIN produtos p ON iv.id_produto = p.id
INNER JOIN tipos_produto tp ON p.id_tipo = tp.id
GROUP BY tp.id, tp.descricao;

-- View: Detalhes completos de cada venda
CREATE VIEW vw_vendas_detalhes AS
SELECT 
    v.id AS id_venda,
    v.data_venda,
    COUNT(iv.id) AS total_itens,
    SUM(iv.quantidade) AS quantidade_total,
    v.valor_total_itens,
    v.valor_total_impostos,
    v.valor_final,
    ROUND((v.valor_total_impostos / v.valor_total_itens * 100), 2) AS percentual_imposto
FROM vendas v
LEFT JOIN itens_venda iv ON v.id = iv.id_venda
GROUP BY v.id, v.data_venda, v.valor_total_itens, v.valor_total_impostos, v.valor_final;

-- =====================================================
-- CRIAR ÍNDICES ADICIONAIS PARA PERFORMANCE
-- =====================================================

CREATE INDEX idx_produtos_ativo ON produtos(ativo);
CREATE INDEX idx_tipos_ativo ON tipos_produto(ativo);
CREATE INDEX idx_unidades_ativo ON unidades(ativo);
CREATE INDEX idx_vendas_valor_final ON vendas(valor_final);

-- =====================================================
-- CRIAR TRIGGERS PARA AUDITORIA
-- =====================================================

-- Trigger: Auditoria de inserção em produtos
DELIMITER $$
CREATE TRIGGER trg_audit_produtos_insert
AFTER INSERT ON produtos
FOR EACH ROW
BEGIN
    INSERT INTO auditoria (tabela, operacao, id_registro, dados_novos)
    VALUES ('produtos', 'INSERT', NEW.id, JSON_OBJECT(
        'nome', NEW.nome,
        'valor_unitario', NEW.valor_unitario,
        'id_tipo', NEW.id_tipo,
        'id_unidade', NEW.id_unidade
    ));
END$$
DELIMITER ;

-- Trigger: Auditoria de atualização em produtos
DELIMITER $$
CREATE TRIGGER trg_audit_produtos_update
AFTER UPDATE ON produtos
FOR EACH ROW
BEGIN
    INSERT INTO auditoria (tabela, operacao, id_registro, dados_anteriores, dados_novos)
    VALUES ('produtos', 'UPDATE', NEW.id, 
        JSON_OBJECT(
            'nome', OLD.nome,
            'valor_unitario', OLD.valor_unitario,
            'id_tipo', OLD.id_tipo
        ),
        JSON_OBJECT(
            'nome', NEW.nome,
            'valor_unitario', NEW.valor_unitario,
            'id_tipo', NEW.id_tipo
        )
    );
END$$
DELIMITER ;

-- Trigger: Auditoria de exclusão em produtos
DELIMITER $$
CREATE TRIGGER trg_audit_produtos_delete
AFTER DELETE ON produtos
FOR EACH ROW
BEGIN
    INSERT INTO auditoria (tabela, operacao, id_registro, dados_anteriores)
    VALUES ('produtos', 'DELETE', OLD.id, JSON_OBJECT(
        'nome', OLD.nome,
        'valor_unitario', OLD.valor_unitario,
        'id_tipo', OLD.id_tipo
    ));
END$$
DELIMITER ;

-- =====================================================
-- CRIAR PROCEDURES ÚTEIS
-- =====================================================

-- Procedure: Inserir nova venda com seus itens
DELIMITER $$
CREATE PROCEDURE sp_inserir_venda(
    IN p_valor_total_itens DECIMAL(12, 2),
    IN p_valor_total_impostos DECIMAL(12, 2),
    IN p_valor_final DECIMAL(12, 2),
    IN p_observacoes TEXT,
    OUT p_id_venda INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_id_venda = -1;
    END;
    
    START TRANSACTION;
    
    INSERT INTO vendas (valor_total_itens, valor_total_impostos, valor_final, observacoes)
    VALUES (p_valor_total_itens, p_valor_total_impostos, p_valor_final, p_observacoes);
    
    SET p_id_venda = LAST_INSERT_ID();
    
    COMMIT;
END$$
DELIMITER ;

-- Procedure: Obter relatório de vendas por período
DELIMITER $$
CREATE PROCEDURE sp_relatorio_vendas(
    IN p_data_inicio DATE,
    IN p_data_fim DATE
)
BEGIN
    SELECT 
        DATE(v.data_venda) AS data,
        COUNT(v.id) AS total_vendas,
        SUM(v.valor_total_itens) AS valor_itens,
        SUM(v.valor_total_impostos) AS valor_impostos,
        SUM(v.valor_final) AS valor_final,
        ROUND(AVG(v.valor_final), 2) AS ticket_medio
    FROM vendas v
    WHERE DATE(v.data_venda) BETWEEN p_data_inicio AND p_data_fim
    GROUP BY DATE(v.data_venda)
    ORDER BY DATE(v.data_venda) DESC;
END$$
DELIMITER ;

-- =====================================================
-- COMENTÁRIOS E DOCUMENTAÇÃO
-- =====================================================

/*
ESTRUTURA DO BANCO DE DADOS:

1. tipos_produto
   - Armazena os 5 tipos de produtos com suas alíquotas
   - Tipos: Isento (0%), Normal (8%), Intermediário (10%), Especial (12%), Premium (17%)

2. unidades
   - Armazena as unidades de medida: UN, KG, L, M, CX

3. produtos
   - Armazena todos os produtos cadastrados
   - Contém: nome, características, valor unitário, unidade, tipo
   - Relacionado com tipos_produto e unidades

4. vendas
   - Armazena o histórico de transações
   - Contém: data, valor total itens, valor total impostos, valor final

5. itens_venda
   - Armazena os itens de cada venda
   - Contém: quantidade, valores, alíquota aplicada
   - Relacionado com vendas e produtos

6. auditoria
   - Registra todas as operações para rastreabilidade
   - Armazena dados anteriores e novos de cada operação

SEGURANÇA:
- Constraints de verificação (CHECK) para garantir integridade
- Foreign keys para manter relacionamentos
- Triggers para auditoria automática
- Validação de valores positivos

PERFORMANCE:
- Índices em campos frequentemente consultados
- Views para queries complexas
- Procedures para operações comuns

EXEMPLO DE USO:
- Inserir produto: INSERT INTO produtos (nome, caracteristicas, valor_unitario, id_unidade, id_tipo) VALUES (...)
- Consultar produtos: SELECT * FROM vw_produtos_completo;
- Inserir venda: CALL sp_inserir_venda(...);
- Relatório: CALL sp_relatorio_vendas('2026-05-01', '2026-05-31');
*/

-- =====================================================
-- FIM DO SCRIPT SQL
-- =====================================================
