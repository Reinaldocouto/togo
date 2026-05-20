# TOGO — Fase 4.5.2: Testes de API para AttendanceController

## 1. Objetivo

Criar testes para validar os endpoints mínimos do `AttendanceController`, cobrindo o mapeamento de respostas HTTP e o contrato básico de retorno.

## 2. Contexto

- Domain/Application/Infrastructure da vertical Attendance já haviam sido concluídos em fases anteriores.
- A Fase 4.5.1 criou o `AttendancesController` com endpoints mínimos.
- Esta fase valida rotas/status codes/responses no nível de API/controller.
- Esta fase não altera regra de negócio.

## 3. Estratégia de teste escolhida

- Estratégia aplicada: **testes unitários diretos de controller**, instanciando `AttendancesController` sem subir servidor real.
- Motivo: menor complexidade e aderência ao estado atual do `Togo.Api.Tests`, que ainda não possui padrão consolidado de testes de endpoint com `WebApplicationFactory`.
- Limitação observada: como os use cases são concretos, foi necessário montar dependências mínimas locais no projeto de testes.
- Foram criados fakes/helpers locais no próprio `Togo.Api.Tests`:
  - `FakeAttendanceRepository`;
  - `FakePetRepository`;
  - `NullLogger<T>`.
- Não houve instalação de pacotes adicionais (incluindo mocking framework).

## 4. Testes criados

Arquivo: `backend/src/Togo.Api.Tests/Controllers/AttendancesControllerTests.cs`.

### GET /api/attendances

- `List_ShouldReturnOk_WhenUseCaseReturnsSuccess` (sucesso / 200).

### GET /api/attendances/{id}

- `GetById_ShouldReturnOk_WhenAttendanceExists` (sucesso / 200);
- `GetById_ShouldReturnBadRequest_WhenIdIsInvalid` (400);
- `GetById_ShouldReturnNotFound_WhenAttendanceDoesNotExist` (404).

### POST /api/attendances

- `Create_ShouldReturnCreatedAtAction_WhenRequestIsValid` (201 + `CreatedAtAction`);
- `Create_ShouldReturnBadRequest_WhenValidationFails` (400);
- `Create_ShouldReturnNotFound_WhenPatientDoesNotExist` (404);
- `Create_ShouldReturnConflict_WhenAttendanceNumberAlreadyExistsOrPatientHasOpenAttendance` (409).

### PATCH /api/attendances/{id}/close

- `Close_ShouldReturnOk_WhenAttendanceIsClosed` (sucesso / 200);
- `Close_ShouldReturnBadRequest_WhenValidationFails` (400);
- `Close_ShouldReturnNotFound_WhenAttendanceDoesNotExist` (404);
- `Close_ShouldReturnConflict_WhenAttendanceCannotBeClosed` (409).

### PATCH /api/attendances/{id}/cancel

- `Cancel_ShouldReturnOk_WhenAttendanceIsCanceled` (sucesso / 200);
- `Cancel_ShouldReturnBadRequest_WhenIdIsInvalid` (400);
- `Cancel_ShouldReturnNotFound_WhenAttendanceDoesNotExist` (404);
- `Cancel_ShouldReturnConflict_WhenAttendanceCannotBeCanceled` (409).

## 5. Mapeamento HTTP validado

- `Success` -> `200 OK`;
- `Created` no POST -> `201 Created`;
- `ValidationError` -> `400 BadRequest`;
- `NotFound` -> `404 NotFound`;
- `Conflict` -> `409 Conflict`.

## 6. Decisões técnicas

- O controller permanece sem regra de negócio.
- Os testes validam a tradução HTTP do `ApplicationResult`.
- Não foram criadas interfaces novas na Application apenas para teste.
- Não houve alteração de Domain/Application/Infrastructure.
- Não houve migration/database update.

## 7. Fora do escopo

- Domain;
- Application;
- Infrastructure;
- alteração de repository;
- migration;
- database update;
- testes end-to-end reais;
- autenticação real/JWT real;
- Swagger;
- paginação;
- filtros;
- endpoint `ListByPatientId`;
- RabbitMQ;
- Redis;
- Docker;
- Kubernetes;
- Frontend.

## 8. Validação

Comandos executados nesta fase:

- `git branch --show-current`
- `git status`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

## 9. Próxima fase recomendada

**Fase 4.5.3 — Validar rotas/status codes/responses e Swagger.**

Objetivo:
Revisar a API exposta, documentação Swagger, consistência de rotas, nomes, status codes e contratos antes do teste manual/end-to-end.
