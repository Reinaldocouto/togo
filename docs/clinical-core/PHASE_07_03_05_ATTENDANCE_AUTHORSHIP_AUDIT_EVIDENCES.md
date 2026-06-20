# Fase 7.3.5 — Testes e evidências de autoria/auditoria de Attendance

## 1. Objetivo

Consolidar as evidências de testes da trilha de autoria e auditoria de `Attendance`, revisar a cobertura existente, corrigir ruídos simples de testes e registrar explicitamente os limites da implementação entregue até a Fase 7.3.4.

Esta fase não introduz comportamento funcional novo. O foco é evidencial: confirmar que a autoria mínima persistida e os eventos críticos de audit log estão cobertos por testes e que não houve ampliação de schema, migrations, endpoints ou ações de auditoria fora do escopo.

## 2. Contexto da Fase 7.3

A Fase 7.3 trata a autoria e a auditoria dos eventos críticos da vertical `Attendance`, dentro da expansão clínica e operacional pós-hardening de `MedicalRecord`.

A trilha revisada nesta fase considera:

- criação de atendimento;
- fechamento de atendimento;
- cancelamento de atendimento;
- persistência dos campos mínimos de autoria;
- gravação de `ClinicalAuditLog` somente para eventos críticos já definidos.

## 3. Referências das fases anteriores

Documentos usados como referência técnica e de escopo:

- `docs/clinical-core/PHASE_07_03_01_ATTENDANCE_AUTHORSHIP_AUDIT_PLANNING.md`;
- `docs/clinical-core/PHASE_07_03_02_ATTENDANCE_AUTHORSHIP_AUDIT_CONTRACTS.md`;
- `docs/clinical-core/PHASE_07_03_03_ATTENDANCE_AUTHORSHIP_IMPLEMENTATION.md`;
- `docs/clinical-core/PHASE_07_03_04_ATTENDANCE_AUDIT_LOG_IMPLEMENTATION.md`.

A Fase 7.3.3 implementou autoria mínima persistida em `Attendance`. A Fase 7.3.4 implementou escrita de `ClinicalAuditLog` para `Attendance.Created`, `Attendance.Closed` e `Attendance.Canceled`.

## 4. Arquivos de teste revisados

Foram revisados os seguintes testes:

- `backend/src/Togo.Domain.Tests/AttendanceTests.cs`;
- `backend/src/Togo.Application.Tests/Attendances/CreateAttendanceUseCaseTests.cs`;
- `backend/src/Togo.Application.Tests/Attendances/CloseAttendanceUseCaseTests.cs`;
- `backend/src/Togo.Application.Tests/Attendances/CancelAttendanceUseCaseTests.cs`;
- `backend/src/Togo.Application.Tests/Attendances/Fakes/FakeClinicalAuditLogWriter.cs`;
- `backend/src/Togo.Infrastructure.Tests/Repositories/AttendanceRepositoryTests.cs`;
- `backend/src/Togo.Api.Tests/Controllers/AttendancesControllerTests.cs`;
- `backend/src/Togo.Application.Tests/Auditing/AttendanceAuditActionsTests.cs`;
- `backend/src/Togo.Infrastructure.Tests/Auditing/EfClinicalAuditLogWriterTests.cs`.

## 5. Arquivos alterados nesta fase

Alterações realizadas:

- `backend/src/Togo.Application.Tests/Attendances/CreateAttendanceUseCaseTests.cs`: remoção de um `Assert.Empty(auditLogWriter.Events);` duplicado no cenário de paciente inexistente;
- `docs/clinical-core/PHASE_07_03_05_ATTENDANCE_AUTHORSHIP_AUDIT_EVIDENCES.md`: criação deste registro de evidências.

Não foram alterados arquivos de domínio, aplicação, infraestrutura, migrations, schema EF ou controllers.

## 6. Evidências de autoria

### Criação

A cobertura revisada confirma que a criação de `Attendance` contempla:

- `CreatedByUserId` preenchido com o usuário atual;
- `CreatedAt` técnico não default;
- `UpdatedByUserId` inicialmente igual ao usuário criador;
- `UpdatedAt` inicialmente igual ao timestamp de criação;
- rejeição de `Guid.Empty` para autoria;
- rejeição de timestamp técnico default;
- uso de `ICurrentUserService` no use case;
- falha segura por `CurrentUserResolutionException` quando o usuário atual não resolve;
- ausência de audit log quando a resolução do usuário falha.

### Fechamento

A cobertura revisada confirma que o fechamento contempla:

- `ClosedByUserId` preenchido com o usuário atual;
- `UpdatedByUserId` atualizado com o usuário que fechou;
- `UpdatedAt` atualizado no fechamento;
- preservação de `CreatedByUserId`;
- preservação de `CreatedAt`;
- validação de estado para impedir fechamento de atendimento fechado ou cancelado;
- validação de `ClosedAt >= OpenedAt`;
- uso de `ICurrentUserService` no use case;
- falha segura por `CurrentUserResolutionException` quando o usuário atual não resolve;
- ausência de audit log quando a resolução do usuário falha.

### Cancelamento

A cobertura revisada confirma que o cancelamento contempla:

- `CanceledByUserId` preenchido com o usuário atual;
- `CanceledAt` preenchido com timestamp técnico do cancelamento;
- `UpdatedByUserId` atualizado com o usuário que cancelou;
- `UpdatedAt` atualizado no cancelamento;
- preservação de `CreatedByUserId`;
- preservação de `CreatedAt`;
- `ClosedAt = null` para atendimentos cancelados;
- validação de estado para impedir cancelamento de atendimento fechado ou já cancelado;
- uso de `ICurrentUserService` no use case;
- falha segura por `CurrentUserResolutionException` quando o usuário atual não resolve;
- ausência de audit log quando a resolução do usuário falha.

## 7. Evidências de AuditLog

A cobertura revisada confirma que os use cases gravam `ClinicalAuditEvent` apenas após sucesso da operação de negócio.

### Create

O cenário de sucesso valida:

- action `Attendance.Created`;
- `EntityName = nameof(Attendance)`;
- `EntityId = attendance.Id.ToString()`;
- `UserId = currentUser.UserId`;
- `UserProfile = currentUser.Profile`;
- metadata com exatamente `PatientId` e `Status`.

Os cenários negativos confirmam ausência de audit log quando:

- paciente não existe;
- número de atendimento já existe;
- paciente já possui atendimento aberto;
- usuário atual não resolve.

### Close

O cenário de sucesso valida:

- action `Attendance.Closed`;
- `EntityName = nameof(Attendance)`;
- `EntityId = attendance.Id.ToString()`;
- `UserId = currentUser.UserId`;
- `UserProfile = currentUser.Profile`;
- metadata com exatamente `PatientId` e `Status`.

Os cenários negativos confirmam ausência de audit log quando:

- id é inválido;
- atendimento não existe;
- fechamento falha por validação de data;
- fechamento falha por estado;
- usuário atual não resolve.

### Cancel

O cenário de sucesso valida:

- action `Attendance.Canceled`;
- `EntityName = nameof(Attendance)`;
- `EntityId = attendance.Id.ToString()`;
- `UserId = currentUser.UserId`;
- `UserProfile = currentUser.Profile`;
- metadata com exatamente `PatientId` e `Status`.

Os cenários negativos confirmam ausência de audit log quando:

- id é inválido;
- atendimento não existe;
- cancelamento falha por estado;
- usuário atual não resolve.

## 8. Eventos auditados

Eventos de `Attendance` auditados e dentro do escopo atual:

- `Attendance.Created`;
- `Attendance.Closed`;
- `Attendance.Canceled`.

A inspeção de código confirma que esses eventos estão centralizados em `AttendanceAuditActions` e usados pelos use cases de criação, fechamento e cancelamento.

## 9. Eventos fora do escopo

Eventos explicitamente fora do escopo desta fase e não implementados como audit actions de `Attendance`:

- `Attendance.AccessDenied`;
- `Attendance.Updated`;
- `Attendance.Reopened`;
- `Attendance.Deleted`;
- `Attendance.Restored`.

A string `Attendance.Read` existe no projeto como permissão de autorização, não como action de audit log de `Attendance`. Nenhuma auditoria de leitura foi criada nesta fase.

## 10. Metadata permitida

A metadata permitida para os eventos auditados de `Attendance` permanece mínima e operacional:

- `PatientId`;
- `Status`.

O casing validado pelos testes é `PatientId` e `Status`.

## 11. Metadata proibida

Os testes de aplicação validam ausência de payload clínico sensível na metadata de audit log, incluindo:

- `GeneralNotes`;
- `FlagsJson`;
- `Prescription`;
- `ClinicalEvolution`.

Também permanece proibido registrar payload completo de request ou conteúdo clínico longitudinal no audit log de `Attendance`.

## 12. Cobertura de falhas sem audit log

A cobertura revisada confirma que não há escrita de audit log quando a operação de negócio não é concluída com sucesso.

Cenários cobertos:

- validação de id inválido;
- paciente inexistente;
- atendimento inexistente;
- número de atendimento duplicado;
- paciente com atendimento aberto;
- fechamento com data inválida;
- fechamento em estado inválido;
- cancelamento em estado inválido;
- falha de resolução do usuário atual.

## 13. Limitações conhecidas

- A falha do writer de auditoria após a operação de negócio não foi redesenhada nesta fase. O padrão atual dos use cases é aguardar `WriteAsync`; portanto, exceções do writer tendem a propagar para o chamador.
- Não houve alteração transacional nesta fase.
- Não foi adicionada auditoria de leitura, acesso negado, atualização, reabertura, exclusão ou restauração de `Attendance`.

## 14. Confirmação de ausência de migration/schema

Esta fase não cria migration nova e não altera schema ou configuração EF.

A migration de autoria de `Attendance` existente permanece a referência histórica da implementação anterior:

- `backend/src/Togo.Infrastructure/Migrations/20260619120000_AddAttendanceAuthorship.cs`.

## 15. Confirmação de ausência de endpoint novo

Esta fase não cria endpoint novo e não altera controller de `Attendance`.

Os testes de API foram apenas revisados como evidência de que os fluxos existentes continuam sem alteração de contrato HTTP nesta fase.

## 16. Confirmação de ausência de mudança funcional

Não houve mudança funcional no ciclo de vida de `Attendance`.

As regras de criação, fechamento e cancelamento permanecem as mesmas. A única alteração em teste foi a remoção de uma asserção duplicada sem efeito comportamental.

## 17. Riscos remanescentes

- A estratégia de tratamento de falhas do writer de auditoria pode ser aprofundada em fase futura, caso o produto decida entre propagação, retry, outbox ou tolerância controlada a falha de auditoria.
- Novos eventos de auditoria de `Attendance` permanecem fora do escopo até decisão técnica e regulatória específica.
- Auditoria de leitura e acesso negado exige desenho próprio para evitar ruído excessivo e exposição indevida de contexto clínico.

## 18. Critérios de aceite

Critérios atendidos nesta fase:

- evidências de autoria documentadas;
- evidências de audit log documentadas;
- testes existentes revisados;
- ruído simples de teste corrigido;
- lacunas objetivas registradas;
- nenhuma regra de negócio alterada;
- nenhuma migration criada;
- nenhum schema alterado;
- nenhum endpoint novo criado;
- nenhum evento fora do escopo implementado;
- documentação da fase criada.

## 19. Próxima fase recomendada

Próxima fase recomendada:

```text
Fase 7.3.6 — Encerramento da trilha de autoria/auditoria de Attendance
```
