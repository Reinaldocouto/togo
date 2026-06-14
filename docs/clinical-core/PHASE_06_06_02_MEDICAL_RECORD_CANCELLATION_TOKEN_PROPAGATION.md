# Fase 6.6.2 — Propagação de CancellationToken no repository MedicalRecord

## 1. Objetivo

Propagar `CancellationToken` até o repository da vertical `MedicalRecord`, garantindo que operações assíncronas de leitura e escrita consigam observar cancelamentos recebidos pelos fluxos HTTP/Application.

## 2. Contexto da Fase 6.6

A Fase 6.6 trata qualidade operacional e evidências finais da vertical `MedicalRecord`. A Fase 6.6.1 documentou o planejamento operacional e identificou como próxima ação técnica a correção do débito de cancelamento no repository.

## 3. Referência ao MR-DEBT-010

Esta fase trata explicitamente o débito `MR-DEBT-010 — CancellationToken não propagado no repository`.

## 4. Estado anterior

Antes desta fase, controllers e use cases já recebiam `CancellationToken`, mas `IMedicalRecordRepository` expunha métodos sem token. A implementação `MedicalRecordRepository` chamava `FirstOrDefaultAsync`, `AnyAsync`, `AddAsync` e `SaveChangesAsync` sem token, e os fakes/testes implementavam as assinaturas antigas.

## 5. Decisão técnica

A decisão foi alterar as assinaturas de todos os métodos assíncronos relevantes de `IMedicalRecordRepository` para exigir `CancellationToken`, sem manter overloads antigos. Assim, todos os callers são forçados a repassar explicitamente o token recebido ou um token de teste.

## 6. Arquivos alterados

- `backend/src/Togo.Application/MedicalRecords/Repositories/IMedicalRecordRepository.cs`
- `backend/src/Togo.Infrastructure/Repositories/MedicalRecordRepository.cs`
- `backend/src/Togo.Application/MedicalRecords/UseCases/*MedicalRecord*UseCase.cs`
- `backend/src/Togo.Application/MedicalRecords/Validators/MedicalRecordExistsValidator.cs`
- `backend/src/Togo.Application/MedicalRecords/Validators/MedicalRecordUniquenessValidator.cs`
- `backend/src/Togo.Application.Tests/MedicalRecords/Fakes/FakeMedicalRecordRepository.cs`
- Testes de Application, API e Infrastructure impactados pelas novas assinaturas
- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`

## 7. Métodos do repository atualizados

- `GetByIdAsync(long id, CancellationToken cancellationToken)`
- `GetByPatientIdAsync(long patientId, CancellationToken cancellationToken)`
- `ExistsByPatientIdAsync(long patientId, CancellationToken cancellationToken)`
- `ExistsIncludingSoftDeletedByPatientIdAsync(long patientId, CancellationToken cancellationToken)`
- `AddAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken)`
- `UpdateAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken)`

## 8. Como o token é propagado

Os use cases e validators repassam o token recebido para o repository. O repository repassa o mesmo token para operações assíncronas do EF Core: `FirstOrDefaultAsync`, `AnyAsync`, `DbSet.AddAsync` e `SaveChangesAsync`.

## 9. Impacto em use cases

`CreateMedicalRecordUseCase`, `GetMedicalRecordByPatientIdUseCase`, `UpdateMedicalRecordUseCase` e `SoftDeleteMedicalRecordUseCase` passaram a preservar o token recebido nas chamadas ao repository. Não houve alteração em validação, payload, autorização, auditoria ou semântica clínica.

## 10. Impacto em validators

`MedicalRecordExistsValidator` e `MedicalRecordUniquenessValidator` passaram a repassar o token recebido para as consultas de existência no repository.

## 11. Impacto em fakes/testes

O fake principal `FakeMedicalRecordRepository` foi atualizado para as novas assinaturas e passou a capturar tokens recebidos em caminhos principais. Testes de use cases e validators cobrem o repasse do token, e testes existentes foram ajustados para chamadas explícitas com token.

## 12. Impacto em Infrastructure/EF Core

`MedicalRecordRepository` passa a entregar o token aos métodos assíncronos do EF Core. O tratamento específico de `DbUpdateException` para a constraint `IX_MedicalRecords_PatientId` foi preservado. `OperationCanceledException` não é convertida em conflito, validation error ou not found.

## 13. Ausência de mudança de regra de negócio

A fase não altera regra de negócio, autorização, contratos HTTP, validações clínicas, soft delete, autoria, auditoria clínica ou unicidade física.

## 14. Ausência de migration/schema

A fase não altera `AppDbContext`, configurações EF, migrations ou schema de banco de dados.

## 15. Riscos remanescentes

O cancelamento efetivo ainda depende do provider EF Core, do banco utilizado e do momento exato da operação. Outras verticais podem manter repositories sem propagação equivalente de token.

## 16. Fora do escopo

Ficaram fora do escopo evidências Swagger, pacote de evidências manuais, decisão sobre `MedicalRecordListItemResponse`, endpoints novos, migrations, schema, frontend, Docker, Redis, RabbitMQ e Kubernetes.

## 17. Critérios de aceite

A fase é aceita quando interface, implementação, callers, validators, fakes e testes usam `CancellationToken`; EF Core recebe o token em leituras e persistência; não há alteração de regra de negócio, schema ou migration; build/testes planejados são executados ou documentados conforme limitação do ambiente.

## 18. Decisão final

`MR-DEBT-010` fica resolvido tecnicamente na vertical `MedicalRecord` por propagação explícita de `CancellationToken` do fluxo Application até o repository/EF Core.

## 19. Próxima fase recomendada

6.6.3 — Evidências manuais versionadas de API/Swagger MedicalRecord.
