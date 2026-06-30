# Fase 8.3.3.2 — ClinicId em ClinicalEvolution

## Objetivo

Adicionar `ClinicId` obrigatório em `ClinicalEvolution`, derivando o escopo clínico de `Attendance.ClinicId` no momento da criação da evolução clínica.

Esta subfase continua a propagação incremental planejada na Fase 8.3.0 e mantém fora do escopo autorização contextual completa, `CurrentClinicalContext`, `UserClinicAccess`, filtros globais por contexto, `ClinicUnitId` e front-end.

## Escopo implementado

- `ClinicalEvolution` passou a possuir `ClinicId` obrigatório no domínio.
- `ClinicalEvolution.Create(...)` passou a exigir `clinicId` como primeiro argumento.
- O fluxo de criação de evolução clínica deriva `ClinicId` exclusivamente da `Attendance` carregada pelo `attendanceId`.
- `CreateClinicalEvolutionRequest` permanece sem `ClinicId`.
- `ClinicalEvolutionResponse` passou a expor `ClinicId` para rastreabilidade do contrato de criação.
- O metadata de auditoria de `ClinicalEvolution.Created` passou a incluir `ClinicId`, mantendo `AttendanceId` e `Type`.
- A configuração EF Core passou a mapear `ClinicId`, FK para `Clinics.Id`, `DeleteBehavior.Restrict` e índices de apoio.
- Foi criada migration para backfill de `ClinicalEvolutions.ClinicId` a partir de `Attendances.ClinicId`.

## Arquivos alterados

- `backend/src/Togo.Domain/Entities/ClinicalEvolution.cs`
- `backend/src/Togo.Domain.Tests/ClinicalEvolutionTests.cs`
- `backend/src/Togo.Application/ClinicalEvolutions/Contracts/ClinicalEvolutionResponse.cs`
- `backend/src/Togo.Application/ClinicalEvolutions/UseCases/CreateClinicalEvolutionUseCase.cs`
- `backend/src/Togo.Application.Tests/ClinicalEvolutions/CreateClinicalEvolutionUseCaseTests.cs`
- `backend/src/Togo.Application.Tests/ClinicalEvolutions/ListClinicalEvolutionsByAttendanceUseCaseTests.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalEvolutionConfiguration.cs`
- `backend/src/Togo.Infrastructure/Migrations/20260630140000_AddClinicalEvolutionClinicId.cs`
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `backend/src/Togo.Infrastructure.Tests/Persistence/ClinicalCascadeDeleteBehaviorTests.cs`
- `backend/src/Togo.Infrastructure.Tests/Repositories/ClinicalEvolutionRepositoryTests.cs`
- `backend/src/Togo.Api.Tests/Controllers/ClinicalEvolutionsControllerTests.cs`
- `docs/security/PHASE_08_03_03_02_CLINICAL_EVOLUTION_CLINIC_ID.md`

## Domain

`ClinicalEvolution` agora possui `ClinicId` com setter privado. A factory pública passou de:

```csharp
ClinicalEvolution.Create(attendanceId, registeredAt, type, text, createdByUserId, createdAt)
```

para:

```csharp
ClinicalEvolution.Create(clinicId, attendanceId, registeredAt, type, text, createdByUserId, createdAt)
```

A validação existente de identificadores foi reaproveitada para rejeitar `ClinicId <= 0`. `UpdateText` continua alterando apenas texto e autoria técnica de atualização; `ClinicId` não é mutável por este método.

## Application e DTOs de API

`CreateClinicalEvolutionUseCase` continua carregando a `Attendance` por `attendanceId`, validando existência e exigindo `AttendanceStatus.Open`.

A decisão desta fase é que o request não deve aceitar `ClinicId`: `CreateClinicalEvolutionRequest` permanece restrito a `AttendanceId`, `RegisteredAt`, `Type` e `Text`. O escopo clínico da evolução é sempre derivado de `attendance.ClinicId` depois que a `Attendance` é encontrada.

`ClinicalEvolutionResponse` passou a expor `ClinicId`, seguindo o padrão recente de rastreabilidade adotado para respostas de entidades clínicas como `AttendanceResponse` e `MedicalRecordResponse`. Esta exposição não representa autorização contextual e não filtra dados por clínica.

## Auditoria

O evento `ClinicalEvolution.Created` mantém a estrutura existente de `ClinicalAuditLog` e adiciona `ClinicId` no JSON de metadata, junto com `AttendanceId` e `Type`.

Não foi implementada auditoria contextual completa, auditoria de leitura ou evento de acesso negado.

## Infrastructure

`ClinicalEvolutionConfiguration` agora configura:

- `ClinicId` obrigatório;
- FK `ClinicalEvolutions.ClinicId -> Clinics.Id`;
- `DeleteBehavior.Restrict` para a FK de clínica;
- FK existente para `Attendance` preservada com `DeleteBehavior.Restrict`;
- índice simples `IX_ClinicalEvolutions_ClinicId`;
- índice composto `IX_ClinicalEvolutions_ClinicId_AttendanceId`;
- índice composto `IX_ClinicalEvolutions_ClinicId_RegisteredAt`;
- índices existentes de `AttendanceId` e `RegisteredAt` preservados.

Os índices compostos foram criados porque a evolução clínica tende a ser consultada por atendimento dentro de uma clínica e por linhas do tempo clínicas ordenadas por data dentro do escopo clínico.

## Migration criada

Migration: `20260630140000_AddClinicalEvolutionClinicId`.

### Estratégia de backfill

1. Adiciona `ClinicalEvolutions.ClinicId` como nullable.
2. Atualiza `ClinicalEvolutions.ClinicId` a partir de `Attendances.ClinicId` via `AttendanceId`.
3. Altera a coluna para non-nullable.
4. Cria índices simples e compostos.
5. Cria FK para `Clinics.Id` com `Restrict`.

A migration não cria clínica de compatibilidade. Se existir evolução clínica órfã ou vinculada a atendimento sem `ClinicId`, a alteração para non-nullable deve falhar intencionalmente para impedir preenchimento arbitrário de escopo.

## Testes criados/atualizados

- Testes de domínio validam criação com `ClinicId`, rejeição de `ClinicId` zero/negativo e preservação de `ClinicId` em `UpdateText`.
- Testes de aplicação validam derivação de `ClinicId` da `Attendance`, ausência de `ClinicId` no request, presença de `ClinicId` no response e metadata de auditoria.
- Testes de infraestrutura validam persistência, propriedade obrigatória, FK, `DeleteBehavior.Restrict` e índices.
- Testes de API foram ajustados para o novo contrato de response.

## Riscos remanescentes

- A migration depende da integridade prévia entre `ClinicalEvolutions.AttendanceId` e `Attendances.Id`.
- Não há filtros globais por clínica; consumidores ainda podem precisar de controles superiores enquanto autorização contextual não existir.
- O response expõe `ClinicId` apenas para rastreabilidade, sem garantia de autorização contextual.

## Fora do escopo desta fase

Esta fase não implementa:

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

Fase 8.3.3.3 — `ClinicId` em `Prescription`, derivado de `Attendance.ClinicId`, mantendo a mesma abordagem incremental e sem autorização contextual completa.
