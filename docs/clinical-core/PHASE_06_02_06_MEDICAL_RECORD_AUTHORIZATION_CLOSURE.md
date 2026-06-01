# PHASE 06.02.06 — MedicalRecord Authorization Closure

## 1. Contexto da Fase 6.2

A Fase 6.2 tratou o débito técnico **MR-DEBT-003 — Roles/permissões finas ausentes** da vertical MedicalRecord. Antes desta sequência, os endpoints dependiam de autorização genérica e ainda não possuíam autorização mínima granular por operação.

Esta Fase 6.2.6 é exclusivamente documental: atualiza o registro vivo da Fase 6.1.1, consolida as evidências produzidas nas Fases 6.2.1 a 6.2.5 e encerra formalmente a Fase 6.2.

## 2. Referência explícita ao MR-DEBT-003

**Status anterior:** aberto, pendente e bloqueante para produção real.

**Novo status:** resolvido tecnicamente para autorização granular mínima.

MR-DEBT-003 deixa de ser bloqueio ativo isolado. Essa mudança não libera a vertical MedicalRecord para produção real: os demais débitos P1 continuam abertos e ainda bloqueiam o uso com dados clínicos reais.

## 3. Sequência responsável pelo tratamento

| Fase | PR | Entrega consolidada |
|---|---:|---|
| 6.2.1 | PR 135 | Planejamento técnico da autorização granular MedicalRecord. |
| 6.2.2 | PR 136 | Criação centralizada das constantes de permissions e policies. |
| 6.2.3 | PR 137 | Profile em `User`, claim `togo:profile`, emissão no JWT e migration. |
| 6.2.4 | PR 138 | Registro das policies e aplicação por operação no controller. |
| 6.2.5 | PR 139 | Testes diretos de autorização e matriz de evidência. |
| 6.2.6 | Atual | Atualização do registro vivo, consolidação das evidências e encerramento formal. |

## 4. Resumo da solução

A autorização MedicalRecord deixou de depender apenas de `[Authorize]` genérico e passou a utilizar autorização granular mínima por operação com policies ASP.NET Core.

A solução é intencionalmente simples e explícita:
- permissões MedicalRecord foram centralizadas;
- policies MedicalRecord foram centralizadas;
- um conjunto mínimo de profiles de usuário foi criado;
- a claim própria `togo:profile` passou a ser emitida no JWT;
- as policies foram registradas no pipeline ASP.NET Core Authorization;
- o `MedicalRecordsController` passou a exigir policy específica para leitura, criação e atualização;
- uma matriz explícita de profile x permissão passou a orientar a decisão de acesso;
- testes automatizados passaram a cobrir a matriz diretamente e os resultados HTTP do controller.

A Fase 6.2 não introduziu RBAC complexo, tabelas dinâmicas de roles/permissões ou endpoint administrativo de gestão de perfis.

## 5. Status pós-Fase 6.2

Após a conclusão da Fase 6.2:
- MedicalRecord deixou de depender apenas de `[Authorize]` genérico;
- existe autorização granular mínima por operação;
- MR-DEBT-003 pode ser considerado tratado e resolvido tecnicamente;
- produção real continua bloqueada pelos demais débitos P1;
- a próxima fase deve seguir para auditoria e autoria clínica.

## 6. Evidências por PR

### PR 135 — Fase 6.2.1

Planejou a autorização granular MedicalRecord, registrou o risco de dependência exclusiva de `[Authorize]` genérico, definiu a abordagem mínima recomendada e estabeleceu os critérios futuros para tratar MR-DEBT-003.

### PR 136 — Fase 6.2.2

Criou as constantes centralizadas de permissions e policies MedicalRecord, evitando strings mágicas espalhadas e preparando o registro futuro no pipeline de autorização.

### PR 137 — Fase 6.2.3

Adicionou profile mínimo ao `User`, criou a claim própria `togo:profile`, passou a emitir essa claim no JWT e criou a migration correspondente para persistência do profile.

### PR 138 — Fase 6.2.4

Registrou as policies no `Program.cs`, implementou a avaliação centralizada da matriz profile x permissão e aplicou policy específica por operação no `MedicalRecordsController`.

### PR 139 — Fase 6.2.5

Criou testes diretos da autorização, consolidou a matriz de evidência e confirmou cobertura de respostas `401 Unauthorized`, `403 Forbidden` e acessos permitidos nos testes do controller. A CI passou nos PRs finais da sequência.

## 7. Evidências consolidadas de implementação e testes

As evidências finais da Fase 6.2 confirmam que:
- policies MedicalRecord foram definidas;
- perfil mínimo de usuário foi criado;
- claim `togo:profile` foi criada e emitida no JWT;
- policies foram registradas no `Program.cs`;
- `MedicalRecordsController` passou a usar policies por operação;
- matriz perfil x permissão foi implementada;
- testes diretos de autorização foram criados;
- testes do controller cobrem `401 Unauthorized`, `403 Forbidden` e acessos permitidos;
- CI passou nos PRs finais.

## 8. Arquivos principais alterados na Fase 6.2

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

## 9. Riscos remanescentes

Não foram resolvidos nesta fase:
- **MR-DEBT-002 — AuditLog ausente:** ainda não há trilha clínica suficiente de alterações;
- **MR-DEBT-004 — Controle de autoria ausente:** ainda não há persistência de autoria clínica;
- **MR-DEBT-001 — Soft Delete ausente:** ainda não há exclusão lógica de MedicalRecord;
- **MR-DEBT-005 — Política de retenção não implementada:** ainda não há mecanismo técnico de retenção clínica;
- **MR-DEBT-006 — `DeleteBehavior.Cascade` pendente de revisão:** ainda é necessária revisão da integridade referencial para dados clínicos.

## 10. Impacto na produção real

A Fase 6.2 melhora significativamente a segurança de acesso da vertical MedicalRecord e faz MR-DEBT-003 deixar de ser um bloqueio ativo isolado.

Entretanto, MedicalRecord ainda **não deve ser usado com dados clínicos reais** enquanto houver outros P1 abertos. A liberação para produção real depende da continuidade das Fases 6.3 e 6.4, responsáveis pelos próximos tratamentos de rastreabilidade, autoria, persistência clínica e retenção.

## 11. Decisão de encerramento da Fase 6.2

**Opção A — Fase 6.2 encerrada com MR-DEBT-003 tratado tecnicamente por autorização granular mínima baseada em profile JWT e policies ASP.NET Core.**

## 12. Próxima fase recomendada

**Fase 6.3 — Auditoria e autoria clínica.**

Objetivo macro: tratar os próximos débitos P1 relacionados à rastreabilidade clínica:
- MR-DEBT-004 — Controle de autoria ausente;
- MR-DEBT-002 — AuditLog ausente.

Recomendação de início:

**Fase 6.3.1 — Planejamento técnico de autoria clínica e auditoria MedicalRecord.**

## 13. Fora do escopo

Esta fase não:
- altera regra de autorização;
- altera controller;
- altera `Program.cs`;
- altera JWT;
- altera `User`;
- cria migration nova;
- altera banco;
- cria endpoint;
- cria `AuditLog`;
- implementa autoria clínica;
- implementa Soft Delete;
- implementa retenção;
- altera frontend;
- altera Docker, Redis, RabbitMQ ou Kubernetes.

## 14. Critérios de aceite

A Fase 6.2.6 atende aos critérios de aceite quando:
- o registro vivo está atualizado;
- MR-DEBT-003 está marcado como tratado e resolvido tecnicamente;
- as Fases 6.2.1 a 6.2.5 estão referenciadas;
- os PRs 135 a 139 estão referenciados;
- as evidências de implementação e testes estão documentadas;
- os riscos remanescentes estão documentados;
- o bloqueio de produção real pelos demais P1 está mantido;
- este documento de encerramento existe;
- nenhuma implementação nova foi feita;
- `dotnet build backend/Togo.sln` passa;
- `dotnet test backend/Togo.sln` passa.
