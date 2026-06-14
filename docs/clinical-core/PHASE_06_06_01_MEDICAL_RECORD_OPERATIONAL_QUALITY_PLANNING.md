# TOGO — Fase 6.6.1: Planejamento técnico de qualidade operacional MedicalRecord

## 1. Objetivo

A Fase 6.6.1 planeja a trilha final de qualidade operacional da vertical `MedicalRecord`, antes de qualquer implementação dos últimos débitos P3 mapeados no registro vivo.

Esta fase é exclusivamente documental e de governança técnica. Ela organiza riscos, decisões futuras, critérios de resolução, sequenciamento e impactos previstos para a Fase 6.6, sem alterar código, testes, contracts, repositories, use cases, controllers, migrations, banco, frontend ou infraestrutura.

## 2. Contexto da Fase 6.6

A Fase 6.6 vem após a consolidação incremental da vertical `MedicalRecord` nas fases anteriores de hardening clínico. Conforme o registro vivo e os documentos de encerramento das Fases 6.3, 6.4 e 6.5, o estado anterior já cobre:

- autorização granular mínima;
- autoria clínica mínima;
- auditoria mínima com `ClinicalAuditLog` para criação e atualização;
- Soft Delete clínico mínimo;
- filtros padrão que ignoram registros logicamente deletados;
- retenção clínica inicial conservadora;
- revisão de cascades clínicos críticos;
- unicidade física por `PatientId`;
- tratamento de conflito concorrente da constraint de unicidade como conflito de negócio/HTTP 409;
- validação estrutural inicial de `FlagsJson`.

A Fase 6.6 deve tratar os débitos finais de qualidade operacional, evidência manual versionada e decisão sobre contrato de listagem ainda não utilizado.

## 3. Débitos envolvidos

Débitos explicitamente envolvidos na Fase 6.6:

```text
MR-DEBT-010 — CancellationToken não propagado no repository
MR-DEBT-011 — Evidências manuais Swagger não versionadas formalmente
MR-DEBT-012 — MedicalRecordListItemResponse ainda não usado
```

No registro vivo, os três itens permanecem como P3 abertos, sem bloqueio direto do MVP técnico e sem bloqueio direto isolado para produção real, mas relevantes para maturidade operacional, rastreabilidade de QA manual e governança de superfície futura de API.

## 4. Estado atual pós-Fase 6.5

O estado técnico atual de `MedicalRecord`, após o encerramento da Fase 6.5, é o seguinte:

- **Segurança/autorização:** a API de `MedicalRecord` está protegida por autenticação e policies granulares mínimas por operação de leitura, criação e atualização. Ainda não há endpoint público de listagem de prontuários.
- **Autoria:** `MedicalRecord` possui autoria clínica mínima persistida para criação e última atualização, com usuário autenticado e timestamps UTC.
- **AuditLog:** existe `ClinicalAuditLog` mínimo para eventos `MedicalRecord.Created` e `MedicalRecord.Updated`, com metadata mínima e sem payload clínico sensível completo.
- **Soft Delete:** a entidade possui exclusão lógica por `IsDeleted`, `DeletedAt` e `DeletedByUserId`; os fluxos clínicos padrão ignoram registros deletados.
- **Retenção:** a política inicial adotada é retenção clínica indefinida e conservadora, sem expurgo automático, sem worker, sem scheduler e sem rotina operacional de arquivamento.
- **Cascades:** relações clínicas críticas foram revisadas para reduzir risco de exclusão indireta destrutiva por cascade.
- **Unicidade:** `MedicalRecords.PatientId` possui unicidade física, preservando o desenho atual de um prontuário por paciente; Soft Delete não libera automaticamente novo prontuário para o mesmo `PatientId`.
- **Conflito concorrente:** a violação física específica da constraint única de `PatientId` é tratada como conflito de negócio, evitando mascarar erros de banco não relacionados.
- **Validação de `FlagsJson`:** `FlagsJson` continua opcional, mas novos fluxos de criação e atualização exigem JSON válido com objeto na raiz quando o campo é informado.
- **Riscos ainda operacionais/documentais:** permanecem abertos a propagação de `CancellationToken` até o repository/EF Core, a formalização versionada de evidências manuais Swagger/HTTP e a decisão sobre o contrato `MedicalRecordListItemResponse` ainda não utilizado.

## 5. Análise de MR-DEBT-010 — CancellationToken

### 5.1 Risco

`MR-DEBT-010 — CancellationToken não propagado no repository` representa uma lacuna de resiliência operacional:

- controllers e use cases já podem receber `CancellationToken` nos fluxos HTTP e de aplicação;
- validators e serviços auxiliares podem propagar tokens próprios em seus métodos assíncronos;
- se o repository não recebe e não repassa o token para EF Core, operações de banco não respeitam integralmente o cancelamento da request;
- isso reduz a resiliência operacional da aplicação sob timeouts, desconexões do cliente, encerramento de request ou pressão de recursos;
- em ambiente real, requests canceladas podem continuar consumindo conexão, CPU, IO e locks no banco até a conclusão natural da operação;
- não é bloqueio de MVP, mas é melhoria operacional importante para maturidade de produção.

A análise dos arquivos técnicos atuais mostra que `IMedicalRecordRepository` expõe métodos assíncronos sem parâmetro `CancellationToken`, enquanto a implementação EF Core chama `FirstOrDefaultAsync`, `AnyAsync`, `AddAsync` e `SaveChangesAsync` sem token. Os use cases recebem `CancellationToken`, mas chamadas como `GetByPatientIdAsync`, `AddAsync` e `UpdateAsync` ainda não repassam esse token ao repository.

### 5.2 Planejamento de solução futura

Uma fase futura deve tratar o débito sem alterar regra de negócio:

- adicionar `CancellationToken` aos métodos relevantes de `IMedicalRecordRepository`;
- propagar o token para operações EF Core como `FirstOrDefaultAsync`, `AnyAsync`, `AddAsync`, `SaveChangesAsync` e `ToListAsync`, se houver listagem futura;
- ajustar `MedicalRecordRepository` para receber e encaminhar o token;
- ajustar fakes/doubles de testes que implementem `IMedicalRecordRepository`;
- ajustar use cases para repassar o `CancellationToken` recebido;
- ajustar validators caso dependam do repository e ainda não repassem token até a camada de persistência;
- ajustar testes de Application, Infrastructure e API afetados por assinatura;
- evitar qualquer mudança semântica de negócio, status HTTP, validação clínica, autorização ou payload;
- manter compatibilidade funcional com os fluxos existentes de GET, POST, PUT e Soft Delete interno/controlado.

### 5.3 Critérios futuros para resolver MR-DEBT-010

`MR-DEBT-010` poderá ser considerado resolvido quando:

- todos os métodos relevantes de `IMedicalRecordRepository` receberem `CancellationToken`;
- use cases repassarem o token recebido para o repository;
- validators/fakes/doubles relacionados forem atualizados de forma consistente;
- EF Core receber o token nas operações async de consulta e persistência;
- `SaveChangesAsync` receber token nos fluxos de criação e atualização;
- build e testes automatizados passarem;
- documentação da fase e registro vivo forem atualizados;
- a alteração permanecer operacional, sem mudança de regra de negócio.

## 6. Análise de MR-DEBT-011 — Evidências Swagger

### 6.1 Risco

`MR-DEBT-011 — Evidências manuais Swagger não versionadas formalmente` é um débito de QA/governança:

- evidência manual pode existir de forma informal em prints, observações locais ou validações visuais não versionadas;
- prints, payloads e anotações manuais não versionados se perdem facilmente;
- a rastreabilidade de QA manual fica menor;
- torna-se difícil provar comportamento esperado da API fora dos testes automatizados;
- evidências versionadas ajudam portfólio técnico, revisão futura, auditoria de decisões e governança incremental da vertical;
- evidência manual não substitui teste automatizado e deve ser descrita como material complementar.

### 6.2 Planejamento de solução futura

Uma fase futura deve criar um pacote documental versionado de evidências manuais:

- criar documento ou subpasta apropriada em `docs/clinical-core` para evidências Swagger/HTTP de `MedicalRecord`;
- documentar cenários verificados manualmente via Swagger ou requisições HTTP equivalentes;
- incluir exemplos sanitizados de request/response;
- não incluir dados reais de pacientes, tutores, usuários ou profissionais;
- não incluir tokens reais, refresh tokens, secrets, cookies ou headers sensíveis;
- não incluir payload clínico sensível real;
- referenciar endpoints cobertos e status HTTP esperados;
- diferenciar explicitamente evidência manual de teste automatizado;
- registrar limitações do ambiente e da evidência;
- atualizar o registro vivo ao concluir o pacote.

### 6.3 Cenários mínimos sugeridos

Cenários mínimos sugeridos para evidência manual versionada:

- GET prontuário existente;
- GET sem prontuário;
- POST válido;
- POST com `FlagsJson` inválido;
- POST duplicado;
- PUT válido;
- PUT com `FlagsJson` inválido;
- bloqueio por perfil sem permissão;
- 401 sem token;
- 403 sem profile/sem permissão.

### 6.4 Critérios futuros para resolver MR-DEBT-011

`MR-DEBT-011` poderá ser considerado resolvido quando:

- documento ou pacote de evidência manual for criado e versionado;
- dados usados nos exemplos estiverem sanitizados;
- tokens, secrets e payload clínico sensível real estiverem ausentes;
- endpoints e cenários mínimos forem cobertos ou limitações forem justificadas;
- a documentação deixar claro que se trata de evidência manual complementar;
- limitações do ambiente forem documentadas;
- o registro vivo for atualizado.

## 7. Análise de MR-DEBT-012 — MedicalRecordListItemResponse

### 7.1 Risco

`MR-DEBT-012 — MedicalRecordListItemResponse ainda não usado` representa ruído técnico e possível risco de privacidade futura:

- o contract `MedicalRecordListItemResponse` existe, mas não é usado pelos endpoints atuais;
- o contract pode indicar intenção anterior ou futura de endpoint de listagem;
- listagem de prontuários é sensível do ponto de vista de privacidade e minimização de dados;
- expor listagem sem desenho específico pode revelar existência de prontuários, associação com pacientes, timestamps e indícios clínicos;
- manter contrato morto também gera ruído técnico e pode induzir implementação futura sem decisão explícita.

A análise técnica atual confirma que há `MedicalRecordResponse` para retornos detalhados de GET/POST/PUT e que o controller atual não expõe endpoint de listagem. `MedicalRecordListItemResponse` contém identificador, `PatientId`, `UpdatedAt` e flags booleanas de presença de conteúdo, mas permanece sem uso.

### 7.2 Planejamento de decisão futura

A decisão futura deve escolher explicitamente uma das opções abaixo.

#### Opção A — Remover `MedicalRecordListItemResponse`

Remover o contract se não houver endpoint de listagem planejado:

- reduz ruído técnico;
- evita contrato morto;
- reduz risco de superfície futura indevida;
- exige ajuste de referências, se alguma surgir até a fase de decisão;
- pode exigir teste simples de build para confirmar ausência de uso.

#### Opção B — Manter como contrato reservado documentado

Manter o contract como reservado para listagem futura segura:

- preserva a intenção de minimização de dados para eventual listagem;
- exige documentação clara do motivo de permanência;
- não deve criar endpoint automaticamente;
- deve deixar explícito que o contract reservado não autoriza exposição de lista sem desenho de produto, autorização e privacidade.

#### Opção C — Implementar endpoint de listagem segura

Implementar endpoint de listagem segura somente se houver decisão explícita de produto e arquitetura:

- maior impacto técnico e de privacidade;
- precisa de autorização específica;
- exige paginação;
- exige filtros e limites;
- exige minimização rigorosa de dados;
- exige testes Application/Infrastructure/API;
- exige análise de privacidade e comportamento de Soft Delete;
- provavelmente permanece fora do escopo imediato da Fase 6.6, salvo decisão explícita.

### 7.3 Recomendação inicial

A recomendação inicial desta fase documental é:

- não implementar endpoint de listagem automaticamente;
- na Fase 6.6.1, apenas planejar a decisão;
- em fase posterior, decidir entre remover o contrato morto ou documentá-lo como reservado;
- se houver listagem futura, ela deve ser altamente minimizada, paginada, autorizada e justificada por decisão de produto/privacidade.

### 7.4 Critérios futuros para resolver MR-DEBT-012

`MR-DEBT-012` poderá ser considerado resolvido quando:

- houver decisão explícita sobre remover, manter documentado ou evoluir para listagem segura;
- o contrato for removido ou justificado/documentado como reservado;
- nenhum endpoint sensível for criado sem política de autorização, minimização e paginação;
- testes forem ajustados se houver remoção ou implementação;
- documentação da fase e registro vivo forem atualizados.

## 8. Sequenciamento recomendado da Fase 6.6

Sequenciamento recomendado:

```text
6.6.1 — Planejamento técnico de qualidade operacional MedicalRecord
6.6.2 — Propagação de CancellationToken no repository MedicalRecord
6.6.3 — Evidências manuais versionadas de API/Swagger MedicalRecord
6.6.4 — Decisão sobre MedicalRecordListItemResponse
6.6.5 — Evidências finais, atualização do registro vivo e encerramento da Fase 6
```

A recomendação é manter 6.6.3 e 6.6.4 separadas, porque têm natureza diferente: 6.6.3 é documentação/evidência manual de comportamento já existente, enquanto 6.6.4 é decisão de governança sobre contrato não utilizado e eventual superfície futura de API. Elas poderiam ser consolidadas em uma fase documental única apenas se a equipe quiser reduzir overhead de PRs, desde que a PR resultante continue pequena, sem implementação de endpoint e sem misturar decisão de contrato com mudanças operacionais de `CancellationToken`.

## 9. Impactos técnicos previstos

Impactos futuros possíveis, conforme a decisão de cada subfase:

- `IMedicalRecordRepository`, especialmente para assinatura com `CancellationToken`;
- `MedicalRecordRepository`, especialmente para repasse de token ao EF Core;
- fakes/doubles de testes que implementem o repository;
- use cases de criação, consulta, atualização e Soft Delete interno/controlado;
- validators que dependam de consultas do repository;
- controller, se for necessário alinhar repasse de token ou documentar comportamento;
- testes Application;
- testes Infrastructure;
- testes API;
- documentação clínica em `docs/clinical-core`;
- registro vivo de débitos técnicos;
- eventual decisão sobre manter ou remover `MedicalRecordListItemResponse`.

## 10. Riscos e cuidados

Cuidados obrigatórios para as próximas subfases:

- não transformar melhoria operacional de `CancellationToken` em mudança de regra de negócio;
- não alterar status HTTP, autorização, payloads ou validações clínicas ao tratar `CancellationToken`;
- não criar endpoint de listagem sensível sem decisão explícita;
- não versionar evidências manuais com dados reais;
- não incluir tokens, secrets, cookies ou headers sensíveis;
- não expor payload clínico sensível real;
- não misturar escopos de `CancellationToken`, Swagger e listagem em uma única PR grande;
- não tratar evidência manual como substituta de teste automatizado;
- não atualizar o registro vivo como resolvido antes de cada débito ter evidência própria.

## 11. Fora do escopo da Fase 6.6.1

Esta fase não implementa:

- código;
- testes;
- migration;
- endpoint;
- listagem;
- Swagger evidence package;
- documentação Swagger versionada final;
- alteração de DTO;
- alteração de contract;
- alteração de repository;
- alteração de interface;
- alteração de use case;
- alteração de controller;
- alteração de validator;
- alteração de `AppDbContext`;
- alteração de configuração EF;
- alteração de frontend;
- alteração de Docker, Redis, RabbitMQ ou Kubernetes;
- alteração de pipeline/CI.

## 12. Critérios de aceite da Fase 6.6.1

A Fase 6.6.1 será considerada concluída se:

- este documento for criado em `docs/clinical-core/PHASE_06_06_01_MEDICAL_RECORD_OPERATIONAL_QUALITY_PLANNING.md`;
- `MR-DEBT-010`, `MR-DEBT-011` e `MR-DEBT-012` forem explicitamente referenciados;
- o estado pós-Fase 6.5 for documentado;
- o risco de cada débito for explicado;
- a estratégia futura para cada débito for definida;
- o sequenciamento da Fase 6.6 for definido;
- critérios futuros de resolução forem registrados;
- riscos e fora do escopo forem documentados;
- a próxima fase recomendada for definida;
- nenhuma implementação for feita;
- nenhuma migration for criada;
- somente `docs/clinical-core` for alterado;
- `git diff --check` passar.

## 13. Próxima fase recomendada

Próxima fase recomendada:

```text
6.6.2 — Propagação de CancellationToken no repository MedicalRecord
```

A 6.6.2 deve ser pequena e focada em resiliência operacional: propagar `CancellationToken` da Application até o repository/EF Core, ajustar fakes e testes impactados, sem alterar semântica clínica ou criar novos endpoints.
