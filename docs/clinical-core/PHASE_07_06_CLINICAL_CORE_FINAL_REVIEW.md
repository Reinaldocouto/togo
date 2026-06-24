# Fase 7.6 — Revisão final da Fase 7 / Clinical Core

## Objetivo e limite da revisão

Esta revisão consolida a entrega da Fase 7 / Clinical Core após a conclusão da vertical **Prescription** na sequência 7.5.3.x. O documento registra o estado final de **Attendance**, **MedicalRecord**, **ClinicalEvolution** e **Prescription** sem ampliar escopo funcional.

Não foram introduzidos endpoints, fluxos, entidades, migrations, repositories, regras de negócio, integrações com estoque/produto, PDF, assinatura, impressão, receituário ou frontend nesta fase.

## Módulos clínicos entregues

| Módulo | Estado entregue | Observações |
| --- | --- | --- |
| Attendance | Entregue como agregado operacional clínico mínimo. | Possui criação, consulta, listagem, fechamento e cancelamento, autoria mínima e AuditLog clínico mínimo. |
| MedicalRecord | Entregue como prontuário por paciente. | Possui criação, consulta e atualização, autorização granular, autoria, AuditLog clínico, soft delete e validação estrutural de `FlagsJson`. |
| ClinicalEvolution | Entregue como evolução clínica vinculada a Attendance. | Possui criação e listagem por atendimento, autoria mínima, AuditLog mínimo e escopo seguro por `attendanceId`. |
| Prescription | Entregue como prescrição vinculada a Attendance. | Possui criação e listagem mínima por atendimento, autoria mínima, AuditLog mínimo e contratos públicos minimizados. |

## Endpoints públicos disponíveis

### Attendance

- `GET /api/attendances` — lista atendimentos.
- `GET /api/attendances/{id}` — consulta atendimento por identificador.
- `POST /api/attendances` — cria atendimento.
- `PATCH /api/attendances/{id}/close` — fecha atendimento.
- `PATCH /api/attendances/{id}/cancel` — cancela atendimento.

### MedicalRecord

- `GET /api/patients/{patientId}/medical-record` — consulta prontuário do paciente.
- `POST /api/patients/{patientId}/medical-record` — cria prontuário do paciente.
- `PUT /api/patients/{patientId}/medical-record` — atualiza prontuário do paciente.

### ClinicalEvolution

- `GET /api/attendances/{attendanceId}/clinical-evolutions` — lista evoluções clínicas do atendimento.
- `POST /api/attendances/{attendanceId}/clinical-evolutions` — cria evolução clínica no atendimento.

### Prescription

- `GET /api/attendances/{attendanceId}/prescriptions` — lista prescrições mínimas do atendimento.
- `POST /api/attendances/{attendanceId}/prescriptions` — cria prescrição no atendimento.

## Endpoints ausentes intencionalmente

- Não há rota global `GET /api/prescriptions`, `GET /api/prescriptions/{id}` ou listagem geral de prescrições.
- Não há endpoint público para atualização, cancelamento, exclusão ou impressão de Prescription.
- Não há endpoint público para atualização, patch ou exclusão de ClinicalEvolution.
- Não há listagem global pública de MedicalRecord com conteúdo sensível.
- Não há endpoint de PDF, assinatura digital, impressão, receituário ou emissão fiscal/sanitária.
- Não há endpoint de Product, Stock ou Inventory conectado à vertical Prescription.

## Policies aplicadas

Todos os controllers clínicos são protegidos por autenticação no nível da classe e policies nos endpoints sensíveis.

| Módulo | Policies usadas |
| --- | --- |
| Attendance | `Attendance.Read`, `Attendance.Create`, `Attendance.Close`, `Attendance.Cancel`. |
| MedicalRecord | `MedicalRecord.Read`, `MedicalRecord.Create`, `MedicalRecord.Update`. |
| ClinicalEvolution | `ClinicalEvolution.Read`, `ClinicalEvolution.Create`. |
| Prescription | `Prescription.Read`, `Prescription.Create`. |

As policies de update/cancel eventualmente declaradas para módulos futuros não implicam endpoint público quando a rota correspondente não existe nesta fase.

## Regras de AuditLog existentes

O AuditLog clínico é mínimo e registra evento, entidade, usuário, perfil e metadados reduzidos quando aplicável.

| Módulo | Ações registradas no contrato atual |
| --- | --- |
| Attendance | `Attendance.Created`, `Attendance.Closed`, `Attendance.Canceled`. |
| MedicalRecord | `MedicalRecord.Created`, `MedicalRecord.Updated`, `MedicalRecord.Read`, `MedicalRecord.AccessDenied`. |
| ClinicalEvolution | `ClinicalEvolution.Created`, `ClinicalEvolution.Updated` como constante reservada; nesta fase a API pública cria e lista, sem endpoint público de update. |
| Prescription | `Prescription.Created`; constantes `Prescription.Updated` e `Prescription.Canceled` permanecem reservadas para evolução futura, sem endpoint público nesta fase. |

Os metadados de Prescription são minimizados: o evento de criação registra o vínculo com `AttendanceId`, sem copiar notas clínicas, posologia, itens, produto, quantidade ou duração para o AuditLog.

## Contratos públicos expostos

Contratos de entrada/saída considerados públicos para a API clínica atual:

- Attendance:
  - `CreateAttendanceRequest`
  - `CloseAttendanceRequest`
  - `AttendanceResponse`
  - `AttendanceListItemResponse`
- MedicalRecord:
  - `CreateMedicalRecordRequest`
  - `UpdateMedicalRecordRequest`
  - `MedicalRecordResponse`
  - `MedicalRecordListItemResponse` como contrato minimizado quando houver listagem/uso público mínimo.
- ClinicalEvolution:
  - `CreateClinicalEvolutionRequest`
  - `ClinicalEvolutionResponse`
  - `ClinicalEvolutionListItemResponse`
- Prescription:
  - `CreatePrescriptionRequest`
  - `CreatePrescriptionItemRequest`
  - `PrescriptionListItemResponse`
  - `PrescriptionCreatedResponse`

## Contratos internos que não devem ser expostos diretamente

Os contratos abaixo são internos ou sensíveis para uso direto em endpoints públicos amplos:

- `PrescriptionResponse` e `PrescriptionItemResponse`, por conterem notas, itens, dosagem, produto, quantidade, unidade e duração.
- `PrescriptionItemDraft` e `PrescriptionListItemProjection`, por pertencerem à fronteira de repository/application interna.
- Entidades de domínio (`Attendance`, `MedicalRecord`, `ClinicalEvolution`, `Prescription`, `PrescriptionItem`, `ClinicalAuditLog`) não devem ser serializadas diretamente pela API.
- Projections de repository não devem virar payload público sem mapeamento explícito.
- Dados de autoria (`CreatedByUserId`, `UpdatedByUserId`) e flags operacionais (`IsDeleted`) não devem ser expostos sem decisão funcional específica.

## Regras de minimização de dados

- Listagens de Prescription retornam apenas `Id`, `AttendanceId`, `IssuedAt` e `ItemCount`.
- A resposta de criação de Prescription retorna apenas `Id`, `AttendanceId`, `IssuedAt` e `ItemCount`, evitando ecoar notas e itens sensíveis.
- Listagens de ClinicalEvolution não retornam o texto clínico completo; o texto é restrito ao contrato de resposta de criação/detalhe já previsto.
- `MedicalRecordListItemResponse` usa indicadores (`HasGeneralNotes`, `HasFlags`) em vez de conteúdo clínico quando houver payload de lista mínimo.
- Erros de validação sensíveis não devem ecoar conteúdo clínico fornecido pelo usuário.
- AuditLog deve registrar metadados mínimos de rastreabilidade e não conteúdo clínico integral.

## Ausência de integração com Product/Stock/Inventory

Prescription aceita `ProductId` opcional apenas como referência informacional no item de entrada/contrato interno atual. A Fase 7 não valida catálogo, não reserva estoque, não baixa inventário, não consulta Product, não cria movimentação de Stock e não integra Inventory.

Qualquer integração com Product/Stock/Inventory deve ser planejada em macrofase específica, com regras transacionais, auditoria, autorização e consistência próprias.

## Ausência de PDF, assinatura e impressão

A Fase 7 não entrega geração de PDF, assinatura digital/eletrônica, impressão, layout de receituário, QR Code, carimbo, numeração externa ou conformidade regulatória de prescrição impressa.

Esses itens continuam fora do escopo e devem ser tratados como produto/regulatório em fase posterior.

## Riscos remanescentes

- Escopo de autorização ainda é baseado em perfil/policy; regras contextuais por clínica, unidade, tutor, profissional responsável ou vínculo assistencial ainda não foram implementadas.
- AuditLog é mínimo e pode precisar de enriquecimento para trilhas regulatórias futuras.
- Não há versionamento imutável de conteúdo clínico para todas as alterações futuras.
- Prescription ainda não tem fluxo público de cancelamento/retificação; quando existir, exigirá regras de rastreabilidade e impedimento de alteração indevida.
- MedicalRecord ainda expõe conteúdo completo em consulta direta autorizada; controles adicionais podem ser necessários para cenários multiunidade ou multi-tenant.
- ClinicalEvolution não possui fluxo de correção/adendo; qualquer evolução futura deve preservar histórico e autoria.
- A ausência de integração com estoque evita acoplamento prematuro, mas significa que ProductId não garante disponibilidade, validade, lote ou rastreabilidade de medicamento.

## Recomendações para a próxima macrofase

1. Definir modelo de autorização contextual além de perfil: clínica/unidade, profissional, vínculo com atendimento e escopo do paciente.
2. Planejar trilha de auditoria regulatória ampliada, com critérios de retenção, consulta administrativa e proteção contra exposição de conteúdo sensível.
3. Projetar fluxos clínicos de retificação/cancelamento para Prescription e ClinicalEvolution antes de disponibilizar endpoints de alteração.
4. Avaliar versionamento de MedicalRecord e eventos clínicos para histórico imutável.
5. Planejar integração Product/Stock/Inventory somente após decisão transacional clara, incluindo reserva, baixa, lote, validade e rollback.
6. Planejar PDF/assinatura/impressão como macrofase própria, incluindo requisitos legais e UX operacional.
7. Manter guardrails automatizados para impedir rotas globais sensíveis, DTOs internos como resposta pública e remoção acidental de policies.

## Evidências de guarda adicionadas na Fase 7.6

Foi adicionada uma suíte consolidada de guardrails para verificar que:

- controllers clínicos exigem autenticação;
- endpoints clínicos sensíveis possuem a policy esperada;
- Prescription permanece sem rota global e vinculado a Attendance;
- MedicalRecord não expõe autoria/soft delete no contrato público direto;
- ClinicalEvolution permanece escopado por Attendance e sem endpoints de mutação além de criação;
- contratos públicos mínimos não usam diretamente payloads internos sensíveis quando já existe contrato público próprio.
