# TOGO — Roadmap Macro até a Fase 12

## 1. Objetivo do documento

Este documento consolida o planejamento macro do projeto TOGO até a Fase 12, reconstruindo a visão ampla do produto com base no MVP acadêmico TOGO Petcare Pro e no backend atual em C#/.NET.

Este roadmap deve servir como:
- referência de continuidade;
- guia de priorização;
- alinhamento entre MVP acadêmico e projeto profissional em C#;
- base para futuras fases e prompts;
- proteção contra perda de histórico do planejamento.

## 2. Contexto

- O MVP TOGO Petcare Pro entregou uma visão ampla de ERP veterinário, cobrindo clínica, operação, comercial, financeiro e gestão.
- O projeto TOGO atual está reconstruindo essa visão com engenharia mais robusta, arquitetura em camadas e maior rastreabilidade técnica.
- O backend atual usa .NET 8, ASP.NET Core, EF Core, MySQL, JWT, Swagger, GitHub Actions e arquitetura em camadas.
- As fases recentes priorizaram o núcleo clínico (Patient/Pet, Attendance, MedicalRecord).
- Docker, Redis e RabbitMQ foram planejados anteriormente, mas ainda não implementados.
- Este roadmap macro organiza quando esses itens devem entrar de forma coerente com a maturidade funcional.
- Este documento não implementa nada; apenas consolida planejamento.

## 3. Relação entre MVP antigo e TOGO C#

| Módulo do MVP antigo | Situação no TOGO C# | Fase recomendada |
|---|---|---|
| Clientes/Tutores | Já implementado e operacional no backend atual | Consolidado (pré-Fase 6) |
| Pets/Patient | Base clínica implementada e em evolução | Consolidado (Fase 3) + hardening na Fase 6 |
| Atendimento | Implementado como núcleo de operação clínica inicial | Consolidado (Fase 4) |
| Prontuário | Implementado como MVP técnico (MedicalRecord) | Consolidado (Fase 5) + hardening na Fase 6 |
| Evolução clínica | Estrutura conceitual existente, sem vertical completa em produção | Fase 7 |
| Prescrição | Estrutura conceitual existente, sem vertical completa em produção | Fase 7 |
| Vacinas | Planejada para expansão clínica | Fase 7 |
| Agenda | Ainda não consolidada no backend atual | Fase 8 |
| Produtos | Ainda não consolidado | Fase 9 |
| Estoque | Ainda não consolidado | Fase 9 |
| PDV | Ainda não consolidado | Fase 10 |
| Financeiro | Ainda não consolidado | Fase 10 |
| Dashboard | Ainda não consolidado | Fase 11 |
| Relatórios | Ainda não consolidado | Fase 11 |
| Roles/perfis | Parcial (autenticação existe, granularidade fina pendente) | Fase 6 |
| Infraestrutura/DevOps | Decisão técnica existente, implementação pendente | Fase 12 |

## 4. Roadmap macro consolidado

| Fase | Tema | Objetivo | Base no MVP antigo | Status atual |
|---|---|---|---|---|
| Fase 6 | Hardening clínico e segurança | Endurecer o núcleo clínico para uso futuro com dados sensíveis | Roles, auditoria, retenção, produção segura | Em andamento |
| Fase 7 | Evolução clínica | Expandir vertical clínica além do prontuário principal | ClinicalEvolution, Prescription, Exams, Vaccines | Planejada |
| Fase 8 | Agenda e fluxo operacional | Conectar agenda, fila e atendimento | Agenda, fila, conversão em atendimento | Planejada |
| Fase 9 | Estoque, produtos e serviços | Estruturar catálogo e controle operacional de insumos/serviços | Products, estoque, service types | Planejada |
| Fase 10 | Financeiro e PDV | Implementar fluxo comercial e financeiro básico | Sales, sale_items, payments, fluxo de caixa | Planejada |
| Fase 11 | Dashboard, relatórios e BI | Criar camada de indicadores e visão gerencial | Métricas, indicadores, relatórios | Planejada |
| Fase 12 | Infra/DevOps/produção | Padronizar execução, observabilidade e deploy | Docker, Redis, RabbitMQ, health checks, observabilidade, deploy | Planejada |

## 5. Fase 6 — Hardening clínico e segurança

**Objetivo:** transformar a vertical MedicalRecord de MVP técnico em base mais segura para uso futuro com dados sensíveis.

**Subfases sugeridas:**
- 6.1 — Governança do hardening e registro de débitos;
- 6.2 — Segurança e autorização granular;
- 6.3 — Auditoria e autoria clínica;
- 6.4 — Persistência clínica, Soft Delete e retenção;
- 6.5 — Integridade e evolução de schema;
- 6.6 — Qualidade operacional;
- 6.7 — Fechamento executivo da Fase 6.

**Principais entregas:**
- roles/permissões finas;
- CreatedBy/UpdatedBy;
- AuditLog;
- Soft Delete;
- revisão de DeleteBehavior.Cascade;
- política de retenção;
- índice único em MedicalRecords.PatientId;
- CreatedAt;
- validação/normalização de FlagsJson;
- CancellationToken;
- evidências manuais versionadas.

**Nota crítica:** produção real com dados clínicos sensíveis depende principalmente da resolução dos débitos P1.

## 6. Fase 7 — Evolução clínica

**Objetivo:** expandir o núcleo clínico além do prontuário principal, criando registros clínicos reais vinculados ao paciente, atendimento e prontuário.

**Subfases sugeridas:**
- 7.1 — ClinicalEvolution;
- 7.2 — Prescription;
- 7.3 — PrescriptionItem;
- 7.4 — Exams/Attachments;
- 7.5 — Vaccines/VaccinationRecords;
- 7.6 — Timeline clínica do paciente;
- 7.7 — Testes, Swagger e fechamento executivo.

**Entregas esperadas:**
- evoluções clínicas;
- prescrições;
- itens de prescrição;
- exames e anexos;
- vacinas;
- histórico cronológico do paciente;
- vínculo com Attendance e MedicalRecord quando aplicável.

## 7. Fase 8 — Agenda e fluxo operacional

**Objetivo:** implementar o fluxo operacional da clínica, conectando agendamento, fila e atendimento.

**Subfases sugeridas:**
- 8.1 — Modelo de Appointment;
- 8.2 — CRUD de Agenda;
- 8.3 — Fila operacional;
- 8.4 — Conversão de agendamento em atendimento;
- 8.5 — Status operacionais;
- 8.6 — Testes, Swagger e fechamento executivo.

**Entregas esperadas:**
- agenda;
- calendário;
- status de agendamento;
- fila de espera;
- conversão Appointment -> Attendance;
- visão operacional do dia.

## 8. Fase 9 — Estoque, produtos e serviços

**Objetivo:** implementar o núcleo de produtos, serviços e estoque, preparando base para PDV e financeiro.

**Subfases sugeridas:**
- 9.1 — ServiceTypes;
- 9.2 — Products;
- 9.3 — Stock;
- 9.4 — StockMovements;
- 9.5 — Baixa de estoque por atendimento/prescrição/venda;
- 9.6 — Testes, Swagger e fechamento executivo.

**Entregas esperadas:**
- cadastro de produtos;
- cadastro de serviços;
- estoque atual;
- estoque mínimo;
- movimentações;
- alertas de baixo estoque;
- vínculo futuro com PDV e atendimento.

## 9. Fase 10 — Financeiro e PDV

**Objetivo:** implementar fluxo comercial e financeiro básico do ERP veterinário.

**Subfases sugeridas:**
- 10.1 — Sales;
- 10.2 — SaleItems;
- 10.3 — Payments;
- 10.4 — CashFlow;
- 10.5 — Integração Atendimento -> Financeiro;
- 10.6 — Eventos futuros com RabbitMQ ou Outbox, se aplicável;
- 10.7 — Testes, Swagger e fechamento executivo.

**Entregas esperadas:**
- vendas;
- itens de venda;
- pagamentos;
- fluxo de caixa;
- cobrança de atendimento;
- integração com produtos/serviços;
- preparação para eventos assíncronos.

## 10. Fase 11 — Dashboard, relatórios e BI

**Objetivo:** criar camada de indicadores operacionais, clínicos e financeiros.

**Subfases sugeridas:**
- 11.1 — Dashboard operacional;
- 11.2 — Indicadores clínicos;
- 11.3 — Indicadores financeiros;
- 11.4 — Relatórios de atendimento;
- 11.5 — Relatórios de estoque;
- 11.6 — Relatórios gerenciais;
- 11.7 — Exportação futura;
- 11.8 — Testes, Swagger e fechamento executivo.

**Entregas esperadas:**
- métricas do dia;
- atendimentos por período;
- pacientes ativos;
- agendamentos;
- vacinas pendentes;
- produtos com estoque baixo;
- faturamento;
- relatórios consolidados.

## 11. Fase 12 — Infraestrutura, DevOps e produção

**Objetivo:** preparar o projeto para ambiente mais próximo de produção, padronizando execução, observabilidade, cache, mensageria e deploy.

**Subfases sugeridas:**
- 12.1 — Dockerfile da API;
- 12.2 — docker-compose com API + MySQL;
- 12.3 — Health Checks;
- 12.4 — Redis cache;
- 12.5 — RabbitMQ/eventos;
- 12.6 — Observabilidade/logs estruturados;
- 12.7 — Configuração por ambiente;
- 12.8 — Deploy strategy;
- 12.9 — Testcontainers ou testes de integração com containers;
- 12.10 — Fechamento executivo de produção/infra.

**Entregas esperadas:**
- Dockerfile;
- docker-compose.yml;
- MySQL containerizado;
- Redis configurado;
- RabbitMQ configurado;
- health checks;
- logs estruturados;
- observabilidade;
- variáveis por ambiente;
- pipeline/deploy documentado.

**Registro explícito:** Docker, Redis e RabbitMQ já estavam aprovados em roadmap anterior, mas não foram implementados nas fases 4, 5 ou 6 porque ainda não havia maturidade funcional suficiente para justificar a entrada dessas ferramentas.

## 12. Ordem recomendada de execução

1. Fase 6 — primeiro proteger e endurecer núcleo clínico.
2. Fase 7 — depois expandir funcionalidades clínicas.
3. Fase 8 — depois organizar operação de agenda/fila.
4. Fase 9 — depois estruturar produtos/estoque/serviços.
5. Fase 10 — depois financeiro e PDV.
6. Fase 11 — depois relatórios e BI.
7. Fase 12 — depois infraestrutura, cache, mensageria e produção.

**Justificativa:** não faz sentido colocar Redis/RabbitMQ/Docker como prioridade de produto antes de estabilizar domínio, endpoints e fluxos de negócio. Infraestrutura entra melhor quando já existe fluxo real para suportar.

## 13. Ferramentas e tecnologias previstas por fase

| Fase | Ferramentas/Tecnologias principais |
|---|---|
| Fase 6 | ASP.NET Core Authorization Policies; JWT; EF Core; AuditLog; migrations; xUnit |
| Fase 7 | EF Core; ASP.NET Core API; xUnit; Swagger; possível upload/anexos futuramente |
| Fase 8 | EF Core; API REST; regras de status; testes de integração |
| Fase 9 | EF Core; controle de estoque; transações; testes |
| Fase 10 | fluxo financeiro; eventos de domínio; Outbox futuro; RabbitMQ futuro |
| Fase 11 | queries otimizadas; endpoints de dashboard; relatórios; cache futuro |
| Fase 12 | Docker; docker-compose; Redis; RabbitMQ; Health Checks; Observabilidade; CI/CD; configuração por ambiente |

## 14. Critérios para não antecipar infraestrutura

**Docker pode ser antecipado se:**
- ambiente local começar a gerar atrito;
- onboarding ficar difícil;
- testes de integração exigirem ambiente padronizado.

**Redis pode ser antecipado se:**
- endpoints de listagem/dashboard ficarem pesados;
- houver necessidade real de cache;
- houver estratégia clara de invalidação.

**RabbitMQ pode ser antecipado se:**
- houver comunicação real entre módulos;
- Attendance precisar acionar Financeiro;
- houver necessidade de desacoplamento assíncrono.

Caso contrário, manter Fase 12 como ponto oficial de entrada de Infra/DevOps/produção.

## 15. Riscos de roadmap

- escopo crescer demais;
- tentar implementar ERP inteiro sem fechar cada vertical;
- antecipar infraestrutura sem necessidade real;
- misturar clínica, financeiro e DevOps na mesma fase;
- perder rastreabilidade de débitos técnicos;
- não atualizar documentação conforme mudanças;
- criar endpoints sem testes;
- usar dados sensíveis sem hardening.

## 16. Decisão de governança

Este documento passa a ser o roadmap macro oficial do TOGO até a Fase 12.

**Regras:**
- cada nova fase deve referenciar este documento;
- se uma fase mudar, este roadmap deve ser atualizado;
- se Docker/Redis/RabbitMQ forem antecipados, registrar justificativa;
- nenhuma fase futura deve avançar sem documento de abertura e planejamento fracionado;
- este documento não substitui documentos específicos de fase.

## 17. Próxima fase recomendada

Como o projeto está na Fase 6, recomenda-se continuar a Fase 6 conforme planejamento atual, priorizando a conclusão da Subfase 6.1 e depois abertura prática da Fase 6.2 — Segurança e autorização granular.

A Fase 7 só deve começar após decisão explícita de encerramento ou pausa controlada da Fase 6.

## 18. Critérios de aceite

A fase/documento será considerado concluído se:
- `docs/ROADMAP_TO_PHASE_12.md` for criado;
- Fases 6 a 12 forem descritas;
- a relação com o MVP antigo for documentada;
- Docker, Redis e RabbitMQ forem posicionados corretamente;
- a ordem recomendada for justificada;
- tecnologias por fase forem listadas;
- riscos de roadmap forem documentados;
- regra de governança for definida;
- nenhuma implementação técnica for feita.

## 19. Fora do escopo

Este documento não implementa:
- código;
- testes;
- migrations;
- banco;
- endpoints;
- frontend;
- Docker;
- Redis;
- RabbitMQ;
- CI/CD;
- deploy;
- novas entidades;
- novas APIs.

## 20. Validações obrigatórias

Comandos de validação operacional desta fase documental:
- `git branch --show-current`;
- `git status --short`;
- `git diff --check`;
- `dotnet build backend/Togo.sln` (se `dotnet` estiver disponível);
- `dotnet test backend/Togo.sln` (se `dotnet` estiver disponível).
