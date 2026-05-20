# TOGO — Fase 4.4.1: AttendanceRepository na Infrastructure

## 1. Objetivo

Esta fase cria o repository concreto de **Attendance** na camada **Infrastructure**, implementando a persistência e consulta da entidade com EF Core sobre o `AppDbContext`.

## 2. Contexto

- A camada **Application** de Attendance foi consolidada na Fase 4.3.
- A interface `IAttendanceRepository` já existia na Application.
- Esta fase implementa a persistência real com **EF Core + AppDbContext**.
- Esta fase **não** cria API, controller ou endpoints.

## 3. Repository criado

- **Arquivo criado:** `backend/src/Togo.Infrastructure/Repositories/AttendanceRepository.cs`
- **Namespace:** `Togo.Infrastructure.Repositories`
- **Interface implementada:** `Togo.Application.Attendances.Repositories.IAttendanceRepository`
- **Dependências usadas:**
  - `AppDbContext` (`Togo.Infrastructure.Persistence`)
  - `Microsoft.EntityFrameworkCore`
  - `Attendance` (`Togo.Domain.Entities`)
  - `AttendanceStatus` (`Togo.Domain.Enums`)

## 4. Métodos implementados

### GetByIdAsync
- **Objetivo:** buscar Attendance por Id.
- **EF Core:** `FirstOrDefaultAsync` com filtro por `Id`.
- **AsNoTracking:** sim.
- **SaveChangesAsync:** não.

### ListAsync
- **Objetivo:** listar attendances de forma determinística.
- **EF Core:** `OrderByDescending(OpenedAt).ThenByDescending(Id).ToListAsync`.
- **AsNoTracking:** sim.
- **SaveChangesAsync:** não.

### ListByPatientIdAsync
- **Objetivo:** listar attendances de um paciente específico.
- **EF Core:** `Where(PatientId == ...)` + ordenação determinística + `ToListAsync`.
- **AsNoTracking:** sim.
- **SaveChangesAsync:** não.

### AddAsync
- **Objetivo:** persistir novo Attendance.
- **EF Core:** `AddAsync` no DbSet.
- **AsNoTracking:** não se aplica (escrita).
- **SaveChangesAsync:** sim.

### UpdateAsync
- **Objetivo:** atualizar Attendance existente.
- **EF Core:** `_context.Attendances.Update(attendance)`.
- **AsNoTracking:** não se aplica (escrita).
- **SaveChangesAsync:** sim.

### ExistsByAttendanceNumberAsync
- **Objetivo:** verificar existência por `AttendanceNumber`.
- **EF Core:** `AnyAsync`.
- **AsNoTracking:** sim.
- **SaveChangesAsync:** não.

### HasOpenAttendanceForPatientAsync
- **Objetivo:** verificar se o paciente já possui atendimento aberto.
- **EF Core:** `AnyAsync` com filtro por `PatientId` e `Status == AttendanceStatus.Open`.
- **AsNoTracking:** sim.
- **SaveChangesAsync:** não.

## 5. Decisões técnicas

- Uso de `AppDbContext` como gateway de persistência.
- Uso de EF Core para consultas e escrita.
- Uso de `CancellationToken` em todos os métodos.
- Queries sem regra de negócio extra (somente consulta/persistência).
- A Application permanece responsável por orquestração e fluxo.
- O Domain permanece responsável por regras de estado/transição.
- O repository atua apenas na persistência e consulta.

## 6. Fora do escopo

- Domain
- Application
- API
- Controllers
- Endpoints
- Program.cs/DI (não alterado nesta fase)
- Migrations
- Database update
- Eventos
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

Resultados observados nesta execução:

- `git branch --show-current` (antes): `work`
- `git status` (antes): branch `work`, sem alterações pendentes
- `git diff --check`: sem saída (sem problemas de whitespace)
- `dotnet build backend/Togo.sln`: `dotnet: command not found`
- `dotnet test backend/Togo.sln`: `dotnet: command not found`

## 8. Próxima fase recomendada

**Fase 4.4.2 — Criar testes de Infrastructure para AttendanceRepository.**

Objetivo:
Validar o comportamento real do AttendanceRepository com AppDbContext/EF Core em ambiente de teste, cobrindo consultas, criação, atualização, unicidade de AttendanceNumber e verificação de atendimento aberto.
