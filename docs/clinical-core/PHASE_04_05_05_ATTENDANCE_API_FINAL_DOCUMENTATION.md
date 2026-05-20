# TOGO — Fase 4.5.5: Documentação Final da API Attendance

## 1. Objetivo

Esta fase consolida a camada API da vertical Attendance, documentando de forma final o controller REST, os endpoints mínimos, o registro de dependências (DI), os testes de API/controller, a validação documental de rotas/status/responses/Swagger, o roteiro de validação E2E local/API, as lacunas conhecidas, os riscos e os próximos passos.

## 2. Contexto

- O **Domain** de Attendance foi concluído na Fase 4.2.
- A **Application** de Attendance foi concluída na Fase 4.3.
- A **Infrastructure** de Attendance foi concluída na Fase 4.4.
- A **API** de Attendance foi implementada e validada ao longo da Fase 4.5.
- Esta fase 4.5.5 fecha formalmente a camada API antes da próxima macrofase.

## 3. Escopo consolidado da Fase 4.5

### 4.5.1 — AttendanceController com endpoints mínimos
- **Objetivo:** expor endpoints REST mínimos de Attendance.
- **Arquivos principais:** `backend/src/Togo.Api/Controllers/AttendancesController.cs`, documentação da fase 4.5.1.
- **Resultado:** controller com rotas de list/get/create/close/cancel.
- **Decisão técnica:** mapear `ApplicationResult` para respostas HTTP no próprio controller, preservando padrão já usado no projeto.

### 4.5.2 — Testes de API/controller
- **Objetivo:** validar tradução de resultado de use case para HTTP na camada controller.
- **Arquivos principais:** `backend/src/Togo.Api.Tests/Controllers/AttendancesControllerTests.cs`, documentação da fase 4.5.2.
- **Resultado:** suíte de testes diretos do controller cobrindo cenários de sucesso e erro.
- **Decisão técnica:** usar testes unitários diretos sem `WebApplicationFactory`, sem novos pacotes, com fakes locais mínimos.

### 4.5.3 — Validação de rotas/status/responses/Swagger
- **Objetivo:** validar coerência da superfície HTTP e documentação automática.
- **Arquivos principais:** `AttendancesController.cs`, `Program.cs`, documentação da fase 4.5.3.
- **Resultado:** rotas/status/responses coerentes e Swagger ativo em ambiente de desenvolvimento.
- **Decisão técnica:** manter alinhamento com o padrão atual de Pets/Tutors e não introduzir `[ProducesResponseType]` isoladamente apenas para Attendance.

### 4.5.3.1 — Correção documental de validação
- **Objetivo:** corrigir/clarificar registro documental de build/test da fase 4.5.3.
- **Arquivos principais:** documentação da fase 4.5.3 e 4.5.3.1.
- **Resultado:** rastreabilidade de validação documental ajustada.
- **Decisão técnica:** separar explicitamente validação documental de execução real HTTP ponta a ponta.

### 4.5.4 — Roteiro de validação local/API E2E
- **Objetivo:** disponibilizar roteiro operacional de validação manual da API Attendance.
- **Arquivos principais:** documentação da fase 4.5.4, `AuthController.cs`, `AttendancesController.cs`.
- **Resultado:** fluxo de login + token + create/get/list/close/cancel documentado com cenários positivos e negativos.
- **Decisão técnica:** priorizar guia executável por humanos com `curl` antes de automatização E2E completa.

## 4. Controller criado

- **Arquivo:** `backend/src/Togo.Api/Controllers/AttendancesController.cs`.
- **Namespace:** `Togo.Api.Controllers`.
- **Rota base:** `[Route("api/attendances")]`.
- **Atributos:**
  - `[Authorize]`;
  - `[ApiController]`.
- **Herança:** `ControllerBase`.
- **Use cases injetados:**
  - `CreateAttendanceUseCase`;
  - `GetAttendanceByIdUseCase`;
  - `ListAttendancesUseCase`;
  - `CloseAttendanceUseCase`;
  - `CancelAttendanceUseCase`.
- **Observabilidade:** uso de `ILogger<AttendancesController>` em todos os endpoints.

## 5. Endpoints consolidados

### GET /api/attendances
- **Use case:** `ListAttendancesUseCase`.
- **Response:** lista de `AttendanceListItemResponse`.
- **Status esperado:** `200 OK`.

### GET /api/attendances/{id:long}
- **Use case:** `GetAttendanceByIdUseCase`.
- **Response:** `AttendanceResponse`.
- **Status esperados:** `200`, `400`, `404`.

### POST /api/attendances
- **Use case:** `CreateAttendanceUseCase`.
- **Request:** `CreateAttendanceRequest`.
- **Response:** `AttendanceResponse`.
- **Comportamento:** usa `CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)`.
- **Status esperados:** `201`, `400`, `404`, `409`.

### PATCH /api/attendances/{id:long}/close
- **Use case:** `CloseAttendanceUseCase`.
- **Request:** `CloseAttendanceRequest`.
- **Response:** `AttendanceResponse`.
- **Status esperados:** `200`, `400`, `404`, `409`.

### PATCH /api/attendances/{id:long}/cancel
- **Use case:** `CancelAttendanceUseCase`.
- **Request:** sem body.
- **Response:** `AttendanceResponse`.
- **Status esperados:** `200`, `400`, `404`, `409`.

## 6. DI e dependências

No `Program.cs`, a API registra em `AddScoped`:

- use cases de Attendance (`Create`, `GetById`, `List`, `Close`, `Cancel`);
- validators de Attendance (`AttendancePatientExistsValidator`, `AttendanceNumberUniqueValidator`, `OpenAttendanceValidator`);
- repositório via interface `IAttendanceRepository` com implementação `AttendanceRepository`.

A estratégia segue o mesmo padrão de DI já aplicado para as verticais de Pets/Tutors.

## 7. Mapeamento ApplicationResult -> HTTP

Mapeamento consolidado no controller:

- `Success` -> `200 OK`;
- sucesso de criação -> `201 Created` (`CreatedAtAction`);
- `ValidationError` -> `400 BadRequest`;
- `NotFound` -> `404 NotFound`;
- `Conflict` -> `409 Conflict`;
- fallback -> `500 InternalServerError`.

## 8. Testes de API/controller

Estratégia consolidada da fase 4.5.2:

- testes unitários diretos do controller;
- sem `WebApplicationFactory`;
- sem inclusão de pacote novo;
- uso de fakes locais mínimos para repository/dependências;
- foco em tradução HTTP e comportamento de `CreatedAtAction`.

Cenários cobertos:

- list success;
- get success;
- get bad request;
- get not found;
- create created;
- create bad request;
- create not found;
- create conflict;
- close success;
- close bad request;
- close not found;
- close conflict;
- cancel success;
- cancel bad request;
- cancel not found;
- cancel conflict.

Correções/aprendizados registrados:

- fake de `PetDetailsProjection` deve usar argumentos nomeados;
- lookup artificial em teste não deve ser confundido com o `Id` persistido real da entidade.

## 9. Rotas/status/responses/Swagger

Conclusão consolidada da fase 4.5.3:

- rotas de Attendance estão coerentes com o contrato REST definido;
- status codes estão coerentes com os resultados da Application;
- responses usam contracts da Application (`AttendanceResponse`/`AttendanceListItemResponse`);
- `CreatedAtAction` está coerente com endpoint de detalhe (`GetById`);
- Swagger está habilitado via `AddSwaggerGen` e `UseSwagger/UseSwaggerUI` em ambiente de desenvolvimento;
- não foi adicionado `[ProducesResponseType]` por ausência de padrão transversal equivalente em Pets/Tutors;
- melhoria futura: padronizar `[ProducesResponseType]` em todos os controllers.

## 10. Validação local/API E2E

Consolidação da fase 4.5.4:

- roteiro local/API foi criado;
- autenticação inicia por `POST /api/Auth/login`;
- fluxo protegido com Bearer token;
- sequência principal documentada: create -> get -> list -> close -> cancel;
- cenários negativos documentados;
- exemplos `curl` documentados;
- evidências esperadas definidas.

Registro explícito desta fase:

- o roteiro E2E foi documentado;
- a execução HTTP real da API **não** foi registrada como concluída nesta PR documental;
- build/test local humano só pode ser citado com evidência externa ao Codex.

Nota consolidada:

Conforme validação humana informada após a fase 4.5.4, `dotnet build backend/Togo.sln` e `dotnet test backend/Togo.sln` passaram localmente com **182 testes, 0 falhas e 0 ignorados**. Essa validação cobre build/test automatizado, mas **não substitui** execução HTTP E2E real dos endpoints.

## 11. Estado final da API Attendance

Ao final da Fase 4.5, a vertical Attendance possui:

- controller REST;
- 5 endpoints mínimos;
- DI de use cases/validators/repository;
- testes unitários de controller;
- validação documental de rotas/status/responses/Swagger;
- roteiro E2E local/API;
- documentação final consolidada.

Ainda não existe:

- `WebApplicationFactory`;
- E2E automatizado real;
- teste JWT real;
- Swagger avançado com `[ProducesResponseType]`;
- paginação/filtros;
- endpoint `ListByPatientId`;
- frontend consumindo a API Attendance;
- execução documentada de fluxo HTTP real ponta a ponta.

## 12. Lacunas conhecidas

- ausência de E2E automatizado com `WebApplicationFactory`;
- ausência de teste de autenticação real/JWT;
- ausência de validação HTTP real registrada em evidência desta fase;
- testes de controller ainda com fakes manuais;
- alguns testes podem ser refatorados para formato multi-line visando legibilidade;
- ausência de paginação/filtros;
- ausência de endpoint por `PatientId`;
- ausência de `[ProducesResponseType]` padronizado;
- dependência de banco local e usuário válido para execução manual completa.

## 13. Riscos

- possível divergência entre teste unitário de controller e pipeline real do ASP.NET;
- autenticação real não coberta por teste de integração/E2E;
- banco local pode estar fora de sincronia no momento do teste manual;
- enum em payload pode exigir string ou número conforme configuração de serialização JSON;
- `CreatedAtAction` depende de `Id` real pós-persistência;
- E2E manual sem evidência pode gerar falsa sensação de validação completa.

## 14. Fora do escopo

- Domain;
- Application;
- Infrastructure;
- migrations;
- database update;
- novos endpoints;
- paginação;
- filtros;
- `ListByPatientId`;
- `WebApplicationFactory`;
- JWT real;
- Swagger avançado;
- RabbitMQ;
- Redis;
- Docker;
- Kubernetes;
- Frontend.

## 15. Validação

Comandos esperados/registrados para esta fase:

- `git branch --show-current`
- `git status`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Se o ambiente Codex não tiver `dotnet`, registrar exatamente:

- `dotnet: command not found`

Diretriz aplicada:

- não declarar sucesso de build/test no ambiente Codex sem execução real no próprio ambiente.

Registros complementares possíveis:

- CI Backend das PRs da Fase 4.5 passou;
- validação local humana informada: 182 testes, 0 falhas.

## 16. Próxima fase recomendada

- **Fase 4.6 — Fechamento técnico da vertical Attendance / preparação para próximos módulos.**

Sugestão de início:

- **Fase 4.6.1 — Auditoria final da vertical Attendance.**

Objetivo:

Revisar Domain, Application, Infrastructure, API, testes, documentação, lacunas e riscos da vertical Attendance antes do início de novas funcionalidades clínicas e/ou integrações.
