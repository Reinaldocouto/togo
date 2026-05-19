
# TOGO — Fase 4.3.6: Close/Cancel Attendance Use Cases

## 1. Objetivo

Esta fase cria os use cases de fechamento e cancelamento de Attendance na camada Application, reutilizando as regras já consolidadas no domínio.

## 2. Contexto

- O domínio Attendance foi consolidado na Fase 4.2.
- Os contracts de Application foram criados na Fase 4.3.1.
- A interface `IAttendanceRepository` foi criada na Fase 4.3.2.
- Os validators de Application foram criados na Fase 4.3.3.
- O `CreateAttendanceUseCase` foi criado na Fase 4.3.4.
- Os use cases de consulta (`Get`/`List`) foram criados na Fase 4.3.5.

## 3. Use cases criados

### CloseAttendanceUseCase

- **Arquivo:** `backend/src/Togo.Application/Attendances/UseCases/CloseAttendanceUseCase.cs`
- **Namespace:** `Togo.Application.Attendances.UseCases`
- **Dependência:** `IAttendanceRepository`
- **Entrada:** `long id`, `CloseAttendanceRequest request`
- **Saída:** `ApplicationResult<AttendanceResponse>`
- **Fluxo:** valida id, busca por id, retorna `NotFound` quando necessário, chama `attendance.Close(request.ClosedAt)`, persiste com `UpdateAsync`, mapeia e retorna `Success`.
- **Erros tratados:**
  - `ArgumentException` (domínio) -> `ValidationError`
  - `InvalidOperationException` (domínio) -> `Conflict`

### CancelAttendanceUseCase

- **Arquivo:** `backend/src/Togo.Application/Attendances/UseCases/CancelAttendanceUseCase.cs`
- **Namespace:** `Togo.Application.Attendances.UseCases`
- **Dependência:** `IAttendanceRepository`
- **Entrada:** `long id`
- **Saída:** `ApplicationResult<AttendanceResponse>`
- **Fluxo:** valida id, busca por id, retorna `NotFound` quando necessário, chama `attendance.Cancel()`, persiste com `UpdateAsync`, mapeia e retorna `Success`.
- **Erros tratados:**
  - `InvalidOperationException` (domínio) -> `Conflict`

## 4. Testes criados

### CloseAttendanceUseCase

- `ExecuteAsync_ShouldReturnValidationError_WhenIdIsInvalid`
- `ExecuteAsync_ShouldReturnNotFound_WhenAttendanceDoesNotExist`
- `ExecuteAsync_ShouldReturnSuccess_WhenAttendanceIsOpen`
- `ExecuteAsync_ShouldReturnValidationError_WhenClosedAtIsDefault`
- `ExecuteAsync_ShouldReturnValidationError_WhenClosedAtIsBeforeOpenedAt`
- `ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsAlreadyClosed`
- `ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsCanceled`

### CancelAttendanceUseCase

- `ExecuteAsync_ShouldReturnValidationError_WhenIdIsInvalid`
- `ExecuteAsync_ShouldReturnNotFound_WhenAttendanceDoesNotExist`
- `ExecuteAsync_ShouldReturnSuccess_WhenAttendanceIsOpen`
- `ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsClosed`
- `ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsAlreadyCanceled`

## 5. Decisões técnicas

- Use cases trabalham com entidade de domínio e retornam DTO da camada Application.
- `CloseAttendanceUseCase` usa `CloseAttendanceRequest`.
- `CancelAttendanceUseCase` não usa request/body nesta fase.
- `UpdateAsync` é usado após alteração de estado no domínio.
- `ArgumentException` vira `ValidationError`.
- `InvalidOperationException` vira `Conflict`.
- Sem acesso direto a EF/banco.
- Sem eventos/RabbitMQ.

## 6. Notas de estabilização das PRs 84, 85 e 86

- PR 84 teve falha inicial por ordem incorreta de catch.
- Em C#, catch de exceção base antes de derivada torna o catch derivado inalcançável.
- PR 85 corrigiu o tratamento para catch `ArgumentException` e também revelou cuidado com default em parâmetro nullable.
- Em testes, `default` em `DateTime?` vira `null`; usar `default(DateTime)` quando a intenção for `DateTime.MinValue`.
- PR 86 revelou que `Attendance.Id` permanece 0 em entidades criadas por factory antes da persistência real.
- Em testes de Application, não usar `attendance.Id` como lookup quando a entidade não passou por infraestrutura.
- Foi criado `AddAttendanceForLookup` no `FakeAttendanceRepository` para simular busca por id.
- Testes de lista devem preferir campos controlados, como `AttendanceNumber`, em vez de `Id` default.
- `git diff --check` pegou trailing whitespace no `FakeAttendanceRepository`, mesmo com build/test passando.
- Build/test não substituem validação de whitespace.

## 7. Fora do escopo

- Infrastructure
- Repository concreto
- API/controller
- Endpoints
- DI/Program.cs
- Migrations
- Banco
- `ListByPatientId`
- Paginação
- Filtros
- Projection Patient/Pet/Tutor
- Eventos
- RabbitMQ
- Redis
- Docker
- Kubernetes

## 8. Próxima fase recomendada

**Fase 4.3.7 — Documentar Application Attendance implementado/testado.**

**Objetivo:**
Consolidar a camada Application de Attendance, documentando contracts, repository interface, validators, use cases, testes, correções manuais e aprendizados das PRs 84–86.
=======
# PHASE 04.03.06 — Attendance Close/Cancel Use Cases (Audit Addendum)

## Notas de auditoria e estabilização dos testes

- PR 84 falhou por ordem incorreta de `catch` (exceção base antes da derivada), gerando tratamento inconsistente.
- PR 85 expôs cuidado com uso de `default` em parâmetro nullable: quando a intenção é `DateTime.MinValue`, o teste deve deixar isso explícito com `default(DateTime)` ou variável tipada.
- PR 86 expôs cuidado com `Attendance.Id = 0` em entidades criadas por factory (`Attendance.Create(...)`) e a necessidade de lookup artificial em testes de aplicação.
- PR 86 também mostrou que build/test não substituem `git diff --check`, que detecta trailing whitespace.
- PR 87 expôs desalinhamento entre mensagens esperadas nos testes e mensagens reais do domínio/use case.
- A correção definitiva desta trilha foi auditar testes, fake repository, mensagens esperadas, lookup artificial por id e tratamento de exceções esperado.

### Regras práticas para próximos testes

- Não usar `Id` não persistido como chave de lookup em testes de aplicação.
- Não usar `default` solto em nullable quando a intenção for `DateTime.MinValue`.
- Não escrever assertion de mensagem sem conferir a origem real (domínio, validator ou use case).
- Executar `git diff --check` antes do commit para prevenir whitespace residual.
- Rodar `dotnet build` e `dotnet test` localmente quando possível antes de abrir PR.

