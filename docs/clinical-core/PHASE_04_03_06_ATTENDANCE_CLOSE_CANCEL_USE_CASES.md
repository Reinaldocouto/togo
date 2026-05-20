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

## 6. Incidentes e aprendizados da Fase 4.3

### 6.1 `default(DateTime)` vs `default` em nullable

- Em C#, `default` em parâmetro `DateTime?` pode ser interpretado como `null`.
- Quando a intenção do teste for validar `DateTime.MinValue`, usar `default(DateTime)` ou variável tipada.
- Esse erro apareceu nos testes de `CreateAttendanceUseCase`.

### 6.2 `Attendance.Id = 0` antes da persistência

- Entidades criadas por `Attendance.Create(...)` nascem com `Id = 0`.
- O ID real será atribuído pela infraestrutura/persistência no futuro.
- Testes de Application não devem usar `attendance.Id` como chave de lookup quando a entidade ainda não foi persistida.
- O padrão correto nos testes é usar `FakeAttendanceRepository.AddAttendanceForLookup(long id, Attendance attendance)`.

### 6.3 Mensagens reais do domínio

- Os use cases retornam `ex.Message` das exceções do domínio.
- Os testes devem alinhar suas assertions com as mensagens reais lançadas por `Attendance`.
- A PR 87 falhou inicialmente porque alguns testes esperavam mensagens genéricas diferentes das mensagens reais.
- Quando a mensagem vier de `ArgumentException` e puder incluir parâmetro, usar `Assert.StartsWith(...)` ou `Assert.Contains(...)` quando apropriado.

### 6.4 Branch correta antes de alterar arquivo

- Antes de alterar qualquer arquivo, confirmar a branch com:
  - `git branch --show-current`
  - `git status`
- Correções em branch errada atrasam o fluxo e podem não impactar a PR correta.
- Esse problema aconteceu durante a correção entre PRs 86/87.

### 6.5 Codex sem `dotnet`

- O ambiente Codex usado em várias PRs não tinha CLI .NET disponível.
- Quando `dotnet build` ou `dotnet test` falharem por `dotnet: command not found`, isso deve ser registrado como limitação de ambiente, não como sucesso.
- A validação final precisa ocorrer localmente ou via GitHub Actions.

### 6.6 CI como gate obrigatório

- GitHub Actions deve ser tratado como gate obrigatório.
- PR só deve ser considerada concluída quando CI passar.
- Validação local ajuda, mas CI é a referência final antes do merge.

### 6.7 `git diff --check`

- Build/test não detectam trailing whitespace.
- `git diff --check` detecta espaços residuais e deve ser executado antes do commit.
- Esse problema ocorreu no `FakeAttendanceRepository`.

### 6.8 Fake repository testado

- `FakeAttendanceRepository` precisou evoluir para suportar:
  - múltiplas entidades com `Id = 0`;
  - lookup artificial por id;
  - contadores de chamadas;
  - atualização sem colapsar lista indevidamente.
- Sempre que o fake ganhar comportamento novo, deve ganhar teste próprio.

### 6.9 Evitar PR auxiliar contra branch errada

- A PR 88 foi criada como auditoria, mas foi mergeada contra `main`, não contra a branch da PR 87.
- Isso não corrigiu diretamente a PR 87.
- Quando a intenção for corrigir uma PR aberta, a nova branch/PR deve ser baseada na branch da PR problemática ou a correção deve ser feita diretamente nela.

### 6.10 Cuidado com conflito documental

- Conflitos em arquivos `.md` também precisam de revisão.
- Marcadores como `<<<<<<<`, `=======` e `>>>>>>>` nunca devem permanecer.
- O documento da Fase 4.3.6 ficou com um `=======` residual e esta fase 4.3.6.1 corrige isso.

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
Consolidar a camada Application de Attendance, documentando contratos, interface de repositório, validators, use cases, testes, correções manuais, validações locais/CI e aprendizados da Fase 4.3.
