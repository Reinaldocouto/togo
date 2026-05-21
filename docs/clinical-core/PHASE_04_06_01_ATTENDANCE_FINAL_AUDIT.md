# TOGO — Fase 4.6.1: Auditoria final da vertical Attendance

## 1. Objetivo

Registrar auditoria final da vertical Attendance, revisando aderência ao planejamento, escopo entregue, arquivos implementados, testes, documentação, validação HTTP E2E, riscos, lacunas e decisão final sobre encerramento da Fase 4.

## 2. Contexto geral da Fase 4

A Fase 4 evoluiu Attendance de forma incremental:

- 4.0: decisão de infra/cache/mensageria fora do escopo imediato;
- 4.1: revisão, nomenclatura, modelo mínimo, relação Patient/Pet e riscos;
- 4.2: domínio Attendance;
- 4.3: camada Application;
- 4.4: Infrastructure;
- 4.5: API;
- 4.6: auditoria e encerramento.

## 3. Escopo auditado

Foram auditadas:

- entidade de domínio;
- enums;
- invariantes;
- contratos;
- repository interface;
- validators;
- use cases;
- repository concreto;
- testes de Infrastructure;
- DI;
- controller;
- testes de API;
- documentação;
- execução HTTP E2E real via Swagger.

## 4. Auditoria da camada Domain

Status: **Conforme esperado**.

Verificações:

- `Attendance.Create` cria atendimento sempre `Open`;
- `ClosedAt` nasce `null`;
- status inicial `Open`;
- `PatientId` obrigatório;
- `AttendanceNumber` obrigatório;
- `OpenedAt` obrigatório;
- `Type` obrigatório no contrato de criação;
- `Close` valida data default;
- `Close` bloqueia data anterior à abertura;
- `Close` bloqueia fechamento duplicado;
- `Close` bloqueia atendimento cancelado;
- `Cancel` só funciona para atendimento `Open`;
- `Cancel` mantém `ClosedAt = null`;
- `Cancel` bloqueia `Closed`;
- `Cancel` bloqueia `Canceled`.

As regras acima estão implementadas em `Attendance` e cobertas por testes diretos de domínio.

## 5. Auditoria da camada Application

Status: **Conforme esperado**.

### Contracts

- `CreateAttendanceRequest`;
- `AttendanceResponse`;
- `AttendanceListItemResponse`;
- `CloseAttendanceRequest`;
- ausência intencional de `CancelAttendanceRequest` (cancelamento sem body).

### Repository interface

`IAttendanceRepository` com métodos:

- `GetByIdAsync`;
- `ListAsync`;
- `ListByPatientIdAsync`;
- `AddAsync`;
- `UpdateAsync`;
- `ExistsByAttendanceNumberAsync`;
- `HasOpenAttendanceForPatientAsync`.

### Validators

- `AttendancePatientExistsValidator`;
- `AttendanceNumberUniqueValidator`;
- `OpenAttendanceValidator`.

### Use cases

- `CreateAttendanceUseCase`;
- `GetAttendanceByIdUseCase`;
- `ListAttendancesUseCase`;
- `CloseAttendanceUseCase`;
- `CancelAttendanceUseCase`.

Os use cases retornam `ApplicationResult` e tratam cenários esperados de validação/negócio sem expor exceções esperadas para a API.

## 6. Auditoria da camada Infrastructure

Status: **Conforme esperado**.

Verificações:

- `AttendanceRepository` implementa `IAttendanceRepository`;
- uso de `AppDbContext`;
- leituras com `AsNoTracking`;
- `AddAsync` com `SaveChangesAsync`;
- `UpdateAsync` com `SaveChangesAsync`;
- consultas por `AttendanceNumber` e atendimento aberto;
- `ListAsync` ordenado;
- `ListByPatientIdAsync` implementado;
- testes de Infrastructure com SQLite in-memory;
- `Togo.Infrastructure.Tests` integrado na solution;
- DI registrado com `AddScoped<IAttendanceRepository, AttendanceRepository>`.

## 7. Auditoria da camada API

Status: **Conforme esperado**.

### Controller

- `AttendancesController` existe;
- usa `[Authorize]`;
- usa `[ApiController]`;
- rota base `api/attendances`.

### Endpoints

- `GET /api/attendances`;
- `GET /api/attendances/{id}`;
- `POST /api/attendances`;
- `PATCH /api/attendances/{id}/close`;
- `PATCH /api/attendances/{id}/cancel`.

### Mapeamento HTTP

- `Success` -> `200`;
- `CreatedAtAction` -> `201` no create;
- `ValidationError` -> `400`;
- `NotFound` -> `404`;
- `Conflict` -> `409`;
- fallback -> `500`.

### DI

- use cases registrados;
- validators registrados;
- repository registrado.

## 8. Auditoria dos testes

Cobertura existente por camada:

### Domain

- criação válida;
- campos inválidos;
- close válido;
- close inválido;
- cancel válido;
- transições inválidas.

### Application

- validators;
- create;
- get by id;
- list;
- close;
- cancel;
- fake repository estabilizado.

### Infrastructure

- repository concreto;
- add;
- get;
- list;
- list by patient;
- exists;
- has open attendance;
- update closed/canceled.

### API

- controller;
- status codes;
- `CreatedAtAction`;
- `200/201/400/404/409`.

Evidência humana consolidada da fase anterior:

- `dotnet build backend/Togo.sln`: sucesso;
- `dotnet test backend/Togo.sln`: sucesso;
- 182 testes;
- 0 falhas;
- 0 ignorados.

## 9. Auditoria da execução HTTP E2E real

Consolidação da Fase 4.5.6.

### Fluxo positivo

- criação de tutor;
- criação de pet/patient;
- confirmação de patient;
- criação de attendance;
- get by id;
- list;
- close;
- criação de attendance para cancelamento;
- cancel.

### Cenários negativos

- 401 sem token;
- 400 id inválido;
- 404 id inexistente;
- 404 patient inexistente;
- 409 attendance number duplicado;
- 409 segundo atendimento aberto;
- 400 close com default date;
- 400 close antes da abertura;
- 409 cancelar fechado;
- 409 cancelar já cancelado.

A execução foi manual via Swagger, usando banco local e autenticação real.

## 10. Auditoria das decisões técnicas

Decisões finais da vertical:

- Attendance representa episódio/visita;
- Attendance não substitui prontuário;
- vínculo direto por `PatientId`;
- sem `TutorId` direto;
- sem vínculo financeiro nesta fase;
- sem eventos/RabbitMQ nesta fase;
- sem Redis/cache nesta fase;
- sem Docker/Kubernetes nesta fase;
- sem delete físico de Attendance;
- `Cancel` usado como encerramento lógico inválido/abortado;
- `Close` usado como finalização do atendimento.

## 11. Incidentes e aprendizados consolidados

Aprendizados relevantes da Fase 4:

- `default(DateTime)` vs default em nullable;
- cuidado com `catch ArgumentException/ArgumentOutOfRangeException`;
- `Attendance.Id = 0` nos testes;
- fake repository não pode colapsar entidade por `Id` default;
- mensagens reais do domínio devem guiar assertions;
- sempre validar branch antes de alterar arquivos;
- Codex pode não ter SDK dotnet;
- CI/local build/test são gates obrigatórios;
- `git diff --check` deve ser gate obrigatório;
- cuidado com PR auxiliar contra branch errada;
- cuidado com conflito documental;
- testes manuais Swagger complementam, mas não substituem automação.

## 12. Riscos remanescentes

Riscos reais identificados:

- ausência de `WebApplicationFactory` E2E automatizado;
- execução Swagger foi manual;
- Swagger ainda sem `ProducesResponseType` padronizado;
- dados de teste ficaram no banco local;
- autenticação JWT E2E não está automatizada;
- não há controle de roles/permissões finas;
- não há auditoria/log de mudança de status de Attendance;
- não há MedicalRecord ainda;
- não há integração com financeiro;
- não há eventos de domínio/publicação.

## 13. Lacunas não bloqueantes

Não bloqueantes para encerramento da Fase 4:

- `WebApplicationFactory` futuro;
- padronização Swagger futura;
- filtros/listagens avançadas futuras;
- paginação futura;
- endpoints por `PatientId` futuros;
- auditoria operacional futura;
- MedicalRecord será Fase 5.

## 14. Critérios de aceite da Fase 4

| Critério | Status |
|---|---|
| Decisão técnica inicial documentada | OK |
| Modelo Attendance definido | OK |
| Domain implementado/testado | OK |
| Application implementada/testada | OK |
| Infrastructure implementada/testada | OK |
| API implementada/testada | OK |
| DI configurado | OK |
| EF/AppDbContext validado | OK |
| Swagger/rotas/status documentados | OK |
| E2E manual via Swagger executado | OK |
| Build local validado | OK |
| Testes locais validados | OK |
| Documentação consolidada | OK |
| Sem pendência bloqueante | OK |

## 15. Decisão final da auditoria

**Opção A — Fase 4 aprovada para encerramento.**

Justificativa:
A vertical Attendance está implementada em todas as camadas planejadas, validada por testes automatizados, documentada por camada e validada manualmente via Swagger com autenticação real e banco local. As lacunas remanescentes são evolutivas e não bloqueiam o encerramento da Fase 4.

## 16. Fora do escopo desta auditoria

- alterar código;
- alterar testes;
- criar migrations;
- executar database update;
- alterar workflows;
- implementar MedicalRecord;
- implementar financeiro;
- implementar eventos;
- implementar cache;
- implementar frontend;
- implementar Docker/Kubernetes;
- implementar WebApplicationFactory.

## 17. Validação executada nesta fase

Comandos executados nesta fase:

- `git branch --show-current`;
- `git status`;
- `git diff --check`.

Comandos opcionais tentados no ambiente:

- `dotnet build backend/Togo.sln`;
- `dotnet test backend/Togo.sln`.

Resultado no ambiente Codex:

- `dotnet: command not found`.

Limitação de Git remoto observada:

- `git fetch origin` não pôde ser executado porque o remoto `origin` não está configurado neste ambiente;
- `main` também não existe localmente;
- a branch desta fase foi criada a partir da branch local disponível (`work`).

## 18. Próxima fase recomendada

Fase 4.6.2 — Encerramento oficial da Fase 4 e abertura da Fase 5 MedicalRecord.

Objetivo:
Gerar documento executivo final declarando a Fase 4 concluída e preparando o início da Fase 5 — MedicalRecord / Prontuário.
