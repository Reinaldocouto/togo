# TOGO — Fase 6.4.5: Decisão de política inicial de retenção clínica de MedicalRecord

## 1. Contexto da Fase 6.4

A Fase 6.4 trata os débitos P1 remanescentes relacionados à persistência clínica segura da vertical `MedicalRecord`:

- **MR-DEBT-001 — Soft Delete ausente**;
- **MR-DEBT-005 — Política de retenção não implementada**;
- **MR-DEBT-006 — DeleteBehavior.Cascade pendente de revisão**.

As fases anteriores da trilha já planejaram e implementaram partes da persistência clínica segura:

- a Fase 6.4.1 planejou a estratégia geral de persistência, Soft Delete, retenção e revisão de cascades;
- a Fase 6.4.2 implementou a base de Soft Delete em `MedicalRecord`;
- a Fase 6.4.3 consolidou filtros explícitos para ignorar registros logicamente deletados nos fluxos clínicos padrão;
- a Fase 6.4.4 revisou relações clínicas críticas e alterou cascades perigosos para `Restrict`.

Esta Fase 6.4.5 trata diretamente o débito **MR-DEBT-005 — Política de retenção não implementada**.

## 2. Objetivo

Formalizar a política inicial de retenção clínica de `MedicalRecord`, definindo preservação, arquivamento e eventual expurgo em nível de governança técnica, sem implementar automação prematura.

Esta fase é exclusivamente documental e de decisão técnica. Ela não cria código, migrations, banco, jobs, endpoints, telas, workers ou qualquer mecanismo automatizado de retenção, arquivamento ou expurgo.

## 3. Fontes consideradas

A decisão desta fase considera as evidências e decisões registradas em:

- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`;
- `docs/clinical-core/PHASE_06_04_01_MEDICAL_RECORD_SAFE_PERSISTENCE_PLANNING.md`;
- `docs/clinical-core/PHASE_06_04_02_MEDICAL_RECORD_SOFT_DELETE_IMPLEMENTATION.md`;
- `docs/clinical-core/PHASE_06_04_03_MEDICAL_RECORD_SOFT_DELETE_QUERY_FILTERS.md`;
- `docs/clinical-core/PHASE_06_04_04_MEDICAL_RECORD_CASCADE_DELETE_REVIEW.md`;
- `docs/clinical-core/PHASE_06_03_06_MEDICAL_RECORD_AUTHORSHIP_AUDIT_CLOSURE.md`;
- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`;
- `backend/src/Togo.Domain/Entities/ClinicalAuditLog.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalAuditLogConfiguration.cs`.

## 4. Estado atual da persistência clínica

No início desta fase, `MedicalRecord` já possui controles mínimos relevantes para persistência clínica segura:

- autorização granular mínima nos fluxos clínicos já hardenizados;
- autoria clínica de criação e atualização;
- `ClinicalAuditLog` mínimo para rastreabilidade clínica de criação e atualização;
- Soft Delete base, com `IsDeleted`, `DeletedAt` e `DeletedByUserId`;
- filtros padrão explícitos para ignorar registros logicamente deletados nos fluxos clínicos comuns;
- revisão de cascades críticos, com relações clínicas perigosas alteradas para `DeleteBehavior.Restrict`;
- preservação física do prontuário quando ocorre Soft Delete.

Apesar desses avanços, antes desta fase ainda não existia política formal de retenção clínica para `MedicalRecord`. Portanto, **MR-DEBT-005** permanecia aberto como débito de governança/compliance: havia mecanismos de preservação e mitigação operacional, mas não uma decisão técnica explícita sobre por quanto tempo preservar, se arquivar e sob quais condições admitir eventual expurgo físico.

## 5. Diferença entre Soft Delete, retenção, arquivamento e expurgo

Para evitar ambiguidade futura, esta fase diferencia quatro conceitos:

### 5.1. Soft Delete

Soft Delete é exclusão lógica. O registro deixa de aparecer nos fluxos clínicos padrão, mas continua fisicamente preservado no banco.

No contexto atual de `MedicalRecord`, Soft Delete significa marcar o registro com `IsDeleted = true` e preencher autoria/tempo da exclusão lógica, sem remover a linha física e sem apagar `GeneralNotes`, `FlagsJson`, autoria de criação, autoria de atualização ou vínculo com `Patient`.

### 5.2. Retenção

Retenção é a decisão de governança sobre preservação do dado ao longo do tempo. Ela responde se o dado deve continuar existindo, por quanto tempo e com quais restrições operacionais.

Retenção não é sinônimo de Soft Delete. Um registro logicamente deletado continua sujeito à política de retenção e, nesta fase, continua preservado fisicamente.

### 5.3. Arquivamento

Arquivamento é uma possível movimentação, segmentação, particionamento ou separação futura de dados preservados. Arquivar não significa apagar. Um registro arquivado continuaria existindo, mas poderia ficar em outro segmento lógico, físico ou operacional, dependendo de decisão futura.

Esta fase não implementa arquivamento e não define mecanismo técnico de arquivamento. Apenas registra que arquivamento pode ser avaliado no futuro se volume, desempenho, operação ou regra regulatória exigirem.

### 5.4. Expurgo físico

Expurgo físico é remoção definitiva da linha/dado persistido. Deve ser tratado como operação excepcional, irreversível ou de reversão altamente limitada, com alto risco clínico, operacional, legal e de auditoria.

Expurgo físico não deve ser inferido a partir de Soft Delete. Soft Delete remove o registro dos fluxos padrão; expurgo remove o dado fisicamente. Essas operações têm riscos e autorizações diferentes.

## 6. Decisão inicial recomendada

A política inicial adotada é conservadora:

- `MedicalRecord` deve ter **retenção indefinida inicial** enquanto o produto não possuir regra regulatória, legal ou de negócio mais específica;
- registros com Soft Delete devem continuar fisicamente preservados;
- nenhum expurgo físico automático deve existir nesta fase;
- nenhum job, scheduler, worker ou mecanismo equivalente deve ser criado sem política regulatória/produto mais detalhada;
- qualquer expurgo futuro deve exigir decisão formal, autorização forte, `AuditLog` próprio e critérios verificáveis;
- `ClinicalAuditLog` deve ser preservado junto à rastreabilidade clínica e não deve ser expurgado automaticamente nesta fase.

Não foi adotada decisão alternativa porque, na ausência de regra regulatória/produto específica para retenção temporal ou expurgo, a alternativa tecnicamente mais segura para prontuário clínico é preservar os registros e impedir automação prematura de remoção definitiva.

## 7. Política inicial para MedicalRecord ativo

Para `MedicalRecord` ativo, isto é, registros sem Soft Delete:

- registros ativos devem ser preservados;
- não há expurgo físico de `MedicalRecord` ativo;
- atualização de notas ou flags não deve apagar histórico de autoria mínima;
- Soft Delete não deve apagar histórico, payload clínico persistido, autoria de criação ou autoria de atualização;
- a retenção inicial é indefinida enquanto o produto não tiver regra regulatória, legal ou de negócio específica;
- qualquer mudança futura nessa decisão deve ocorrer em fase própria, com documentação, critérios objetivos e validação de impacto.

## 8. Política inicial para MedicalRecord com Soft Delete

Para `MedicalRecord` marcado com Soft Delete:

- Soft Delete remove o registro dos fluxos clínicos padrão;
- Soft Delete não autoriza expurgo físico;
- registros deletados logicamente continuam preservados no banco;
- registros deletados logicamente podem ser usados para auditoria, investigação, rastreabilidade clínica, análise de incidente e reconstrução operacional;
- eventual consulta administrativa futura de registros deletados deve exigir autorização forte, policy própria, caso de uso explícito e trilha de auditoria adequada;
- esta fase não cria consulta administrativa, endpoint, tela, restore, reprocessamento ou expurgo de registros deletados.

## 9. Política inicial para ClinicalAuditLog

Para `ClinicalAuditLog`, a decisão inicial é:

- `ClinicalAuditLog` deve ser preservado como evidência mínima de rastreabilidade clínica;
- não deve haver expurgo automático de audit logs nesta fase;
- logs não possuem payload clínico sensível completo, mas ainda são sensíveis operacionalmente porque carregam entidade, identificador, ação, usuário, perfil, instante e metadata mínima;
- `ClinicalAuditLog` deve acompanhar a preservação da rastreabilidade de `MedicalRecord`;
- eventual retenção, arquivamento ou expurgo de `ClinicalAuditLog` deve ser decidida em fase futura própria, com critérios específicos para auditoria, segurança, investigação e operação.

## 10. Expurgo físico

Expurgo físico **não será implementado nesta fase**.

Também ficam registradas as seguintes decisões:

- expurgo físico não será assumido como comportamento padrão de `MedicalRecord`;
- expurgo físico não será inferido a partir de Soft Delete;
- expurgo físico não será implementado por migration, script, endpoint, job, worker, scheduler ou rotina manual documentada nesta fase;
- expurgo físico futuro só poderá existir com:
  - regra formal;
  - autorização forte;
  - trilha de auditoria própria;
  - critérios objetivos;
  - testes;
  - documentação;
  - decisão explícita sobre impacto legal, clínico, operacional e de suporte;
  - definição sobre preservação ou não de evidências mínimas de auditoria;
  - estratégia de falha, revisão e reconciliação.

Até que esses requisitos existam, o comportamento seguro é preservar fisicamente os dados clínicos.

## 11. Automação futura

Esta fase proíbe explicitamente a criação de automações de retenção, arquivamento ou expurgo.

Não serão criados agora:

- scheduler;
- background service;
- worker;
- fila;
- RabbitMQ;
- Redis;
- outbox;
- job de expurgo;
- job de arquivamento;
- job de retenção;
- endpoint administrativo de retenção;
- tela frontend;
- script operacional de remoção física.

Qualquer automação futura precisa de fase própria, com escopo explícito, política aprovada, critérios de elegibilidade, autorização, observabilidade, testes, documentação e decisão sobre risco regulatório/produto.

## 12. Critérios futuros para automação de retenção

Se algum dia a retenção avançada exigir automação, a fase futura deverá responder, no mínimo:

- qual regra formal torna um registro elegível para arquivamento ou expurgo;
- se a regra vem de produto, compliance, operação, contrato, legislação ou combinação desses fatores;
- quais entidades entram no escopo além de `MedicalRecord` e `ClinicalAuditLog`;
- como registros com Soft Delete são diferenciados de registros ativos;
- quem pode autorizar execução e exceções;
- qual policy/role/profile é exigido;
- qual trilha de auditoria será criada para simulação, execução, falha e revisão;
- se haverá modo dry-run ou relatório de elegibilidade antes de qualquer ação destrutiva;
- quais testes de domínio, aplicação, infraestrutura e integração serão obrigatórios;
- como garantir que uma automação não apague dados clínicos por erro de filtro;
- como lidar com restauração, contestação, investigação ou hold legal/operacional;
- como monitorar volume, desempenho, particionamento e impacto em backup/restore.

Sem essas respostas, automação de retenção, arquivamento ou expurgo não deve ser implementada.

## 13. Critérios futuros para considerar MR-DEBT-005 resolvido

**MR-DEBT-005 — Política de retenção não implementada** poderá ser considerado resolvido tecnicamente se:

- existir decisão formal de retenção clínica;
- a decisão diferenciar retenção, Soft Delete, arquivamento e expurgo;
- houver decisão explícita de não expurgar `MedicalRecord` automaticamente;
- registros com Soft Delete continuarem preservados;
- `ClinicalAuditLog` continuar preservado;
- os limites de automação futura estiverem documentados;
- o registro vivo de débitos técnicos for atualizado na Fase 6.4.6 para refletir esta decisão.

Esta Fase 6.4.5 entrega a decisão formal e a documentação técnica inicial. A baixa formal do débito no registro vivo deve ocorrer na Fase 6.4.6, junto com as evidências finais da Fase 6.4.

## 14. Riscos remanescentes

Mesmo com a política inicial formalizada, permanecem os seguintes riscos:

- a política ainda é inicial e conservadora;
- não há automação de retenção;
- não há arquivamento;
- não há consulta administrativa de registros deletados;
- não há expurgo físico;
- requisitos regulatórios futuros podem exigir revisão;
- volume de dados futuro pode exigir estratégia de arquivamento, particionamento, storage tiering, índice dedicado ou separação operacional;
- retenção indefinida pode aumentar custos de armazenamento, backup, restore e observabilidade;
- consulta administrativa e retenção avançada devem virar fases futuras, se necessário;
- eventual expurgo futuro pode exigir regras específicas para manter ou anonimizar evidências mínimas de auditoria;
- a decisão atual não substitui análise jurídica, regulatória ou de compliance específica para produção real.

## 15. Fora do escopo

Esta fase não implementa:

- código;
- testes;
- migrations;
- banco;
- jobs;
- schedulers;
- workers;
- endpoints;
- frontend;
- expurgo;
- arquivamento;
- alteração de Soft Delete;
- alteração de cascade;
- alteração de `AuditLog`;
- alteração de `MedicalRecord`;
- alteração de `ClinicalAuditLog`;
- alteração de `DbContext`;
- alteração de queries/filtros;
- Docker, Redis, RabbitMQ ou Kubernetes.

## 16. Critérios de aceite da Fase 6.4.5

A fase é considerada concluída quando:

- o documento for criado em `docs/clinical-core/PHASE_06_04_05_MEDICAL_RECORD_RETENTION_POLICY_DECISION.md`;
- **MR-DEBT-005** for explicitamente referenciado;
- a política inicial de retenção for formalizada;
- Soft Delete, retenção, arquivamento e expurgo forem diferenciados;
- a decisão sobre retenção indefinida inicial for registrada;
- a decisão sobre não expurgo automático for registrada;
- a decisão sobre preservação de `MedicalRecord` com Soft Delete for registrada;
- a decisão sobre preservação de `ClinicalAuditLog` for registrada;
- automações futuras forem explicitamente proibidas nesta fase;
- riscos remanescentes forem documentados;
- nenhuma implementação for feita;
- nenhuma migration for criada;
- o escopo permanecer exclusivamente documental;
- `git diff --check` passar.

## 17. Próxima fase recomendada

**Fase 6.4.6 — Evidências finais, atualização do registro vivo e encerramento da Fase 6.4.**

Objetivo recomendado da Fase 6.4.6:

- consolidar evidências das fases 6.4.1 a 6.4.5;
- atualizar o registro vivo de débitos técnicos;
- registrar o status de **MR-DEBT-001**, **MR-DEBT-005** e **MR-DEBT-006** após as entregas da Fase 6.4;
- encerrar formalmente a Fase 6.4 como trilha de persistência clínica segura de `MedicalRecord`.
