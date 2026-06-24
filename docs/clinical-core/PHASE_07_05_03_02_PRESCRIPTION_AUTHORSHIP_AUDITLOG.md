# TOGO — Fase 7.5.3.2: Autoria e AuditLog mínimos de Prescription no fluxo interno de criação

## Objetivo

Adicionar autoria mínima e AuditLog clínico mínimo ao fluxo interno de criação de `Prescription`, mantendo a vertical sem controller, rota ou endpoint público.

## Decisões implementadas

- `CreatePrescriptionUseCase` passou a depender de `ICurrentUserService` e `IClinicalAuditLogWriter`, seguindo o padrão já adotado em fluxos clínicos como `ClinicalEvolution`.
- A validação do request, a verificação de existência do `Attendance` e o bloqueio para atendimentos não abertos continuam ocorrendo antes de qualquer persistência de `Prescription`.
- A criação continua limitada a `AttendanceStatus.Open`.
- `ListPrescriptionsByAttendanceUseCase` permanece sem autoria e sem AuditLog nesta fase, pois não há exigência explícita de auditoria de leitura para este fluxo interno mínimo.

## Fluxo de autoria

1. O request é validado.
2. O `Attendance` é carregado.
3. O fluxo retorna `NotFound` se o atendimento não existir.
4. O fluxo retorna `Conflict` se o atendimento não estiver aberto.
5. Após essas garantias, o usuário atual é resolvido por `ICurrentUserService`.
6. A prescrição e seus itens são persistidos pelo `IPrescriptionRepository`.

Como as entidades `Prescription` e `PrescriptionItem` ainda não possuem campos de autoria próprios nesta fase, a autoria mínima fica registrada no evento clínico de auditoria por meio do `UserId` e do perfil do usuário quando disponível.

## Fluxo de AuditLog

Após a persistência bem-sucedida, o use case escreve um evento clínico via `IClinicalAuditLogWriter` com ação `PrescriptionAuditActions.Created`.

Falhas de validação, `NotFound` e `Conflict` retornam antes da criação da prescrição e antes da escrita de auditoria. Nesses cenários, o repositório de `Prescription` e o writer de AuditLog não são chamados.

## Dados incluídos no AuditLog

O evento de auditoria inclui apenas metadados mínimos e seguros:

- entidade: `Prescription`;
- identificador da `Prescription` criada;
- ação: `Prescription.Created`;
- `AttendanceId` no metadata JSON;
- `UserId` atual;
- perfil/claim do usuário quando disponível no `CurrentUserInfo`;
- timestamp do evento conforme contrato `ClinicalAuditEvent`.

## Dados explicitamente não incluídos no AuditLog

O AuditLog não inclui payload clínico sensível ou detalhes completos da prescrição, incluindo:

- `Notes`;
- `Dosage`;
- lista completa de `Items`;
- `ProductId`;
- texto livre clínico;
- quantidades, unidades ou duração dos itens.

## Ausência de endpoint/controller

Esta fase não cria controller, endpoint público, rota, política aplicada em rota ou alteração de Swagger para `Prescription`.

## Ausência de migration/schema

Esta fase não cria migration, não altera schema, não modifica entidades de domínio e não altera configuração do `AppDbContext`/EF.

## Ausência de estoque/Product

Esta fase não integra `Prescription` com estoque, `Product`, baixa, reserva, venda, financeiro ou qualquer fluxo operacional de dispensação.

## Riscos remanescentes

- A autoria ainda não está persistida diretamente nas entidades `Prescription`/`PrescriptionItem` por ausência de campos de domínio/schema nesta fase.
- O fluxo permanece interno e sem endpoint público; futuras exposições HTTP precisarão aplicar autorização, validação de perfil e cobertura de testes de API.
- O retorno de criação ainda reflete os itens solicitados, mas o AuditLog foi mantido mínimo para evitar vazamento de payload clínico.

## Próxima fase recomendada

A próxima fase recomendada é definir, de forma planejada, se `Prescription` deve receber campos próprios de autoria/timestamps no domínio e no schema, com migration dedicada, ou se a rastreabilidade mínima via AuditLog clínico continuará sendo suficiente para o ciclo inicial da vertical.
