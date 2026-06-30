# Fase 8.3.3.3 — ClinicId em Prescription

## Objetivo
Adicionar `ClinicId` obrigatório em `Prescription`, derivando o escopo clínico de `Attendance.ClinicId` no fluxo de criação de prescrição, sem introduzir autorização contextual completa ou `CurrentClinicalContext`.

## Escopo implementado
- `Prescription` passou a possuir `ClinicId` obrigatório no domínio.
- `Prescription.Create(...)` passou a exigir `clinicId` antes de `attendanceId`.
- O fluxo de criação de prescrição deriva `ClinicId` exclusivamente da `Attendance` carregada.
- `CreatePrescriptionRequest` continua sem `ClinicId`.
- `PrescriptionResponse` interno da aplicação expõe `ClinicId` para rastreabilidade.
- O metadata de auditoria de `Prescription.Created` inclui `ClinicId` e mantém `AttendanceId`.
- EF Core mapeia `Prescriptions.ClinicId` como obrigatório, com FK para `Clinics.Id`, `DeleteBehavior.Restrict` e índices clínicos.
- Foi criada migration para backfill de `Prescriptions.ClinicId` a partir de `Attendances.ClinicId`.
- `PrescriptionItem` não recebeu `ClinicId` direto.

## Arquivos alterados
- `backend/src/Togo.Domain/Entities/Prescription.cs`
- `backend/src/Togo.Domain.Tests/PrescriptionTests.cs`
- `backend/src/Togo.Application/Prescriptions/UseCases/CreatePrescriptionUseCase.cs`
- `backend/src/Togo.Application/Prescriptions/Contracts/PrescriptionResponse.cs`
- `backend/src/Togo.Application.Tests/Prescriptions/CreatePrescriptionUseCaseTests.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/PrescriptionConfiguration.cs`
- `backend/src/Togo.Infrastructure/Migrations/20260630150000_AddPrescriptionClinicId.cs`
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- Testes de infraestrutura/API impactados por alteração da factory.

## Domain
`Prescription` agora valida `ClinicId > 0`, mantém `ClinicId` sem setter público e preserva as validações existentes de `AttendanceId` e `IssuedAt`. `UpdateNotes` altera somente notas e preserva o escopo clínico.

## Application / API DTOs
`CreatePrescriptionUseCase` continua validando a existência e o status aberto da `Attendance`. Após carregar a `Attendance`, o use case passa `attendance.ClinicId` para `Prescription.Create(...)`.

`CreatePrescriptionRequest` não aceita `ClinicId`, evitando que o cliente escolha livremente o escopo da prescrição.

`PrescriptionResponse` interno expõe `ClinicId` para rastreabilidade, seguindo o padrão de respostas internas clínicas recentes. Isso não representa autorização contextual.

O contrato público endurecido de criação no controller continua retornando apenas os dados mínimos já definidos para o endpoint público.

## Auditoria
O evento `Prescription.Created` passa a registrar metadata JSON com `ClinicId` e `AttendanceId`. A estrutura de `ClinicalAuditLog` não foi alterada, e a fase não implementa auditoria contextual completa, auditoria de leitura ou acesso negado.

## Infrastructure
`PrescriptionConfiguration` mapeia:
- `ClinicId` obrigatório;
- FK `Prescriptions.ClinicId -> Clinics.Id`;
- `DeleteBehavior.Restrict`;
- índice simples `IX_Prescriptions_ClinicId`;
- índices compostos `IX_Prescriptions_ClinicId_AttendanceId` e `IX_Prescriptions_ClinicId_IssuedAt`;
- FKs e índices existentes de `AttendanceId` e `IssuedAt` preservados.

`PrescriptionItemConfiguration` não recebeu `ClinicId`.

## Migration criada
Migration: `20260630150000_AddPrescriptionClinicId`.

### Estratégia de backfill
A migration adiciona `ClinicId` inicialmente nullable, executa backfill com `UPDATE Prescriptions p INNER JOIN Attendances a ON a.Id = p.AttendanceId SET p.ClinicId = a.ClinicId`, e depois torna a coluna obrigatória.

Se houver prescrição órfã ou atendimento sem `ClinicId`, o `ALTER COLUMN` para não nulo falha intencionalmente. Não foi criada clínica de compatibilidade.

## Decisões
- `ClinicId` não é aceito no request; a fonte de verdade é `Attendance.ClinicId`.
- `PrescriptionResponse` interno expõe `ClinicId` para rastreabilidade antes da autorização contextual.
- Audit metadata inclui `ClinicId` por ser simples e útil para rastreio.
- `PrescriptionItem` não recebeu `ClinicId` porque herda o escopo pela prescrição.
- Índices compostos `ClinicId + AttendanceId` e `ClinicId + IssuedAt` foram criados para consultas futuras por escopo clínico.

## Testes criados/atualizados
- Testes de domínio para criação válida, `ClinicId` zero/negativo, validações existentes e preservação de `ClinicId` em `UpdateNotes`.
- Testes de aplicação para derivação de `ClinicId`, response, request sem `ClinicId`, `PrescriptionItemResponse` sem `ClinicId` e audit metadata.
- Testes de infraestrutura para persistência, FK, `DeleteBehavior.Restrict`, índices e ausência de `ClinicId` em `PrescriptionItem`.
- Testes existentes de API/infra foram ajustados para a nova assinatura de `Prescription.Create(...)`.

## Riscos remanescentes
- As migrations com `UPDATE ... INNER JOIN` são orientadas a MySQL e devem ser revisadas caso outro provedor seja usado.
- Avaliar criação futura de `PatientScopeRepository` ou `ClinicalScopeReader`.
- Padronizar projections de escopo clínico.
- Avaliar validação amigável de existência de `Clinic`.
- Revisar respostas que expõem `ClinicId` antes da autorização contextual.
- Revisar estratégia de backfill antes de produção.
- A revisão final deve consolidar débitos em `docs/security/PHASE_08_TECHNICAL_DEBT.md`.

## Fora do escopo
Esta fase não implementa:
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
Fase 8.3.3.4 — Revisão final da propagação clínica e débitos técnicos.
