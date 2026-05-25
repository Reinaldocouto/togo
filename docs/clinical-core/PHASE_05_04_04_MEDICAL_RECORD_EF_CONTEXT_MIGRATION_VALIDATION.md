# TOGO — Fase 5.4.4: Validação EF/AppDbContext/Migration existente de MedicalRecord

## Resumo da Subfase 5.4

Subfase 5.4 — Infrastructure MedicalRecord

Planejamento:
- 5.4.1 — Repository EF Core de MedicalRecord.
- 5.4.2 — Registro de DI/Program.cs para MedicalRecord.
- 5.4.3 — Testes de Infrastructure de MedicalRecord.
- 5.4.4 — Validação EF/AppDbContext/Migration existente.
- 5.4.5 — Documentação final da camada Infrastructure.
- 5.4.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Validar o estado atual do mapeamento EF Core da entidade `MedicalRecord` sem criar nova migration e sem alterar banco, código de produção ou testes.

Esta validação confirma:
- presença de `DbSet<MedicalRecord>`;
- aplicação da configuração EF;
- existência da tabela `MedicalRecords` na migration vigente;
- colunas e tipos esperados;
- FK com `Patient`;
- índice em `PatientId`;
- `DeleteBehavior` atual;
- coerência com repository e testes de Infrastructure;
- riscos e débitos técnicos remanescentes.

## Contexto

- `MedicalRecord` já está consolidado no Domain.
- A camada Application de MedicalRecord foi encerrada.
- O repository concreto foi criado na fase 5.4.1.
- O DI foi registrado na fase 5.4.2.
- Os testes de Infrastructure passaram na fase 5.4.3.
- Nesta fase, faltava validar formalmente `AppDbContext`, `MedicalRecordConfiguration`, migration existente e snapshot.
- Esta fase não implementa alteração estrutural no banco.

## Validação do AppDbContext

Arquivo inspecionado: `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`.

Evidências:
- Existe `DbSet<MedicalRecord>`.
- Nome da propriedade: `MedicalRecords`.
- O contexto usa `modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);`.
- O mapeamento de `MedicalRecord` é aplicado por varredura de assembly, sem registro manual explícito da configuração.
- `MedicalRecord` está integrado ao contexto e disponível para query/update via EF Core.

Conclusão:
- `AppDbContext` está adequado para a vertical MedicalRecord no estado atual.
- Não há necessidade de alteração nesta fase.

## Validação da MedicalRecordConfiguration

Arquivo inspecionado: `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`.

Evidências de mapeamento:
- Tabela mapeada: `MedicalRecords`.
- Chave primária: `Id`.
- `Id` com `ValueGeneratedOnAdd()`.
- `PatientId` obrigatório (`IsRequired()`).
- `GeneralNotes` com tipo SQL `text`.
- `FlagsJson` com tipo SQL `longtext`.
- `UpdatedAt` obrigatório (`IsRequired()`).
- Relacionamento com `Patient` via `HasForeignKey(m => m.PatientId)`.
- `DeleteBehavior` configurado como `DeleteBehavior.Cascade`.
- Índice em `PatientId` criado com `HasIndex(m => m.PatientId)`.
- O índice em `PatientId` não é único (não há `IsUnique()`).

Avaliação técnica:
- Aderência ao MVP: adequada para persistência de prontuário principal e consultas por paciente.
- Risco de `DeleteBehavior.Cascade`: possível exclusão em cascata de prontuário ao excluir `Patient`, sensível para contexto clínico.
- Risco de `PatientId` não único: permite duplicidade física de prontuários por paciente em concorrência.
- Risco de `FlagsJson` como `longtext`: alta flexibilidade sem contrato estrutural rígido.
- Ausência de limite de tamanho explícito para `GeneralNotes`/`FlagsJson`.
- Ausência de Soft Delete.
- Ausência de AuditLog.

## Validação da migration existente

Arquivo inspecionado: `backend/src/Togo.Infrastructure/Migrations/20260428200839_AddClinicalCoreEntities.cs`.

Evidências:
- A tabela `MedicalRecords` é criada na migration.
- Colunas presentes:
  - `Id` (`bigint`, não nulo, identity/auto increment);
  - `PatientId` (`bigint`, não nulo);
  - `GeneralNotes` (`text`, nulo);
  - `FlagsJson` (`longtext`, nulo);
  - `UpdatedAt` (`datetime(6)`, não nulo).
- PK: `PK_MedicalRecords` em `Id`.
- FK: `FK_MedicalRecords_Patients_PatientId` para `Patients(Id)`.
- `onDelete`: `ReferentialAction.Cascade`.
- Índice em `PatientId`: `IX_MedicalRecords_PatientId`.
- Não há índice único em `PatientId`.
- Há coerência geral com `MedicalRecordConfiguration`.

Conclusão:
- A migration vigente está consistente com o mapeamento configurado para MedicalRecord.
- Não foi criada nova migration nesta fase.

## Validação do AppDbContextModelSnapshot

Arquivo inspecionado: `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`.

Evidências:
- `MedicalRecords` está presente no snapshot.
- O snapshot reflete:
  - `PatientId` como `bigint`;
  - `GeneralNotes` como `text`;
  - `FlagsJson` como `longtext`;
  - `UpdatedAt` como `datetime(6)`.
- FK para `Patients` e índice em `PatientId` aparecem no snapshot.
- Não foi identificada divergência relevante entre snapshot, migration e configuration para MedicalRecord.

## Validação com MedicalRecordRepository

Arquivo inspecionado: `backend/src/Togo.Infrastructure/Repositories/MedicalRecordRepository.cs`.

Coerência repository x mapeamento:
- `GetByIdAsync` depende de `Id` e está coerente com PK em `Id`.
- `GetByPatientIdAsync` depende de `PatientId`; índice existente reduz custo de busca.
- `ExistsByPatientIdAsync` depende de `PatientId` e também se beneficia do índice.
- `AddAsync` depende da existência de `MedicalRecords` e de FK válida para `Patients`.
- `UpdateAsync` depende do mecanismo de tracking/update do EF e persistência em `MedicalRecords`.
- Leitura com `AsNoTracking` foi implementada para consultas.

Conclusão:
- O repository está coerente com o mapeamento EF atual de MedicalRecord.

## Validação com testes de Infrastructure

Arquivo inspecionado: `backend/src/Togo.Infrastructure.Tests/Repositories/MedicalRecordRepositoryTests.cs`.

Coberturas observadas:
- persistência (`AddAsync`);
- consulta por `Id`;
- consulta por `PatientId`;
- verificação de existência por `PatientId`;
- update (`UpdateAsync` + `UpdateNotes`);
- validação de `AsNoTracking` em métodos de leitura;
- uso de `Patient` real de suporte para respeitar FK;
- execução com SQLite in-memory;
- sem banco real.

Conclusão:
- Os testes reduzem risco funcional de repository/mapeamento.
- Os testes não substituem revisão formal de migration/schema real em provider alvo.

## Análise de riscos técnicos

Riscos identificados:
- `PatientId` não único no banco para `MedicalRecords`.
- risco de duplicidade em concorrência (regra de unicidade apenas lógica em Application).
- `DeleteBehavior.Cascade` em entidade clínica sensível.
- ausência de Soft Delete.
- ausência de AuditLog.
- `FlagsJson` flexível sem validação estrutural forte.
- ausência de `CreatedAt` em `MedicalRecord`.
- ausência de controle de autoria (`CreatedBy`/`UpdatedBy`).
- ausência de `CancellationToken` no contract de repository.
- ausência de política de retenção de dados clínicos implementada em nível técnico.

## Decisões técnicas da fase

Nesta fase 5.4.4 foi decidido:
- não criar migration agora;
- não alterar `AppDbContext` agora;
- não alterar `MedicalRecordConfiguration` agora;
- não alterar `DeleteBehavior` agora;
- não criar índice único agora;
- não implementar Soft Delete agora;
- não implementar AuditLog agora;
- seguir para documentação final da Infrastructure antes de abrir API;
- manter `PatientId` não-único e cascade delete como débitos técnicos rastreados.

## Débitos técnicos identificados

| Débito | Evidência | Risco | Fase futura recomendada |
|---|---|---|---|
| Índice único em `MedicalRecords.PatientId` | Configuration/migration criam apenas índice não único | Duplicidade de prontuário em concorrência | Fase de hardening de dados clínicos antes/na 5.5 |
| Revisão de `DeleteBehavior.Cascade` | FK `MedicalRecords -> Patients` com cascade | Exclusão em cascata de histórico clínico | Fase de revisão de integridade clínica |
| Soft Delete | Diretriz clínica já registrada e não implementada | Perda de histórico com delete físico | Fase dedicada de persistência clínica |
| AuditLog | Não há trilha de auditoria em MedicalRecord | Baixa rastreabilidade de alterações | Fase dedicada de auditoria |
| `CreatedAt` em `MedicalRecord` | Entidade possui apenas `UpdatedAt` | Perda de contexto temporal de criação | Evolução de schema clínico |
| Controle de autoria | Sem `CreatedBy`/`UpdatedBy` | Falta de responsabilização por alteração | Fase de segurança/auditoria |
| Validação estrutural de `FlagsJson` | Campo `longtext` flexível | Inconsistência estrutural de dados clínicos | Evolução de domain/validation |
| `CancellationToken` no repository | Métodos async sem token | Menor controle de cancelamento cooperativo | Evolução Application/Infrastructure |
| Política de retenção de dados clínicos | Diretriz existe, implementação não | Risco regulatório/operacional | Fase de compliance clínica |
| Roles/permissões finas na API | API ainda não implementada | Acesso além do mínimo necessário | Fase 5.5 (API MedicalRecord) |

## Critérios de aceite

A fase é considerada concluída com:
- validação de `AppDbContext`;
- validação de `MedicalRecordConfiguration`;
- validação da migration existente;
- validação do snapshot;
- validação de coerência com repository;
- validação de coerência com testes de Infrastructure;
- riscos técnicos listados;
- débitos técnicos registrados;
- nenhuma migration criada;
- nenhum código de produção alterado;
- nenhum teste alterado;
- nenhum banco real alterado;
- documentação da fase criada;
- `git diff --check` sem problemas;
- `dotnet build backend/Togo.sln` passando;
- `dotnet test backend/Togo.sln` passando.

## Fora do escopo

Esta fase não implementa:
- alteração de `AppDbContext`;
- alteração de EF Configuration;
- alteração de migration;
- nova migration;
- `database update`;
- alteração de banco real;
- repository;
- DI/`Program.cs`;
- controller;
- API;
- endpoints;
- Soft Delete;
- AuditLog;
- índice único;
- roles/permissões;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## Próxima fase recomendada

**Fase 5.4.5 — Documentação final da camada Infrastructure MedicalRecord.**

Objetivo:
Consolidar repository concreto, DI, testes de Infrastructure, validação EF/AppDbContext/Migration, riscos e autorização para avançar para a Fase 5.5 — API MedicalRecord.

## Validações obrigatórias

Comandos obrigatórios da fase:
- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`
