# TOGO — Fase 6.5.4: Evidências finais e encerramento da integridade estrutural de MedicalRecord

## 1. Objetivo

Consolidar e encerrar a Fase 6.5 da vertical `MedicalRecord`, reunindo as evidências documentais e técnicas das subfases 6.5.1, 6.5.2, 6.5.2.1 e 6.5.3.

Esta fase é exclusivamente documental e de governança. Não implementa código novo, não altera testes, não cria migration e não modifica schema.

## 2. Contexto da Fase 6.5

A Fase 6.5 tratou integridade física e validação estrutural inicial de `MedicalRecord`, após o encerramento da trilha de persistência clínica segura da Fase 6.4.

Os focos principais foram:

- resolver a ausência de unicidade física em `MedicalRecords.PatientId`;
- alinhar a regra de negócio para `MedicalRecord` como relação 1:1 total por `Patient`;
- tratar a janela concorrente remanescente da constraint física;
- deixar `FlagsJson` opcional, mas não mais livre de validação estrutural.

## 3. Subfases consolidadas

Subfases consolidadas nesta fase de encerramento:

- 6.5.1 — Planejamento técnico de integridade física e validação estrutural;
- 6.5.2 — Decisão e implementação de unicidade física de `PatientId`;
- 6.5.2.1 — Tratamento de conflito concorrente da constraint de unicidade;
- 6.5.3 — Validação estrutural inicial de `FlagsJson`.

## 4. PRs consolidadas

PRs correspondentes consolidadas:

- PR 155 — Fase 6.5.1;
- PR 156 — Fase 6.5.2;
- PR 157 — Fase 6.5.2.1;
- PR 158 — Fase 6.5.3.

## 5. Débitos tratados

Débitos consolidados pela Fase 6.5:

```text
MR-DEBT-007 — Índice único em MedicalRecords.PatientId ausente
MR-DEBT-009 — FlagsJson flexível
```

## 6. Evidências de resolução de MR-DEBT-007

`MR-DEBT-007` está consolidado como resolvido tecnicamente pelas seguintes evidências:

- a decisão arquitetural adotada foi unicidade total `Patient -> MedicalRecord`;
- `MedicalRecord` passou a ser tratado como relação 1:1 total por `Patient`;
- Soft Delete não libera novo prontuário para o mesmo `PatientId`;
- `MedicalRecords.PatientId` possui índice único físico;
- o validator de criação foi alinhado à unicidade total, incluindo registros logicamente deletados;
- a configuração EF e a migration da Fase 6.5.2 materializam a constraint única;
- testes de infraestrutura cobrem bloqueio de duplicidade ativa;
- testes de infraestrutura cobrem bloqueio de novo prontuário quando o anterior está logicamente deletado;
- testes preservam permissão para prontuários de pacientes diferentes;
- a janela concorrente da constraint foi tratada na Fase 6.5.2.1;
- a violação física específica de `IX_MedicalRecords_PatientId` é traduzida para conflito de negócio;
- o fluxo de API resulta em HTTP 409 para duplicidade concorrente esperada;
- outros erros de banco, como FK inválida, timeout, conexão, erro de coluna ou outros índices únicos, não são mascarados como conflito de `MedicalRecord`.

## 7. Evidências de resolução de MR-DEBT-009

`MR-DEBT-009` está consolidado como resolvido tecnicamente no escopo inicial de validação estrutural pelas seguintes evidências:

- `FlagsJson` continua opcional;
- `null`, string vazia e whitespace são normalizados para `null`;
- quando informado, o valor deve ser JSON válido;
- a raiz do JSON deve ser objeto;
- arrays na raiz são rejeitados;
- escalares na raiz são rejeitados, incluindo string, número, booleano e literal JSON `null`;
- create e update usam a mesma regra estrutural;
- a validação fica no domínio, em `MedicalRecord`, porque a regra compõe o estado válido da entidade;
- update inválido preserva o estado anterior da entidade;
- falha de validação é convertida para `ValidationError` na Application;
- o controller mantém o mapeamento de `ValidationError` para HTTP 400;
- falha de validação não chama persistência;
- falha de validação não escreve `ClinicalAuditLog`;
- o payload de `FlagsJson` não é copiado para `AuditLog`, que permanece com metadata mínima.

## 8. Estado final de integridade física

Ao final da Fase 6.5, `MedicalRecord` possui:

- unicidade física por `PatientId`;
- proteção contra concorrência por constraint física;
- tratamento de conflito concorrente da constraint como conflito de negócio;
- compatibilidade com Soft Delete conservador, no qual registro logicamente deletado continua contando para unicidade;
- validação estrutural inicial de `FlagsJson`.

## 9. Estado final de validação estrutural

`FlagsJson` ainda é uma `string?` persistida como texto, mas não é mais string livre nos novos fluxos de criação e atualização.

A Fase 6.5.3 implementou apenas validação estrutural mínima: ausência continua permitida; valor informado precisa ser JSON válido com objeto na raiz. A fase não implementou schema semântico, contrato clínico estável, normalização relacional ou validação por chaves conhecidas.

## 10. Limites explícitos

Ainda não existe:

- schema semântico de flags;
- allowlist de chaves;
- JSON Schema;
- normalização relacional;
- entidade própria de flags;
- tabela própria de flags;
- indexação interna do JSON;
- query por flags;
- saneamento automático de dados legados;
- tratamento específico de propriedades duplicadas;
- contrato clínico estável de interoperabilidade.

## 11. Ausência de alterações indevidas

A Fase 6.5.4 confirma, com base nas subfases consolidadas, a ausência das seguintes alterações indevidas no fechamento desta trilha:

- nenhuma migration na 6.5.3;
- nenhuma alteração de schema na 6.5.3;
- nenhum pacote JSON externo;
- nenhuma alteração de frontend;
- nenhuma alteração de JWT/autorização;
- nenhuma alteração de Docker, Redis, RabbitMQ ou Kubernetes.

Nesta Fase 6.5.4, a alteração é apenas documental em `docs/clinical-core`.

## 12. Riscos remanescentes

Riscos remanescentes registrados para fases futuras:

- dados legados duplicados podem exigir saneamento antes de aplicar a migration de unicidade em ambiente real;
- dados legados com `FlagsJson` inválido não são saneados automaticamente;
- `FlagsJson` ainda não possui schema semântico;
- propriedades duplicadas no JSON não são tratadas especificamente;
- evolução futura pode exigir allowlist, versionamento, normalização ou contrato clínico formal;
- produção real ainda depende de validações macro de segurança, compliance, infraestrutura, produto e operação.

## 13. Impacto sobre produção real

A Fase 6.5 remove os bloqueios técnicos específicos de integridade física e validação estrutural mapeados para `MR-DEBT-007` e `MR-DEBT-009`.

Isso não equivale a liberação irrestrita para produção real. Produção real com dados clínicos sensíveis ainda depende de validações macro posteriores de segurança, compliance, infraestrutura, produto, operação, observabilidade, governança e processos de implantação.

## 14. Critérios de aceite

A Fase 6.5.4 é considerada aceita porque:

- o documento de encerramento foi criado;
- 6.5.1, 6.5.2, 6.5.2.1 e 6.5.3 foram consolidadas;
- PRs 155, 156, 157 e 158 foram referenciadas;
- `MR-DEBT-007` foi consolidado como resolvido;
- `MR-DEBT-009` foi consolidado como resolvido;
- riscos remanescentes foram documentados;
- limites explícitos foram documentados;
- a próxima fase foi recomendada;
- nenhuma implementação nova foi feita;
- nenhuma migration foi criada;
- somente `docs/clinical-core` foi alterado;
- `git diff --check` foi definido como validação obrigatória da fase.

## 15. Decisão final da Fase 6.5

```text
A Fase 6.5 está encerrada como trilha de integridade física e validação estrutural inicial de MedicalRecord.
```

## 16. Próxima fase recomendada

Próxima fase recomendada:

```text
Fase 6.6 — Qualidade operacional e evidências finais da vertical MedicalRecord.
```

Fase inicial sugerida:

```text
6.6.1 — Planejamento técnico de qualidade operacional MedicalRecord.
```

Débitos envolvidos:

```text
MR-DEBT-010 — CancellationToken não propagado no repository
MR-DEBT-011 — Evidências manuais Swagger não versionadas formalmente
MR-DEBT-012 — MedicalRecordListItemResponse ainda não usado
```
