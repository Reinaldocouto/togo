# TOGO — Fase 5.1.2: Fechamento da auditoria MedicalRecord e padrão de planejamento das subfases

## 1. Objetivo

Este documento fecha formalmente a subfase **5.1** (auditoria e preparação da vertical MedicalRecord) e institucionaliza um padrão de governança documental para as próximas aberturas de subfases da Fase 5.

Esta fase é **exclusivamente documental** e **não implementa código**.

## 2. Contexto

- A **Fase 5.0.0** definiu diretrizes clínicas, regulatórias e arquiteturais da vertical de prontuário.
- A **Fase 5.0.1** definiu o planejamento técnico da implementação de MedicalRecord.
- A **Fase 5.1.1** auditou o estado atual real de MedicalRecord.
- A auditoria confirmou que MedicalRecord já existe em **Domain / EF Core / migration**, mas ainda não existe nas camadas **Application, Infrastructure, API e Tests**.
- A próxima etapa funcional recomendada permanece: **Fase 5.2.1 — Testes de domínio de MedicalRecord**.

## 3. Fechamento da subfase 5.1

### Subfase 5.1 — Auditoria e preparação da vertical MedicalRecord

**Planejamento executado:**

- **5.1.1** — Auditoria do estado atual de MedicalRecord.
- **5.1.2** — Fechamento da auditoria e padronização do planejamento das subfases.

A subfase 5.1 teve como papel:

- verificar o estado real do projeto;
- confirmar o que já existe;
- mapear lacunas;
- mapear riscos;
- confirmar a próxima fase segura;
- evitar implementação prematura.

## 4. Achados consolidados da auditoria

Principais achados consolidados:

- MedicalRecord existe no Domain.
- MedicalRecord possui `Create` e `UpdateNotes`.
- MedicalRecord possui configuração EF Core.
- `MedicalRecords` já existe na migration.
- `AppDbContext` já possui `DbSet`.
- Não há repository de MedicalRecord.
- Não há interface `IMedicalRecordRepository`.
- Não há contracts.
- Não há validators.
- Não há use cases.
- Não há controller.
- Não há testes específicos.
- Índice `PatientId` não é único.
- `DeleteBehavior.Cascade` deve ser revisado futuramente.
- Soft Delete e AuditLog continuam como débitos técnicos obrigatórios.

## 5. Padrão obrigatório para abertura de novas subfases

A partir da **Fase 5.2**, toda abertura de subfase maior deve conter um resumo curto de planejamento/fracionamento da própria subfase.

Exemplo de estrutura:

### Subfase 5.2 — Domínio MedicalRecord

**Planejamento:**

- 5.2.1 — Testes de domínio de MedicalRecord.
- 5.2.2 — Ajustes de domínio, se necessários.
- 5.2.3 — Documentação final do domínio MedicalRecord.
- 5.2.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

Este padrão vale para:

- 5.2 — Domain;
- 5.3 — Application;
- 5.4 — Infrastructure;
- 5.5 — API;
- 5.6 — Validação, documentação final e auditoria.

## 6. Planejamento macro atualizado da Fase 5

- 5.0 — Diretrizes e planejamento inicial.
- 5.1 — Auditoria e preparação.
- 5.2 — Domain MedicalRecord.
- 5.3 — Application MedicalRecord.
- 5.4 — Infrastructure MedicalRecord.
- 5.5 — API MedicalRecord.
- 5.6 — Validação final, documentação e auditoria da vertical.

## 7. Planejamento esperado da subfase 5.2

### Subfase 5.2 — Domain MedicalRecord

**Planejamento sugerido:**

- 5.2.1 — Testes de domínio de MedicalRecord.
- 5.2.2 — Ajustes de domínio, se os testes revelarem necessidade.
- 5.2.3 — Documentação final do domínio MedicalRecord.

**Possíveis fases complementares:**

- 5.2.1.1 — Correção de testes de domínio.
- 5.2.2.1 — Ajuste pontual de entidade.
- 5.2.3.1 — Complemento documental.

## 8. Regras para fases complementares

Fases complementares podem ser criadas quando houver:

- correção de bug;
- inconsistência técnica;
- documentação insuficiente;
- ajuste emergente;
- necessidade de refatoração pequena;
- evidência adicional;
- correção de PR;
- conflito documental;
- validação complementar.

Formato sugerido:

- `5.x.y.z`

Exemplos:

- 5.2.1.1 — Correção de teste de domínio.
- 5.3.4.1 — Ajuste de use case.
- 5.5.2.1 — Correção de teste de API.

## 9. Decisões confirmadas

- A subfase 5.1 está concluída após este documento.
- A próxima fase funcional será 5.2.1.
- A Fase 5 seguirá fracionada por camadas.
- Toda nova subfase maior deverá iniciar com breve resumo de planejamento.
- A prática melhora rastreabilidade, comunicação e organização do projeto.

## 10. Riscos se esse padrão não for seguido

- perda de orientação do roadmap;
- documentação fragmentada;
- dificuldade de comunicação;
- dificuldade de explicar a evolução do projeto;
- aumento de retrabalho;
- confusão entre fases principais e correções pontuais.

## 11. Critérios de aceite da Fase 5.1.2

Esta fase será considerada concluída se:

- fechar a subfase 5.1;
- registrar os achados principais da auditoria;
- formalizar o padrão de planejamento na abertura das subfases;
- atualizar o macroplanejamento da Fase 5;
- planejar a subfase 5.2;
- confirmar que a próxima fase funcional é 5.2.1;
- não alterar código;
- não alterar testes;
- não alterar migrations;
- não alterar banco;
- não executar database update.

## 12. Fora do escopo

Esta fase não implementa:

- código;
- testes;
- contracts;
- repository;
- validators;
- use cases;
- controller;
- migration;
- database update;
- Soft Delete;
- AuditLog;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## 13. Próxima fase recomendada

**Fase 5.2.1 — Testes de domínio de MedicalRecord.**

**Objetivo:**

Criar testes unitários de domínio para validar a entidade MedicalRecord antes de alterar ou expandir qualquer camada.

**Testes esperados:**

- Create válido;
- `PatientId` inválido;
- `UpdatedAt` default;
- normalização de `GeneralNotes`;
- normalização de `FlagsJson`;
- campos opcionais nulos;
- campos opcionais vazios normalizados para `null`;
- `UpdateNotes` válido;
- `UpdateNotes` com `GeneralNotes = null`;
- `UpdateNotes` com `FlagsJson = null`;
- `UpdateNotes` com `UpdatedAt = default`;
- preservação de `PatientId`;
- preservação de `Id` quando aplicável.

## 14. Validações obrigatórias no final

Validações executadas nesta fase documental:

- `git branch --show-current`
- `git status --short`
- `git diff --check`

Se `dotnet` não estiver disponível no ambiente, a limitação deve ser registrada sem inventar resultados de build/test.

## 15. Entrega esperada

PR documental com:

- summary claro;
- testing claro;
- confirmação de que apenas um documento foi criado;
- confirmação de que não houve alteração em código, teste, migration, banco ou workflow.
