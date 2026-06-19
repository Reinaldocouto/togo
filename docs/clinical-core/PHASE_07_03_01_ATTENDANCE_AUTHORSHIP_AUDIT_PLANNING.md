# TOGO — Fase 7.3.1: Planejamento técnico de autoria e auditoria de Attendance

## 1. Objetivo

Esta fase planeja, de forma exclusivamente documental, a autoria e a auditoria dos eventos críticos de `Attendance`.

O objetivo é definir o desenho técnico inicial para:

- identificar quais eventos de `Attendance` exigem autoria explícita;
- identificar quais eventos devem gerar `ClinicalAuditLog`;
- propor campos mínimos futuros para autoria;
- propor ações mínimas futuras de auditoria;
- preservar a minimização de dados sensíveis;
- reutilizar a base já existente de usuário atual e auditoria clínica criada na trilha de `MedicalRecord`;
- fracionar a implementação futura da Fase 7.3 sem alterar código nesta etapa.

Esta fase não implementa campos, migrations, código, testes, endpoints, actions ou writers de auditoria.

## 2. Contexto

A Fase 6 consolidou a trilha de autoria/auditoria de `MedicalRecord`, incluindo autoria técnica (`CreatedByUserId`, `CreatedAt`, `UpdatedByUserId`, `UpdatedAt`), resolução de usuário atual por `ICurrentUserService`, exceção de falha segura com `CurrentUserResolutionException`, ações `MedicalRecord.Created`/`MedicalRecord.Updated` e persistência de eventos em `ClinicalAuditLog` com metadata minimizada.

A Fase 7 iniciou a expansão clínica e operacional pós-hardening de `MedicalRecord`. A Fase 7.2 encerrou o hardening mínimo de autorização da vertical `Attendance`, consolidando permissões centralizadas, policies por operação, matriz por profile, proteção por action no `AttendancesController`, testes de permissões/policies/matriz/attributes e documentação de encerramento.

Com a autorização granular de `Attendance` já planejada, implementada, testada e encerrada, a próxima lacuna é a rastreabilidade dos eventos críticos de ciclo de vida de atendimento. Criação, fechamento e cancelamento são eventos operacionais e clínicos sensíveis porque registram abertura de episódio, encerramento de atendimento e interrupção/cancelamento de um fluxo assistencial.

## 3. Estado atual de Attendance

### 3.1 Propriedades atuais

A entidade `Attendance` possui atualmente:

- `Id`;
- `PatientId`;
- `AttendanceNumber`;
- `OpenedAt`;
- `ClosedAt`;
- `Status`;
- `Type`.

A factory `Attendance.Create(long patientId, string attendanceNumber, DateTime openedAt, AttendanceType type)` valida `PatientId`, `AttendanceNumber` e `OpenedAt`, normaliza `AttendanceNumber`, define `ClosedAt = null`, define `Status = AttendanceStatus.Open` e persiste o `Type` informado.

### 3.2 Timestamps atuais

O estado atual tem:

| Timestamp | Presença atual | Observação |
| --- | --- | --- |
| `OpenedAt` | Presente | Obrigatório no domínio e na configuração EF; representa a abertura operacional/clínica informada. |
| `ClosedAt` | Presente | Nullable; preenchido no fechamento; limpo no cancelamento. |
| `CanceledAt` | Ausente | O cancelamento muda `Status` para `Canceled`, mas não registra quando ocorreu. |
| `CreatedAt` | Ausente | Não há timestamp técnico de persistência/criação. |
| `UpdatedAt` | Ausente | Não há timestamp técnico da última alteração. |

`OpenedAt` não deve ser tratado automaticamente como equivalente a `CreatedAt`. `OpenedAt` pode representar um horário operacional/clínico informado no request, enquanto `CreatedAt` deve representar o instante técnico de persistência no sistema. A equivalência entre ambos não deve ser presumida.

### 3.3 Eventos e transições existentes

O ciclo atual de `AttendanceStatus` contempla:

- `Open`;
- `Closed`;
- `Canceled`.

As transições implementadas no domínio são:

| Operação | Origem esperada | Destino | Efeito atual |
| --- | --- | --- | --- |
| `Create` | Novo registro | `Open` | Define `OpenedAt`, `ClosedAt = null`, `Status = Open`. |
| `Close` | `Open` | `Closed` | Valida `ClosedAt`, impede fechamento antes de `OpenedAt`, preenche `ClosedAt`, define `Status = Closed`. |
| `Cancel` | `Open` | `Canceled` | Impede cancelar fechado ou já cancelado, define `Status = Canceled`, limpa `ClosedAt`. |

Não foram identificadas operações reais atuais de `Reopen`, `Delete`, `Restore`, `Read` auditado ou `AccessDenied` auditado para `Attendance`.

### 3.4 Ausência/presença de autoria

`Attendance` ainda não possui autoria. Não foram encontrados campos como:

- `CreatedByUserId`;
- `CreatedAt`;
- `UpdatedByUserId`;
- `UpdatedAt`;
- `ClosedByUserId`;
- `CanceledByUserId`;
- `CanceledAt`;
- `OpenedByUserId`.

Os use cases atuais de `Attendance` também não dependem de `ICurrentUserService`.

### 3.5 Ausência/presença de AuditLog

Não há `AttendanceAuditActions` e não há escrita de `ClinicalAuditLog` nos fluxos atuais de criação, fechamento ou cancelamento de `Attendance`.

Também foi verificado que os arquivos citados como `backend/src/Togo.Domain/Enums/ClinicalAuditLogAction.cs` e `backend/src/Togo.Domain/Enums/ClinicalAuditLogEntityType.cs` não existem no estado atual do repositório. O padrão real observado usa strings centralizadas em `MedicalRecordAuditActions` e `ClinicalAuditEvent`, não enums de action/entity type.

### 3.6 Use cases afetados futuramente

Os fluxos que deverão ser avaliados nas próximas subfases são:

- `CreateAttendanceUseCase` para autoria técnica de criação e auditoria `Attendance.Created`;
- `CloseAttendanceUseCase` para autoria de fechamento, atualização de estado e auditoria `Attendance.Closed`;
- `CancelAttendanceUseCase` para autoria de cancelamento, timestamp de cancelamento, atualização de estado e auditoria `Attendance.Canceled`.

## 4. Referência ao padrão MedicalRecord

A trilha de `MedicalRecord` é a principal referência técnica para `Attendance`.

### 4.1 Autoria existente em MedicalRecord

`MedicalRecord` possui:

- `CreatedByUserId`;
- `CreatedAt`;
- `UpdatedByUserId`;
- `UpdatedAt`;
- `DeletedByUserId`;
- `DeletedAt`;
- validação de `Guid.Empty` para usuário;
- validação de data default;
- atualização explícita de autoria em `UpdateNotes`;
- falha de domínio quando a autoria é inválida.

### 4.2 Resolução de usuário atual

A base existente expõe:

- `ICurrentUserService.GetCurrentUser()`;
- `CurrentUserInfo(UserId, Profile, IsAuthenticated)`;
- `CurrentUserResolutionException` para falha segura quando não há usuário autenticado ou identificador válido;
- `HttpContextCurrentUserService`, que resolve `ClaimTypes.NameIdentifier` ou `sub` e lê o profile do claim da aplicação.

Essa base deve ser reaproveitada para `Attendance` nas subfases de implementação, sem criar uma segunda abstração de usuário atual.

### 4.3 AuditLog existente

A base existente expõe:

- `ClinicalAuditEvent` com `EntityName`, `EntityId`, `Action`, `UserId`, `UserProfile`, `OccurredAt` e `MetadataJson`;
- `IClinicalAuditLogWriter`;
- `EfClinicalAuditLogWriter`, que cria `ClinicalAuditLog` e chama `SaveChangesAsync`;
- entidade `ClinicalAuditLog` com validação de entity/action/user/occurredAt;
- `MedicalRecordAuditActions` com `MedicalRecord.Created`, `MedicalRecord.Updated`, `MedicalRecord.Read` e `MedicalRecord.AccessDenied`.

### 4.4 Eventos MedicalRecord e minimização de payload

Os use cases de criação e atualização de `MedicalRecord` escrevem respectivamente:

- `MedicalRecord.Created`;
- `MedicalRecord.Updated`.

A metadata observada é minimizada e contém `PatientId`, sem serializar texto clínico livre, request completo, `GeneralNotes` ou `FlagsJson`. Esse padrão deve orientar `Attendance`: registrar apenas identificadores e estado operacional estritamente necessário.

### 4.5 Observação transacional

O writer atual de auditoria usa o mesmo `AppDbContext`, mas executa `SaveChangesAsync` dentro do próprio writer após o repositório já persistir a alteração. Assim, a atomicidade entre alteração de negócio e log de auditoria deve ser analisada em subfase futura. Esta fase não resolve o desenho transacional.

## 5. Eventos críticos de Attendance

### 5.1 Eventos mínimos da primeira trilha

| Evento | Origem | Risco | Autoria necessária? | AuditLog necessário? | Observação |
| --- | --- | --- | --- | --- | --- |
| `Attendance.Created` | `CreateAttendanceUseCase` / `Attendance.Create` | Abertura de episódio sem identificação de quem criou; divergência entre `OpenedAt` e instante técnico de criação. | Sim | Sim | Deve registrar autoria técnica de criação e preservar `OpenedAt` como timestamp operacional/clínico. |
| `Attendance.Closed` | `CloseAttendanceUseCase` / `attendance.Close` | Encerramento de atendimento por usuário não rastreável; alteração crítica de estado sem autoria específica. | Sim | Sim | Deve distinguir quem fechou de quem criou/atualizou genericamente. |
| `Attendance.Canceled` | `CancelAttendanceUseCase` / `attendance.Cancel` | Cancelamento sem autor e sem timestamp próprio; perda de rastreabilidade operacional. | Sim | Sim | Deve registrar quem cancelou e quando cancelou, sem transformar cancelamento em fechamento. |

### 5.2 Eventos avaliados e fora do escopo inicial

| Evento | Decisão inicial | Justificativa |
| --- | --- | --- |
| `Attendance.Updated` | Fora do escopo inicial | Não há use case genérico de update no ciclo atual analisado para esta trilha. |
| `Attendance.Reopened` | Fora do escopo inicial | Não há operação real de reabertura. |
| `Attendance.Deleted` | Fora do escopo inicial | Não há operação real de exclusão de `Attendance` nesta trilha. |
| `Attendance.Restored` | Fora do escopo inicial | Não há soft delete/restore de `Attendance` nesta trilha. |
| `Attendance.Read` | Fora do escopo inicial | A primeira etapa deve priorizar eventos críticos de escrita/estado; leitura auditada pode ser fase posterior. |
| `Attendance.AccessDenied` | Fora do escopo inicial | A autorização de `Attendance` foi encerrada na Fase 7.2; auditoria de negação deve ser planejada separadamente para não reabrir escopo. |

## 6. Autoria mínima proposta

A recomendação é seguir a consistência de `MedicalRecord` para autoria técnica geral, adicionando autoria explícita para fechamento e cancelamento.

| Campo | Propósito | Obrigatório? | Fase sugerida | Observação |
| --- | --- | --- | --- | --- |
| `CreatedByUserId` | Identificar quem criou tecnicamente o registro. | Sim para novos registros após implementação | 7.3.3 | Alinha com `MedicalRecord`; não substitui `OpenedAt`. |
| `CreatedAt` | Registrar instante técnico de persistência/criação. | Sim para novos registros após implementação | 7.3.3 | Deve ser UTC; não deve presumir igualdade com `OpenedAt`. |
| `UpdatedByUserId` | Identificar o último usuário que alterou estado/dados auditáveis. | Sim para novos registros após implementação | 7.3.3 | Útil como trilha geral, mas insuficiente para distinguir fechamento/cancelamento. |
| `UpdatedAt` | Registrar instante técnico da última alteração. | Sim para novos registros após implementação | 7.3.3 | Deve ser UTC; será atualizado em fechamento/cancelamento. |
| `ClosedByUserId` | Identificar quem fechou o atendimento. | Sim quando `Status = Closed` | 7.3.3 | Campo específico evita esconder o evento em `UpdatedByUserId`. |
| `CanceledByUserId` | Identificar quem cancelou o atendimento. | Sim quando `Status = Canceled` | 7.3.3 | Campo específico evita ambiguidade com fechamento ou update genérico. |
| `CanceledAt` | Registrar quando o cancelamento ocorreu. | Sim quando `Status = Canceled` | 7.3.3 | O estado atual não possui timestamp de cancelamento. |
| `OpenedByUserId` | Alternativa para autoria de abertura operacional. | Não recomendado na primeira decisão | Posterior, se necessário | Pode gerar ambiguidade com `CreatedByUserId`; avaliar apenas se abertura operacional for separada da criação técnica. |

## 7. Decisão recomendada de autoria

A decisão recomendada para as próximas subfases é:

> Usar `CreatedByUserId`/`CreatedAt` para autoria técnica de criação, preservar `OpenedAt` como timestamp operacional/clínico, usar `UpdatedByUserId`/`UpdatedAt` para última alteração de estado, e adicionar `ClosedByUserId`, `CanceledByUserId` e `CanceledAt` para eventos críticos de fechamento e cancelamento.

Justificativas:

- `OpenedAt` pode ser horário operacional informado e não necessariamente o instante técnico em que o registro foi persistido;
- criação, fechamento e cancelamento são eventos distintos e precisam de rastreabilidade própria;
- `UpdatedByUserId` sozinho pode esconder quem fechou ou cancelou quando houver múltiplas alterações;
- `ClosedByUserId` e `CanceledByUserId` tornam explícita a autoria dos eventos críticos;
- `CanceledAt` corrige lacuna atual, pois o cancelamento hoje não possui timestamp próprio;
- a consistência com `MedicalRecord` reduz o risco de criar um segundo padrão de autoria no domínio clínico.

## 8. AuditLog mínimo proposto

### 8.1 Ações mínimas futuras

As ações mínimas recomendadas são:

- `Attendance.Created`;
- `Attendance.Closed`;
- `Attendance.Canceled`.

A subfase 7.3.2 deve avaliar a criação de `AttendanceAuditActions` em `Togo.Application.Auditing`, seguindo o padrão de `MedicalRecordAuditActions`.

### 8.2 Metadata mínima permitida

A metadata deve ser minimizada e conter apenas informações necessárias para correlação operacional/auditável. Exemplo permitido:

```json
{
  "attendanceId": 123,
  "patientId": 456,
  "status": "Closed"
}
```

Também pode ser aceitável registrar apenas `PatientId` quando `EntityName = Attendance` e `EntityId = attendance.Id` já estiverem presentes no evento.

### 8.3 Conteúdo a evitar

Não registrar em `MetadataJson`:

- texto clínico;
- evolução clínica;
- prescrição;
- observações completas;
- payload completo do request;
- dados pessoais desnecessários;
- dados duplicados de Patient/Pet/Tutor;
- qualquer conteúdo que amplie exposição de dados sensíveis sem necessidade de auditoria.

## 9. Uso de ClinicalAuditLog existente

A recomendação é reutilizar `ClinicalAuditLog` para eventos críticos de `Attendance`.

Diretrizes propostas:

- não criar nova tabela de auditoria neste momento;
- ampliar a base de actions de forma controlada com `AttendanceAuditActions`;
- manter o formato `ClinicalAuditEvent` já existente;
- usar `EntityName = nameof(Attendance)` e `EntityId = attendance.Id.ToString()`;
- manter `UserId`, `UserProfile` e `OccurredAt` resolvidos pelo padrão existente;
- manter metadata mínima, sem payload sensível;
- manter sem endpoint público de auditoria por enquanto;
- registrar em subfase futura a decisão sobre atomicidade/transação entre persistência do atendimento e persistência do log.

Como os arquivos `ClinicalAuditLogAction.cs` e `ClinicalAuditLogEntityType.cs` não existem atualmente, qualquer menção futura a enums de action/entity type deve ser tratada como possível evolução técnica, não como reaproveitamento de código existente.

## 10. Impacto técnico futuro

### 10.1 Fase 7.3.2 — Contratos/base técnica de autoria e audit actions de Attendance

Escopo sugerido:

- criar `AttendanceAuditActions` com `Created`, `Closed` e `Canceled`;
- avaliar se haverá constantes auxiliares de entity name ou se será usado `nameof(Attendance)`;
- revisar necessidade de contratos auxiliares sem persistir campos;
- confirmar integração com `ICurrentUserService`, `CurrentUserInfo` e `IClinicalAuditLogWriter`;
- não adicionar campos em `Attendance`;
- não criar migration;
- não escrever audit log nos use cases ainda, salvo se a fase for explicitamente expandida.

### 10.2 Fase 7.3.3 — Implementação de autoria mínima de Attendance

Escopo provável:

- adicionar campos de autoria em `Attendance`;
- configurar EF para os novos campos;
- criar migration controlada;
- ajustar `CreateAttendanceUseCase`, `CloseAttendanceUseCase` e `CancelAttendanceUseCase`;
- usar `ICurrentUserService`;
- validar falha segura quando usuário atual não resolve;
- preservar `OpenedAt` como timestamp operacional/clínico.

### 10.3 Fase 7.3.4 — Implementação de AuditLog para eventos críticos de Attendance

Escopo provável:

- escrever `ClinicalAuditLog` para `Attendance.Created`;
- escrever `ClinicalAuditLog` para `Attendance.Closed`;
- escrever `ClinicalAuditLog` para `Attendance.Canceled`;
- usar `IClinicalAuditLogWriter`;
- garantir metadata minimizada;
- decidir/registrar o comportamento transacional observado.

### 10.4 Fase 7.3.5 — Testes e evidências de autoria/auditoria de Attendance

Escopo provável:

- testes de domínio para autoria de criação/fechamento/cancelamento;
- testes de application para uso de usuário atual;
- testes de audit writer/eventos quando aplicável;
- testes de API quando houver reflexo observável;
- evidências documentais;
- validação de falha segura quando o usuário atual não resolve;
- validação de ausência de payload clínico sensível em audit log.

### 10.5 Fase 7.3.6 — Encerramento da trilha de autoria/auditoria de Attendance

Escopo provável:

- consolidar decisões;
- registrar arquivos alterados;
- registrar testes/evidências;
- documentar limitações remanescentes;
- recomendar próxima trilha clínica/operacional.

## 11. Riscos

- Criar autoria sem diferenciar criação, fechamento e cancelamento.
- Tratar `OpenedAt` como `CreatedAt` sem decisão clara.
- Manter cancelamento sem `CanceledAt`.
- Usar apenas `UpdatedByUserId` e perder leitura direta de quem fechou/cancelou.
- Auditar payload clínico sensível ou request completo.
- Criar `AuditLog` sem decisão sobre transação/atomicidade com a operação de negócio.
- Criar migration grande demais ou misturar múltiplas preocupações em uma única fase.
- Misturar autoria/auditoria de `Attendance` com `ClinicalEvolution`, `Prescription` ou outras verticais clínicas ainda não envolvidas.
- Quebrar o fluxo existente de `Attendance` ao alterar assinatura de domínio/use cases sem cobertura adequada.
- Reabrir autorização de `Attendance` já encerrada na Fase 7.2 sem necessidade.
- Criar tabela nova de auditoria antes de esgotar o reaproveitamento de `ClinicalAuditLog`.
- Planejar eventos inexistentes (`Reopened`, `Deleted`, `Restored`) como se já fossem suportados.

## 12. Fora do escopo da Fase 7.3.1

Esta fase não implementa:

- campos;
- migrations;
- código C#;
- testes;
- `AuditLog`;
- writer;
- endpoint;
- alteração de enum;
- alteração de controller;
- alteração de use case;
- alteração de repository;
- alteração de domínio;
- alteração de autorização/JWT;
- frontend;
- infraestrutura;
- Docker;
- Redis;
- RabbitMQ;
- Kubernetes;
- evidência manual Swagger.

## 13. Critérios de aceite

A Fase 7.3.1 será aceita se:

- este documento for criado em `docs/clinical-core/PHASE_07_03_01_ATTENDANCE_AUTHORSHIP_AUDIT_PLANNING.md`;
- o estado atual de `Attendance` for inspecionado;
- a base de autoria/auditoria de `MedicalRecord` for usada como referência;
- eventos críticos de `Attendance` forem mapeados;
- autoria mínima for proposta;
- `AuditLog` mínimo for proposto;
- riscos forem documentados;
- subfases da Fase 7.3 forem propostas;
- a próxima fase 7.3.2 for recomendada;
- nenhuma implementação for feita;
- nenhuma migration for criada;
- somente documentação for alterada;
- `git diff --check` passar.

## 14. Próxima fase recomendada

Recomenda-se executar:

```text
Fase 7.3.2 — Contratos/base técnica de autoria e audit actions de Attendance
```

Objetivo sugerido:

```text
Criar a base técnica mínima para autoria/auditoria de Attendance, incluindo ações de auditoria e contratos auxiliares, sem ainda persistir campos de autoria nem escrever AuditLog.
```

## 15. Fontes consultadas

### 15.1 Documentos da Fase 7

- `docs/clinical-core/PHASE_07_00_CLINICAL_OPERATIONAL_EXPANSION_PLANNING.md`;
- `docs/clinical-core/PHASE_07_01_ATTENDANCE_TECHNICAL_PLANNING.md`;
- `docs/clinical-core/PHASE_07_02_01_ATTENDANCE_AUTHORIZATION_PLANNING.md`;
- `docs/clinical-core/PHASE_07_02_02_ATTENDANCE_AUTHORIZATION_IMPLEMENTATION.md`;
- `docs/clinical-core/PHASE_07_02_03_ATTENDANCE_AUTHORIZATION_TESTS.md`;
- `docs/clinical-core/PHASE_07_02_04_ATTENDANCE_AUTHORIZATION_CLOSURE.md`.

### 15.2 Documentos da Fase 6

- `docs/clinical-core/PHASE_06_03_01_MEDICAL_RECORD_AUTHORSHIP_AUDIT_PLANNING.md`;
- `docs/clinical-core/PHASE_06_03_02_CURRENT_USER_AND_AUDIT_CONTRACTS.md`;
- `docs/clinical-core/PHASE_06_03_03_MEDICAL_RECORD_AUTHORSHIP_IMPLEMENTATION.md`;
- `docs/clinical-core/PHASE_06_03_06_MEDICAL_RECORD_AUTHORSHIP_AUDIT_CLOSURE.md`.

Ausências observadas entre as fontes solicitadas:

- `docs/clinical-core/PHASE_06_03_04_MEDICAL_RECORD_AUDIT_LOG_IMPLEMENTATION.md`;
- `docs/clinical-core/PHASE_06_03_05_MEDICAL_RECORD_AUTHORSHIP_AUDIT_EVIDENCES.md`.

### 15.3 Arquivos técnicos de Attendance

- `backend/src/Togo.Domain/Entities/Attendance.cs`;
- `backend/src/Togo.Domain/Enums/AttendanceStatus.cs`;
- `backend/src/Togo.Application/Attendances/UseCases/CreateAttendanceUseCase.cs`;
- `backend/src/Togo.Application/Attendances/UseCases/CloseAttendanceUseCase.cs`;
- `backend/src/Togo.Application/Attendances/UseCases/CancelAttendanceUseCase.cs`;
- `backend/src/Togo.Application/Attendances/Repositories/IAttendanceRepository.cs`;
- `backend/src/Togo.Infrastructure/Repositories/AttendanceRepository.cs`;
- `backend/src/Togo.Api/Controllers/AttendancesController.cs`.

### 15.4 Arquivos técnicos de autoria/auditoria

- `backend/src/Togo.Application/Security/ICurrentUserService.cs`;
- `backend/src/Togo.Application/Security/CurrentUserInfo.cs`;
- `backend/src/Togo.Application/Security/CurrentUserResolutionException.cs`;
- `backend/src/Togo.Api/Security/HttpContextCurrentUserService.cs`;
- `backend/src/Togo.Application/Auditing/MedicalRecordAuditActions.cs`;
- `backend/src/Togo.Application/Auditing/ClinicalAuditEvent.cs`;
- `backend/src/Togo.Application/Auditing/IClinicalAuditLogWriter.cs`;
- `backend/src/Togo.Infrastructure/Auditing/EfClinicalAuditLogWriter.cs`;
- `backend/src/Togo.Domain/Entities/ClinicalAuditLog.cs`.

Ausências observadas entre os arquivos técnicos solicitados:

- `backend/src/Togo.Domain/Enums/ClinicalAuditLogAction.cs`;
- `backend/src/Togo.Domain/Enums/ClinicalAuditLogEntityType.cs`.

## 16. Inspeções executadas

Foram executadas as inspeções obrigatórias para confirmar o estado real antes deste planejamento:

```bash
rg -n "class Attendance|CreatedBy|UpdatedBy|OpenedAt|ClosedAt|Canceled|Cancel|Close|AttendanceStatus" backend/src/Togo.Domain backend/src/Togo.Application backend/src/Togo.Infrastructure backend/src/Togo.Api
```

```bash
rg -n "ICurrentUserService|CurrentUserInfo|CurrentUserResolutionException|HttpContextCurrentUserService" backend/src
```

```bash
rg -n "ClinicalAuditLog|IClinicalAuditLogWriter|EfClinicalAuditLogWriter|MedicalRecordAuditActions|AuditActions" backend/src
```

```bash
rg -n "MedicalRecord.Created|MedicalRecord.Updated|ClinicalAuditLogAction|ClinicalAuditLogEntityType" backend/src docs/clinical-core
```

```bash
rg -n "Attendance" docs/clinical-core backend/src/Togo.Application/Auditing backend/src/Togo.Domain/Enums
```
