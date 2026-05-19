# TOGO — Fase 4.2.6: Documentação final do domínio Attendance

## 1. Objetivo

Esta fase fecha a Fase 4.2 por meio da consolidação documental do domínio Attendance já implementado e testado.

A Fase 4.2.6 é exclusivamente documental, sem alteração de código, testes, infraestrutura, banco ou pipeline.

## 2. Contexto da Fase 4.2

Resumo das subfases concluídas:

- **4.2.1:** criação dos testes unitários iniciais de Attendance;
- **4.2.2:** ajuste de invariantes mínimas da entidade Attendance;
- **4.2.3:** implementação de `Cancel()` e testes de ciclo de vida `Open/Closed/Canceled`;
- **4.2.4:** documentação dos enums `AttendanceStatus`/`AttendanceType` e matriz de ciclo de vida;
- **4.2.5:** validação build/test no ambiente Codex, com registro da limitação de ambiente;
- **4.2.5.1:** validação local/CI pós-PR 77, confirmando sucesso de build/test.

## 3. Arquivos finais relacionados ao domínio Attendance

- `backend/src/Togo.Domain/Entities/Attendance.cs`
- `backend/src/Togo.Domain/Enums/AttendanceStatus.cs`
- `backend/src/Togo.Domain/Enums/AttendanceType.cs`
- `backend/src/Togo.Domain.Tests/AttendanceTests.cs`

## 4. Entidade Attendance

A entidade `Attendance` está consolidada com as propriedades abaixo:

- **Id:** identificador técnico da entidade no domínio.
- **PatientId:** referência obrigatória ao paciente do atendimento.
- **AttendanceNumber:** identificador funcional do atendimento, obrigatório e normalizado.
- **OpenedAt:** data/hora de abertura do atendimento, obrigatória.
- **ClosedAt:** data/hora de fechamento, nula enquanto o atendimento não está fechado.
- **Status:** estado de ciclo de vida (`Open`, `Closed`, `Canceled`).
- **Type:** classificação do atendimento conforme enum `AttendanceType`.

## 5. Factory Create

Assinatura atual:

`Attendance.Create(long patientId, string attendanceNumber, DateTime openedAt, AttendanceType type)`

Comportamentos consolidados de `Create`:

- cria atendimento em estado aberto;
- força `Status = Open`;
- força `ClosedAt = null`;
- valida `PatientId > 0`;
- valida `AttendanceNumber` obrigatório;
- valida `OpenedAt` válido (não default);
- aplica `Trim()` em `AttendanceNumber` antes de persistir no estado interno.

## 6. Método Close

Comportamentos consolidados de `Close(DateTime closedAt)`:

- recebe `closedAt` como data/hora de fechamento;
- bloqueia fechamento de atendimento `Canceled`;
- bloqueia fechamento de atendimento já `Closed`;
- valida `closedAt` contra default;
- impede `closedAt` anterior a `OpenedAt`;
- define `ClosedAt`;
- altera `Status` para `Closed`.

## 7. Método Cancel

Comportamentos consolidados de `Cancel()`:

- permite cancelamento apenas para atendimento em `Open`;
- bloqueia cancelamento de atendimento `Closed`;
- bloqueia cancelamento de atendimento já `Canceled`;
- altera `Status` para `Canceled`;
- mantém `ClosedAt` como `null`.

Esta fase **não** implementa:

- motivo do cancelamento;
- `CancelledAt`;
- usuário/profissional responsável pelo cancelamento.

## 8. Enums

### AttendanceStatus

| Status | Valor | Semântica |
|---|---:|---|
| Open | 1 | Atendimento aberto/em andamento |
| Closed | 2 | Atendimento finalizado |
| Canceled | 3 | Atendimento cancelado |

### AttendanceType

| Type | Valor | Semântica |
|---|---:|---|
| Consultation | 1 | Consulta |
| Emergency | 2 | Emergência |
| Return | 3 | Retorno |
| Procedure | 4 | Procedimento |
| Exam | 5 | Exame |
| Other | 6 | Outro |

## 9. Ciclo de vida consolidado

Fluxo consolidado:

- `Create -> Open`;
- `Open -> Close -> Closed`;
- `Open -> Cancel -> Canceled`;
- `Closed` é terminal;
- `Canceled` é terminal.

Matriz de transição:

| Estado atual | Ação | Resultado | Permitido? |
|---|---|---|---:|
| nenhum | Create | Open | Sim |
| Open | Close | Closed | Sim |
| Open | Cancel | Canceled | Sim |
| Closed | Close | Closed | Não |
| Closed | Cancel | Canceled | Não |
| Canceled | Close | Closed | Não |
| Canceled | Cancel | Canceled | Não |

## 10. Testes implementados

Testes de `AttendanceTests` consolidados por agrupamento funcional:

### Create

- `Create_ShouldCreateAttendance_WhenDataIsValid`;
- `Create_ShouldThrowArgumentOutOfRangeException_WhenPatientIdIsInvalid`;
- `Create_ShouldThrowArgumentException_WhenAttendanceNumberIsEmpty`;
- `Create_ShouldThrowArgumentException_WhenOpenedAtIsDefault`.

### Close

- `Close_ShouldCloseAttendance_WhenClosedAtIsValid`;
- `Close_ShouldThrowArgumentException_WhenClosedAtIsDefault`;
- `Close_ShouldThrowArgumentException_WhenClosedAtIsBeforeOpenedAt`;
- `Close_ShouldThrowInvalidOperationException_WhenAttendanceIsAlreadyClosed`;
- `Close_ShouldThrowInvalidOperationException_WhenAttendanceIsCanceled`.

### Cancel

- `Cancel_ShouldCancelAttendance_WhenAttendanceIsOpen`;
- `Cancel_ShouldThrowInvalidOperationException_WhenAttendanceIsClosed`;
- `Cancel_ShouldThrowInvalidOperationException_WhenAttendanceIsAlreadyCanceled`.

## 11. Validação build/test

Validação consolidada (Fase 4.2.5.1):

- build local: sucesso;
- test local: sucesso;
- total de testes: 110;
- falhos: 0;
- bem-sucedidos: 110;
- ignorados: 0;
- CI GitHub: sucesso.

Registro técnico importante:

- a falha descrita na Fase 4.2.5 foi limitação de ambiente Codex (`dotnet` indisponível no PATH);
- a validação local/CI da Fase 4.2.5.1 confirmou que não havia falha de código.

## 12. Decisões técnicas consolidadas

- `Create` não aceita `Status` externo.
- `Create` não aceita `ClosedAt` externo.
- `Attendance` nasce sempre `Open`.
- `ClosedAt` só é definido por `Close`.
- `Cancel` não define `ClosedAt`.
- `Closed` e `Canceled` são estados terminais nesta fase.
- Não foram adicionados status/tipos futuros.
- Não foi implementada reabertura.
- Não foi implementado agendamento.
- Não foi implementado financeiro.
- Não foi implementado evento de domínio/RabbitMQ.

## 13. Riscos evitados

- criação de atendimento já fechado;
- criação de atendimento já cancelado;
- `ClosedAt` preenchido em atendimento `Open`;
- fechamento duplicado;
- cancelamento duplicado;
- fechamento de atendimento cancelado;
- cancelamento de atendimento fechado;
- `ClosedAt` anterior a `OpenedAt`;
- crescimento prematuro da máquina de estados.

## 14. Nota técnica sobre default em nullable

- na Fase 4.2.1 houve falha causada por `default` em parâmetro nullable `DateTime?`;
- `default` em `DateTime?` pode resultar em `null`;
- quando a intenção for testar `DateTime.MinValue`, usar `default(DateTime)` ou variável tipada;
- em parâmetros `DateTime` não-nullable, `default` é aceitável.

## 15. Fora do escopo da Fase 4.2

- Application use cases;
- `IAttendanceRepository`;
- `AttendanceRepository`;
- validators de Application;
- controller;
- endpoints;
- migrations;
- banco;
- MedicalRecord/Prontuário;
- Financeiro;
- eventos;
- RabbitMQ;
- Redis;
- Docker;
- Kubernetes;
- dashboard;
- anexos;
- assinatura digital;
- auditoria avançada.

## 16. Próximos passos recomendados

**Fase 4.3 — Preparar camada Application de Attendance.**

Sugestão de primeira subfase:

**Fase 4.3.1 — Definir contratos Application de Attendance.**

Objetivo:

Criar e documentar os contratos mínimos para Application/API antes de implementar use cases e repository, respeitando o domínio Attendance já consolidado.

Contratos esperados futuramente:

- `CreateAttendanceRequest`;
- `AttendanceResponse`;
- `AttendanceListItemResponse`;
- `CloseAttendanceRequest`, se necessário;
- `CancelAttendanceRequest`, se necessário.

## 17. Decisão final

- Fase 4.2 está concluída;
- domínio Attendance está implementado, testado e documentado;
- próxima macrofase pode avançar para Application;
- nenhuma pendência bloqueante de domínio permanece aberta.
