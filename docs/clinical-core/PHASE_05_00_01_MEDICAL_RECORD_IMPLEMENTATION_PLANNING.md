# TOGO — Fase 5.0.1: Planejamento técnico da implementação de MedicalRecord / Prontuário

## 1. Objetivo

Este documento define o planejamento técnico da implementação da vertical **MedicalRecord / Prontuário** de forma incremental, segura, testável e alinhada à arquitetura atual do TOGO.

Esta fase é **exclusivamente documental** e **não implementa código**.

## 2. Contexto herdado da Fase 5.0.0

Decisões herdadas e obrigatórias para continuidade da Fase 5:

- TOGO é ERP veterinário.
- Prontuário não é Atendimento.
- `Attendance` representa episódio/visita/evento clínico.
- `MedicalRecord` representa histórico longitudinal principal do paciente.
- Um `Patient` deve ter no máximo um `MedicalRecord` principal ativo.
- Dados clínicos são sensíveis.
- Soft Delete é requisito arquitetural futuro obrigatório.
- `FlagsJson` é MVP técnico aceito, com débito técnico controlado.
- FHIR, Redis, RabbitMQ e Kubernetes não entram nesta fase.
- Docker/Health Check seguem como trilha de Infra separada.
- O sistema não deve ser declarado como PEP humano legalmente homologado.

## 3. Estado atual identificado

### 3.1 Entidade MedicalRecord

Estado atual encontrado:

- A entidade `MedicalRecord` **já existe**.
- Propriedades atuais:
  - `Id`;
  - `PatientId`;
  - `GeneralNotes`;
  - `FlagsJson`;
  - `UpdatedAt`.
- Factory `Create` já existe: recebe `(patientId, generalNotes, flagsJson, updatedAt)`.
- Método `UpdateNotes` já existe: atualiza `GeneralNotes`, `FlagsJson` e `UpdatedAt`.
- Validações atuais na entidade:
  - `PatientId` deve ser maior que zero (`ValidateId`);
  - `UpdatedAt` não pode ser `default` (`ValidateDate`);
  - `GeneralNotes` e `FlagsJson` passam por normalização (`Trim`, `null` quando vazio/branco).

### 3.2 Configuração EF Core e schema

Estado atual encontrado:

- `MedicalRecordConfiguration` **já existe**.
- Tabela mapeada: `MedicalRecords`.
- Relacionamento com `Patient` já configurado por `PatientId` (FK).
- `DeleteBehavior` atual: `Cascade`.
- Índice atual em `PatientId`: **existe**, porém **não único**.
- `MedicalRecords` já existe em migration (`20260428200839_AddClinicalCoreEntities`).

### 3.3 AppDbContext

Estado atual encontrado:

- `AppDbContext` já possui `DbSet<MedicalRecord> MedicalRecords`.
- Configurações são aplicadas via `ApplyConfigurationsFromAssembly`.

### 3.4 Camadas Application, Infrastructure, API e testes para MedicalRecord

Estado atual encontrado:

- Repository concreto de MedicalRecord: **não existe**.
- Interface de repository de MedicalRecord: **não existe**.
- Contracts de MedicalRecord: **não existem**.
- Validators de MedicalRecord: **não existem**.
- Use cases de MedicalRecord: **não existem**.
- Controller de MedicalRecord: **não existe**.
- Testes específicos de MedicalRecord (Domain/Application/Infrastructure/API): **não encontrados**.

### 3.5 Padrão de referência no projeto (Attendance)

Para manter consistência arquitetural, a vertical Attendance foi usada como base de padrão:

- Application estruturada com `Contracts`, `Repositories`, `Validators`, `UseCases`.
- Repository interface na Application e implementação concreta na Infrastructure.
- Repository com leituras `AsNoTracking` e `SaveChangesAsync` em escrita.
- Use cases retornando `ApplicationResult<T>`.
- Controller fino com `[Authorize]`, mapeamento `ApplicationResult -> HTTP`, logging sem payload sensível completo.

## 4. Decisão de modelo mínimo para MVP da Fase 5

Modelo mínimo inicial aprovado para primeira implementação:

`MedicalRecord`
- `Id`
- `PatientId`
- `GeneralNotes`
- `FlagsJson`
- `UpdatedAt`

Decisões:

- `MedicalRecord` será o prontuário principal do `Patient`.
- O identificador público principal poderá ser `patientId` em endpoints orientados ao paciente.
- Não criar múltiplos prontuários para o mesmo `Patient`.
- Não incluir `ClinicalEvolution`, `Prescription`, `Exam` ou `Attachment` dentro de `MedicalRecord`.
- Não transformar `GeneralNotes` em campo gigante para tudo.
- Não usar `FlagsJson` como substituto de entidade clínica futura.

## 5. Decisão sobre unicidade de prontuário

Regra registrada:

> Um `Patient` deve ter no máximo um `MedicalRecord` principal.

Avaliação técnica:

- O índice atual em `MedicalRecords.PatientId` existe, porém não garante unicidade.
- Há necessidade de evolução futura para possível índice único.

Limite desta fase:

- Nesta fase documental, **não** será criada migration.

Planejamento de fase futura específica:

- avaliar índice único em `MedicalRecords.PatientId`;
- avaliar impacto em dados existentes;
- planejar migration dedicada;
- incluir validação de duplicidade;
- incluir regra de negócio em validator/use case;
- mapear duplicidade para `409 Conflict`.

## 6. Rotas/API recomendadas

### 6.1 Opções analisadas

**Opção A**
- `GET /api/medical-records/{id}`
- `GET /api/medical-records/by-patient/{patientId}`
- `POST /api/medical-records`
- `PUT /api/medical-records/{id}`

**Opção B**
- `GET /api/patients/{patientId}/medical-record`
- `POST /api/patients/{patientId}/medical-record`
- `PUT /api/patients/{patientId}/medical-record`

### 6.2 Recomendação

Recomendada para Fase 5: **Opção B**.

Justificativa:

- prontuário pertence ao paciente;
- fluxo clínico principal é orientado por `Patient`;
- simplifica contrato e evita duplicidade semântica de identificadores no corpo.

### 6.3 Endpoints mínimos recomendados

- `GET /api/patients/{patientId}/medical-record`
- `POST /api/patients/{patientId}/medical-record`
- `PUT /api/patients/{patientId}/medical-record`

Status HTTP esperados:

- `200` para consulta/update com sucesso;
- `201` para criação;
- `400` para validação;
- `401` sem token;
- `404` para patient inexistente ou prontuário inexistente;
- `409` para prontuário já existente no patient;
- `500` fallback inesperado.

## 7. Contracts planejados

Contracts futuros:

### 7.1 CreateMedicalRecordRequest
- `GeneralNotes`
- `FlagsJson`

### 7.2 UpdateMedicalRecordRequest
- `GeneralNotes`
- `FlagsJson`

### 7.3 MedicalRecordResponse
- `Id`
- `PatientId`
- `GeneralNotes`
- `FlagsJson`
- `UpdatedAt`

### 7.4 MedicalRecordListItemResponse (futuro, se necessário)
- `Id`
- `PatientId`
- `UpdatedAt`
- `HasGeneralNotes`
- `HasFlags`

Decisão:

- Não incluir `PatientId` no body dos endpoints orientados a `/api/patients/{patientId}/medical-record`.
- `patientId` deve vir da rota.

## 8. Repository planejado

Interface futura:

`IMedicalRecordRepository`

Métodos sugeridos:

- `Task<MedicalRecord?> GetByIdAsync(long id);`
- `Task<MedicalRecord?> GetByPatientIdAsync(long patientId);`
- `Task<bool> ExistsByPatientIdAsync(long patientId);`
- `Task AddAsync(MedicalRecord medicalRecord);`
- `Task UpdateAsync(MedicalRecord medicalRecord);`

Decisões de arquitetura:

- interface na camada Application;
- implementação concreta na Infrastructure;
- leituras com `AsNoTracking` quando aplicável;
- `AddAsync` e `UpdateAsync` com `SaveChangesAsync` na implementação concreta.

## 9. Validators planejados

Validators futuros:

- `MedicalRecordPatientExistsValidator`: valida se `Patient` existe.
- `MedicalRecordUniquenessValidator`: valida se já existe prontuário para o `Patient`.
- `MedicalRecordExistsValidator` (se necessário): valida existência do prontuário antes de update/get.

Regras de validação:

- `PatientId` deve ser maior que zero.
- `Patient` precisa existir.
- Não permitir duplicidade de `MedicalRecord` por `Patient`.
- `GeneralNotes` e `FlagsJson` opcionais, com limites futuros.
- Não validar estrutura profunda de `FlagsJson` na primeira implementação; manter débito técnico documentado.

## 10. Use cases planejados

Use cases futuros:

- `CreateMedicalRecordUseCase`
- `GetMedicalRecordByPatientIdUseCase`
- `UpdateMedicalRecordUseCase`

Opcional:

- `GetMedicalRecordByIdUseCase`

Decisões:

- retornar `ApplicationResult<T>` (padrão Attendance);
- não retornar `IActionResult`;
- não acessar `AppDbContext` diretamente;
- não conter regra HTTP;
- não logar conteúdo clínico completo.

## 11. Infrastructure planejada

Implementação futura planejada:

`MedicalRecordRepository`

Responsabilidades:

- consultar por `id`;
- consultar por `patientId`;
- verificar duplicidade por `patientId`;
- adicionar `MedicalRecord`;
- atualizar `MedicalRecord`;
- usar `AppDbContext`;
- usar `AsNoTracking` em leituras;
- preservar padrão adotado em `AttendanceRepository`;
- preparar testes com SQLite in-memory.

Atenção:

- Não implementar nesta fase.

## 12. Controller planejado

Alternativas de nome:

- `PatientsMedicalRecordsController`
- `MedicalRecordsController` com rota aninhada por paciente

Recomendação:

- usar `MedicalRecordsController` com rota aninhada:
  - `[Route("api/patients/{patientId:long}/medical-record")]`

Justificativa:

- mantém nomenclatura por recurso clínico principal (`MedicalRecord`), sem perder semântica de pertencimento ao `Patient`;
- reduz risco de proliferação de controllers excessivamente acoplados a naming composto.

Diretrizes obrigatórias do controller:

- usar `[Authorize]`;
- usar `[ApiController]`;
- receber use cases por DI;
- usar `ILogger`;
- não acessar `AppDbContext`;
- não conter regra de negócio;
- mapear `ApplicationResult` para status HTTP;
- não logar `GeneralNotes`/`FlagsJson` completos.

## 13. Testes planejados

### 13.1 Domain
- Create válido;
- `PatientId` inválido;
- `UpdatedAt` default;
- normalização de `GeneralNotes`;
- normalização de `FlagsJson`;
- `UpdateNotes` válido;
- `UpdateNotes` com campos nulos;
- preservação de `PatientId`.

### 13.2 Application
- validator patient exists;
- validator uniqueness;
- create válido;
- create com patient inexistente;
- create duplicado;
- get por patient;
- update válido;
- update com prontuário inexistente.

### 13.3 Infrastructure
- `AddAsync`;
- `GetByIdAsync`;
- `GetByPatientIdAsync`;
- `ExistsByPatientIdAsync`;
- `UpdateAsync`;
- leitura com `AsNoTracking`;
- SQLite in-memory.

### 13.4 API
- GET 200;
- GET 404;
- POST 201;
- POST 400;
- POST 404;
- POST 409;
- PUT 200;
- PUT 400;
- PUT 404;
- 401 sem token (integração/manual, se aplicável);
- mapeamento `ApplicationResult -> HTTP`.

## 14. Validação HTTP E2E planejada

Validação manual prevista (Swagger/Postman).

Fluxo positivo:

1. Autenticar.
2. Criar tutor.
3. Criar pet/patient.
4. Confirmar `patientId`.
5. Criar prontuário para `patientId`.
6. Consultar prontuário por `patientId`.
7. Atualizar prontuário.
8. Consultar novamente.

Cenários negativos:

- sem token;
- `patientId` inválido;
- patient inexistente;
- prontuário inexistente;
- criação duplicada;
- payload inválido;
- erro de validação em data, se aplicável.

## 15. Segurança e logging

Nenhum fluxo deve logar:

- `GeneralNotes` completo;
- `FlagsJson` completo;
- payload clínico completo;
- evolução clínica completa;
- prescrição completa.

Logs devem privilegiar metadados:

- `PatientId`;
- `MedicalRecordId`;
- operação;
- status;
- tipo de erro;
- flags (`HasGeneralNotes`, `HasFlagsJson`).

## 16. Soft Delete e AuditLog nesta fase

Posicionamento oficial:

- Soft Delete e AuditLog são indispensáveis para maturidade clínica.
- Não serão implementados na primeira implementação de MedicalRecord.
- Permanecem como débitos técnicos herdados da Fase 5.0.0.

Planejamento futuro dedicado:

- Fase futura: Soft Delete clínico.
- Fase futura: AuditLog clínico.
- Fase futura: revisão de cascade delete.

## 17. Subfases recomendadas da Fase 5

Roadmap recomendado:

- Fase 5.0.1 — Planejamento técnico da implementação de MedicalRecord.
- Fase 5.1.1 — Auditoria do estado atual de MedicalRecord.
- Fase 5.2.1 — Testes de domínio de MedicalRecord.
- Fase 5.2.2 — Ajustes de domínio, se necessários.
- Fase 5.3.1 — Contracts de MedicalRecord.
- Fase 5.3.2 — Repository interface.
- Fase 5.3.3 — Validators.
- Fase 5.3.4 — Use cases.
- Fase 5.3.5 — Testes de Application.
- Fase 5.4.1 — Repository EF Core.
- Fase 5.4.2 — Testes de Infrastructure.
- Fase 5.4.3 — Validação EF/AppDbContext/Migration existente.
- Fase 5.5.1 — Controller/API.
- Fase 5.5.2 — Testes de API.
- Fase 5.5.3 — Validação Swagger/rotas/status.
- Fase 5.5.4 — Execução HTTP E2E manual.
- Fase 5.6.1 — Documentação final da vertical MedicalRecord.
- Fase 5.6.2 — Auditoria final da Fase 5.

## 18. Riscos técnicos

Riscos principais mapeados:

- transformar prontuário em campo gigante;
- duplicar prontuário por `Patient`;
- misturar responsabilidades entre `Attendance` e `MedicalRecord`;
- usar `FlagsJson` como lixeira de dados;
- logar dado clínico sensível;
- permitir delete físico de histórico clínico;
- manter cascade delete sem revisão futura;
- não automatizar validação E2E;
- não padronizar roles/permissões;
- avançar para `ClinicalEvolution` antes de estabilizar `MedicalRecord`;
- criar migration prematura sem avaliar schema existente.

## 19. Critérios de aceite da Fase 5.0.1

Esta fase será considerada concluída se este documento:

- planejar a implementação completa de MedicalRecord;
- respeitar decisões da Fase 5.0.0;
- reforçar que prontuário não é atendimento;
- definir modelo mínimo;
- definir endpoints recomendados;
- definir contracts;
- definir repository;
- definir validators;
- definir use cases;
- definir testes;
- definir validação E2E;
- definir subfases futuras;
- registrar riscos;
- não alterar código;
- não alterar testes;
- não alterar banco;
- não criar migration.

## 20. Fora do escopo

Esta fase **não implementa**:

- entidade;
- ajuste de entidade;
- contracts;
- repository;
- validators;
- use cases;
- controller;
- testes;
- migration;
- database update;
- Soft Delete;
- AuditLog;
- roles/permissões;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## 21. Próxima fase recomendada

**Fase 5.1.1 — Auditoria do estado atual de MedicalRecord.**

Objetivo:

Auditar em detalhe a entidade `MedicalRecord`, configuração EF Core, migration existente, `AppDbContext`, lacunas de Application/Infrastructure/API/Tests antes da primeira implementação.

