# Fase 7.5.3.4 — Endurecimento do contrato público de Prescription

## Objetivo

Endurecer o contrato público do endpoint `POST /api/attendances/{attendanceId}/prescriptions`, garantindo que a criação de Prescription retorne apenas uma confirmação mínima e segura, sem replicar os dados clínicos enviados no request.

## Motivo da separação entre contrato interno e contrato público

O use case `CreatePrescriptionUseCase` continua produzindo o contrato interno `PrescriptionResponse`, usado como resultado de aplicação e compatível com o fluxo existente. Entretanto, esse contrato interno contém dados clínicos de prescrição, como notas e itens completos.

A API pública agora faz uma camada explícita de mapeamento antes de responder ao cliente. Essa separação reduz a exposição acidental de dados sensíveis e permite que o contrato HTTP evolua independentemente do contrato interno da aplicação.

## Novo DTO público de criação

Foi criado o DTO público `PrescriptionCreatedResponse`, usado exclusivamente como resposta segura de criação no controller de Prescriptions.

## Campos expostos

A resposta pública de criação expõe somente:

- `Id`: identificador da Prescription criada;
- `AttendanceId`: identificador do atendimento relacionado;
- `IssuedAt`: data/hora de emissão;
- `ItemCount`: quantidade de itens associados à prescrição.

## Campos explicitamente não expostos

O contrato público de criação não expõe:

- `Notes`;
- `Dosage`;
- `Items` completos;
- `ProductId`;
- `Quantity`;
- `Unit`;
- `DurationDays`;
- qualquer texto livre clínico informado no request.

## Comportamento HTTP mantido

O endpoint mantém os comportamentos HTTP já definidos na fase anterior:

- `200 OK` em criação válida, agora com `PrescriptionCreatedResponse`;
- `400 Bad Request` para erros de validação;
- `404 Not Found` quando o atendimento não existe;
- `409 Conflict` quando o atendimento não permite criação de prescrição;
- `403 Forbidden` quando o usuário autenticado não possui `PrescriptionPolicies.Create`.

A listagem `GET /api/attendances/{attendanceId}/prescriptions` permanece com contrato mínimo via `PrescriptionListItemResponse`.

## Ausência de migration/schema

Esta fase não altera entidade de domínio, `DbContext`, configurações de entidade, migrations, tabelas ou colunas. A mudança é restrita ao contrato público da API e aos testes relacionados.

## Ausência de Product/estoque

Esta fase não adiciona nem altera integração com Product, Stock ou Inventory. O mapeamento público não expõe `ProductId` e não executa qualquer regra de estoque.

## Ausência de alteração no AuditLog

O fluxo de criação continua chamando o mesmo use case e preserva a escrita de AuditLog existente. A mudança ocorre apenas após o retorno do use case, no mapeamento da resposta HTTP pública.

## Riscos remanescentes

- O contrato interno `PrescriptionResponse` ainda contém dados clínicos e deve permanecer restrito às camadas internas da aplicação.
- Outros endpoints futuros de Prescription devem evitar reutilizar contratos internos diretamente como payload público.
- A documentação OpenAPI, se formalizada em fase posterior, deve refletir explicitamente o DTO público mínimo.

## Próxima fase recomendada

Recomenda-se revisar os demais contratos públicos clínicos para garantir separação consistente entre DTOs internos de use case e DTOs expostos pela API, além de adicionar documentação OpenAPI/Swagger explícita para os payloads mínimos permitidos.
