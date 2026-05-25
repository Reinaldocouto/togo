# TOGO — Fase 6.1.3: Documentação final da governança do hardening da vertical MedicalRecord

## 2. Reabertura do contexto da Subfase 6.1

Subfase 6.1 — Governança do hardening e registro de débitos.

Planejamento consolidado:
- 6.1.1 — Registro vivo de débitos técnicos e plano de hardening.
- 6.1.2 — Priorização P1/P2/P3 e critérios de produção segura.
- 6.1.3 — Documentação final da governança do hardening.
- 6.1.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional.

## 3. Objetivo da Fase 6.1.3

Encerrar formalmente a Subfase 6.1, consolidando a governança documental do hardening da vertical MedicalRecord.

Objetivos específicos:
- consolidar as decisões da 6.1.1 e da 6.1.2;
- definir como os débitos MR-DEBT serão controlados;
- definir como as fases práticas 6.2, 6.3, 6.4, 6.5 e 6.6 devem referenciar o registro vivo;
- definir regras mínimas de evidência para considerar débito resolvido;
- manter a decisão de que MedicalRecord é MVP técnico, mas não está aprovado para produção real com dados clínicos sensíveis.

## 4. Fontes principais desta fase

- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`;
- `docs/clinical-core/PHASE_06_01_02_MEDICAL_RECORD_PRODUCTION_READINESS_PRIORIZATION.md`;
- `docs/clinical-core/PHASE_05_06_02_MEDICAL_RECORD_RISK_AND_TECH_DEBT_MATRIX.md`;
- `docs/clinical-core/PHASE_05_06_03_PHASE_5_MEDICAL_RECORD_EXECUTIVE_CLOSURE.md`.

## 5. Consolidação da Subfase 6.1

- A Fase 6.1.1 criou o registro vivo oficial de débitos técnicos da vertical MedicalRecord.
- A Fase 6.1.2 transformou o registro vivo em matriz executável de priorização P1/P2/P3 e critérios de produção segura.
- A Fase 6.1.3 consolida a governança final, as regras de controle e a forma de atualização contínua do hardening.

Com esta fase, a Subfase 6.1 é formalmente encerrada como trilha documental de preparação para as fases práticas.

## 6. Governança oficial do hardening MedicalRecord

Regras oficiais a partir do encerramento da Subfase 6.1:
- o registro vivo (`PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`) é a fonte oficial dos débitos MR-DEBT;
- toda fase futura que resolver débito deve citar explicitamente o ID MR-DEBT correspondente;
- todo PR que resolver débito deve atualizar o registro vivo;
- débito resolvido não deve ser removido do registro vivo;
- débito resolvido deve manter histórico, fase responsável, PR responsável e resumo da solução;
- novos débitos devem receber ID sequencial;
- mudanças de prioridade devem ser justificadas documentalmente;
- produção real continua bloqueada enquanto houver qualquer P1 aberto.

## 7. Regras para atualização do registro vivo

Para cada débito tratado, o registro vivo deve incluir, no mínimo:
- status atualizado;
- fase responsável;
- PR responsável;
- resumo da solução;
- impacto da solução;
- evidências de teste;
- evidências manuais, quando aplicável;
- riscos remanescentes, se existirem.

Diretriz complementar:
- se o débito não for resolvido e houver mudança de decisão (adiamento, substituição, recorte), a justificativa deve ficar explícita para preservar rastreabilidade histórica.

## 8. Definition of Done para resolução de débito

Um débito MR-DEBT só pode ser considerado resolvido se todos os critérios abaixo forem atendidos:
- implementação técnica correspondente existir, ou decisão documental equivalente existir quando não houver código;
- testes automatizados forem atualizados quando houver alteração de código;
- build passar;
- test suite passar;
- documentação da fase for atualizada;
- registro vivo for atualizado;
- PR e fase responsável estiverem referenciados;
- não houver regressão arquitetural identificada;
- não houver vazamento de dado sensível em log, teste ou documentação.

## 9. Política de bloqueio de produção real

- MedicalRecord pode seguir como MVP técnico.
- MedicalRecord pode ser usado para estudo, demonstração e validação controlada.
- MedicalRecord não deve ser usado com dados clínicos reais enquanto houver P1 aberto.
- Resolver P2/P3 antes dos P1 não libera produção real.
- Produção segura futura depende da resolução dos P1 com evidências objetivas.

## 10. Relação entre débitos e fases práticas

### 6.2 — Segurança e autorização granular
- MR-DEBT-003.

### 6.3 — Auditoria e autoria clínica
- MR-DEBT-004.
- MR-DEBT-002.

### 6.4 — Persistência clínica e retenção
- MR-DEBT-006.
- MR-DEBT-001.
- MR-DEBT-005.

### 6.5 — Integridade e evolução de schema
- MR-DEBT-007.
- MR-DEBT-008.
- MR-DEBT-009.

### 6.6 — Qualidade operacional
- MR-DEBT-010.
- MR-DEBT-011.
- MR-DEBT-012.

## 11. Ordem recomendada de abertura das próximas fases

Recomendação de execução:
1. 6.2 primeiro, porque autorização granular é base de segurança.
2. 6.3 depois, porque autoria e auditoria dependem de identidade/autorização bem definida.
3. 6.4 depois, porque persistência clínica, cascade, soft delete e retenção dependem da governança dos dados.
4. 6.5 depois, para schema/integridade complementar.
5. 6.6 depois, para qualidade operacional e evidências.

## 12. Critérios mínimos para abrir fase prática de hardening

Antes de abrir cada fase prática, deve existir:
- escopo claro;
- IDs MR-DEBT envolvidos;
- fora de escopo;
- critérios de aceite;
- validações obrigatórias;
- impacto esperado;
- plano de teste;
- plano de documentação;
- decisão explícita se haverá código, migration, teste ou apenas documentação.

## 13. Fora do escopo

Esta fase não implementa:
- código;
- testes;
- migrations;
- database update;
- banco real;
- novos endpoints;
- roles/permissões;
- AuditLog;
- autoria;
- Soft Delete;
- política de retenção;
- índice único;
- CreatedAt;
- FlagsJson estrutural;
- CancellationToken;
- alteração em Program.cs;
- alteração em appsettings;
- alteração em workflows;
- frontend;
- Docker;
- Redis;
- RabbitMQ;
- Kubernetes.

## 14. Critérios de aceite da Fase 6.1.3

A fase será considerada concluída se:
- o documento for criado no caminho correto;
- a Subfase 6.1 for formalmente encerrada;
- as decisões da 6.1.1 e 6.1.2 forem consolidadas;
- as regras de atualização do registro vivo forem definidas;
- a Definition of Done para resolução de débito for definida;
- a política de bloqueio de produção real for documentada;
- a relação entre MR-DEBT e fases práticas for documentada;
- a ordem recomendada das próximas fases for definida;
- nenhuma implementação for feita;
- o escopo permanecer exclusivamente documental.

## 15. Decisão da fase

**Opção A — Governança do hardening MedicalRecord aprovada e Subfase 6.1 formalmente encerrada.**

## 16. Próxima fase recomendada

**Fase 6.2 — Segurança e autorização granular da vertical MedicalRecord.**

Objetivo da 6.2:
Iniciar a primeira fase prática de hardening P1, tratando MR-DEBT-003 por meio da definição e implementação de autorização granular por perfil/operação para MedicalRecord.

## 17. Validações obrigatórias

Comandos obrigatórios desta fase:
- `git branch --show-current`;
- `git status --short`;
- `git diff --check`;
- `dotnet build backend/Togo.sln`;
- `dotnet test backend/Togo.sln`.

Observação:
- esta fase permanece exclusivamente documental e não implementa hardening técnico.
