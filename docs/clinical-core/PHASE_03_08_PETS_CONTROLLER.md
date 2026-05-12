# TOGO — Fase 3.8: PetsController

## 1. Objetivo da fase

A Fase 3.8 expõe o CRUD público de Pet/Patient via API HTTP, disponibilizando as operações essenciais de consulta, criação, atualização e remoção de pets por meio do controller `PetsController`.

O controller atua como camada de entrada HTTP para o fluxo de Pet/Patient e delega a execução das regras para os use cases da camada Application. A revisão técnica da Fase 3.8.2 confirmou que o controller não concentra regra de negócio e mantém o padrão já utilizado em `TutorsController` para conversão de resultados da aplicação em respostas HTTP.

## 2. Arquivo criado

Arquivo criado na Fase 3.8.1:

- `backend/src/Togo.Api/Controllers/PetsController.cs`

A revisão da Fase 3.8.2 foi documental e não alterou o arquivo do controller.

## 3. Endpoints criados

O `PetsController` usa a rota base `api/pets` e expõe os seguintes endpoints:

- `GET /api/pets`
  - Lista pets cadastrados.
  - Chama `ListPetsUseCase`.
  - Em sucesso, retorna `200 OK` com a lista.

- `GET /api/pets/{patientId}`
  - Consulta um pet pelo identificador público `PatientId`.
  - Chama `GetPetByIdUseCase`.
  - Em sucesso, retorna `200 OK` com os dados do pet.

- `POST /api/pets`
  - Cria um novo pet/patient.
  - Chama `CreatePetUseCase`.
  - Em sucesso, retorna `201 Created` via `CreatedAtAction`, apontando para `GetById` com `patientId`.

- `PUT /api/pets/{patientId}`
  - Atualiza um pet/patient existente pelo identificador público `PatientId`.
  - Chama `UpdatePetUseCase`.
  - Em sucesso, retorna `200 OK` com os dados atualizados.

- `DELETE /api/pets/{patientId}`
  - Remove um pet/patient existente pelo identificador público `PatientId`.
  - Chama `DeletePetUseCase`.
  - Em sucesso, retorna `204 No Content`.

A revisão confirmou os atributos obrigatórios do controller:

- `[Authorize]`
- `[ApiController]`
- `[Route("api/pets")]`

## 4. Dependências injetadas

O `PetsController` injeta exclusivamente dependências da camada Application para o fluxo de Pet e o logger do próprio controller:

- `ListPetsUseCase`
- `GetPetByIdUseCase`
- `CreatePetUseCase`
- `UpdatePetUseCase`
- `DeletePetUseCase`
- `ILogger<PetsController>`

A revisão confirmou que não há injeção direta de repository, `AppDbContext` ou serviços de infraestrutura no controller.

## 5. Uso de PatientId

A revisão confirmou que `PatientId` é o identificador público utilizado nas rotas e nas operações HTTP de detalhe, atualização e exclusão.

Essa decisão é importante porque o modelo de Pet está associado ao conceito de Patient e não expõe um `PetId` público próprio para a API. Portanto:

- as rotas usam `{patientId}`;
- os métodos do controller recebem `patientId`;
- os use cases de detalhe, atualização e exclusão recebem `patientId`;
- o `CreatedAtAction` do `POST /api/pets` usa `new { patientId = result.Data!.PatientId }`;
- não foi identificado uso de `petId` no controller.

## 6. Conversão de ApplicationResult para HTTP

O controller converte `ApplicationResult<T>` para `IActionResult` por meio de métodos privados, mantendo a tradução de status HTTP na camada de API e a regra de negócio na camada Application.

Mapeamento geral aplicado às operações de listagem, detalhe, criação e atualização:

| `ApplicationResultType` | HTTP |
| --- | --- |
| `Success` | `200 OK` |
| `ValidationError` | `400 Bad Request` |
| `NotFound` | `404 Not Found` |
| `Conflict` | `409 Conflict` |
| Demais tipos, incluindo `Error` | `500 Internal Server Error` |

Observação para criação:

- quando `CreatePetUseCase` retorna sucesso, o controller retorna `201 Created` com `CreatedAtAction` antes de aplicar o mapeamento geral;
- quando a criação falha, o resultado é convertido pelo mesmo mapeamento geral.

Mapeamento aplicado ao `DELETE /api/pets/{patientId}`:

| `ApplicationResultType` | HTTP |
| --- | --- |
| `Success` | `204 No Content` |
| `ValidationError` | `400 Bad Request` |
| `NotFound` | `404 Not Found` |
| `Conflict` | `409 Conflict` |
| Demais tipos, incluindo `Error` | `500 Internal Server Error` |

A revisão também confirmou que `ApplicationResultType` contém os tipos esperados para esse mapeamento: `Success`, `NotFound`, `ValidationError`, `Conflict` e `Error`.

## 7. Logs e segurança

A revisão confirmou que os logs do `PetsController` são seguros para os pontos verificados na Fase 3.8.2.

Campos permitidos observados nos logs:

- `PatientId`
- `TutorId`
- `HasMicrochip`

Boas práticas confirmadas:

- o microchip completo não é registrado em logs;
- microchip parcial ou mascarado não é registrado em logs;
- payload completo de criação ou atualização não é registrado em logs;
- token JWT não é registrado em logs;
- dados clínicos detalhados não são registrados em logs pelo controller;
- `HasMicrochip` é usado apenas como indicador booleano de presença do microchip.

A revisão está alinhada com os use cases de Pet, que também registram identificadores técnicos e `HasMicrochip`, sem expor o valor do microchip.

## 8. Separação de responsabilidades

A revisão confirmou a separação de responsabilidades esperada para o controller:

- o controller não contém regra de negócio;
- o controller não acessa repository diretamente;
- o controller não acessa `AppDbContext`;
- o controller não cria entidades `Patient` ou `Pet` diretamente;
- o controller recebe a requisição HTTP, chama o use case correspondente e converte `ApplicationResult` para resposta HTTP;
- a lógica de validação e persistência permanece nos use cases, validators, mappings e repositories da camada Application/Infrastructure.

Também foi verificado que o desenho mantém coerência com o padrão de `TutorsController`, especialmente no uso de `[Authorize]`, `[ApiController]`, rota base, injeção de use cases e conversão de resultados de aplicação em respostas HTTP.

## 9. Validação esperada

Comandos esperados para validação técnica da fase:

```bash
dotnet build backend/Togo.sln
dotnet test backend/Togo.sln
```

Nesta revisão documental, também deve ser executado:

```bash
git diff --check
```

Resultado registrado pela Fase 3.8.2 no ambiente Codex:

- `git diff --check`: executado para validar ausência de problemas de whitespace no diff.
- `dotnet build backend/Togo.sln`: não executado com sucesso no ambiente Codex porque o comando `dotnet` não está disponível (`dotnet: command not found`).
- `dotnet test backend/Togo.sln`: não executado com sucesso no ambiente Codex porque o comando `dotnet` não está disponível (`dotnet: command not found`).

Após a revisão documental, o desenvolvedor humano executou validação local em ambiente com .NET disponível, confirmando build e testes com sucesso.

Validação humana local registrada:

- `dotnet build backend/Togo.sln`
  - Resultado: sucesso.
- `dotnet test backend/Togo.sln`
  - Resultado: sucesso.
  - Total: 45 testes.
  - Falhou: 0.
  - Bem-sucedido: 45.
  - Ignorado: 0.

## 10. Pontos de atenção futuros

Pontos que permanecem para fases futuras:

- testes manuais no Swagger/Postman ainda precisam ser feitos;
- testes automatizados de API ainda podem ser criados em fase futura;
- paginação e filtros ainda não fazem parte destes endpoints;
- o delete físico será reavaliado antes da evolução para Atendimento/Prontuário.

## 11. Fechamento da Fase 3.8

A Fase 3.8 criou o `PetsController` para expor o CRUD público de Pet/Patient via API HTTP, utilizando `PatientId` como identificador público e delegando a execução das regras aos use cases da camada Application.

A revisão técnica da Fase 3.8.2 confirmou:

- presença dos atributos `[Authorize]`, `[ApiController]` e `[Route("api/pets")]`;
- exposição dos cinco endpoints planejados;
- injeção dos cinco use cases de Pet e de `ILogger<PetsController>`;
- uso correto de `PatientId` nas rotas e no `CreatedAtAction`;
- ausência de `petId` no controller;
- conversão adequada de `ApplicationResult` para respostas HTTP;
- logs sem exposição de microchip completo/parcial ou payload completo;
- ausência de acesso direto a repository ou `AppDbContext` no controller;
- manutenção da lógica de negócio nos use cases.

A Fase 3.8 também foi validada localmente pelo desenvolvedor humano em ambiente com .NET disponível, com `dotnet build backend/Togo.sln` bem-sucedido e `dotnet test backend/Togo.sln` concluído com 45/45 testes passando.

Próxima etapa recomendada:

**Fase 3.9 — Revisar logs do fluxo Pet e executar validação manual inicial no Swagger/Postman.**
