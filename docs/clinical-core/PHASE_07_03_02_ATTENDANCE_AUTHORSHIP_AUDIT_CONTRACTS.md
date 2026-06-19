# TOGO — Fase 7.3.2: Contratos/base técnica de autoria e audit actions de Attendance

## 1. Objetivo

Esta fase cria a base técnica mínima para permitir que fases futuras implementem autoria e auditoria dos eventos críticos de `Attendance`.

O objetivo principal é introduzir `AttendanceAuditActions` com as actions mínimas de ciclo de vida de atendimento, seguindo o padrão simples de constantes já usado por `MedicalRecordAuditActions`.

## 2. Contexto

A Fase 7.3.1 planejou a autoria e auditoria de `Attendance` e identificou ausência de autoria de criação, fechamento e cancelamento, além de ausência de `CreatedAt`, `UpdatedAt`, `CanceledAt`, escrita de `ClinicalAuditLog` e actions específicas de auditoria.

Esta fase materializa apenas as actions mínimas necessárias para as próximas etapas. Ela ainda não persiste autoria, ainda não altera entidade, ainda não cria migration e ainda não escreve `ClinicalAuditLog`.

## 3. Referência à Fase 7.3.1

A referência técnica e documental desta fase é:

```text
docs/clinical-core/PHASE_07_03_01_ATTENDANCE_AUTHORSHIP_AUDIT_PLANNING.md
```

Esse documento definiu que os eventos críticos mínimos da primeira trilha de auditoria de `Attendance` são criação, fechamento e cancelamento.

## 4. Actions criadas

Foram criadas as seguintes actions:

```text
Attendance.Created
Attendance.Closed
Attendance.Canceled
```

Essas actions representam a base inicial para auditoria futura dos eventos críticos de ciclo de vida de `Attendance`.

## 5. Actions deliberadamente não criadas

As seguintes actions foram deliberadamente mantidas fora desta fase:

```text
Attendance.Read
Attendance.AccessDenied
Attendance.Updated
Attendance.Reopened
Attendance.Deleted
Attendance.Restored
```

Essas actions ficaram fora do escopo inicial porque a Fase 7.3.1 priorizou eventos críticos de escrita/estado já existentes no ciclo de vida atual. Leitura auditada, negação de acesso, update genérico, reabertura, exclusão e restauração devem ser planejados separadamente quando houver necessidade real.

## 6. Arquivos criados

Foram criados os seguintes arquivos:

```text
backend/src/Togo.Application/Auditing/AttendanceAuditActions.cs
backend/src/Togo.Application.Tests/Auditing/AttendanceAuditActionsTests.cs
docs/clinical-core/PHASE_07_03_02_ATTENDANCE_AUTHORSHIP_AUDIT_CONTRACTS.md
```

## 7. Contratos existentes reaproveitados

A inspeção confirmou que os contratos existentes são suficientes para a próxima etapa de autoria e auditoria de `Attendance`:

```text
ICurrentUserService
CurrentUserInfo
CurrentUserResolutionException
ClinicalAuditEvent
IClinicalAuditLogWriter
ClinicalAuditLog
EfClinicalAuditLogWriter
```

`ICurrentUserService`, `CurrentUserInfo` e `CurrentUserResolutionException` já formam a base de resolução segura de usuário atual. `ClinicalAuditEvent`, `IClinicalAuditLogWriter`, `ClinicalAuditLog` e `EfClinicalAuditLogWriter` já formam a base genérica de escrita futura de auditoria clínica.

## 8. Contratos novos não criados

Não foram criados contratos específicos adicionais, como:

```text
AttendanceAuditEvent
AttendanceAuditMetadata
AttendanceCurrentUserService
AttendanceAuditWriter
```

A decisão evita duplicação e overengineering. `ClinicalAuditEvent` e `IClinicalAuditLogWriter` já cobrem a necessidade inicial de transporte e persistência futura de eventos de auditoria, enquanto a resolução de usuário atual já está coberta por `ICurrentUserService`.

## 9. Impacto técnico

Esta fase confirma os seguintes impactos técnicos:

- nenhuma entidade foi alterada;
- nenhum campo de autoria foi adicionado;
- nenhuma migration foi criada;
- nenhum `AuditLog` é escrito ainda;
- nenhum use case foi alterado;
- nenhum controller foi alterado;
- nenhuma regra de negócio foi alterada;
- nenhum endpoint foi criado;
- nenhum schema foi alterado.

## 10. Impacto em testes

Foram adicionados testes unitários simples para validar as constantes de `AttendanceAuditActions`.

Esta fase não adiciona teste funcional de `AuditLog`, não adiciona teste funcional de autoria e não valida escrita real em banco. Esses testes ficam reservados para fases futuras, quando autoria e escrita de auditoria forem implementadas.

## 11. Riscos remanescentes

Permanecem os seguintes riscos e decisões para fases futuras:

- autoria ainda não persistida;
- `AuditLog` ainda não escrito;
- atomicidade entre operação de negócio e log ainda não decidida;
- metadata real ainda será definida na implementação;
- campos em `Attendance` ainda exigirão migration futura;
- actions de leitura e acesso negado ainda estão fora do escopo.

## 12. Fora do escopo

Esta fase não implementa:

- campos;
- migrations;
- autoria;
- writer;
- escrita de `AuditLog`;
- alteração de use case;
- alteração de controller;
- alteração de repository;
- alteração de domínio;
- endpoint;
- frontend;
- infraestrutura.

## 13. Critérios de aceite

A fase será aceita se:

- `AttendanceAuditActions` for criado;
- actions mínimas forem `Attendance.Created`, `Attendance.Closed`, `Attendance.Canceled`;
- actions fora do escopo não forem criadas;
- testes de constantes forem criados;
- contratos existentes forem documentados como reaproveitados;
- nenhum campo de autoria for persistido;
- nenhum `AuditLog` for escrito;
- nenhuma migration for criada;
- nenhuma regra de negócio for alterada;
- documento da fase for criado;
- build passar;
- testes passarem.

## 14. Próxima fase recomendada

Recomenda-se a próxima fase:

```text
Fase 7.3.3 — Implementação de autoria mínima de Attendance
```

Objetivo sugerido:

```text
Adicionar campos mínimos de autoria em Attendance e ajustar os fluxos de criação, fechamento e cancelamento para usar ICurrentUserService, sem ainda escrever ClinicalAuditLog.
```
