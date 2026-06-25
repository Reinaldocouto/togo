# Fase 8.0 — Planejamento da segurança contextual, escopo clínico e governança multiunidade

## 1. Objetivo da Fase 8

A Fase 8 tem como objetivo introduzir segurança contextual no TOGO, garantindo que dados clínicos, administrativos e operacionais sejam acessados, criados, atualizados, listados e auditados somente dentro do contexto correto de organização, clínica e unidade.

A entrega deve evoluir a base clínica já existente sem descaracterizar a Clean Architecture do projeto. O foco é preparar o sistema para operação multiunidade com isolamento de dados, rastreabilidade, autorização contextual e guardrails técnicos contra bypass de escopo.

A Fase 8 deve ser implementada em partes pequenas, revisáveis e testáveis, evitando uma solução genérica demais ou um modelo SaaS completo antes da validação incremental do domínio.

## 2. Problema que a fase resolve

Nas fases anteriores, o sistema consolidou o núcleo clínico com entidades e fluxos como tutor, paciente/pet, atendimento, prontuário, evolução clínica, prescrição, auditoria mínima e policies por perfil/permissão. O risco remanescente é que a autorização ainda está centrada em identidade e permissões gerais, sem garantir de forma consistente que o recurso acessado pertence ao mesmo escopo clínico ativo da requisição.

Sem uma camada explícita de escopo clínico, um usuário autenticado e autorizado por policy poderia, por erro de implementação ou falha de validação, acessar registros de outra clínica, unidade ou organização usando identificadores válidos de recursos alheios. Esse problema é especialmente sensível em dados veterinários clínicos, pois atendimentos, prontuários, prescrições, evoluções e auditorias podem conter informações privadas de tutores, pacientes e operações internas.

A Fase 8 resolve esse problema por meio de:

- modelagem mínima de organização, clínica e unidade;
- persistência do escopo clínico nas entidades relevantes;
- propagação de `ClinicId` ou identificador equivalente nos fluxos clínicos;
- criação de um contexto clínico ativo por requisição;
- autorização contextual além de perfil/permissão;
- filtros de consulta baseados no contexto ativo;
- auditoria contextual enriquecida;
- registro de leituras sensíveis e tentativas negadas;
- guardrails automatizados para reduzir o risco de novas rotas ou consultas sem escopo.

## 3. Riscos mitigados

| Risco | Impacto | Mitigação esperada na Fase 8 |
| --- | --- | --- |
| Acesso cruzado entre clínicas | Exposição indevida de dados clínicos ou administrativos. | Validação de escopo em use cases, repositories e authorization handlers. |
| Listagens sem filtro contextual | Vazamento massivo de registros de outras unidades. | Filtros por contexto ativo e testes de isolamento. |
| Escrita de registros em clínica incorreta | Inconsistência assistencial e administrativa. | Propagação controlada de `ClinicId` a partir do contexto ativo. |
| Auditoria sem contexto | Dificuldade de investigação e rastreabilidade. | Enriquecimento do AuditLog com organização, clínica, unidade e origem do acesso. |
| Bypass por acesso direto ao repository | Regressões futuras por consultas sem escopo. | Guardrails técnicos e padrões explícitos de repository/use case. |
| Uso indevido de permissões globais | Usuário com perfil válido acessa recurso fora do seu vínculo. | Autorização contextual combinando permissão e escopo. |
| Quebra de contratos públicos | Clientes atuais da API deixam de funcionar sem necessidade. | Evolução incremental, preferindo defaults seguros e versionamento quando necessário. |
| Acoplamento de regra de negócio na API | Regras ficam difíceis de testar e reutilizar. | API apenas resolve contexto HTTP e delega decisões à Application/Domain. |
| Auditoria com dados sensíveis demais | Logs passam a vazar conteúdo clínico. | Metadados mínimos, sem copiar conteúdo clínico integral. |
| Solução multi-tenant excessivamente genérica | Complexidade prematura e alto custo de manutenção. | Modelo mínimo suficiente para isolamento clínico atual. |

## 4. Conceitos centrais

### Organization

Representa o agrupador administrativo superior. Uma organização pode possuir uma ou mais clínicas. Nesta fase, a organização deve ser modelada de forma mínima para sustentar governança, segregação futura e auditoria, sem tentar resolver billing, contratos comerciais, SaaS completo ou hierarquias corporativas complexas.

### Clinic

Representa o principal limite de isolamento clínico-operacional da Fase 8. A clínica é o escopo primário para filtrar dados como pacientes, tutores, atendimentos, prontuários, evoluções, prescrições e logs clínicos. Quando houver dúvida entre organização, clínica e unidade, a decisão inicial recomendada é tratar `ClinicId` como escopo obrigatório para fluxos clínicos sensíveis.

### ClinicUnit

Representa uma filial, local físico, sala operacional ou subdivisão da clínica. A unidade deve ser planejada como escopo complementar. A implementação inicial pode permitir `UnitId` opcional em alguns fluxos, desde que `ClinicId` permaneça obrigatório para isolamento. Regras mais avançadas por unidade podem ser evoluídas após a consolidação do filtro por clínica.

### User

Representa a identidade autenticada que executa ações no sistema. O usuário já possui vínculo com autenticação/autorização e deve passar a ser relacionado ao contexto clínico permitido. O planejamento deve prever como validar se o usuário pode atuar em uma clínica/unidade, sem embutir essa regra diretamente nos controllers.

### Profile/Permission

Representa o que o usuário pode fazer funcionalmente, por exemplo ler prontuário, criar atendimento ou emitir prescrição. Na Fase 8, perfil/permissão deixa de ser suficiente isoladamente: uma ação só deve ser permitida quando a permissão funcional e o escopo clínico forem válidos simultaneamente.

### UserClinicAccess

Representa o vínculo autorizativo entre `User` e os escopos clínicos nos quais ele pode atuar. Deve permitir validar, no backend, se o usuário autenticado possui acesso à `Clinic` e, quando aplicável, à `ClinicUnit` selecionada. Esse vínculo não substitui `Profile/Permission`: ele limita territorialmente permissões funcionais já concedidas.

### Clinical Scope

É o conjunto mínimo de identificadores que delimita onde uma operação pode ocorrer. A proposta inicial é que o escopo clínico contenha pelo menos `OrganizationId`, `ClinicId` e, quando aplicável, `UnitId`. O escopo deve ser persistido nos registros sensíveis e usado em criação, consulta, atualização, listagem e auditoria.

### Active Clinical Context

É a representação do escopo clínico selecionado para a requisição atual. Deve ser resolvido na borda HTTP, por header, claim, rota ou combinação controlada, e exposto à Application por uma abstração como `ICurrentClinicalContext`. O contexto ativo não deve carregar regra de negócio de autorização; ele deve informar a identidade do escopo selecionado para que use cases e serviços de autorização contextual validem a operação.

### Contextual Audit

É a evolução do AuditLog clínico para registrar não apenas usuário, perfil, ação e entidade, mas também organização, clínica, unidade, resultado da decisão, origem da requisição e metadados mínimos de correlação. A auditoria contextual deve preservar minimização de dados e não deve copiar conteúdo clínico integral para logs.

### Regras explícitas de segurança contextual

- Qualquer `ClinicId`, `OrganizationId` ou `ClinicUnitId` recebido por header, claim, rota, query string ou corpo da requisição deve ser validado contra `UserClinicAccess` antes de autorizar operação clínica sensível.
- O backend não deve confiar em contexto informado pelo cliente sem validação server-side. Headers e payloads são apenas candidatos a contexto, não evidência suficiente de autorização.
- Queries clínicas sensíveis não devem possuir listagens globais sem decisão técnica explícita, justificativa documentada e controles compensatórios.
- Identificadores de recursos clínicos recebidos por rota devem ser resolvidos junto com o escopo ativo; consultar por ID sem validar `ClinicId` equivale a risco de acesso cruzado.

## 5. Divisão da Fase 8 em subfases

A Fase 8 deve ser dividida em subfases pequenas para reduzir risco de regressão, facilitar review e permitir validação incremental:

1. **8.0 — Planejamento da segurança contextual**: documento técnico, escopo, riscos e critérios de aceite.
2. **8.0.1 — Refinamento documental do planejamento da Fase 8**: revisão técnica do documento, remoção de linguagem conversacional, formalização de conceitos e delimitação de decisões pendentes.
3. **8.1 — Modelo mínimo de Organização/Clínica/Unidade**: entidades, invariantes mínimas e testes de domínio.
4. **8.2 — Persistência do escopo clínico**: mapeamentos EF, migrations e índices.
5. **8.3 — Propagação de ClinicId nos fluxos clínicos**: criação e atualização de fluxos para gravar escopo.
6. **8.4 — CurrentClinicalContext**: abstração de contexto ativo e implementação HTTP.
7. **8.5 — Autorização contextual**: combinação de permissões existentes com validação de escopo.
8. **8.6 — Filtros de consulta por contexto**: repositories e listagens protegidas por escopo.
9. **8.7 — Auditoria contextual enriquecida**: AuditLog com identificadores de escopo.
10. **8.8 — Auditoria de leitura e acesso negado**: eventos para leituras sensíveis e negações.
11. **8.9 — Guardrails contra bypass de escopo**: testes arquiteturais e padrões obrigatórios.
12. **8.10 — Evidências manuais de segurança**: roteiros e evidências de testes manuais.
13. **8.11 — Encerramento técnico da Fase 8**: relatório final, riscos remanescentes e recomendações.

## 6. Subfases detalhadas

### 8.0 — Planejamento da segurança contextual

**Objetivo**

Definir o plano técnico da Fase 8 antes de qualquer implementação, documentando escopo, riscos, decisões iniciais, critérios de aceite e sequência incremental.

**Escopo**

- Documentar problema, objetivo e riscos.
- Definir conceitos de organização, clínica, unidade, usuário, permissão, escopo clínico, contexto ativo e auditoria contextual.
- Propor subfases e limites explícitos de escopo.
- Registrar decisões pendentes.

**Arquivos prováveis impactados**

- `docs/security/PHASE_08_00_CONTEXTUAL_SECURITY_PLANNING.md`.

**Entregáveis esperados**

- Documento técnico de planejamento em português.
- Checklist de aceite da Fase 8.0.
- Indicação da próxima subfase recomendada.

**Critérios de aceite**

- Documento criado em `docs/security`.
- Subfases 8.1 a 8.11 descritas com objetivo, escopo, arquivos prováveis, entregáveis, critérios de aceite e riscos.
- Itens fora de escopo explicitados.
- Arquitetura esperada após a Fase 8 descrita.
- Decisões pendentes registradas.

**Riscos técnicos**

- Planejamento amplo demais para implementação incremental.
- Definições conceituais ambíguas entre clínica e unidade.
- Falta de alinhamento futuro sobre origem do contexto ativo.

### 8.0.1 — Refinamento documental do planejamento da Fase 8

**Objetivo**

Refinar o documento da Fase 8.0 para torná-lo um planejamento técnico completo, neutro e acionável, sem iniciar implementação de sistema.

**Escopo**

- Revisar linguagem para remover formulações conversacionais e substituir por termos técnicos.
- Formalizar conceitos centrais em nomenclatura alinhada ao domínio e à arquitetura esperada.
- Reforçar fora de escopo, riscos mitigados, decisões pendentes e checklist de aceite.
- Detalhar cada subfase de 8.0 a 8.11 com objetivo, escopo, arquivos prováveis, entregáveis, critérios de aceite e riscos técnicos.

**Arquivos prováveis impactados**

- `docs/security/PHASE_08_00_CONTEXTUAL_SECURITY_PLANNING.md`.

**Entregáveis esperados**

- Documento refinado de planejamento técnico.
- Subfase 8.0.1 registrada como ajuste documental.
- Critérios de segurança contextual explicitados para orientar a Fase 8.1.

**Critérios de aceite**

- Nenhum código produtivo é alterado.
- Nenhuma entidade, migration, API, Application, Domain ou Infrastructure é implementada.
- Documento permite iniciar a Fase 8.1 com decisões delimitadas e riscos conhecidos.
- Checklist da Fase 8.0 reflete o refinamento documental.

**Riscos técnicos**

- Documento ficar prescritivo demais e antecipar desenho que deveria ser validado na Fase 8.1.
- Manter decisões críticas implícitas, especialmente sobre escopo inicial e validação de contexto.
- Confundir refinamento documental com implementação antecipada.

### 8.1 — Modelo mínimo de Organização/Clínica/Unidade

**Objetivo**

Criar o modelo de domínio mínimo para representar organização, clínica e unidade, sustentando isolamento clínico sem implementar um SaaS completo.

**Escopo**

- Adicionar entidades de domínio mínimas para `Organization`, `Clinic` e `Unit` ou nomes equivalentes aprovados.
- Definir identificadores, nome, status ativo/inativo e vínculos hierárquicos mínimos.
- Definir invariantes simples, como clínica pertencer a uma organização e unidade pertencer a uma clínica.
- Adicionar testes de domínio para criação e validações mínimas.

**Arquivos prováveis impactados**

- `backend/src/Togo.Domain/Entities/*`.
- `backend/src/Togo.Domain/Enums/*`, se houver status específico.
- `backend/src/Togo.Domain.Tests/*`.
- `docs/security/PHASE_08_01_ORGANIZATION_CLINIC_UNIT_MODEL.md`.

**Entregáveis esperados**

- Entidades mínimas no Domain.
- Testes automatizados de domínio.
- Documento da subfase com decisões e evidências.

**Critérios de aceite**

- Domain continua sem dependência de EF Core, API ou infraestrutura.
- Entidades não expõem setters públicos desnecessários.
- Regras mínimas testadas.
- Nenhum fluxo clínico existente é alterado nesta subfase, salvo necessidade documental.

**Riscos técnicos**

- Criar modelo organizacional complexo demais.
- Confundir unidade operacional com clínica.
- Introduzir dependência de persistência no Domain.

### 8.2 — Persistência do escopo clínico

**Objetivo**

Persistir organização, clínica e unidade, e preparar entidades clínicas para armazenar escopo clínico de forma indexada e consultável.

**Escopo**

- Criar configurações EF para organização, clínica e unidade.
- Adicionar `DbSet` no `AppDbContext`.
- Criar migrations para novas tabelas e colunas de escopo em entidades sensíveis.
- Planejar índices por `ClinicId`, possivelmente compostos com `PatientId`, `AttendanceId`, `TutorId` e datas.
- Definir estratégia segura para dados existentes em ambiente de desenvolvimento/testes.

**Arquivos prováveis impactados**

- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/*`.
- `backend/src/Togo.Infrastructure/Migrations/*`.
- `backend/src/Togo.Infrastructure.Tests/Persistence/*`.
- `docs/security/PHASE_08_02_CLINICAL_SCOPE_PERSISTENCE.md`.

**Entregáveis esperados**

- Mapeamentos EF Core.
- Migration revisável.
- Testes de persistência e/ou snapshot.
- Documento com estratégia de compatibilidade.

**Critérios de aceite**

- Tabelas e colunas criadas com constraints mínimas.
- Índices relevantes definidos.
- Testes de infraestrutura passam.
- Não há cascade delete perigoso entre organização/clínica/unidade e dados clínicos sensíveis.

**Riscos técnicos**

- Migration quebrar dados existentes.
- FKs obrigatórias sem estratégia de backfill.
- Índices insuficientes para listagens futuras.

### 8.3 — Propagação de ClinicId nos fluxos clínicos

**Objetivo**

Garantir que novos registros clínicos sejam criados com escopo clínico consistente, priorizando `ClinicId` como delimitador obrigatório.

**Escopo**

- Atualizar use cases de criação de atendimento, prontuário, evolução clínica e prescrição para receber ou resolver escopo.
- Validar que vínculos derivados, como evolução e prescrição, herdem escopo do atendimento quando aplicável.
- Evitar que a API aceite arbitrariamente `ClinicId` sem validação contextual.
- Atualizar DTOs somente quando necessário e com cuidado para não quebrar contratos públicos sem justificativa.

**Arquivos prováveis impactados**

- `backend/src/Togo.Application/Attendances/UseCases/*`.
- `backend/src/Togo.Application/MedicalRecords/UseCases/*`.
- `backend/src/Togo.Application/ClinicalEvolutions/UseCases/*`.
- `backend/src/Togo.Application/Prescriptions/UseCases/*`.
- `backend/src/Togo.Application/*/Requests/*` e `Responses/*`, se existirem.
- `backend/src/Togo.Domain/Entities/Attendance.cs`.
- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`.
- `backend/src/Togo.Domain/Entities/ClinicalEvolution.cs`.
- `backend/src/Togo.Domain/Entities/Prescription.cs`.
- Testes em `backend/src/Togo.Application.Tests/*`.

**Entregáveis esperados**

- Fluxos de criação gravando escopo.
- Testes unitários de propagação.
- Documento com decisões sobre contratos.

**Critérios de aceite**

- Registros novos não ficam sem `ClinicId` quando o escopo é obrigatório.
- Prescrição e evolução vinculadas a atendimento não podem divergir do escopo do atendimento.
- Contratos públicos preservados ou alterações justificadas.
- Testes cobrem cenários de escopo ausente e escopo inconsistente.

**Riscos técnicos**

- Duplicar lógica de propagação em múltiplos use cases.
- Permitir spoofing de `ClinicId` pelo payload HTTP.
- Quebrar endpoints existentes sem necessidade.

### 8.4 — CurrentClinicalContext

**Objetivo**

Criar uma abstração para representar o contexto clínico ativo da requisição, desacoplando a Application da API e padronizando a resolução de escopo.

**Escopo**

- Criar contrato como `ICurrentClinicalContext` na Application.
- Criar record/DTO como `CurrentClinicalContextInfo` com `OrganizationId`, `ClinicId`, `UnitId` opcional e indicadores de resolução.
- Implementar resolução HTTP na API, possivelmente por claims e/ou headers controlados.
- Registrar serviço no DI.
- Definir exceções para contexto ausente ou inválido.

**Arquivos prováveis impactados**

- `backend/src/Togo.Application/Security/*`.
- `backend/src/Togo.Api/Security/*`.
- `backend/src/Togo.Api/Program.cs`.
- `backend/src/Togo.Api.Tests/Security/*`.
- `backend/src/Togo.Application.Tests/Security/*`.
- `docs/security/PHASE_08_04_CURRENT_CLINICAL_CONTEXT.md`.

**Entregáveis esperados**

- Interface e modelo de contexto na Application.
- Implementação HTTP na API.
- Testes de resolução do contexto.
- Documento com fonte oficial do contexto ativo.

**Critérios de aceite**

- Application não depende de `HttpContext`.
- API não decide regra clínica, apenas resolve dados da requisição.
- Ausência de contexto em endpoint sensível falha de forma segura.
- Logs e erros não expõem dados sensíveis.

**Riscos técnicos**

- Definir fonte de contexto difícil de migrar no futuro.
- Aceitar header manipulável sem validação contra vínculo do usuário.
- Misturar `CurrentUser` e `CurrentClinicalContext` de forma confusa.

### 8.5 — Autorização contextual

**Objetivo**

Combinar permissões funcionais existentes com validações de escopo clínico, impedindo que usuários autorizados genericamente acessem recursos fora de sua clínica/unidade.

**Escopo**

- Definir serviços ou handlers de autorização contextual na Application e/ou API, respeitando responsabilidades de camada.
- Validar vínculo entre usuário e clínica/unidade antes de executar operações sensíveis.
- Atualizar policies existentes para considerar contexto quando necessário.
- Criar exceções/resultados padronizados para acesso negado por escopo.

**Arquivos prováveis impactados**

- `backend/src/Togo.Application/Security/*`.
- `backend/src/Togo.Api/Security/*`.
- `backend/src/Togo.Api/Program.cs`.
- `backend/src/Togo.Application/Attendances/UseCases/*`.
- `backend/src/Togo.Application/MedicalRecords/UseCases/*`.
- `backend/src/Togo.Application/ClinicalEvolutions/UseCases/*`.
- `backend/src/Togo.Application/Prescriptions/UseCases/*`.
- `backend/src/Togo.Api.Tests/Security/*`.
- `backend/src/Togo.Application.Tests/Security/*`.

**Entregáveis esperados**

- Mecanismo de autorização contextual.
- Testes de acesso permitido e negado por escopo.
- Documento com matriz de permissões e contexto.

**Critérios de aceite**

- Acesso com permissão correta, mas escopo incorreto, é negado.
- Acesso sem permissão continua negado.
- Controllers permanecem finos e sem regra de negócio.
- Resultado HTTP de negação é consistente com o padrão atual.

**Riscos técnicos**

- Duplicar autorização em controller e use case.
- Retornar `404` versus `403` sem decisão consistente.
- Criar dependência circular entre authorization handlers e repositories.

### 8.6 — Filtros de consulta por contexto

**Objetivo**

Garantir que consultas e listagens retornem apenas registros pertencentes ao contexto clínico ativo.

**Escopo**

- Atualizar interfaces de repository para aceitar escopo ou aplicar métodos explicitamente escopados.
- Filtrar listagens de atendimentos, prontuários, evoluções e prescrições por `ClinicId`.
- Validar consultas diretas por identificador contra o escopo ativo.
- Criar testes de isolamento com dados de clínicas diferentes.

**Arquivos prováveis impactados**

- `backend/src/Togo.Application/*/Repositories/*`.
- `backend/src/Togo.Infrastructure/Repositories/*`.
- `backend/src/Togo.Application/*/UseCases/*`.
- `backend/src/Togo.Infrastructure.Tests/Repositories/*`.
- `backend/src/Togo.Application.Tests/*`.

**Entregáveis esperados**

- Métodos de consulta escopados.
- Testes garantindo que dados de outra clínica não aparecem.
- Documento com padrão obrigatório para novas consultas.

**Critérios de aceite**

- Listagens globais sensíveis não retornam registros fora do contexto.
- Consulta por ID falha de forma segura quando o recurso pertence a outra clínica.
- Testes cobrem pelo menos dois escopos clínicos distintos.
- Não há filtro apenas em memória quando deve ocorrer no banco.

**Riscos técnicos**

- Métodos legados sem escopo continuarem disponíveis.
- Filtro aplicado tarde demais após materialização.
- Consultas com joins esquecerem escopo de entidades relacionadas.

### 8.7 — Auditoria contextual enriquecida

**Objetivo**

Evoluir a auditoria clínica para registrar contexto de organização, clínica e unidade em eventos relevantes.

**Escopo**

- Atualizar contrato de evento de auditoria clínica.
- Atualizar entidade e persistência de `ClinicalAuditLog`.
- Registrar `OrganizationId`, `ClinicId` e `UnitId` quando disponíveis.
- Manter metadados mínimos e não copiar conteúdo clínico integral.

**Arquivos prováveis impactados**

- `backend/src/Togo.Application/Auditing/*`.
- `backend/src/Togo.Domain/Entities/ClinicalAuditLog.cs`.
- `backend/src/Togo.Infrastructure/Auditing/EfClinicalAuditLogWriter.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalAuditLogConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Migrations/*`.
- `backend/src/Togo.Application.Tests/Auditing/*`.
- `backend/src/Togo.Infrastructure.Tests/Auditing/*`.

**Entregáveis esperados**

- AuditLog enriquecido com contexto.
- Migration e mapeamento EF.
- Testes de escrita de auditoria contextual.
- Documento com campos permitidos e proibidos em auditoria.

**Critérios de aceite**

- Eventos clínicos persistem `ClinicId` quando aplicável.
- Auditoria não armazena conteúdo clínico sensível integral.
- Eventos existentes continuam funcionando ou são migrados com compatibilidade clara.
- Índices de auditoria suportam investigação por clínica e data.

**Riscos técnicos**

- Quebrar testes existentes de auditoria.
- Aumentar excesso de dados em logs.
- Permitir auditoria sem contexto em endpoints sensíveis.

### 8.8 — Auditoria de leitura e acesso negado

**Objetivo**

Registrar leituras sensíveis e tentativas negadas de acesso cruzado, criando evidência para investigação de incidentes e validação de segurança.

**Escopo**

- Definir eventos padronizados para leitura sensível e acesso negado.
- Registrar tentativas de leitura de prontuário, atendimento, evolução ou prescrição fora de escopo.
- Evitar registrar conteúdo clínico no evento de negação.
- Diferenciar falhas por permissão, contexto ausente e escopo divergente quando seguro.

**Arquivos prováveis impactados**

- `backend/src/Togo.Application/Auditing/*`.
- `backend/src/Togo.Application/Security/*`.
- `backend/src/Togo.Api/Security/*`.
- Use cases clínicos sensíveis em `backend/src/Togo.Application/*/UseCases/*`.
- Testes em `backend/src/Togo.Application.Tests/*` e `backend/src/Togo.Api.Tests/*`.

**Entregáveis esperados**

- Eventos de leitura sensível.
- Eventos de acesso negado contextual.
- Testes de auditoria para negações e leituras.
- Documento de evidências.

**Critérios de aceite**

- Tentativas negadas por escopo geram audit log com contexto e usuário quando disponíveis.
- Leituras sensíveis definidas no escopo geram evento de auditoria.
- Nenhum conteúdo clínico sensível é copiado para metadados.
- Falhas de auditoria não mascaram indevidamente decisão de segurança.

**Riscos técnicos**

- Gerar volume excessivo de logs.
- Expor existência de recurso de outra clínica pela resposta HTTP.
- Registrar auditoria duplicada em múltiplas camadas.

### 8.9 — Guardrails contra bypass de escopo

**Objetivo**

Adicionar verificações automatizadas para reduzir o risco de novos fluxos clínicos ignorarem escopo contextual.

**Escopo**

- Criar testes arquiteturais ou de reflexão para controllers clínicos, policies e repositories.
- Detectar rotas globais sensíveis sem escopo quando aplicável.
- Verificar que métodos de repository sensíveis exigem `ClinicId` ou escopo equivalente.
- Documentar padrão obrigatório para novas features clínicas.

**Arquivos prováveis impactados**

- `backend/src/Togo.Api.Tests/*`.
- `backend/src/Togo.Application.Tests/*`.
- `backend/src/Togo.Infrastructure.Tests/*`.
- `docs/security/PHASE_08_09_SCOPE_BYPASS_GUARDRAILS.md`.

**Entregáveis esperados**

- Testes guardrail automatizados.
- Documento de padrões obrigatórios.
- Evidência de execução dos testes.

**Critérios de aceite**

- Guardrails falham quando um controller clínico sensível remove authorization/contexto esperado.
- Guardrails falham quando uma consulta sensível nova ignora escopo obrigatório, na medida detectável por teste.
- Padrão de exceções justificadas é documentado.
- Testes são estáveis e não dependem de ordem de execução.

**Riscos técnicos**

- Guardrails frágeis ou excessivamente acoplados à implementação.
- Falso positivo bloquear refatorações legítimas.
- Falso negativo dar falsa sensação de segurança.

### 8.10 — Evidências manuais de segurança

**Objetivo**

Documentar evidências manuais de que o isolamento por contexto funciona em cenários representativos.

**Escopo**

- Criar roteiro de testes manuais com duas clínicas e usuários distintos.
- Validar criação, listagem, consulta por ID, leitura sensível e tentativa de acesso cruzado.
- Registrar comandos, payloads ou passos via Swagger/Postman/cURL, sem expor segredos reais.
- Documentar resultados esperados e obtidos.

**Arquivos prováveis impactados**

- `docs/security/PHASE_08_10_MANUAL_SECURITY_EVIDENCES.md`.
- Eventuais coleções ou exemplos em `docs/security/*`, se aprovados.

**Entregáveis esperados**

- Documento de evidências manuais.
- Matriz de cenários permitidos e negados.
- Registro de limitações conhecidas.

**Critérios de aceite**

- Cenários usam pelo menos duas clínicas distintas.
- Acesso cruzado é negado e auditado.
- Listagens não vazam dados de outro escopo.
- Evidências não contêm tokens, senhas ou dados pessoais reais.

**Riscos técnicos**

- Evidência manual ficar desatualizada.
- Teste manual depender de seed frágil.
- Documentar dados sensíveis por acidente.

### 8.11 — Encerramento técnico da Fase 8

**Objetivo**

Consolidar o estado final da Fase 8, registrar evidências, decisões tomadas, riscos remanescentes e recomendações para fases futuras.

**Escopo**

- Revisar entregas 8.1 a 8.10.
- Consolidar arquitetura final da segurança contextual.
- Registrar endpoints protegidos, campos de escopo, eventos de auditoria e guardrails.
- Documentar limitações e backlog futuro.

**Arquivos prováveis impactados**

- `docs/security/PHASE_08_11_CONTEXTUAL_SECURITY_CLOSURE.md`.
- Possíveis atualizações em `docs/ARCHITECTURE.md` e `docs/ROADMAP_TO_PHASE_12.md`.

**Entregáveis esperados**

- Relatório técnico de encerramento.
- Lista de evidências automatizadas e manuais.
- Riscos remanescentes e recomendações.

**Critérios de aceite**

- Relatório reflete o código efetivamente entregue.
- Riscos e limitações são explícitos.
- Próxima macrofase recomendada é documentada.
- Não há promessa de funcionalidades não implementadas.

**Riscos técnicos**

- Encerramento omitir exceções relevantes.
- Documentação divergir do comportamento real.
- Guardrails não cobrirem fluxos futuros prioritários.

## 7. Fora do escopo da Fase 8

A Fase 8 não deve incluir:

- front-end avançado de administração multiunidade;
- multi-tenant SaaS completo;
- billing, cobrança, plano, assinatura ou contrato comercial;
- infraestrutura cloud definitiva;
- deploy em produção;
- LGPD completa em nível jurídico;
- integrações externas com clínicas reais;
- federação de identidade corporativa;
- hierarquia organizacional complexa com grupos econômicos, franquias e holdings;
- particionamento físico por tenant, schema por tenant ou banco por clínica;
- criptografia campo a campo de conteúdo clínico;
- motor ABAC genérico;
- workflow completo de convite, aceite e gestão avançada de usuários por unidade;
- relatórios gerenciais multiunidade avançados;
- BI, data warehouse ou exportação analítica;
- integrações fiscais, laboratório, seguradora ou convênios;
- estoque avançado;
- financeiro avançado;
- PDF, assinatura e impressão;
- revisão jurídica final de termos, consentimentos e retenção de dados.

Esses temas podem ser planejados em fases futuras, mas não devem bloquear o objetivo de isolamento contextual mínimo, seguro e testável.

## 8. Visão de arquitetura esperada após a Fase 8

Ao final da Fase 8, a arquitetura esperada deve preservar as responsabilidades de cada camada:

### Domain

- Contém entidades mínimas de organização, clínica e unidade.
- Entidades clínicas sensíveis possuem identificadores de escopo quando o escopo for parte do estado do domínio.
- Invariantes simples impedem criação de objetos com vínculos hierárquicos inválidos.
- Não conhece HTTP, EF Core, claims, headers ou detalhes de autenticação.

### Application

- Define contratos de contexto clínico ativo, como `ICurrentClinicalContext`.
- Orquestra autorização contextual em conjunto com permissões funcionais.
- Use cases clínicos validam e propagam escopo antes de criar, consultar, listar, atualizar ou auditar registros sensíveis.
- Interfaces de repository favorecem métodos explicitamente escopados.
- Regras de acesso cruzado são testáveis sem subir API real.

### Infrastructure

- Persiste organização, clínica, unidade e escopo clínico nas entidades relevantes.
- Implementa repositories com filtros por escopo aplicados no banco.
- Mantém migrations e índices alinhados às consultas esperadas.
- Implementa escrita de auditoria contextual sem vazar conteúdo sensível.

### API

- Resolve usuário autenticado e contexto clínico ativo da requisição.
- Registra services e authorization handlers.
- Aplica `[Authorize]` e policies nos endpoints.
- Não contém regra de negócio clínica nem decisão final de isolamento de dados.
- Retorna respostas seguras para acesso negado ou recurso fora de escopo.

### Testes

- Testes de domínio cobrem entidades e invariantes de organização/clínica/unidade.
- Testes de Application cobrem propagação, autorização contextual e negações.
- Testes de Infrastructure cobrem filtros SQL/EF, constraints e auditoria.
- Testes de API cobrem policies, resolução de contexto e respostas HTTP.
- Guardrails protegem contra regressões estruturais.

### Fluxo esperado de uma requisição clínica sensível

1. API autentica o usuário.
2. API resolve o contexto clínico ativo por mecanismo aprovado.
3. Controller chama o use case com payload mínimo.
4. Application valida permissão funcional e escopo contextual.
5. Repository consulta ou persiste dados já filtrando por escopo.
6. Application registra auditoria contextual para ações sensíveis.
7. API retorna resposta minimizada e segura.

## 9. Decisões técnicas pendentes para validação futura

As decisões abaixo devem ser validadas antes ou durante as subfases correspondentes. Enquanto não forem decididas, o documento deve tratá-las como restrições de planejamento, não como implementação aprovada:

1. Se o escopo inicial obrigatório será `Clinic` ou `Organization`, considerando isolamento clínico, auditoria e evolução multiunidade.
2. Se `ClinicUnit` entra na implementação inicial ou permanece como planejamento futuro com campo opcional/reservado.
3. Se `Tutor` terá `ClinicId` direto ou relacionamento intermediário para suportar tutor atendido por múltiplas clínicas.
4. Quais entidades receberão `ClinicId` direto, incluindo tutor, paciente, atendimento, prontuário, evolução, prescrição, auditoria e entidades administrativas sensíveis.
5. Se `MedicalRecord` terá `ClinicId` direto ou herdará o escopo de `Patient`, considerando performance, integridade e facilidade de auditoria.
6. Como o contexto ativo será resolvido: claim, header, rota, seleção persistida do usuário ou combinação controlada.
7. Como evitar confiança cega em header de `ClinicId`, exigindo validação contra `UserClinicAccess` no backend.
8. Onde ficarão os filtros por contexto: repositories explícitos, especificações, handlers de Application, filtros globais EF Core ou combinação documentada.
9. Como auditar leitura sensível sem gerar volume excessivo, definindo eventos obrigatórios, amostragem quando aplicável e retenção.
10. `ClinicId` será obrigatório em todos os registros clínicos existentes ou haverá período de transição com registros legados?
11. Como representar vínculo usuário-clínica-unidade: tabela própria, claim do token, repository de membership ou modelo híbrido?
12. A resposta para recurso fora de escopo deve ser `403 Forbidden`, `404 Not Found` ou política híbrida por tipo de endpoint?
13. Quais leituras serão consideradas sensíveis e obrigatoriamente auditadas na primeira entrega?
14. Qual o nível mínimo de metadados de auditoria: IP, user agent, correlation id, request id, rota e método HTTP?
15. Como executar backfill seguro de `ClinicId` em ambientes com dados pré-existentes?
16. Quais índices compostos são necessários para os endpoints atuais?
17. Como documentar exceções legítimas a escopo, por exemplo administrador interno ou suporte técnico?
18. Como versionar contratos públicos se algum payload precisar receber informação de escopo no futuro?
19. Como integrar esta fase com seeds, testes E2E e dados de desenvolvimento?
20. Qual nomenclatura final será adotada: `Clinic`, `ClinicUnit`, `ClinicalUnit`, `Unit`, `Organization`, `Tenant` ou termos equivalentes?
21. Quais guardrails serão obrigatórios para aprovação de PRs futuros?

## 10. Resumo do refinamento

Foi refinado o planejamento técnico da Fase 8.0 para orientar a implementação gradual de segurança contextual, escopo clínico e governança multiunidade no TOGO. O documento define o problema, os riscos, os conceitos principais, a divisão em subfases, os itens fora de escopo, a arquitetura esperada após a fase e as decisões pendentes para validação futura.

## 11. Arquivo ajustado

- `docs/security/PHASE_08_00_CONTEXTUAL_SECURITY_PLANNING.md`

## 12. Próxima fase recomendada

**Fase 8.1 — Modelo mínimo de Organização/Clínica/Unidade**.

A próxima fase deve introduzir apenas o modelo mínimo de domínio, com testes, sem alterar ainda os fluxos clínicos existentes e sem tentar resolver toda a governança multiunidade de uma só vez.

## 13. Checklist de aceite da Fase 8.0

- [x] Documento de planejamento criado em `docs/security`.
- [x] Objetivo da Fase 8 documentado.
- [x] Problema resolvido pela Fase 8 documentado.
- [x] Riscos mitigados pela fase documentados.
- [x] Conceitos principais definidos.
- [x] Subfases 8.0, 8.0.1 e 8.1 a 8.11 propostas.
- [x] Cada subfase contém objetivo, escopo, arquivos prováveis impactados, entregáveis, critérios de aceite e riscos técnicos.
- [x] Itens fora do escopo definidos.
- [x] Visão de arquitetura esperada após a Fase 8 documentada.
- [x] Decisões técnicas pendentes registradas.
- [x] Próxima fase recomendada indicada.
- [x] Refinamento documental 8.0.1 registrado sem alteração de código produtivo.
- [x] Segurança contra confiança cega em `ClinicId` de header, claim, rota ou request explicitada.
- [x] Decisões sobre `Clinic`, `Organization`, `ClinicUnit`, `Tutor`, `MedicalRecord`, filtros e auditoria de leitura registradas como pendentes.
