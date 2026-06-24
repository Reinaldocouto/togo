# Fase 7.5.3.5 — Revisão final e consolidação da vertical Prescription

## Objetivo

Esta fase consolida a revisão final da vertical `Prescription`, sem ampliar escopo funcional. O foco é registrar os contratos entregues nas fases 7.5.3.x, reforçar os limites públicos da API, documentar decisões de segurança e manter guardrails por testes.

## Arquivos revisados

- `backend/src/Togo.Application/Prescriptions/**`.
- `backend/src/Togo.Infrastructure/Repositories/PrescriptionRepository.cs`.
- `backend/src/Togo.Api/Controllers/PrescriptionsController.cs`.
- `backend/src/Togo.Api/Models/PrescriptionCreatedResponse.cs`.
- Testes relacionados em `Togo.Domain.Tests`, `Togo.Application.Tests`, `Togo.Infrastructure.Tests` e `Togo.Api.Tests`.
- Documentação das fases `docs/clinical-core/PHASE_07_05_03_01_*` até `PHASE_07_05_03_04_*`.

## Endpoints disponíveis

A superfície pública atual de `Prescription` é restrita ao contexto de um atendimento (`Attendance`):

| Método | Rota | Policy | Resposta pública |
| --- | --- | --- | --- |
| `GET` | `/api/attendances/{attendanceId:long}/prescriptions` | `PrescriptionPolicies.Read` | Lista de `PrescriptionListItemResponse` |
| `POST` | `/api/attendances/{attendanceId:long}/prescriptions` | `PrescriptionPolicies.Create` | `PrescriptionCreatedResponse` |

Não existe rota global de prescrição como `/api/prescriptions`.

## Use cases disponíveis

- `CreatePrescriptionUseCase`: criação interna de prescrição vinculada a `Attendance` aberta, com validações mínimas, autoria via usuário autenticado e escrita de AuditLog mínimo.
- `ListPrescriptionsByAttendanceUseCase`: listagem mínima de prescrições por `AttendanceId`, retornando apenas dados resumidos e não sensíveis.

## Repository disponível

- Interface: `IPrescriptionRepository`.
- Implementação: `PrescriptionRepository`.
- Operações disponíveis:
  - `AddAsync(Prescription prescription, IReadOnlyList<PrescriptionItemDraft> items, CancellationToken cancellationToken = default)`.
  - `ListByAttendanceIdAsync(long attendanceId, CancellationToken cancellationToken = default)`.

O repository não expõe método de listagem global, como `ListAsync`, `GetAllAsync` ou equivalente para todas as prescrições.

## Contratos públicos expostos

### `PrescriptionCreatedResponse`

Usado exclusivamente na resposta pública do `POST`:

- `Id`.
- `AttendanceId`.
- `IssuedAt`.
- `ItemCount`.

### `PrescriptionListItemResponse`

Usado na resposta pública do `GET` por atendimento:

- `Id`.
- `AttendanceId`.
- `IssuedAt`.
- `ItemCount`.

## Contratos internos usados apenas na Application

Os contratos abaixo permanecem na camada de Application e não devem ser assumidos como contrato público seguro de API:

- `CreatePrescriptionRequest`: entrada de criação, contendo os dados necessários para montar a prescrição e seus itens.
- `CreatePrescriptionItemRequest`: entrada dos itens da prescrição.
- `PrescriptionResponse`: retorno interno completo do use case de criação, usado pelo controller apenas como fonte para mapear o contrato público seguro.
- `PrescriptionItemResponse`: item interno do retorno completo do use case.
- `PrescriptionItemDraft`: estrutura interna para persistência dos itens pelo repository.
- `PrescriptionListItemProjection`: projeção interna de repository para listagem mínima.

## Policies aplicadas

- `GET /api/attendances/{attendanceId:long}/prescriptions` exige `PrescriptionPolicies.Read`.
- `POST /api/attendances/{attendanceId:long}/prescriptions` exige `PrescriptionPolicies.Create`.
- A classe `PrescriptionsController` exige autenticação (`[Authorize]`).
- As policies `Update` e `Cancel` podem existir como constantes/registro de autorização, mas não há endpoint público nesta sequência para atualização ou cancelamento.

## Regras de AuditLog

Na criação de prescrição:

- Um evento de auditoria é escrito após persistência bem-sucedida.
- `EntityName` é `Prescription`.
- `Action` é `PrescriptionAuditActions.Created`.
- `EntityId` usa o identificador da prescrição criada.
- `UserId` e `UserProfile` vêm do usuário autenticado.
- `MetadataJson` registra apenas `AttendanceId`.

O AuditLog não deve registrar conteúdo clínico sensível dos itens, como notas, posologia, quantidade, unidade, produto ou duração.

## Dados permitidos em response público

As respostas públicas podem conter somente:

- `Id`.
- `AttendanceId`.
- `IssuedAt`.
- `ItemCount`.

## Dados proibidos em response público

As respostas públicas de criação e listagem não devem expor:

- `Notes`.
- `Dosage`.
- `Items` completos.
- `ProductId`.
- `Quantity`.
- `Unit`.
- `DurationDays`.
- Texto livre clínico informado na criação.

## Limites explícitos de escopo

A vertical consolidada nesta sequência mantém as seguintes ausências intencionais:

- Ausência de listagem global de prescrições.
- Ausência de endpoint de detalhe de prescrição.
- Ausência de update, delete ou cancelamento de prescrição.
- Ausência de geração de PDF, receituário, assinatura clínica ou impressão.
- Ausência de integração com `Product`, estoque, `Stock` ou `Inventory`.
- Ausência de migration/schema nesta sequência.
- Ausência de novas regras de negócio além das já aceitas nas fases anteriores.

## Segurança e minimização de dados

A API pública minimiza dados clínicos retornados. Mesmo que o fluxo interno de criação manipule `Notes`, itens, dosagem, produto, quantidade, unidade e duração para persistência, o controller mapeia a resposta do `POST` para `PrescriptionCreatedResponse`, impedindo que o contrato interno completo seja serializado ao cliente.

A listagem por atendimento usa `PrescriptionListItemResponse`, que mantém apenas dados identificadores e contagem de itens.

## Riscos remanescentes

- A existência de contratos internos completos na Application exige manutenção contínua dos testes de guarda para impedir exposição acidental por controllers.
- A ausência de migration/schema nesta sequência pressupõe que a infraestrutura necessária já esteja disponível ou será tratada em fluxo próprio.
- A falta de endpoint de detalhe impede consulta pública segura de conteúdo clínico completo; uma fase futura deverá definir RBAC, auditoria e contrato específico antes de expor detalhes.
- A falta de cancelamento/update mantém o ciclo de vida de prescrição propositalmente limitado nesta fase.
- Integrações com catálogo de produtos, estoque e impressão ainda precisam de desenho próprio para evitar acoplamento indevido e vazamento de dados sensíveis.

## Recomendação para próximas fases

- Definir, em fase separada, se haverá endpoint de detalhe e qual contrato público mínimo poderá expor conteúdo clínico.
- Planejar cancelamento/retificação com AuditLog completo e políticas específicas antes de qualquer alteração de ciclo de vida.
- Tratar PDF/assinatura/impressão como vertical própria, com controles de acesso, rastreabilidade e proteção contra reemissão indevida.
- Planejar integração com `Product`/estoque somente após contrato clínico estabilizado, evitando que a prescrição dependa de disponibilidade de inventário para existir.
- Manter testes de guarda de contratos públicos sempre que novos controllers ou DTOs forem adicionados.
