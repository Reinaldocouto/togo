# TOGO — Fase 4.4.2: Testes de Infrastructure para AttendanceRepository

## 1. Objetivo

Criar testes de Infrastructure para validar o comportamento real do `AttendanceRepository`, incluindo persistência e consultas via `AppDbContext` com EF Core.

## 2. Contexto

- A camada Application foi consolidada na Fase 4.3.
- O `AttendanceRepository` concreto foi implementado na Fase 4.4.1.
- Esta fase valida persistência real com `AppDbContext`/EF Core.
- Esta fase não cria API, DI, controller ou migration.

## 3. Estratégia de teste

- Projeto de testes usado: `backend/src/Togo.Infrastructure.Tests`.
- Provider EF Core usado: SQLite in-memory (`Microsoft.EntityFrameworkCore.Sqlite`).
- Instanciação do `AppDbContext`: factory de teste `SqliteAppDbContextFactory`, criando conexão `DataSource=:memory:`, configurando `UseSqlite` e executando `EnsureCreated`.
- Preparação de dados: cada teste cria um `Patient` válido antes de persistir `Attendance`, respeitando FK `Attendances.PatientId -> Patients.Id`.
- Isolamento: cada teste cria conexão/contexto próprios, com banco em memória isolado por teste.

## 4. Testes criados

Por método do `AttendanceRepository`:

- `AddAsync`
  - `AddAsync_ShouldPersistAttendance_WhenAttendanceIsValid`
- `GetByIdAsync`
  - `GetByIdAsync_ShouldReturnAttendance_WhenAttendanceExists`
  - `GetByIdAsync_ShouldReturnNull_WhenAttendanceDoesNotExist`
- `ListAsync`
  - `ListAsync_ShouldReturnAttendancesOrderedByOpenedAtDescendingThenIdDescending`
- `ListByPatientIdAsync`
  - `ListByPatientIdAsync_ShouldReturnOnlyAttendancesForPatient`
- `ExistsByAttendanceNumberAsync`
  - `ExistsByAttendanceNumberAsync_ShouldReturnTrue_WhenAttendanceNumberExists`
  - `ExistsByAttendanceNumberAsync_ShouldReturnFalse_WhenAttendanceNumberDoesNotExist`
- `HasOpenAttendanceForPatientAsync`
  - `HasOpenAttendanceForPatientAsync_ShouldReturnTrue_WhenPatientHasOpenAttendance`
  - `HasOpenAttendanceForPatientAsync_ShouldReturnFalse_WhenPatientHasNoOpenAttendance`
  - `HasOpenAttendanceForPatientAsync_ShouldReturnFalse_WhenPatientHasOnlyClosedOrCanceledAttendance`
- `UpdateAsync`
  - `UpdateAsync_ShouldPersistClosedAttendance_WhenAttendanceIsClosed`
  - `UpdateAsync_ShouldPersistCanceledAttendance_WhenAttendanceIsCanceled`

## 5. Decisões técnicas

- Os testes validam repository real (não fake).
- O repository não aplica regra de negócio, apenas persistência/consulta.
- Regras de estado (`Close`, `Cancel`) permanecem no domínio.
- A Application continua responsável por validação/orquestração.
- A Infrastructure apenas persiste e consulta.
- Todos os métodos assíncronos aceitam `CancellationToken`.
- Não houve criação de migration e não houve database update.
- Na Fase 4.4.2.1, o projeto `Togo.Infrastructure.Tests` foi adicionado à solution `backend/Togo.sln` para garantir que `dotnet build backend/Togo.sln` e `dotnet test backend/Togo.sln` compilem/executem os testes de Infrastructure; antes dessa correção, os testes existiam como projeto, mas não estavam integrados ao `.sln`.

## 6. Fora do escopo

- Domain
- Application
- API
- Controllers
- Endpoints
- Program.cs/DI
- migrations
- database update
- RabbitMQ
- Redis
- Docker
- Kubernetes
- Frontend

## 7. Validação

Comandos executados:

- `git branch --show-current`
- `git status`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Resultado no ambiente para `dotnet`:

`dotnet: command not found`

Como o SDK .NET não está disponível no ambiente, não foi possível executar build/test.

## 8. Próxima fase recomendada

**Fase 4.4.3 — Registrar AttendanceRepository no DI.**

Objetivo:
Registrar a implementação concreta `AttendanceRepository` para a interface `IAttendanceRepository` no container de dependência da aplicação, preparando a API para consumir os use cases de Attendance.
