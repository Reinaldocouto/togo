# TOGO — Fase 4.3.2: Interface IAttendanceRepository

## 1. Objetivo

Criar o contrato de persistência da camada Application para Attendance, preparando a base para os próximos use cases sem acoplar a aplicação a detalhes de infraestrutura.

## 2. Contexto

- O domínio Attendance foi fechado na Fase 4.2.
- Os contratos de Application de Attendance foram criados na Fase 4.3.1.
- Nesta fase, a camada Application passa a ter a abstração de repositório necessária antes da implementação dos use cases.

## 3. Interface criada

- **Arquivo:** `backend/src/Togo.Application/Attendances/Repositories/IAttendanceRepository.cs`
- **Namespace:** `Togo.Application.Attendances.Repositories`
- **Métodos definidos:**
  - `Task<Attendance?> GetByIdAsync(long id, CancellationToken cancellationToken = default);`
  - `Task<IReadOnlyList<Attendance>> ListAsync(CancellationToken cancellationToken = default);`
  - `Task<IReadOnlyList<Attendance>> ListByPatientIdAsync(long patientId, CancellationToken cancellationToken = default);`
  - `Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default);`
  - `Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default);`
  - `Task<bool> ExistsByAttendanceNumberAsync(string attendanceNumber, CancellationToken cancellationToken = default);`
  - `Task<bool> HasOpenAttendanceForPatientAsync(long patientId, CancellationToken cancellationToken = default);`

## 4. Decisões técnicas

- O repository trabalha com a entidade de domínio `Attendance`, sem retorno de DTOs.
- Não há delete físico nesta fase, pois o ciclo de vida inicial de Attendance é governado por `Close`/`Cancel`.
- A validação de unicidade de `AttendanceNumber` será suportada por `ExistsByAttendanceNumberAsync`.
- A regra de um atendimento `Open` por `Patient` será suportada por `HasOpenAttendanceForPatientAsync`.
- `ListByPatientIdAsync` foi criada nesta fase por utilidade direta para a relação `Patient 1:N Attendance` e alinhamento com necessidades de leitura por agregação em use cases futuros.
- A implementação EF/Core foi deliberadamente postergada para fase futura.

## 5. Fora do escopo

- Implementação concreta do repositório.
- Infrastructure.
- DbContext.
- Queries EF.
- Migrations.
- Banco de dados.
- Use cases.
- Validators.
- Controller.
- Endpoints.
- Delete físico.
- Paginação.
- Projections Patient/Pet/Tutor.
- RabbitMQ.
- Redis.
- Docker.
- Kubernetes.

## 6. Próxima fase recomendada

**Fase 4.3.3 — Criar validators Application de Attendance.**

Objetivo: criar validadores mínimos para existência de Patient, unicidade de AttendanceNumber e bloqueio de atendimento Open por Patient, antes da implementação dos use cases.
