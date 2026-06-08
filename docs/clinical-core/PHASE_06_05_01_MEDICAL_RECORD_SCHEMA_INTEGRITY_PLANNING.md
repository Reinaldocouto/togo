# TOGO — Fase 6.5.1: Planejamento de integridade física e validação estrutural de MedicalRecord

## 1. Contexto e objetivo

A Fase 6.4 foi encerrada formalmente pela PR 154 e pelo documento `docs/clinical-core/PHASE_06_04_06_MEDICAL_RECORD_SAFE_PERSISTENCE_CLOSURE.md`.

Na Fase 6.4, a vertical `MedicalRecord` tratou tecnicamente os débitos P1 restantes de persistência clínica segura:

- `MR-DEBT-001` — Soft Delete ausente;
- `MR-DEBT-005` — Política de retenção não implementada;
- `MR-DEBT-006` — `DeleteBehavior.Cascade` pendente de revisão.

A Fase 6.5 reabre o hardening de `MedicalRecord` com foco em integridade física, evolução de schema e validação estrutural. Esta subfase 6.5.1 é exclusivamente documental e de governança técnica: ela define uma estratégia segura e incremental para tratar, em subfases posteriores, os débitos:

- `MR-DEBT-007` — Índice único em `MedicalRecords.PatientId` ausente;
- `MR-DEBT-009` — `FlagsJson` flexível.

Nenhuma implementação é realizada nesta fase.

## 2. Fontes consideradas

Este planejamento foi elaborado a partir das seguintes fontes principais:

- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`;
- `docs/clinical-core/PHASE_06_04_01_MEDICAL_RECORD_SAFE_PERSISTENCE_PLANNING.md`;
- `docs/clinical-core/PHASE_06_04_03_MEDICAL_RECORD_SOFT_DELETE_QUERY_FILTERS.md`;
- `docs/clinical-core/PHASE_06_04_06_MEDICAL_RECORD_SAFE_PERSISTENCE_CLOSURE.md`;
- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Repositories/MedicalRecordRepository.cs`;
- `backend/src/Togo.Application/MedicalRecords/Validators/MedicalRecordUniquenessValidator.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/CreateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/UpdateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Infrastructure/Migrations`;
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`.

## 3. Estado atual pós-Fase 6.4

Após o encerramento da Fase 6.4, a vertical `MedicalRecord` possui os seguintes controles técnicos mínimos:

- proteção de autorização granular mínima para fluxos clínicos de `MedicalRecord`;
- autoria clínica mínima em criação e atualização;
- `AuditLog` mínimo para eventos clínicos de criação e atualização;
- Soft Delete em `MedicalRecord`;
- filtros padrão que ignoram registros com Soft Delete;
- retenção clínica inicial definida como indefinida;
- revisão de cascades críticos em relacionamentos clínicos;
- `CreatedAt` e `UpdatedAt`;
- `CreatedByUserId` e `UpdatedByUserId`;
- `IsDeleted`, `DeletedAt` e `DeletedByUserId`.

Entretanto, ainda permanecem dois pontos estruturais importantes para a Fase 6.5:

- `MedicalRecords.PatientId` ainda não possui constraint ou índice único físico; a configuração/snapshot atual registra índice simples não único para `PatientId`;
- `MedicalRecord.FlagsJson` ainda é mantido como `string` flexível, persistida como texto, sem validação estrutural de JSON e sem contrato semântico mínimo documentado como regra executável.

## 4. Risco atual de `MR-DEBT-007` — unicidade apenas lógica em `PatientId`

O débito `MR-DEBT-007` permanece relevante porque a regra de prontuário 1:1 por paciente ainda depende de controle lógico na camada de aplicação.

Hoje, a criação de `MedicalRecord` passa por validator/use case antes de persistir um novo registro. Essa estratégia reduz o risco no fluxo padrão, mas não equivale a uma constraint física no banco. Sem índice único físico:

- concorrência extrema pode permitir duas transações passarem pela validação lógica antes da persistência final;
- scripts, operações administrativas, cargas de dados ou integrações fora da aplicação podem inserir duplicidades;
- falhas futuras de código podem contornar o validator e criar registros inconsistentes;
- duplicidade de `MedicalRecord` por `PatientId` quebra o conceito clínico de prontuário 1:1 por paciente;
- consultas por paciente passam a depender de uma premissa que o banco não garante;
- relatórios, integrações, auditorias e rotinas de retenção podem produzir resultados ambíguos quando houver mais de um prontuário para o mesmo paciente.

O Soft Delete torna a decisão de unicidade mais delicada. A Fase 6.4.3 documentou que o fluxo padrão permite criação de novo `MedicalRecord` quando o paciente possui apenas registro logicamente deletado, porque `ExistsByPatientIdAsync` ignora deletados e porque não existe índice único físico em `PatientId`. Essa decisão operacional entra em tensão com uma visão mais conservadora de prontuário clínico 1:1, na qual mesmo um registro deletado logicamente continuaria contando como histórico clínico do paciente.

## 5. Decisão a planejar sobre unicidade e Soft Delete

A Fase 6.5 precisa escolher explicitamente como a unicidade de `MedicalRecord.PatientId` interage com Soft Delete antes de qualquer migration.

### 5.1. Opção A — índice único simples em `PatientId`

Criar um índice único simples em `MedicalRecords.PatientId`.

Efeitos esperados:

- impede qualquer segundo `MedicalRecord` para o mesmo paciente;
- bloqueia duplicidade inclusive quando o registro anterior estiver com `IsDeleted = true`;
- mantém a interpretação mais conservadora para histórico clínico, em que Soft Delete não apaga a existência histórica do prontuário;
- simplifica a garantia física do banco;
- reduz risco de inconsistência causada por concorrência ou operação fora da aplicação.

Risco/atenção:

- pode conflitar com a decisão operacional documentada na Fase 6.4.3, que permitiu criação após Soft Delete;
- exigirá decisão de produto/arquitetura sobre restauração, consulta administrativa e retenção antes da implementação;
- exigirá verificação e saneamento de dados preexistentes antes de aplicar a constraint.

### 5.2. Opção B — índice composto/filtrado considerando `IsDeleted`

Criar uma estratégia que permita apenas um `MedicalRecord` ativo por paciente, ignorando registros logicamente deletados.

Possibilidades conceituais:

- índice único composto envolvendo `PatientId` e `IsDeleted`;
- índice filtrado equivalente a `PatientId` único apenas quando `IsDeleted = false`;
- coluna auxiliar para materializar a unicidade de registros ativos, caso o provider/banco não suporte filtro da forma desejada.

Efeitos esperados:

- permite um prontuário ativo por paciente;
- preserva a possibilidade de criação após Soft Delete;
- alinha-se melhor ao comportamento operacional decidido na Fase 6.4.3.

Risco/atenção:

- pode ser mais complexo no provider MySQL/Pomelo, pois índice filtrado não possui a mesma semântica direta do SQL Server;
- índice único composto simples em `(PatientId, IsDeleted)` pode não resolver todos os cenários se houver múltiplos registros deletados para o mesmo paciente;
- pode exigir coluna auxiliar, constraint específica ou desenho adicional;
- pode aumentar complexidade de migration, testes de infraestrutura e documentação;
- pode ser menos conservador para histórico clínico se o produto entender que prontuário deletado ainda deve bloquear recriação.

### 5.3. Opção C — manter unicidade lógica por enquanto e documentar decisão

Manter o comportamento atual, no qual validators/use cases controlam a unicidade operacional e o banco mantém apenas índice não único.

Efeitos esperados:

- evita migration imediata;
- preserva a flexibilidade atual durante definição de produto;
- reduz impacto de curto prazo.

Risco/atenção:

- é menos seguro fisicamente;
- não protege contra concorrência extrema ou operações fora da aplicação;
- não resolve completamente `MR-DEBT-007`;
- deve ser tratada apenas como decisão temporária ou exceção formal muito bem justificada.

### 5.4. Recomendação inicial

Para um prontuário clínico 1:1, a recomendação técnica inicial é preferir a Opção A — índice único simples em `PatientId` — salvo se houver decisão forte de produto permitindo recriação de prontuário após Soft Delete.

A Fase 6.5.1 registra explicitamente a tensão técnica: a Fase 6.4.3 permitiu criação após Soft Delete no fluxo operacional atual, enquanto a Opção A impediria esse comportamento no banco. Portanto, antes da implementação de `MR-DEBT-007`, deve haver decisão explícita sobre se um registro logicamente deletado ainda conta para unicidade clínica.

Caso a decisão final seja manter a recriação após Soft Delete, recomenda-se criar uma subfase documental intermediária curta antes da migration, por exemplo:

- 6.5.2 — Decisão final sobre unicidade física e Soft Delete;
- 6.5.3 — Implementação da unicidade física escolhida;
- 6.5.4 — Validação estrutural inicial de `FlagsJson`;
- 6.5.5 — Testes finais, atualização do registro vivo e encerramento da Fase 6.5.

Se a decisão final for Opção A sem divergência de produto, a subfase intermediária pode ser incorporada diretamente à implementação da 6.5.2, desde que a decisão fique registrada antes da migration.

## 6. Plano futuro para `MR-DEBT-007`

Antes de criar qualquer índice único ou constraint para `MedicalRecords.PatientId`, será necessário:

1. verificar se há dados duplicados existentes por `PatientId`;
2. definir estratégia de saneamento caso existam duplicidades;
3. alinhar comportamento de unicidade com Soft Delete;
4. decidir explicitamente se registro deletado ainda conta para unicidade;
5. confirmar se a regra clínica final é “um prontuário total por paciente” ou “um prontuário ativo por paciente”;
6. criar teste de infraestrutura validando que duplicidade é bloqueada pelo banco;
7. revisar coerência dos validators e use cases com a decisão final;
8. criar migration segura, se a decisão for constraint/índice físico;
9. atualizar documentação da fase correspondente;
10. atualizar o registro vivo de débitos técnicos.

## 7. Risco atual de `MR-DEBT-009` — `FlagsJson` como string flexível

O débito `MR-DEBT-009` permanece relevante porque `MedicalRecord.FlagsJson` ainda aceita conteúdo textual flexível.

Essa flexibilidade tem baixo custo inicial, mas cria riscos estruturais:

- pode receber JSON inválido;
- pode receber estrutura JSON válida, porém inconsistente;
- pode receber formatos diferentes para o mesmo significado clínico;
- pode vazar para uso clínico futuro sem contrato claro;
- dificulta validação de regras de domínio;
- dificulta evolução segura de schema;
- dificulta busca, indexação e relatórios;
- dificulta interoperabilidade com integrações futuras;
- pode induzir logs, auditorias ou integrações a propagarem payloads sensíveis sem desenho específico.

Atualmente, `FlagsJson` é retornado nos responses de criação/atualização/consulta e pode ser atualizado pelo fluxo padrão. Sem validação estrutural, a aplicação não diferencia ausência de flags, string inválida, JSON válido sem contrato ou payload semanticamente incompatível.

## 8. Estratégias possíveis para `FlagsJson`

### 8.1. Opção A — validar se `FlagsJson` é JSON válido, mantendo `string`

Manter `FlagsJson` como `string`, mas rejeitar valores não nulos/não vazios que não sejam JSON válido.

Efeitos esperados:

- menor impacto no domínio, DTOs, banco e consumers;
- boa primeira etapa de hardening;
- reduz risco de lixo estrutural persistido;
- preserva compatibilidade de schema físico.

Limitações:

- não resolve schema semântico;
- ainda permite qualquer objeto/array/valor JSON se não houver restrição adicional;
- não melhora diretamente busca ou interoperabilidade.

### 8.2. Opção B — validar JSON válido e estrutura mínima permitida

Manter `FlagsJson` como `string`, mas exigir JSON válido com estrutura mínima permitida.

Exemplo conceitual, a confirmar em fase posterior:

- permitir `null` ou vazio como ausência de flags;
- quando informado, exigir objeto JSON;
- aceitar somente chaves conhecidas ou um subconjunto aprovado;
- rejeitar arrays, números, strings JSON soltas ou objetos com estrutura incompatível, se essa for a decisão de contrato.

Efeitos esperados:

- melhor controle sem normalizar imediatamente;
- preserva flexibilidade moderada;
- reduz divergência semântica;
- prepara caminho para normalização futura se regras clínicas amadurecerem.

Limitações:

- exige decisão mínima de produto/técnica sobre chaves e formato;
- pode quebrar payloads preexistentes se houver dados fora do contrato;
- ainda não oferece modelagem relacional para busca avançada.

### 8.3. Opção C — normalizar flags em entidade/tabela própria

Criar entidade/tabela própria para representar flags clínicas.

Efeitos esperados:

- solução mais robusta para integridade e evolução relacional;
- permite constraints por campo, busca, auditoria mais específica e relações claras;
- facilita interoperabilidade quando regras clínicas estiverem fechadas.

Limitações:

- maior impacto no domínio, migrations, repositories, use cases, testes e contratos;
- pode ser prematuro sem regra clínica concreta;
- aumenta escopo e risco de regressão se feito antes de validar o formato real necessário.

### 8.4. Opção D — substituir `FlagsJson` por campos fortemente tipados

Remover a flexibilidade do JSON e representar as flags conhecidas como propriedades explícitas.

Efeitos esperados:

- melhor para regras conhecidas e estáveis;
- validação mais simples;
- facilita queries e documentação de contrato.

Limitações:

- pode limitar flexibilidade;
- exige regra de produto clara;
- pode gerar migrations frequentes se o conjunto de flags ainda estiver instável.

### 8.5. Recomendação inicial

A recomendação técnica inicial é começar pela Opção A ou B:

- validar JSON válido;
- rejeitar string inválida;
- preservar `null`/vazio como ausência de flags;
- preferir estrutura mínima se houver consenso rápido sobre formato aceitável;
- não normalizar em tabela própria sem regra clínica concreta;
- não substituir por campos fortemente tipados sem estabilidade funcional das flags.

## 9. Plano futuro para `MR-DEBT-009`

Para resolver `MR-DEBT-009`, será necessário:

1. decidir o formato mínimo aceito de `FlagsJson`;
2. decidir se o valor informado deve ser objeto JSON ou se outros tipos JSON serão aceitos;
3. decidir se haverá chaves conhecidas, allowlist, versionamento ou contrato mínimo;
4. implementar validação no domínio, em use case ou em componente de validação dedicado, conforme arquitetura aprovada;
5. preservar `null` e vazio como ausência de flags, se esta decisão for mantida;
6. rejeitar JSON inválido;
7. criar testes de domínio e/ou Application cobrindo flags válidas, inválidas, vazias e nulas;
8. garantir que metadata e `AuditLog` não recebam payload sensível completo de `FlagsJson`;
9. revisar responses apenas se a decisão futura alterar contrato de API;
10. atualizar documentação da fase correspondente;
11. atualizar o registro vivo de débitos técnicos.

## 10. Sequenciamento recomendado da Fase 6.5

Sequenciamento base recomendado:

- 6.5.1 — Planejamento técnico de integridade física e validação estrutural;
- 6.5.2 — Decisão/implementação de unicidade física de `MedicalRecord.PatientId`;
- 6.5.3 — Validação estrutural inicial de `FlagsJson`;
- 6.5.4 — Testes finais, atualização do registro vivo e encerramento da Fase 6.5.

Sequenciamento alternativo recomendado se a tensão entre índice único e Soft Delete exigir decisão formal separada:

- 6.5.1 — Planejamento técnico de integridade física e validação estrutural;
- 6.5.2 — Decisão documental final sobre unicidade física, Soft Delete e recriação de prontuário;
- 6.5.3 — Implementação da unicidade física escolhida para `MedicalRecord.PatientId`;
- 6.5.4 — Validação estrutural inicial de `FlagsJson`;
- 6.5.5 — Testes finais, atualização do registro vivo e encerramento da Fase 6.5.

A subfase documental intermediária é recomendada se houver qualquer divergência entre produto, retenção clínica e arquitetura sobre recriação de prontuário após Soft Delete. O objetivo é evitar uma migration incompatível com a política clínica futura.

## 11. Critérios futuros para resolver `MR-DEBT-007`

`MR-DEBT-007` só poderá ser considerado resolvido quando todos os critérios aplicáveis forem atendidos:

- houver decisão explícita sobre unicidade considerando Soft Delete;
- a decisão deixar claro se registro deletado ainda conta para unicidade;
- dados existentes forem considerados e, se necessário, saneados;
- existir constraint/índice físico ou decisão formal alternativa muito bem justificada;
- testes de infraestrutura validarem duplicidade bloqueada pelo banco, quando houver constraint/índice;
- use cases continuarem coerentes com a regra final;
- validators continuarem coerentes com a regra final;
- migration for criada, revisada e aplicada em ambiente de teste, se aplicável;
- documentação da subfase for atualizada;
- o registro vivo de débitos técnicos for atualizado.

## 12. Critérios futuros para resolver `MR-DEBT-009`

`MR-DEBT-009` só poderá ser considerado resolvido quando todos os critérios aplicáveis forem atendidos:

- houver decisão explícita sobre formato permitido de `FlagsJson`;
- JSON inválido for rejeitado;
- `null`/vazio for tratado como ausência de flags;
- testes cobrirem flags válidas, inválidas, vazias e nulas;
- não houver vazamento de payload sensível completo em `AuditLog`;
- a camada escolhida para validação estiver documentada;
- eventual impacto em responses/contratos estiver documentado;
- documentação da subfase for atualizada;
- o registro vivo de débitos técnicos for atualizado.

## 13. Impactos técnicos previstos

As subfases futuras da Fase 6.5 podem impactar:

- domínio `MedicalRecord`, especialmente validação de `FlagsJson` e semântica de Soft Delete;
- validators de existência/unicidade;
- use cases de criação e atualização;
- repository, especialmente consultas por `PatientId` e comportamento com registros deletados;
- configuração EF de `MedicalRecord`;
- migrations e snapshot do DbContext;
- testes de domínio;
- testes de Application;
- testes de Infrastructure;
- documentação de fases clínicas;
- registro vivo de débitos técnicos.

## 14. Riscos e pontos de atenção

- A escolha de índice único simples pode invalidar a criação após Soft Delete documentada na Fase 6.4.3.
- A escolha de unicidade apenas para registros ativos pode exigir desenho específico para MySQL/Pomelo.
- Dados duplicados preexistentes podem bloquear migration de unicidade.
- Validação muito permissiva de `FlagsJson` pode não resolver o risco semântico.
- Validação muito restritiva de `FlagsJson` pode quebrar clientes ou dados já persistidos, se existirem.
- Normalização precoce de flags pode aumentar escopo sem regra clínica madura.
- Payloads clínicos em `FlagsJson` não devem ser copiados integralmente para metadata de auditoria sem desenho específico.

## 15. Fora do escopo da Fase 6.5.1

A Fase 6.5.1 não implementa:

- índice único;
- constraint física;
- migration;
- alteração em `MedicalRecord`;
- alteração em `FlagsJson`;
- validação JSON;
- normalização de flags;
- nova entidade para flags;
- nova tabela de flags;
- alteração de `DbContext`;
- alteração de configuração EF;
- alteração de repository;
- alteração de validators;
- alteração de use cases;
- alteração de controller;
- endpoint novo;
- alteração de frontend;
- alteração de Docker, Redis, RabbitMQ ou Kubernetes;
- alteração de testes.

## 16. Próxima fase recomendada

Próxima fase recomendada: Fase 6.5.2.

Escopo recomendado para a 6.5.2:

- decidir explicitamente se Soft Delete bloqueia recriação de `MedicalRecord` para o mesmo `PatientId`;
- confirmar Opção A, B ou C para `MR-DEBT-007`;
- verificar/sanear duplicidades existentes antes de qualquer constraint;
- implementar a unicidade física escolhida apenas se a decisão estiver fechada;
- criar migration segura, se aplicável;
- criar teste de infraestrutura para duplicidade;
- atualizar o registro vivo e a documentação.

## 17. Critérios de aceite desta subfase

A Fase 6.5.1 é considerada concluída quando:

- este documento existe em `docs/clinical-core/PHASE_06_05_01_MEDICAL_RECORD_SCHEMA_INTEGRITY_PLANNING.md`;
- `MR-DEBT-007` e `MR-DEBT-009` são explicitamente referenciados;
- o estado pós-Fase 6.4 está documentado;
- o risco de unicidade lógica sem constraint física está explicado;
- a tensão entre Soft Delete e índice único está documentada;
- opções de unicidade estão comparadas;
- recomendação inicial de unicidade está registrada;
- risco de `FlagsJson` flexível está explicado;
- opções para `FlagsJson` estão comparadas;
- recomendação inicial para `FlagsJson` está registrada;
- sequenciamento da Fase 6.5 está definido;
- critérios futuros para resolver `MR-DEBT-007` e `MR-DEBT-009` estão definidos;
- nenhum código foi alterado;
- nenhuma migration foi criada;
- o escopo permaneceu exclusivamente documental;
- `git diff --check` passa.
