# Fase 8.3.3.1 — ClinicId em MedicalRecord

## Objetivo

Adicionar `ClinicId` obrigatório em `MedicalRecord`, derivando o escopo clínico a partir de `Patient.ClinicId` no momento de criação do prontuário, sem introduzir autorização contextual completa e sem criar `CurrentClinicalContext`.

## Escopo implementado

- `MedicalRecord` passou a possuir `ClinicId` obrigatório no domínio.
- `MedicalRecord.Create(...)` passou a exigir `clinicId` antes de `patientId`.
- O fluxo de criação de prontuário deriva `ClinicId` do paciente validado.
- `CreateMedicalRecordRequest` permaneceu sem `ClinicId`.
- `MedicalRecordResponse` passou a expor `ClinicId` para rastreabilidade, seguindo o padrão adotado em `AttendanceResponse`.
- O audit metadata de `MedicalRecord.Created` passou a incluir `ClinicId` junto de `PatientId`.
- A configuração EF Core de `MedicalRecord` passou a mapear `ClinicId`, FK para `Clinics`, índices e delete behavior restritivo.
- Foi criada migration específica para adicionar e popular `MedicalRecords.ClinicId`.

## Arquivos alterados

- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`
- `backend/src/Togo.Domain.Tests/MedicalRecordTests.cs`
- `backend/src/Togo.Application/MedicalRecords/Contracts/MedicalRecordPatientScope.cs`
- `backend/src/Togo.Application/MedicalRecords/Contracts/MedicalRecordResponse.cs`
- `backend/src/Togo.Application/MedicalRecords/Validators/MedicalRecordPatientExistsValidator.cs`
- `backend/src/Togo.Application/MedicalRecords/UseCases/CreateMedicalRecordUseCase.cs`
- `backend/src/Togo.Application/MedicalRecords/UseCases/GetMedicalRecordByPatientIdUseCase.cs`
- `backend/src/Togo.Application/MedicalRecords/UseCases/UpdateMedicalRecordUseCase.cs`
- `backend/src/Togo.Application/MedicalRecords/UseCases/SoftDeleteMedicalRecordUseCase.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`
- `backend/src/Togo.Infrastructure/Migrations/20260630130000_AddMedicalRecordClinicId.cs`
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- Testes de aplicação, infraestrutura e API impactados pelo novo contrato.

## Domain

`MedicalRecord` agora possui `ClinicId` com setter privado. A factory passou de:

```csharp
MedicalRecord.Create(patientId, generalNotes, flagsJson, createdByUserId, createdAt)
```

para:

```csharp
MedicalRecord.Create(clinicId, patientId, generalNotes, flagsJson, createdByUserId, createdAt)
```

A entidade valida `clinicId > 0` com a mesma regra de identificadores positivos já usada por `PatientId`. `UpdateNotes` e `SoftDelete` não recebem nem alteram `ClinicId`.

## Application e DTOs

`MedicalRecordPatientExistsValidator` passou a retornar a projection pequena `MedicalRecordPatientScope(long PatientId, long ClinicId)`. O `CreateMedicalRecordUseCase` usa essa projection para chamar `MedicalRecord.Create(...)`, garantindo que o escopo venha de `Patient.ClinicId` e não do request público.

`CreateMedicalRecordRequest` não aceita `ClinicId` e continua contendo apenas os campos clínicos do prontuário.

`MedicalRecordResponse` expõe `ClinicId` para rastreabilidade operacional. Esta exposição não representa autorização contextual, filtro por contexto ou isolamento de acesso por clínica.

## Auditoria

O metadata JSON de `MedicalRecord.Created` passou a incluir:

- `ClinicId`
- `PatientId`

Não houve alteração na estrutura de `ClinicalAuditLog`, auditoria contextual completa, auditoria de leitura ou registro de acesso negado.

## Infrastructure

`MedicalRecordConfiguration` agora mapeia:

- `ClinicId` obrigatório;
- FK `MedicalRecords.ClinicId -> Clinics.Id`;
- `DeleteBehavior.Restrict` para a FK de clínica;
- índice `IX_MedicalRecords_ClinicId`;
- índice composto `IX_MedicalRecords_ClinicId_PatientId`;
- índice único existente `IX_MedicalRecords_PatientId`, preservando a regra 1:1 entre paciente e prontuário.

## Migration criada

Migration: `20260630130000_AddMedicalRecordClinicId`.

### Estratégia de backfill

A migration adiciona `MedicalRecords.ClinicId` inicialmente nullable, popula o valor via join com `Patients` usando `MedicalRecords.PatientId`, e então altera a coluna para obrigatória.

Se existir prontuário órfão ou paciente sem `ClinicId`, o `ALTER COLUMN` para não nulo falhará intencionalmente. Não foi criada clínica de compatibilidade nesta migration porque `Patient.ClinicId` já é obrigatório desde a Fase 8.3.1.

## Testes criados/atualizados

- Testes de domínio para `ClinicId` válido, inválido, preservação em `UpdateNotes` e preservação em `SoftDelete`.
- Testes de aplicação para derivação de `ClinicId` do paciente, response com `ClinicId` e audit metadata com `ClinicId`.
- Testes de validator para retorno de `MedicalRecordPatientScope`.
- Testes de infraestrutura para persistência de `ClinicId`, FK restritiva, índice simples e índice composto.
- Testes de API/guardrail atualizados para o novo payload público de `MedicalRecordResponse`.

## Riscos remanescentes

- Registros históricos inconsistentes, como prontuários órfãos, farão a migration falhar conforme esperado e precisarão de correção de dados antes da aplicação.
- Ainda não há autorização contextual por clínica; `ClinicId` é persistido e exposto, mas não é usado para bloquear acesso.
- Ainda não há filtros globais por contexto clínico.

## Fora do escopo nesta fase

Esta fase não implementa:

- `ClinicId` em `ClinicalEvolution`;
- `ClinicId` em `Prescription`;
- `ClinicId` em `PrescriptionItem`;
- `ClinicId` em `Pet`;
- `CurrentClinicalContext`;
- `UserClinicAccess`;
- autorização contextual;
- filtros por contexto;
- auditoria contextual completa;
- auditoria de leitura;
- acesso negado;
- `ClinicUnitId`;
- front-end.

## Próxima fase recomendada

Fase 8.3.3.2 — `ClinicId` em `ClinicalEvolution`.
