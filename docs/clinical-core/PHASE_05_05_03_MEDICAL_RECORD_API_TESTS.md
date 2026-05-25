# TOGO — Fase 5.5.3: Testes de API MedicalRecord

## Subfase 5.5 — API MedicalRecord

Planejamento:
- 5.5.1 — Planejamento da API MedicalRecord.
- 5.5.2 — Controller MedicalRecord / rotas orientadas por Patient.
- 5.5.3 — Testes de API MedicalRecord.
- 5.5.4 — Validação Swagger/HTTP manual.
- 5.5.5 — Documentação final da API MedicalRecord.
- 5.5.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Criar testes de API para `MedicalRecord`, validando comportamento HTTP dos endpoints GET/POST/PUT antes da validação manual em Swagger/Postman.

## Contexto

- O controller `MedicalRecordsController` já existe.
- Os endpoints são protegidos por `[Authorize]`.
- As rotas são orientadas por `Patient`.
- Domain/Application/Infrastructure já foram concluídos.
- Esta fase valida contrato HTTP (status/resposta), não regra interna de domínio.

## Testes criados

Arquivo criado:
- `backend/src/Togo.Api.Tests/MedicalRecords/MedicalRecordsControllerTests.cs`

### GET `/api/patients/{patientId}/medical-record`

Cobertura:
- `200 OK` quando prontuário existe.
- `400 BadRequest` quando `patientId = 0`.
- `401 Unauthorized` sem token.
- `404 NotFound` quando patient não existe.
- `404 NotFound` quando patient existe e prontuário não existe.

Validação de resposta de sucesso:
- `Id`, `PatientId`, `GeneralNotes`, `FlagsJson`, `UpdatedAt`.

### POST `/api/patients/{patientId}/medical-record`

Cobertura:
- `201 Created` quando criado com sucesso.
- `400 BadRequest` quando `patientId = 0`.
- `401 Unauthorized` sem token.
- `404 NotFound` quando patient não existe.
- `409 Conflict` quando patient já possui prontuário.
- payload com `GeneralNotes = null` e `FlagsJson = null` aceito.
- payload com campos em branco aceito e normalizado para `null`.

Validação de resposta de sucesso:
- `Id`, `PatientId`, `GeneralNotes`, `FlagsJson`, `UpdatedAt`.
- `Location` header presente (fluxo com `CreatedAtAction`).

### PUT `/api/patients/{patientId}/medical-record`

Cobertura:
- `200 OK` quando atualizado.
- `400 BadRequest` quando `patientId = 0`.
- `401 Unauthorized` sem token.
- `404 NotFound` quando patient não existe.
- `404 NotFound` quando prontuário não existe.
- update com campos em branco normalizando para `null`.

Validação de resposta de sucesso:
- `Id` original.
- `PatientId` original.
- `GeneralNotes` atualizado.
- `FlagsJson` atualizado.
- `UpdatedAt` atualizado.

## Decisões técnicas

- Testes implementados como testes de API HTTP.
- Padrão alinhado ao `Togo.Api.Tests` (xUnit, fakes de suporte, sem banco real).
- Uso de `TestServer` para pipeline HTTP real com autenticação/autorizações ativas.
- Sem MySQL real e sem banco real.
- Autenticação de teste dedicada; cenário `401` validado sem token.
- Dados clínicos usados são totalmente fictícios.
- Não houve criação de migration.
- Não houve alteração de banco real.

## Segurança e privacidade

- Nenhum dado clínico real foi usado.
- `GeneralNotes`/`FlagsJson` são exemplos fictícios.
- Endpoints permanecem autenticados.
- Cenário `401 Unauthorized` foi coberto.
- Nenhum endpoint público foi criado.

## Pontos de atenção

- Roles/permissões finas seguem fora do escopo.
- Soft Delete e AuditLog seguem fora do escopo.
- Índice único em `PatientId` segue fora do escopo.
- Validação manual Swagger/HTTP ficará para a fase 5.5.4.
- Endpoint por Id segue fora do MVP.
- Endpoint de listagem segue fora do MVP.

## Critérios de aceite

Checklist:
- [x] Testes de API GET criados.
- [x] Testes de API POST criados.
- [x] Testes de API PUT criados.
- [x] Cenários 200/201 cobertos.
- [x] Cenários 400 cobertos.
- [x] Cenário 401 coberto.
- [x] Cenários 404 cobertos.
- [x] Cenário 409 coberto.
- [x] Respostas validadas.
- [ ] Build passou (bloqueado no ambiente atual por ausência do SDK `dotnet`).
- [ ] Testes passaram (bloqueado no ambiente atual por ausência do SDK `dotnet`).
- [x] Nenhuma migration criada.
- [x] Nenhum banco real alterado.
- [x] Documentação da fase criada.

## Fora do escopo

Esta fase não implementa:
- validação Swagger/manual;
- controller novo além do já existente;
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

**Fase 5.5.4 — Validação Swagger/HTTP manual.**

Objetivo:
Executar validação manual via Swagger/Postman dos endpoints MedicalRecord, incluindo autenticação, criação, consulta, atualização, duplicidade, not found e payloads nulos/brancos.

## Validações obrigatórias

Comandos executados:
- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`
