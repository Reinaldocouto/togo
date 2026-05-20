# TOGO — Fase 4.4.5: Documentação Final da Infrastructure Attendance

## 1. Objetivo

Esta fase consolida a camada **Infrastructure** de Attendance, registrando de forma unificada:

- repository concreto;
- testes de Infrastructure;
- integração do projeto de testes à solution;
- registro no DI;
- configuração EF/AppDbContext;
- riscos, lacunas e próximos passos.

## 2. Contexto

- A camada **Application** de Attendance foi concluída na Fase 4.3.
- A Fase 4.4 implementou a persistência real de Attendance.
- Esta fase é exclusivamente documental e **não** cria API/controller/endpoints.
- Ao final, a vertical Attendance fica pronta para avançar para a macrofase de API.

## 3. Escopo consolidado da Fase 4.4

### 4.4.1 — AttendanceRepository concreto

- **Objetivo:** implementar persistência e consultas reais de Attendance na Infrastructure com EF Core.
- **Arquivos principais:**
  - `backend/src/Togo.Infrastructure/Repositories/AttendanceRepository.cs`
  - `backend/src/Togo.Application/Attendances/Repositories/IAttendanceRepository.cs`
- **Decisão técnica:** manter a Application dependente da abstração (`IAttendanceRepository`) e concentrar no repository apenas responsabilidades de persistência/consulta.
- **Resultado:** repository concreto criado e aderente ao contrato da Application.

### 4.4.2 — Testes de Infrastructure

- **Objetivo:** validar comportamento real do `AttendanceRepository` com `AppDbContext`.
- **Arquivos principais:**
  - `backend/src/Togo.Infrastructure.Tests/Repositories/AttendanceRepositoryTests.cs`
  - `backend/src/Togo.Infrastructure.Tests/Support/SqliteAppDbContextFactory.cs`
  - `backend/src/Togo.Infrastructure.Tests/Togo.Infrastructure.Tests.csproj`
- **Decisão técnica:** usar SQLite in-memory para testes de integração de Infrastructure com isolamento por teste.
- **Resultado:** suíte de testes cobrindo criação, consulta, listagem, atualização e verificações por número/status.

### 4.4.2.1 — Inclusão de Togo.Infrastructure.Tests na solution

- **Objetivo:** integrar os testes de Infrastructure ao fluxo padrão de build/test da solution.
- **Arquivos principais:**
  - `backend/Togo.sln`
  - `backend/src/Togo.Infrastructure.Tests/Togo.Infrastructure.Tests.csproj`
- **Decisão técnica:** adicionar o projeto de testes à solution principal.
- **Resultado:** `dotnet build/test` da solution passa a incluir também `Togo.Infrastructure.Tests`.

### 4.4.3 — Registro de DI

- **Objetivo:** registrar `AttendanceRepository` como implementação de `IAttendanceRepository` no container.
- **Arquivos principais:**
  - `backend/src/Togo.Api/Program.cs`
- **Decisão técnica:** usar `AddScoped`, seguindo o padrão dos demais repositories.
- **Resultado:** injeção de dependência pronta para consumo pelos use cases de Attendance.

### 4.4.4 — Validação EF/AppDbContext

- **Objetivo:** validar consistência de mapeamento EF Core para Attendance.
- **Arquivos principais:**
  - `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`
  - `backend/src/Togo.Infrastructure/Persistence/Configurations/AttendanceConfiguration.cs`
  - `backend/src/Togo.Infrastructure/Persistence/Configurations/PatientConfiguration.cs`
- **Decisão técnica:** manter mapeamento atual com enum como string e relacionamento `Attendance -> Patient` com cascade.
- **Resultado:** configuração EF consolidada e documentada para fechamento da macrofase de Infrastructure.

## 4. AttendanceRepository

- **Arquivo:** `backend/src/Togo.Infrastructure/Repositories/AttendanceRepository.cs`
- **Namespace:** `Togo.Infrastructure.Repositories`
- **Interface implementada:** `Togo.Application.Attendances.Repositories.IAttendanceRepository`
- **Dependências usadas:**
  - `AppDbContext`;
  - `Microsoft.EntityFrameworkCore`;
  - entidade `Attendance`;
  - enum `AttendanceStatus`.
- **Base técnica:** EF Core sobre `AppDbContext`.

### GetByIdAsync

- usa `AsNoTracking`;
- busca por `Id`;
- retorna `Attendance?`.

### ListAsync

- usa `AsNoTracking`;
- ordena por `OpenedAt` desc e `Id` desc.

### ListByPatientIdAsync

- filtra por `PatientId`;
- usa `AsNoTracking`;
- ordena por `OpenedAt` desc e `Id` desc.

### AddAsync

- usa `AddAsync`;
- chama `SaveChangesAsync`.

### UpdateAsync

- usa `Update`;
- chama `SaveChangesAsync`.

### ExistsByAttendanceNumberAsync

- usa `AnyAsync`;
- verifica `AttendanceNumber`.

### HasOpenAttendanceForPatientAsync

- usa `AnyAsync`;
- filtra `PatientId + Status Open`.

## 5. Testes de Infrastructure

- **Projeto criado:** `backend/src/Togo.Infrastructure.Tests`.
- **Provider:** SQLite in-memory.
- **Factory:** `SqliteAppDbContextFactory`.
- **Isolamento:** banco em memória e conexão isolados por teste.
- **Inicialização de schema:** `EnsureCreated`.
- **Integridade referencial:** criação prévia de `Patient` para atender FK de `Attendance`.

Testes criados:

- `AddAsync_ShouldPersistAttendance_WhenAttendanceIsValid`;
- `GetByIdAsync_ShouldReturnAttendance_WhenAttendanceExists`;
- `GetByIdAsync_ShouldReturnNull_WhenAttendanceDoesNotExist`;
- `ListAsync_ShouldReturnAttendancesOrderedByOpenedAtDescendingThenIdDescending`;
- `ListByPatientIdAsync_ShouldReturnOnlyAttendancesForPatient`;
- `ExistsByAttendanceNumberAsync_ShouldReturnTrue_WhenAttendanceNumberExists`;
- `ExistsByAttendanceNumberAsync_ShouldReturnFalse_WhenAttendanceNumberDoesNotExist`;
- `HasOpenAttendanceForPatientAsync_ShouldReturnTrue_WhenPatientHasOpenAttendance`;
- `HasOpenAttendanceForPatientAsync_ShouldReturnFalse_WhenPatientHasNoOpenAttendance`;
- `HasOpenAttendanceForPatientAsync_ShouldReturnFalse_WhenPatientHasOnlyClosedOrCanceledAttendance`;
- `UpdateAsync_ShouldPersistClosedAttendance_WhenAttendanceIsClosed`;
- `UpdateAsync_ShouldPersistCanceledAttendance_WhenAttendanceIsCanceled`.

## 6. Solution integration

- O projeto `Togo.Infrastructure.Tests` foi adicionado ao `backend/Togo.sln` na Fase 4.4.2.1.
- Isso garante que `dotnet build/test` da solution incluam os testes de Infrastructure.
- Caminho do projeto na solution:
  - `src\Togo.Infrastructure.Tests\Togo.Infrastructure.Tests.csproj`.

## 7. Registro no DI

- **Arquivo:** `backend/src/Togo.Api/Program.cs`.
- **Registro:** `IAttendanceRepository -> AttendanceRepository`.
- **Lifetime:** `Scoped`.
- **Padrão seguido:** mesmo padrão de `ITutorRepository` e `IPetRepository`.

## 8. EF/AppDbContext

- `AppDbContext` possui `DbSet<Attendance>`.
- `ApplyConfigurationsFromAssembly` carrega `AttendanceConfiguration`.
- `AttendanceConfiguration` define tabela, chave, campos, relacionamento, índices e conversão de enums.

## 9. Relacionamento Attendance/Patient

- FK em `PatientId`.
- Cardinalidade `Patient 1:N Attendance`.
- Não existe `TutorId` direto em `Attendance`.
- O acesso ao tutor permanece indireto via relação existente.
- `DeleteBehavior.Cascade` configurado.
- Risco funcional: impacto em retenção histórica de prontuário ao excluir paciente.

## 10. Índices e constraints

- índice em `PatientId`;
- índice único em `AttendanceNumber`;
- índice em `OpenedAt`;
- recomendação futura de índice composto `PatientId + Status`;
- recomendação futura de índice composto `PatientId + OpenedAt`.

## 11. Enums e persistência

- `AttendanceStatus` persistido como string;
- `AttendanceType` persistido como string.

Vantagens:
- legibilidade no banco;
- menor risco de interpretação incorreta em evolução de enums.

Riscos:
- maior consumo de armazenamento;
- potencial impacto em índices/performance em volumes elevados.

## 12. Validação local/CI

- CI das PRs da fase passou.
- PR 91 passou CI.
- PR 93 passou CI.
- PR 94 passou CI.
- PR 95 passou CI.
- O ambiente Codex desta fase não possui `dotnet`.
- A validação por `dotnet build/test` deve ser executada localmente quando possível.

## 13. Lacunas conhecidas

- sem teste explícito de violação de unique constraint de `AttendanceNumber`;
- sem teste explícito de violação de FK com `Patient` inexistente;
- sem teste específico de cascade delete `Patient -> Attendance`;
- SQLite in-memory pode se comportar diferente do MySQL real;
- migration não foi criada nesta fase;
- possível necessidade futura de índices compostos;
- revisão futura de `DeleteBehavior.Cascade` conforme política de retenção clínica.

## 14. Estado final da Infrastructure Attendance

Ao final da Fase 4.4 existem:

- repository concreto;
- testes de Infrastructure;
- projeto de testes integrado à solution;
- registro de DI;
- configuração EF validada/documentada;
- riscos e lacunas documentados.

Ainda não existem nesta fase:

- controller/API de Attendance;
- endpoints REST;
- documentação Swagger específica;
- migrations novas;
- database update;
- eventos;
- cache.

## 15. Fora do escopo

- Domain;
- Application;
- API controllers;
- endpoints;
- DTOs de API;
- Swagger;
- migrations;
- database update;
- RabbitMQ;
- Redis;
- Docker;
- Kubernetes;
- Frontend.

## 16. Próxima fase recomendada

**Fase 4.5 — Implementar API de Attendance.**

Sugestão de início:

**Fase 4.5.1 — Criar AttendanceController com endpoints mínimos.**

Objetivo:

Expor os use cases de Attendance pela API REST, iniciando por endpoints mínimos de create/get/list/close/cancel, reutilizando os contracts/Application já implementados e a Infrastructure já registrada no DI.
