# TOGO — Fase 5.5.1: Planejamento da API MedicalRecord

## 2. Resumo da Subfase 5.5

**Subfase 5.5 — API MedicalRecord**

Planejamento:
- 5.5.1 — Planejamento da API MedicalRecord.
- 5.5.2 — Controller MedicalRecord / rotas orientadas por Patient.
- 5.5.3 — Testes de API MedicalRecord.
- 5.5.4 — Validação Swagger/HTTP manual.
- 5.5.5 — Documentação final da API MedicalRecord.
- 5.5.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## 3. Objetivo

Esta fase define, de forma exclusivamente documental, o planejamento da API HTTP de `MedicalRecord` antes da implementação de controller/endpoints.

A fase deve definir:
- rotas;
- métodos HTTP;
- request/response;
- status codes;
- segurança/autorização;
- mapeamento de `ApplicationResult` para HTTP;
- regras de logging seguro;
- Swagger/OpenAPI futuro;
- validação manual futura;
- fora de escopo.

## 4. Contexto

- Domain `MedicalRecord` foi concluído na subfase 5.2.
- Application `MedicalRecord` foi concluída na subfase 5.3.
- Infrastructure `MedicalRecord` foi concluída na subfase 5.4.
- A vertical está pronta para ganhar API.
- Prontuário não é atendimento.
- `MedicalRecord` pertence ao `Patient`.
- `PatientId` deve vir pela rota.
- Dados clínicos são sensíveis.
- Esta fase não implementa controller.

## 5. Decisão de desenho da API

### Decisão

A API `MedicalRecord` será orientada por `Patient`.

Rotas planejadas:
- `GET /api/patients/{patientId}/medical-record`
- `POST /api/patients/{patientId}/medical-record`
- `PUT /api/patients/{patientId}/medical-record`

Justificativa:
- `MedicalRecord` é o prontuário principal longitudinal do `Patient`.
- Um `Patient` deve ter no máximo um `MedicalRecord` principal.
- O fluxo clínico parte do paciente.
- Evita tratar prontuário como recurso solto sem contexto clínico.
- Reforça a decisão “Prontuário não é Atendimento”.

### Rotas não escolhidas no MVP

- `GET /api/medical-records/{id}`
- `GET /api/medical-records/by-patient/{patientId}`
- `POST /api/medical-records`
- `PUT /api/medical-records/{id}`

Essas rotas ficam fora do MVP por serem menos alinhadas ao fluxo clínico atual, que é orientado por paciente.

## 6. Controller planejado

Controller futuro recomendado:
- `MedicalRecordsController`

Rota base recomendada:
- `[Route("api/patients/{patientId:long}/medical-record")]`

Atributos esperados:
- `[ApiController]`
- `[Authorize]`
- `[Route("api/patients/{patientId:long}/medical-record")]`

Dependências planejadas:
- `CreateMedicalRecordUseCase`
- `GetMedicalRecordByPatientIdUseCase`
- `UpdateMedicalRecordUseCase`
- `ILogger<MedicalRecordsController>`

Regras:
- não acessar `AppDbContext`;
- não acessar repository diretamente;
- não conter regra de negócio;
- não retornar entidade de domínio diretamente;
- usar contracts de Application;
- converter `ApplicationResult` para HTTP;
- não logar `GeneralNotes`/`FlagsJson`.

## 7. Endpoints planejados

### 7.1 GET /api/patients/{patientId}/medical-record

Use case:
- `GetMedicalRecordByPatientIdUseCase`

Request:
- route: `patientId`

Response:
- `MedicalRecordResponse`

Status codes:
- `200 OK` quando encontrado;
- `400 Bad Request` para validation error;
- `401 Unauthorized` sem token;
- `404 Not Found` para patient inexistente ou prontuário inexistente;
- `500` fallback inesperado.

### 7.2 POST /api/patients/{patientId}/medical-record

Use case:
- `CreateMedicalRecordUseCase`

Request:
- route: `patientId`
- body: `CreateMedicalRecordRequest`

Response:
- `MedicalRecordResponse`

Status codes:
- `201 Created` quando criado;
- `400 Bad Request` para validation error;
- `401 Unauthorized` sem token;
- `404 Not Found` para patient inexistente;
- `409 Conflict` quando `Patient` já possui prontuário;
- `500` fallback inesperado.

Observação:
- Definir se `CreatedAtAction` será usado.
- Como não haverá rota por `Id` no MVP, avaliar retorno `201` com `Location` baseado no `GET` por `patientId`.

### 7.3 PUT /api/patients/{patientId}/medical-record

Use case:
- `UpdateMedicalRecordUseCase`

Request:
- route: `patientId`
- body: `UpdateMedicalRecordRequest`

Response:
- `MedicalRecordResponse`

Status codes:
- `200 OK` quando atualizado;
- `400 Bad Request` para validation error;
- `401 Unauthorized` sem token;
- `404 Not Found` para patient/prontuário inexistente;
- `500` fallback inesperado.

## 8. Mapeamento ApplicationResult para HTTP

Mapeamento planejado:

`ApplicationResultType.Success`:
- GET -> `200`
- POST -> `201`
- PUT -> `200`

`ApplicationResultType.ValidationError`:
- `400 Bad Request`

`ApplicationResultType.NotFound`:
- `404 Not Found`

`ApplicationResultType.Conflict`:
- `409 Conflict`

Outros/inesperados:
- `500` ou fallback existente do projeto

Atenção:
- Seguir o padrão já usado em `AttendancesController`, `PetsController` e `TutorsController`.
- Não inventar novo envelope de erro se o projeto já possui padrão.

## 9. Segurança e autorização

- endpoints devem usar `[Authorize]`;
- token JWT obrigatório;
- sem token deve retornar `401`;
- roles/permissões finas ainda não serão implementadas nesta fase;
- em fase futura, prontuário pode exigir permissão mais restrita que cadastro básico;
- logs não devem expor `GeneralNotes`/`FlagsJson`;
- Swagger não deve conter exemplos com dados reais;
- erros técnicos não devem expor payload clínico.

## 10. Privacidade e LGPD

- `MedicalRecord` contém dados clínicos sensíveis no contexto veterinário e dados vinculados ao tutor/responsável;
- `GeneralNotes` e `FlagsJson` devem ser tratados com cuidado;
- API deve retornar dados apenas para usuários autenticados;
- não deve haver endpoint público;
- não deve haver listagem ampla de prontuários no MVP;
- listagens futuras devem usar `MedicalRecordListItemResponse`;
- payloads clínicos não devem ser logados.

## 11. Swagger/OpenAPI planejado

Na implementação futura do controller deve haver:
- `ProducesResponseType` para `200/201/400/401/404/409` quando aplicável;
- `summary/remarks` se o padrão do projeto usar;
- ausência de exemplos reais sensíveis;
- descrição clara de que `patientId` vem pela rota;
- documentação dos bodies `CreateMedicalRecordRequest` e `UpdateMedicalRecordRequest`.

## 12. Testes de API planejados

Cobertura futura da 5.5.3.

GET:
- `200` com prontuário existente;
- `400` `patientId` inválido;
- `401` sem token;
- `404` patient inexistente;
- `404` prontuário inexistente.

POST:
- `201` criado;
- `400` `patientId` inválido;
- `400` body inválido, se aplicável;
- `401` sem token;
- `404` patient inexistente;
- `409` prontuário duplicado.

PUT:
- `200` atualizado;
- `400` `patientId` inválido;
- `401` sem token;
- `404` patient inexistente;
- `404` prontuário inexistente.

## 13. Validação HTTP manual planejada

Fluxo 5.5.4 via Swagger/Postman.

Fluxo positivo:
1. Autenticar.
2. Criar tutor, se necessário.
3. Criar patient/pet.
4. Confirmar `patientId`.
5. POST criar prontuário.
6. GET consultar prontuário.
7. PUT atualizar prontuário.
8. GET consultar novamente.
9. Validar dados atualizados.
10. Validar que segunda criação retorna `409`.

Fluxos negativos:
- sem token;
- `patientId = 0`;
- `patientId` inexistente;
- prontuário inexistente;
- criação duplicada;
- body vazio;
- body com campos nulos;
- body com campos em branco.

## 14. Pontos de atenção

- API ainda não terá roles/permissões finas.
- API ainda não implementará Soft Delete.
- API ainda não implementará AuditLog.
- API ainda não resolverá índice único no banco.
- API deve evitar listagem ampla de prontuários.
- API deve evitar logar payload clínico.
- `CreatedAt` ainda não existe em `MedicalRecord`.
- `CancellationToken` ainda não é propagado no repository.

## 15. Riscos aceitos temporariamente

- acesso ainda protegido apenas por autenticação JWT genérica;
- sem permissão granular específica para prontuário;
- duplicidade física ainda possível em concorrência;
- ausência de Soft Delete;
- ausência de AuditLog;
- `FlagsJson` ainda flexível;
- `DeleteBehavior.Cascade` ainda pendente;
- ausência de endpoint por Id pode limitar integração futura, mas é aceitável no MVP.

## 16. Critérios de aceite da Fase 5.5.1

A fase será considerada concluída se:
- documento de planejamento da API for criado;
- subfase 5.5 tiver resumo inicial;
- rotas forem definidas;
- controller planejado for definido;
- endpoints forem planejados;
- status codes forem definidos;
- segurança/autorização forem documentadas;
- privacidade/LGPD forem documentadas;
- Swagger/OpenAPI futuro for planejado;
- testes de API forem planejados;
- validação HTTP manual for planejada;
- nenhuma implementação for feita;
- nenhum código/teste/migration/banco for alterado.

## 17. Fora do escopo

Esta fase não implementa:
- controller;
- endpoints;
- API;
- testes de API;
- Swagger attributes;
- `Program.cs`;
- DI;
- migrations;
- database update;
- banco real;
- Soft Delete;
- AuditLog;
- índice único;
- roles/permissões finas;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## 18. Próxima fase recomendada

**Fase 5.5.2 — Controller MedicalRecord / rotas orientadas por Patient.**

Objetivo:
Implementar `MedicalRecordsController` com rotas:
- `GET /api/patients/{patientId}/medical-record`
- `POST /api/patients/{patientId}/medical-record`
- `PUT /api/patients/{patientId}/medical-record`

Usando os use cases já registrados em DI, protegendo endpoints com `[Authorize]`, mapeando `ApplicationResult` para HTTP e sem acessar Infrastructure diretamente.

## 19. Validações obrigatórias

Validações executadas nesta fase:
- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln` (se disponível)
- `dotnet test backend/Togo.sln` (se disponível)
