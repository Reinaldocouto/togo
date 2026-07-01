# Fase 8.5 — Autorização contextual

## Objetivo

Implementar a primeira camada real de autorização contextual do TOGO, validando se o usuário autenticado possui vínculo ativo com a clínica selecionada no contexto clínico atual.

## Problema resolvido

Até a Fase 8.4, `X-Clinic-Id` era apenas um contexto transitório resolvido por `ICurrentClinicalContext`. A Fase 8.5 adiciona o modelo mínimo que permite diferenciar contexto informado pelo cliente de autorização efetiva de acesso.

## Escopo implementado

- Entidade de domínio `UserClinicAccess`.
- Persistência EF Core de `UserClinicAccess`.
- Migration `20260701120000_AddUserClinicAccess`.
- Contrato `IUserClinicAccessRepository`.
- Repository EF `UserClinicAccessRepository`.
- Serviço `IClinicalContextAuthorizationService` / `ClinicalContextAuthorizationService`.
- Exceção previsível `ClinicalContextAccessDeniedException`.
- Registro de DI para os serviços mínimos.
- Testes de domínio, Application, Infrastructure e DI.

## Arquivos criados/alterados

- `backend/src/Togo.Domain/Entities/UserClinicAccess.cs`
- `backend/src/Togo.Domain.Tests/UserClinicAccessTests.cs`
- `backend/src/Togo.Application/Security/IUserClinicAccessRepository.cs`
- `backend/src/Togo.Application/Security/IClinicalContextAuthorizationService.cs`
- `backend/src/Togo.Application/Security/ClinicalContextAuthorizationService.cs`
- `backend/src/Togo.Application/Security/ClinicalContextAccessDeniedException.cs`
- `backend/src/Togo.Application.Tests/Security/ClinicalContextAuthorizationServiceTests.cs`
- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/UserClinicAccessConfiguration.cs`
- `backend/src/Togo.Infrastructure/Repositories/UserClinicAccessRepository.cs`
- `backend/src/Togo.Infrastructure/Migrations/20260701120000_AddUserClinicAccess.cs`
- `backend/src/Togo.Infrastructure.Tests/Persistence/UserClinicAccessPersistenceTests.cs`
- `backend/src/Togo.Infrastructure.Tests/Repositories/UserClinicAccessRepositoryTests.cs`
- `backend/src/Togo.Api/DependencyInjection/ClinicalContextAuthorizationServiceCollectionExtensions.cs`
- `backend/src/Togo.Api/Program.cs`
- `backend/src/Togo.Api.Tests/Security/ClinicalContextAuthorizationDependencyInjectionTests.cs`

## Entidade `UserClinicAccess`

`UserClinicAccess` representa o vínculo mínimo entre usuário e clínica:

- `Id`
- `UserId`
- `ClinicId`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

Decisão importante: o projeto já usa `Guid` para `User.Id` e `CurrentUserInfo.UserId`. Por isso, `UserClinicAccess.UserId` foi implementado como `Guid`, e não como `long`. A validação equivalente para usuário obrigatório rejeita `Guid.Empty`.

O vínculo nasce ativo e pode ser inativado/reativado por métodos explícitos `Inactivate(updatedAt)` e `Activate(updatedAt)`.

## Persistência e migration

A tabela criada é `UserClinicAccesses`, com:

- PK em `Id`.
- FK obrigatória para `Users.Id` com `Restrict`.
- FK obrigatória para `Clinics.Id` com `Restrict`.
- Índice em `UserId`.
- Índice em `ClinicId`.
- Índice único composto em `UserId` + `ClinicId`.

A decisão pelo índice único evita vínculo duplicado no MVP. Se no futuro for necessário histórico de vínculos, o modelo deverá evoluir para entidade histórica/auditável sem permitir duplicidade ativa ambígua.

## Repository criado

`IUserClinicAccessRepository` expõe consultas simples:

- `HasActiveAccessAsync(Guid userId, long clinicId, CancellationToken cancellationToken)`
- `GetAsync(Guid userId, long clinicId, CancellationToken cancellationToken)`

A implementação EF usa `AsNoTracking` e não introduz filtros globais por contexto.

## Serviço de autorização contextual

`ClinicalContextAuthorizationService` fornece:

- `EnsureCanAccessCurrentClinicAsync` — lê o `ClinicId` obrigatório de `ICurrentClinicalContext`.
- `EnsureCanAccessClinicAsync` — valida um `ClinicId` explícito.

Dependências:

- `ICurrentClinicalContext`
- `ICurrentUserService`
- `IUserClinicAccessRepository`

Comportamento esperado:

- Sem contexto clínico atual: `MissingClinicalContextException`.
- `ClinicId` explícito inválido: `InvalidClinicalContextException`.
- Usuário ausente/não autenticado: `CurrentUserResolutionException`.
- Sem vínculo ativo usuário-clínica: `ClinicalContextAccessDeniedException`.
- Vínculo ativo encontrado: operação permitida.

## Decisões tomadas

- Não foram aplicados filtros globais ou filtros automáticos em queries clínicas.
- Não foi plugada autorização contextual em todos os endpoints nesta fase.
- `X-Clinic-Id` continua sendo somente seleção transitória de contexto; autorização é feita pelo novo serviço quando chamado.
- `UserId` foi mantido como `Guid`, respeitando o modelo existente de autenticação do projeto.
- `ClinicUnitId` não foi incluído no vínculo.
- Não foram criadas permissões granulares ou roles por clínica.

## Testes criados/executados

Foram criados testes para:

- Criação, ativação e inativação de `UserClinicAccess`.
- Rejeição de `Guid.Empty` e `ClinicId` inválido.
- Autorização contextual com vínculo ativo.
- Negação por vínculo inexistente ou inativo.
- Falha por contexto clínico ausente.
- Falha por usuário não autenticado.
- Validação de `ClinicId` explícito.
- Mapping EF, FKs, índices e delete behavior.
- Repository `HasActiveAccessAsync`.
- Registro DI mínimo.

## Riscos remanescentes

- Endpoints clínicos ainda precisam chamar sistematicamente o serviço ou usar pipeline específico em fases posteriores.
- Listagens e consultas ainda não filtram por contexto clínico automaticamente.
- Ainda não há auditoria transversal de acesso negado.
- Ainda não há padronização HTTP completa para erros de contexto/autorização contextual.
- Não há seed obrigatório de vínculos usuário-clínica.

## O que não foi implementado

- Filtros globais por contexto.
- Alteração ampla de queries clínicas.
- Auditoria contextual completa.
- Auditoria de leitura.
- Evento transversal de acesso negado.
- Front-end.
- `ClinicUnitId` obrigatório.
- Multi-tenant SaaS completo.
- `ClinicId` direto em `Pet`.
- `ClinicId` direto em `PrescriptionItem`.
- Roles/permissões granulares complexas.

## Relação com `PHASE_08_TECHNICAL_DEBT.md`

O débito de ausência de `UserClinicAccess` foi tratado no nível mínimo de MVP. O débito de `CurrentClinicalContext` sem autorização foi parcialmente tratado: existe serviço de autorização contextual, mas ele ainda não é aplicado transversalmente em endpoints e queries.

Permanecem abertos os débitos de filtros por contexto, auditoria contextual completa e padronização HTTP.

## Próxima fase recomendada

Fase 8.6 — Filtros de consulta por contexto.
