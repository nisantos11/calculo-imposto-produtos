# Cálculo Imposto Produtos

Aplicação web simples para cadastro de produtos com cálculo automático de impostos.

## Descrição

Sistema desenvolvido com HTML, CSS e JavaScript puro para demonstrar o cadastro de produtos e cálculo automático de impostos conforme o tipo de produto selecionado.

## Funcionalidades

- **RF01 - Cadastro de Produto**: Formulário para adicionar produtos com campos: Produto, Características, Valor Unitário, Unidade e Tipo
- **RF02 - Listagem de Produtos**: Exibição dinâmica dos produtos cadastrados com cálculos em tempo real
- **RF03 - Limpeza do Formulário**: Reset automático após cada cadastro
- **RF04 - Remoção de Produto**: Botão para remover produtos da lista

## Estrutura de Arquivos

```
projeto-produtos/
├── index.html
├── css/
│   └── style.css
├── js/
│   └── script.js
└── README.md
```

## Tipos de Produto e Alíquotas

| Tipo | Descrição | Alíquota |
|------|-----------|----------|
| 1 | Isento | 0% |
| 2 | Normal | 8% |
| 3 | Intermediário | 10% |
| 4 | Especial | 12% |
| 5 | Premium | 17% |

## Fórmulas de Cálculo

- **Valor Total do Item** = Quantidade × Valor Unitário
- **Valor Imposto** = Valor Total × Alíquota
- **Valor Final** = Valor Total + Valor Imposto

## Como Usar

1. Abra o arquivo `index.html` em um navegador moderno
2. Preencha todos os campos do formulário
3. Clique em "Adicionar Produto"
4. O produto aparecerá na listagem com os cálculos automáticos
5. Altere a quantidade para recalcular automaticamente
6. Clique em "Remover Produto" para deletar um item

## Requisitos

- Navegador moderno (Chrome, Firefox, Safari, Edge)
- Sem dependências externas

## Autor

Nicolly Santos  
SENAI - Centro de Educação e Tecnologia Albano Franco  
Curso: Ensino Médio Integrado em Desenvolvimento de Sistemas para Internet  
Unidade Curricular: Codificação Front-End

## Data

Maio de 2026
