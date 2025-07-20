# Saigor

Sistema de Gerenciamento de Jobs e Processos

## 📋 Descrição

Aplicação web moderna para gerenciamento de jobs, processos, tarefas e logs, desenvolvida com ASP.NET Core, Entity Framework Core, Quartz.NET e MudBlazor.

## ✨ Funcionalidades

- **Gerenciamento de Jobs**: Cadastro, edição e agendamento de jobs com expressões CRON
- **Sistema de Tarefas**: Criação e organização de tarefas reutilizáveis
- **Conexões**: Gerenciamento de conexões de banco de dados e sistemas
- **Agendamento Inteligente**: Execução automática de jobs com Quartz.NET
- **Logs Detalhados**: Rastreamento completo de execuções com logs estruturados
- **Dashboard Interativo**: Visualização de estatísticas e métricas em tempo real
- **Interface Moderna**: UI responsiva e intuitiva com MudBlazor
- **Validação Robusta**: Sistema de validação com FluentValidation
- **Cache Inteligente**: Otimização de performance com cache em memória

## 🛠️ Tecnologias

- **Backend**: ASP.NET Core 9.0, Entity Framework Core, Quartz.NET
- **Frontend**: Blazor Server, MudBlazor
- **Banco de Dados**: SQLite (configurável para outros SGBDs)
- **Validação**: FluentValidation
- **Logging**: Microsoft.Extensions.Logging
- **Cache**: Microsoft.Extensions.Caching.Memory

## 📋 Requisitos

- .NET 9.0 SDK ou superior
- SQLite (ou outro banco configurado)
- Navegador moderno com suporte a WebAssembly

## 🚀 Como Executar

### 1. Clone o repositório
```bash
git clone <url-do-repositorio>
cd saigor
```

### 2. Restaure as dependências
```bash
dotnet restore
```

### 3. Configure o banco de dados
```bash
# Aplicar migrations (cria as tabelas automaticamente)
dotnet ef database update
```

### 4. Execute a aplicação
```bash
dotnet run
```

### 5. Acesse a aplicação
Abra o navegador e acesse: **http://localhost:5000**

## 📁 Estrutura do Projeto

```
Saigor/
├── Configuration/     # Configurações da aplicação
├── Data/             # Contexto EF Core e migrations
├── Domain/           # Interfaces e validadores
├── Jobs/             # Jobs personalizados
├── Middleware/       # Middlewares customizados
├── Models/           # Modelos de dados (JobModel, LogModel, etc.)
├── Pages/            # Páginas Razor (UI)
├── Repositories/     # Camada de acesso a dados
├── Services/         # Serviços de negócio
├── Shared/           # Componentes compartilhados
└── Utils/            # Utilitários e helpers
```

## 🔧 Configuração

### Arquivos de Configuração
- `appsettings.json` - Configurações gerais
- `appsettings.Development.json` - Configurações de desenvolvimento
- `appsettings.Production.json` - Configurações de produção

### Variáveis de Ambiente
```bash
# String de conexão do banco
ConnectionStrings__DefaultConnection="Data Source=Saigor.db"

# Configurações de logging
Logging__LogLevel__Default="Information"
```

### User Secrets (Desenvolvimento)
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=Saigor.db"
```

## 📊 Funcionalidades Principais

### Jobs
- Criação e edição de jobs com comandos personalizados
- Agendamento com expressões CRON
- Controle de status (Pendente, Rodando, Completado, Falhou)
- Ordenação por prioridade
- Execução manual e automática

### Tarefas
- Cadastro de tarefas reutilizáveis
- Associação de tarefas a jobs
- Controle de status e ativação/desativação

### Conexões
- Gerenciamento de conexões de banco
- Suporte a diferentes ambientes
- Configuração de conectores

### Logs
- Rastreamento detalhado de execuções
- Filtros por data, job e status
- Visualização de saídas de comando
- Limpeza automática de logs antigos

## 🔒 Segurança

- Headers de segurança configurados
- Validação de entrada com FluentValidation
- Sanitização de dados
- Controle de acesso (preparado para implementação)

## 📈 Performance

- Cache em memória para consultas frequentes
- Otimização de queries com Entity Framework
- Lazy loading configurado
- Paginação em listagens grandes

## 🧪 Testes

Para executar os testes:
```bash
dotnet test
```

## 📝 Logs

Os logs são gerados automaticamente e podem ser configurados em:
- Console (desenvolvimento)
- Arquivo (produção)
- Sistema de logging externo

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 🆘 Suporte

Para dúvidas ou problemas:
1. Verifique a documentação
2. Consulte as issues do projeto
3. Abra uma nova issue se necessário

---

**Desenvolvido com ❤️ usando .NET Core e Blazor**
