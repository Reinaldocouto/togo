# TOGO — Fase 4.3.1: Contratos Application de Attendance

## 1. Objetivo

Esta fase cria os contratos mínimos da camada Application para Attendance, limitados a DTOs de request/response para suportar os próximos casos de uso.

## 2. Contexto

O domínio Attendance foi concluído na Fase 4.2, com regras de ciclo de vida e invariantes já implementadas e testadas.

## 3. Contratos criados

- `CreateAttendanceRequest`
  - `PatientId`
  - `AttendanceNumber`
  - `OpenedAt`
  - `Type`

- `AttendanceResponse`
  - `Id`
  - `PatientId`
  - `AttendanceNumber`
  - `OpenedAt`
  - `ClosedAt`
  - `Status`
  - `Type`

- `AttendanceListItemResponse`
  - `Id`
  - `PatientId`
  - `AttendanceNumber`
  - `OpenedAt`
  - `ClosedAt`
  - `Status`
  - `Type`

- `CloseAttendanceRequest`
  - `ClosedAt`

## 4. Decisões técnicas

- `CreateAttendanceRequest` não recebe `Status`.
- `CreateAttendanceRequest` não recebe `ClosedAt`.
- Attendance nasce `Open` no domínio.
- `ClosedAt` só é definido por `Close`.
- Cancel não tem request/body nesta fase (não foi criado `CancelAttendanceRequest`).
- Nenhum dado de `Patient`/`Pet`/`Tutor` entra nas responses nesta fase.

## 5. Fora do escopo

- use cases;
- validators;
- repositories;
- Infrastructure;
- API/controller;
- migrations;
- banco;
- eventos;
- RabbitMQ;
- Redis;
- Docker;
- Kubernetes.

## 6. Próxima fase recomendada

**Fase 4.3.2 — Criar interface `IAttendanceRepository`.**

Objetivo: definir o contrato de persistência necessário para os futuros use cases de Attendance, sem implementar Infrastructure ainda.
