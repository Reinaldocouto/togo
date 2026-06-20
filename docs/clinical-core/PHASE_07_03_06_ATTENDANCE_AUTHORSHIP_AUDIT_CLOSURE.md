# Fase 7.3.6 — Encerramento da trilha de autoria/auditoria de Attendance

## 1. Objetivo

Esta fase encerra formalmente a trilha de autoria e auditoria de `Attendance` da Fase 7.3.

O objetivo é consolidar, em um único documento de governança, o planejamento, a base técnica, a autoria mínima, o audit log, os testes, as evidências, os limites, os riscos remanescentes, a decisão final e a próxima fase recomendada para a expansão clínica e operacional pós-hardening `MedicalRecord`.

Esta fase é exclusivamente documental. Não implementa código, teste, migration, endpoint, schema, campo novo, action nova, infraestrutura ou alteração funcional.

## 2. Contexto

A vertical `Attendance` já teve autorização granular encerrada na Fase 7.2, consolidando permissões, policies, matriz por profile, proteção por action no controller e evidências de segurança por operação.

A Fase 7.3 adicionou rastreabilidade técnica e auditoria mínima aos eventos críticos do ciclo de vida de `Attendance`, seguindo o padrão conservador já usado na trilha de `MedicalRecord`: autoria explícita, usuário atual resolvido por contrato de aplicação, audit log clínico mínimo e minimização de metadata.

Criação, fechamento e cancelamento foram tratados como eventos críticos da operação clínica porque representam, respectivamente, abertura de episódio assistencial, encerramento de atendimento e interrupção/cancelamento de fluxo assistencial.

## 3. Subfases consolidadas

- 7.3.1 — Planejamento técnico de autoria e auditoria de Attendance.
- 7.3.2 — Contratos/base técnica de autoria e audit actions de Attendance.
- 7.3.3 — Implementação de autoria mínima de Attendance.
- 7.3.3.1 — Correção documental dos caminhos de migration da autoria de Attendance.
- 7.3.4 — Implementação de AuditLog para eventos críticos de Attendance.
- 7.3.5 — Testes e evidências de autoria/auditoria de Attendance.
- 7.3.6 — Encerramento da trilha de autoria/auditoria de Attendance.

## 4. Artefatos documentais consolidados

Documentos da trilha 7.3 consolidados neste encerramento:

- `docs/clinical-core/PHASE_07_03_01_ATTENDANCE_AUTHORSHIP_AUDIT_PLANNING.md`.
- `docs/clinical-core/PHASE_07_03_02_ATTENDANCE_AUTHORSHIP_AUDIT_CONTRACTS.md`.
- `docs/clinical-core/PHASE_07_03_03_ATTENDANCE_AUTHORSHIP_IMPLEMENTATION.md`.
- `docs/clinical-core/PHASE_07_03_04_ATTENDANCE_AUDIT_LOG_IMPLEMENTATION.md`.
- `docs/clinical-core/PHASE_07_03_05_ATTENDANCE_AUTHORSHIP_AUDIT_EVIDENCES.md`.
- `docs/clinical-core/PHASE_07_03_06_ATTENDANCE_AUTHORSHIP_AUDIT_CLOSURE.md`.

Documentos de contexto consultados para governança e continuidade da Fase 7:

- `docs/clinical-core/PHASE_07_02_04_ATTENDANCE_AUTHORIZATION_CLOSURE.md`.
- `docs/clinical-core/PHASE_07_01_ATTENDANCE_TECHNICAL_PLANNING.md`.
- `docs/clinical-core/PHASE_07_00_CLINICAL_OPERATIONAL_EXPANSION_PLANNING.md`.
- `docs/clinical-core/PHASE_06_06_05_MEDICAL_RECORD_PHASE_06_CLOSURE.md`.

## 5. Artefatos técnicos consolidados

Arquivos técnicos criados ou alterados ao longo da trilha 7.3 e consolidados documentalmente neste encerramento:

- `backend/src/Togo.Domain/Entities/Attendance.cs`.
- `backend/src/Togo.Application/Auditing/AttendanceAuditActions.cs`.
- `backend/src/Togo.Application/Attendances/UseCases/CreateAttendanceUseCase.cs`.
- `backend/src/Togo.Application/Attendances/UseCases/CloseAttendanceUseCase.cs`.
- `backend/src/Togo.Application/Attendances/UseCases/CancelAttendanceUseCase.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/AttendanceConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Migrations/20260619120000_AddAttendanceAuthorship.cs`.
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`.
- `backend/src/Togo.Application.Tests/Attendances/Fakes/FakeClinicalAuditLogWriter.cs`.

Arquivos técnicos principais consultados nesta fase de encerramento:

- `backend/src/Togo.Application/Security/ICurrentUserService.cs`.
- `backend/src/Togo.Application/Auditing/IClinicalAuditLogWriter.cs`.
- `backend/src/Togo.Application/Auditing/ClinicalAuditEvent.cs`.

Não foi identificada ausência entre os arquivos técnicos e de testes obrigatórios informados para esta fase.

## 6. Autoria mínima consolidada

A trilha 7.3 consolidou autoria mínima persistida em `Attendance` com os seguintes campos:

```text
CreatedByUserId
CreatedAt
UpdatedByUserId
UpdatedAt
ClosedByUserId
CanceledByUserId
CanceledAt
```

Semântica consolidada:

- `CreatedByUserId`: usuário que criou tecnicamente o atendimento.
- `CreatedAt`: timestamp técnico de criação.
- `UpdatedByUserId`: último usuário que alterou estado/dados auditáveis.
- `UpdatedAt`: timestamp técnico da última atualização.
- `ClosedByUserId`: usuário que fechou o atendimento.
- `CanceledByUserId`: usuário que cancelou o atendimento.
- `CanceledAt`: timestamp técnico do cancelamento.

A autoria mínima cobre criação, fechamento e cancelamento. Ela não declara auditoria completa de todos os acessos, leituras, alterações genéricas ou operações futuras possíveis de `Attendance`.

## 7. Decisão sobre `OpenedAt`

`OpenedAt` permanece como timestamp operacional/clínico e não substitui `CreatedAt`.

A decisão preserva a distinção entre o horário informado para abertura clínica/operacional do atendimento e o instante técnico em que o registro foi criado no sistema.

## 8. AuditLog consolidado

Eventos de audit log implementados para `Attendance`:

```text
Attendance.Created
Attendance.Closed
Attendance.Canceled
```

Os eventos usam a base técnica já existente de auditoria clínica:

- `ClinicalAuditEvent`.
- `IClinicalAuditLogWriter`.
- `ClinicalAuditLog`.
- `AttendanceAuditActions`.
- `UserId`.
- `UserProfile`.
- metadata mínima.

A escrita de audit log foi consolidada nos fluxos de sucesso de criação, fechamento e cancelamento. Cenários de falha de validação ou falha de resolução de usuário atual não devem gerar audit log de sucesso.

## 9. Metadata consolidada

Metadata permitida para eventos auditados de `Attendance`:

```json
{
  "PatientId": 123,
  "Status": "Open"
}
```

O mesmo formato mínimo vale para `Attendance.Closed` e `Attendance.Canceled`, ajustando o valor de `Status` conforme o estado final do atendimento.

Metadata proibida:

- texto clínico;
- evolução clínica;
- prescrição;
- observações completas;
- payload completo do request;
- dados pessoais desnecessários;
- dados duplicados de tutor/pet/paciente;
- conteúdo sensível além do mínimo necessário.

A metadata consolidada tem finalidade operacional mínima: identificar o paciente por chave técnica e registrar o status resultante do evento auditado, sem transportar conteúdo clínico longitudinal.

## 10. Eventos fora do escopo

Eventos explicitamente não implementados como audit actions de `Attendance` nesta trilha:

```text
Attendance.Read
Attendance.AccessDenied
Attendance.Updated
Attendance.Reopened
Attendance.Deleted
Attendance.Restored
```

`Attendance.Read` existe como permissão de autorização, não como auditoria de leitura. Nenhuma auditoria de leitura foi implementada na Fase 7.3.

Leitura auditada, acesso negado, update genérico, reabertura, exclusão e restauração permanecem dependentes de planejamento específico, decisão de produto, controle de ruído e desenho técnico adequado.

## 11. Evidências de testes

A trilha 7.3 consolidou cobertura de testes e evidências para:

- autoria na criação;
- autoria no fechamento;
- autoria no cancelamento;
- persistência de campos de autoria;
- falha segura com usuário não resolvido;
- `Attendance.Created` em sucesso;
- `Attendance.Closed` em sucesso;
- `Attendance.Canceled` em sucesso;
- ausência de audit log em falhas;
- metadata mínima;
- ausência de payload sensível;
- testes de constantes de `AttendanceAuditActions`;
- ajustes em API tests para novos constructors.

Testes principais consultados e consolidados:

- `backend/src/Togo.Domain.Tests/AttendanceTests.cs`.
- `backend/src/Togo.Application.Tests/Attendances/CreateAttendanceUseCaseTests.cs`.
- `backend/src/Togo.Application.Tests/Attendances/CloseAttendanceUseCaseTests.cs`.
- `backend/src/Togo.Application.Tests/Attendances/CancelAttendanceUseCaseTests.cs`.
- `backend/src/Togo.Application.Tests/Attendances/Fakes/FakeClinicalAuditLogWriter.cs`.
- `backend/src/Togo.Infrastructure.Tests/Repositories/AttendanceRepositoryTests.cs`.
- `backend/src/Togo.Api.Tests/Controllers/AttendancesControllerTests.cs`.
- `backend/src/Togo.Application.Tests/Auditing/AttendanceAuditActionsTests.cs`.

## 12. Limitações conhecidas

Limitações conhecidas ao encerrar a Fase 7.3:

- ausência de transação atômica única entre operação de negócio e audit log;
- falha do writer de auditoria tende a propagar;
- auditoria de leitura/acesso negado ainda não implementada;
- eventos de update/reopen/delete/restore fora do escopo;
- registros legados podem conter valores técnicos transitórios da migration de autoria;
- saneamento futuro pode ser necessário em ambientes com dados reais.

Essas limitações não bloqueiam o encerramento da trilha mínima de autoria/auditoria de `Attendance`, mas devem permanecer visíveis para evolução futura.

## 13. Riscos remanescentes

Riscos e decisões remanescentes:

- necessidade futura de decisão sobre tratamento de falha do audit writer;
- possível necessidade de outbox/retry se auditoria crítica exigir garantia transacional forte;
- risco de ruído em auditoria de leitura/acesso negado se implementada sem planejamento;
- necessidade futura de minimização/paginação/listagens;
- decisões de produto sobre auditoria adicional ainda pendentes.

A continuidade deve evitar transformar audit log em repositório de payload clínico ou em fonte ruidosa sem critérios claros de evento, retenção, consulta e governança.

## 14. Fora do escopo desta fase

A Fase 7.3.6 não implementa:

- código;
- testes;
- migration;
- endpoint;
- campo novo;
- action nova;
- mudança transacional;
- frontend;
- infraestrutura.

Também não altera `MedicalRecord`, `ClinicalEvolution`, `Prescription`, authorization/policies, controllers, use cases, repositories, contracts/DTOs, domain entities, audit writer, Docker, Redis, RabbitMQ ou Kubernetes.

## 15. Critérios finais de aceite

A trilha 7.3 é considerada encerrada se:

- documentos 7.3.1 a 7.3.5 forem consolidados;
- autoria mínima estiver documentada;
- audit log mínimo estiver documentado;
- eventos auditados e fora do escopo estiverem claros;
- evidências de testes estiverem consolidadas;
- riscos remanescentes estiverem registrados;
- próxima fase for recomendada;
- nenhuma implementação nova for feita nesta fase;
- somente documentação for alterada;
- `git diff --check` passar.

## 16. Decisão final da Fase 7.3

A Fase 7.3 fica encerrada como a trilha de autoria e auditoria mínima da vertical `Attendance`, adicionando autoria técnica persistida para criação, fechamento e cancelamento e audit log clínico mínimo para `Attendance.Created`, `Attendance.Closed` e `Attendance.Canceled`, sem payload clínico sensível e sem endpoint público de auditoria.

O encerramento da Fase 7.3 não significa auditoria completa de todos os acessos e eventos possíveis de `Attendance`. Leitura, acesso negado, update genérico, reabertura, exclusão, restauração, transação atômica e estratégia de retry/outbox permanecem como possíveis fases futuras, mediante decisão técnica e de produto.

## 17. Próxima fase recomendada

Próxima fase macro recomendada:

```text
Fase 7.4 — Integração segura de ClinicalEvolution com Attendance
```

Fracionamento sugerido:

```text
7.4.1 — Planejamento técnico da integração ClinicalEvolution com Attendance
7.4.2 — Contratos/base técnica para ClinicalEvolution vinculada a Attendance
7.4.3 — Implementação mínima de ClinicalEvolution vinculada a Attendance
7.4.4 — Autoria/auditoria mínima de ClinicalEvolution
7.4.5 — Testes e evidências da integração ClinicalEvolution
7.4.6 — Encerramento da trilha ClinicalEvolution
```

Nada da Fase 7.4 é implementado neste documento. A recomendação apenas orienta a continuidade incremental da expansão clínica e operacional pós-hardening `MedicalRecord`.
