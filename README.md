# 🔐 HumanResourcesManagement

[![.NET Version](https://img.shields.io/badge/.NET-9-blue)](https://dotnet.microsoft.com/)   
[![License: MIT](https://img.shields.io/badge/License-MIT-green)](LICENSE)

## ℹ️ Sobre

HumanResourcesManagement é uma aplicação web construída com **ASP.NET Core Razor Pages** para gerir:

- Projetos  
- Clientes / Empresas  
- Contratos  
- Colaboradores e alocação de funcionários  
- Relatórios de desempenho  

Feita para empresas que querem ter visibilidade completa sobre seus projetos, recursos humanos e contratos em um só lugar.

---

## 🛠️ Tecnologias

- .NET 9  
- C# 13  
- ASP.NET Core Razor Pages  
- Entity Framework Core  
- SQL Server  
- Bootstrap 5  

---

## ⚙️ Instalação e Configuração

### Pré-requisitos

- Visual Studio 2022 ou superior, com workload **ASP.NET e desenvolvimento web**  
- .NET 9 SDK  
- SQL Server (local ou remoto)  

### Setup local

1. Clonar o repositório:  
   ```bash
   git clone https://github.com/GoncaloPinto13/HumanResourcesManagement.git
   cd HumanResourcesManagement
   ```

2. Abrir a solução no Visual Studio:  
   - Duplo clique em `HumanResources.sln`  
   - Definir o projeto “HumanResources” como Startup  

3. Atualizar connection string em `appsettings.Development.json` ou `appsettings.json` com credenciais do teu SQL Server.

4. Aplicar migrações no **Package Manager Console**:  
   ```powershell
   Update-Database
   ```

5. Seed de dados (se houver): garantir que exista seed automático ou documento explicando como popular dados de exemplo.

6. Rodar a aplicação: pressione **F5** ou clique em *Start Debugging*.

7. Acessar via navegador em `https://localhost:5001` (ou configuração equivalente).

---

## 📅 Roadmap

- [ ] Autenticações externas (Google, Microsoft)  
- [ ] Notificações por e-mail para prazos de contrato  
- [ ] Painel de estatísticas / dashboards gráficos  
- [ ] Internacionalização (localização de idiomas)  

---

## 🤝 Contribuição

Se quiser ajudar:  

1. Faça um fork  
2. Crie uma branch com sua feature: `git checkout -b feature/nome-da-feature`  
3. Faça commit das alterações: `git commit -m "Descrição da feature"`  
4. Envie um pull request  

Por favor, siga o estilo de código existente. Se possível, inclua testes para novas funcionalidades.

---

## 🚀 Deploy em Produção

*(Se aplicável)*

- Versões release ou tags GitHub  
- Configurar variáveis de ambiente para produção: connection string, chave secreta (appsettings.Production.json ou equivalentes)  
- Hospedagem sugerida: Azure App Service, IIS, Docker  

---

## 📄 Licença

Este projeto está licenciado sob MIT — veja o arquivo [LICENSE](LICENSE) para detalhes.

---

## 📬 Contato

**Gonçalo Pinto** — *Mantenedor Principal*  
Outros contribuidores: HelomInerreli, DiogoRodriguest0130425, Henrique Magalhães  

Para qualquer dúvida, sugestão ou bug: abra **issue** ou envie mensagem direta no GitHub.

