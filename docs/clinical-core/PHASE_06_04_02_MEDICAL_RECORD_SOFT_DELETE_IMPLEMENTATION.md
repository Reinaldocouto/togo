# TOGO — Fase 6.4.2: Implementação base de Soft Delete em MedicalRecord

## 1. Contexto da Fase 6.4

A Fase 6.4 trata os débitos P1 remanescentes da vertical `MedicalRecord` relacionados à persistência clínica segura. Após a Fase 6.3, a vertical já possuía autorização granular mínima, autoria clínica de criação/atualização e `ClinicalAuditLog` mínimo para criação e atualização, mas a produção real continuava bloqueada por riscos de persistência e preservação histórica.

Os débitos P1 em aberto na Fase 6.4 são:

- **MR-DEBT-001 — Soft Delete ausente**;
- **MR-DEBT-005 — Política de retenção não implementada**;
- **MR-DEBT-006 — DeleteBehavior.Cascade pendente de revisão**.

## 2. Referência explícita ao MR-DEBT-001

Esta fase trata diretamente o débito **MR-DEBT-001 — Soft Delete ausente**. O objetivo é criar a base técnica para exclusão lógica de `MedicalRecord`, preservando o registro físico, autoria clínica, timestamps e payload clínico persistido.

## 3. Relação com a Fase 6.4.1

A Fase 6.4.1 documentou o planejamento técnico de persistência clínica segura no arquivo `docs/clinical-core/PHASE_06_04_01_MEDICAL_RECORD_SAFE_PERSISTENCE_PLANNING.md`. A Fase 6.4.2 executa a primeira implementação incremental desse plano, limitada ao Soft Delete mínimo de `MedicalRecord`.

## 4. Objetivo da fase

Implementar Soft Delete base em `MedicalRecord` com:

- campos persistidos mínimos;
- comportamento de domínio explícito;
- use case interno/controlado de aplicação;
- autoria pelo usuário autenticado atual;
- persistência por update, sem remoção física;
- migration mínima;
- testes de domínio, aplicação e infraestrutura.

## 5. Decisão técnica adotada

Foi adotada a opção conservadora: **Soft Delete base implementado em domínio/schema/use case/testes, sem exclusão física e sem endpoint público inicial**.

A tentativa de Soft Delete duplicado **não é idempotente** nesta fase. O domínio lança `InvalidOperationException` com mensagem controlada, e o use case converte essa situação em `ApplicationResultType.Conflict`. Essa decisão evita apagar evidências originais de deleção e força uma decisão explícita em fases futuras caso seja necessário reprocessar ou reverter registros deletados.

## 6. Arquivos criados/alterados

Arquivos alterados:

- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`;
- `backend/src/Togo.Api/Program.cs`;
- `backend/src/Togo.Domain.Tests/MedicalRecordTests.cs`;
- `backend/src/Togo.Infrastructure.Tests/Repositories/MedicalRecordRepositoryTests.cs`.

Arquivos criados:

- `backend/src/Togo.Application/MedicalRecords/UseCases/SoftDeleteMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/SoftDeleteMedicalRecordUseCaseTests.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260604120000_AddMedicalRecordSoftDelete.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260604120000_AddMedicalRecordSoftDelete.Designer.cs`;
- `docs/clinical-core/PHASE_06_04_02_MEDICAL_RECORD_SOFT_DELETE_IMPLEMENTATION.md`.

## 7. Campos de Soft Delete adicionados

`MedicalRecord` passa a possuir os seguintes campos mínimos:

- `IsDeleted: bool`;
- `DeletedAt: DateTime?`;
- `DeletedByUserId: Guid?`.

Não foi adicionado `DeletedReason` nesta fase porque não há regra de negócio clara para motivo de exclusão lógica. A decisão foi manter o schema mínimo e reduzir acoplamento prematuro.

## 8. Comportamento de domínio implementado

Foi adicionado o método:

```csharp
SoftDelete(Guid deletedByUserId, DateTime deletedAt)
```

Regras implementadas:

- rejeita `Guid.Empty` para `deletedByUserId`;
- rejeita `DateTime` default para `deletedAt`;
- exige `DateTimeKind.Utc` em `deletedAt`;
- marca `IsDeleted = true`;
- preenche `DeletedAt`;
- preenche `DeletedByUserId`;
- rejeita tentativa duplicada de Soft Delete com erro controlado;
- preserva `CreatedAt`;
- preserva `CreatedByUserId`;
- preserva `UpdatedAt`;
- preserva `UpdatedByUserId`;
- preserva `GeneralNotes`;
- preserva `FlagsJson`;
- preserva o vínculo com `PatientId`.

## 9. Estratégia de autoria do Soft Delete

A autoria do Soft Delete é própria e independente da autoria clínica de criação/atualização. O `SoftDeleteMedicalRecordUseCase` resolve o usuário autenticado atual via `ICurrentUserService` e usa `currentUser.UserId` como `DeletedByUserId`.

O Soft Delete não sobrescreve `UpdatedByUserId` nem `UpdatedAt`, porque a decisão desta fase foi preservar a autoria clínica da última atualização como informação distinta da autoria de exclusão lógica.

## 10. Impacto em EF/migration

`MedicalRecordConfiguration` passa a mapear:

- `IsDeleted` obrigatório com default `false`;
- `DeletedAt` nullable;
- `DeletedByUserId` nullable.

A migration `AddMedicalRecordSoftDelete` adiciona:

- `IsDeleted` com default `false` para registros existentes;
- `DeletedAt` nullable, sem default fake;
- `DeletedByUserId` nullable, sem default `Guid.Empty`.

Não foram criadas constraints complexas nesta fase.

## 11. Impacto em use case/repository

Foi criado `SoftDeleteMedicalRecordUseCase` com fluxo controlado:

1. valida `patientId` via `MedicalRecordPatientExistsValidator`;
2. valida existência de `MedicalRecord` via `MedicalRecordExistsValidator`;
3. carrega o registro via `GetByPatientIdAsync`;
4. resolve usuário atual via `ICurrentUserService`;
5. chama `medicalRecord.SoftDelete(currentUser.UserId, DateTime.UtcNow)`;
6. persiste por `IMedicalRecordRepository.UpdateAsync`.

Nenhum método de exclusão física foi criado em `IMedicalRecordRepository` ou em `MedicalRecordRepository`. A persistência usa o padrão de update já existente.

## 12. Decisão sobre endpoint DELETE público

Não foi criado endpoint `DELETE` público nesta fase. Também não foi criada rota pública para deletar `MedicalRecord`.

O use case existe como base técnica interna para evolução controlada. Uma rota pública exigirá decisão posterior sobre autorização explícita, semântica de resposta, auditoria, comportamento de consultas e integração com filtros.

## 13. Decisão sobre AuditLog de Soft Delete

Foi adotada a **Opção A — sem AuditLog de Soft Delete nesta fase**.

Justificativa:

- mantém o escopo focado em MR-DEBT-001;
- reduz acoplamento com a trilha de auditoria até que a semântica de endpoint/fluxo seja fechada;
- evita introduzir evento `MedicalRecord.SoftDeleted` antes de decidir payload mínimo, policy e comportamento transacional.

A criação de um evento `MedicalRecord.SoftDeleted` deve ser avaliada em fase futura, preferencialmente junto da consolidação de filtros/queries ou antes de qualquer endpoint público.

## 14. Testes criados/ajustados

Testes de domínio:

- novo `MedicalRecord` inicia com `IsDeleted = false`;
- novo `MedicalRecord` inicia com `DeletedAt = null`;
- novo `MedicalRecord` inicia com `DeletedByUserId = null`;
- `SoftDelete` marca `IsDeleted = true`;
- `SoftDelete` preenche `DeletedAt`;
- `SoftDelete` preenche `DeletedByUserId`;
- `SoftDelete` rejeita `Guid.Empty`;
- `SoftDelete` rejeita `DateTime` default;
- `SoftDelete` rejeita data não UTC;
- `SoftDelete` preserva autoria/timestamps de criação e atualização;
- `SoftDelete` preserva `GeneralNotes`, `FlagsJson` e `PatientId`;
- Soft Delete duplicado retorna erro controlado.

Testes de aplicação:

- use case resolve usuário autenticado atual;
- use case persiste `IsDeleted = true`;
- use case persiste `DeletedByUserId` com `currentUser.UserId`;
- use case persiste `DeletedAt` UTC;
- use case falha se paciente não existir;
- use case falha se `MedicalRecord` não existir;
- use case falha de forma segura se usuário atual não puder ser resolvido;
- use case não chama adição nem exclusão física;
- Soft Delete duplicado retorna conflito.

Testes de infraestrutura:

- repository persiste campos de Soft Delete;
- configuração aceita `DeletedAt` e `DeletedByUserId` nulos;
- `IsDeleted` é obrigatório e tem default `false`;
- criação mantém campos de Soft Delete em estado inicial não deletado.

## 15. O que ainda não foi implementado

Não foi implementado nesta fase:

- filtro global de Soft Delete;
- exclusão de registros deletados das queries padrão;
- bloqueio de update sobre registro já deletado;
- endpoint público `DELETE`;
- evento `MedicalRecord.SoftDeleted` em `ClinicalAuditLog`;
- consulta administrativa de registros deletados;
- retenção clínica;
- job, scheduler, worker, arquivamento ou expurgo;
- revisão de `DeleteBehavior.Cascade`.

## 16. Riscos remanescentes

Riscos ainda existentes após esta fase:

- consultas padrão ainda podem retornar registros logicamente deletados até a Fase 6.4.3;
- updates ainda não possuem política consolidada para registros já deletados;
- ausência de `MedicalRecord.SoftDeleted` no AuditLog reduz rastreabilidade operacional do evento até fase futura;
- `DeleteBehavior.Cascade` ainda não foi revisto;
- retenção clínica ainda não foi definida ou automatizada.

## 17. Critérios de aceite

Critérios atendidos nesta fase:

- `MedicalRecord` possui `IsDeleted` persistido;
- `MedicalRecord` possui `DeletedAt` nullable;
- `MedicalRecord` possui `DeletedByUserId` nullable;
- `MedicalRecord` inicia como não deletado;
- existe comportamento de domínio para Soft Delete;
- Soft Delete usa usuário autenticado atual no fluxo de aplicação;
- Soft Delete não faz exclusão física;
- Soft Delete preserva autoria e timestamps de criação;
- Soft Delete preserva autoria e timestamps de última atualização;
- migration foi criada;
- testes de domínio cobrem invariantes principais;
- testes de aplicação cobrem o fluxo controlado;
- testes de infraestrutura cobrem persistência dos campos;
- não há endpoint `DELETE` público;
- não há retenção implementada;
- não há alteração de cascade;
- documentação da fase foi criada.

## 18. Fora do escopo

Permaneceram fora do escopo:

- criação de rota pública para deletar `MedicalRecord`;
- exclusão física;
- remoção de registros do banco;
- retenção clínica;
- jobs, schedulers, workers ou expurgo;
- revisão de `DeleteBehavior.Cascade`;
- alterações em relações clínicas;
- alteração de autorização granular existente;
- alteração de JWT;
- alteração de `User/Profile`;
- frontend;
- Docker, Redis, RabbitMQ ou Kubernetes;
- auditoria de leitura;
- auditoria de acesso negado;
- consulta administrativa de registros deletados;
- tela para registros deletados.

## 19. Próxima fase recomendada

A próxima fase recomendada é a **Fase 6.4.3 — Ajustes de queries/filtros e testes de Soft Delete**.

Objetivo recomendado da 6.4.3:

- garantir que consultas padrão de `MedicalRecord` não retornem registros deletados indevidamente;
- revisar comportamento de atualização sobre registros deletados;
- avaliar o momento correto para `MedicalRecord.SoftDeleted` em `ClinicalAuditLog`;
- consolidar evidências de Soft Delete antes da revisão de `DeleteBehavior.Cascade`.
