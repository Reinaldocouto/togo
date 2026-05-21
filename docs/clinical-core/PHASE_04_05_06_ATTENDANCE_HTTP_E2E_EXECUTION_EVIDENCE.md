# TOGO — Fase 4.5.6: Evidência de execução HTTP E2E da API Attendance

## 1. Objetivo

Registrar a execução real/manual via Swagger da API Attendance, validando autenticação, criação de dados auxiliares, criação de atendimento, consulta, listagem, fechamento, cancelamento e cenários negativos.

## 2. Contexto

A trilha da vertical Attendance evoluiu de forma incremental nas fases anteriores:

- Fase 4.5.1 criou o `AttendancesController`;
- Fase 4.5.2 criou testes de controller/API;
- Fase 4.5.3 validou rotas/status/responses/Swagger;
- Fase 4.5.4 criou o roteiro E2E;
- Fase 4.5.5 consolidou a documentação final da API;
- esta fase (4.5.6) registra a execução HTTP real via Swagger.

## 3. Ambiente usado

- Execução local via Swagger.
- URL usada: `https://localhost:3003/swagger/index.html`
- API local em execução.
- Autenticação via Bearer JWT.
- Banco local com dados criados durante o teste.
- Token real não foi documentado.
- Nos exemplos, foi utilizado apenas `Bearer <TOKEN>` quando necessário.

## 4. Build/test local

Evidência humana previamente informada:

- `dotnet build backend/Togo.sln`: sucesso.
- `dotnet test backend/Togo.sln`: sucesso.
- Total de testes: 182.
- Falhas: 0.
- Ignorados: 0.

Observação: no ambiente Codex, o SDK .NET pode estar indisponível; ainda assim, a validação humana local foi executada e registrada.

## 5. Dados auxiliares criados

### Tutor criado

Endpoint:

`POST /api/tutors`

Resultado:

- Status: 201 Created.
- Tutor criado com sucesso.
- TutorId: 2.

### Pet/Patient criado

Endpoint:

`POST /api/pets`

Resultado:

- Status: 201 Created.
- PatientId: 2.
- TutorId: 2.
- Nome: Pet E2E Attendance.

### Confirmação do Patient

Endpoint:

`GET /api/pets/2`

Resultado:

- Status: 200 OK.
- PatientId confirmado como válido.

## 6. Fluxo positivo da API Attendance

### 6.1 Criar Attendance inicial

Endpoint:

`POST /api/attendances`

Payload usado:

```json
{
  "patientId": 2,
  "attendanceNumber": "ATT-E2E-001",
  "openedAt": "2026-05-20T10:00:00Z",
  "type": 1
}
```

Resultado:

- Status: 201 Created.
- AttendanceId: 1.
- PatientId: 2.
- Status: 1.
- Type: 1.
- ClosedAt: null.

### 6.2 Buscar Attendance por ID

Endpoint:

`GET /api/attendances/1`

Resultado:

- Status: 200 OK.
- Attendance encontrada.
- Id: 1.
- PatientId: 2.
- AttendanceNumber: ATT-E2E-001.
- Status: 1.
- ClosedAt: null.

### 6.3 Listar Attendances

Endpoint:

`GET /api/attendances`

Resultado:

- Status: 200 OK.
- Lista retornada com Attendance criada.
- ATT-E2E-001 presente na listagem.

### 6.4 Fechar Attendance

Endpoint:

`PATCH /api/attendances/1/close`

Payload usado:

```json
{
  "closedAt": "2026-05-21T12:47:08.494Z"
}
```

Resultado:

- Status: 200 OK.
- Status retornado: 2.
- ClosedAt preenchido.
- Attendance fechada com sucesso.

### 6.5 Criar Attendance para cancelamento

Endpoint:

`POST /api/attendances`

Payload usado:

```json
{
  "patientId": 2,
  "attendanceNumber": "ATT-E2E-002",
  "openedAt": "2026-05-20T12:00:00Z",
  "type": 1
}
```

Resultado:

- Status: 201 Created.
- AttendanceId: 2.
- Status: 1.
- ClosedAt: null.

### 6.6 Cancelar Attendance aberta

Endpoint:

`PATCH /api/attendances/2/cancel`

Resultado:

- Status: 200 OK.
- Status retornado: 3.
- ClosedAt: null.
- Attendance cancelada com sucesso.

## 7. Cenários negativos validados

### 7.1 Cancelar Attendance fechada

Endpoint:

`PATCH /api/attendances/1/cancel`

Resultado:

- Status: 409 Conflict.
- Mensagem: "Closed attendance cannot be canceled".

### 7.2 Chamada sem token

Endpoint:

`GET /api/attendances`

Resultado:

- Status: 401 Unauthorized.
- Header: `www-authenticate: Bearer`.

### 7.3 ID inválido

Endpoint:

`GET /api/attendances/0`

Resultado:

- Status: 400 BadRequest.
- Mensagem: "Attendance id is invalid.".

### 7.4 ID inexistente

Endpoint:

`GET /api/attendances/999999`

Resultado:

- Status: 404 NotFound.
- Mensagem: "Attendance not found.".

### 7.5 Patient inexistente

Endpoint:

`POST /api/attendances`

Payload usado:

```json
{
  "patientId": 999999,
  "attendanceNumber": "ATT-E2E-PATIENT-NOT-FOUND",
  "openedAt": "2026-05-20T13:00:00Z",
  "type": 1
}
```

Resultado:

- Status: 404 NotFound.
- Mensagem: "Patient not found.".

### 7.6 AttendanceNumber duplicado

Endpoint:

`POST /api/attendances`

Payload usado:

```json
{
  "patientId": 2,
  "attendanceNumber": "ATT-E2E-001",
  "openedAt": "2026-05-20T14:00:00Z",
  "type": 1
}
```

Resultado:

- Status: 409 Conflict.
- Mensagem: "An attendance with this number already exists.".

### 7.7 Segundo atendimento aberto para o mesmo Patient

Primeiro foi criado atendimento aberto:

`POST /api/attendances`

Payload:

```json
{
  "patientId": 2,
  "attendanceNumber": "ATT-E2E-OPEN-001",
  "openedAt": "2026-05-20T15:00:00Z",
  "type": 1
}
```

Resultado:

- Status: 201 Created.
- AttendanceId: 3.
- Status: 1.

Depois foi feita tentativa de criar outro atendimento aberto para o mesmo Patient:

```json
{
  "patientId": 2,
  "attendanceNumber": "ATT-E2E-OPEN-002",
  "openedAt": "2026-05-20T16:00:00Z",
  "type": 1
}
```

Resultado:

- Status: 409 Conflict.
- Mensagem: "Patient already has an open attendance.".

### 7.8 Close com data default

Endpoint:

`PATCH /api/attendances/3/close`

Payload:

```json
{
  "closedAt": "0001-01-01T00:00:00Z"
}
```

Resultado:

- Status: 400 BadRequest.
- Mensagem: "Date is required (Parameter 'closedAt')".

### 7.9 Close antes da abertura

Endpoint:

`PATCH /api/attendances/3/close`

Payload:

```json
{
  "closedAt": "2026-05-20T14:00:00Z"
}
```

Resultado:

- Status: 400 BadRequest.
- Mensagem: "ClosedAt cannot be before OpenedAt (Parameter 'closedAt')".

### 7.10 Cancelar atendimento aberto de limpeza

Endpoint:

`PATCH /api/attendances/3/cancel`

Resultado:

- Status: 200 OK.
- Status retornado: 3.
- ClosedAt: null.

### 7.11 Cancelar Attendance já cancelada

Endpoint:

`PATCH /api/attendances/2/cancel`

Resultado:

- Status: 409 Conflict.
- Mensagem: "Attendance is already canceled".

## 8. Resultado consolidado

| Cenário | Resultado |
|---|---|
| Login/autorização Swagger | Executado |
| Criar Tutor | 201 |
| Criar Pet/Patient | 201 |
| Confirmar Patient | 200 |
| Criar Attendance | 201 |
| Buscar Attendance por ID | 200 |
| Listar Attendances | 200 |
| Fechar Attendance | 200 |
| Cancelar Attendance aberta | 200 |
| Cancelar Attendance fechada | 409 |
| Sem token | 401 |
| ID inválido | 400 |
| ID inexistente | 404 |
| Patient inexistente | 404 |
| AttendanceNumber duplicado | 409 |
| Segundo atendimento aberto | 409 |
| Close com data default | 400 |
| Close antes da abertura | 400 |
| Cancelar já cancelado | 409 |

## 9. Conclusão da execução

**Opção A — Execução HTTP E2E concluída com sucesso.**

Justificativa: todos os endpoints mínimos de Attendance foram exercitados via Swagger com autenticação real, banco local, dados reais criados no fluxo, respostas HTTP compatíveis com o planejamento e cenários negativos cobrindo 400/401/404/409.

## 10. Cuidados de segurança

- Não documentar token JWT real.
- Não documentar senha.
- Não documentar connection string.
- Prints devem ocultar ou evitar token real.
- Usar `Bearer <TOKEN>` em exemplos.

## 11. Lacunas remanescentes

- Não há WebApplicationFactory E2E automatizado.
- Execução foi manual via Swagger.
- Autenticação JWT foi validada manualmente, não por teste automatizado E2E.
- Banco local foi usado.
- Dados de teste ficaram no banco local.
- Swagger ainda não possui `ProducesResponseType` padronizado.

## 12. Fora do escopo

- Alteração de código.
- Alteração de testes.
- Alteração em Domain.
- Alteração em Application.
- Alteração em Infrastructure.
- Alteração em API.
- Migration.
- Database update.
- WebApplicationFactory.
- Testes automatizados E2E.
- Frontend.
- RabbitMQ.
- Redis.
- Docker.
- Kubernetes.

## 13. Validação da branch

Comandos de validação desta fase:

- `git branch --show-current`
- `git status`
- `git diff --check`

Opcionalmente, se o ambiente permitir:

- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Se dotnet não estiver disponível no Codex, registrar:

- `dotnet: command not found`

## 14. Próxima fase recomendada

**Fase 4.6.1 — Auditoria final da vertical Attendance.**

Objetivo: revisar toda a vertical Attendance — Domain, Application, Infrastructure, API, testes, documentação, validação HTTP real, lacunas e riscos — e declarar oficialmente a Fase 4 como concluída.
