# TOGO — Fase 6.4.6: Encerramento da persistência clínica segura de MedicalRecord

## 1. Objetivo

A Fase 6.4.6 consolida as evidências finais das Fases 6.4.1 a 6.4.5, atualiza o registro vivo de débitos técnicos e encerra formalmente a Fase 6.4 como trilha de persistência clínica segura da vertical `MedicalRecord`.

Esta fase é exclusivamente documental e de governança. Ela não altera código, testes, migrations, banco, `DbContext`, configurações EF, controllers, use cases, repositories, JWT, autorização, frontend ou infraestrutura.

## 2. Contexto da Fase 6.4

A Fase 6.4 tratou os débitos P1 restantes da vertical `MedicalRecord` relacionados à persistência clínica segura:

- **MR-DEBT-001 — Soft Delete ausente**;
- **MR-DEBT-005 — Política de retenção não implementada**;
- **MR-DEBT-006 — DeleteBehavior.Cascade pendente de revisão**.

A trilha foi iniciada após as entregas de autorização granular mínima, autoria clínica e `ClinicalAuditLog` mínimo da Fase 6.3. O objetivo da Fase 6.4 foi reduzir riscos de perda de histórico clínico, exclusão física direta, exclusão indireta por cascade e ausência de decisão formal de retenção.

## 3. Resumo das fases 6.4.1 a 6.4.5

| Fase | Tema | Entrega principal |
|---|---|---|
| 6.4.1 | Planejamento técnico de persistência clínica segura | Definiu estratégia incremental para Soft Delete, revisão de cascades e política inicial de retenção. |
| 6.4.2 | Implementação base de Soft Delete | Adicionou campos, comportamento de domínio, use case interno/controlado, migration e testes para Soft Delete mínimo. |
| 6.4.3 | Queries/filtros de Soft Delete | Ajustou consultas padrão para ignorarem registros logicamente deletados e consolidou testes de repositório/use case/controller. |
| 6.4.4 | Revisão de `DeleteBehavior.Cascade` | Alterou relações clínicas críticas para `Restrict`, manteve `Cascade` apenas onde justificado e criou evidências de infraestrutura. |
| 6.4.5 | Decisão inicial de retenção clínica | Formalizou retenção indefinida inicial para `MedicalRecord` e `ClinicalAuditLog`, sem expurgo automático e sem automação. |

## 4. PRs da Fase 6.4

- PR 149 — Fase 6.4.1: planejamento técnico de persistência clínica segura.
- PR 150 — Fase 6.4.2: implementação base de Soft Delete.
- PR 151 — Fase 6.4.3: queries/filtros de Soft Delete.
- PR 152 — Fase 6.4.4: revisão de `DeleteBehavior.Cascade`.
- PR 153 — Fase 6.4.5: decisão inicial de retenção clínica.

## 5. Débitos tratados

| Débito | Status final na Fase 6.4.6 | Resultado |
|---|---|---|
| MR-DEBT-001 — Soft Delete ausente | Resolvido tecnicamente para Soft Delete clínico mínimo | `MedicalRecord` possui exclusão lógica persistida e consultas padrão ignoram registros deletados. |
| MR-DEBT-005 — Política de retenção não implementada | Resolvido tecnicamente por decisão formal de retenção clínica inicial | Retenção indefinida inicial foi documentada, com preservação de `MedicalRecord` e `ClinicalAuditLog`. |
| MR-DEBT-006 — DeleteBehavior.Cascade pendente de revisão | Resolvido tecnicamente por revisão de cascades clínicos críticos | Relações clínicas críticas foram revisadas para evitar exclusões indiretas destrutivas. |

## 6. Atualizações feitas no registro vivo

O registro vivo `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md` foi atualizado nesta fase para refletir que:

- **MR-DEBT-001** não bloqueia mais produção real como bloqueio ativo isolado, dentro do escopo mínimo de Soft Delete clínico;
- **MR-DEBT-005** não bloqueia mais produção real como bloqueio ativo isolado, considerando a decisão formal de retenção clínica inicial;
- **MR-DEBT-006** não bloqueia mais produção real como bloqueio ativo isolado, considerando a revisão das relações clínicas críticas;
- as evidências finais desses três débitos ficam centralizadas nesta Fase 6.4.6.

## 7. Evidências de Soft Delete

As evidências consolidadas de Soft Delete são:

- `MedicalRecord` passou a possuir `IsDeleted`, `DeletedAt` e `DeletedByUserId`;
- `SoftDelete(Guid deletedByUserId, DateTime deletedAt)` valida autoria, timestamp UTC e tentativa duplicada;
- Soft Delete preserva `CreatedAt`, `CreatedByUserId`, `UpdatedAt`, `UpdatedByUserId`, `GeneralNotes`, `FlagsJson` e `PatientId`;
- a persistência ocorre por update, sem criação de método de exclusão física em repository;
- não foi exposto endpoint público `DELETE`;
- a migration `AddMedicalRecordSoftDelete` adicionou os campos mínimos de exclusão lógica;
- testes de domínio, aplicação e infraestrutura cobrem o comportamento mínimo.

## 8. Evidências de queries/filtros de Soft Delete

As evidências consolidadas de queries/filtros são:

- consultas clínicas padrão deixam de retornar `MedicalRecord` com `IsDeleted = true`;
- `GetByPatientIdAsync` e fluxos dependentes passam a tratar registros deletados como ausentes nos caminhos padrão;
- tentativa de criação após Soft Delete continua bloqueada por existência física do prontuário, preservando unicidade lógica e histórica;
- update de registro logicamente deletado é bloqueado pelos validadores/queries padrão;
- controller e use cases mantêm o comportamento público sem revelar registros deletados nos fluxos clínicos comuns;
- não foi criada consulta administrativa para listar ou carregar registros deletados.

## 9. Evidências de revisão de DeleteBehavior.Cascade

As evidências consolidadas da revisão de cascades são:

- `Patient -> MedicalRecord` foi alterado para `DeleteBehavior.Restrict`;
- `Patient -> Attendance` foi alterado para `DeleteBehavior.Restrict`;
- `Attendance -> ClinicalEvolution` foi alterado para `DeleteBehavior.Restrict`;
- `Attendance -> Prescription` foi alterado para `DeleteBehavior.Restrict`;
- `Patient -> Pet` permaneceu com `Cascade` por modelagem de especialização, com decisão documentada para reavaliação se o escopo clínico mudar;
- relações internas de item dependente de prescrição permaneceram com `Cascade` onde o ciclo de vida é estritamente dependente;
- a migration `ReviewClinicalCascadeDeleteBehavior` registrou a mudança;
- testes de infraestrutura cobrem a política de cascade revisada.

## 10. Evidências de política inicial de retenção clínica

As evidências consolidadas da política inicial de retenção são:

- foi adotada retenção indefinida inicial para `MedicalRecord`;
- registros com Soft Delete permanecem fisicamente preservados;
- `ClinicalAuditLog` permanece preservado como evidência mínima de rastreabilidade clínica;
- expurgo físico não foi implementado;
- arquivamento não foi implementado;
- automação de retenção, arquivamento ou expurgo foi explicitamente proibida nesta fase;
- job, scheduler, worker, filas, RabbitMQ, Redis, outbox, endpoint administrativo, frontend e script operacional ficaram fora do escopo;
- a política atual é conservadora e poderá exigir revisão futura por compliance, regulação, produto, operação ou volume.

## 11. Estado final da persistência clínica segura

Ao final da Fase 6.4, `MedicalRecord` possui uma base clínica mínima mais segura, composta por:

- autorização granular mínima;
- autoria clínica;
- `AuditLog` mínimo;
- Soft Delete;
- filtros padrão que ignoram registros deletados;
- revisão de cascades críticos;
- decisão inicial de retenção clínica;
- `CreatedAt` e `UpdatedAt`;
- `CreatedByUserId` e `UpdatedByUserId`;
- `IsDeleted`, `DeletedAt` e `DeletedByUserId`;
- retenção indefinida inicial.

Esse estado não representa uma plataforma clínica final, mas remove os bloqueios P1 específicos de persistência clínica segura mapeados para a vertical `MedicalRecord` no escopo mínimo/incremental da Fase 6.4.

## 12. Limites explícitos da solução

A Fase 6.4 não entrega e não presume a existência de:

- endpoint público `DELETE`;
- consulta administrativa de registros deletados;
- restore de registros deletados;
- expurgo físico;
- job, scheduler ou worker;
- automação de retenção;
- arquivamento;
- auditoria de leitura;
- auditoria de acesso negado;
- transação única entre operação principal e `AuditLog`;
- automação de arquivamento;
- rotina operacional de retenção;
- tela frontend administrativa;
- script de remoção física.

## 13. Arquivos principais alterados durante a Fase 6.4

A Fase 6.4 alterou ou criou, entre outros, os seguintes arquivos principais:

- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/SoftDeleteMedicalRecordUseCase.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/AttendanceConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalEvolutionConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/PrescriptionConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260604120000_AddMedicalRecordSoftDelete.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260604120000_AddMedicalRecordSoftDelete.Designer.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260605120000_ReviewClinicalCascadeDeleteBehavior.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260605120000_ReviewClinicalCascadeDeleteBehavior.Designer.cs`;
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`;
- `backend/src/Togo.Domain.Tests/MedicalRecordTests.cs`;
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/SoftDeleteMedicalRecordUseCaseTests.cs`;
- `backend/src/Togo.Infrastructure.Tests/Repositories/MedicalRecordRepositoryTests.cs`;
- `backend/src/Togo.Infrastructure.Tests/Persistence/ClinicalCascadeDeleteBehaviorTests.cs`;
- `docs/clinical-core/PHASE_06_04_01_MEDICAL_RECORD_SAFE_PERSISTENCE_PLANNING.md`;
- `docs/clinical-core/PHASE_06_04_02_MEDICAL_RECORD_SOFT_DELETE_IMPLEMENTATION.md`;
- `docs/clinical-core/PHASE_06_04_03_MEDICAL_RECORD_SOFT_DELETE_QUERY_FILTERS.md`;
- `docs/clinical-core/PHASE_06_04_04_MEDICAL_RECORD_CASCADE_DELETE_REVIEW.md`;
- `docs/clinical-core/PHASE_06_04_05_MEDICAL_RECORD_RETENTION_POLICY_DECISION.md`;
- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`.

Nesta Fase 6.4.6, somente arquivos em `docs/clinical-core` foram alterados.

## 14. Riscos remanescentes

Mesmo após a Fase 6.4, permanecem riscos e limites relevantes:

- não há endpoint público `DELETE`, portanto o Soft Delete existe como base técnica controlada, não como fluxo público completo;
- não há consulta administrativa de registros deletados;
- não há restore de registros deletados;
- não há expurgo físico, arquivamento ou automação de retenção;
- a política de retenção é inicial, conservadora e pode exigir revisão jurídica, regulatória, de compliance, produto ou operação;
- crescimento de volume pode exigir estratégia futura de arquivamento, particionamento, storage tiering, índices ou retenção operacional;
- novas entidades e relações clínicas futuras precisam seguir a política de `Restrict` em dados clínicos críticos;
- `Patient -> Pet` permanece com `Cascade` por decisão de modelagem atual e deve ser reavaliado se `Pet` passar a carregar histórico clínico independente;
- auditoria de leitura e auditoria de acesso negado ainda não existem;
- ainda não há transação única garantindo atomicidade entre operação principal e `AuditLog`;
- a liberação para produção real pode depender de validações macro além da vertical `MedicalRecord`.

## 15. Impacto sobre liberação para produção real

A Fase 6.4 remove os bloqueios P1 específicos de persistência clínica segura mapeados para `MedicalRecord`:

- Soft Delete clínico mínimo foi implementado;
- queries padrão passaram a ignorar registros logicamente deletados;
- cascades clínicos críticos foram revisados;
- retenção clínica inicial foi formalmente decidida.

Os principais P1 originais da vertical `MedicalRecord` foram tecnicamente tratados em escopo mínimo/incremental ao longo das Fases 6.2, 6.3 e 6.4. Mesmo assim, esta decisão não declara liberação irrestrita para produção real com dados clínicos sensíveis.

Produção real ainda pode depender de validação adicional de segurança, infraestrutura, ambiente, LGPD/compliance, revisão de produto, critérios de operação e próximas fases macro do roadmap. A conclusão correta é que a Fase 6.4 remove os bloqueios P1 específicos de persistência clínica segura mapeados para `MedicalRecord`, sem substituir uma avaliação final de produção.

## 16. Critérios de aceite

A Fase 6.4.6 atende aos critérios de aceite quando:

- o registro vivo foi atualizado;
- MR-DEBT-001 foi marcado como resolvido tecnicamente;
- MR-DEBT-005 foi marcado como resolvido tecnicamente;
- MR-DEBT-006 foi marcado como resolvido tecnicamente;
- as Fases 6.4.1 a 6.4.5 foram referenciadas;
- os PRs 149 a 153 foram referenciados;
- evidências de Soft Delete foram consolidadas;
- evidências de filtros de Soft Delete foram consolidadas;
- evidências de revisão de cascades foram consolidadas;
- evidências de política de retenção foram consolidadas;
- limites da solução foram documentados;
- riscos remanescentes foram documentados;
- próxima Fase 6.5 foi recomendada;
- nenhuma implementação nova foi feita;
- nenhuma migration foi criada;
- somente arquivos `docs/clinical-core` foram alterados;
- `git diff --check` passou.

## 17. Decisão final da Fase 6.4

A Fase 6.4 está formalmente encerrada como trilha de persistência clínica segura de `MedicalRecord`.

Decisão final:

- **MR-DEBT-001** fica resolvido tecnicamente para Soft Delete clínico mínimo;
- **MR-DEBT-005** fica resolvido tecnicamente por decisão formal de retenção clínica inicial;
- **MR-DEBT-006** fica resolvido tecnicamente por revisão de cascades clínicos críticos;
- os limites explícitos da solução permanecem registrados para evitar interpretação de liberação funcional ou operacional maior do que a entregue;
- a vertical `MedicalRecord` avança para a próxima trilha de hardening sem bloqueio P1 ativo isolado de persistência clínica segura.

## 18. Próxima fase recomendada

A próxima fase recomendada é a **Fase 6.5 — Integridade e evolução de schema**.

A fase inicial recomendada é a **Fase 6.5.1 — Planejamento técnico de integridade física e validação estrutural MedicalRecord**.

Débitos inicialmente envolvidos:

- **MR-DEBT-007 — Índice único em MedicalRecords.PatientId ausente**;
- **MR-DEBT-009 — FlagsJson flexível**.
