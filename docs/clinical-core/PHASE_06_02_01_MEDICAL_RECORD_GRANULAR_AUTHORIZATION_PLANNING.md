# TOGO — Fase 6.2.1: Planejamento da autorização granular da vertical MedicalRecord

## 1. Reabertura do contexto da Fase 6.2

Fase 6.2 — Segurança e autorização granular da vertical MedicalRecord.

Débito focal desta fase:
- MR-DEBT-003 — Roles/permissões finas ausentes.

Diretriz de governança mantida:
- produção real continua bloqueada enquanto MR-DEBT-003 permanecer em aberto.

## 2. Objetivo da Fase 6.2.1

Planejar tecnicamente a autorização granular da vertical MedicalRecord antes de qualquer implementação.

Objetivos específicos:
- evitar implementação improvisada de roles/policies;
- definir estratégia segura de autorização para MedicalRecord;
- estabelecer base técnica para implementação futura (6.2.2 ou fase equivalente);
- manter escopo exclusivamente documental nesta etapa.

## 3. Fontes principais desta fase

- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`;
- `docs/clinical-core/PHASE_06_01_02_MEDICAL_RECORD_PRODUCTION_READINESS_PRIORIZATION.md`;
- `docs/clinical-core/PHASE_06_01_03_MEDICAL_RECORD_HARDENING_GOVERNANCE_CLOSURE.md`;
- `backend/src/Togo.Api/Controllers/MedicalRecordsController.cs`;
- `backend/src/Togo.Api/Program.cs`;
- `backend/src/Togo.Infrastructure/Tokens/JwtTokenService.cs`;
- `backend/src/Togo.Domain/Entities/User.cs`;
- `backend/src/Togo.Api.Tests/MedicalRecords/MedicalRecordsControllerTests.cs`.

## 4. Estado atual da autorização MedicalRecord

Estado documentado da baseline atual:
- `MedicalRecordsController` usa apenas `[Authorize]` genérico;
- não há policies específicas para operações da vertical MedicalRecord;
- o token JWT não carrega claims de role/perfil/permissão;
- a entidade `User` não possui campo dedicado de role/perfil/permissão;
- os testes atuais da API validam autenticação, mas não autorização granular por perfil/operação.

## 5. Risco atual

Risco de segurança/produção na situação atual:
- qualquer usuário autenticado pode acessar operações MedicalRecord;
- não há distinção de autorização entre leitura, criação e atualização;
- não há separação por perfil clínico e administrativo;
- este cenário bloqueia uso em produção real com dados clínicos sensíveis.

## 6. Modelo mínimo de autorização recomendado

Proposta inicial (planejamento, sem implementação nesta fase):

Perfis mínimos recomendados:
- Admin;
- Veterinarian;
- Assistant;
- Reception;
- ReadOnly/Auditor.

Permissões mínimas recomendadas:
- `MedicalRecord.Read`;
- `MedicalRecord.Create`;
- `MedicalRecord.Update`.

## 7. Matriz de permissões recomendada

| Perfil | Pode ler prontuário? | Pode criar prontuário? | Pode atualizar prontuário? | Justificativa | Observações de segurança |
|---|---|---|---|---|---|
| Admin | Sim | Sim | Sim | Perfil de administração com responsabilidade de supervisão e contingência operacional. | Exigir rastreabilidade futura de acesso/ação e revisão periódica de concessão. |
| Veterinarian | Sim | Sim | Sim | Perfil clínico principal para registro e evolução de conteúdo médico. | Deve operar sob princípio de menor privilégio fora da vertical clínica. |
| Assistant | Sim | Não (por padrão) | Não (por padrão) | Necessidade frequente de consulta de contexto clínico, sem autoria clínica primária nesta proposta mínima. | Exceções futuras devem ser explícitas, auditáveis e justificadas por processo. |
| Reception | Não | Não | Não | Perfil administrativo/comercial sem necessidade operacional de acesso ao conteúdo clínico sensível. | Manter bloqueio por padrão e evitar exposição indireta em telas, logs e testes. |
| ReadOnly/Auditor | Sim (condicional) | Não | Não | Leitura restrita para auditoria/controle quando houver necessidade operacional comprovada. | Acesso condicionado a rastreabilidade reforçada e política formal de necessidade de saber. |

## 8. Estratégia técnica recomendada

Abordagem recomendada para implementação futura:
- adotar **Authorization Policies** do ASP.NET Core como mecanismo principal de autorização;
- utilizar claims no JWT para transportar role/perfil e/ou permissões mínimas necessárias;
- evitar hardcode de regras diretamente nos controllers;
- centralizar nomes de policies/permissões em constantes ou classe dedicada;
- preparar evolução de `User` e `JwtTokenService` de forma controlada e compatível com segurança;
- evitar introdução prematura de modelagem RBAC complexa, salvo necessidade objetiva.

## 9. Impactos técnicos previstos

Impactos esperados para fases futuras:
- `Domain/User` pode precisar evoluir para incluir Role/Profile (ou estrutura equivalente mínima);
- `JwtTokenService` pode precisar emitir claim de role/permissão;
- `Program.cs` pode precisar registrar policies de MedicalRecord;
- `MedicalRecordsController` pode substituir `[Authorize]` genérico por policies por operação;
- testes de API devem cobrir cenários 401 (não autenticado) e 403 (autenticado sem permissão);
- seeds/factories de teste podem precisar gerar usuários com perfis distintos para validar matriz de acesso.

## 10. Plano recomendado de implementação futura

Divisão sugerida para execução controlada:
1. **6.2.2 — Definição de contracts/constantes de policies e permissões.**
2. **6.2.3 — Evolução de User/JWT para suportar perfil/claim mínima.**
3. **6.2.4 — Aplicação de policies no `MedicalRecordsController` por operação.**
4. **6.2.5 — Criação/ajuste de testes de autorização granular.**
5. **6.2.6 — Atualização documental, registro vivo e evidências.**

## 11. Critérios de aceite futuros para resolver MR-DEBT-003

MR-DEBT-003 só poderá ser marcado como resolvido quando:
- houver modelo mínimo de perfil/permissão definido;
- JWT carregar informação suficiente para autorização;
- policies forem registradas no `Program.cs`;
- `MedicalRecordsController` usar policies por operação;
- testes cobrirem acesso permitido e bloqueado;
- cenários 401 e 403 forem validados;
- documentação da fase correspondente for atualizada;
- registro vivo da 6.1.1 for atualizado com fase/PR/status;
- não houver exposição de payload clínico em logs/testes/documentação.

## 12. Fora do escopo

Esta fase não implementa:
- código;
- testes;
- migrations;
- database update;
- alteração em `User`;
- alteração em `JwtTokenService`;
- alteração em `Program.cs`;
- alteração em `MedicalRecordsController`;
- criação de roles reais;
- criação de permissões reais;
- alteração em appsettings;
- alteração em workflows;
- frontend;
- Docker;
- Redis;
- RabbitMQ;
- Kubernetes.

## 13. Critérios de aceite da Fase 6.2.1

A fase será considerada concluída se:
- o documento for criado no caminho correto;
- MR-DEBT-003 for explicitamente referenciado;
- o estado atual da autorização for documentado;
- o risco atual for descrito;
- o modelo mínimo recomendado de perfis/permissões for definido;
- a matriz de permissões for criada;
- a estratégia técnica recomendada for documentada;
- os impactos técnicos previstos forem listados;
- o plano de implementação futura for definido;
- os critérios futuros para resolver MR-DEBT-003 forem definidos;
- nenhuma implementação for feita;
- o escopo permanecer exclusivamente documental.

## 14. Decisão da fase

**Opção A — Planejamento técnico da autorização granular aprovado como base para implementação futura de MR-DEBT-003.**

## 15. Próxima fase recomendada

**Fase 6.2.2 — Definição de policies/permissões MedicalRecord.**

Objetivo da 6.2.2:
Criar a base técnica inicial para autorização granular, preferencialmente por meio de constantes/classes de policies e permissões, com escopo controlado e sem modelagem RBAC complexa.

## 16. Validações obrigatórias

Comandos obrigatórios desta fase:
- `git branch --show-current`;
- `git status --short`;
- `git diff --check`;
- `dotnet build backend/Togo.sln`;
- `dotnet test backend/Togo.sln`.

Observação:
- esta fase permanece exclusivamente documental e não implementa autorização granular.
