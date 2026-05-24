# TOGO — Fase 5.3.1: Contracts de MedicalRecord

## Resumo da Subfase 5.3

Subfase 5.3 — Application MedicalRecord

Planejamento:
- 5.3.1 — Contracts de MedicalRecord.
- 5.3.2 — Interface IMedicalRecordRepository.
- 5.3.3 — Validators de MedicalRecord.
- 5.3.4 — Use cases de MedicalRecord.
- 5.3.5 — Testes de Application.
- 5.3.6 — Documentação final da camada Application.
- 5.3.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Iniciar a camada Application de MedicalRecord com contratos de entrada e saída para os futuros use cases e endpoints, preservando separação de responsabilidades entre domínio, aplicação e API.

## Contexto

- A subfase 5.2 (Domain MedicalRecord) foi oficialmente encerrada.
- `MedicalRecord` já possui entidade de domínio validada por testes.
- A próxima evolução segura é criar contratos da camada Application.
- Os endpoints planejados são orientados por `patientId` em rota.
- Prontuário não é atendimento.
- `PatientId` não deve ser enviado no body das requests.
- Dados clínicos continuam sensíveis.

## Contracts criados

### 1) `CreateMedicalRecordRequest`

Finalidade:
- Representar entrada de criação do prontuário no fluxo da Application.

Campos:
- `string? GeneralNotes`
- `string? FlagsJson`

Por que existe:
- Permite receber dados iniciais do prontuário sem acoplar contrato à entidade de domínio.

O que ficou fora:
- `PatientId` (virá pela rota: `POST /api/patients/{patientId}/medical-record`)
- `AttendanceId`, `TutorId`, `CreatedAt`, Soft Delete e AuditLog.

### 2) `UpdateMedicalRecordRequest`

Finalidade:
- Representar entrada de atualização do prontuário no fluxo da Application.

Campos:
- `string? GeneralNotes`
- `string? FlagsJson`

Por que existe:
- Permite atualização de conteúdo clínico resumido do prontuário sem lógica de negócio no contrato.

O que ficou fora:
- `PatientId` (virá pela rota: `PUT /api/patients/{patientId}/medical-record`)
- `AttendanceId`, `TutorId`, `CreatedAt`, Soft Delete e AuditLog.

### 3) `MedicalRecordResponse`

Finalidade:
- Representar retorno detalhado do prontuário em consultas/operações de leitura direta.

Campos:
- `long Id`
- `long PatientId`
- `string? GeneralNotes`
- `string? FlagsJson`
- `DateTime UpdatedAt`

Por que existe:
- Entrega o estado detalhado esperado do prontuário para fluxos autorizados.

O que ficou fora:
- `CreatedAt` (inexistente na entidade atual)
- `AttendanceId`, `TutorId`, Soft Delete e AuditLog.

### 4) `MedicalRecordListItemResponse`

Finalidade:
- Representar contrato resumido para futuras listagens sem carga clínica completa.

Campos:
- `long Id`
- `long PatientId`
- `DateTime UpdatedAt`
- `bool HasGeneralNotes`
- `bool HasFlags`

Por que existe:
- Evita transportar conteúdo clínico sensível/pesado quando apenas visão de listagem for necessária.

O que ficou fora:
- `GeneralNotes` e `FlagsJson` completos
- `AttendanceId`, `TutorId`, `CreatedAt`, Soft Delete e AuditLog.

## Decisões técnicas

- Contracts ficam na camada Application.
- Contracts não possuem validação.
- Contracts não acessam banco.
- Contracts não dependem de EF Core.
- Contracts não dependem de ASP.NET Core.
- Requests não incluem `PatientId`.
- `PatientId` virá pela rota.
- Response detalhada pode conter `GeneralNotes` e `FlagsJson`.
- List item evita carregar conteúdo clínico completo.
- Nenhum contrato inclui `AttendanceId`.
- Nenhum contrato inclui `TutorId`.
- Nenhum contrato inclui `CreatedAt`.
- Nenhum contrato inclui dados de Soft Delete ou AuditLog.

## Segurança e privacidade

- `GeneralNotes` e `FlagsJson` são dados clínicos sensíveis.
- Response detalhada pode retornar esses campos apenas em fluxos autorizados.
- Listagens futuras devem evitar conteúdo clínico completo.
- Logs futuros não devem registrar `GeneralNotes` nem `FlagsJson` completos.
- Erros técnicos não devem expor payload clínico.

## Pontos de atenção

- `FlagsJson` continua como débito técnico controlado.
- Validação estrutural de `FlagsJson` não será feita nesta fase.
- Unicidade por `PatientId` não será tratada nesta fase.
- Soft Delete e AuditLog permanecem fora do escopo.
- Contratos não garantem regra de negócio sozinhos.
- Validators e use cases virão nas próximas fases.

## Critérios de aceite

A fase será considerada concluída se:

- Pasta `MedicalRecords/Contracts` existir na Application.
- `CreateMedicalRecordRequest` for criado.
- `UpdateMedicalRecordRequest` for criado.
- `MedicalRecordResponse` for criado.
- `MedicalRecordListItemResponse` for criado.
- `PatientId` não estiver no body dos requests.
- Contracts não tiverem validação/lógica de negócio.
- Contracts não dependerem de EF/API.
- Documentação da fase for criada.
- Nenhuma camada fora de Application Contracts/docs for alterada.
- Nenhuma migration for criada.
- `git diff --check` não apontar problemas.
- Build/test forem executados se SDK estiver disponível, ou a limitação for registrada.

## Fora do escopo

Esta fase não implementa:

- repository;
- validators;
- use cases;
- controller;
- API;
- Infrastructure;
- migration;
- database update;
- CreatedAt;
- Soft Delete;
- AuditLog;
- roles/permissões;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## Próxima fase recomendada

**Fase 5.3.2 — Interface IMedicalRecordRepository.**

Objetivo:
- Criar a abstração de persistência da camada Application para MedicalRecord, definindo os métodos necessários para os futuros use cases sem acoplar Application ao EF Core.

Métodos esperados para avaliação na próxima fase:
- `GetByIdAsync`;
- `GetByPatientIdAsync`;
- `ExistsByPatientIdAsync`;
- `AddAsync`;
- `UpdateAsync`.

## Validações obrigatórias

Comandos executados nesta fase:

- `git branch --show-current`
- `git status --short`
- `git diff --check`

Comandos de build/test condicionais:

- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Se `dotnet` não estiver disponível, a limitação deve ser registrada sem inventar resultados.
