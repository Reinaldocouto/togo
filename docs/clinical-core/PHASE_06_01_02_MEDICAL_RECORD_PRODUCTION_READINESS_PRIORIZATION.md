# TOGO — Fase 6.1.2: Priorização P1/P2/P3 e critérios de produção segura da vertical MedicalRecord

## 2. Resumo da Subfase 6.1

Subfase 6.1 — Governança do hardening e registro de débitos

Planejamento:
- 6.1.1 — Registro vivo de débitos técnicos e plano de hardening.
- 6.1.2 — Priorização P1/P2/P3 e critérios de produção segura.
- 6.1.3 — Documentação final da governança do hardening.
- 6.1.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## 3. Objetivo da Fase 6.1.2

Esta fase transforma o registro vivo de débitos técnicos da vertical MedicalRecord em uma matriz de priorização executável para hardening.

Objetivos específicos:
- definir objetivamente o que bloqueia produção real;
- definir os critérios mínimos de produção segura para MedicalRecord;
- definir a ordem recomendada de tratamento dos débitos P1;
- separar claramente MVP técnico de produção real;
- preparar base documental para abertura das fases práticas 6.2, 6.3 e 6.4.

## 4. Fontes principais desta fase

- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`;
- `docs/clinical-core/PHASE_05_06_02_MEDICAL_RECORD_RISK_AND_TECH_DEBT_MATRIX.md`;
- `docs/clinical-core/PHASE_05_06_03_PHASE_5_MEDICAL_RECORD_EXECUTIVE_CLOSURE.md`.

## 5. Matriz de priorização executável de débitos (P1/P2/P3)

| ID do débito | Nome do débito | Prioridade atual | Impacto em produção real | Risco principal | Ordem recomendada de execução | Dependências técnicas | Fase futura recomendada | Critério de aceite para considerar resolvido |
|---|---|---|---|---|---|---|---|---|
| MR-DEBT-003 | Roles/permissões finas ausentes | P1 | Bloqueia produção real | Acesso não granular por perfil clínico | 1 | Modelo de papéis/perfis, políticas de autorização, revisão de endpoints protegidos | 6.2.x — Segurança/autorização granular | Perfis clínicos definidos; políticas por operação aplicadas; evidência de bloqueio de acesso indevido validada em testes automatizados e validação manual. |
| MR-DEBT-004 | Controle de autoria ausente | P1 | Bloqueia produção real | Impossibilidade de identificar autor de alteração clínica | 2 | Contexto de identidade autenticada, persistência de `CreatedBy/UpdatedBy`, contrato de atualização de autoria | 6.3.x — Autoria clínica | Criação e alteração persistem autoria de forma consistente; trilha de autoria visível em consultas técnicas/auditoria; evidência de testes e validação manual. |
| MR-DEBT-002 | AuditLog ausente | P1 | Bloqueia produção real | Baixa rastreabilidade clínica | 3 | Eventos clínicos mapeados, store de auditoria, correlação com identidade/autoria | 6.3.x — Auditoria clínica | Eventos relevantes registram trilha mínima (quem, quando, ação, alvo); logs sem payload sensível; evidência de cobertura automatizada e validação operacional. |
| MR-DEBT-006 | DeleteBehavior.Cascade pendente de revisão | P1 | Bloqueia produção real | Exclusão em cascata de dados clínicos por configuração referencial | 4 | Revisão de relacionamentos EF, decisão explícita de comportamento referencial, testes de integridade | 6.4.x — Revisão de integridade referencial | Todos os relacionamentos clínicos críticos revisados e documentados; comportamento de exclusão aprovado; testes impedem perda indevida de histórico. |
| MR-DEBT-001 | Soft Delete ausente | P1 | Bloqueia produção real | Perda de histórico clínico em exclusão física futura | 5 | Estratégia de exclusão lógica, filtros de consulta, alinhamento com retenção | 6.4.x — Soft Delete e persistência clínica | Exclusão lógica implementada ou política equivalente aprovada; consultas respeitam preservação histórica; evidência de testes em ciclo de vida do dado clínico. |
| MR-DEBT-005 | Política de retenção não implementada | P1 | Bloqueia produção real | Risco regulatório e operacional de ciclo de vida de dado clínico | 6 | Catálogo de retenção por tipo de dado, regras de arquivamento/expurgo controlado, alinhamento com Soft Delete | 6.4.x — Retenção clínica | Política mínima formalizada e aplicada ao domínio; fluxo técnico compatível com preservação histórica; evidência de revisão técnica e governança. |
| MR-DEBT-007 | Índice único em `MedicalRecords.PatientId` ausente | P2 | Pode comprometer integridade em produção real | Duplicidade em concorrência extrema | 7 | Migration de índice único, estratégia para saneamento prévio de duplicidades | 6.5.x — Hardening de schema | Restrição física de unicidade aplicada e validada; cenários de concorrência crítica testados. |
| MR-DEBT-008 | `CreatedAt` ausente | P2 | Impacto parcial em governança de produção | Rastreabilidade temporal incompleta | 8 | Evolução de schema e contratos de leitura/escrita temporal | 6.5.x — Evolução de schema | Campo `CreatedAt` persistido e populado corretamente; evidência de consistência temporal em testes. |
| MR-DEBT-009 | `FlagsJson` flexível | P2 | Impacto parcial em consistência de produção | Inconsistência estrutural futura | 9 | Especificação de schema/validação estrutural, estratégia de compatibilidade com payloads existentes | 6.5.x — Validação estrutural ou normalização | Estrutura validada ou normalizada conforme contrato; rejeição de payload inválido evidenciada em testes. |
| MR-DEBT-010 | `CancellationToken` não propagado no repository | P3 | Não bloqueia diretamente produção real | Menor resiliência/cancelamento em operações longas | 10 | Ajuste de contratos de repository/use cases e propagação de cancelamento | 6.6.x — Qualidade operacional | Contratos e chamadas propagam `CancellationToken`; cenários de cancelamento exercitados em testes técnicos. |
| MR-DEBT-011 | Evidências manuais Swagger não versionadas formalmente | P3 | Não bloqueia diretamente produção real | Menor rastreabilidade de QA manual | 11 | Padrão de evidências operacionais e versionamento documental | 6.6.x — Evidências operacionais | Pacote mínimo de evidências manuais versionado e referenciado por fase/PR. |
| MR-DEBT-012 | `MedicalRecordListItemResponse` ainda não usado | P3 | Não bloqueia produção atual | Futuras listagens podem ignorar contrato seguro | 12 | Estratégia de endpoint de listagem segura e privacidade por contrato | Consultas/listagens seguras futuras | Endpoint de listagem futura utiliza contrato seguro e validações de privacidade. |

## 6. Destaque obrigatório dos P1

Débitos P1 obrigatórios antes de produção real:
- MR-DEBT-003 — Roles/permissões finas ausentes;
- MR-DEBT-002 — AuditLog ausente;
- MR-DEBT-004 — Controle de autoria ausente;
- MR-DEBT-001 — Soft Delete ausente;
- MR-DEBT-006 — `DeleteBehavior.Cascade` pendente de revisão;
- MR-DEBT-005 — Política de retenção não implementada.

## 7. Ordem técnica recomendada para os P1

Ordem recomendada:
1. segurança/autorização granular (MR-DEBT-003);
2. autoria clínica (MR-DEBT-004);
3. AuditLog (MR-DEBT-002);
4. revisão de Cascade (MR-DEBT-006);
5. Soft Delete (MR-DEBT-001);
6. política de retenção (MR-DEBT-005).

Justificativa da ordem:
- não faz sentido implementar auditoria/autoria sem saber quem pode acessar o quê;
- não faz sentido liberar produção real com `[Authorize]` genérico sem granularidade por perfil;
- não faz sentido tratar retenção/exclusão antes de revisar comportamento de exclusão e integridade referencial;
- Soft Delete e retenção devem nascer alinhados à preservação de histórico clínico;
- AuditLog e autoria são obrigatórios para rastreabilidade clínica em uso real.

## 8. Critérios mínimos de produção segura (MedicalRecord)

Para habilitar produção real com dados clínicos sensíveis, o mínimo exigido é:
- autenticação obrigatória;
- autorização granular por perfil;
- registro de autoria em criação e alteração;
- AuditLog de eventos clínicos relevantes;
- ausência de exclusão física insegura;
- Soft Delete ou política equivalente de preservação histórica;
- revisão de Cascade em dados clínicos;
- política mínima de retenção documentada;
- logs sem payload clínico sensível;
- dados sensíveis não versionados;
- evidências mínimas de testes e validação manual;
- banco com integridade suficiente para evitar duplicidade crítica.

## 9. MVP técnico x produção real x produção segura futura

### MVP técnico
Permitido para:
- demonstração controlada;
- estudo e evolução arquitetural;
- validação funcional em ambiente restrito.

### Produção real
Não permitida enquanto os débitos P1 estiverem abertos.

### Produção segura futura
Possível apenas após:
- resolução dos bloqueios P1;
- documentação de evidências técnicas e operacionais;
- confirmação objetiva dos critérios mínimos de produção segura.

## 10. Definition of Production Ready

MedicalRecord será considerado apto para uso real com dados clínicos sensíveis somente quando:
- todos os débitos P1 estiverem resolvidos com evidência objetiva;
- autorização granular por perfil estiver implementada e validada;
- autoria clínica (`CreatedBy/UpdatedBy`) estiver persistida e auditável;
- AuditLog clínico relevante estiver ativo e sem vazamento de payload sensível;
- estratégia de exclusão estiver segura (sem exclusão física insegura);
- Soft Delete (ou equivalente aprovado) estiver operacional;
- comportamento referencial (incluindo Cascade) estiver revisado e testado;
- política mínima de retenção estiver formalizada e tecnicamente suportada;
- evidências de testes automatizados e validação manual estiverem registradas;
- integridade de banco para evitar duplicidade crítica estiver assegurada.

## 11. Pacote mínimo de hardening inicial

Pacote mínimo recomendado para início prático do hardening:
- **6.2 — Segurança e autorização granular** (base para controle de acesso por perfil);
- **6.3 — Auditoria e autoria clínica** (rastreabilidade de autoria e eventos);
- **6.4 — Persistência clínica e retenção** (integridade referencial, preservação histórica e ciclo de vida de dado clínico).

## 12. Fora do escopo desta fase

Esta fase **não implementa**:
- código;
- testes;
- migrations;
- database update;
- banco real;
- novos endpoints;
- roles/permissões;
- AuditLog;
- Soft Delete;
- `CreatedAt`;
- índice único;
- alteração em `Program.cs`;
- alteração em `appsettings`;
- alteração em workflows;
- frontend;
- Docker;
- Redis;
- RabbitMQ;
- Kubernetes.

## 13. Critérios de aceite da Fase 6.1.2

A fase será considerada concluída se:
- o documento for criado no caminho correto;
- os débitos P1/P2/P3 forem priorizados;
- a ordem dos P1 for definida;
- os critérios mínimos de produção segura forem definidos;
- a diferença entre MVP técnico e produção real for documentada;
- a seção “Definition of Production Ready” for criada;
- o pacote mínimo de hardening inicial for recomendado;
- nenhuma implementação for feita;
- o escopo permanecer exclusivamente documental.

## 14. Decisão da fase

**Opção A — Priorização P1/P2/P3 e critérios de produção segura aprovados como base para abertura das fases práticas de hardening.**

## 15. Próxima fase recomendada

**Fase 6.1.3 — Documentação final da governança do hardening.**

Objetivo da 6.1.3:
Consolidar a governança da Subfase 6.1, registrando como o hardening será controlado, atualizado, auditado e conectado às fases práticas 6.2, 6.3, 6.4, 6.5 e 6.6.

## 16. Validações obrigatórias

Comandos esperados para validação desta fase:
- `git branch --show-current`;
- `git status --short`;
- `git diff --check`;
- `dotnet build backend/Togo.sln`, se o ambiente tiver `dotnet` disponível;
- `dotnet test backend/Togo.sln`, se o ambiente tiver `dotnet` disponível.

Observação:
- esta fase permanece exclusivamente documental e não realiza implementação técnica de hardening.
