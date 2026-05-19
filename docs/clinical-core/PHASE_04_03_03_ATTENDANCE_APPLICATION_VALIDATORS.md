# TOGO — Fase 4.3.3: Validators Application de Attendance

## 1. Objetivo

Criar validators mínimos na camada Application para Attendance, focando validações prévias de consistência e regras de negócio antes da implementação dos use cases.

## 2. Contexto

- O domínio Attendance foi concluído na Fase 4.2.
- Os contratos Application de Attendance foram criados na Fase 4.3.1.
- A interface `IAttendanceRepository` foi criada na Fase 4.3.2.
- Estes validators preparam a implementação segura dos próximos use cases.

## 3. Validators criados

- `AttendancePatientExistsValidator`
  - Arquivo: `backend/src/Togo.Application/Attendances/Validators/AttendancePatientExistsValidator.cs`
  - Dependências: `IPetRepository`
  - Regra: valida `patientId > 0` e existência do paciente por `GetByPatientIdAsync`.

- `AttendanceNumberUniqueValidator`
  - Arquivo: `backend/src/Togo.Application/Attendances/Validators/AttendanceNumberUniqueValidator.cs`
  - Dependências: `IAttendanceRepository`
  - Regra: valida número obrigatório, aplica `Trim()` e verifica unicidade por `ExistsByAttendanceNumberAsync`.

- `OpenAttendanceValidator`
  - Arquivo: `backend/src/Togo.Application/Attendances/Validators/OpenAttendanceValidator.cs`
  - Dependências: `IAttendanceRepository`
  - Regra: valida `patientId > 0` e impede paciente com atendimento aberto por `HasOpenAttendanceForPatientAsync`.

## 4. Testes criados

- `AttendancePatientExistsValidatorTests`
  - `ValidateAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid`
  - `ValidateAsync_ShouldReturnNotFound_WhenPatientDoesNotExist`
  - `ValidateAsync_ShouldReturnSuccess_WhenPatientExists`

- `AttendanceNumberUniqueValidatorTests`
  - `ValidateAsync_ShouldReturnValidationError_WhenAttendanceNumberIsEmpty`
  - `ValidateAsync_ShouldReturnConflict_WhenAttendanceNumberAlreadyExists`
  - `ValidateAsync_ShouldReturnSuccess_WhenAttendanceNumberDoesNotExist`
  - `ValidateAsync_ShouldTrimAttendanceNumberBeforeChecking`

- `OpenAttendanceValidatorTests`
  - `ValidateAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid`
  - `ValidateAsync_ShouldReturnConflict_WhenPatientAlreadyHasOpenAttendance`
  - `ValidateAsync_ShouldReturnSuccess_WhenPatientDoesNotHaveOpenAttendance`

## 5. Decisões técnicas

- Foi seguido o padrão de validação existente em Pets/Tutors com `ApplicationResult<bool>` e tipos `ValidationError`, `NotFound`, `Conflict` e `Success`.
- A validação de paciente existente reutilizou `IPetRepository` já disponível na camada Application, sem criação de nova interface nesta fase.
- A unicidade de `AttendanceNumber` foi validada via `IAttendanceRepository.ExistsByAttendanceNumberAsync`.
- A regra de atendimento aberto por paciente foi validada via `IAttendanceRepository.HasOpenAttendanceForPatientAsync`.
- `AttendanceNumber` é trimado antes da consulta de unicidade.
- Não houve acesso direto a banco/EF; apenas abstrações de repositório foram utilizadas.

## 6. Fora do escopo

- use cases;
- implementação Infrastructure;
- repositories concretos;
- controller/API;
- endpoints;
- DI/Program.cs;
- migrations;
- banco;
- eventos;
- RabbitMQ;
- Redis;
- Docker;
- Kubernetes.

## 7. Próxima fase recomendada

**Fase 4.3.4 — Criar use case CreateAttendanceUseCase.**

Objetivo: criar o primeiro use case de Attendance usando os contratos, validators e `IAttendanceRepository` já preparados.
