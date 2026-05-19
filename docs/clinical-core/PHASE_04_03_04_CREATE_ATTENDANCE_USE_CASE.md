# TOGO — Fase 4.3.4: CreateAttendanceUseCase

## 1. Objetivo

Implementar o primeiro use case de Attendance na camada Application, cobrindo o fluxo de criação de atendimento com validações de negócio e retorno padronizado via `ApplicationResult`.

## 2. Contexto

Esta fase continua a evolução iniciada anteriormente:
- Fase 4.2 consolidou o domínio de Attendance;
- Fase 4.3.1 criou os contratos de Application de Attendance;
- Fase 4.3.2 criou a interface `IAttendanceRepository`;
- Fase 4.3.3 criou os validators de Application de Attendance;
- Fase 4.3.3.1 corrigiu o fake repository para evitar sobrescrita com `Id = 0`.

## 3. Use case criado

- **Arquivo:** `backend/src/Togo.Application/Attendances/UseCases/CreateAttendanceUseCase.cs`
- **Namespace:** `Togo.Application.Attendances.UseCases`
- **Dependências:**
  - `IAttendanceRepository`
  - `AttendancePatientExistsValidator`
  - `AttendanceNumberUniqueValidator`
  - `OpenAttendanceValidator`
  - `ILogger<CreateAttendanceUseCase>`
- **Entrada:** `CreateAttendanceRequest`
- **Saída:** `ApplicationResult<AttendanceResponse>`

## 4. Fluxo implementado

Ordem de execução aplicada:
1. validar Patient;
2. validar AttendanceNumber;
3. validar existência de Attendance Open para o Patient;
4. criar entidade de domínio com `Attendance.Create`;
5. persistir com `IAttendanceRepository.AddAsync`;
6. retornar `AttendanceResponse` em `ApplicationResult.Success`.

## 5. Tratamento de erros

O use case retorna o erro no ponto de falha e interrompe o fluxo:
- erro de Patient (`ValidationError`/`NotFound`);
- erro de AttendanceNumber (`ValidationError`/`Conflict`);
- erro de Attendance Open (`ValidationError`/`Conflict`);
- erro de domínio esperado (`ArgumentException`/`ArgumentOutOfRangeException`) convertido para `ApplicationResult.ValidationError`.

## 6. Testes criados

Arquivo: `backend/src/Togo.Application.Tests/Attendances/CreateAttendanceUseCaseTests.cs`

Cenários cobertos:
- `ExecuteAsync_ShouldReturnSuccess_WhenRequestIsValid`
- `ExecuteAsync_ShouldReturnError_WhenPatientDoesNotExist`
- `ExecuteAsync_ShouldReturnConflict_WhenAttendanceNumberAlreadyExists`
- `ExecuteAsync_ShouldReturnConflict_WhenPatientAlreadyHasOpenAttendance`
- `ExecuteAsync_ShouldReturnValidationError_WhenAttendanceNumberIsInvalid`
- `ExecuteAsync_ShouldTrimAttendanceNumber_WhenCreatingAttendance`

## 7. Fora do escopo

- Infrastructure;
- repository concreto;
- API/controller;
- endpoints;
- DI/Program.cs;
- migrations;
- banco;
- `CloseAttendanceUseCase`;
- `CancelAttendanceUseCase`;
- use cases de List/Get;
- RabbitMQ;
- Redis;
- Docker;
- Kubernetes.

## 8. Próxima fase recomendada

Fase 4.3.5 — Criar `GetAttendanceByIdUseCase` e `ListAttendancesUseCase`.
