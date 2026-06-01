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
| MR-DEBT-001 | Soft Delete ausente | Persistência/Compliance | P1 | Aberto | Não | Sim | Não há exclusão lógica em MedicalRecord. | Perda de histórico clínico se exclusão física for usada futuramente. | Não há endpoint DELETE no MVP. | 6.4.x — Soft Delete e persistência clínica. |
| MR-DEBT-002 | AuditLog ausente | Auditoria | P1 | Aberto | Não | Sim | Não há trilha de alterações clínicas. | Baixa rastreabilidade. | Logs técnicos sem payload sensível. | 6.3.x — Auditoria clínica. |
| MR-DEBT-003 | Roles/permissões finas ausentes | Segurança | P1 | Resolvido tecnicamente para autorização granular mínima | Não | Não, como bloqueio ativo isolado; produção real permanece bloqueada pelos demais P1 abertos. | Policies por operação, perfil mínimo e claim `togo:profile` aplicados ao fluxo MedicalRecord. | Risco original mitigado por autorização granular mínima; permanecem riscos clínicos dos demais P1. | Policies ASP.NET Core avaliadas por profile JWT, com matriz explícita e testes de autorização/controller. | Tratado nas fases 6.2.1 a 6.2.5; encerramento formal na 6.2.6. |
| MR-DEBT-004 | Controle de autoria ausente | Auditoria/Segurança | P1 | Aberto | Não | Sim | MedicalRecord não possui `CreatedBy/UpdatedBy`. | Não identificar autor de alteração clínica. | Autenticação existe, mas não persiste autoria. | 6.3.x — Autoria clínica. |
| MR-DEBT-005 | Política de retenção não implementada | Compliance/Governança | P1 | Aberto | Não | Sim | Diretriz existe, mecanismo técnico não. | Risco regulatório/operacional. | Ausência de fluxo de exclusão e documentação. | 6.4.x — Retenção clínica. |
| MR-DEBT-006 | DeleteBehavior.Cascade pendente de revisão | Persistência/Integridade clínica | P1 | Aberto | Não | Sim | Cascade registrado na configuração/migration. | Exclusão em cascata de dado clínico. | Sem endpoint DELETE. | 6.4.x — Revisão de integridade referencial. |
| MR-DEBT-007 | Índice único em MedicalRecords.PatientId ausente | Integridade/Banco | P2 | Aberto | Não | Sim | Unicidade lógica, não física. | Duplicidade em concorrência extrema. | Validator + use case. | 6.5.x — Hardening de schema. |
| MR-DEBT-008 | CreatedAt ausente | Rastreabilidade temporal | P2 | Aberto | Não | Parcialmente | Entidade possui UpdatedAt, mas não CreatedAt. | Menor clareza de criação do prontuário. | UpdatedAt presente. | 6.5.x — Evolução de schema. |
| MR-DEBT-009 | FlagsJson flexível | Modelagem/Validação | P2 | Aberto | Não | Parcialmente | Campo aceita string livre. | Inconsistência estrutural futura. | Débito documentado. | 6.5.x — Validação estrutural ou normalização. |
| MR-DEBT-010 | CancellationToken não propagado no repository | Operação/Resiliência | P3 | Aberto | Não | Não diretamente | IMedicalRecordRepository não recebe CancellationToken. | Menor capacidade de cancelamento em operações longas. | Métodos assíncronos. | 6.6.x — Qualidade operacional. |
| MR-DEBT-011 | Evidências manuais Swagger não versionadas formalmente | QA/Governança | P3 | Aberto | Não | Não diretamente | Fluxo validado visualmente, sem pacote formal versionado. | Menor rastreabilidade de QA manual. | Testes automatizados + prints/observações. | 6.6.x — Evidências operacionais. |
| MR-DEBT-012 | MedicalRecordListItemResponse ainda não usado | API/Privacidade futura | P3 | Aberto | Não | Não | Contract existe, mas não há endpoint de listagem. | Listagens futuras podem ignorar contrato seguro. | Não existe endpoint de listagem. | Consultas/listagens seguras futuras. |

## 8. Débitos P1 — obrigatórios antes de produção real

Itens P1 originalmente mapeados:
- Soft Delete;
- AuditLog;
- roles/permissões finas;
- controle de autoria;
- política de retenção;
- revisão DeleteBehavior.Cascade.

Após a Fase 6.2, **MR-DEBT-003 — Roles/permissões finas ausentes** está resolvido tecnicamente para o escopo mínimo de autorização granular MedicalRecord. Os demais P1 permanecem abertos e continuam impedindo recomendação de produção real com dados clínicos sensíveis, por envolverem riscos diretos de rastreabilidade, integridade clínica e compliance.

## 9. Débitos P2 — hardening técnico próximo

Itens P2 mapeados:
- índice único em PatientId;
- CreatedAt;
- validação estrutural/normalização de FlagsJson.

Esses itens aumentam robustez técnica de schema/modelagem e reduzem risco operacional de médio prazo, devendo ser tratados no ciclo de hardening subsequente ao fechamento dos bloqueios P1.

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
- MR-DEBT-002;
- MR-DEBT-004.

### 6.4 — Persistência clínica e retenção
- MR-DEBT-001;
- MR-DEBT-005;
- MR-DEBT-006.

### 6.5 — Integridade e evolução de schema
- MR-DEBT-007;
- MR-DEBT-008;
- MR-DEBT-009.

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
- CreatedAt;
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

MR-DEBT-003 deixa de ser bloqueio ativo isolado. Entretanto, a vertical MedicalRecord ainda **não deve ser usada com dados clínicos reais**, pois permanecem abertos outros P1:
- MR-DEBT-002 — AuditLog ausente;
- MR-DEBT-004 — Controle de autoria ausente;
- MR-DEBT-001 — Soft Delete ausente;
- MR-DEBT-005 — Política de retenção não implementada;
- MR-DEBT-006 — `DeleteBehavior.Cascade` pendente de revisão.

A liberação de produção real depende da continuidade das Fases 6.3 e 6.4.

### 18.4 Encerramento e próxima fase

**Decisão:** Opção A — Fase 6.2 encerrada com MR-DEBT-003 tratado tecnicamente por autorização granular mínima baseada em profile JWT e policies ASP.NET Core.

**Próxima fase recomendada:** Fase 6.3 — Auditoria e autoria clínica, iniciando por **Fase 6.3.1 — Planejamento técnico de autoria clínica e auditoria MedicalRecord** para tratar MR-DEBT-004 e MR-DEBT-002.
