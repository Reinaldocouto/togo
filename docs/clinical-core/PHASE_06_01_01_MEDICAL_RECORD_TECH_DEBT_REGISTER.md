# TOGO — Fase 6.1.1: Registro vivo de débitos técnicos e plano de hardening da vertical MedicalRecord

## 2. Resumo da Subfase 6.1

Subfase 6.1 — Governança do hardening e registro de débitos

Planejamento:
- 6.1.1 — Registro vivo de débitos técnicos e plano de hardening.
- 6.1.2 — Priorização P1/P2/P3 e critérios de produção segura.
- 6.1.3 — Documentação final da governança do hardening.
- 6.1.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## 3. Objetivo

Esta fase cria um registro vivo dos débitos técnicos da vertical MedicalRecord, servindo simultaneamente como backlog técnico de hardening, lembrete formal de riscos aceitos no MVP e guia de priorização para as fases futuras da trilha de produção segura.

Este documento responde, de forma rastreável e atualizável:
- quais débitos existem;
- por que existem;
- qual risco cada débito representa;
- qual prioridade cada débito possui;
- se bloqueia MVP;
- se bloqueia produção real;
- qual fase futura deve tratar cada item;
- qual é a recomendação de execução.

## 4. Contexto

- A Fase 5 entregou MedicalRecord como MVP técnico.
- A Fase 5 não aprovou a vertical para produção real com dados sensíveis.
- Os débitos P1 precisam ser tratados antes de produção real.
- A Fase 6 será dedicada ao hardening clínico, segurança, auditoria, persistência e governança.
- Esta fase não implementa correções, apenas organiza o registro vivo.

## 5. Decisão de governança

Este documento passa a ser o registro vivo oficial dos débitos técnicos da vertical MedicalRecord.

Regras:
- todo débito novo relacionado a MedicalRecord deve ser registrado aqui;
- todo débito resolvido deve ser marcado como resolvido;
- toda fase futura que tratar débito deve referenciar o ID correspondente;
- débitos P1 não devem ser esquecidos antes de produção real;
- este documento não substitui issues/backlog, mas serve como registro arquitetural permanente.

## 6. Classificação dos débitos

### Prioridade
- P1 — obrigatório antes de produção real;
- P2 — hardening técnico próximo;
- P3 — evolução técnica posterior.

### Status
- Aberto;
- Em planejamento;
- Em execução;
- Resolvido;
- Adiado;
- Substituído.

### Bloqueio
- Bloqueia MVP técnico;
- Bloqueia produção real;
- Não bloqueia.

### Categorias
- Segurança;
- Auditoria;
- Persistência;
- Compliance;
- Integridade;
- Modelagem;
- Operação;
- Qualidade.

## 7. Registro vivo de débitos técnicos

| ID | Débito | Categoria | Prioridade | Status | Bloqueia MVP? | Bloqueia produção real? | Evidência | Risco | Mitigação atual | Fase futura recomendada |
|---|---|---|---|---|---|---|---|---|---|---|
| MR-DEBT-001 | Soft Delete ausente | Persistência/Compliance | P1 | Resolvido tecnicamente para Soft Delete clínico mínimo | Não | Não, como bloqueio ativo isolado. | MedicalRecord possui IsDeleted, DeletedAt e DeletedByUserId; Soft Delete preserva dados clínicos e autoria; consultas padrão ignoram registros logicamente deletados. | Risco original de exclusão física direta mitigado nos fluxos clínicos padrão; endpoint público DELETE, restore e consulta administrativa permanecem fora do escopo. | Soft Delete persistido, filtros explícitos em queries padrão e ausência de endpoint DELETE público. | Tratado nas Fases 6.4.2 e 6.4.3; evidências finais na Fase 6.4.6. |
| MR-DEBT-002 | AuditLog ausente | Auditoria | P1 | Resolvido tecnicamente para AuditLog clínico mínimo | Não | Não, como bloqueio ativo isolado; produção real pode depender de validações macro posteriores. | `ClinicalAuditLogs` persiste eventos `MedicalRecord.Created` e `MedicalRecord.Updated` com usuário, entidade, ação, instante UTC e metadata mínima. | Risco original mitigado para criação/atualização; leitura, acesso negado, endpoint público e transação única permanecem fora do escopo. | AuditLog interno sem payload clínico sensível, sem endpoint público e sem tela frontend. | Tratado na Fase 6.3.4; evidências finais na Fase 6.3.5. |
| MR-DEBT-003 | Roles/permissões finas ausentes | Segurança | P1 | Resolvido tecnicamente para autorização granular mínima | Não | Não, como bloqueio ativo isolado; produção real pode depender de validações macro posteriores. | Policies por operação, perfil mínimo e claim `togo:profile` aplicados ao fluxo MedicalRecord. | Risco original mitigado por autorização granular mínima; permanecem validações macro de segurança, compliance, infraestrutura e produto. | Policies ASP.NET Core avaliadas por profile JWT, com matriz explícita e testes de autorização/controller. | Tratado nas fases 6.2.1 a 6.2.5; encerramento formal na 6.2.6. |
| MR-DEBT-004 | Controle de autoria ausente | Auditoria/Segurança | P1 | Resolvido tecnicamente para autoria clínica mínima | Não | Não, como bloqueio ativo isolado; produção real pode depender de validações macro posteriores. | `MedicalRecord` possui `CreatedByUserId`, `CreatedAt`, `UpdatedByUserId` e `UpdatedAt` preenchidos por usuário autenticado. | Risco original mitigado para criação/atualização; escopos avançados permanecem como possíveis débitos futuros. | Autoria mínima persistida sem defaults finais de banco e sem autoria fake/hardcoded em produção. | Tratado na Fase 6.3.3; hotfix de defaults na 6.3.3.1; evidências finais na 6.3.5. |
| MR-DEBT-005 | Política de retenção não implementada | Compliance/Governança | P1 | Resolvido tecnicamente por decisão formal de retenção clínica inicial | Não | Não, como bloqueio ativo isolado, considerando a política inicial documentada. | Decisão de retenção indefinida inicial para MedicalRecord, preservação de registros com Soft Delete, preservação de ClinicalAuditLog e proibição de expurgo automático nesta fase. | Risco original mitigado por decisão conservadora; política pode exigir revisão futura por compliance/regulação/produto. | Retenção indefinida inicial, sem expurgo automático, sem job/scheduler/worker e sem automação prematura. | Tratado na Fase 6.4.5; evidências finais na Fase 6.4.6. |
| MR-DEBT-006 | DeleteBehavior.Cascade pendente de revisão | Persistência/Integridade clínica | P1 | Resolvido tecnicamente por revisão de cascades clínicos críticos | Não | Não, como bloqueio ativo isolado. | Relações Patient -> MedicalRecord, Patient -> Attendance, Attendance -> ClinicalEvolution e Attendance -> Prescription alteradas para Restrict; Cascade mantido apenas onde justificado. | Risco original de exclusão indireta mitigado nas relações clínicas críticas revisadas; novas entidades/relações futuras devem seguir a mesma política. | DeleteBehavior.Restrict em relações clínicas críticas, migration ReviewClinicalCascadeDeleteBehavior e testes de infraestrutura. | Tratado na Fase 6.4.4; evidências finais na Fase 6.4.6. |
| MR-DEBT-007 | Índice único em MedicalRecords.PatientId ausente | Integridade/Banco | P2 | Resolvido tecnicamente por índice único físico em MedicalRecords.PatientId | Não | Não, como bloqueio ativo isolado. | Índice único físico em PatientId, validator alinhado com unicidade total e testes de infraestrutura bloqueando duplicidade ativa e duplicidade com registro deletado. | Risco original mitigado; ambientes com dados legados devem validar duplicidades antes de aplicar a migration. | Constraint física + validação lógica coerente. | Tratado na Fase 6.5.2, complementado na Fase 6.5.2.1 e consolidado na Fase 6.5.4. |
| MR-DEBT-008 | CreatedAt ausente | Rastreabilidade temporal | P2 | Resolvido tecnicamente pela Fase 6.3.3 | Não | Não, como bloqueio ativo isolado. | `MedicalRecord` possui `CreatedAt` persistido, preenchido na criação e preservado na atualização. | Risco original mitigado; rastreabilidade temporal de criação agora existe. | `CreatedAt` persistido junto da autoria clínica mínima. | Tratado na Fase 6.3.3; evidências finais na Fase 6.3.5. |
| MR-DEBT-009 | FlagsJson flexível | Modelagem/Validação | P2 | Resolvido tecnicamente no escopo inicial de validação estrutural de FlagsJson | Não | Não, como bloqueio ativo isolado para novos fluxos. | MedicalRecord aceita null/vazio como ausência e exige objeto JSON válido para valores informados, com aplicação da mesma regra em criação e atualização e cobertura automatizada. | Risco original mitigado: novos fluxos não persistem JSON inválido nem valores JSON escalares/arrays na raiz. Riscos remanescentes: não existe schema semântico, allowlist de chaves, saneamento de dados legados, normalização relacional, indexação interna do JSON ou tratamento específico de propriedades duplicadas; interoperabilidade futura ainda depende de contrato clínico mais estável. | Validação estrutural no domínio, rejeição segura, testes de domínio/Application/API e ausência de payload sensível no AuditLog. | Tratado na Fase 6.5.3, com consolidação final na Fase 6.5.4. |
| MR-DEBT-010 | CancellationToken não propagado no repository | Operação/Resiliência | P3 | Resolvido tecnicamente por propagação de CancellationToken no repository MedicalRecord | Não | Não diretamente | IMedicalRecordRepository recebe CancellationToken, MedicalRecordRepository repassa token às operações async do EF Core, use cases/validators propagam o token recebido e testes cobrem os caminhos principais. | Requests canceladas passam a ter caminho de cancelamento até operações de banco da vertical MedicalRecord; cancelamento efetivo ainda depende do provider, do banco e do momento da operação; outras verticais podem manter repositories sem token. | Propagação do token na vertical MedicalRecord sem alteração de regra de negócio. | Tratado na Fase 6.6.2; evidências finais na Fase 6.6.5. |
| MR-DEBT-011 | Evidências manuais Swagger não versionadas formalmente | QA/Governança | P3 | Resolvido tecnicamente por evidência manual versionada de API/Swagger MedicalRecord | Não | Não diretamente | Documento versionado de evidência manual cobre cenários GET/POST/PUT, validação, conflito, autenticação/autorização e respostas esperadas com dados sanitizados; trata-se de roteiro versionado com resultados esperados, sem declaração de execução real da API nesta fase. | Evidências manuais deixam de depender apenas de prints ou validações informais não versionadas; evidência manual não substitui testes automatizados; execução real pode variar por ambiente; novos endpoints futuros exigirão nova evidência. | Documento versionado em docs/clinical-core com cenários mínimos sanitizados e relação com testes automatizados. | Tratado na Fase 6.6.3; evidências finais na Fase 6.6.5. |
| MR-DEBT-012 | MedicalRecordListItemResponse ainda não usado | API/Privacidade futura | P3 | Aberto | Não | Não | Contract existe, mas não há endpoint de listagem. | Listagens futuras podem ignorar contrato seguro. | Não existe endpoint de listagem. | Consultas/listagens seguras futuras. |

## 8. Débitos P1 — obrigatórios antes de produção real

Itens P1 originalmente mapeados:
- Soft Delete;
- AuditLog;
- roles/permissões finas;
- controle de autoria;
- política de retenção;
- revisão DeleteBehavior.Cascade.

Após a Fase 6.2, **MR-DEBT-003 — Roles/permissões finas ausentes** está resolvido tecnicamente para o escopo mínimo de autorização granular MedicalRecord.

Após a Fase 6.3, **MR-DEBT-004 — Controle de autoria ausente** e **MR-DEBT-002 — AuditLog ausente** também estão resolvidos tecnicamente no escopo incremental planejado. O tratamento é técnico e mínimo: cobre autoria persistida em criação/atualização e AuditLog interno para `MedicalRecord.Created`/`MedicalRecord.Updated`, sem payload clínico sensível.

Continuam fora do escopo deste tratamento:
- auditoria de leitura;
- auditoria de acesso negado;
- endpoint público de auditoria;
- tela frontend de auditoria;
- transação única entre a operação principal e o AuditLog.

Esses pontos podem virar débitos futuros, se necessário. Soft Delete, retenção e revisão de `DeleteBehavior.Cascade` não permanecem como P1 abertos nesta trilha: foram resolvidos tecnicamente nas Fases 6.4.2 a 6.4.6. A produção real com dados clínicos sensíveis ainda depende de validações macro posteriores de segurança, compliance, infraestrutura e produto.

## 9. Débitos P2 — hardening técnico próximo

Itens P2 nesta trilha:
- `MR-DEBT-007 — Índice único em MedicalRecords.PatientId ausente` foi resolvido tecnicamente na Fase 6.5.2, com índice único físico em `MedicalRecords.PatientId`, validação lógica alinhada e Soft Delete contando para unicidade;
- a janela de conflito concorrente remanescente de `MR-DEBT-007` foi tratada complementarmente na Fase 6.5.2.1, traduzindo a violação física específica de `IX_MedicalRecords_PatientId` para conflito de negócio/HTTP 409;
- `MR-DEBT-009 — FlagsJson flexível` foi resolvido tecnicamente no escopo inicial de validação estrutural de `FlagsJson` na Fase 6.5.3.

O item originalmente mapeado como `CreatedAt` (**MR-DEBT-008**) não permanece como P2 aberto: foi resolvido tecnicamente na Fase 6.3.3 como parte da autoria clínica mínima e consolidado documentalmente nas Fases 6.3.5 e 6.3.6.

`MR-DEBT-009` agora mitiga a persistência de novos valores inválidos ao exigir objeto JSON válido na raiz, preservando `null`, vazio e whitespace como ausência. Permanecem fora do escopo schema semântico, allowlist de chaves, saneamento de dados legados, normalização relacional, indexação interna do JSON e contrato clínico estável futuro.

## 10. Débitos P3 — evolução técnica posterior

Itens P3 mapeados:
- CancellationToken;
- evidências manuais versionadas;
- uso futuro de MedicalRecordListItemResponse.

Esses itens são evoluções de qualidade e governança, relevantes para maturidade operacional, porém sem bloqueio direto para o MVP técnico atual.

## 11. Roadmap recomendado da Fase 6

### 6.2 — Segurança e autorização granular
- MR-DEBT-003 — resolvido tecnicamente para autorização granular mínima pelas fases 6.2.1 a 6.2.5; encerramento formal registrado na Fase 6.2.6.

### 6.3 — Auditoria e autoria clínica
- MR-DEBT-004 — resolvido tecnicamente para autoria clínica mínima pela Fase 6.3.3, com hotfix de defaults na 6.3.3.1 e evidências finais na 6.3.5;
- MR-DEBT-002 — resolvido tecnicamente para AuditLog clínico mínimo pela Fase 6.3.4, com evidências finais na 6.3.5.

### 6.4 — Persistência clínica e retenção
- MR-DEBT-001 — resolvido tecnicamente para Soft Delete clínico mínimo nas Fases 6.4.2 e 6.4.3, com consolidação na Fase 6.4.6;
- MR-DEBT-005 — resolvido tecnicamente por decisão formal de retenção clínica inicial na Fase 6.4.5, com consolidação na Fase 6.4.6;
- MR-DEBT-006 — resolvido tecnicamente por revisão de cascades clínicos críticos na Fase 6.4.4, com consolidação na Fase 6.4.6.

### 6.5 — Integridade e evolução de schema
- MR-DEBT-007 — resolvido tecnicamente por índice único físico em `MedicalRecords.PatientId` na Fase 6.5.2; a Fase 6.5.2.1 complementou o hardening ao tratar conflito concorrente da constraint como conflito de negócio;
- MR-DEBT-009 — resolvido tecnicamente no escopo inicial de validação estrutural de `FlagsJson` na Fase 6.5.3; consolidação final realizada na Fase 6.5.4.

Observação: MR-DEBT-008 foi retirado da fila aberta da Fase 6.5 porque `CreatedAt` foi implementado na Fase 6.3.3 como parte da autoria clínica mínima.

### 6.6 — Qualidade operacional
- MR-DEBT-010;
- MR-DEBT-011;
- MR-DEBT-012.

## 12. Como atualizar este registro

Quando um débito for tratado:
- atualizar Status para Resolvido;
- registrar fase/PR responsável;
- registrar resumo da solução;
- registrar impacto da solução;
- manter histórico do débito;
- não apagar o débito resolvido.

Se um débito for substituído:
- marcar como Substituído;
- apontar para o novo ID.

Se um novo débito surgir:
- criar novo ID sequencial;
- classificar categoria/prioridade;
- registrar evidência e risco.

## 13. Decisão da Fase 6.1.1

Opção A — Registro vivo de débitos técnicos aprovado como base de governança da Fase 6.

Justificativa:
A Fase 6 deve começar com rastreabilidade clara dos débitos antes de implementar hardening, para evitar correções soltas e perda de contexto arquitetural.

## 14. Critérios de aceite

A fase será considerada concluída se:
- documento de registro vivo for criado;
- débitos da Fase 5 forem consolidados;
- cada débito tiver ID;
- cada débito tiver prioridade;
- cada débito tiver status;
- cada débito tiver recomendação de fase futura;
- débitos P1 forem destacados;
- roadmap da Fase 6 for proposto;
- regra de atualização do registro for definida;
- nenhuma implementação for feita;
- escopo permanecer exclusivamente documental.

## 15. Fora do escopo

Esta fase não implementa:
- código;
- testes;
- migrations;
- database update;
- banco real;
- novos endpoints;
- Soft Delete;
- AuditLog;
- roles/permissões;
- índice único;
- FlagsJson estrutural;
- CancellationToken;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## 16. Próxima fase recomendada

Fase 6.1.2 — Priorização P1/P2/P3 e critérios de produção segura.

Objetivo:
Definir quais débitos P1 devem ser tratados primeiro, quais critérios mínimos autorizam produção real futura e quais entregas compõem o primeiro pacote de hardening.

## 17. Validações obrigatórias

Validações executadas nesta fase documental:
- `git branch --show-current`;
- `git status --short`;
- `git diff --check`;
- `dotnet build backend/Togo.sln` (quando disponível);
- `dotnet test backend/Togo.sln` (quando disponível).


## 18. Atualização viva — encerramento da Fase 6.2

### 18.1 MR-DEBT-003 — histórico e status atualizado

**Status anterior:** aberto, pendente e bloqueante para produção real.

**Novo status:** resolvido tecnicamente para autorização granular mínima.

**Fases responsáveis pela solução:** 6.2.1, 6.2.2, 6.2.3, 6.2.4 e 6.2.5.

**PRs responsáveis:** PR 135, PR 136, PR 137, PR 138 e PR 139.

**Resumo da solução:** MedicalRecord deixou de depender apenas de `[Authorize]` genérico. A solução adotou permissões e policies centralizadas, profile mínimo persistido no usuário, claim própria `togo:profile` emitida no JWT, registro das policies no ASP.NET Core Authorization, aplicação de policy por operação no `MedicalRecordsController`, matriz explícita de perfil x permissão e cobertura automatizada da avaliação direta e do controller.

### 18.2 Evidências de implementação e testes do MR-DEBT-003

Foram consolidadas as seguintes evidências:
- policies MedicalRecord foram definidas;
- perfil mínimo de usuário foi criado;
- claim `togo:profile` foi criada e emitida no JWT;
- policies foram registradas no `Program.cs`;
- `MedicalRecordsController` passou a usar policies por operação;
- matriz perfil x permissão foi implementada;
- testes diretos de autorização foram criados;
- testes do controller cobrem `401 Unauthorized`, `403 Forbidden` e acessos permitidos;
- CI passou nos PRs finais da sequência de implementação e evidências.

Arquivos principais envolvidos:
- `backend/src/Togo.Application/Security/MedicalRecordPermissions.cs`;
- `backend/src/Togo.Api/Security/MedicalRecordPolicies.cs`;
- `backend/src/Togo.Api/Security/MedicalRecordAuthorization.cs`;
- `backend/src/Togo.Domain/Security/UserProfiles.cs`;
- `backend/src/Togo.Domain/Security/TogoClaimTypes.cs`;
- `backend/src/Togo.Domain/Entities/User.cs`;
- `backend/src/Togo.Infrastructure/Tokens/JwtTokenService.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/UserConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260528120000_AddUserProfile.cs`;
- `backend/src/Togo.Api/Program.cs`;
- `backend/src/Togo.Api/Controllers/MedicalRecordsController.cs`;
- `backend/src/Togo.Api.Tests/Security/MedicalRecordAuthorizationTests.cs`;
- `backend/src/Togo.Api.Tests/MedicalRecords/MedicalRecordsControllerTests.cs`.

### 18.3 Riscos remanescentes e impacto na produção real

MR-DEBT-003 deixa de ser bloqueio ativo isolado. À época do encerramento da Fase 6.2, a vertical MedicalRecord ainda **não deveria ser usada com dados clínicos reais**, pois permaneciam abertos outros P1, incluindo MR-DEBT-002, MR-DEBT-004, MR-DEBT-001, MR-DEBT-005 e MR-DEBT-006.

Após a Fase 6.3, MR-DEBT-002 e MR-DEBT-004 foram resolvidos tecnicamente no escopo mínimo planejado. A liberação de produção real segue dependente da Fase 6.4 para tratar MR-DEBT-001, MR-DEBT-005 e MR-DEBT-006.

### 18.4 Encerramento e próxima fase

**Decisão:** Opção A — Fase 6.2 encerrada com MR-DEBT-003 tratado tecnicamente por autorização granular mínima baseada em profile JWT e policies ASP.NET Core.

**Próxima fase recomendada:** Fase 6.3 — Auditoria e autoria clínica, iniciando por **Fase 6.3.1 — Planejamento técnico de autoria clínica e auditoria MedicalRecord** para tratar MR-DEBT-004 e MR-DEBT-002.


## 19. Atualização viva — encerramento da Fase 6.3

### 19.1 MR-DEBT-004 — histórico e status atualizado

**Status anterior:** aberto, pendente e bloqueante para produção real.

**Novo status:** resolvido tecnicamente para autoria clínica mínima.

**Fases responsáveis pela solução:** 6.3.3, 6.3.3.1 e consolidação documental na 6.3.5.

**Resumo da solução:** `MedicalRecord` passou a persistir `CreatedByUserId`, `CreatedAt`, `UpdatedByUserId` e `UpdatedAt`. A criação preenche autoria de criação e de atualização com o usuário autenticado atual. A atualização preserva a autoria original de criação e altera apenas a autoria da última modificação.

**Observação de escopo:** a solução é técnica, incremental e limitada aos fluxos de criação/atualização de `MedicalRecord`. Não inclui auditoria de leitura, auditoria de acesso negado, endpoint público de auditoria, frontend de auditoria ou transação única com AuditLog.

### 19.2 MR-DEBT-002 — histórico e status atualizado

**Status anterior:** aberto, pendente e bloqueante para produção real.

**Novo status:** resolvido tecnicamente para AuditLog clínico mínimo.

**Fases responsáveis pela solução:** 6.3.4 e consolidação documental na 6.3.5.

**Resumo da solução:** foi criada a entidade/tabela `ClinicalAuditLogs` e um writer EF interno. Os use cases de criação e atualização de `MedicalRecord` registram `MedicalRecord.Created` e `MedicalRecord.Updated` com `UserId`, `UserProfile` quando disponível, `EntityName`, `EntityId`, `Action`, `OccurredAt` UTC e `MetadataJson` mínimo.

**Minimização de dados:** o AuditLog não armazena `GeneralNotes` completo nem `FlagsJson` completo. A metadata mínima atualmente registra apenas `PatientId`.

**Observação de escopo:** a solução não inclui auditoria de leitura, auditoria de acesso negado, endpoint público de auditoria, tela frontend de auditoria, retenção/expurgo ou transação única entre a operação principal e o AuditLog. Esses itens permanecem candidatos a débitos futuros, se necessário.

### 19.3 MR-DEBT-008 — correção documental e status atualizado

**Status anterior no registro vivo:** aberto como P2, com evidência de ausência de `CreatedAt`.

**Novo status:** resolvido tecnicamente pela Fase 6.3.3.

**Fases responsáveis pela solução:** 6.3.3 e consolidação documental nas Fases 6.3.5 e 6.3.6.

**Resumo da solução:** `MedicalRecord` passou a possuir `CreatedAt` persistido como parte da autoria clínica mínima. O campo é preenchido na criação e preservado na atualização, junto de `CreatedByUserId`, `UpdatedByUserId` e `UpdatedAt`.

**Impacto no backlog:** MR-DEBT-008 não permanece como P2 aberto nem como bloqueio ativo isolado para produção real. A rastreabilidade temporal de criação passou a existir no modelo persistido.

### 19.4 Impacto na produção real

MR-DEBT-004 e MR-DEBT-002 deixam de ser bloqueios ativos isolados após a Fase 6.3. Entretanto, a vertical MedicalRecord ainda não deve ser considerada pronta para produção real com dados clínicos sensíveis enquanto outros P1 permanecerem abertos, especialmente:

- MR-DEBT-001 — Soft Delete ausente;
- MR-DEBT-005 — Política de retenção não implementada;
- MR-DEBT-006 — `DeleteBehavior.Cascade` pendente de revisão.

### 19.5 Evidência documental final

A consolidação técnica e os critérios finais de aceite da Fase 6.3 estão registrados em:

- `docs/clinical-core/PHASE_06_03_05_MEDICAL_RECORD_AUTHORSHIP_AUDIT_EVIDENCE.md`;
- `docs/clinical-core/PHASE_06_03_06_MEDICAL_RECORD_AUTHORSHIP_AUDIT_CLOSURE.md`.
