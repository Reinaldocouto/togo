# TOGO — Fase 4.5.3: Validação de rotas, status codes, responses e Swagger da API Attendance

## 1. Objetivo

Esta fase revisa tecnicamente a API de Attendance exposta pelo `AttendancesController`, com foco em consistência de rotas, contratos HTTP (status codes e responses) e documentação Swagger/OpenAPI básica.

## 2. Contexto

- As camadas de Domain/Application/Infrastructure de Attendance já foram concluídas nas fases anteriores.
- O controller de Attendance foi criado na fase 4.5.1.
- Os testes unitários diretos de controller foram criados na fase 4.5.2.
- Esta fase 4.5.3 valida a consistência externa da API sem expandir escopo funcional.

## 3. Rotas validadas

Validação concluída para os 5 endpoints atuais, mantendo o desenho existente:

- `GET /api/attendances`
- `GET /api/attendances/{id:long}`
- `POST /api/attendances`
- `PATCH /api/attendances/{id:long}/close`
- `PATCH /api/attendances/{id:long}/cancel`

Resultado da revisão:

- Rota base `api/attendances` está correta e em plural inglês.
- Uso de `{id:long}` está consistente com o padrão de rotas por identificador no projeto.
- Uso de `PATCH` para transições de estado (`close` e `cancel`) está tecnicamente coerente.
- Não foram identificadas rotas conflitantes dentro do controller.
- O padrão geral está coerente com a abordagem utilizada em `PetsController` e `TutorsController`.

## 4. Status codes validados

### `GET /api/attendances`

- `200 OK`.

### `GET /api/attendances/{id:long}`

- `200 OK`.
- `400 BadRequest`.
- `404 NotFound`.

### `POST /api/attendances`

- `201 Created`.
- `400 BadRequest`.
- `404 NotFound`.
- `409 Conflict`.

### `PATCH /api/attendances/{id:long}/close`

- `200 OK`.
- `400 BadRequest`.
- `404 NotFound`.
- `409 Conflict`.

### `PATCH /api/attendances/{id:long}/cancel`

- `200 OK`.
- `400 BadRequest`.
- `404 NotFound`.
- `409 Conflict`.

## 5. Responses validados

- `GET /api/attendances` retorna `IReadOnlyList<AttendanceListItemResponse>` quando sucesso.
- `GET /api/attendances/{id:long}` retorna `AttendanceResponse` quando sucesso.
- `POST /api/attendances` retorna `AttendanceResponse` com `CreatedAtAction` quando sucesso.
- `PATCH /api/attendances/{id:long}/close` retorna `AttendanceResponse` quando sucesso.
- `PATCH /api/attendances/{id:long}/cancel` retorna `AttendanceResponse` quando sucesso.
- Erros seguem padrão simples no controller via payload anônimo com chave `message` (ex.: `{ message = result.Error }`), coerente com o padrão já utilizado em outros controllers.

## 6. CreatedAtAction

No endpoint `POST /api/attendances`:

- É utilizado `CreatedAtAction`.
- `ActionName` aponta para `GetById`.
- `route values` contém `id`.
- O corpo retorna `AttendanceResponse`.

Risco conhecido registrado:

- Em teste unitário com fake, o `Id` pode ficar `0` dependendo de como o fake mapeia a entidade em memória.
- Isso não representa necessariamente o comportamento real pós-persistência.
- No fluxo real com EF + banco, o identificador é gerado na persistência e refletido na entidade.

## 7. Swagger/OpenAPI

Estado atual identificado:

- `AddSwaggerGen` está configurado em `Program.cs`.
- `UseSwagger` e `UseSwaggerUI` estão habilitados em ambiente de desenvolvimento.
- Controllers revisados (`Attendances`, `Pets`, `Tutors`) não utilizam padrão explícito de `[ProducesResponseType]`.

Decisão da fase 4.5.3:

- **Não** foram adicionados atributos `[ProducesResponseType]` no `AttendancesController` para evitar introduzir padrão novo isolado nesta fase documental.
- Melhoria futura recomendada: padronizar documentação de respostas por atributo em todos os controllers de forma transversal.

## 8. Autorização

- `AttendancesController` usa `[Authorize]`, em linha com o padrão dos demais controllers de API.
- Testes unitários diretos de controller (fase 4.5.2) não validam pipeline real de autenticação/autorização JWT.
- Validação de autenticação real fica fora do escopo da fase 4.5.3.

## 9. Cobertura pelos testes da Fase 4.5.2

Cobertura existente confirmada para:

- `200` (`List`, `GetById`, `Close`, `Cancel`).
- `201` (`Create` com `CreatedAtAction`).
- `400` (`GetById` inválido, `Create` inválido, `Close` inválido, `Cancel` inválido).
- `404` (`GetById`, `Create`, `Close`, `Cancel`).
- `409` (`Create`, `Close`, `Cancel`).
- Verificação de `CreatedAtAction` (nome da action, route value e corpo).

Lacunas registradas:

- Não há teste E2E real.
- Não há suíte com `WebApplicationFactory`.
- Não há teste de autenticação/JWT real.
- Não há validação de Swagger renderizado.

## 10. Decisões técnicas

- Nenhuma nova rota avançada criada nesta fase.
- Sem paginação/filtros nesta fase.
- Sem endpoint `ListByPatientId` nesta fase.
- Sem alteração de regra de negócio.
- Sem migration/database update.

## 11. Lacunas e melhorias futuras

- Testes E2E usando `WebApplicationFactory`.
- Teste de autenticação real/JWT.
- Validação de Swagger renderizado.
- Possível padronização com `[ProducesResponseType]` de forma transversal.
- Refatoração dos testes em estilo one-line para multi-line visando legibilidade.
- Endpoint futuro por `patientId` (quando entrar no escopo).
- Paginação/filtros em listagem (quando entrar no escopo).

## 12. Fora do escopo

- Domain.
- Application.
- Infrastructure.
- Migrations.
- Database update.
- Novos endpoints.
- Paginação.
- Filtros.
- Autenticação real/JWT real.
- E2E.
- Swagger avançado.
- RabbitMQ.
- Redis.
- Docker.
- Kubernetes.
- Frontend.

## 13. Validação

Comandos executados nesta fase:

- `git branch --show-current`
- `git status`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Observações:

- Tentativa de sincronização com remoto (`git fetch origin` / `git pull origin main`) não foi possível no ambiente porque o remoto `origin` não está configurado.
- Build/test foram executados localmente neste ambiente.

## 14. Próxima fase recomendada

**Fase 4.5.4 — Testar fluxo ponta a ponta local/API.**

Objetivo:

Executar validação prática da API Attendance em ambiente local, testando `create/get/list/close/cancel` com autenticação, banco e endpoints reais.

Critérios de aceite da próxima fase:

1. Inspecionar `AttendancesController`.
2. Inspecionar padrão de `Pets/Tutors`.
3. Validar rotas.
4. Validar status codes.
5. Validar responses.
6. Validar `CreatedAtAction`.
7. Validar estado Swagger/OpenAPI.
8. Validar cobertura da Fase 4.5.2.
9. Criar documentação da fase.
10. Não criar migration.
11. Não executar database update.
12. Não alterar Domain.
13. Não alterar Application.
14. Não alterar Infrastructure.
15. Não criar endpoints novos.
16. Executar `git diff --check`.
17. Executar build/test se ambiente permitir.
