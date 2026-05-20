# TOGO — Fase 4.5.1: AttendanceController com endpoints mínimos

## 1. Objetivo

Criar o controller REST da vertical Attendance com endpoints mínimos para listagem, consulta por id, criação, fechamento e cancelamento de atendimento.

## 2. Contexto

- Domain de Attendance foi concluído.
- Application de Attendance foi concluída.
- Infrastructure de Attendance foi concluída.
- O repository `IAttendanceRepository -> AttendanceRepository` já estava registrado no DI.
- Esta fase inicia a exposição da vertical Attendance pela API REST.

## 3. Controller criado

- Arquivo: `backend/src/Togo.Api/Controllers/AttendancesController.cs`
- Namespace: `Togo.Api.Controllers`
- Rota base: `api/attendances`
- Atributos: `[Authorize]` e `[ApiController]`
- Use cases injetados:
  - `CreateAttendanceUseCase`
  - `GetAttendanceByIdUseCase`
  - `ListAttendancesUseCase`
  - `CloseAttendanceUseCase`
  - `CancelAttendanceUseCase`
- Logger injetado: `ILogger<AttendancesController>`

## 4. Endpoints criados

### GET /api/attendances

- Use case: `ListAttendancesUseCase`
- Retorno esperado: `200 OK` com lista de atendimentos (inclusive vazia).

### GET /api/attendances/{id}

- Use case: `GetAttendanceByIdUseCase`
- Retornos esperados:
  - `200 OK`
  - `400 BadRequest` para id inválido
  - `404 NotFound`

### POST /api/attendances

- Request: `CreateAttendanceRequest`
- Use case: `CreateAttendanceUseCase`
- Sucesso: `CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)`
- Retornos esperados:
  - `201 Created`
  - `400 ValidationError`
  - `404 NotFound` (patient não encontrado)
  - `409 Conflict`

### PATCH /api/attendances/{id}/close

- Request: `CloseAttendanceRequest`
- Use case: `CloseAttendanceUseCase`
- Retornos esperados:
  - `200 OK`
  - `400 ValidationError`
  - `404 NotFound`
  - `409 Conflict`

### PATCH /api/attendances/{id}/cancel

- Sem body
- Use case: `CancelAttendanceUseCase`
- Retornos esperados:
  - `200 OK`
  - `400 ValidationError`
  - `404 NotFound`
  - `409 Conflict`

## 5. Mapeamento de ApplicationResult para HTTP

- `Success` -> `200 OK`
- `Created` no POST de sucesso -> `201 Created`
- `ValidationError` -> `400 BadRequest`
- `NotFound` -> `404 NotFound`
- `Conflict` -> `409 Conflict`
- fallback -> `500 InternalServerError`

## 6. DI atualizado

Registrados com `AddScoped` em `Program.cs`:

- Use cases:
  - `CreateAttendanceUseCase`
  - `GetAttendanceByIdUseCase`
  - `ListAttendancesUseCase`
  - `CloseAttendanceUseCase`
  - `CancelAttendanceUseCase`
- Validators:
  - `AttendancePatientExistsValidator`
  - `AttendanceNumberUniqueValidator`
  - `OpenAttendanceValidator`
- Repository já existente no DI:
  - `IAttendanceRepository -> AttendanceRepository`
- Lifetime utilizado: `Scoped`.

## 7. Decisões técnicas

- O controller não contém regra de negócio.
- A regra permanece em Domain/Application.
- A API recebe request, chama use case e traduz `ApplicationResult` para HTTP.
- Endpoints de close/cancel usam `PATCH` por representarem transições parciais de estado.
- Cancelamento não recebe body nesta fase.

## 8. Fora do escopo

- Domain
- Application (exceto bug bloqueante)
- Infrastructure repository
- migrations
- database update
- testes de API
- paginação
- filtros
- endpoint `ListByPatientId`
- Swagger avançado
- autenticação customizada
- eventos
- RabbitMQ
- Redis
- Docker
- Kubernetes
- Frontend

## 9. Validação

Comandos executados nesta fase:

- `git branch --show-current`
- `git status`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

## 10. Próxima fase recomendada

**Fase 4.5.2 — Criar testes de API para AttendanceController**

Objetivo:
Validar os endpoints mínimos de Attendance, incluindo status codes, rotas, requests e integração com use cases/fakes/mocks conforme padrão atual de `Togo.Api.Tests`.
