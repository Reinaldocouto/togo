# TOGO — Fase 6.4.1: Planejamento técnico de persistência clínica segura para MedicalRecord

## 1. Objetivo

A Fase 6.4.1 reabre formalmente o contexto da Fase 6.4 — Persistência clínica, Soft Delete e retenção — após o encerramento da Fase 6.3 pela PR 148 e pelo documento `docs/clinical-core/PHASE_06_03_06_MEDICAL_RECORD_AUTHORSHIP_AUDIT_CLOSURE.md`.

O objetivo desta subfase é definir, antes de qualquer implementação, uma estratégia incremental e governável para tratar os três débitos P1 restantes da vertical `MedicalRecord` relacionados à persistência clínica segura:

- **MR-DEBT-001 — Soft Delete ausente**;
- **MR-DEBT-005 — Política de retenção não implementada**;
- **MR-DEBT-006 — DeleteBehavior.Cascade pendente de revisão**.

Esta fase é exclusivamente documental e de governança técnica. Ela não altera domínio, banco, migrations, DbContext, configurações EF, controllers, use cases, testes, frontend ou infraestrutura operacional.

## 2. Fontes consideradas

Este planejamento foi elaborado a partir das seguintes fontes principais do projeto:

- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`;
- `docs/clinical-core/PHASE_06_03_06_MEDICAL_RECORD_AUTHORSHIP_AUDIT_CLOSURE.md`;
- `docs/clinical-core/PHASE_06_03_05_MEDICAL_RECORD_AUTHORSHIP_AUDIT_EVIDENCE.md`;
- `docs/ROADMAP_TO_PHASE_12.md`;
- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`;
- `backend/src/Togo.Domain/Entities/ClinicalAuditLog.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Migrations`;
- `backend/src/Togo.Api/Controllers/MedicalRecordsController.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/CreateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/UpdateMedicalRecordUseCase.cs`.

## 3. Estado atual pós-Fase 6.3

Após o encerramento da Fase 6.3, a vertical `MedicalRecord` possui uma base clínica mínima mais segura do que o MVP original:

- autorização granular mínima por policies para leitura, criação e atualização;
- profile JWT aplicado ao fluxo de autorização;
- autoria clínica mínima em criação e atualização;
- `CreatedAt` e `UpdatedAt` persistidos;
- `CreatedByUserId` e `UpdatedByUserId` persistidos;
- `ClinicalAuditLog` mínimo para eventos `MedicalRecord.Created` e `MedicalRecord.Updated`;
- metadata mínima de auditoria sem payload clínico sensível.

Mesmo com esses avanços, a vertical ainda não está liberada para produção real com dados clínicos sensíveis porque permanecem abertos os seguintes bloqueios P1:

- **MR-DEBT-001 — Soft Delete ausente**: `MedicalRecord` ainda não possui exclusão lógica persistida;
- **MR-DEBT-005 — Política de retenção não implementada**: ainda não há decisão técnica formal sobre preservação, arquivamento ou eventual expurgo de dados clínicos;
- **MR-DEBT-006 — DeleteBehavior.Cascade pendente de revisão**: a configuração relacional clínica ainda depende de revisão para evitar apagamento de histórico por efeito colateral.

Portanto, a produção real permanece bloqueada não por ausência de autorização, autoria ou auditoria mínima, mas pela ausência de garantias suficientes de persistência clínica segura, preservação histórica e integridade referencial contra exclusões destrutivas.

## 4. Riscos atuais

Os riscos atuais da vertical `MedicalRecord` são arquiteturais e evolutivos, ainda que não exista endpoint `DELETE` público no controller atual.

- Sem Soft Delete, uma exclusão física futura de `MedicalRecord` poderia destruir histórico clínico, autoria, timestamps e rastreabilidade operacional associados ao prontuário.
- Sem uma política de retenção clínica, não existe regra técnica clara para decidir por quanto tempo preservar dados, quando arquivar informações, quando bloquear expurgo e em quais condições uma remoção física poderia ser considerada aceitável.
- Com `DeleteBehavior.Cascade` sem revisão, exclusões em entidades relacionadas podem apagar dados clínicos por efeito colateral, especialmente em relações entre `Patient`, `MedicalRecord` e futuras entidades clínicas como `Attendance`, `ClinicalEvolution` e `Prescription`.
- Mesmo sem endpoint `DELETE` hoje, o risco arquitetural existe porque migrations, repositórios, rotinas administrativas, scripts futuros ou novas features podem acionar exclusões físicas ou cascades sem uma política clínica segura previamente definida.

## 5. Diferença entre Soft Delete, retenção e cascade

Soft Delete, retenção e cascade são temas relacionados à persistência segura, mas não são a mesma coisa.

### 5.1. Soft Delete

Soft Delete é uma estratégia de exclusão lógica. O registro é marcado como removido, inativo ou indisponível para fluxos padrão, mas não é apagado fisicamente do banco. No contexto de `MedicalRecord`, Soft Delete deve preservar histórico clínico, autoria e rastreabilidade.

### 5.2. Retenção clínica

Retenção clínica define por quanto tempo dados clínicos devem ser preservados, arquivados ou eventualmente expurgados. Ela é uma política de governança e compliance, não apenas uma flag de banco. A retenção pode conviver com Soft Delete: um registro logicamente removido pode continuar retido por prazo indefinido ou por prazo definido por regra regulatória/produto.

### 5.3. DeleteBehavior.Cascade

`DeleteBehavior.Cascade` define o comportamento de exclusão relacional no banco/EF. Quando configurado, a exclusão de uma entidade principal pode apagar automaticamente entidades dependentes. Em domínio clínico, esse comportamento precisa ser tratado com cautela porque pode remover histórico por consequência indireta de outra exclusão.

### 5.4. Relação entre os três temas

Os três assuntos se complementam:

- Soft Delete evita exclusão física direta de `MedicalRecord`;
- retenção define a regra de preservação, arquivamento ou expurgo ao longo do tempo;
- revisão de cascade impede que exclusões relacionais apaguem dados clínicos indiretamente.

Resolver apenas um deles não elimina os demais riscos. A Fase 6.4 deve tratá-los de forma coordenada.

## 6. Estratégia recomendada para Soft Delete

A implementação futura de **MR-DEBT-001 — Soft Delete ausente** deve ser conservadora e incremental. Nenhuma implementação é feita nesta subfase.

### 6.1. Campos candidatos

A modelagem futura deve avaliar a inclusão dos seguintes campos em `MedicalRecord`:

- `IsDeleted: bool` — indica se o registro foi logicamente removido;
- `DeletedAt: DateTime?` — registra o instante UTC da exclusão lógica;
- `DeletedByUserId: Guid?` — registra o usuário autenticado responsável pela exclusão lógica;
- `DeletedReason: string?` — campo opcional para justificativa clínica/operacional, caso faça sentido para o produto e para a governança.

### 6.2. Autoria e usuário autenticado

O fluxo futuro de Soft Delete deve usar usuário autenticado via `ICurrentUserService`, evitando autoria fake, hardcoded ou anônima. O usuário responsável pelo Soft Delete deve ser persistido de forma equivalente ao padrão já usado para criação e atualização.

### 6.3. Exclusão física

A abordagem recomendada é evitar exclusão física de `MedicalRecord`. Mesmo após a introdução de Soft Delete, a remoção física deve continuar inexistente ou explicitamente bloqueada para fluxos clínicos padrão.

### 6.4. Preservação de autoria e timestamps existentes

A implementação futura não deve sobrescrever indevidamente `CreatedAt`, `CreatedByUserId`, `UpdatedAt` e `UpdatedByUserId`. A exclusão lógica deve adicionar rastreabilidade própria, preservando a autoria original e a última atualização clínica.

### 6.5. Queries e filtros

A fase prática deverá decidir entre:

- query filters globais no EF para ocultar registros com `IsDeleted = true` por padrão;
- filtros explícitos nos repositórios/use cases para manter maior controle operacional;
- abordagem híbrida, se houver necessidade de consultas administrativas controladas.

Essa decisão deve considerar testabilidade, previsibilidade em queries clínicas, risco de vazamento de registros deletados e manutenção futura.

### 6.6. Comportamento de leitura por paciente

A Fase 6.4.3 deve decidir explicitamente se o `GET` por paciente deve ignorar registros logicamente deletados. A recomendação inicial é que consultas clínicas padrão não retornem registros deletados indevidamente.

### 6.7. Visualização administrativa futura

Também deve ser decidido se perfis administrativos ou fluxos de auditoria poderão visualizar registros deletados futuramente. Caso isso seja permitido, deve ocorrer por caso de uso explícito, policy específica e sem exposição acidental no endpoint clínico padrão.

### 6.8. Auditoria futura de Soft Delete

A criação de evento `MedicalRecord.SoftDeleted` deve ser avaliada como evolução futura, desde que esteja dentro do escopo da subfase prática correspondente. A auditoria deve manter o padrão já adotado de metadata mínima sem payload clínico sensível.

### 6.9. Abordagem conservadora recomendada

A ordem recomendada é:

1. não criar endpoint `DELETE` público ainda;
2. primeiro preparar domínio, schema e migration de Soft Delete;
3. depois criar caso de uso controlado para marcar registro como deletado;
4. somente depois criar endpoint, se houver regra de negócio clara, autorização explícita e critérios de auditoria definidos.

## 7. Estratégia recomendada para retenção clínica

A implementação futura de **MR-DEBT-005 — Política de retenção não implementada** deve começar por decisão documental e governança, não por automação. Nenhuma política real de retenção é implementada nesta subfase.

### 7.1. Política antes de automação

Antes de qualquer job, scheduler, worker ou expurgo, o projeto deve documentar a política clínica esperada. A política deve responder:

- quais dados clínicos devem ser retidos;
- por quanto tempo devem ser retidos;
- se haverá arquivamento;
- se haverá expurgo físico;
- quem pode autorizar exceções;
- quais evidências devem ser mantidas.

### 7.2. Sem expurgo automático nesta fase

Não se recomenda expurgo automático de prontuário clínico nesta fase. Enquanto não houver regra regulatória/produto clara, a alternativa mais conservadora é considerar retenção indefinida inicial para `MedicalRecord`.

### 7.3. Retenção operacional versus exclusão lógica

Retenção não deve ser confundida com Soft Delete:

- Soft Delete remove o registro dos fluxos clínicos padrão sem apagá-lo fisicamente;
- retenção define por quanto tempo o dado deve existir, mesmo que esteja logicamente deletado;
- arquivamento pode mover ou segmentar dados preservados;
- expurgo físico, se algum dia existir, deve ser seguro, auditável e formalmente autorizado.

### 7.4. Possíveis mecanismos futuros

Dependendo da decisão de produto/compliance, a retenção pode exigir futuramente:

- job controlado de retenção;
- arquivamento de registros antigos;
- exportação controlada para fins legais/operacionais;
- relatório de dados elegíveis para ação;
- logs e evidências de execução.

### 7.5. Restrições da Fase 6.4

Nesta etapa de planejamento, não devem ser criados scheduler, background service, worker, integração RabbitMQ, Redis, fila, outbox ou mecanismo de expurgo. A automação deve vir apenas depois da política formal.

## 8. Estratégia recomendada para revisão de DeleteBehavior.Cascade

A resolução futura de **MR-DEBT-006 — DeleteBehavior.Cascade pendente de revisão** deve começar por mapeamento das relações clínicas e análise de impacto. Nenhuma alteração de `DeleteBehavior` é feita nesta subfase.

### 8.1. Mapeamento de relações clínicas

A revisão deve mapear relações atuais e futuras envolvendo, no mínimo:

- `MedicalRecord`;
- `Patient`;
- `Attendance`;
- `ClinicalEvolution`;
- `Prescription`;
- demais entidades clínicas que possam referenciar prontuário, paciente ou atendimento.

### 8.2. Identificação de cascades relevantes

A análise deve identificar onde há cascade configurado em código, snapshot ou migrations, e quais cascades podem provocar perda de histórico clínico. O relacionamento atual entre `MedicalRecord` e `Patient` deve ser tratado como ponto de atenção explícito.

### 8.3. Restrict/NoAction para relações críticas

A estratégia recomendada é avaliar troca para `Restrict` ou `NoAction` em relações que possam apagar histórico clínico por exclusão de entidade principal. Cascades devem ser mantidos apenas onde fizerem sentido clínico e técnico, por exemplo em dados estritamente dependentes, não históricos ou não clínicos.

### 8.4. Impacto em migrations

Qualquer alteração de cascade pode gerar migration e alteração de constraint no banco. A subfase prática deverá documentar impacto, revisar snapshot e garantir compatibilidade com dados existentes.

### 8.5. Testes de infraestrutura

A revisão de cascade deve prever testes de infraestrutura que validem o comportamento esperado de exclusão, especialmente para impedir apagamento acidental de histórico clínico em relações críticas.

## 9. Sequenciamento recomendado da Fase 6.4

O sequenciamento recomendado para a Fase 6.4 é:

- **6.4.1 — Planejamento técnico de persistência clínica segura**: documentação atual, sem implementação;
- **6.4.2 — Implementação de Soft Delete em MedicalRecord**: preparar domínio/schema/migration e fluxo mínimo controlado sem endpoint público inicial, se possível;
- **6.4.3 — Ajustes de queries/filtros e testes de Soft Delete**: garantir que consultas padrão não retornem registros deletados indevidamente e cobrir criação, consulta, atualização e Soft Delete;
- **6.4.4 — Revisão de DeleteBehavior.Cascade em entidades clínicas**: mapear e ajustar cascades críticos para reduzir risco de exclusão indireta de histórico;
- **6.4.5 — Planejamento/decisão de política de retenção clínica**: formalizar decisão de retenção, arquivamento e eventual expurgo antes de qualquer automação;
- **6.4.6 — Evidências finais, atualização do registro vivo e encerramento da Fase 6.4**: consolidar evidências, atualizar status dos débitos e registrar pendências remanescentes.

A ordem mantém Soft Delete antes da revisão de cascade porque reduz primeiro o risco de exclusão direta de `MedicalRecord` e cria base semântica para consultas e fluxos clínicos. A revisão de cascade vem antes da decisão final de retenção automatizável porque a política de retenção depende de garantias mínimas de que o banco não apagará histórico por efeito colateral. Se durante a execução for identificado que a decisão formal de retenção é pré-requisito para alterar cascades, 6.4.4 e 6.4.5 podem ser invertidas mediante justificativa técnica documentada.

## 10. Critérios futuros para resolver MR-DEBT-001

**MR-DEBT-001 — Soft Delete ausente** só poderá ser considerado resolvido quando:

- `MedicalRecord` possuir Soft Delete persistido;
- exclusão física continuar inexistente ou bloqueada nos fluxos clínicos padrão;
- houver fluxo controlado para marcar registro como deletado;
- houver usuário autenticado associado ao Soft Delete;
- consultas padrão não retornarem registros deletados indevidamente;
- testes cobrirem criação, consulta, atualização e Soft Delete;
- documentação e registro vivo forem atualizados.

## 11. Critérios futuros para resolver MR-DEBT-005

**MR-DEBT-005 — Política de retenção não implementada** só poderá ser considerado resolvido quando:

- houver decisão formal de política de retenção clínica;
- a política diferenciar retenção, arquivamento e exclusão lógica;
- a política indicar se haverá ou não expurgo físico;
- se houver expurgo, existir estratégia segura e auditável;
- documentação e registro vivo forem atualizados;
- não houver job, scheduler ou automação prematura sem regra clara.

## 12. Critérios futuros para resolver MR-DEBT-006

**MR-DEBT-006 — DeleteBehavior.Cascade pendente de revisão** só poderá ser considerado resolvido quando:

- os `DeleteBehavior.Cascade` relevantes forem mapeados;
- relações clínicas críticas forem revisadas;
- cascades perigosos forem alterados para `Restrict` ou `NoAction` quando necessário;
- migrations forem criadas se houver alteração de schema/constraint;
- testes de infraestrutura validarem comportamento de exclusão;
- documentação e registro vivo forem atualizados.

## 13. Impactos técnicos previstos

As próximas subfases da Fase 6.4 podem impactar os seguintes pontos do projeto:

- domínio `MedicalRecord`, caso sejam adicionados campos e comportamento de Soft Delete;
- use cases de criação, consulta, atualização e eventual Soft Delete;
- repository de `MedicalRecord`, especialmente queries por paciente e filtros de registros deletados;
- queries por paciente, que deverão definir comportamento padrão para registros logicamente deletados;
- configuração EF, incluindo propriedades novas, filtros e/ou comportamento relacional;
- migrations e snapshot, caso schema, constraints ou cascade sejam alterados;
- testes de Application para validar regras de Soft Delete e visibilidade;
- testes de Infrastructure para validar persistência, filtros e comportamento de exclusão relacional;
- `MedicalRecordsController`, somente se endpoint futuro de Soft Delete for criado com regra de negócio clara;
- documentação e registro vivo de débitos técnicos.

## 14. Fora do escopo da Fase 6.4.1

A Fase 6.4.1 não implementa:

- Soft Delete;
- retenção clínica;
- alteração de cascade;
- migration;
- endpoint `DELETE`;
- background job;
- scheduler;
- AuditLog adicional;
- frontend;
- infraestrutura operacional;
- alteração em Docker, Redis, RabbitMQ ou Kubernetes;
- alteração em domínio, DbContext, configurações EF, controller, use cases, repository ou testes.

## 15. Riscos e impactos de governança

- A ausência de Soft Delete continua bloqueando produção real porque o projeto ainda não possui proteção persistida contra exclusão física futura de prontuário.
- A ausência de política de retenção continua bloqueando produção real porque não há decisão formal para preservar, arquivar ou expurgar dados clínicos.
- A ausência de revisão de cascade continua bloqueando produção real porque relações clínicas podem permitir apagamento indireto de histórico.
- A criação prematura de endpoint `DELETE`, job de expurgo ou scheduler sem política formal aumentaria o risco em vez de reduzi-lo.
- A alteração de cascade sem testes de infraestrutura pode gerar falsa sensação de segurança ou quebrar constraints existentes.
- A adoção de query filter global sem estratégia administrativa pode dificultar auditoria, suporte e investigação futura.

## 16. Recomendação para a próxima fase prática

A próxima fase prática recomendada é a **Fase 6.4.2 — Implementação de Soft Delete em MedicalRecord**.

A Fase 6.4.2 deve iniciar de forma conservadora, preparando domínio, schema e fluxo controlado de exclusão lógica, sem criar endpoint `DELETE` público automaticamente. A criação de endpoint deve ser tratada apenas depois de haver regra de negócio clara, autorização explícita, comportamento de consulta definido e decisão sobre auditoria de `MedicalRecord.SoftDeleted`.

## 17. Critérios de aceite desta subfase

A Fase 6.4.1 é considerada concluída quando:

- este documento existir em `docs/clinical-core/PHASE_06_04_01_MEDICAL_RECORD_SAFE_PERSISTENCE_PLANNING.md`;
- **MR-DEBT-001**, **MR-DEBT-005** e **MR-DEBT-006** estiverem explicitamente referenciados;
- o estado atual pós-Fase 6.3 estiver documentado;
- os riscos atuais estiverem explicados;
- Soft Delete, retenção e cascade estiverem diferenciados;
- a estratégia futura de Soft Delete estiver proposta;
- a estratégia futura de retenção clínica estiver proposta;
- a estratégia futura de revisão de `DeleteBehavior.Cascade` estiver proposta;
- o sequenciamento da Fase 6.4 estiver definido;
- critérios futuros de resolução dos três débitos estiverem definidos;
- nenhum código tiver sido alterado;
- nenhuma migration tiver sido criada;
- o escopo permanecer documental;
- `git diff --check` passar.
