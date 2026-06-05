# Fase 6.4.3 — MedicalRecord Soft Delete Query Filters

## 1. Contexto da Fase 6.4

A Fase 6.4 consolida os itens P1 remanescentes ligados à persistência clínica segura de `MedicalRecord`.

Os débitos acompanhados nesta frente são:

- `MR-DEBT-001` — Soft Delete ausente.
- `MR-DEBT-005` — Política de retenção não implementada.
- `MR-DEBT-006` — `DeleteBehavior.Cascade` pendente de revisão.

A Fase 6.4.3 trata somente o comportamento operacional mínimo do Soft Delete nos fluxos clínicos padrão.

## 2. Referência explícita ao MR-DEBT-001

Esta fase avança o `MR-DEBT-001` ao garantir que registros marcados com `IsDeleted = true` deixem de aparecer como ativos nas consultas e validações clínicas padrão.

A implementação base do Soft Delete já existia desde a Fase 6.4.2. A Fase 6.4.3 adiciona a camada de filtragem operacional necessária para que a marcação lógica tenha efeito nos fluxos de leitura, existência, atualização e criação.

## 3. Relação com as Fases 6.4.1 e 6.4.2

- A Fase 6.4.1 planejou a persistência clínica segura e organizou a abordagem para os débitos P1.
- A Fase 6.4.2 implementou a base técnica do Soft Delete em `MedicalRecord`, incluindo campos, método de domínio, use case interno, migration e testes iniciais.
- A Fase 6.4.3 complementa a Fase 6.4.2 aplicando filtros explícitos nas consultas padrão e adicionando testes para evidenciar que registros logicamente deletados não são tratados como ativos.

## 4. Objetivo da fase

Garantir que os fluxos clínicos padrão de `MedicalRecord` não retornem, atualizem ou validem como ativo um registro logicamente deletado.

Esta fase não cria endpoint público de DELETE e não implementa política de retenção, restauração, expurgo físico ou consulta administrativa de registros deletados.

## 5. Decisão técnica adotada

Foi adotado filtro explícito no repository, e não query filter global do Entity Framework Core.

Motivos:

- maior previsibilidade para os fluxos clínicos padrão;
- menor risco de afetar consultas administrativas futuras;
- escopo da fase restrito a comportamento operacional padrão;
- evita esconder implicitamente registros em cenários futuros que precisem de visão administrativa ou retenção.

## 6. Comportamento das consultas padrão

As consultas padrão de `MedicalRecordRepository` passam a considerar apenas registros com `IsDeleted = false`.

Isso significa que, para os fluxos clínicos normais, um registro com `IsDeleted = true` é tratado como inexistente.

## 7. Comportamento de `GetByPatientIdAsync`

`GetByPatientIdAsync` retorna o `MedicalRecord` do paciente somente quando há registro com:

- `PatientId` correspondente;
- `IsDeleted = false`.

Quando o único registro do paciente está logicamente deletado, o método retorna `null`.

## 8. Comportamento de `GetByIdAsync`

`GetByIdAsync` retorna o `MedicalRecord` somente quando há registro com:

- `Id` correspondente;
- `IsDeleted = false`.

Quando o registro encontrado pelo identificador está logicamente deletado, o método retorna `null`.

## 9. Comportamento de `ExistsByPatientIdAsync`

`ExistsByPatientIdAsync` retorna `true` somente quando existe registro ativo para o paciente.

Registros com `IsDeleted = true` são ignorados e fazem o método retornar `false` quando não houver outro registro ativo.

## 10. Comportamento de Update sobre registro deletado

O fluxo padrão de `UpdateMedicalRecordUseCase` não atualiza registro deletado.

Como o validator de existência usa `ExistsByPatientIdAsync`, e esse método ignora deletados, a atualização de paciente cujo único `MedicalRecord` está deletado retorna `NotFound`.

Evidências esperadas e testadas:

- retorno `NotFound`;
- `GeneralNotes` não é alterado;
- `FlagsJson` não é alterado;
- `UpdatedAt` e `UpdatedByUserId` permanecem preservados;
- nenhum audit log `MedicalRecord.Updated` é gravado.

## 11. Comportamento de Create quando existe registro deletado

Decisão desta fase: permitir criação de novo `MedicalRecord` quando o paciente possui apenas registro logicamente deletado.

Justificativa:

- o fluxo clínico padrão passa a considerar registros deletados como não ativos;
- o validator de unicidade usa `ExistsByPatientIdAsync`, que agora ignora deletados;
- não há índice único físico em `PatientId` na configuração atual, apenas índice não único;
- a política definitiva de retenção/histórico ainda será desenhada em fase posterior.

Risco documentado: essa decisão pode ser revista quando a política de retenção clínica for implementada, especialmente se houver necessidade de restringir múltiplos históricos por paciente ou introduzir regras administrativas para restauração.

## 12. Comportamento de Soft Delete duplicado

Decisão desta fase: tentativa de Soft Delete quando o único registro do paciente já está logicamente deletado retorna `NotFound` no fluxo de aplicação.

Justificativa:

- evita criar método administrativo ou `IncludingDeleted` nesta fase;
- preserva a regra operacional de que deletado não deve ser exposto como ativo;
- mantém a superfície do repository restrita a fluxos clínicos padrão;
- o conflito de domínio continua existindo se `SoftDelete` for chamado duas vezes na mesma entidade já carregada em memória.

## 13. Impacto em validators

`MedicalRecordExistsValidator` passa a considerar registros deletados como inexistentes porque depende de `ExistsByPatientIdAsync`.

`MedicalRecordUniquenessValidator` passa a permitir criação quando só existem registros deletados porque também depende de `ExistsByPatientIdAsync`.

Nenhuma nova interface administrativa foi criada para consultar registros deletados.

## 14. Impacto em repository

`MedicalRecordRepository` foi ajustado para filtrar explicitamente `!IsDeleted` em:

- `GetByIdAsync`;
- `GetByPatientIdAsync`;
- `ExistsByPatientIdAsync`.

`AddAsync` permanece persistindo novos registros com `IsDeleted = false` pela criação de domínio e pelo default de banco.

`UpdateAsync` permanece capaz de persistir os campos de Soft Delete quando recebe uma entidade ativa que acabou de ser marcada via método de domínio.

Não foi criado método de exclusão física.

## 15. Impacto em use cases

- `GetMedicalRecordByPatientIdUseCase`: retorna `NotFound` quando o registro do paciente está deletado.
- `UpdateMedicalRecordUseCase`: retorna `NotFound` quando o registro do paciente está deletado e não grava audit log de update.
- `CreateMedicalRecordUseCase`: permite criar novo registro quando só existe registro deletado.
- `SoftDeleteMedicalRecordUseCase`: retorna `NotFound` em tentativa duplicada contra registro já deletado, sem carregar registro deletado por caminho especial.

## 16. Testes criados/alterados

Testes de Infrastructure adicionados/ajustados:

- `GetByIdAsync` não retorna registro deletado;
- `GetByPatientIdAsync` não retorna registro deletado;
- `ExistsByPatientIdAsync` retorna `false` para registro deletado;
- `AddAsync` segue persistindo `IsDeleted = false`;
- `UpdateAsync` segue persistindo campos de Soft Delete;
- contrato do repository não expõe método de exclusão física.

Testes de Application adicionados/ajustados:

- `GetMedicalRecordByPatientIdUseCase` retorna `NotFound` para registro deletado;
- `UpdateMedicalRecordUseCase` retorna `NotFound` para registro deletado, não altera campos clínicos e não grava audit log de update;
- `CreateMedicalRecordUseCase` cria novo registro quando só existe registro deletado;
- `SoftDeleteMedicalRecordUseCase` passa a retornar `NotFound` em tentativa duplicada contra registro já deletado.

## 17. Decisão sobre endpoint DELETE público

Nenhum endpoint público DELETE foi criado nesta fase.

A existência de `SoftDeleteMedicalRecordUseCase` permanece como base de aplicação sem exposição pública por rota.

## 18. Decisão sobre AuditLog `MedicalRecord.SoftDeleted`

Não foi implementado evento de audit log `MedicalRecord.SoftDeleted` nesta fase.

Motivo: manter o escopo restrito aos filtros operacionais do Soft Delete e evitar ampliar auditoria clínica sem desenho específico.

## 19. Riscos remanescentes

- `MR-DEBT-005` permanece aberto: política de retenção clínica ainda não foi implementada.
- `MR-DEBT-006` permanece aberto: `DeleteBehavior.Cascade` ainda não foi revisado nesta fase.
- A recriação após Soft Delete pode gerar múltiplos registros históricos por paciente; isso está documentado e deve ser revisitado na política de retenção/histórico.
- Não há consulta administrativa de registros deletados.
- Não há restauração de registros deletados.
- Não há expurgo físico controlado.

## 20. Critérios de aceite

A fase atende aos critérios quando:

- consultas padrão não retornam registros com `IsDeleted = true`;
- `ExistsByPatientIdAsync` ignora registros deletados;
- `GetByPatientIdAsync` ignora registros deletados;
- `GetByIdAsync` ignora registros deletados;
- update padrão não atualiza registro deletado;
- criação com registro anterior deletado tem comportamento decidido, testado e documentado;
- Soft Delete duplicado tem comportamento decidido, testado e documentado;
- nenhum endpoint DELETE público é criado;
- nenhuma exclusão física é implementada;
- retenção clínica não é implementada nesta fase;
- revisão de cascade não é implementada nesta fase;
- testes de Application cobrem os fluxos principais;
- testes de Infrastructure cobrem os filtros;
- documentação da fase é criada.

## 21. Fora do escopo

- endpoint público DELETE;
- rota pública para Soft Delete;
- política de retenção clínica;
- expurgo físico;
- job, scheduler ou worker;
- revisão de `DeleteBehavior.Cascade`;
- alteração de relações clínicas;
- alteração de JWT;
- alteração de `User` ou `Profile`;
- autorização granular;
- tela frontend;
- consulta administrativa de registros deletados;
- restauração de registros deletados;
- auditoria de leitura;
- auditoria de acesso negado;
- evento de audit log `MedicalRecord.SoftDeleted`.

## 22. Próxima fase recomendada

Próxima fase recomendada: Fase 6.4.4 — Revisão de `DeleteBehavior.Cascade` em entidades clínicas.

Objetivo recomendado: mapear e revisar cascades clínicos que possam causar exclusão física indireta de histórico, especialmente envolvendo `Patient` e `MedicalRecord`, e ajustar para `Restrict` ou `NoAction` quando necessário.
