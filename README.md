# TOGO Backend

Backend do sistema TOGO, desenvolvido em .NET 8 com ASP.NET Core Web API, Entity Framework Core e MySQL. O projeto é organizado em camadas, separando domínio, aplicação, infraestrutura e API.

## Estrutura do projeto
- `backend/Togo.sln` – solução contendo todos os projetos do backend.
- `backend/src/Togo.Domain` – entidades e regras de negócio.
- `backend/src/Togo.Application` – interfaces e casos de uso.
- `backend/src/Togo.Infrastructure` – EF Core (MySQL), repositórios e segurança.
- `backend/src/Togo.Api` – Web API com Swagger, autenticação e endpoint de perfil.

## Pré-requisitos
- .NET SDK 8.0 ou superior.
- Servidor MySQL acessível em `localhost:3306` com:
  - Database: `togo`
  - User: `togo_app`
  - Password: `Togo@12345`
- (Opcional) Ferramenta `dotnet-ef` instalada globalmente para gerenciar migrations.

## Configuração do banco de dados (MySQL)
A connection string padrão está definida em `backend/src/Togo.Api/appsettings.json` e `backend/src/Togo.Api/appsettings.Development.json`:

```txt
Server=localhost;Port=3306;Database=togo;User=togo_app;Password=Togo@12345;SslMode=None;
```

Na inicialização, a API aplica `Database.Migrate()` para garantir que as tabelas existam e realiza seed do usuário administrador caso a tabela `Users` esteja vazia.

## Setup inicial
1. Restaurar dependências:
   ```bash
   dotnet restore backend/Togo.sln
   ```

2. Aplicar migrations (opcional, pois a API aplica automaticamente na inicialização):
   ```bash
   dotnet ef database update --project backend/src/Togo.Infrastructure/Togo.Infrastructure.csproj --startup-project backend/src/Togo.Api/Togo.Api.csproj
   ```

3. Usuário inicial de teste:
   - Email: `admin@togo.com`
   - Senha: `ChangeMe123!`

## Executando a API
Dentro do repositório, execute:

```bash
dotnet run --project backend/src/Togo.Api/Togo.Api.csproj
```

A API ficará disponível em `http://localhost:5000` (HTTP) e `https://localhost:7000` (HTTPS, padrão do Kestrel).

## Testes rápidos com curl/Postman
1. Login:
   ```bash
   curl -X POST http://localhost:5000/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"admin@togo.com","password":"ChangeMe123!"}'
   ```

   O token retornado deve ser enviado no header `Authorization: Bearer {token}`.

2. Perfil do usuário autenticado:
   ```bash
   curl http://localhost:5000/api/user/me \
     -H "Authorization: Bearer {token}"
   ```
