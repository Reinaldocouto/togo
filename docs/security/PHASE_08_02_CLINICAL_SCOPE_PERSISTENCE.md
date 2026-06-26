# Fase 8.2 — Persistência do Escopo Clínico

## Objetivo

Implementar a persistência mínima do escopo clínico planejado nas fases 8.0, 8.0.1 e 8.1, mapeando `Organization`, `Clinic` e `ClinicUnit` na camada Infrastructure com Entity Framework Core, sem ativar isolamento clínico em runtime.

## Escopo implementado

- `Organization`, `Clinic` e `ClinicUnit` passaram a existir na persistência.
- `AppDbContext` passou a expor `DbSet` para organizações, clínicas e unidades clínicas.
- Foram criadas configurações EF Core dedicadas para as três entidades.
- Foi criada migration revisável para as tabelas `Organizations`, `Clinics` e `ClinicUnits`.
- Foram configuradas chaves primárias, campos obrigatórios, relacionamentos, índices mínimos e delete behaviors seguros.

## Arquivos alterados

- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicConfiguration.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicUnitConfiguration.cs`
- `backend/src/Togo.Infrastructure/Migrations/20260625120000_AddClinicalScopePersistence.cs`
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `backend/src/Togo.Infrastructure.Tests/Persistence/ClinicalScopePersistenceTests.cs`
- `docs/security/PHASE_08_02_CLINICAL_SCOPE_PERSISTENCE.md`

## Entidades e tabelas persistidas

| Entidade | Tabela | Observação |
| --- | --- | --- |
| `Organization` | `Organizations` | Raiz administrativa do agrupamento clínico. |
| `Clinic` | `Clinics` | Escopo primário planejado para isolamento clínico-operacional. |
| `ClinicUnit` | `ClinicUnits` | Escopo complementar para unidades internas de uma clínica. |

## Configurações EF Core criadas

### Organization

- Tabela: `Organizations`.
- Chave primária: `Id`.
- `Name` obrigatório com limite de 120 caracteres, seguindo o padrão já usado em nomes clínicos do projeto.
- `IsActive` obrigatório.
- `CreatedAt` obrigatório.
- `UpdatedAt` opcional.
- Índice simples em `Name` para busca futura.
- Não foi criada unicidade global de `Name`, evitando regra prematura sobre nomes de organizações.

### Clinic

- Tabela: `Clinics`.
- Chave primária: `Id`.
- `OrganizationId` obrigatório.
- `Name` obrigatório com limite de 120 caracteres.
- `IsActive` obrigatório.
- `CreatedAt` obrigatório.
- `UpdatedAt` opcional.
- Relacionamento obrigatório com `Organization`.
- Delete behavior: `Restrict`, para evitar cascade delete perigoso de `Organization` para `Clinic`.

### ClinicUnit

- Tabela: `ClinicUnits`.
- Chave primária: `Id`.
- `ClinicId` obrigatório.
- `Name` obrigatório com limite de 120 caracteres.
- `IsActive` obrigatório.
- `CreatedAt` obrigatório.
- `UpdatedAt` opcional.
- Relacionamento obrigatório com `Clinic`.
- Delete behavior: `Restrict`, para evitar cascade delete perigoso de `Clinic` para `ClinicUnit`.

## Migration criada

Migration: `20260625120000_AddClinicalScopePersistence`.

A migration cria:

- tabela `Organizations`;
- tabela `Clinics`;
- tabela `ClinicUnits`;
- PKs das três tabelas;
- FK `Clinics.OrganizationId -> Organizations.Id` com `Restrict`;
- FK `ClinicUnits.ClinicId -> Clinics.Id` com `Restrict`;
- índices mínimos definidos para a subfase.

## Índices definidos

- `IX_Organizations_Name` em `Organizations.Name` para busca futura por nome.
- `IX_Clinics_OrganizationId` em `Clinics.OrganizationId` para navegação por organização.
- `IX_Clinics_OrganizationId_Name` único em `Clinics.OrganizationId + Clinics.Name`, evitando duplicidade de nome dentro da mesma organização.
- `IX_ClinicUnits_ClinicId` em `ClinicUnits.ClinicId` para navegação por clínica.
- `IX_ClinicUnits_ClinicId_Name` único em `ClinicUnits.ClinicId + ClinicUnits.Name`, evitando duplicidade de nome de unidade dentro da mesma clínica.

A unicidade composta foi considerada tecnicamente adequada por atuar apenas dentro do respectivo escopo, sem impor unicidade global prematura.

## Delete behaviors definidos

- `Organization -> Clinic`: `Restrict`.
- `Clinic -> ClinicUnit`: `Restrict`.

Essa decisão evita remoções físicas em cascata que poderiam apagar ou impactar dados clínicos e operacionais futuros quando as entidades clínicas passarem a referenciar `Clinic`.

## Testes criados e executados

Foi criado `ClinicalScopePersistenceTests` cobrindo:

- existência dos `DbSet` de `Organization`, `Clinic` e `ClinicUnit` no `AppDbContext`;
- mapeamento das tabelas esperadas;
- constraints de campos obrigatórios e limite de tamanho para `Name`;
- índices simples e compostos;
- delete behavior seguro (`Restrict`);
- persistência do relacionamento `Clinic -> Organization`;
- persistência do relacionamento `ClinicUnit -> Clinic`;
- bloqueio de remoção física de `Organization` com `Clinic` vinculada.

Comandos tentados/executados nesta subfase:

- `dotnet test backend/src/Togo.Infrastructure.Tests/Togo.Infrastructure.Tests.csproj --no-restore`: não executado no ambiente porque o comando `dotnet` não está disponível.
- `dotnet build`: não executado no ambiente porque o comando `dotnet` não está disponível.
- `git diff --check`: executado com sucesso.

## Decisões tomadas

- `Clinic` permanece como escopo primário planejado para isolamento clínico-operacional.
- `ClinicUnit` permanece como escopo complementar.
- A persistência foi limitada a `Organization`, `Clinic` e `ClinicUnit`.
- O limite de 120 caracteres para `Name` foi adotado por consistência com entidades clínicas já existentes.
- Foi usado `Restrict` em relacionamentos hierárquicos para evitar cascade delete perigoso.
- Foram criados índices compostos únicos apenas dentro do escopo pai (`OrganizationId + Name` e `ClinicId + Name`).

## Decisões adiadas

- Adição de `ClinicId` em entidades clínicas existentes fica para a Fase 8.3.
- Propagação controlada de escopo em fluxos clínicos fica para a Fase 8.3.
- Definição de filtros por contexto fica para fases posteriores.
- Autorização contextual e `UserClinicAccess` permanecem para fases futuras.
- Estratégia de seed obrigatório não foi implementada nesta subfase.

## Riscos remanescentes

- O isolamento clínico real em runtime ainda não existe.
- Entidades clínicas como Tutor, Patient, Attendance, MedicalRecord, Prescription e ClinicalEvolution ainda não possuem `ClinicId`.
- Repositórios clínicos ainda não filtram por contexto clínico.
- A migration deve ser validada em ambiente com SDK .NET e provider MySQL disponíveis, pois o ambiente atual não possui o comando `dotnet`.

## O que não foi feito nesta fase

- Não foi alterada API/controller.
- Não foram alterados DTOs públicos.
- Não foi implementado `CurrentClinicalContext`.
- Não foi implementada autorização contextual.
- Não foi criado `UserClinicAccess`.
- Não foram alterados login, autenticação ou claims.
- Não foi propagado `ClinicId` em fluxos clínicos.
- Não foram alterados use cases de Attendance, Patient, Tutor, MedicalRecord, Prescription ou ClinicalEvolution.
- Não foram criados filtros por contexto nos repositories clínicos.
- Não foi implementada auditoria contextual.
- Não foi criado seed obrigatório.
- Não foi criada tela/front-end.
- Não foram implementadas regras avançadas de unidade.

## Próxima fase recomendada

Fase 8.3 — Propagação de `ClinicId` nos fluxos clínicos, de forma controlada e documentada, preparando o início da proteção real do escopo clínico entre as fases 8.3 e 8.6.
