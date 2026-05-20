# TOGO — Fase 4.5.4: Validação ponta a ponta local/API da Attendance

## 1. Objetivo

Esta fase valida, ou prepara de forma auditável, a validação prática dos endpoints reais da API Attendance em ambiente local, cobrindo autenticação, banco de dados, requests, responses HTTP e o fluxo completo de negócio da vertical Attendance.

## 2. Contexto

- Domain, Application e Infrastructure da vertical Attendance já foram concluídos em fases anteriores.
- O controller da Attendance foi criado na Fase 4.5.1.
- Os testes de controller/API foram criados na Fase 4.5.2.
- A validação documental de rotas, status code, responses e Swagger foi concluída na Fase 4.5.3.
- Esta Fase 4.5.4 foca a validação prática/manual/local da API real.

## 3. Pré-requisitos para execução local

### 3.1 Itens de ambiente

- SDK .NET instalado e disponível no terminal.
- Banco configurado conforme ambiente local (compatível com `AppDbContext`/MySQL).
- String de conexão `ConnectionStrings:Default` configurada para a API.
- API rodando localmente.
- Usuário válido para autenticação (`POST /api/Auth/login`).
- Patient existente no banco para criação de Attendance.
- Ferramenta para chamadas HTTP:
  - Swagger UI;
  - Postman;
  - Insomnia;
  - `curl`.

### 3.2 Comandos esperados

```bash
dotnet restore backend/Togo.sln
dotnet build backend/Togo.sln
dotnet test backend/Togo.sln
dotnet run --project backend/src/Togo.Api/Togo.Api.csproj
```

### 3.3 Observação de limitação no ambiente Codex

Caso o ambiente não possua .NET SDK, registrar explicitamente:

```text
dotnet: command not found
```

Não declarar execução real da API se ela não foi efetivamente executada.

## 4. Autenticação

Fluxo de autenticação da API:

- Endpoint: `POST /api/Auth/login`
- Body (baseado em `LoginRequest`):

```json
{
  "email": "usuario@exemplo.com",
  "password": "senha"
}
```

Resultado esperado:

- `200 OK` com token em caso de sucesso.
- `401 Unauthorized` em caso de credenciais inválidas.

Os endpoints da Attendance estão protegidos por `[Authorize]` e exigem:

```http
Authorization: Bearer <token>
```

## 5. Dados mínimos necessários

Para criar Attendance:

- `PatientId` existente.
- `AttendanceNumber` único.
- `OpenedAt` válido.
- `Type` válido.

Payload base para criação:

```json
{
  "patientId": 1,
  "attendanceNumber": "ATT-E2E-001",
  "openedAt": "2026-05-20T10:00:00Z",
  "type": "Consultation"
}
```

Atenção sobre enum `AttendanceType`:

- A API está configurada para serialização JSON padrão e os enums de persistência usam conversão para string no EF.
- Para requests HTTP, validar no ambiente real se o binding aceita string (`"Consultation"`) ou valor numérico.
- Nesta fase documental, o payload principal usa `"Consultation"`; se o ambiente exigir numérico, usar o valor correspondente do enum atual do domínio.

## 6. Fluxo ponta a ponta esperado

### 6.1 Login

- Endpoint: `POST /api/Auth/login`
- Validar:
  - status `200`;
  - token retornado;
  - token aplicado nos próximos requests.

### 6.2 Criar Attendance

- Endpoint: `POST /api/attendances`
- Headers:
  - `Authorization: Bearer <token>`
  - `Content-Type: application/json`
- Body:

```json
{
  "patientId": 1,
  "attendanceNumber": "ATT-E2E-001",
  "openedAt": "2026-05-20T10:00:00Z",
  "type": "Consultation"
}
```

Validar:

- `201 Created`;
- response contém `Id`;
- `PatientId` correto;
- `AttendanceNumber` correto;
- `Status` = `Open`;
- `ClosedAt` = `null`;
- `Type` coerente;
- header `Location`/`CreatedAtAction` quando disponível.

### 6.3 Buscar Attendance por Id

- Endpoint: `GET /api/attendances/{id}`
- Validar:
  - `200 OK`;
  - response representa o atendimento criado.

### 6.4 Listar Attendances

- Endpoint: `GET /api/attendances`
- Validar:
  - `200 OK`;
  - lista contém o atendimento criado.

### 6.5 Fechar Attendance

- Endpoint: `PATCH /api/attendances/{id}/close`
- Body:

```json
{
  "closedAt": "2026-05-20T11:00:00Z"
}
```

Validar:

- `200 OK`;
- `Status` = `Closed`;
- `ClosedAt` preenchido.

### 6.6 Tentar cancelar Attendance fechado

- Endpoint: `PATCH /api/attendances/{id}/cancel`
- Validar:
  - `409 Conflict`;
  - mensagem coerente com regra de domínio.

### 6.7 Criar outro Attendance para testar cancelamento

- Endpoint: `POST /api/attendances`
- Body:

```json
{
  "patientId": 1,
  "attendanceNumber": "ATT-E2E-002",
  "openedAt": "2026-05-20T12:00:00Z",
  "type": "Consultation"
}
```

Validar:

- `201 Created`.

### 6.8 Cancelar Attendance aberto

- Endpoint: `PATCH /api/attendances/{id}/cancel`
- Validar:
  - `200 OK`;
  - `Status` = `Canceled`;
  - `ClosedAt` permanece `null`.

## 7. Cenários negativos mínimos

### 7.1 GetById com id inválido

- `GET /api/attendances/0`
- Esperado: `400 BadRequest`.

### 7.2 GetById inexistente

- `GET /api/attendances/999999`
- Esperado: `404 NotFound`.

### 7.3 Create com Patient inexistente

- `POST /api/attendances` com `patientId` inexistente.
- Esperado: `404 NotFound`.

### 7.4 Create com AttendanceNumber duplicado

- Repetir `attendanceNumber` já utilizado.
- Esperado: `409 Conflict`.

### 7.5 Create com paciente já com atendimento aberto

- Tentar criar segundo atendimento aberto para o mesmo `PatientId`.
- Esperado: `409 Conflict`.

### 7.6 Close com ClosedAt default/inválido

Body:

```json
{
  "closedAt": "0001-01-01T00:00:00Z"
}
```

Esperado: `400 BadRequest`.

### 7.7 Close com ClosedAt antes de OpenedAt

- Esperado: `400 BadRequest`.

### 7.8 Cancel de Attendance já cancelado

- Esperado: `409 Conflict`.

### 7.9 Chamada sem token

- Qualquer endpoint de Attendance sem `Authorization`.
- Esperado: `401 Unauthorized`.

## 8. Exemplos curl

> Placeholders: `{{BASE_URL}}`, `{{TOKEN}}`, `{{ATTENDANCE_ID}}`, `{{PATIENT_ID}}`.

### 8.1 Login

```bash
curl -X POST "{{BASE_URL}}/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "usuario@exemplo.com",
    "password": "senha"
  }'
```

### 8.2 Create

```bash
curl -X POST "{{BASE_URL}}/api/attendances" \
  -H "Authorization: Bearer {{TOKEN}}" \
  -H "Content-Type: application/json" \
  -d '{
    "patientId": {{PATIENT_ID}},
    "attendanceNumber": "ATT-E2E-001",
    "openedAt": "2026-05-20T10:00:00Z",
    "type": "Consultation"
  }'
```

### 8.3 GetById

```bash
curl -X GET "{{BASE_URL}}/api/attendances/{{ATTENDANCE_ID}}" \
  -H "Authorization: Bearer {{TOKEN}}"
```

### 8.4 List

```bash
curl -X GET "{{BASE_URL}}/api/attendances" \
  -H "Authorization: Bearer {{TOKEN}}"
```

### 8.5 Close

```bash
curl -X PATCH "{{BASE_URL}}/api/attendances/{{ATTENDANCE_ID}}/close" \
  -H "Authorization: Bearer {{TOKEN}}" \
  -H "Content-Type: application/json" \
  -d '{
    "closedAt": "2026-05-20T11:00:00Z"
  }'
```

### 8.6 Cancel

```bash
curl -X PATCH "{{BASE_URL}}/api/attendances/{{ATTENDANCE_ID}}/cancel" \
  -H "Authorization: Bearer {{TOKEN}}"
```

## 9. Resultado da execução

### 9.1 Executado nesta fase

- Inspeção técnica dos artefatos obrigatórios (controller, auth, contracts, Program, repository, configurações e testes).
- Criação deste roteiro auditável de validação ponta a ponta local/API.
- Execução de comandos Git de validação de branch e integridade do diff.

### 9.2 Não executado por limitação de ambiente

- Execução real da API (`dotnet run`) e chamadas HTTP reais não foram realizadas neste ambiente Codex.
- Build/test automatizado depende de disponibilidade de `dotnet` no ambiente.

### 9.3 Pendente para execução manual local

- Rodar API localmente.
- Executar todo fluxo prático com autenticação e banco real.
- Coletar evidências (status, payloads, respostas e logs).

Se aplicável ao ambiente:

```text
dotnet: command not found
```

Nesse caso, a validação prática fica pendente e este documento funciona como roteiro auditável para execução manual.

## 10. Evidências esperadas

Coletar quando executar manualmente:

- print/log da autenticação (`/api/Auth/login`);
- response de create;
- `Id` gerado;
- response de get by id;
- response de list;
- response de close;
- response de cancel;
- status codes de cada etapa;
- resultado de `dotnet build` e `dotnet test`;
- versão do SDK e descrição do ambiente utilizado.

## 11. Decisões técnicas

- Esta fase não altera regra de negócio.
- Esta fase valida o fluxo real da API.
- A validação depende de autenticação.
- A validação depende de banco local acessível.
- A validação depende de `Patient` existente.
- Esta fase não cria migration.
- Esta fase não executa `database update` automaticamente.

## 12. Lacunas e riscos

- O ambiente Codex pode não executar `dotnet`.
- A autenticação real depende de usuário local válido.
- Dados locais podem variar entre ambientes.
- Banco pode não ter `Patient` válido para cenário de create.
- Migrations podem estar fora de sincronia caso o ambiente local esteja desatualizado.
- Swagger acelera testes manuais, mas não substitui validação real de fluxo completo.

## 13. Fora do escopo

- Domain.
- Application.
- Infrastructure.
- Alteração de controller.
- Alteração de repository.
- Migrations.
- Database update.
- Novos endpoints.
- Paginação.
- Filtros.
- E2E automatizado com WebApplicationFactory.
- Teste de carga.
- RabbitMQ.
- Redis.
- Docker.
- Kubernetes.
- Frontend.

## 14. Validação técnica da branch

Comandos executados nesta fase:

```bash
git branch --show-current
git status
git diff --check
dotnet build backend/Togo.sln
dotnet test backend/Togo.sln
```

Se `dotnet` não estiver disponível, registrar:

```text
dotnet: command not found
```

## 15. Próxima fase recomendada

**Fase 4.5.5 — Documentar fechamento da API Attendance**.

Objetivo:
Consolidar a camada API da vertical Attendance, incluindo controller, testes, validação de rotas/status/responses/Swagger, plano/resultado de validação ponta a ponta, lacunas e próximos passos.
