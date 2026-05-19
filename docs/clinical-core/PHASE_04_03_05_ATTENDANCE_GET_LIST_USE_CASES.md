# TOGO — Fase 4.3.5: Get/List Attendance Use Cases

## 1. Objetivo

Esta fase cria os use cases de consulta de Attendance na camada Application, cobrindo busca por identificador e listagem geral.

## 2. Contexto

- O domínio de Attendance foi consolidado na Fase 4.2.
- Os contratos de Application foram criados na Fase 4.3.1.
- A interface `IAttendanceRepository` foi criada na Fase 4.3.2.
- Os validators de Attendance foram criados na Fase 4.3.3.
- O `CreateAttendanceUseCase` foi criado na Fase 4.3.4.
- O tratamento de exceções de domínio no `CreateAttendanceUseCase` foi corrigido na Fase 4.3.4.1.

## 3. Use cases criados

### GetAttendanceByIdUseCase

- **Arquivo:** `backend/src/Togo.Application/Attendances/UseCases/GetAttendanceByIdUseCase.cs`
- **Namespace:** `Togo.Application.Attendances.UseCases`
- **Dependência:** `IAttendanceRepository`
- **Entrada:** `long id`
- **Saída:** `ApplicationResult<AttendanceResponse>`
- **Fluxo:**
  1. Valida `id <= 0` e retorna `ValidationError`.
  2. Busca com `GetByIdAsync`.
  3. Retorna `NotFound` quando não existe.
  4. Mapeia entidade para `AttendanceResponse` e retorna `Success`.

### ListAttendancesUseCase

- **Arquivo:** `backend/src/Togo.Application/Attendances/UseCases/ListAttendancesUseCase.cs`
- **Namespace:** `Togo.Application.Attendances.UseCases`
- **Dependência:** `IAttendanceRepository`
- **Entrada:** nenhuma obrigatória
- **Saída:** `ApplicationResult<IReadOnlyList<AttendanceListItemResponse>>`
- **Fluxo:**
  1. Busca lista com `ListAsync`.
  2. Mapeia entidade para `AttendanceListItemResponse`.
  3. Retorna `Success`.

## 4. Testes criados

- `GetAttendanceByIdUseCaseTests`
  - `ExecuteAsync_ShouldReturnValidationError_WhenIdIsInvalid`
  - `ExecuteAsync_ShouldReturnNotFound_WhenAttendanceDoesNotExist`
  - `ExecuteAsync_ShouldReturnSuccess_WhenAttendanceExists`
- `ListAttendancesUseCaseTests`
  - `ExecuteAsync_ShouldReturnSuccessWithEmptyList_WhenNoAttendancesExist`
  - `ExecuteAsync_ShouldReturnSuccessWithAttendances_WhenAttendancesExist`

## 5. Decisões técnicas

- As responses usam DTOs de Application (`AttendanceResponse` e `AttendanceListItemResponse`).
- O repository retorna entidade de domínio.
- Cada use case faz o mapeamento da entidade para response.
- Não foi criado filtro por `PatientId` nesta fase.
- Não foi adicionada paginação.
- Não foi adicionada projection de Patient/Pet/Tutor.
- Não há acesso direto a EF/banco pelos use cases.

## 6. Fora do escopo

- Infrastructure
- Repository concreto
- API/controller
- Endpoints
- DI/`Program.cs`
- Migrations
- Banco
- `CloseAttendanceUseCase`
- `CancelAttendanceUseCase`
- `ListByPatientId`
- Paginação
- Filtros
- Projection Patient/Pet/Tutor
- RabbitMQ
- Redis
- Docker
- Kubernetes

## 7. Próxima fase recomendada

**Fase 4.3.6 — Criar `CloseAttendanceUseCase` e `CancelAttendanceUseCase`.**

Objetivo: criar use cases de alteração de ciclo de vida de Attendance usando `IAttendanceRepository`, `CloseAttendanceRequest` e regras de domínio já existentes.
