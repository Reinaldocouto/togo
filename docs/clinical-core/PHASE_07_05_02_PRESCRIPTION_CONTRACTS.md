# Fase 7.5.2 — Contratos/base técnica para Prescription vinculada a Attendance

## 1. Objetivo

Criar a base técnica mínima para a futura integração segura de `Prescription` vinculada a `Attendance`, sem expor endpoint público, sem repository/use case funcional, sem migration/schema e sem integração com estoque.

## 2. Contexto da Fase 7.5

A Fase 7.5 trata a integração incremental de `Prescription` ao eixo operacional `Attendance`. A prescrição é conteúdo clínico sensível e potencialmente medicamentoso, portanto a base inicial deve preparar contratos, permissões, policies, matriz de autorização e ações de auditoria antes de qualquer superfície pública de escrita.

## 3. Referências às Fases 7.5.1 e 7.5.1.1

Esta fase sucede a Fase 7.5.1, registrada em `docs/clinical-core/PHASE_07_05_01_PRESCRIPTION_ATTENDANCE_PLANNING.md`, e incorpora o sequenciamento seguro corrigido pela Fase 7.5.1.1: endpoint público de criação deve aguardar autoria mínima e AuditLog mínimo.

## 4. Estado atual que motivou a base técnica

A análise anterior confirmou que `Prescription` e `PrescriptionItem` já existem no domínio, `Prescription` já possui `AttendanceId`, `PrescriptionItem.ProductId` é opcional, não há entidade real `Product`, não há vertical real de estoque/inventário e não há contracts, use cases, repositories, controllers, testes, autorização granular ou ações de auditoria específicas para `Prescription`.

## 5. Contracts criados

Foram criados contracts mínimos em `backend/src/Togo.Application/Prescriptions/Contracts`:

- `CreatePrescriptionRequest`.
- `CreatePrescriptionItemRequest`.
- `PrescriptionResponse`.
- `PrescriptionItemResponse`.
- `PrescriptionListItemResponse`.

## 6. Fields dos contracts

`CreatePrescriptionRequest` contém `AttendanceId`, `IssuedAt`, `Notes` e `Items`.

`CreatePrescriptionItemRequest` contém `ProductId`, `Quantity`, `Unit`, `Dosage` e `DurationDays`.

`PrescriptionResponse` contém `Id`, `AttendanceId`, `IssuedAt`, `Notes` e `Items`.

`PrescriptionItemResponse` contém `Id`, `ProductId`, `Quantity`, `Unit`, `Dosage` e `DurationDays`.

`PrescriptionListItemResponse` contém apenas `Id`, `AttendanceId`, `IssuedAt` e `ItemCount`.

## 7. Decisão de manter ProductId opcional

`ProductId` foi mantido como `long?` nos contracts de item porque o domínio atual já modela a referência como opcional e não existe catálogo real de produtos implementado.

## 8. Decisão de não inventar Product

Nenhuma entidade, configuração, repository ou integração de `Product` foi criada. `ProductId` permanece como referência técnica opcional e latente, sem relacionamento funcional com estoque, venda ou financeiro.

## 9. Decisão de minimizar o list item

`PrescriptionListItemResponse` não expõe `Notes`, `Dosage` nem itens completos. A listagem inicial planejada deve ser por `AttendanceId`, mas ainda assim deve minimizar conteúdo clínico sensível e evitar payloads amplos.

## 10. Decisão de não incluir autoria no request

Autoria não foi incluída em `CreatePrescriptionRequest`. Em fases futuras, a autoria deve ser derivada do usuário autenticado/contexto interno, não enviada pelo cliente.

## 11. Permissões criadas

Foram criadas permissões centralizadas em `PrescriptionPermissions`:

- `Prescription.Read`.
- `Prescription.Create`.
- `Prescription.Update`.
- `Prescription.Cancel`.

`Prescription.Delete` não foi criado porque exclusão física de prescrição clínica pode apagar histórico relevante; cancelamento/retificação auditável é o caminho mais seguro para fases futuras.

## 12. Policies criadas

Foram criadas policies em `PrescriptionPolicies`, apontando diretamente para as permissões centralizadas:

- `Read` → `Prescription.Read`.
- `Create` → `Prescription.Create`.
- `Update` → `Prescription.Update`.
- `Cancel` → `Prescription.Cancel`.

## 13. Matriz de autorização planejada

A matriz implementada em `PrescriptionAuthorization` segue o padrão de `ClinicalEvolution`:

| Perfil       | Read | Create | Update | Cancel |
| ------------ | ---: | -----: | -----: | -----: |
| Admin        | sim  | sim    | sim    | sim    |
| Veterinarian | sim  | sim    | sim    | sim    |
| Assistant    | sim  | não    | não    | não    |
| Reception    | não  | não    | não    | não    |
| ReadOnly     | não  | não    | não    | não    |

A decisão mantém `Assistant` com leitura conservadora moderada, consistente com `ClinicalEvolution`, mas sem permissões de criação, atualização ou cancelamento.

## 14. Decisão sobre Program.cs

`AddPrescriptionPolicies()` foi criado, mas não foi registrado em `Program.cs` nesta fase. Como ainda não há controller público de `Prescription`, o registro será feito quando a superfície de API for criada em fase posterior, após autoria mínima e AuditLog mínimo.

## 15. Audit actions criadas

Foram criadas ações mínimas em `PrescriptionAuditActions`:

- `Prescription.Created`.
- `Prescription.Updated`.
- `Prescription.Canceled`.

## 16. Audit actions fora do escopo

Não foram criadas ações `Prescription.Deleted`, `Prescription.Read` ou `Prescription.AccessDenied`. Também não houve escrita de AuditLog nesta fase.

## 17. Arquivos criados

- `backend/src/Togo.Application/Prescriptions/Contracts/CreatePrescriptionRequest.cs`.
- `backend/src/Togo.Application/Prescriptions/Contracts/CreatePrescriptionItemRequest.cs`.
- `backend/src/Togo.Application/Prescriptions/Contracts/PrescriptionResponse.cs`.
- `backend/src/Togo.Application/Prescriptions/Contracts/PrescriptionItemResponse.cs`.
- `backend/src/Togo.Application/Prescriptions/Contracts/PrescriptionListItemResponse.cs`.
- `backend/src/Togo.Application/Security/PrescriptionPermissions.cs`.
- `backend/src/Togo.Api/Security/PrescriptionPolicies.cs`.
- `backend/src/Togo.Api/Security/PrescriptionAuthorization.cs`.
- `backend/src/Togo.Application/Auditing/PrescriptionAuditActions.cs`.
- `docs/clinical-core/PHASE_07_05_02_PRESCRIPTION_CONTRACTS.md`.

## 18. Testes criados

- `backend/src/Togo.Application.Tests/Security/PrescriptionPermissionsTests.cs`.
- `backend/src/Togo.Api.Tests/Security/PrescriptionPoliciesTests.cs`.
- `backend/src/Togo.Api.Tests/Security/PrescriptionAuthorizationTests.cs`.
- `backend/src/Togo.Application.Tests/Auditing/PrescriptionAuditActionsTests.cs`.

## 19. Ausência de endpoint público

Nenhum controller, rota ou endpoint público de `Prescription` foi criado nesta fase.

## 20. Ausência de repository/use case funcional

Nenhum repository ou use case funcional de `Prescription` foi criado nesta fase.

## 21. Ausência de migration/schema

Nenhuma migration, configuração EF, `AppDbContext` ou schema foi alterado.

## 22. Ausência de persistência nova

Não houve nova persistência, alteração de entidade ou escrita de AuditLog.

## 23. Ausência de estoque

Não houve baixa, reserva, catálogo, inventário, venda ou financeiro. Nenhuma estrutura de `Product`, `Stock` ou `Inventory` foi criada.

## 24. Riscos remanescentes

- Criação pública ainda depende de autoria mínima e AuditLog mínimo.
- Regras sobre status de `Attendance` para prescrição ainda precisam ser definidas.
- Cancelamento/retificação ainda requer modelagem de domínio e auditoria.
- Integração real com produto/estoque continua inexistente e deve ser tratada em trilha separada.

## 25. Fora do escopo

Ficaram fora do escopo: controller, endpoint, repository funcional, use case funcional, migration, alteração de entidades, alteração de `AppDbContext`, configuração EF, autoria persistida, escrita de AuditLog, estoque, `Product`, venda, financeiro, PDF/receituário, assinatura clínica, cancelamento real, update real, delete, soft delete, frontend e infraestrutura.

## 26. Critérios de aceite

A fase é aceita quando os contracts mínimos existem, o list item não expõe conteúdo clínico sensível amplo, permissões/policies/matriz/audit actions estão criadas e testadas, não há endpoint público, não há repository/use case funcional, não há migration/schema, não há alteração de entidade e não há integração com estoque.

## 27. Próxima fase recomendada

Recomenda-se seguir com **Fase 7.5.3.1 — Repository/use case mínimo interno de Prescription, sem endpoint público de criação**, mantendo o sequenciamento seguro antes de qualquer endpoint público.
