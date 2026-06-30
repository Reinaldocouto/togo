# TOGO — Fase 8.3.2: ClinicId em Attendance

## Objetivo

Adicionar `ClinicId` obrigatório em `Attendance`, persistindo o escopo clínico do atendimento a partir do `Patient.ClinicId` existente no momento da criação.

Esta fase não implementa autorização contextual completa. O `ClinicId` gravado em `Attendance` é uma fotografia persistida do escopo clínico do paciente no momento da abertura do atendimento.

## Escopo implementado

- `Attendance` passou a possuir `ClinicId` obrigatório no domínio.
- `Attendance.Create(...)` passou a exigir `clinicId` como primeiro argumento.
- `CreateAttendanceUseCase` deriva o `ClinicId` do paciente validado via `AttendancePatientExistsValidator`.
- `CreateAttendanceRequest` permanece sem `ClinicId` e não é fonte de verdade para escopo clínico.
- `AttendanceResponse` passou a expor `ClinicId` para rastreabilidade do escopo persistido.
- O audit metadata mínimo de criação de atendimento passou a incluir `ClinicId`.
- EF Core mapeia `Attendances.ClinicId` como obrigatório, com FK para `Clinics.Id`, delete behavior `Restrict` e índices mínimos.
- Foi criada migration revisável para backfill de `Attendances.ClinicId` a partir de `Patients.ClinicId`.

## Arquivos alterados

- `backend/src/Togo.Domain/Entities/Attendance.cs`
- `backend/src/Togo.Domain.Tests/AttendanceTests.cs`
- `backend/src/Togo.Application/Attendances/Contracts/AttendancePatientScope.cs`
- `backend/src/Togo.Application/Attendances/Contracts/AttendanceResponse.cs`
- `backend/src/Togo.Application/Attendances/Validators/AttendancePatientExistsValidator.cs`
- `backend/src/Togo.Application/Attendances/UseCases/CreateAttendanceUseCase.cs`
- `backend/src/Togo.Application/Attendances/UseCases/GetAttendanceByIdUseCase.cs`
- `backend/src/Togo.Application/Attendances/UseCases/CloseAttendanceUseCase.cs`
- `backend/src/Togo.Application/Attendances/UseCases/CancelAttendanceUseCase.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/AttendanceConfiguration.cs`
- `backend/src/Togo.Infrastructure/Migrations/20260630120000_AddAttendanceClinicId.cs`
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- Testes impactados de Application, Infrastructure e API por mudança de assinatura de `Attendance.Create(...)` e contrato de response.

## Mudanças em Domain

`Attendance` agora possui `ClinicId` com setter privado. A factory ficou:

```csharp
Attendance.Create(clinicId, patientId, attendanceNumber, openedAt, type, createdByUserId, createdAtUtc)
```

A validação usa a mesma regra de identificadores obrigatórios do agregado: `ClinicId` deve ser maior que zero. As operações `Close` e `Cancel` não recebem nem alteram `ClinicId`.

## Mudanças em Application/API DTOs

`CreateAttendanceRequest` não foi alterado e continua recebendo apenas `PatientId`, `AttendanceNumber`, `OpenedAt` e `Type`.

`AttendancePatientExistsValidator` continua responsável por validar a existência do paciente, mas agora retorna uma projection pequena, `AttendancePatientScope`, contendo `PatientId` e `ClinicId`. Essa decisão evita aceitar `ClinicId` livre no payload e mantém o validator simples.

`AttendanceResponse` expõe `ClinicId` para facilitar rastreabilidade, testes e consistência com o escopo persistido. Essa exposição não representa autorização contextual.

## Auditoria

O metadata JSON de `Attendance.Created` foi enriquecido com `ClinicId`, além de `PatientId` e `Status`. A estrutura de `ClinicalAuditLog` não foi alterada. Isso é apenas enriquecimento mínimo e não substitui a Fase 8.7.

## Mudanças em Infrastructure

`AttendanceConfiguration` passou a mapear:

- `ClinicId` obrigatório;
- FK `Attendances.ClinicId -> Clinics.Id`;
- `DeleteBehavior.Restrict`;
- índice simples em `ClinicId`;
- índices compostos `(ClinicId, OpenedAt)` e `(ClinicId, Status)` para consultas futuras;
- FK existente para `Patient` preservada;
- unicidade global de `AttendanceNumber` preservada.

## Migration criada

Migration: `20260630120000_AddAttendanceClinicId`.

Estratégia:

1. adiciona `Attendances.ClinicId` como nullable;
2. preenche `Attendances.ClinicId` com `Patients.ClinicId` por join em `PatientId`;
3. falha explicitamente se algum atendimento não puder ser preenchido;
4. altera `ClinicId` para obrigatório;
5. cria índices;
6. cria FK para `Clinics.Id` com `Restrict`.

A migration não cria clínica de compatibilidade, pois `Patient.ClinicId` já é obrigatório desde a Fase 8.3.1.

## Testes criados/atualizados

- Testes de domínio para criação válida com `ClinicId`, rejeição de `ClinicId` zero/negativo e preservação de `ClinicId` em `Close` e `Cancel`.
- Testes de Application para derivar `ClinicId` de `Patient.ClinicId`, manter `CreateAttendanceRequest` sem `ClinicId` e incluir `ClinicId` no metadata de criação.
- Testes impactados atualizados para a nova assinatura de `Attendance.Create(...)`.

## Riscos remanescentes

- Listagens e consultas continuam sem filtro contextual por clínica.
- A presença de `ClinicId` em `Attendance` ainda não garante que o usuário autenticado possui acesso à clínica.
- Bancos com atendimentos órfãos ou pacientes inconsistentes falharão a migration, como decisão intencional de segurança de dados.

## Fora do escopo desta fase

Esta fase não implementa:

- `ClinicId` em `MedicalRecord`;
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
- auditoria de acesso negado;
- `ClinicUnitId`;
- front-end.

## Próxima fase recomendada

Fase 8.3.3 — `ClinicId` em `MedicalRecord`, `ClinicalEvolution` e `Prescription`.
