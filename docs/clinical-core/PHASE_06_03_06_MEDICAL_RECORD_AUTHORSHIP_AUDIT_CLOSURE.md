# Fase 6.3.6 — Encerramento formal de autoria e AuditLog clínico em MedicalRecord

## 1. Objetivo

Encerrar formalmente a **Fase 6.3 — Auditoria e autoria clínica** da vertical `MedicalRecord`, revisar a consistência do registro vivo de débitos técnicos após as implementações de autoria e AuditLog, corrigir inconsistências documentais remanescentes e preparar a abertura da Fase 6.4.

Esta fase é exclusivamente documental e de governança. Não altera domínio, use cases, controllers, testes, migrations, banco, JWT, autorização, endpoints, frontend ou infraestrutura.

## 2. Contexto da Fase 6.3

A Fase 6.3 sucedeu a Fase 6.2, que resolveu tecnicamente o débito de autorização granular mínima de `MedicalRecord` (**MR-DEBT-003**). O foco da Fase 6.3 foi tratar débitos P1 de rastreabilidade clínica que impediam a evolução segura para uso real com dados sensíveis:

- **MR-DEBT-004 — Controle de autoria ausente**;
- **MR-DEBT-002 — AuditLog ausente**.

A abordagem foi incremental e deliberadamente limitada: primeiro garantir autoria clínica mínima com usuário autenticado e timestamps persistidos; depois registrar AuditLog clínico mínimo para criação e atualização. Escopos avançados, como auditoria de leitura, auditoria de acesso negado, endpoint público de auditoria, frontend de auditoria e transação única entre operação principal e AuditLog, permaneceram fora da Fase 6.3.

## 3. Resumo das entregas 6.3.1 a 6.3.5

### 3.1 Fase 6.3.1 — Planejamento técnico

A Fase 6.3.1 definiu o plano técnico de autoria e auditoria, estabelecendo a ordem segura para tratar identidade/autoria antes de persistir eventos de auditoria.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_01_MEDICAL_RECORD_AUTHORSHIP_AUDIT_PLANNING.md`.

### 3.2 Fase 6.3.2 — Contratos de usuário atual e auditoria

A Fase 6.3.2 criou os contratos mínimos necessários para desacoplar a Application de HTTP/JWT e preparar a escrita posterior de AuditLog:

- `CurrentUserInfo`;
- `ICurrentUserService`;
- `ClinicalAuditEvent`;
- `IClinicalAuditLogWriter`;
- `MedicalRecordAuditActions`.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_02_CURRENT_USER_AND_AUDIT_CONTRACTS.md`.

### 3.3 Fase 6.3.3 — Autoria clínica mínima

A Fase 6.3.3 implementou autoria persistida em `MedicalRecord`, incluindo:

- `CreatedByUserId`;
- `CreatedAt`;
- `UpdatedByUserId`;
- `UpdatedAt`.

A criação passou a registrar autoria de criação e de última atualização com o usuário autenticado atual. A atualização passou a preservar autoria original de criação e atualizar apenas autoria de última modificação.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_03_MEDICAL_RECORD_AUTHORSHIP_IMPLEMENTATION.md`.

### 3.4 Fase 6.3.3.1 — Hotfix de defaults persistentes

A Fase 6.3.3.1 removeu defaults persistentes de banco que haviam sido introduzidos apenas para compatibilizar a migration inicial de autoria. A decisão final evitou autoria artificial por default de banco em novos registros.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_03_01_MEDICAL_RECORD_AUTHORSHIP_DEFAULTS_HOTFIX.md`.

### 3.5 Fase 6.3.4 — AuditLog clínico mínimo

A Fase 6.3.4 criou a persistência interna de `ClinicalAuditLogs` e registrou eventos mínimos para:

- `MedicalRecord.Created`;
- `MedicalRecord.Updated`.

Os eventos registram usuário, perfil quando disponível, entidade, identificador da entidade, ação, instante UTC e metadata mínima sem payload clínico sensível.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_04_MEDICAL_RECORD_CLINICAL_AUDIT_LOG_IMPLEMENTATION.md`.

### 3.6 Fase 6.3.5 — Evidências finais

A Fase 6.3.5 consolidou evidências técnicas, documentais e de testes de autoria e AuditLog, além de atualizar o registro vivo para marcar MR-DEBT-002 e MR-DEBT-004 como resolvidos tecnicamente no escopo mínimo planejado.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_05_MEDICAL_RECORD_AUTHORSHIP_AUDIT_EVIDENCE.md`.

## 4. Débitos tratados

A Fase 6.3 tratou tecnicamente os seguintes P1 de rastreabilidade clínica:

| ID | Débito | Resultado da Fase 6.3 |
|---|---|---|
| MR-DEBT-004 | Controle de autoria ausente | Resolvido tecnicamente para autoria clínica mínima. |
| MR-DEBT-002 | AuditLog ausente | Resolvido tecnicamente para AuditLog clínico mínimo. |

Além disso, a implementação de autoria clínica mínima também resolveu a lacuna documental/técnica associada ao **MR-DEBT-008 — CreatedAt ausente**, porque `CreatedAt` passou a existir como campo persistido, preenchido na criação e preservado na atualização.

## 5. Débitos atualizados no registro vivo

O registro vivo de débitos técnicos foi revisado para refletir o estado atual após a Fase 6.3:

- **MR-DEBT-002** permanece marcado como resolvido tecnicamente para AuditLog clínico mínimo;
- **MR-DEBT-004** permanece marcado como resolvido tecnicamente para autoria clínica mínima;
- **MR-DEBT-008** foi corrigido para deixar de constar como P2 aberto, pois `CreatedAt` foi implementado na Fase 6.3.3;
- a seção de P2 foi ajustada para listar apenas os P2 ainda abertos;
- o roadmap da Fase 6.5 foi ajustado para remover MR-DEBT-008 da fila aberta de evolução de schema.

Arquivo atualizado:

- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`.

## 6. Correção documental do MR-DEBT-008

Antes desta fase, **MR-DEBT-008 — CreatedAt ausente** permanecia como aberto no registro vivo, com evidência de que a entidade possuía `UpdatedAt`, mas não `CreatedAt`.

Essa descrição deixou de ser verdadeira após a Fase 6.3.3. A correção documental registra agora que:

- `MedicalRecord` possui `CreatedAt` persistido;
- `CreatedAt` é preenchido na criação;
- `CreatedAt` é preservado na atualização;
- a rastreabilidade temporal de criação existe junto da autoria clínica mínima;
- MR-DEBT-008 não é mais bloqueio ativo isolado nem P2 aberto.

Decisão documental: **MR-DEBT-008 está resolvido tecnicamente pela Fase 6.3.3**, com evidências finais consolidadas na Fase 6.3.5 e fechamento formal nesta Fase 6.3.6.

## 7. Estado final da rastreabilidade MedicalRecord

Ao final da Fase 6.3, a rastreabilidade mínima de `MedicalRecord` cobre:

- autoria de criação por usuário autenticado;
- instante UTC de criação;
- autoria de última atualização por usuário autenticado;
- instante UTC de última atualização;
- preservação da autoria original durante atualização;
- eventos internos de AuditLog para criação e atualização;
- minimização de metadata de auditoria, sem gravar payload clínico sensível completo;
- ausência de endpoint público e frontend para AuditLog.

Esse estado é suficiente para encerrar os débitos MR-DEBT-002 e MR-DEBT-004 no escopo mínimo planejado, mas não equivale a liberação total para produção real com dados clínicos sensíveis.

## 8. Confirmação de resolução técnica do MR-DEBT-002

**MR-DEBT-002 — AuditLog ausente** está resolvido tecnicamente no escopo de AuditLog clínico mínimo porque a aplicação passou a registrar internamente:

- `MedicalRecord.Created` no fluxo de criação;
- `MedicalRecord.Updated` no fluxo de atualização;
- `UserId` do usuário autenticado;
- `UserProfile` quando disponível;
- entidade e identificador da entidade;
- ação executada;
- instante UTC de ocorrência;
- `MetadataJson` mínimo, atualmente restrito a `PatientId`.

Limites aceitos da resolução:

- não há auditoria de leitura;
- não há auditoria de acesso negado;
- não há endpoint público de AuditLog;
- não há frontend de AuditLog;
- não há garantia de transação única entre operação principal e AuditLog;
- não há política de retenção/expurgo implementada nesta fase.

## 9. Confirmação de resolução técnica do MR-DEBT-004

**MR-DEBT-004 — Controle de autoria ausente** está resolvido tecnicamente no escopo de autoria clínica mínima porque `MedicalRecord` passou a persistir:

- `CreatedByUserId`;
- `CreatedAt`;
- `UpdatedByUserId`;
- `UpdatedAt`.

A criação preenche os campos com o usuário autenticado atual e o instante UTC. A atualização preserva os dados de criação e altera somente os dados de última modificação. A implementação final não depende de defaults persistentes de banco para criar autoria artificial em produção.

## 10. Confirmação de resolução do MR-DEBT-008

**MR-DEBT-008 — CreatedAt ausente** foi resolvido pela implementação de `CreatedAt` na Fase 6.3.3, como parte do pacote de autoria clínica mínima associado ao MR-DEBT-004.

Decisão final para o registro vivo:

- Status: resolvido tecnicamente pela Fase 6.3.3;
- Bloqueia produção real?: não, como bloqueio ativo isolado;
- Evidência: `MedicalRecord` possui `CreatedAt` persistido, preenchido na criação e preservado na atualização;
- Risco: risco original mitigado;
- Mitigação atual: `CreatedAt` persistido junto da autoria clínica mínima;
- Fase futura recomendada: tratado na Fase 6.3.3, com evidências finais na Fase 6.3.5.

## 11. P1 ainda abertos

Após o encerramento formal da Fase 6.3, permanecem abertos os seguintes P1 da vertical `MedicalRecord`:

| ID | Débito | Motivo de permanência |
|---|---|---|
| MR-DEBT-001 | Soft Delete ausente | Ainda não há exclusão lógica para proteção de histórico clínico. |
| MR-DEBT-005 | Política de retenção não implementada | Ainda não há mecanismo técnico de retenção/expurgo governado. |
| MR-DEBT-006 | DeleteBehavior.Cascade pendente de revisão | Ainda há risco arquitetural de cascades incompatíveis com persistência clínica segura. |

Esses itens devem orientar a Fase 6.4.

## 12. Motivo pelo qual produção real ainda não está liberada

A produção real com dados clínicos sensíveis continua bloqueada porque a vertical `MedicalRecord` ainda possui P1 abertos relacionados à persistência clínica segura, retenção e integridade referencial.

Embora autoria, autorização granular mínima e AuditLog mínimo já estejam tratados tecnicamente, ainda faltam decisões e mecanismos para impedir perda indevida de histórico clínico, governar retenção/expurgo e revisar comportamentos de exclusão em cascata.

Portanto, a Fase 6.3 encerra a trilha de rastreabilidade clínica mínima, mas **não libera produção real**.

## 13. Riscos remanescentes

Riscos remanescentes após a Fase 6.3:

- ausência de Soft Delete para `MedicalRecord`;
- ausência de política técnica de retenção/expurgo;
- necessidade de revisar `DeleteBehavior.Cascade` em relacionamentos clínicos;
- AuditLog ainda limitado a criação e atualização;
- ausência de auditoria de leitura;
- ausência de auditoria de acesso negado;
- ausência de endpoint público e frontend de consulta de auditoria;
- ausência de transação única entre `MedicalRecord` e `ClinicalAuditLogs`;
- necessidade futura de hardening de schema para unicidade física de `PatientId`;
- necessidade futura de validação estrutural ou normalização de `FlagsJson`.

Os riscos de auditoria avançada podem ser transformados em novos débitos futuros se forem exigidos por requisitos regulatórios, operacionais ou de produto. Os riscos P1 imediatos a atacar permanecem MR-DEBT-001, MR-DEBT-005 e MR-DEBT-006.

## 14. Próxima fase recomendada

Próxima fase recomendada:

**Fase 6.4.1 — Planejamento técnico de persistência clínica segura, Soft Delete, retenção e revisão de cascades.**

Objetivo sugerido para a Fase 6.4.1:

- planejar solução de Soft Delete para `MedicalRecord`;
- definir estratégia de retenção clínica;
- revisar comportamentos de cascade em entidades clínicas;
- definir ordem segura de implementação dos P1 remanescentes;
- manter produção real bloqueada até que os P1 remanescentes estejam tecnicamente tratados.

## 15. Critérios de aceite

A Fase 6.3.6 é considerada aceita se:

- este documento formal de encerramento existir;
- o registro vivo refletir MR-DEBT-002 como resolvido tecnicamente no escopo mínimo de AuditLog;
- o registro vivo refletir MR-DEBT-004 como resolvido tecnicamente no escopo mínimo de autoria clínica;
- o registro vivo corrigir MR-DEBT-008 para deixar de constar como P2 aberto;
- a seção de P2 não listar `CreatedAt` como débito aberto;
- a produção real permanecer explicitamente bloqueada pelos P1 restantes;
- a próxima fase recomendada apontar para planejamento de Soft Delete, retenção e revisão de cascades;
- nenhuma alteração de código, teste, migration, banco, JWT, autorização, endpoint, frontend ou infraestrutura for realizada.

## 16. Decisão final da Fase 6.3

**Decisão:** Fase 6.3 encerrada formalmente.

**Resultado:** MR-DEBT-002 e MR-DEBT-004 estão resolvidos tecnicamente no escopo incremental planejado. MR-DEBT-008 foi corrigido no registro vivo e considerado resolvido pela implementação de `CreatedAt` na Fase 6.3.3.

**Restrição mantida:** a vertical `MedicalRecord` ainda não está liberada para produção real com dados clínicos sensíveis.

**Motivo da restrição:** permanecem abertos MR-DEBT-001, MR-DEBT-005 e MR-DEBT-006.

**Próximo passo aprovado:** abrir a Fase 6.4.1 para planejamento técnico de persistência clínica segura, Soft Delete, retenção e revisão de cascades.
