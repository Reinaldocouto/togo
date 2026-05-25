# TOGO — Fase 5.4.1: Repository EF Core de MedicalRecord

## Resumo da Subfase 5.4

Subfase 5.4 — Infrastructure MedicalRecord

Planejamento:
- 5.4.1 — Repository EF Core de MedicalRecord.
- 5.4.2 — Registro de DI/Program.cs para MedicalRecord.
- 5.4.3 — Testes de Infrastructure de MedicalRecord.
- 5.4.4 — Validação EF/AppDbContext/Migration existente.
- 5.4.5 — Documentação final da camada Infrastructure.
- 5.4.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Implementar o repositório concreto de `IMedicalRecordRepository` na camada Infrastructure com EF Core e `AppDbContext`, para permitir persistência real dos use cases da vertical de prontuário sem expor API/controller nesta etapa.

## Contexto

- Domain MedicalRecord foi encerrado na subfase 5.2.
- Application MedicalRecord foi encerrada na subfase 5.3.
- A interface `IMedicalRecordRepository` já existe na Application.
- `AppDbContext` já possui `DbSet<MedicalRecord>` confirmado.
- `MedicalRecordConfiguration` já existe e mapeia `MedicalRecords`.
- A migration de Clinical Core já contempla a estrutura de `MedicalRecords` (sem nova migration nesta fase).
- Esta fase não cria migration.
- Esta fase não registra DI.
- Esta fase não implementa API.

## Repository criado

- Caminho: `backend/src/Togo.Infrastructure/Repositories/MedicalRecordRepository.cs`.
- Namespace: `Togo.Infrastructure.Repositories`.
- Dependência: `AppDbContext`.
- Interface implementada: `IMedicalRecordRepository`.

Métodos implementados:

1. `GetByIdAsync(long id)`
   - leitura com `AsNoTracking`;
   - filtro por `Id`;
   - retorno `MedicalRecord?` (`null` quando não encontrado);
   - sem `SaveChangesAsync`.

2. `GetByPatientIdAsync(long patientId)`
   - leitura com `AsNoTracking`;
   - filtro por `PatientId`;
   - retorno `MedicalRecord?` (`null` quando não encontrado);
   - sem `SaveChangesAsync`.

3. `ExistsByPatientIdAsync(long patientId)`
   - verificação com `AnyAsync`;
   - retorno `bool`;
   - sem `SaveChangesAsync`.

4. `AddAsync(MedicalRecord medicalRecord)`
   - adiciona em `MedicalRecords`;
   - chama `SaveChangesAsync`;
   - não usa `AsNoTracking`.

5. `UpdateAsync(MedicalRecord medicalRecord)`
   - atualiza entidade em `MedicalRecords`;
   - chama `SaveChangesAsync`;
   - sem criar nova entidade;
   - sem Soft Delete.

## Decisões técnicas

- Leituras usam `AsNoTracking`.
- `ExistsByPatientIdAsync` usa `AnyAsync`.
- `AddAsync` chama `SaveChangesAsync`.
- `UpdateAsync` chama `SaveChangesAsync`.
- O repository não expõe `IQueryable`.
- O repository não expõe `DbSet`.
- O repository não implementa `DeleteAsync`.
- O repository não implementa `ListAsync`.
- Esta fase não altera migrations.
- O repository não resolve sozinho regra de unicidade de domínio.
- A unicidade física por `PatientId` permanece para fase futura de migration/validação.
- `CancellationToken` ainda não é propagado pela interface e permanece como débito técnico.

## Segurança e privacidade

- O repository não realiza logging de `GeneralNotes`/`FlagsJson`.
- O repository não deve expor dados clínicos fora dos métodos definidos em contrato.
- Leituras retornam dados somente quando requisitadas pelos use cases.
- Reforços de autorização/acesso por roles ficam para fases futuras de API/segurança.

## Pontos de atenção

- O repository concreto ainda não está registrado em DI.
- Testes de Infrastructure ficam para 5.4.3.
- Registro em Program.cs/DI fica para 5.4.2.
- Unicidade por `PatientId` ainda não está como constraint física no banco.
- `DeleteBehavior.Cascade` ainda demanda revisão futura.
- Soft Delete e AuditLog continuam fora do escopo.
- `CancellationToken` ainda não está na interface.

## Critérios de aceite

A fase será considerada concluída se:

- `MedicalRecordRepository.cs` for criado;
- implementar `IMedicalRecordRepository`;
- `GetByIdAsync` usar `AsNoTracking`;
- `GetByPatientIdAsync` usar `AsNoTracking`;
- `ExistsByPatientIdAsync` usar `AnyAsync`;
- `AddAsync` chamar `SaveChangesAsync`;
- `UpdateAsync` chamar `SaveChangesAsync`;
- não houver `DeleteAsync`;
- não houver `ListAsync`;
- não houver migration;
- `AppDbContext` não for alterado sem necessidade;
- `Program.cs` não for alterado;
- API não for alterada;
- documentação da fase for criada;
- `git diff --check` não apontar problemas;
- `dotnet build backend/Togo.sln` passar;
- `dotnet test backend/Togo.sln` passar.

## Fora do escopo

Esta fase não implementa:

- DI/Program.cs;
- controller;
- API;
- testes de Infrastructure;
- migration;
- database update;
- Soft Delete;
- AuditLog;
- roles/permissões;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## Próxima fase recomendada

**Fase 5.4.2 — Registro de DI/Program.cs para MedicalRecord.**

Objetivo:
Registrar `MedicalRecordRepository`, validators e use cases necessários para resolver a vertical MedicalRecord em runtime, sem implementar controller/API ainda.

## Validações obrigatórias

Comandos executados nesta fase:

- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`
