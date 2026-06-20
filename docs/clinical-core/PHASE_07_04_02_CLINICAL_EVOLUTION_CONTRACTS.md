# Fase 7.4.2 — Contratos/base técnica para ClinicalEvolution vinculada a Attendance

## 1. Objetivo

Criar a base técnica mínima para a futura implementação segura de `ClinicalEvolution` vinculada a `Attendance`, sem expor endpoint público, sem alterar persistência e sem modificar regras de negócio.

## 2. Contexto da Fase 7.4

A Fase 7.4 prepara a integração de evoluções clínicas ao episódio de atendimento. `ClinicalEvolution` pertence ao episódio `Attendance`; `MedicalRecord` permanece como memória clínica longitudinal e não deve ser inflado com coleção de evoluções.

## 3. Referência à Fase 7.4.1

Esta fase continua o planejamento registrado em `docs/clinical-core/PHASE_07_04_01_CLINICAL_EVOLUTION_ATTENDANCE_PLANNING.md`, que confirmou a existência da entidade, de `AttendanceId` e do relacionamento EF já configurado com `DeleteBehavior.Restrict`, além da ausência de contracts, autorização granular, auditoria específica e endpoints de `ClinicalEvolution`.

## 4. Contracts criados

Foram criados contracts mínimos em `Togo.Application.ClinicalEvolutions.Contracts`:

- `CreateClinicalEvolutionRequest`
- `ClinicalEvolutionResponse`
- `ClinicalEvolutionListItemResponse`

## 5. Fields dos contracts

### `CreateClinicalEvolutionRequest`

- `AttendanceId`
- `RegisteredAt`
- `Type`
- `Text`

O contrato mantém `AttendanceId` explícito nesta base técnica para deixar clara a vinculação futura ao episódio `Attendance`. Uma fase posterior ainda poderá decidir receber esse identificador pela rota, desde que mantenha a validação de consistência.

### `ClinicalEvolutionResponse`

- `Id`
- `AttendanceId`
- `RegisteredAt`
- `Type`
- `Text`

`Text` fica restrito a um response de detalhe/criação futura. Esta fase não cria endpoint que retorne esse payload.

### `ClinicalEvolutionListItemResponse`

- `Id`
- `AttendanceId`
- `RegisteredAt`
- `Type`

## 6. Decisão de não incluir `Text` em list item

`ClinicalEvolution.Text` contém conteúdo clínico sensível. Por isso, o list item não expõe `Text`, evitando vazamento em listagens amplas ou telas resumidas futuras.

## 7. Decisão de não incluir autoria no request

A autoria não foi incluída em `CreateClinicalEvolutionRequest`. A autoria futura deve derivar do usuário autenticado/contexto de execução, não de payload informado pelo cliente.

## 8. Permissões criadas

Foram criadas as permissões centralizadas:

- `ClinicalEvolution.Read`
- `ClinicalEvolution.Create`
- `ClinicalEvolution.Update`

`ClinicalEvolution.Delete` não foi criada nesta fase porque exclusão, soft delete e retificação permanecem fora do escopo.

## 9. Policies criadas

Foram criadas policies com os mesmos valores das permissões:

- `ClinicalEvolutionPolicies.Read`
- `ClinicalEvolutionPolicies.Create`
- `ClinicalEvolutionPolicies.Update`

## 10. Matriz de autorização planejada

| Perfil | Read | Create | Update |
| --- | ---: | ---: | ---: |
| Admin | ✅ | ✅ | ✅ |
| Veterinarian | ✅ | ✅ | ✅ |
| Assistant | ✅ | ❌ | ❌ |
| Reception | ❌ | ❌ | ❌ |
| ReadOnly | ❌ | ❌ | ❌ |

A matriz segue o padrão de autorização por perfil já usado por `MedicalRecord` e `Attendance`. `Assistant` pode ler, mas não criar/alterar evolução clínica textual nesta fase.

## 11. Decisão sobre `Program.cs`

Foi criado o extension method `AddClinicalEvolutionPolicies`, mas ele não foi registrado em `Program.cs` nesta fase. O registro deve ocorrer na Fase 7.4.3, quando a superfície de API for criada.

## 12. Audit actions criadas

Foram criadas as actions mínimas:

- `ClinicalEvolution.Created`
- `ClinicalEvolution.Updated`

## 13. Audit actions fora do escopo

Não foram criadas:

- `ClinicalEvolution.Deleted`
- `ClinicalEvolution.Read`
- `ClinicalEvolution.AccessDenied`

Em fases futuras, logs de auditoria não devem registrar texto clínico sensível.

## 14. Arquivos criados

- `backend/src/Togo.Application/ClinicalEvolutions/Contracts/CreateClinicalEvolutionRequest.cs`
- `backend/src/Togo.Application/ClinicalEvolutions/Contracts/ClinicalEvolutionResponse.cs`
- `backend/src/Togo.Application/ClinicalEvolutions/Contracts/ClinicalEvolutionListItemResponse.cs`
- `backend/src/Togo.Application/Security/ClinicalEvolutionPermissions.cs`
- `backend/src/Togo.Api/Security/ClinicalEvolutionPolicies.cs`
- `backend/src/Togo.Api/Security/ClinicalEvolutionAuthorization.cs`
- `backend/src/Togo.Application/Auditing/ClinicalEvolutionAuditActions.cs`
- `backend/src/Togo.Application.Tests/Security/ClinicalEvolutionPermissionsTests.cs`
- `backend/src/Togo.Api.Tests/Security/ClinicalEvolutionPoliciesTests.cs`
- `backend/src/Togo.Api.Tests/Security/ClinicalEvolutionAuthorizationTests.cs`
- `backend/src/Togo.Application.Tests/Auditing/ClinicalEvolutionAuditActionsTests.cs`
- `docs/clinical-core/PHASE_07_04_02_CLINICAL_EVOLUTION_CONTRACTS.md`

## 15. Testes criados

Foram criados testes unitários simples para:

- valores, unicidade e ausência de `Delete` em permissões;
- vínculo entre policies e permissões;
- matriz de autorização por perfil, incluindo perfil ausente, inválido, casing normalizado e permissão desconhecida;
- valores, unicidade e ausência de audit actions fora do escopo.

## 16. Ausência de endpoint público

Nenhum controller ou endpoint público de `ClinicalEvolution` foi criado nesta fase.

## 17. Ausência de migration/schema

Nenhuma migration, configuração EF ou alteração de schema foi criada.

## 18. Ausência de persistência nova

Nenhum repository, use case funcional, escrita de `ClinicalAuditLog` ou alteração de persistência foi criada.

## 19. Riscos remanescentes

- Definir validação de atendimento aberto antes da criação de evolução.
- Definir autoria persistida a partir do usuário autenticado.
- Definir regras de atualização, retificação e assinatura clínica.
- Definir audit logging sem registrar texto clínico.
- Definir endpoints restritos ao episódio `Attendance`, evitando listagem global.

## 20. Fora do escopo

Permanecem fora do escopo desta fase: controller público, endpoint, use cases funcionais, repositories, migrations, alterações em entidades, alterações em `AppDbContext`, escrita de auditoria, assinatura clínica, soft delete, retificação, anexos, frontend e infraestrutura.

## 21. Critérios de aceite

Esta fase atende aos critérios quando:

- contracts mínimos existem;
- list item não expõe texto clínico;
- permissões e policies existem;
- matriz de autorização existe e é testada;
- audit actions mínimas existem;
- não há endpoint público;
- não há migration/schema novo;
- não há alteração de entidade/repository/persistência;
- documentação existe;
- testes de constants/matriz passam.

## 22. Próxima fase recomendada

Fase 7.4.3 — Implementação mínima de ClinicalEvolution vinculada a Attendance.
