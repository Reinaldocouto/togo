# TOGO — Fase 4.2.4: Enums e ciclo de vida de Attendance

## 1. Objetivo

Esta fase revisa e documenta os enums `AttendanceStatus` e `AttendanceType`, além do ciclo de vida mínimo de `Attendance`.

Também fica explícito que a fase não implementa nova regra de negócio.

## 2. Contexto

- A Fase 4.2.1 criou testes iniciais de domínio.
- A Fase 4.2.2 reforçou invariantes mínimas da entidade.
- A Fase 4.2.3 adicionou cancelamento e testes de ciclo de vida/status.
- Agora, a Fase 4.2.4 consolida a semântica dos enums e das transições.

## 3. AttendanceStatus

Valores existentes observados no domínio:

| Status | Valor numérico | Semântica |
|---|---:|---|
| Open | 1 | Atendimento criado e em andamento |
| Closed | 2 | Atendimento finalizado por fechamento explícito |
| Canceled | 3 | Atendimento cancelado antes da conclusão |

Nesta fase, não serão adicionados novos status.

## 4. AttendanceType

Valores existentes observados no domínio:

| Type | Valor numérico | Semântica |
|---|---:|---|
| Consultation | 1 | Consulta clínica comum |
| Emergency | 2 | Atendimento emergencial |
| Return | 3 | Retorno/reavaliação |
| Procedure | 4 | Procedimento |
| Exam | 5 | Atendimento relacionado a exame |
| Other | 6 | Outro tipo não classificado inicialmente |

Esses tipos são suficientes para o modelo mínimo desta fase.

## 5. Ciclo de vida atual

Ciclo de vida atual de `Attendance`:

- `Attendance` nasce como `Open` por meio de `Create`.
- `Attendance` pode sair de `Open` para `Closed` por meio de `Close`.
- `Attendance` pode sair de `Open` para `Canceled` por meio de `Cancel`.
- `Closed` é terminal nesta fase.
- `Canceled` é terminal nesta fase.

## 6. Matriz de transição de estados

| Estado atual | Ação | Novo estado | Permitido? | Observação |
|---|---|---|---:|---|
| Open | Create | Open | Sim | Create sempre cria Open |
| Open | Close | Closed | Sim | Exige `closedAt` válido |
| Open | Cancel | Canceled | Sim | `ClosedAt` permanece `null` |
| Closed | Close | Closed | Não | Bloqueia fechamento duplicado |
| Closed | Cancel | Canceled | Não | Closed não pode ser cancelado |
| Canceled | Close | Closed | Não | Canceled não pode ser fechado |
| Canceled | Cancel | Canceled | Não | Bloqueia cancelamento duplicado |

## 7. Regras implementadas na entidade

Regras atuais observadas em `Attendance.cs`:

- `Create` valida `PatientId > 0`.
- `Create` valida `AttendanceNumber` obrigatório.
- `Create` valida `OpenedAt`.
- `Create` normaliza `AttendanceNumber` com `Trim`.
- `Create` força `Status` `Open`.
- `Create` força `ClosedAt` `null`.
- `Close` bloqueia `Canceled`.
- `Close` bloqueia `Closed`.
- `Close` valida `closedAt`.
- `Close` impede `closedAt` anterior a `OpenedAt`.
- `Close` altera `Status` para `Closed`.
- `Close` define `ClosedAt`.
- `Cancel` bloqueia `Closed`.
- `Cancel` bloqueia `Canceled`.
- `Cancel` altera `Status` para `Canceled`.
- `Cancel` mantém `ClosedAt` `null`.

## 8. Regras cobertas por testes

Com base em `AttendanceTests.cs`:

### Create

- criação válida;
- `PatientId` inválido;
- `AttendanceNumber` vazio;
- `OpenedAt` inválido.

### Close

- fechamento válido;
- `closedAt` default;
- `closedAt` anterior a `OpenedAt`;
- fechamento duplicado;
- fechamento de atendimento cancelado.

### Cancel

- cancelamento válido;
- cancelamento de atendimento fechado;
- cancelamento duplicado.

## 9. Status e tipos que ficam fora do escopo

Não entram agora:

- `Scheduled`;
- `InProgress`;
- `WaitingPayment`;
- `NoShow`;
- `Reopened`;
- `Paid`;
- `PendingPayment`;
- `Archived`.

Também não entram novos tipos de atendimento nesta fase.

## 10. Riscos evitados

Riscos evitados pelas decisões atuais:

- criação de atendimento já fechado;
- criação de atendimento cancelado;
- fechamento duplicado;
- cancelamento duplicado;
- fechamento de atendimento cancelado;
- cancelamento de atendimento fechado;
- `closedAt` anterior a `openedAt`;
- crescimento prematuro da máquina de estados.

## 11. Lacunas futuras

Lacunas futuras possíveis:

- motivo de cancelamento;
- data/hora de cancelamento;
- usuário/profissional responsável pelo cancelamento;
- reabertura;
- agendamento;
- atendimento em progresso separado de aberto;
- integração com financeiro;
- eventos de domínio;
- auditoria.

Essas lacunas não bloqueiam o modelo mínimo atual.

## 12. Nota técnica sobre default em testes

Erro explícito observado na Fase 4.2.1:

- usar `default` em parâmetro nullable como `DateTime?` pode representar `null`, não `DateTime.MinValue`;
- quando a intenção for testar valor default informado, usar `default(DateTime)` ou variável tipada `DateTime invalidDate = default`;
- em parâmetros não-nullable `DateTime`, `default` é aceitável.

## 13. Decisão final

- manter `AttendanceStatus` com `Open`, `Closed` e `Canceled`;
- manter `AttendanceType` com os seis tipos atuais;
- não adicionar novos status/tipos agora;
- manter `Closed` e `Canceled` como estados terminais nesta fase;
- seguir para build/test final do domínio.

## 14. Próxima fase recomendada

**Fase 4.2.5 — Rodar build/test completo e validar fechamento da camada de domínio.**

Objetivo:
Executar validação completa do backend após as alterações de domínio, garantindo que a entidade `Attendance`, seus testes e o restante da solução estejam estáveis antes da documentação final da Fase 4.2.

## 15. Fora do escopo

Esta fase não implementa:

- novos status;
- novos tipos;
- novas regras de transição;
- alteração de comportamento de `Attendance`;
- alteração de Application;
- alteração de Infrastructure;
- alteração de API;
- alteração de EF Core;
- alteração de `AppDbContext`;
- migration;
- alteração de banco;
- repository;
- use cases;
- validators de Application;
- controller;
- endpoints;
- RabbitMQ;
- Redis;
- Docker;
- Kubernetes.
