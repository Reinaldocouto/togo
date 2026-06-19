# Fase 7.3.3 — Implementação de autoria mínima de Attendance

## 1. Objetivo

Implementar autoria mínima persistida na entidade `Attendance`, registrando usuário e instante técnico de criação, usuário e instante da última atualização auditável, usuário de fechamento, usuário de cancelamento e instante técnico do cancelamento.

## 2. Contexto da Fase 7.3

A Fase 7.3 trata autoria e auditoria dos eventos críticos da vertical `Attendance`, após o hardening de `MedicalRecord`.

## 3. Referências às fases anteriores

Esta implementação segue as decisões da Fase 7.3.1 documentadas em `PHASE_07_03_01_ATTENDANCE_AUTHORSHIP_AUDIT_PLANNING.md` e preserva os contratos criados na Fase 7.3.2 em `PHASE_07_03_02_ATTENDANCE_AUTHORSHIP_AUDIT_CONTRACTS.md`.

## 4. Campos de autoria adicionados

Foram adicionados a `Attendance`:

- `CreatedByUserId: Guid`
- `CreatedAt: DateTime`
- `UpdatedByUserId: Guid`
- `UpdatedAt: DateTime`
- `ClosedByUserId: Guid?`
- `CanceledByUserId: Guid?`
- `CanceledAt: DateTime?`

## 5. Decisão sobre `OpenedAt` vs `CreatedAt`

`OpenedAt` continua sendo o timestamp operacional/clínico informado na abertura do atendimento. `CreatedAt` é o timestamp técnico de criação do registro no sistema e é obtido com `DateTime.UtcNow` no use case de criação.

## 6. Arquivos alterados/criados

- `backend/src/Togo.Domain/Entities/Attendance.cs`
- `backend/src/Togo.Application/Attendances/UseCases/CreateAttendanceUseCase.cs`
- `backend/src/Togo.Application/Attendances/UseCases/CloseAttendanceUseCase.cs`
- `backend/src/Togo.Application/Attendances/UseCases/CancelAttendanceUseCase.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/AttendanceConfiguration.cs`
- `backend/src/Togo.Infrastructure/Persistence/Migrations/20260619120000_AddAttendanceAuthorship.cs`
- `backend/src/Togo.Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs`
- Testes de domínio, aplicação, API e infraestrutura relacionados a `Attendance`.

## 7. Impacto em domínio

A factory `Attendance.Create` passou a receber autoria técnica obrigatória. Os métodos `Close` e `Cancel` passaram a receber usuário atual e timestamp técnico da alteração. A entidade rejeita `Guid.Empty` e `default(DateTime)` para campos obrigatórios de autoria.

## 8. Impacto em use cases

`CreateAttendanceUseCase`, `CloseAttendanceUseCase` e `CancelAttendanceUseCase` passaram a depender de `ICurrentUserService`. A falha de resolução de usuário atual é propagada por `CurrentUserResolutionException`, preservando falha segura.

## 9. Impacto em EF/migration

A configuração EF mapeia os novos campos. A migration `AddAttendanceAuthorship` adiciona colunas obrigatórias e opcionais em `Attendances`.

## 10. Impacto em testes

Os testes foram ajustados para criar atendimentos com autoria explícita e validar persistência/autoria nas transições de criação, fechamento e cancelamento.

## 11. Ausência de AuditLog nesta fase

Esta fase não grava `ClinicalAuditLog` para `Attendance.Created`, `Attendance.Closed` ou `Attendance.Canceled`.

## 12. Ausência de `IClinicalAuditLogWriter` nos use cases

Os use cases de `Attendance` não usam `IClinicalAuditLogWriter` nesta fase.

## 13. Riscos remanescentes

A auditoria clínica dos eventos críticos ainda depende da próxima fase. A autoria mínima permite rastreabilidade técnica, mas ainda não substitui o log clínico formal.

## 14. Limitações para registros legados

A autoria histórica real de registros `Attendance` já existentes não pode ser reconstruída automaticamente. A migration usa `Guid.Empty` e `1970-01-01T00:00:00Z` como valores técnicos transitórios para manter o banco migrável. Ambientes com dados reais devem planejar saneamento posterior.

## 15. Fora do escopo

Não foram implementados AuditLog, novos endpoints, alterações de autorização, alterações em `MedicalRecord`, `ClinicalEvolution` ou `Prescription`, soft delete, retenção, justificativa de cancelamento, frontend ou infraestrutura operacional.

## 16. Critérios de aceite

A fase atende aos critérios quando os campos são persistidos, os use cases usam usuário atual, `OpenedAt` mantém semântica clínica/operacional, nenhuma escrita de AuditLog é feita, a migration existe e os testes são executáveis.

## 17. Próxima fase recomendada

Fase 7.3.4 — Implementação de AuditLog para eventos críticos de Attendance.
