# TOGO — Fase 4.1.5: Decisões e riscos antes da implementação de Attendance

## 1. Objetivo

Esta fase consolida as decisões da Fase 4.1 antes do início da implementação da vertical Attendance.

Também fica explícito que a Fase 4.1.5 é exclusivamente documental e não implementa código.

## 2. Contexto

A Fase 4.1 preparou a implementação de Atendimento por meio de:

- inspeção do estado atual;
- decisão de nomenclatura;
- definição do modelo mínimo;
- definição do relacionamento com Patient/Pet.

Com essas bases registradas, a próxima etapa será iniciar a implementação controlada da vertical Attendance.

## 3. Decisões já consolidadas

### Nomenclatura

- Código/API: `Attendance` / `attendances`.
- Documentação de negócio: `Atendimento`.

### Modelo mínimo

- `Id`;
- `PatientId`;
- `AttendanceNumber`;
- `OpenedAt`;
- `ClosedAt`;
- `Status`;
- `Type`.

### Relacionamento

- Attendance pertence a Patient.
- Patient pode ter múltiplos Attendances.
- Attendance não duplica dados de Patient/Pet.
- Tutor não entra diretamente em Attendance.

### Ciclo de vida

- `Open`;
- `Closed`;
- `Canceled`.

### Tipo de atendimento

- `Consultation`;
- `Emergency`;
- `Return`;
- `Procedure`;
- `Exam`;
- `Other`.

### Regra de atendimento aberto

- Decisão inicial: bloquear mais de um Attendance `Open` por Patient.

## 4. Decisões sobre o que NÃO implementar agora

Não será implementado agora:

- Prontuário dentro de Attendance;
- Evolução clínica dentro de Attendance;
- Prescrição dentro de Attendance;
- Financeiro dentro de Attendance;
- cobrança;
- pagamento;
- eventos;
- RabbitMQ;
- Redis;
- Docker;
- Kubernetes;
- dashboard;
- anexos;
- assinatura digital;
- agendamento complexo;
- vínculo obrigatório com veterinário/profissional;
- vínculo obrigatório com unidade/sala.

## 5. Riscos principais antes de codar

1. Inflar Attendance com campos clínicos demais.
2. Misturar Attendance com MedicalRecord/Prontuário.
3. Misturar Attendance com Financeiro.
4. Criar eventos/mensageria antes da hora.
5. Criar status demais antes de validar fluxo real.
6. Criar projections grandes demais na primeira versão.
7. Duplicar dados de Patient/Pet em Attendance.
8. Não validar existência de Patient na Application.
9. Depender apenas da FK do banco para regra de negócio.
10. Permitir múltiplos atendimentos abertos sem controle.
11. Implementar repository/use cases fora do padrão Pet/Patient.
12. Criar testes fracos ou apenas felizes, sem cenários de erro.

## 6. Mitigações recomendadas

- **Risco 1**: manter o modelo mínimo definido e evitar novos campos sem evidência de necessidade operacional.
- **Risco 2**: separar responsabilidades entre Attendance (episódio) e MedicalRecord/Prontuário (histórico longitudinal).
- **Risco 3**: manter Financeiro fora da primeira versão e tratar integração apenas em fase posterior.
- **Risco 4**: não criar eventos/mensageria nesta macrofase.
- **Risco 5**: iniciar somente com `Open`, `Closed` e `Canceled` até validar fluxo real.
- **Risco 6**: usar projections enxutas e incrementais na API inicial.
- **Risco 7**: não duplicar atributos de Patient/Pet em Attendance; consultar por relacionamento.
- **Risco 8**: validar existência de Patient na Application antes de persistir.
- **Risco 9**: não depender somente da FK; explicitar regra de negócio em validator/use case.
- **Risco 10**: criar validator específico para atendimento aberto por Patient.
- **Risco 11**: seguir padrão arquitetural já aplicado nas verticais Pet/Patient.
- **Risco 12**: criar testes de domínio e aplicação com cenários de sucesso e erro antes ou junto da implementação.

## 7. Ordem recomendada para implementação

1. Revisar/criar testes de domínio para Attendance.
2. Criar ou ajustar métodos mínimos na entidade, se necessário.
3. Criar contratos Application.
4. Criar `IAttendanceRepository`.
5. Criar validators.
6. Criar use cases.
7. Criar `AttendanceRepository` EF Core.
8. Registrar DI.
9. Criar `AttendancesController`.
10. Criar testes automatizados de Application/API.
11. Executar build/test.
12. Validar manualmente via API/Postman.
13. Documentar resultado.

Importante: a ordem acima é recomendação de roadmap para a próxima macrofase e não representa implementação nesta fase documental.

## 8. Validators e regras esperadas

- `AttendancePatientExistsValidator`: garante que o `PatientId` informado existe e está apto para vínculo.
- `OpenAttendanceValidator`: impede mais de um atendimento em status `Open` para o mesmo Patient.
- `AttendanceNumberUniqueValidator`: garante unicidade de `AttendanceNumber` no escopo definido.
- `AttendanceStatusTransitionValidator`: garante transições válidas de status (ex.: `Open` -> `Closed`/`Canceled`).
- `AttendanceBelongsToPatientValidator` (se necessário): garante consistência do vínculo em operações por `AttendanceId` + `PatientId`.

## 9. Use cases esperados

Use cases prováveis para a vertical inicial:

- `CreateAttendanceUseCase`;
- `ListAttendancesUseCase`;
- `GetAttendanceByIdUseCase`;
- `UpdateAttendanceUseCase`;
- `CloseAttendanceUseCase`;
- `CancelAttendanceUseCase`.

Delete físico deve ser evitado inicialmente se o fluxo de negócio apontar para cancelamento lógico como estratégia padrão.

## 10. Endpoints esperados

Endpoints prováveis:

- `GET /api/attendances`
- `GET /api/attendances/{id}`
- `POST /api/attendances`
- `PUT /api/attendances/{id}`
- `PATCH /api/attendances/{id}/close`, se a decisão futura preferir endpoint dedicado
- `PATCH /api/attendances/{id}/cancel`, se a decisão futura preferir endpoint dedicado
- `DELETE /api/attendances/{id}`, somente se a decisão futura justificar

Os endpoints finais serão decididos na fase de controller, respeitando as decisões de domínio e aplicação.

## 11. Contratos esperados

Contratos prováveis:

- `CreateAttendanceRequest`;
- `UpdateAttendanceRequest`;
- `AttendanceResponse`;
- `AttendanceListItemResponse`;
- `CloseAttendanceRequest`, se necessário;
- `CancelAttendanceRequest`, se necessário.

## 12. Testes esperados

### Domain

- criar Attendance válido;
- impedir `PatientId` inválido;
- impedir `AttendanceNumber` vazio;
- impedir `OpenedAt` inválido;
- fechar Attendance com data válida;
- impedir fechar com data inválida, se aplicável.

### Application

- criar com Patient existente;
- erro ao criar com Patient inexistente;
- erro ao criar com `AttendanceNumber` duplicado;
- erro ao criar se Patient já tiver Attendance `Open`;
- listar atendimentos;
- buscar por id;
- fechar atendimento;
- cancelar atendimento.

### API

- POST válido;
- POST inválido por Patient inexistente;
- GET list;
- GET by id;
- fluxo de close/cancel conforme decisão futura.

## 13. Critérios para começar a codar

A implementação só deve começar quando estiverem claros e aprovados:

- modelo mínimo;
- relacionamento com Patient;
- regra de atendimento aberto;
- padrão de nomenclatura;
- escopo fora da primeira versão;
- ordem de implementação;
- riscos e mitigações.

## 14. Decisão final da Fase 4.1

A Fase 4.1 conclui a preparação documental da vertical Attendance.

Com isso, a próxima fase pode iniciar a implementação controlada com foco em domínio e testes.

## 15. Próxima fase recomendada

**Fase 4.2 — Criar/testar domínio Atendimento.**

Mais especificamente:

**Fase 4.2.1 — Revisar entidade Attendance e criar/revisar testes de domínio.**

Objetivo:
Validar a entidade Attendance existente, seus invariantes, factory, ciclo de vida e regras mínimas antes de avançar para Application/API.

## 16. Fora do escopo

Esta fase não implementa:

- alteração da entidade Attendance;
- alteração dos enums;
- alteração do EF Core;
- alteração do AppDbContext;
- alteração de migration;
- alteração de banco;
- repository;
- use cases;
- contracts;
- validators;
- controller;
- testes;
- Program.cs;
- appsettings;
- workflow;
- Docker;
- Redis;
- RabbitMQ;
- Kubernetes.
