# Fase 8.4 — CurrentClinicalContext

## Objetivo

Criar a base estrutural para representar o contexto clínico ativo de uma operação no backend do TOGO por meio de um contrato de Application e de uma implementação HTTP inicial. Esta fase prepara a Fase 8.5 — Autorização contextual, mas não implementa validação de vínculo usuário-clínica.

## Problema que resolve

As fases anteriores persistiram `ClinicId` em entidades clínicas principais, mas ainda não havia uma abstração transversal para indicar qual clínica está ativa durante uma requisição. Sem essa abstração, fases futuras não têm um ponto único para obter o contexto selecionado antes de validar autorização e aplicar filtros.

## Escopo implementado

- Criação do contrato `ICurrentClinicalContext` na camada Application.
- Criação das exceções pequenas e específicas `MissingClinicalContextException` e `InvalidClinicalContextException` na camada Application.
- Criação da implementação HTTP `HttpCurrentClinicalContext` na API.
- Registro de DI com lifetime scoped para `ICurrentClinicalContext`.
- Resolução lazy do `ClinicId` a partir do header temporário `X-Clinic-Id`.
- Testes unitários para contrato/comportamento esperado e para a implementação HTTP.

## Arquivos alterados

- `backend/src/Togo.Application/Security/ICurrentClinicalContext.cs`
- `backend/src/Togo.Application/Security/MissingClinicalContextException.cs`
- `backend/src/Togo.Application/Security/InvalidClinicalContextException.cs`
- `backend/src/Togo.Api/Security/HttpCurrentClinicalContext.cs`
- `backend/src/Togo.Api/DependencyInjection/ClinicalContextServiceCollectionExtensions.cs`
- `backend/src/Togo.Api/Program.cs`
- `backend/src/Togo.Application.Tests/Security/CurrentClinicalContextTests.cs`
- `backend/src/Togo.Api.Tests/Security/HttpCurrentClinicalContextTests.cs`
- `docs/security/PHASE_08_TECHNICAL_DEBT.md`

## Contrato criado

`ICurrentClinicalContext` expõe:

- `long? ClinicId`: pode ser `null` quando a operação ainda não exige contexto clínico.
- `bool HasClinic`: indica se há clínica ativa resolvida.
- `long GetRequiredClinicId()`: retorna o `ClinicId` ativo ou falha com erro previsível quando o contexto é obrigatório e está ausente.

A camada Application permanece independente de `HttpContext`, headers, claims e ASP.NET.

## Implementação criada

`HttpCurrentClinicalContext` fica na camada API e usa `IHttpContextAccessor` para ler a requisição atual. A resolução é lazy: o header só é interpretado quando `ClinicId`, `HasClinic` ou `GetRequiredClinicId()` são acessados.

Comportamento:

- Header ausente: `ClinicId = null` e `HasClinic = false`.
- Header presente com `long` positivo: valor aceito como contexto clínico ativo.
- Header presente com zero, negativo, texto inválido, vazio ou múltiplos valores: falha com `InvalidClinicalContextException`.
- `GetRequiredClinicId()` sem contexto: falha com `MissingClinicalContextException`.

## Fonte temporária do contexto clínico

A fonte temporária definida para a Fase 8.4 é o header HTTP:

```http
X-Clinic-Id: 123
```

## Decisão sobre `X-Clinic-Id`

O header `X-Clinic-Id` foi escolhido como mecanismo transitório simples para permitir que clientes informem a clínica selecionada durante a operação. Essa decisão reduz acoplamento com front-end e autenticação nesta fase, permitindo testar a base estrutural antes da autorização contextual.

Importante: `X-Clinic-Id` não é autorização. O valor informado pelo cliente não comprova permissão, vínculo, associação com organização, unidade ativa ou direito de acessar dados daquela clínica.

## Riscos remanescentes

- Um cliente ainda pode informar qualquer `ClinicId` positivo; a Fase 8.4 apenas faz parsing e valida formato.
- Não há validação de `UserClinicAccess`.
- Não há padronização HTTP específica para contexto clínico inválido/ausente; exceções previsíveis foram criadas para permitir padronização futura.
- Não há filtros globais por contexto clínico.
- Use cases clínicos ainda não dependem de `ICurrentClinicalContext`.

## O que não foi implementado

- `UserClinicAccess`.
- Validação de vínculo usuário-clínica.
- Autorização contextual completa.
- Filtros globais por contexto.
- Alteração de queries clínicas para filtrar por `ClinicId`.
- Alteração das regras de criação das fases 8.3.x.
- `ClinicId` direto em `Pet`.
- `ClinicId` direto em `PrescriptionItem`.
- Auditoria contextual completa ou auditoria de leitura.
- Acesso negado contextual.
- Front-end.
- `ClinicUnitId` obrigatório como contexto.

## Relação com débitos técnicos

Esta fase reduz o débito de ausência de `CurrentClinicalContext`, mas mantém débitos relevantes abertos: autorização contextual, validação de `UserClinicAccess`, filtros por contexto, padronização de respostas HTTP para contexto inválido/ausente e eventual substituição do header transitório por mecanismo definitivo integrado à seleção de clínica e identidade do usuário.

## Próxima fase recomendada

Fase 8.5 — Autorização contextual: criar e validar o vínculo usuário-clínica, impedir uso de clínicas não autorizadas e definir a política de erro/acesso negado para contexto clínico inválido, ausente ou não permitido.
