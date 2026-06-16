# Fase 7.1 — Planejamento técnico da vertical Attendance pós-hardening MedicalRecord

## 1. Objetivo

A Fase 7.1 planeja tecnicamente a evolução da vertical `Attendance` após o hardening de `MedicalRecord` concluído na Fase 6. O objetivo é revisar o estado atual do atendimento clínico, compará-lo com o padrão de maturidade já consolidado em `MedicalRecord`, registrar lacunas candidatas e orientar uma evolução futura segura, incremental e auditável.

Esta fase é exclusivamente documental: não implementa código, migrations, endpoints, permissões, alterações de domínio, infraestrutura ou frontend.

## 2. Contexto pós-Fase 6 e Fase 7.0

A Fase 6 endureceu `MedicalRecord` como vertical clínica sensível, consolidando autorização granular por operação, autoria, `ClinicalAuditLog`, Soft Delete, retenção conservadora, cascades restritivos, unicidade física, validação estrutural, propagação de `CancellationToken` e evidência manual versionada.

A Fase 7.0 abriu a expansão clínica-operacional e recomendou `Attendance` como próximo eixo, pois o atendimento já existe no domínio, aplicação, infraestrutura e API, enquanto `ClinicalEvolution` e `Prescription` já apontam para `Attendance` como vínculo natural.

Premissas preservadas nesta fase:

- `Patient` continua sendo o centro do histórico clínico.
- `MedicalRecord` continua sendo a memória clínica longitudinal consolidada.
- `Attendance` deve continuar sendo evento/caso clínico independente, associado ao paciente.
- `MedicalRecord` não deve ser inflado com lista de atendimentos, evolução, prescrição, agenda, estoque ou financeiro.
- `Attendance` não deve ser transformado em prontuário longitudinal.
- Evoluções clínicas e prescrições futuras devem compor o histórico do paciente por meio do atendimento, sem acoplar indevidamente todos os conceitos em uma única entidade.

## 3. Estado atual da vertical Attendance

### 3.1 Fontes documentais consultadas

Foram consultadas as fontes obrigatórias abaixo:

- `docs/clinical-core/PHASE_07_00_CLINICAL_OPERATIONAL_EXPANSION_PLANNING.md`.
- `docs/clinical-core/PHASE_06_06_05_MEDICAL_RECORD_PHASE_06_CLOSURE.md`.
- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`.
- `docs/clinical-core/PHASE_04_02_06_ATTENDANCE_DOMAIN_FINAL_DOCUMENTATION.md`.
- `docs/clinical-core/PHASE_04_03_07_ATTENDANCE_APPLICATION_FINAL_DOCUMENTATION.md`.
- `docs/clinical-core/PHASE_04_04_05_ATTENDANCE_INFRASTRUCTURE_FINAL_DOCUMENTATION.md`.
- `docs/clinical-core/PHASE_04_05_05_ATTENDANCE_API_FINAL_DOCUMENTATION.md`.
- `docs/clinical-core/PHASE_04_05_06_ATTENDANCE_HTTP_E2E_EXECUTION_EVIDENCE.md`.
- `docs/clinical-core/PHASE_04_06_01_ATTENDANCE_FINAL_AUDIT.md`.

Não houve ausência documental entre os documentos obrigatórios da Fase 4 listados para `Attendance`.

### 3.2 Entidade atual

A entidade `Attendance` possui atualmente:

- `Id`.
- `PatientId`.
- `AttendanceNumber`.
- `OpenedAt`.
- `ClosedAt`.
- `Status`.
- `Type`.

A fábrica `Attendance.Create(patientId, attendanceNumber, openedAt, type)` valida `patientId`, `attendanceNumber` e `openedAt`, normaliza `AttendanceNumber` com `Trim()`, inicializa `ClosedAt` como `null`, define `Status` como `Open` e registra o `Type` informado.

### 3.3 Enum/status

O enum `AttendanceStatus` contém:

```text
Open = 1
Closed = 2
Canceled = 3
```

A entidade também usa `AttendanceType`, observado nos fluxos e testes, com exemplos como `Consultation` e `Emergency`.

### 3.4 Métodos de domínio e invariantes

Métodos atuais:

- `Create(...)` cria atendimento aberto.
- `Close(closedAt)` fecha atendimento aberto, rejeita atendimento cancelado, rejeita atendimento já fechado, exige `closedAt` válido e impede `closedAt` anterior a `OpenedAt`.
- `Cancel()` cancela atendimento aberto, rejeita atendimento fechado, rejeita atendimento já cancelado e mantém `ClosedAt` como `null`.

Invariantes atuais confirmadas:

- `PatientId` deve ser maior que zero.
- `AttendanceNumber` é obrigatório e é normalizado por trim.
- `OpenedAt` é obrigatório.
- `ClosedAt` é obrigatório apenas no fechamento.
- `ClosedAt` não pode ser anterior a `OpenedAt`.
- atendimento criado nasce `Open`.
- atendimento `Closed` não pode ser cancelado.
- atendimento `Canceled` não pode ser fechado.
- atendimento já fechado ou já cancelado não aceita repetição da mesma transição.

### 3.5 Use cases e contracts existentes

Contracts atuais:

- `CreateAttendanceRequest`.
- `CloseAttendanceRequest`.
- `AttendanceResponse`.
- `AttendanceListItemResponse`.

Use cases atuais:

- `CreateAttendanceUseCase`.
- `GetAttendanceByIdUseCase`.
- `ListAttendancesUseCase`.
- `CloseAttendanceUseCase`.
- `CancelAttendanceUseCase`.

Validadores atuais:

- `AttendancePatientExistsValidator`.
- `AttendanceNumberUniqueValidator`.
- `OpenAttendanceValidator`.

O fluxo de criação valida existência do paciente, unicidade do número do atendimento e ausência de outro atendimento aberto para o mesmo paciente antes de criar o registro.

### 3.6 Endpoints existentes

O controller `AttendancesController` está protegido por `[Authorize]` genérico e expõe:

- `GET /api/attendances` para listagem.
- `GET /api/attendances/{id}` para detalhe.
- `POST /api/attendances` para criação.
- `PATCH /api/attendances/{id}/close` para fechamento.
- `PATCH /api/attendances/{id}/cancel` para cancelamento.

Não foram observadas policies granulares por operação equivalentes às policies de `MedicalRecord`.

### 3.7 Repository e persistência atuais

`IAttendanceRepository` expõe:

- `GetByIdAsync`.
- `ListAsync`.
- `ListByPatientIdAsync`.
- `AddAsync`.
- `UpdateAsync`.
- `ExistsByAttendanceNumberAsync`.
- `HasOpenAttendanceForPatientAsync`.

`AttendanceRepository` usa EF Core com `AsNoTracking()` em consultas, ordena listagens por `OpenedAt` e `Id` descendentes, persiste via `SaveChangesAsync(cancellationToken)` e propaga `CancellationToken` nas operações assíncronas observadas.

A configuração EF atual de `Attendance` define tabela `Attendances`, chave `Id`, `PatientId` obrigatório, `AttendanceNumber` obrigatório com tamanho máximo 30, `OpenedAt`, `ClosedAt`, `Status` e `Type`, vínculo com `Patient` por `DeleteBehavior.Restrict`, índice em `PatientId`, índice único em `AttendanceNumber` e índice em `OpenedAt`.

### 3.8 Testes existentes

Foram identificados testes para `Attendance` em:

- `backend/src/Togo.Domain.Tests/AttendanceTests.cs`, cobrindo criação, validações, fechamento, cancelamento e transições inválidas.
- `backend/src/Togo.Application.Tests/Attendances`, cobrindo use cases e validators.
- `backend/src/Togo.Infrastructure.Tests/Repositories/AttendanceRepositoryTests.cs`, cobrindo persistência e consultas de repository.
- `backend/src/Togo.Infrastructure.Tests/Persistence/ClinicalCascadeDeleteBehaviorTests.cs`, cobrindo cascades restritivos em relações clínicas críticas.
- `backend/src/Togo.Api.Tests/Controllers/AttendancesControllerTests.cs`, cobrindo resultados HTTP do controller.

### 3.9 Documentação anterior existente

A Fase 4 possui documentação final de domínio, aplicação, infraestrutura, API, evidência HTTP E2E e auditoria final de `Attendance`. Esses documentos existem e servem como baseline histórico da vertical antes da revisão pós-hardening de `MedicalRecord`.

## 4. Ciclo de vida atual de Attendance

O ciclo de vida atual é:

```text
Open
Closed
Canceled
```

Transições permitidas:

| Origem | Destino | Permitida? | Evidência técnica atual |
| --- | --- | --- | --- |
| criação | `Open` | Sim | `Attendance.Create` define `Status = AttendanceStatus.Open` e `ClosedAt = null`. |
| `Open` | `Closed` | Sim | `Close(closedAt)` aceita fechamento quando o atendimento está aberto e `closedAt >= OpenedAt`. |
| `Open` | `Canceled` | Sim | `Cancel()` altera status para `Canceled` e mantém `ClosedAt = null`. |

Transições proibidas:

| Origem | Destino | Proibida? | Evidência técnica atual |
| --- | --- | --- | --- |
| `Closed` | `Canceled` | Sim | `Cancel()` lança conflito de domínio para atendimento fechado. |
| `Canceled` | `Closed` | Sim | `Close(closedAt)` lança conflito de domínio para atendimento cancelado. |
| `Closed` | `Closed` | Sim | `Close(closedAt)` rejeita atendimento já fechado. |
| `Canceled` | `Canceled` | Sim | `Cancel()` rejeita atendimento já cancelado. |

Confirmações:

- Atendimento nasce `Open`.
- Atendimento aberto pode ser fechado.
- Atendimento aberto pode ser cancelado.
- Atendimento fechado não pode ser cancelado.
- Atendimento cancelado não pode ser fechado.
- `OpenedAt` é preenchido na criação.
- `ClosedAt` é preenchido no fechamento.
- `ClosedAt` permanece `null` no cancelamento.

Não foi identificada regra atual de autoria, justificativa de cancelamento, usuário responsável por fechamento/cancelamento, auditoria transacional ou timestamps específicos de cancelamento.

## 5. Relação com Patient e MedicalRecord

`Attendance` se vincula diretamente a `Patient` por `PatientId`. Esse vínculo confirma o atendimento como evento/caso clínico do paciente.

`MedicalRecord` representa memória clínica longitudinal: dados consolidados, notas gerais e flags clínicas do paciente, com maturidade técnica maior após a Fase 6. `Attendance` representa episódio/caso operacional e clínico: abertura, fechamento ou cancelamento de uma interação clínica específica.

Diretrizes para evolução futura:

- Não transformar `MedicalRecord` em lista de atendimentos.
- Não transformar `Attendance` em prontuário longitudinal.
- Não mover dados longitudinais consolidados para `Attendance` sem decisão explícita.
- Não inflar `MedicalRecord` com evolução, prescrição, agenda, financeiro ou estoque.
- Permitir que futuras evoluções clínicas e prescrições componham o histórico do paciente por meio do vínculo com `Attendance` e, indiretamente, com `Patient`.

## 6. Relação com ClinicalEvolution e Prescription

Estado atual identificado:

- `ClinicalEvolution` possui `AttendanceId`, `RegisteredAt`, `Type` e `Text`.
- `Prescription` possui `AttendanceId`, `IssuedAt` e `Notes`.
- `PrescriptionItem` possui `PrescriptionId`, `ProductId`, `Quantity`, `Unit`, `Dosage` e `DurationDays`.
- `ClinicalEvolutionConfiguration` vincula `ClinicalEvolution` a `Attendance` com `DeleteBehavior.Restrict`.
- `PrescriptionConfiguration` vincula `Prescription` a `Attendance` com `DeleteBehavior.Restrict`.
- `PrescriptionItemConfiguration` vincula `PrescriptionItem` a `Prescription` com `DeleteBehavior.Cascade`.

Não foram identificados, nesta inspeção, repositories, use cases ou controllers próprios para `ClinicalEvolution`, `Prescription` ou `PrescriptionItem`. O estado atual dessas entidades é majoritariamente de domínio e configuração de persistência, sem API operacional própria.

Riscos de implementar evoluções ou prescrições antes de endurecer `Attendance`:

- registrar texto clínico sem autoria e sem auditoria de evento crítico;
- permitir prescrição sem ciclo de atendimento claramente governado;
- criar endpoints sensíveis sem policies granulares;
- expor dados clínicos em listagens amplas;
- acoplar prescrição clínica a estoque/produto antes de decisão explícita;
- exigir migrations novas sem revisar retenção, cascades e minimização.

## 7. Comparação com padrão MedicalRecord pós-Fase 6

| Dimensão | MedicalRecord após Fase 6 | Attendance atual | Lacuna provável |
| --- | --- | --- | --- |
| Autorização granular | Policies por operação para leitura, criação e atualização. | `[Authorize]` genérico no controller. | Definir policies por operação: listar, detalhar, criar, fechar e cancelar. |
| Autoria | `CreatedByUserId`, `CreatedAt`, `UpdatedByUserId`, `UpdatedAt`. | Não há autoria persistida específica. | Adicionar autoria de criação, fechamento e cancelamento em fase futura. |
| AuditLog | `ClinicalAuditLog` mínimo para criação e atualização. | Não há AuditLog específico para eventos de atendimento. | Registrar eventos críticos como criação, fechamento e cancelamento. |
| Soft Delete | Soft Delete clínico mínimo, sem endpoint público `DELETE`. | Não há Soft Delete; cancelamento é estado de domínio. | Decidir se cancelamento basta ou se haverá política de Soft Delete/retention administrativa. |
| Retenção | Retenção clínica inicial indefinida e sem expurgo automático. | Não há política documental específica pós-Fase 6 para `Attendance`. | Formalizar retenção de atendimentos e eventos vinculados. |
| Cascades | Relações clínicas críticas revisadas para `Restrict`. | `Attendance -> Patient`, `ClinicalEvolution -> Attendance` e `Prescription -> Attendance` em `Restrict`; `PrescriptionItem -> Prescription` em `Cascade`. | Confirmar se a composição de itens por prescrição justifica cascade e manter revisão a cada nova relação. |
| Unicidade/integridade | Índice único físico em `MedicalRecords.PatientId`, conflito traduzido. | Índice único em `AttendanceNumber`; regra lógica de um atendimento aberto por paciente; sem constraint física parcial para aberto. | Avaliar concorrência e conflito físico/lógico para número e atendimento aberto por paciente. |
| Validação estrutural | `FlagsJson` validado como objeto JSON. | Validações simples de id, texto obrigatório, datas e transição de status. | Planejar validações estruturais para payload clínico futuro de evoluções/prescrições. |
| CancellationToken | Propagado até EF Core na vertical `MedicalRecord`. | Use cases, controller e repository já aceitam e repassam `CancellationToken` nas rotas atuais. | Confirmar cobertura total e preservar padrão em futuras entidades vinculadas. |
| Evidência manual versionada | Roteiro manual versionado e sanitizado para API/Swagger. | Existe evidência da Fase 4, anterior ao hardening de `MedicalRecord`. | Atualizar evidência manual pós-hardening para fluxo `Attendance`. |
| Documentação de encerramento | Encerramento formal da Fase 6. | Auditoria final da Fase 4 existe; não há encerramento pós-Fase 6 para trilha 7.x. | Criar encerramento futuro da trilha 7.x após hardening e integrações. |

## 8. Lacunas técnicas candidatas

| Lacuna candidata | Classificação | Observação para fase futura |
| --- | --- | --- |
| Autorização granular por operação | Obrigatório antes de expansão clínica | Deve preceder endpoints de evolução/prescrição e reduzir dependência de `[Authorize]` genérico. |
| Autoria de criação/fechamento/cancelamento | Obrigatório antes de expansão clínica | Eventos críticos precisam de usuário e timestamp rastreáveis. |
| AuditLog de eventos críticos | Obrigatório antes de expansão clínica | Criação, fechamento e cancelamento devem ser auditáveis antes de dados clínicos derivados. |
| Política de cancelamento, Soft Delete e retenção | Obrigatório antes de expansão clínica | Definir se cancelamento é estado clínico suficiente e qual retenção se aplica a atendimentos. |
| Revisão contínua de `DeleteBehavior.Cascade` | Obrigatório antes de expansão clínica | Relações atuais críticas estão restritivas; novas relações devem seguir o mesmo padrão. |
| Propagação de `CancellationToken` | Importante para maturidade | O fluxo atual parece propagar token; deve ser mantido e testado em novas verticais. |
| Evidência manual versionada atualizada | Importante para maturidade | A evidência da Fase 4 deve ser renovada após hardening operacional. |
| Resposta minimizada para listagens | Obrigatório antes de expansão clínica | Listagens futuras não devem expor payload clínico sensível. |
| Paginação/filtros | Importante para maturidade | `GET /api/attendances` lista sem paginação/filtros explícitos. |
| Proteção de payload clínico futuro | Obrigatório antes de expansão clínica | Evolução e prescrição exigirão minimização e autorização própria. |
| Relação segura com evoluções e prescrições | Obrigatório antes de expansão clínica | Só implementar depois do hardening mínimo de `Attendance`. |
| Tratamento de conflito concorrente para `AttendanceNumber` | Importante para maturidade | Índice único existe; convém avaliar tradução consistente para conflito de negócio. |
| Constraint física para um atendimento aberto por paciente | Pode ser posterior | A regra lógica existe; constraint parcial pode depender de banco e estratégia futura. |
| Justificativa de cancelamento | Pode ser posterior | Pode ser requisito clínico-operacional, mas deve ser planejado antes de uso real amplo. |

## 9. Riscos de evoluir Attendance sem planejamento

- Criar evolução clínica sobre atendimento sem autoria/auditoria.
- Permitir prescrição sem ciclo de atendimento bem definido.
- Expor listagem de atendimentos com dados demais.
- Misturar consulta clínica, agenda e financeiro.
- Criar migrations sem revisar cascades.
- Repetir débitos já resolvidos em `MedicalRecord`.
- Introduzir endpoints sem policies específicas.
- Acoplar prescrição clínica a produto/estoque antes de decisão explícita.
- Tratar cancelamento como exclusão sem política de retenção.
- Criar respostas de API difíceis de minimizar depois que dados clínicos sensíveis forem adicionados.

## 10. Sequência recomendada da trilha Attendance

Sequência recomendada para a trilha 7.x:

```text
7.1 — Planejamento técnico da vertical Attendance pós-hardening MedicalRecord
7.2 — Hardening operacional mínimo de Attendance
7.3 — Autoria e auditoria de eventos críticos de Attendance
7.4 — Integração segura de ClinicalEvolution com Attendance
7.5 — Integração segura de Prescription com Attendance
7.6 — Evidências finais e encerramento da primeira entrega clínica operacional
```

Justificativa: a inspeção confirma que `Attendance` já possui entidade, use cases, repository, controller e testes, mas ainda não tem maturidade equivalente à consolidada em `MedicalRecord` para autorização granular, autoria, auditoria, política documental pós-hardening e evidência atualizada. Portanto, é mais seguro endurecer `Attendance` antes de implementar `ClinicalEvolution` ou `Prescription` como fluxos operacionais.

## 11. Recomendação para a Fase 7.2

Próxima fase executável recomendada:

```text
Fase 7.2 — Hardening operacional mínimo de Attendance
```

Objetivo sugerido:

```text
Revisar e ajustar a vertical Attendance para alinhar autorização, CancellationToken, evidência manual, respostas e governança operacional ao padrão consolidado em MedicalRecord, sem ainda implementar ClinicalEvolution ou Prescription.
```

Não parece necessária uma fase documental intermediária antes da 7.2, desde que a Fase 7.2 permaneça incremental, preserve escopo estrito e não implemente evolução clínica nem prescrição.

## 12. Fora do escopo da Fase 7.1

Esta fase não implementa:

- código;
- testes;
- migrations;
- endpoints;
- mudanças de schema;
- alterações no domínio;
- alterações em repository;
- alterações em controller;
- novas permissões;
- frontend;
- infraestrutura;
- vínculo novo entre `Attendance` e `MedicalRecord`;
- evolução clínica;
- prescrição;
- exames;
- vacinas;
- agenda;
- financeiro;
- estoque.

## 13. Critérios de aceite da Fase 7.1

A Fase 7.1 será considerada concluída se:

- documento da Fase 7.1 for criado;
- fontes obrigatórias forem consultadas;
- estado atual de `Attendance` for documentado;
- ciclo de vida atual for descrito;
- relação com `Patient` e `MedicalRecord` for explicada;
- relação com `ClinicalEvolution` e `Prescription` for analisada;
- comparação com `MedicalRecord` pós-Fase 6 for criada;
- lacunas candidatas forem mapeadas;
- riscos forem registrados;
- sequência recomendada for definida;
- próxima fase recomendada for registrada;
- nenhuma implementação for feita;
- nenhuma migration for criada;
- somente documentação for alterada;
- `git diff --check` passar.

## 14. Decisão final

A Fase 7.1 aprova o início da trilha `Attendance` como próximo eixo clínico-operacional da Fase 7, mas condiciona qualquer implementação futura a planejamento incremental, revisão de segurança, autoria, auditoria, retenção, minimização de respostas e preservação da separação entre `Patient`, `MedicalRecord` e `Attendance`.
