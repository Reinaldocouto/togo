# Fase 7.3.4 — Implementação de AuditLog para eventos críticos de Attendance

## 1. Objetivo

Implementar a escrita mínima de `ClinicalAuditLog` para eventos críticos de `Attendance`, reutilizando a infraestrutura existente de auditoria clínica (`ClinicalAuditEvent`, `IClinicalAuditLogWriter`, `EfClinicalAuditLogWriter` e `ClinicalAuditLog`).

## 2. Contexto da Fase 7.3

A Fase 7.3 trata autoria e auditoria dos eventos críticos da vertical `Attendance` na expansão clínica e operacional pós-hardening de `MedicalRecord`.

## 3. Referências às fases anteriores

Esta fase depende das entregas:

- Fase 7.3.1 — planejamento técnico de autoria e auditoria de `Attendance`.
- Fase 7.3.2 — contratos/base técnica de autoria e criação de `AttendanceAuditActions`.
- Fase 7.3.3 — autoria mínima em `Attendance`.
- Fase 7.3.3.1 — correção documental dos caminhos de migration da autoria de `Attendance`.

## 4. Eventos auditados

Foram implementados somente os eventos:

- `Attendance.Created`.
- `Attendance.Closed`.
- `Attendance.Canceled`.

## 5. Eventos deliberadamente não auditados

Permanecem fora desta fase:

- `Attendance.Read`.
- `Attendance.AccessDenied`.
- `Attendance.Updated`.
- `Attendance.Reopened`.
- `Attendance.Deleted`.
- `Attendance.Restored`.

## 6. Arquivos alterados/criados

Arquivos alterados:

- `backend/src/Togo.Application/Attendances/UseCases/CreateAttendanceUseCase.cs`.
- `backend/src/Togo.Application/Attendances/UseCases/CloseAttendanceUseCase.cs`.
- `backend/src/Togo.Application/Attendances/UseCases/CancelAttendanceUseCase.cs`.
- `backend/src/Togo.Application.Tests/Attendances/CreateAttendanceUseCaseTests.cs`.
- `backend/src/Togo.Application.Tests/Attendances/CloseAttendanceUseCaseTests.cs`.
- `backend/src/Togo.Application.Tests/Attendances/CancelAttendanceUseCaseTests.cs`.
- `backend/src/Togo.Api.Tests/Controllers/AttendancesControllerTests.cs`.

Arquivos criados:

- `backend/src/Togo.Application.Tests/Attendances/Fakes/FakeClinicalAuditLogWriter.cs`.
- `docs/clinical-core/PHASE_07_03_04_ATTENDANCE_AUDIT_LOG_IMPLEMENTATION.md`.

## 7. Metadata permitida

A metadata gravada é minimizada e contém apenas:

```json
{
  "PatientId": 123,
  "Status": "Open"
}
```

O mesmo formato é usado para os status `Closed` e `Canceled`.

## 8. Metadata proibida

Não deve ser gravado no audit log:

- texto clínico;
- evolução clínica;
- prescrição;
- observações completas;
- payload completo do request;
- dados pessoais desnecessários;
- dados duplicados de tutor/pet/paciente;
- conteúdo sensível além do mínimo necessário.

## 9. Impacto em `CreateAttendanceUseCase`

Após `AddAsync` bem-sucedido, o use case escreve `ClinicalAuditEvent` com:

- `EntityName = nameof(Attendance)`;
- `EntityId = attendance.Id.ToString()`;
- `Action = AttendanceAuditActions.Created`;
- `UserId = currentUser.UserId`;
- `UserProfile = currentUser.Profile`;
- `OccurredAt = DateTime.UtcNow`;
- metadata com `PatientId` e `Status`.

O audit log não é escrito quando validações anteriores falham ou quando o usuário atual não é resolvido antes da criação.

## 10. Impacto em `CloseAttendanceUseCase`

Após `UpdateAsync` bem-sucedido, o use case escreve `ClinicalAuditEvent` com `AttendanceAuditActions.Closed`, usando o mesmo usuário resolvido para autoria e metadata mínima com `PatientId` e `Status`.

O audit log não é escrito quando o id é inválido, o atendimento não existe, o fechamento falha por validação/domínio/estado, ou o usuário atual não é resolvido.

## 11. Impacto em `CancelAttendanceUseCase`

Após `UpdateAsync` bem-sucedido, o use case escreve `ClinicalAuditEvent` com `AttendanceAuditActions.Canceled`, usando o mesmo usuário resolvido para autoria e metadata mínima com `PatientId` e `Status`.

O audit log não é escrito quando o id é inválido, o atendimento não existe, o cancelamento falha por estado, ou o usuário atual não é resolvido.

## 12. Impacto em testes

Os testes de aplicação passaram a usar `FakeClinicalAuditLogWriter` para capturar eventos sem banco real. Foram cobertos fluxos de sucesso, uso de `UserId`/`UserProfile`, metadata minimizada e não escrita de audit log em falhas relevantes.

Os testes de API foram ajustados apenas para fornecer o novo writer fake aos constructors dos use cases.

## 13. Ausência de migration/schema

Nenhuma migration foi criada e nenhuma alteração de schema foi realizada. A fase reaproveita a tabela e entidade de audit log já existentes.

## 14. Ausência de endpoint novo

Nenhum endpoint novo foi criado. A fase não expõe audit log publicamente.

## 15. Observação sobre transação/atomicidade

A escrita de audit log segue o padrão atual usado em `MedicalRecord` e não resolve, nesta fase, uma transação atômica única entre operação de negócio e audit log.

## 16. Riscos remanescentes

- A operação de negócio e a escrita do audit log continuam em chamadas sequenciais, seguindo o padrão atual.
- Auditorias adicionais, como leitura, acesso negado, atualização, reabertura, exclusão e restauração, permanecem para fases futuras se forem aprovadas.

## 17. Fora do escopo

Não foram implementados nesta fase:

- nova entidade de audit log;
- nova tabela;
- nova migration;
- endpoint público de auditoria;
- auditoria de leitura;
- auditoria de acesso negado;
- auditoria de `Attendance.Updated`;
- auditoria de `Attendance.Reopened`;
- auditoria de `Attendance.Deleted`;
- auditoria de `Attendance.Restored`;
- alteração de autorização/policies;
- alteração de schema;
- frontend ou infraestrutura externa.

## 18. Critérios de aceite

A fase atende aos critérios quando:

- `Attendance.Created` é escrito após criação bem-sucedida;
- `Attendance.Closed` é escrito após fechamento bem-sucedido;
- `Attendance.Canceled` é escrito após cancelamento bem-sucedido;
- falhas de validação/estado/not found/usuário não resolvido não gravam audit log;
- `IClinicalAuditLogWriter` e `ClinicalAuditEvent` são reaproveitados;
- metadata é mínima;
- payload clínico sensível não é registrado;
- nenhuma migration, schema ou endpoint novo é criado;
- testes de aplicação cobrem sucesso e não escrita em falha.

## 19. Próxima fase recomendada

Fase 7.3.5 — Testes e evidências de autoria/auditoria de Attendance.
