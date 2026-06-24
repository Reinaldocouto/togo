# Fase 7.5.3.3 — Endpoints seguros mínimos de Prescription

## Objetivo

Expor endpoints HTTP mínimos e seguros para `Prescription`, sempre vinculados a um `Attendance`, reutilizando os use cases internos já existentes e sem ampliar regras de domínio.

## Endpoints criados

- `POST /api/attendances/{attendanceId}/prescriptions`
  - Cria uma prescrição para o atendimento informado na rota.
  - Delega a regra de criação para `CreatePrescriptionUseCase`.
- `GET /api/attendances/{attendanceId}/prescriptions`
  - Lista prescrições mínimas do atendimento informado na rota.
  - Delega a consulta para `ListPrescriptionsByAttendanceUseCase`.

Não foram criados endpoints globais, update, delete, cancelamento ou impressão.

## Policies aplicadas

- Criação: `PrescriptionPolicies.Create` / `Prescription.Create`.
- Listagem: `PrescriptionPolicies.Read` / `Prescription.Read`.

As policies são registradas no pipeline de autorização da API e seguem a matriz de perfis já existente para Prescription.

## Contrato de entrada

### POST

Usa `CreatePrescriptionRequest`:

- `attendanceId`: deve coincidir com o `{attendanceId}` da rota.
- `issuedAt`: data/hora de emissão.
- `notes`: aceito apenas no fluxo de criação, conforme contrato interno já existente.
- `items`: lista obrigatória de itens para criação, conforme validação do use case.

O controller não duplica validação complexa; validações de negócio permanecem no use case.

### GET

Não possui body. O parâmetro obrigatório é o `{attendanceId}` da rota.

## Contrato de saída

### POST

Retorna o contrato já existente de criação (`PrescriptionResponse`) em caso de sucesso, mantendo compatibilidade com o fluxo interno mínimo.

### GET

Retorna lista de `PrescriptionListItemResponse`, com dados mínimos:

- `id`;
- `attendanceId`;
- `issuedAt`;
- `itemCount`.

## Regras de retorno HTTP

- Sucesso de criação: `200 OK`, seguindo o padrão de controller clínico mínimo já adotado para criação vinculada a Attendance.
- Sucesso de listagem: `200 OK`.
- Erro de validação: `400 BadRequest`.
- Recurso inexistente: `404 NotFound`.
- Conflito de estado do atendimento: `409 Conflict`.
- Ausência de autenticação: tratada pelo pipeline como `401 Unauthorized`.
- Falha de autorização/policy: tratada pelo pipeline como `403 Forbidden`.

## Dados expostos na listagem

A listagem expõe apenas metadados mínimos por prescrição: identificador, atendimento, emissão e quantidade de itens.

## Dados explicitamente não expostos

A listagem não expõe:

- `Notes`;
- `Dosage`;
- itens completos (`Items`);
- `ProductId`;
- payload clínico sensível dos itens ou observações.

## AuditLog mínimo de criação

A criação continua usando o AuditLog mínimo implementado na fase anterior por meio do `CreatePrescriptionUseCase`. O controller apenas encaminha a chamada ao use case, mantendo o AuditLog sem payload clínico sensível.

## Ausência de migration/schema

Esta fase não cria migration, não altera schema, não altera entidades de domínio e não altera configuração de EF ou `AppDbContext`.

## Ausência de estoque/Product

Não há integração com estoque, Product, baixa, reserva, venda ou financeiro. `ProductId`, quando presente no contrato interno de criação, não é consultado nem exposto na listagem mínima.

## Riscos remanescentes

- O contrato de criação ainda retorna o detalhe completo criado, incluindo dados clínicos informados no request, por compatibilidade com o use case já existente; a restrição de não exposição foi aplicada à listagem mínima.
- Não há cancelamento, impressão, assinatura, PDF ou versionamento de prescrição nesta fase.
- Integrações futuras com Product/estoque precisarão de novas regras explícitas de privacidade, autorização e auditoria.

## Próxima fase recomendada

Implementar uma fase específica para endurecimento do contrato público de detalhe/criação, caso seja necessário separar DTOs externos dos contratos internos, e só depois avaliar impressão/PDF, cancelamento ou integração controlada com Product/estoque.
