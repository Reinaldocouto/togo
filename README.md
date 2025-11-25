# Togo Backend

A .NET 8 backend organized under `backend/` with Domain, Application, Infrastructure, and API projects. The API exposes authentication endpoints backed by SQLite and Entity Framework Core migrations.

## Estrutura do projeto
- `backend/Togo.sln` – solução contendo todos os projetos do back-end.
- `backend/src/Togo.Domain` – entidades e regras de negócio.
- `backend/src/Togo.Application` – interfaces e casos de uso.
- `backend/src/Togo.Infrastructure` – EF Core (SQLite), repositórios e segurança.
- `backend/src/Togo.Api` – Web API com Swagger, login e endpoint de perfil.

## Pré-requisitos
- .NET SDK 8.0 ou superior
- (Opcional) Ferramentas `dotnet-ef` instaladas globalmente para gerenciar migrations

## Configuração do banco de dados
O banco padrão usa SQLite em `togo.db` (ao lado do executável). As migrations já estão incluídas no projeto.

1. Restaurar dependências e aplicar migrations manualmente (opcional quando executar a API, pois ela aplica automaticamente):
   ```bash
   dotnet restore backend/Togo.sln
   dotnet ef database update --project backend/src/Togo.Infrastructure/Togo.Infrastructure.csproj --startup-project backend/src/Togo.Api/Togo.Api.csproj
   ```

2. Usuário inicial de teste
   - Email: `admin@togo.local`
   - Senha: `ChangeMe123!`

   A aplicação aplica as migrations e cria esse usuário automaticamente na inicialização caso a tabela `Users` esteja vazia.

## Executando a API
Dentro do repositório, execute:
```bash
dotnet run --project backend/src/Togo.Api/Togo.Api.csproj
```
A API ficará disponível em `http://localhost:5000` (HTTP) e `https://localhost:7000` (HTTPS por padrão do Kestrel).

## Testes rápidos com curl/Postman
- Autenticação:
  ```bash
  curl -X POST http://localhost:5000/login \
    -H "Content-Type: application/json" \
    -d '{"email":"admin@togo.local","password":"ChangeMe123!"}'
  ```
  O token retornado é temporário e deve ser enviado em `Authorization: Bearer {token}`.

- Perfil do usuário autenticado:
  ```bash
  curl http://localhost:5000/api/user/me -H "Authorization: Bearer {token}"
  ```

## Limpeza de arquivos antigos
Arquivos do projeto de console original foram removidos. Utilize apenas a solução em `backend/Togo.sln` e os projetos contidos em `backend/src/`.
