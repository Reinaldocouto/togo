# Fase 7.0 — Abertura e planejamento técnico da expansão clínica e operacional pós-hardening MedicalRecord

## 1. Objetivo

A Fase 7.0 abre a macrofase de expansão clínica e operacional pós-hardening da vertical `MedicalRecord` no projeto TOGO.

Esta fase é exclusivamente documental e estratégica. Seu objetivo é transformar o encerramento técnico da Fase 6 em um plano incremental para evoluir o núcleo clínico sem reabrir débitos já tratados, sem implementar código novo e sem antecipar decisões de schema, endpoints, infraestrutura ou frontend.

A Fase 7 parte da premissa de que `MedicalRecord` se tornou uma vertical tecnicamente mais madura e deve funcionar como base segura para novas capacidades clínicas e operacionais.

## 2. Contexto pós-Fase 6

A Fase 6 encerrou a trilha de hardening técnico da vertical `MedicalRecord`. O documento de encerramento consultado foi `docs/clinical-core/PHASE_06_06_05_MEDICAL_RECORD_PHASE_06_CLOSURE.md`, e o registro vivo de débitos consultado foi `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`.

A Fase 6 consolidou tecnicamente a vertical em:

- autorização granular mínima por operação;
- autoria clínica mínima com usuário e timestamps;
- `ClinicalAuditLog` mínimo para criação e atualização;
- Soft Delete clínico mínimo;
- política inicial conservadora de retenção;
- revisão de cascades em relações clínicas críticas;
- unicidade física de `MedicalRecords.PatientId`;
- tratamento de conflito concorrente de unicidade;
- validação estrutural de `FlagsJson`;
- propagação de `CancellationToken` até persistência EF Core;
- evidências manuais versionadas e sanitizadas;
- decisão formal de manter `MedicalRecordListItemResponse` como contrato reservado, sem endpoint amplo de listagem.

O encerramento da Fase 6 é técnico e documental. Ele não significa liberação irrestrita para produção real com dados clínicos sensíveis, nem substitui validações futuras de segurança, compliance, infraestrutura, produto e operação.

A Fase 7 deve, portanto, partir desse estado mais seguro e usar as garantias consolidadas de `MedicalRecord` como referência para novas verticais clínicas.

## 3. Premissas da Fase 7

- `MedicalRecord` é a base clínica consolidada e não deve ser inflado com responsabilidades de atendimento, prescrição, exames, vacinas, agenda, estoque ou financeiro.
- `Patient` continua sendo o centro clínico do histórico do paciente.
- `Attendance` deve ser tratado como evento/caso clínico independente, associado ao paciente e potencialmente usado como eixo operacional para evolução clínica, prescrição e futuras solicitações.
- Evolução clínica, prescrição, exames e vacinas devem se integrar ao histórico do paciente sem transformar `MedicalRecord` em agregado único e excessivamente acoplado.
- Listagens amplas de prontuário continuam sensíveis e devem permanecer fora de implementação até haver planejamento específico de autorização, minimização, auditoria e paginação segura.
- Dados clínicos devem ser minimizados nas responses, especialmente em listagens e endpoints operacionais.
- Novas features clínicas devem respeitar autoria, auditoria, privacidade, retenção, integridade referencial e autorização granular desde o planejamento.
- Novas demandas não devem reabrir `MR-DEBT-001` a `MR-DEBT-012` sem justificativa explícita; devem ser classificadas como evolução de produto, evolução clínica, evolução operacional, nova vertical, novo débito técnico, melhoria de segurança, melhoria de compliance, melhoria de infraestrutura ou melhoria de experiência do usuário.
- O padrão de cascades restritivos adotado nas relações clínicas críticas deve orientar novas entidades clínicas.
- O padrão de evidência documental e manual versionada deve continuar sendo usado antes de declarar uma entrega clínica encerrada.

## 4. O que a Fase 7 não deve fazer

A Fase 7 não deve:

- reabrir débitos da Fase 6 sem justificativa formal e rastreável;
- criar endpoint sensível sem política de autorização clara;
- implementar listagem ampla de prontuários sem planejamento específico;
- misturar várias verticais clínicas e operacionais em uma única PR grande;
- criar migrations sem decisão clara de domínio, schema, relações, retenção e cascades;
- implementar frontend antes de estabilizar contratos e regras de autorização;
- criar automações de infraestrutura prematuramente;
- acoplar prescrição clínica a estoque sem decisão explícita;
- transformar `MedicalRecord` em depósito genérico de todas as informações clínicas;
- declarar produção real com dados sensíveis sem validações macro posteriores.

## 5. Trilhas candidatas para a Fase 7

A inspeção técnica obrigatória identificou que já existem entidades de domínio para `Patient`, `MedicalRecord`, `Attendance`, `ClinicalEvolution`, `Prescription` e `PrescriptionItem`. Também existem controller e contratos de aplicação para `Attendance` e `MedicalRecord`. Não foram encontradas classes correspondentes a `Vaccine`, `Appointment`, `Product`, `Stock` ou `Financial` na busca obrigatória em `backend/src`.

Também foram consultados os diretórios e documentos disponíveis. O arquivo `docs/ROADMAP.md` não foi encontrado; existe `docs/ROADMAP_TO_PHASE_12.md`. Os diretórios `docs/architecture`, `docs/clinical-core`, `backend/src/Togo.Domain/Entities`, `backend/src/Togo.Application`, `backend/src/Togo.Api/Controllers` e `backend/src/Togo.Infrastructure/Persistence` existem.

### Trilha A — Atendimento clínico

Possível foco:

- consolidar `Attendance` como evento/caso clínico independente;
- alinhar atendimento ao paciente;
- definir ciclo de vida mínimo do atendimento;
- vincular evolução clínica e prescrição ao atendimento;
- garantir autoria, timestamps, status e rastreabilidade;
- definir permissões por operação;
- avaliar se o modelo atual de `Attendance` precisa de hardening semelhante ao recebido por `MedicalRecord`.

Diagnóstico inicial: é a trilha mais natural para abrir a Fase 7 porque `Attendance` já existe no domínio, na aplicação, na infraestrutura e na API. Além disso, `ClinicalEvolution` e `Prescription` já se vinculam a `Attendance`, indicando que o atendimento deve ser o eixo operacional da próxima expansão.

### Trilha B — Evoluções clínicas

Possível foco:

- evolução clínica como registro incremental;
- vínculo com atendimento;
- histórico por paciente derivado do vínculo com atendimento;
- minimização em responses;
- autoria e auditoria;
- regras para edição, retificação ou bloqueio de alterações após fechamento do atendimento.

Diagnóstico inicial: a entidade `ClinicalEvolution` existe, mas deve ser planejada depois do ciclo de atendimento, pois depende da semântica de abertura, fechamento, cancelamento, autoria e permissões do `Attendance`.

### Trilha C — Prescrições

Possível foco:

- prescrição clínica vinculada a atendimento e paciente;
- medicamentos, dose, frequência, quantidade, unidade e duração;
- segurança de dados e minimização;
- autoria e auditoria;
- separação entre prescrição clínica e estoque;
- política clara para item com `ProductId` opcional.

Diagnóstico inicial: `Prescription` e `PrescriptionItem` já existem no domínio e indicam vínculo com `Attendance`. A trilha deve vir depois do planejamento de atendimento e, preferencialmente, depois da evolução clínica mínima, para evitar acoplamento indevido entre conduta, texto clínico e produto/estoque.

### Trilha D — Exames e solicitações

Possível foco:

- solicitação de exame;
- resultado de exame;
- vínculo com atendimento;
- anexos futuros;
- armazenamento seguro de arquivos;
- proteção de dados sensíveis;
- auditoria de acesso e minimização de metadados.

Diagnóstico inicial: não foi identificada entidade de exame na busca obrigatória. A trilha deve permanecer como planejamento posterior, pois provavelmente exigirá novas entidades, decisões de schema e, no futuro, política específica para anexos.

### Trilha E — Vacinas

Possível foco:

- histórico vacinal;
- pendências;
- reforços;
- calendário;
- alertas futuros;
- vínculo com paciente e/ou atendimento.

Diagnóstico inicial: não foi identificada entidade `Vaccine` na busca obrigatória. A trilha tem valor clínico, mas deve ser posterior à estabilização do atendimento e à definição do padrão de novas entidades clínicas auditáveis.

### Trilha F — Agenda / operação clínica

Possível foco:

- agendamento;
- fila de atendimento;
- check-in;
- status operacional;
- vínculo com atendimento clínico;
- transição de agendamento para atendimento.

Diagnóstico inicial: não foi identificada entidade `Appointment` na busca obrigatória. A trilha é importante para operação clínica, mas deve ser separada do núcleo clínico inicial para evitar misturar agenda, atendimento e prontuário na mesma entrega.

### Trilha G — Evidências e documentação operacional

Possível foco:

- Swagger executado real;
- coleções Postman/Insomnia;
- documentação de API;
- cenários manuais versionados;
- matriz de autorização por endpoint;
- critérios de encerramento operacional por vertical.

Diagnóstico inicial: esta trilha deve acompanhar as entregas executáveis da Fase 7, especialmente ao final da primeira entrega clínica operacional. Ela não substitui implementação, mas reduz risco de regressão documental e operacional.

Nenhuma dessas trilhas deve ser implementada na Fase 7.0. Esta fase apenas analisa, prioriza e recomenda sequência.

## 6. Critérios de priorização

As trilhas da Fase 7 devem ser priorizadas pelos seguintes critérios:

- valor clínico direto para registrar ou consultar o cuidado prestado;
- dependência técnica entre entidades e fluxos;
- risco de privacidade e sensibilidade dos dados expostos;
- complexidade de schema, migrations e relações;
- impacto em autorização granular;
- necessidade de autoria, auditoria e retenção;
- proximidade com MVP operacional útil;
- facilidade de teste automatizado e validação manual;
- alinhamento com o portfólio técnico já existente;
- capacidade de entrega incremental em PRs pequenas;
- risco de acoplamento entre clínica, estoque, financeiro e operação;
- maturidade documental existente em fases anteriores.

Aplicando esses critérios ao estado atual do repositório, `Attendance` deve ser tratado antes de evoluções clínicas e prescrições, porque já existe como entidade e fluxo de API e é o vínculo natural para `ClinicalEvolution` e `Prescription`.

## 7. Recomendação de sequência

Sequência inicial recomendada:

```text
7.0 — Abertura e planejamento técnico da Fase 7
7.1 — Planejamento técnico da vertical Attendance pós-MedicalRecord
7.2 — Implementação mínima do ciclo de atendimento clínico
7.3 — Evoluções clínicas vinculadas ao atendimento
7.4 — Prescrições clínicas vinculadas ao atendimento
7.5 — Evidências, documentação e encerramento da primeira entrega clínica operacional
```

Justificativa: a inspeção do repositório confirma que `Attendance` já existe em domínio, aplicação, infraestrutura e API, enquanto `ClinicalEvolution` e `Prescription` dependem dele como eixo de vínculo. Portanto, é mais seguro planejar e endurecer o ciclo de atendimento antes de implementar evoluções, prescrições, exames, vacinas, agenda ou integrações operacionais mais amplas.

Trilhas de exames, vacinas, agenda, estoque, financeiro, frontend e infraestrutura devem permanecer fora da primeira sequência executável até que o contrato clínico-operacional mínimo esteja estabilizado.

## 8. Próxima subfase recomendada

Próxima subfase executável recomendada:

```text
Fase 7.1 — Planejamento técnico da vertical Attendance pós-hardening MedicalRecord
```

Objetivo:

```text
Definir o ciclo de vida mínimo de Atendimento, seus vínculos com Patient, MedicalRecord, ClinicalEvolution e Prescription, sem implementar código ainda.
```

A Fase 7.1 deve revisar o estado atual de `Attendance`, identificar lacunas frente ao padrão de `MedicalRecord`, propor política de autorização, autoria, auditoria, retenção, minimização de responses e regras de transição de status, mas ainda sem implementar código, migrations ou endpoints novos.

## 9. Riscos da Fase 7

- Expansão clínica sem autorização adequada.
- Endpoints com excesso de dados clínicos em responses.
- Mistura indevida de atendimento, prontuário, agenda, estoque e financeiro.
- Acoplamento indevido entre prescrição clínica e estoque/produtos.
- Migrations prematuras antes de decisão clara de domínio.
- Auditoria insuficiente em novas entidades clínicas.
- Autoria inconsistente entre `MedicalRecord`, `Attendance`, `ClinicalEvolution` e `Prescription`.
- Crescimento de escopo em PRs grandes e difíceis de revisar.
- Frontend iniciado antes de contrato estável.
- Reabertura informal de débitos encerrados da Fase 6.
- Listagens sensíveis criadas sem minimização, paginação, auditoria e autorização.
- Anexos clínicos futuros tratados sem política de armazenamento seguro.
- Uso de dados clínicos reais em evidências manuais ou documentação.

## 10. Fora do escopo da Fase 7.0

Esta fase não implementa:

- código;
- testes;
- migrations;
- endpoints;
- frontend;
- novas entidades;
- novas tabelas;
- novas permissões;
- integrações;
- infraestrutura;
- alterações em banco;
- alterações em `AppDbContext`;
- alterações em controllers;
- alterações em use cases;
- alterações em repositories;
- alterações em contracts/DTOs;
- alterações em domain entities;
- alterações em autorização, JWT, Docker, Redis, RabbitMQ ou Kubernetes.

## 11. Critérios de aceite da Fase 7.0

A Fase 7.0 será considerada concluída se:

- o documento da Fase 7.0 for criado;
- o estado pós-Fase 6 for resumido;
- as fontes obrigatórias da Fase 6 forem consultadas;
- as entidades e fluxos clínicos existentes forem inspecionados;
- as trilhas candidatas forem mapeadas;
- os riscos forem registrados;
- os critérios de priorização forem definidos;
- a sequência recomendada da Fase 7 for proposta;
- a próxima subfase for recomendada;
- nenhuma implementação for feita;
- nenhuma migration for criada;
- somente documentação for alterada;
- `git diff --check` passar.

## 12. Decisão final

A Fase 7 será iniciada como expansão clínica e operacional pós-hardening `MedicalRecord`, priorizando planejamento incremental antes de implementação e evitando reabrir débitos encerrados da Fase 6 sem justificativa formal.

A primeira direção recomendada é planejar a vertical `Attendance` como eixo de atendimento clínico, preservando `Patient` como centro do histórico e `MedicalRecord` como base clínica consolidada, sem inflar o prontuário e sem antecipar prescrições, evoluções, exames, vacinas, agenda, frontend ou infraestrutura.
