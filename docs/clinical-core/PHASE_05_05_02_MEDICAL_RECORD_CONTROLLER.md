# TOGO — Fase 5.5.2: Controller MedicalRecord / rotas orientadas por Patient

## Resumo da Subfase 5.5

**Subfase 5.5 — API MedicalRecord**

Planejamento:
- 5.5.1 — Planejamento da API MedicalRecord.
- 5.5.2 — Controller MedicalRecord / rotas orientadas por Patient.
- 5.5.3 — Testes de API MedicalRecord.
- 5.5.4 — Validação Swagger/HTTP manual.
- 5.5.5 — Documentação final da API MedicalRecord.
- 5.5.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Implementar o controller HTTP de MedicalRecord, conforme o planejamento da fase 5.5.1, com rotas orientadas por Patient e uso exclusivo de use cases da camada Application.

## Contexto

- Domain, Application e Infrastructure de MedicalRecord já foram concluídos.
- As dependências de MedicalRecord já estavam registradas em DI.
- A API planejada para MedicalRecord é orientada por Patient.
- Prontuário não é atendimento.
- `patientId` vem pela rota.
- Dados clínicos são sensíveis e exigem cuidado em logs e exposição.

## Controller criado

- Arquivo criado: `backend/src/Togo.Api/Controllers/MedicalRecordsController.cs`.
- Nome do controller: `MedicalRecordsController`.
- Rota base: `api/patients/{patientId:long}/medical-record`.
- Atributos aplicados:
  - `[ApiController]`
  - `[Authorize]`
  - `[Route("api/patients/{patientId:long}/medical-record")]`
- Dependências injetadas:
  - `CreateMedicalRecordUseCase`
  - `GetMedicalRecordByPatientIdUseCase`
  - `UpdateMedicalRecordUseCase`
  - `ILogger<MedicalRecordsController>`
- Endpoints implementados:
  - `GET /api/patients/{patientId}/medical-record`
  - `POST /api/patients/{patientId}/medical-record`
  - `PUT /api/patients/{patientId}/medical-record`

## Endpoints implementados

### GET /api/patients/{patientId}/medical-record

- Use case: `GetMedicalRecordByPatientIdUseCase`.
- Request: `patientId` via rota.
- Response: `MedicalRecordResponse` em caso de sucesso.
- Status codes:
  - `200 OK`
  - `400 Bad Request`
  - `401 Unauthorized`
  - `404 Not Found`
  - `500 Internal Server Error` (fallback)

### POST /api/patients/{patientId}/medical-record

- Use case: `CreateMedicalRecordUseCase`.
- Request:
  - `patientId` via rota.
  - body: `CreateMedicalRecordRequest`.
- Response: `MedicalRecordResponse` em caso de sucesso.
- Status codes:
  - `201 Created`
  - `400 Bad Request`
  - `401 Unauthorized`
  - `404 Not Found`
  - `409 Conflict`
  - `500 Internal Server Error` (fallback)
- Decisão de criação:
  - Foi utilizado `CreatedAtAction(nameof(GetByPatientIdAsync), new { patientId = result.Data!.PatientId }, result.Data)`, mantendo o padrão REST e apontando para o GET orientado por patient.

### PUT /api/patients/{patientId}/medical-record

- Use case: `UpdateMedicalRecordUseCase`.
- Request:
  - `patientId` via rota.
  - body: `UpdateMedicalRecordRequest`.
- Response: `MedicalRecordResponse` em caso de sucesso.
- Status codes:
  - `200 OK`
  - `400 Bad Request`
  - `401 Unauthorized`
  - `404 Not Found`
  - `500 Internal Server Error` (fallback)

## Mapeamento ApplicationResult para HTTP

Padrão implementado, alinhado aos controllers existentes:

- `Success`:
  - GET -> `Ok(result.Data)`
  - POST -> `CreatedAtAction(...)`
  - PUT -> `Ok(result.Data)`
- `ValidationError` -> `BadRequest(new { message = result.Error })`
- `NotFound` -> `NotFound(new { message = result.Error })`
- `Conflict` -> `Conflict(new { message = result.Error })`
- fallback -> `StatusCode(500, new { message = result.Error ?? "Unexpected error." })`

## Segurança e privacidade

- `[Authorize]` aplicado no controller.
- JWT obrigatório para todos os endpoints.
- Nenhum endpoint público criado.
- Nenhuma listagem ampla de prontuários implementada.
- Logs não expõem `GeneralNotes` ou `FlagsJson`.
- Controller não acessa `AppDbContext`.
- Controller não acessa repository diretamente.

## Swagger/OpenAPI

- Foram adicionados `ProducesResponseType` no controller, pois o padrão do projeto já utiliza o recurso (ex.: `AuthController`).
- Cobertura aplicada:
  - GET: `200`, `400`, `401`, `404`
  - POST: `201`, `400`, `401`, `404`, `409`
  - PUT: `200`, `400`, `401`, `404`
- Não foram adicionados exemplos com dados sensíveis reais.

## Pontos de atenção

- Testes de API serão implementados na fase 5.5.3.
- Validação Swagger/HTTP manual será executada na fase 5.5.4.
- Roles/permissões finas permanecem fora do escopo atual.
- Soft Delete e AuditLog permanecem fora do escopo atual.
- Índice único em `PatientId` ainda não foi implementado.
- Endpoint por Id permanece fora do MVP.

## Critérios de aceite

A fase é considerada concluída porque:

- `MedicalRecordsController.cs` foi criado.
- O controller possui `[ApiController]`.
- O controller possui `[Authorize]`.
- A rota base é `api/patients/{patientId:long}/medical-record`.
- GET foi implementado.
- POST foi implementado.
- PUT foi implementado.
- O controller usa use cases da Application.
- O controller não acessa `AppDbContext`.
- O controller não acessa repository diretamente.
- `ApplicationResult` foi mapeado para HTTP.
- `GeneralNotes`/`FlagsJson` não são logados.
- Build/test foram executados como validação obrigatória, porém não concluíram neste ambiente por ausência do SDK `dotnet` (`dotnet: command not found`).
- Nenhuma migration foi criada.
- A documentação da fase foi criada.

## Fora do escopo

Esta fase não implementa:

- testes de API;
- validação Swagger/manual;
- frontend;
- endpoint de listagem;
- endpoint por Id;
- Soft Delete;
- AuditLog;
- roles/permissões finas;
- migration;
- database update;
- índice único;
- Redis;
- RabbitMQ;
- Docker.

## Próxima fase recomendada

**Fase 5.5.3 — Testes de API MedicalRecord.**

Objetivo:
Criar testes de API para os endpoints GET, POST e PUT de MedicalRecord, cobrindo sucesso, validação, ausência de token, not found e conflict.

## Validações obrigatórias

Comandos executados nesta fase:
- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`
