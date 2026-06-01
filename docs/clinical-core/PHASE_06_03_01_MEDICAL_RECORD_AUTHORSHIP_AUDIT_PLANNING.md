# TOGO — Fase 6.3.1: Planejamento técnico de autoria clínica e auditoria MedicalRecord

## 2. Reabertura do contexto da Fase 6

A Fase 6 permanece dedicada ao hardening da vertical MedicalRecord antes de qualquer liberação para produção real com dados clínicos sensíveis.

- **Fase 6.1 — Governança do hardening:** criou o registro vivo de débitos técnicos, definiu a priorização P1/P2/P3 e formalizou que qualquer P1 aberto mantém a produção real bloqueada.
- **Fase 6.2 — Segurança e autorização granular:** tratou o débito **MR-DEBT-003 — Roles/permissões finas ausentes** e foi formalmente encerrada com autorização granular mínima por operação e profile JWT.
- **Fase 6.3 — Auditoria e autoria clínica:** inicia o planejamento dos débitos P1 ainda abertos **MR-DEBT-004 — Controle de autoria ausente** e **MR-DEBT-002 — AuditLog ausente**.

A Fase 6.3 não reabre MR-DEBT-003. Ela parte da autorização mínima já entregue e acrescentará, de forma incremental, rastreabilidade de responsabilidade atual e histórico de eventos relevantes.

## 3. Objetivo da Fase 6.3.1

Esta fase é exclusivamente documental. Seu objetivo é definir uma estratégia mínima, segura e incremental para a futura implementação de autoria clínica e AuditLog da vertical MedicalRecord, sem overengineering.

Objetivos específicos:

- planejar tecnicamente a autoria clínica;
- planejar tecnicamente o AuditLog clínico;
- separar claramente autoria de auditoria;
- definir um sequenciamento incremental para implementação futura;
- evitar a implementação improvisada de campos, tabelas ou logs;
- manter MedicalRecord como MVP técnico ainda não liberado para produção real;
- registrar critérios objetivos para a resolução futura de MR-DEBT-004 e MR-DEBT-002.

Nenhuma proposta deste documento representa implementação já realizada.

## 4. Fontes principais desta fase

### 4.1 Documentação de governança e encerramento anterior

- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`;
- `docs/clinical-core/PHASE_06_01_02_MEDICAL_RECORD_PRODUCTION_READINESS_PRIORIZATION.md`;
- `docs/clinical-core/PHASE_06_01_03_MEDICAL_RECORD_HARDENING_GOVERNANCE_CLOSURE.md`;
- `docs/clinical-core/PHASE_06_02_06_MEDICAL_RECORD_AUTHORIZATION_CLOSURE.md`.

### 4.2 Código atual consultado para delimitar o planejamento

- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`;
- `backend/src/Togo.Api/Controllers/MedicalRecordsController.cs`;
- `backend/src/Togo.Domain/Entities/User.cs`;
- `backend/src/Togo.Domain/Security/TogoClaimTypes.cs`;
- `backend/src/Togo.Infrastructure/Tokens/JwtTokenService.cs`;
- `backend/src/Togo.Api.Tests/MedicalRecords/MedicalRecordsControllerTests.cs`.

## 5. Estado atual pós-Fase 6.2

O estado atual observado e formalmente herdado da Fase 6.2 é:

- MedicalRecord possui autorização granular mínima;
- o acesso já é filtrado por profile JWT e policy por operação;
- a claim customizada `togo:profile` já representa o profile mínimo usado pelas policies;
- o JWT emitido atualmente inclui identificadores `sub` e `ClaimTypes.NameIdentifier`, além de nome, e-mail e profile;
- a entidade `MedicalRecord` ainda persiste apenas `Id`, `PatientId`, `GeneralNotes`, `FlagsJson` e `UpdatedAt`;
- ainda não há persistência de autoria clínica;
- ainda não há AuditLog clínico;
- ainda não há trilha formal de criação ou alteração;
- produção real segue bloqueada por **MR-DEBT-004**, **MR-DEBT-002** e pelos demais P1 abertos no registro vivo.

A autorização entregue na Fase 6.2 reduz o conjunto de usuários autorizados por operação, mas não substitui autoria nem auditoria.

## 6. Diferença entre autoria clínica e AuditLog

Autoria clínica e AuditLog são mecanismos complementares, mas não equivalentes.

| Tema | Autoria clínica | AuditLog clínico |
|---|---|---|
| Finalidade | Identificar o responsável pela criação e pela alteração mais recente do dado clínico. | Manter uma trilha histórica de eventos relevantes ocorridos ao longo do tempo. |
| Forma mínima esperada | Campos no próprio registro ou em entidade relacionada. | Tabela ou entidade append-only dedicada a eventos. |
| Pergunta principal | **Quem é o responsável atual pelo registro ou pela alteração mais recente?** | **O que aconteceu ao longo do tempo?** |
| Exemplos | Usuário criador, data de criação, usuário da última alteração e data da última alteração. | Criação, alteração, leitura quando exigida pela decisão de auditoria, tentativa negada, mudança de conteúdo sensível ou alteração administrativa. |

A autoria clínica oferece uma visão consolidada do estado atual. O AuditLog oferece uma sequência histórica. Apenas adicionar `CreatedByUserId` e `UpdatedByUserId` não preservaria eventos intermediários; apenas adicionar AuditLog não tornaria explícita a autoria corrente no registro.

## 7. Risco atual

Mesmo após a autorização granular mínima da Fase 6.2:

- ainda não é possível saber, a partir do MedicalRecord persistido, quem criou o registro;
- ainda não é possível saber quem realizou a alteração clínica mais recente;
- não há rastreabilidade histórica de eventos clínicos;
- não há evidência operacional suficiente para produção real;
- incidentes clínicos ou alterações indevidas seriam difíceis de investigar;
- logs técnicos isolados não substituem uma trilha clínica estruturada e segura;
- o bloqueio de produção real permanece ativo.

O risco deve ser tratado sem registrar conteúdo clínico desnecessário em logs e sem confundir observabilidade técnica com auditoria clínica.

## 8. Modelo mínimo de autoria clínica recomendado

### 8.1 Campos candidatos

Para a futura implementação de MR-DEBT-004, recomenda-se avaliar a inclusão mínima dos seguintes campos em `MedicalRecord` ou em persistência relacionada, preservando um modelo simples:

| Campo | Recomendação | Justificativa |
|---|---|---|
| `CreatedByUserId` | Obrigatório. | Identificador estável do usuário autenticado responsável pela criação. |
| `CreatedAt` | Obrigatório. | A entidade atual não possui data explícita de criação; `UpdatedAt` não deve substituir essa informação. |
| `UpdatedByUserId` | Obrigatório. | Identificador estável do usuário autenticado responsável pela alteração mais recente. |
| `UpdatedAt` | Obrigatório, aproveitando ou revisando o campo existente. | A entidade já possui esse campo; a implementação futura deverá preservar semântica clara e consistente. |
| `CreatedByUserName` ou `CreatedByProfile` | Opcional; não incluir automaticamente no mínimo inicial. | Snapshot pode ajudar leitura histórica quando nome ou profile mudarem, mas aumenta duplicação e deve ter justificativa explícita. |
| `UpdatedByUserName` ou `UpdatedByProfile` | Opcional; não incluir automaticamente no mínimo inicial. | Snapshot pode ser útil para investigação, porém deve ser ponderado contra minimização de dados e inconsistência histórica. |

### 8.2 Origem do identificador do usuário

A implementação futura deve preferir um identificador estável do usuário autenticado. O código atual já emite o `User.Id` como `sub` e como `ClaimTypes.NameIdentifier`. Recomenda-se que a futura abstração de usuário atual:

1. leia prioritariamente `ClaimTypes.NameIdentifier`;
2. aceite `sub` como fallback compatível;
3. valide o formato esperado para o identificador estável, atualmente `Guid` em `User.Id`;
4. não use nome, e-mail ou profile como chave de autoria;
5. exponha ao Application um contrato limpo, sem acoplar o domínio diretamente ao `HttpContext`.

### 8.3 Snapshots de nome e profile

A preferência inicial é persistir apenas identificadores estáveis. Nome e profile podem mudar e não devem ser necessários para resolver a identidade do usuário.

Se uma fase futura decidir armazenar snapshot de nome ou profile, a decisão deverá justificar objetivamente o valor histórico ou operacional. O snapshot deverá ser mínimo, não poderá incluir e-mail, credenciais ou outros dados pessoais desnecessários e não deverá substituir `CreatedByUserId` ou `UpdatedByUserId`.

### 8.4 Usuário não resolvido

Para operações autenticadas de criação e alteração de MedicalRecord, a estratégia recomendada é **falhar de forma fechada** quando o usuário não puder ser resolvido para um identificador estável válido. A aplicação não deve persistir autoria vazia, fake ou hardcoded e não deve concluir silenciosamente a mutação clínica.

Para eventos futuros de acesso negado, o AuditLog poderá aceitar usuário ausente quando isso for inerente ao evento, desde que registre apenas metadados seguros e nunca invente uma identidade.

## 9. Modelo mínimo de AuditLog recomendado

### 9.1 Entidade ou tabela dedicada

Para a futura implementação de MR-DEBT-002, recomenda-se criar uma tabela ou entidade dedicada, denominada `AuditLog` ou `ClinicalAuditLog`. A escolha final do nome pertence à fase de contratos e implementação, mas o mecanismo deve permanecer separado de `MedicalRecord`.

Campos mínimos candidatos:

| Campo | Recomendação | Observação de segurança |
|---|---|---|
| `Id` | Obrigatório. | Identificador do evento de auditoria. |
| `EntityName` | Obrigatório. | Exemplo: `MedicalRecord`; evitar informação clínica no valor. |
| `EntityId` | Obrigatório quando conhecido. | Identificador técnico da entidade auditada. |
| `Action` | Obrigatório. | Ação controlada por catálogo ou constante. |
| `UserId` | Obrigatório quando o usuário autenticado puder ser resolvido. | Usar identificador estável; permitir ausência apenas quando a natureza do evento justificar. |
| `UserProfile` | Recomendado como snapshot mínimo quando disponível. | Útil para investigar a autorização efetiva no momento do evento; não substitui `UserId`. |
| `OccurredAt` | Obrigatório. | Instante UTC do evento. |
| `CorrelationId` | Recomendado se puder reutilizar de forma simples o identificador de rastreamento da requisição. | Não criar infraestrutura complexa apenas para esta fase. |
| `MetadataJson` | Opcional e estritamente limitado. | Armazenar somente metadados mínimos permitidos, nunca payload clínico sensível. |
| `IpAddress` | Opcional; decidir posteriormente. | Incluir somente se houver necessidade operacional ou de segurança clara e tratamento adequado de dado pessoal. |
| `UserAgent` | Opcional; decidir posteriormente. | Incluir somente se trouxer valor investigativo proporcional; evitar exagero no MVP. |

`CorrelationId`, `IpAddress` e `UserAgent` não devem bloquear o primeiro incremento se exigirem complexidade desproporcional. O primeiro objetivo é persistir eventos clínicos relevantes com identidade estável, ação e instante.

### 9.2 Ações mínimas sugeridas

| Ação | Tratamento recomendado |
|---|---|
| `MedicalRecord.Created` | Obrigatória no primeiro incremento de AuditLog. |
| `MedicalRecord.Updated` | Obrigatória no primeiro incremento de AuditLog. |
| `MedicalRecord.Read` | Exige decisão explícita antes da implementação; habilitar somente se a política de auditoria concluir que leitura clínica deve ser auditável. |
| `MedicalRecord.AccessDenied` | Avaliar implementação quando tecnicamente viável sem vazar dados; pode exigir integração fora do fluxo normal do controller devido à avaliação prévia das policies. |

A implementação futura deverá definir transação e consistência entre mutação clínica e evento correspondente, evitando declarar sucesso clínico sem o evento mínimo exigido pela política adotada.

## 10. Política de segurança do AuditLog

O AuditLog clínico deverá seguir minimização de dados e segurança por padrão:

- não registrar conteúdo clínico sensível completo;
- não registrar `GeneralNotes` integralmente;
- não registrar `FlagsJson` integralmente;
- não registrar payloads de request ou response;
- não copiar prontuário, observações ou flags para `MetadataJson`;
- registrar somente metadados mínimos necessários para identificar entidade, ação, usuário, profile quando aplicável, instante e correlação;
- preferir indicadores controlados, como nomes de campos alterados, quando essa granularidade for futuramente necessária, em vez dos valores anteriores ou novos;
- não expor AuditLog em endpoint público nesta fase;
- tratar AuditLog como append-only conceitualmente;
- proibir ou restringir fortemente alterações e deleções futuras de eventos de auditoria;
- manter logs técnicos sem payload clínico sensível.

A definição de retenção do AuditLog deverá ser tratada em fase própria de governança e retenção. Esta fase não resolve política de retenção.

## 11. Sequenciamento recomendado da Fase 6.3

| Subfase | Objetivo |
|---|---|
| **6.3.1 — Planejamento técnico de autoria clínica e auditoria** | Definir estratégia incremental, limites de segurança e critérios futuros antes de implementar. |
| **6.3.2 — Definição de contratos e abstrações de usuário atual/auditoria** | Criar base mínima e reutilizável para obter usuário autenticado e registrar auditoria sem acoplamento indevido. |
| **6.3.3 — Implementação de autoria clínica em MedicalRecord** | Persistir autoria mínima de criação e alteração derivada do usuário autenticado. |
| **6.3.4 — Implementação mínima de AuditLog clínico** | Persistir eventos mínimos de auditoria clínica sem payload sensível. |
| **6.3.5 — Testes de autoria e auditoria** | Validar autoria, eventos principais, falha segura e ausência de vazamento sensível. |
| **6.3.6 — Atualização do registro vivo e encerramento da Fase 6.3** | Atualizar MR-DEBT-004 e MR-DEBT-002 conforme evidências e formalizar o encerramento. |

O sequenciamento reduz improvisação e mantém cada incremento revisável. As subfases poderão receber correções documentais pontuais se necessário, sem misturar responsabilidades.

## 12. Impactos técnicos previstos

As fases futuras poderão gerar os seguintes impactos, ainda não implementados nesta fase:

- Domain/`MedicalRecord` pode precisar de campos e invariantes de autoria;
- Application use cases podem precisar receber contexto do usuário atual;
- API pode precisar expor um serviço de `CurrentUser` ou adaptador equivalente;
- Infrastructure pode precisar persistir `AuditLog` ou `ClinicalAuditLog`;
- migrations podem ser necessárias;
- testes de controller e use case precisarão simular usuário autenticado;
- testes de infraestrutura poderão precisar validar persistência da autoria e do AuditLog;
- o tratamento de `MedicalRecord.AccessDenied` poderá exigir integração apropriada ao pipeline de autorização;
- logs técnicos devem continuar sem payload clínico sensível;
- o registro vivo deverá ser atualizado ao final da Fase 6.3.

Esses impactos devem ser detalhados e implementados apenas nas subfases correspondentes.

## 13. Critérios futuros para resolver MR-DEBT-004

**MR-DEBT-004 — Controle de autoria ausente** somente poderá ser considerado resolvido quando:

- MedicalRecord persistir autoria mínima de criação e alteração;
- a autoria vier do usuário autenticado;
- testes validarem criação com autor;
- testes validarem atualização com autor;
- documentação for atualizada;
- registro vivo for atualizado;
- não houver autoria fake ou hardcoded;
- não houver persistência ou vazamento de dados sensíveis desnecessários;
- operações de mutação falharem de forma segura quando não houver usuário autenticado resolvível.

## 14. Critérios futuros para resolver MR-DEBT-002

**MR-DEBT-002 — AuditLog ausente** somente poderá ser considerado resolvido quando:

- existir mecanismo mínimo de AuditLog clínico;
- eventos relevantes forem registrados;
- AuditLog não armazenar payload clínico sensível;
- testes validarem os eventos principais;
- documentação for atualizada;
- registro vivo for atualizado;
- houver decisão clara sobre auditoria de leitura;
- não houver endpoint público inseguro de AuditLog;
- a política mínima de append-only estiver preservada conceitualmente.

## 15. Decisão recomendada para a 6.3.1

**Opção A — Planejamento técnico de autoria clínica e auditoria aprovado como base para implementação incremental de MR-DEBT-004 e MR-DEBT-002.**

Essa decisão não resolve os débitos e não libera MedicalRecord para produção real. Ela autoriza apenas a continuidade organizada da Fase 6.3.

## 16. Fora do escopo

Esta fase não deve:

- alterar código;
- alterar testes;
- criar migration;
- alterar banco;
- alterar `MedicalRecord`;
- alterar controller;
- alterar `Program.cs`;
- alterar JWT;
- criar `AuditLog`;
- criar endpoint de auditoria;
- implementar autoria clínica;
- alterar frontend;
- alterar Docker, Redis, RabbitMQ ou Kubernetes;
- resolver Soft Delete;
- resolver retenção;
- resolver `DeleteBehavior.Cascade`.

## 17. Critérios de aceite da Fase 6.3.1

A fase será considerada concluída se:

- este documento for criado no caminho correto;
- MR-DEBT-004 e MR-DEBT-002 forem explicitamente referenciados;
- o estado atual pós-Fase 6.2 for documentado;
- a diferença entre autoria e AuditLog for explicada;
- o risco atual for descrito;
- o modelo mínimo de autoria clínica for proposto;
- o modelo mínimo de AuditLog for proposto;
- a política de segurança do AuditLog for definida;
- o sequenciamento da Fase 6.3 for definido;
- os impactos técnicos previstos forem listados;
- os critérios futuros de resolução de MR-DEBT-004 e MR-DEBT-002 forem definidos;
- nenhuma implementação for feita;
- o escopo permanecer exclusivamente documental.

## 18. Próxima fase recomendada

**Fase 6.3.2 — Definição de contratos e abstrações de usuário atual/auditoria.**

### Objetivo da Fase 6.3.2

Criar a base técnica mínima para capturar o usuário autenticado de forma limpa e reutilizável, preparando a implementação de autoria clínica e AuditLog sem acoplar controller diretamente ao domínio.
