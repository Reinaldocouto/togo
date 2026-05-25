# TOGO — Fase 5.5.5: Documentação final da API MedicalRecord

## 1. Título

TOGO — Fase 5.5.5: Documentação final da API MedicalRecord

## 2. Resumo da Subfase 5.5

Subfase 5.5 — API MedicalRecord

Planejamento:
- 5.5.1 — Planejamento da API MedicalRecord.
- 5.5.2 — Controller MedicalRecord / rotas orientadas por Patient.
- 5.5.3 — Testes de API MedicalRecord.
- 5.5.4 — Validação Swagger/HTTP manual.
- 5.5.5 — Documentação final da API MedicalRecord.
- 5.5.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## 3. Objetivo

Esta fase consolida o estado final da API de `MedicalRecord`, documentando:

- planejamento;
- controller;
- rotas;
- status codes;
- segurança/autorização;
- testes automatizados de API;
- roteiro manual Swagger/HTTP;
- correções aplicadas;
- riscos remanescentes;
- débitos técnicos;
- decisão final da subfase 5.5.

## 4. Contexto

- Domain foi concluído na subfase 5.2.
- Application foi concluída na subfase 5.3.
- Infrastructure foi concluída na subfase 5.4.
- API foi implementada na subfase 5.5.
- `MedicalRecord` pertence ao `Patient`.
- Prontuário não é Atendimento.
- Dados clínicos são sensíveis.
- Esta fase não implementa código.

## 5. Entregas consolidadas da subfase 5.5

### 5.1 Planejamento da API

Consolidação da fase documental `docs/clinical-core/PHASE_05_05_01_MEDICAL_RECORD_API_PLANNING.md`:

- decisão de API orientada por `Patient`;
- rotas planejadas:
  - `GET /api/patients/{patientId}/medical-record`;
  - `POST /api/patients/{patientId}/medical-record`;
  - `PUT /api/patients/{patientId}/medical-record`;
- status codes planejados por endpoint;
- diretrizes de segurança e privacidade planejadas;
- planejamento de Swagger/OpenAPI;
- planejamento de testes de API.

### 5.2 Controller MedicalRecord

Consolidação do arquivo `backend/src/Togo.Api/Controllers/MedicalRecordsController.cs`:

- atributos presentes:
  - `[ApiController]`;
  - `[Authorize]`;
  - `[Route("api/patients/{patientId:long}/medical-record")]`;
- rota base orientada por paciente: `api/patients/{patientId:long}/medical-record`;
- dependências injetadas:
  - `CreateMedicalRecordUseCase`;
  - `GetMedicalRecordByPatientIdUseCase`;
  - `UpdateMedicalRecordUseCase`;
  - `ILogger<MedicalRecordsController>`;
- endpoints implementados: GET, POST e PUT;
- uso exclusivo de use cases da Application;
- ausência de acesso direto a `AppDbContext`/repositories;
- ausência de logging de `GeneralNotes` e `FlagsJson` (logs com metadado de `PatientId`).

### 5.3 Endpoints implementados

#### GET /api/patients/{patientId}/medical-record

- objetivo: consultar prontuário principal do paciente;
- request: `patientId` via rota;
- response: `MedicalRecordResponse` em caso de sucesso;
- status codes:
  - `200 OK`;
  - `400 Bad Request`;
  - `401 Unauthorized`;
  - `404 Not Found`;
  - `500` fallback inesperado.

#### POST /api/patients/{patientId}/medical-record

- objetivo: criar prontuário principal do paciente;
- request:
  - `patientId` via rota;
  - body `CreateMedicalRecordRequest`;
- response: `MedicalRecordResponse` em caso de sucesso;
- status codes:
  - `201 Created`;
  - `400 Bad Request`;
  - `401 Unauthorized`;
  - `404 Not Found`;
  - `409 Conflict`;
  - `500` fallback inesperado;
- retorno de criação via `CreatedAtRoute` com rota nomeada `GetMedicalRecordByPatientId`;
- `Location` header apontando para o GET por `patientId`.

#### PUT /api/patients/{patientId}/medical-record

- objetivo: atualizar prontuário principal do paciente;
- request:
  - `patientId` via rota;
  - body `UpdateMedicalRecordRequest`;
- response: `MedicalRecordResponse` em caso de sucesso;
- status codes:
  - `200 OK`;
  - `400 Bad Request`;
  - `401 Unauthorized`;
  - `404 Not Found`;
  - `500` fallback inesperado.

### 5.4 Mapeamento ApplicationResult para HTTP

Mapeamento consolidado no controller:

- `Success` -> `200/201`;
- `ValidationError` -> `400`;
- `NotFound` -> `404`;
- `Conflict` -> `409`;
- fallback -> `500`.

### 5.5 Testes de API

Consolidação de `backend/src/Togo.Api.Tests/MedicalRecords/MedicalRecordsControllerTests.cs` e do projeto de testes:

- uso de `TestServer` para validar pipeline HTTP;
- pacote `Microsoft.AspNetCore.TestHost` adicionado ao `Togo.Api.Tests.csproj`;
- autenticação de teste dedicada (cenários com e sem token);
- fakes/in-memory repositories para isolamento de infraestrutura real;
- cobertura GET;
- cobertura POST;
- cobertura PUT;
- status validados: `200`, `201`, `400`, `401`, `404`, `409`;
- payloads nulos e brancos validados;
- `Location` header no POST validado;
- build/test local e CI: considerados aprovados conforme registros das fases anteriores e histórico documentado/PR da subfase.

### 5.6 Correções aplicadas na subfase

Correções consolidadas:

- adição de `Microsoft.AspNetCore.TestHost` para permitir uso de `TestServer` nos testes de API;
- correção de `CreatedAtAction` para `CreatedAtRoute` com rota nomeada no controller;
- motivo: evitar erro “No route matches the supplied values” no POST;
- impacto: preserva `201 Created` e `Location` header no fluxo de criação.

### 5.7 Validação manual Swagger/HTTP

Consolidação do arquivo `docs/clinical-core/PHASE_05_05_04_MEDICAL_RECORD_SWAGGER_HTTP_VALIDATION.md`:

- roteiro positivo documentado;
- roteiro negativo documentado;
- payloads fictícios documentados;
- checklists de segurança, privacidade e Swagger/OpenAPI documentados;
- modelo de evidência manual documentado.

Observação importante:

- testes automatizados validam comportamento técnico HTTP;
- validação manual confirma experiência real em Swagger/Postman;
- validação manual é recomendada antes de demonstração, mas não substitui testes automatizados.

## 6. Segurança e privacidade

- endpoints protegidos por `[Authorize]`;
- sem endpoint público de `MedicalRecord`;
- sem listagem ampla de prontuários no MVP;
- sem endpoint DELETE no MVP;
- sem endpoint por Id no MVP;
- logs não devem expor `GeneralNotes`/`FlagsJson`;
- payloads clínicos são sensíveis;
- exemplos documentados devem ser fictícios;
- tokens e dados reais não devem ser registrados em evidências.

## 7. Decisões técnicas finais da subfase 5.5

- API orientada por `Patient` foi mantida.
- Controller usa use cases.
- Controller não acessa Infrastructure diretamente.
- GET/POST/PUT são suficientes para o MVP.
- `CreatedAtRoute` foi escolhido para retorno `201`.
- `TestServer` foi usado para validar pipeline HTTP.
- Não implementar DELETE foi decisão clínica.
- Não implementar listagem ampla foi decisão de privacidade.
- Roles/permissões finas ficam para fase futura.
- Soft Delete/AuditLog ficam para fase futura.
- Índice único em `PatientId` fica para fase futura.

## 8. Débitos técnicos remanescentes

| Débito | Evidência | Risco | Fase futura recomendada |
|---|---|---|---|
| Roles/permissões finas para prontuário | Endpoints protegidos por autenticação genérica `[Authorize]` | Acesso além do mínimo necessário por perfil clínico | 5.6.x ou fase de segurança clínica |
| Soft Delete | Diretriz formal em compliance e fora do escopo do MVP | Perda de histórico em exclusões físicas futuras | Fase dedicada de persistência clínica |
| AuditLog | Ausência de trilha de auditoria específica do prontuário | Baixa rastreabilidade de alterações clínicas | Fase dedicada de auditoria |
| Índice único em `MedicalRecords.PatientId` | Unicidade principal ainda garantida por regra lógica | Duplicidade física em concorrência extrema | Hardening de banco/migration futura |
| Revisão de `DeleteBehavior.Cascade` | Ponto já rastreado na documentação de Infrastructure | Exclusão em cascata indesejada de dados clínicos | Revisão de integridade referencial |
| `CreatedAt` | Modelo atual mantém `UpdatedAt` | Menor rastreabilidade de criação | Evolução de schema clínico |
| Controle de autoria | Ausência de `CreatedBy/UpdatedBy` | Falta de responsabilização | Fase de segurança/auditoria |
| Validação estrutural de `FlagsJson` | Campo flexível no MVP | Inconsistência de estrutura | Evolução Domain/Application |
| Política de retenção | Diretriz sem implementação técnica final | Risco regulatório/operacional | Fase de compliance clínica |
| Endpoint/relatório com `MedicalRecordListItemResponse` | Contract existe na Application, sem uso na API atual | Limitação de cenários futuros de listagem segura | Fase futura de consultas/listagens |
| Validação manual real local (se ainda não executada) | Documento 5.5.4 é roteiro e checklist | Divergências de ambiente podem ocultar ajustes | Pré-demo/fechamento 5.6 |
| Evidências com prints mascarados | Modelo existe, execução depende de rotina operacional | Exposição acidental de dados/tokens | Governança de evidências em 5.6 |

## 9. Riscos aceitos temporariamente

- proteção ainda baseada em autenticação genérica;
- ausência de autorização granular;
- ausência de Soft Delete;
- ausência de AuditLog;
- duplicidade física ainda possível em concorrência extrema;
- `FlagsJson` ainda flexível;
- ausência de `CreatedAt`;
- ausência de controle de autoria;
- validação manual pode depender do ambiente local;
- ausência de endpoint por Id pode limitar integrações futuras, mas é aceitável no MVP.

## 10. Critérios de aceite da subfase 5.5

A subfase 5.5 será considerada concluída se:

- planejamento da API existir;
- controller existir;
- GET/POST/PUT existirem;
- endpoints estiverem protegidos por `[Authorize]`;
- testes de API existirem;
- testes cobrirem principais status codes;
- roteiro Swagger/HTTP manual existir;
- documentação final existir;
- CI estiver aprovado;
- nenhuma migration indevida tiver sido criada;
- nenhum endpoint fora do MVP tiver sido criado.

## 11. Fora do escopo

Esta fase não implementa:

- código;
- testes;
- novos endpoints;
- endpoint DELETE;
- endpoint de listagem;
- endpoint por Id;
- migration;
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

## 12. Decisão final da subfase 5.5

**Opção A — Subfase 5.5 aprovada para encerramento.**

Justificativa:
A API MedicalRecord possui planejamento, controller, endpoints GET/POST/PUT, autenticação obrigatória, testes automatizados de API, roteiro manual Swagger/HTTP e documentação suficiente para ser considerada pronta como MVP técnico.

## 13. Próxima fase recomendada

Recomendação:

**Fase 5.6.1 — Auditoria final da vertical MedicalRecord.**

Objetivo:
Executar auditoria final da vertical MedicalRecord atravessando Domain, Application, Infrastructure, API, testes, documentação, riscos e débitos técnicos, antes de encerrar a Fase 5.

Como esta será a abertura da subfase maior 5.6, o próximo documento deve iniciar com:

Subfase 5.6 — Auditoria e fechamento da vertical MedicalRecord

Planejamento sugerido:
- 5.6.1 — Auditoria final da vertical MedicalRecord.
- 5.6.2 — Matriz de riscos e débitos técnicos da vertical MedicalRecord.
- 5.6.3 — Documentação final executiva da Fase 5.
- 5.6.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## 14. Validações obrigatórias

Comandos mandatórios desta fase:

- `git branch --show-current`
- `git status --short`
- `git diff --check`

Se `dotnet` estiver disponível:
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Os resultados devem sempre refletir execução real, sem inferência.
