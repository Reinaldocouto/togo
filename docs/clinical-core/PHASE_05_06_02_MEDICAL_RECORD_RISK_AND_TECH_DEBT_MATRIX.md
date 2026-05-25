# TOGO — Fase 5.6.2: Matriz de riscos e débitos técnicos da vertical MedicalRecord

## 1. Título

TOGO — Fase 5.6.2: Matriz de riscos e débitos técnicos da vertical MedicalRecord

## 2. Resumo da Subfase 5.6

Subfase 5.6 — Auditoria e fechamento da vertical MedicalRecord

Planejamento:
- 5.6.1 — Auditoria final da vertical MedicalRecord.
- 5.6.2 — Matriz de riscos e débitos técnicos da vertical MedicalRecord.
- 5.6.3 — Documentação final executiva da Fase 5.
- 5.6.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## 3. Objetivo

Esta fase consolida os riscos e débitos técnicos da vertical MedicalRecord em uma matriz única e executável para priorização futura.

A matriz deve responder de forma objetiva:
- o que está pendente;
- por que está pendente;
- qual o impacto;
- qual a severidade;
- se bloqueia ou não o MVP;
- qual prioridade futura;
- qual fase futura recomendada.

## 4. Contexto

- A vertical MedicalRecord foi aprovada preliminarmente para MVP técnico.
- A aprovação depende da manutenção formal dos débitos rastreados.
- Os principais débitos concentram-se em segurança, auditoria, compliance, persistência e governança.
- Esta fase não implementa correções.
- Esta fase organiza os riscos para tomada de decisão futura.

## 5. Critérios de classificação

### 5.1 Severidade

- **Alta**: risco relevante de segurança, privacidade, rastreabilidade, compliance ou perda de dado clínico.
- **Média**: risco técnico ou operacional importante, mas com mitigação parcial no MVP.
- **Baixa**: melhoria desejável, sem impacto imediato relevante no MVP.

### 5.2 Prioridade

- **P1**: deve ser tratado antes de uso real/produção.
- **P2**: deve ser tratado em fase de hardening próxima.
- **P3**: pode ser tratado em evolução planejada posterior.

### 5.3 Bloqueia MVP?

- **Sim**: impede considerar a vertical pronta para demonstração/MVP técnico.
- **Não**: pode seguir como débito rastreado.

## 6. Matriz consolidada de riscos e débitos técnicos

| ID | Risco/Débito | Categoria | Evidência | Impacto | Severidade | Prioridade | Bloqueia MVP? | Mitigação atual | Fase futura recomendada |
|---|---|---|---|---|---|---|---|---|---|
| MR-RISK-001 | Soft Delete ausente | Persistência/Compliance | Não há exclusão lógica implementada em MedicalRecord. | Risco de perda de histórico clínico se exclusão física for habilitada futuramente. | Alta | P1 | Não, porque não há endpoint DELETE no MVP. | Ausência de endpoint DELETE e débito documentado. | Hardening clínico/persistência. |
| MR-RISK-002 | AuditLog ausente | Auditoria/Rastreabilidade | Não há trilha de quem alterou o prontuário. | Baixa rastreabilidade clínica. | Alta | P1 | Não. | Logs técnicos sem payload sensível. | Auditoria clínica. |
| MR-RISK-003 | Roles/permissões finas ausentes | Segurança/Autorização | Endpoints usam `[Authorize]` genérico. | Controle insuficiente por perfil clínico. | Alta | P1 | Não para MVP técnico; sim antes de produção real. | Autenticação obrigatória. | Segurança/autorização granular. |
| MR-RISK-004 | Índice único em `MedicalRecords.PatientId` ausente | Banco/Integridade | Unicidade é lógica, não física. | Duplicidade em concorrência extrema. | Média | P2 | Não. | Validator de unicidade + use case. | Migration/hardening de schema. |
| MR-RISK-005 | `DeleteBehavior.Cascade` pendente de revisão | Banco/Integridade clínica | Cascade delete registrado na configuração/migration. | Risco de exclusão em cascata de dados clínicos. | Média/Alta | P1 | Não, porque não há delete físico exposto. | Ausência de endpoint DELETE. | Revisão de integridade referencial. |
| MR-RISK-006 | Controle de autoria ausente | Auditoria/Segurança | MedicalRecord não possui `CreatedBy/UpdatedBy`. | Não identifica autor da alteração clínica. | Alta | P1 | Não. | Autenticação existe, mas não persiste autoria. | Auditoria/autoria clínica. |
| MR-RISK-007 | `CreatedAt` ausente | Rastreabilidade temporal | Entidade tem `UpdatedAt`, mas não `CreatedAt`. | Menor clareza do início do prontuário. | Baixa/Média | P2 | Não. | `UpdatedAt` presente. | Evolução de schema. |
| MR-RISK-008 | `FlagsJson` flexível | Modelagem/Validação | `FlagsJson` aceita string livre. | Inconsistência estrutural futura. | Média | P2 | Não. | Débito técnico documentado. | Validação estrutural ou normalização. |
| MR-RISK-009 | `CancellationToken` não propagado no repository | Performance/Resiliência | `IMedicalRecordRepository` não recebe `CancellationToken`. | Menor capacidade de cancelamento em operações longas. | Baixa | P3 | Não. | Uso assíncrono dos métodos. | Refino técnico de repository. |
| MR-RISK-010 | Política de retenção ainda não implementada | Compliance/Governança | Diretriz documentada, sem mecanismo técnico. | Risco regulatório/operacional em uso real. | Alta | P1 | Não para MVP técnico; sim antes de produção real. | Ausência de fluxo de exclusão e documentação. | Compliance/retenção. |
| MR-RISK-011 | Validação manual Swagger com evidências parciais | QA/Operação | Fluxo principal validado visualmente, mas sem pacote formal completo de evidências versionadas. | Menor rastreabilidade de QA manual. | Baixa | P3 | Não. | Testes automatizados de API + prints/observações. | Governança de evidências. |
| MR-RISK-012 | `MedicalRecordListItemResponse` ainda não usado | API/Privacidade futura | Contract existe, mas não há endpoint de listagem. | Listagens futuras podem ser implementadas de forma menos segura se ignorarem o contract. | Baixa | P3 | Não. | Não existe endpoint de listagem. | Consultas/listagens seguras. |

Justificativa de priorização de MR-RISK-005:
- Foi classificado como **P1** por envolver potencial impacto direto em integridade clínica caso haja evolução com exclusão física sem revisão do comportamento referencial.
- Apesar de não bloquear o MVP técnico (sem endpoint DELETE), deve ser tratado antes de produção real.

## 7. Priorização recomendada

### P1 — Antes de produção real
- Soft Delete;
- AuditLog;
- roles/permissões finas;
- controle de autoria;
- política de retenção;
- revisão `DeleteBehavior.Cascade`.

### P2 — Hardening técnico próximo
- índice único em `PatientId`;
- `CreatedAt`;
- validação estrutural de `FlagsJson`.

### P3 — Evolução técnica
- `CancellationToken`;
- evidências manuais versionadas;
- uso futuro de `MedicalRecordListItemResponse`.

## 8. Riscos que NÃO bloqueiam o MVP técnico

Os riscos abaixo foram aceitos como não bloqueantes para MVP técnico porque:
- não há endpoint `DELETE`;
- endpoints exigem `[Authorize]`;
- unicidade é validada logicamente;
- testes cobrem fluxos principais;
- API já foi validada automatizadamente;
- documentação deixa débitos explícitos.

Riscos aceitos como não bloqueantes do MVP técnico:
- MR-RISK-001;
- MR-RISK-002;
- MR-RISK-003 (somente para MVP técnico);
- MR-RISK-004;
- MR-RISK-005;
- MR-RISK-006;
- MR-RISK-007;
- MR-RISK-008;
- MR-RISK-009;
- MR-RISK-010 (somente para MVP técnico);
- MR-RISK-011;
- MR-RISK-012.

## 9. Riscos que bloqueiam produção real

Para uso real/produção, os itens abaixo devem ser tratados previamente:
- Soft Delete;
- AuditLog;
- roles/permissões finas;
- controle de autoria;
- política de retenção;
- revisão de exclusão/cascade.

Assim, MR-RISK-001, MR-RISK-002, MR-RISK-003, MR-RISK-005, MR-RISK-006 e MR-RISK-010 são considerados bloqueantes para produção real.

## 10. Recomendações de roadmap futuro

### Hardening 1 — Segurança e auditoria
- roles/permissões;
- AuditLog;
- autoria.

### Hardening 2 — Persistência clínica
- Soft Delete;
- revisão Cascade;
- política de retenção.

### Hardening 3 — Integridade e schema
- índice único `PatientId`;
- `CreatedAt`;
- `FlagsJson` validado/normalizado.

### Hardening 4 — Operação e qualidade
- `CancellationToken`;
- evidências manuais versionadas;
- testes adicionais de concorrência.

## 11. Decisão da Fase 5.6.2

**Opção A — Matriz de riscos aprovada para fechamento executivo.**

Justificativa:
Os riscos estão identificados, classificados, priorizados e não impedem o fechamento do MVP técnico, desde que permaneçam como débitos formais para fases futuras.

## 12. Critérios de aceite

A fase será considerada concluída se:
- matriz de riscos for criada;
- todos os achados da 5.6.1 forem considerados;
- severidade for definida;
- prioridade for definida;
- bloqueio de MVP for indicado;
- bloqueio de produção real for separado;
- roadmap futuro for sugerido;
- nenhuma implementação for feita;
- documento da matriz for criado.

## 13. Fora do escopo

Esta fase não implementa:
- código;
- testes;
- migration;
- database update;
- banco real;
- novos endpoints;
- Soft Delete;
- AuditLog;
- índice único;
- roles/permissões finas;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## 14. Próxima fase recomendada

**Fase 5.6.3 — Documentação final executiva da Fase 5.**

Objetivo:
Consolidar toda a Fase 5 em um documento executivo final, resumindo o que foi entregue, evidências de funcionamento, testes, decisões arquiteturais, riscos aceitos, débitos técnicos e recomendação de encerramento oficial da Fase 5.

## 15. Validações obrigatórias

Comandos executados:
- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln` (se disponível)
- `dotnet test backend/Togo.sln` (se disponível)

Observação:
- esta fase é exclusivamente documental e não realiza implementação técnica dos débitos listados.
