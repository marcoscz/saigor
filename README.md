# Saigor

Sistema de Fluxo de Processo

## Descrição

Aplicação web para gerenciamento de jobs, processos, tarefas e logs, utilizando ASP.NET, Entity Framework, Quartz e MudBlazor.

## Funcionalidades

- Cadastro e agendamento de jobs (tarefas)
- Gerenciamento de workers (processos)
- Visualização e exclusão de logs de execução
- Interface moderna com MudBlazor
- Agendamento e execução automática de tarefas com Quartz

## Requisitos

- .NET 9.0 SDK
- SQLite (ou outro banco, se configurado)
- Node.js (opcional, para desenvolvimento frontend)

## Como rodar o projeto

```bash
# Restaurar dependências
dotnet restore

# Aplicar migrations (opcional, se necessário)
dotnet ef database update

# Rodar a aplicação
dotnet run
```

Acesse em: http://localhost:5000

## Configuração

- As strings de conexão estão em `appsettings.json`.
- Segredos de desenvolvimento podem ser configurados via User Secrets.

## Estrutura do Projeto

- `Models/` - Modelos de dados
- `Data/` - Contexto e serviços de dados
- `Pages/` - Páginas Razor (UI)
- `Services/` - Serviços de negócio e agendamento
- `Shared/` - Componentes e layouts compartilhados

## Licença

[MIT](LICENSE) (ou outra, se aplicável)
