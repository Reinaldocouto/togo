# Fase 7.5.3.1 — Fluxo interno mínimo de Prescription

## Objetivo

Implementar o fluxo interno mínimo de `Prescription` vinculada a `Attendance`, sem criação de controller, rota ou endpoint público.

## Contexto da Fase 7.5

Esta fase continua a integração segura de `Prescription` com `Attendance` iniciada na Fase 7.5.1. A correção documental da Fase 7.5.1.1 estabeleceu que não deve existir endpoint público de criação antes de autoria mínima e AuditLog mínimo. A Fase 7.5.2 entregou os contratos, permissões e ações de auditoria como base técnica, mas sem repository/use case funcional.

## Decisões implementadas

- Foi criado `IPrescriptionRepository` para escrita interna de prescrição e listagem por `AttendanceId`.
- Foi criado `PrescriptionRepository` com EF Core para persistir `Prescription` e `PrescriptionItem`.
- Foram criados `CreatePrescriptionUseCase` e `ListPrescriptionsByAttendanceUseCase` como fluxos internos de aplicação.
- A criação valida existência do atendimento e permite persistência somente quando `Attendance.Status == AttendanceStatus.Open`.
- A criação valida consistência entre `attendanceId` recebido pelo parâmetro interno e `request.AttendanceId`.
- Os itens são persistidos em segunda etapa: primeiro a prescrição é salva para obter `Prescription.Id`; depois os itens são criados com o identificador real e salvos. A decisão evita alterar `Prescription` para expor coleção de itens nesta fase.
- `ProductId` permanece opcional e, quando informado, deve ser maior que zero.
- Não há integração com `Product`, estoque, reserva, baixa, venda ou financeiro.
- Não há autoria persistida nesta fase.
- Não há escrita de AuditLog nesta fase.
- Não há controller, rota, Swagger ou endpoint público.
- Não há migration, alteração de schema, alteração em `AppDbContext` ou configuração EF.
- Não há listagem global de prescrições.
- A listagem usa projeção mínima com `Id`, `AttendanceId`, `IssuedAt` e `ItemCount`, sem `Notes`, `Dosage`, itens completos, `ProductId` ou payload clínico amplo.

## Regras de validação

### Criação

- `attendanceId` interno deve ser maior que zero.
- `request.AttendanceId` deve ser maior que zero.
- `attendanceId` e `request.AttendanceId` devem ser iguais.
- `IssuedAt` é obrigatório.
- `Items` é obrigatório e não pode ser vazio.
- Cada item exige `Quantity > 0`, `Unit` obrigatório, `Dosage` obrigatório, `DurationDays > 0` quando informado e `ProductId > 0` quando informado.
- Atendimento inexistente retorna `NotFound`.
- Atendimento diferente de `Open` retorna `Conflict`.

### Listagem por atendimento

- `attendanceId` deve ser maior que zero.
- Atendimento inexistente retorna `NotFound`.
- Retorna somente prescrições do atendimento solicitado.
- Não há listagem global.

## Testes

Foram adicionados testes de domínio para `Prescription` e `PrescriptionItem`, testes de aplicação para criação e listagem interna, fake de repository de prescrição e testes de infraestrutura para persistência e projeção com `ItemCount`.

## Riscos remanescentes

- A persistência de prescrição e itens usa duas chamadas de `SaveChangesAsync` para preservar o domínio atual sem coleção de itens. Caso a aplicação passe a exigir atomicidade transacional forte neste fluxo, uma transação explícita poderá ser adicionada em fase posterior.
- O response de criação retorna itens com `Id = 0` porque a interface interna evita expor entidades de infraestrutura ou reconsultar detalhe completo nesta fase. O ponto pode ser refinado quando houver endpoint seguro e autoria/auditoria.

## Fora do escopo

Controller, endpoint público, autorização aplicada em rota, autoria, AuditLog, migration/schema, estoque, `Product`, PDF/receituário, assinatura clínica, cancelamento, update, delete, frontend e integrações operacionais permanecem fora do escopo.

## Critérios de aceite

A fase é aceita quando repository, use cases internos, persistência de itens, validações de atendimento aberto, listagem por `AttendanceId`, testes e documentação estiverem presentes, sem endpoint público, controller, autoria, AuditLog, migration/schema ou estoque.

## Próxima fase recomendada

Fase 7.5.3.2 — Autoria e AuditLog mínimos de Prescription aplicados ao fluxo de criação.
