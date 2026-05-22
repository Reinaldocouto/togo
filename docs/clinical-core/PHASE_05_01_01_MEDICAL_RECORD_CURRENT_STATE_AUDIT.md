# TOGO — Fase 5.1.1: Auditoria do estado atual de MedicalRecord

## 1. Objetivo

Esta fase audita o estado atual real da vertical **MedicalRecord** antes de qualquer implementação.

A auditoria tem como objetivo identificar com precisão:

- o que já existe;
- o que está parcialmente pronto;
- o que não existe;
- riscos técnicos;
- lacunas por camada;
- próximos passos seguros para iniciar a implementação.

Esta fase é **exclusivamente documental** e não implementa código.

## 2. Contexto herdado

Premissas herdadas e mantidas:

- TOGO é ERP veterinário;
- prontuário não é atendimento;
- `MedicalRecord` representa o prontuário principal longitudinal do `Patient`;
- `Attendance` representa episódio/visita/evento clínico;
- um `Patient` deve ter no máximo um `MedicalRecord` principal;
- dados clínicos são sensíveis;
- Soft Delete e AuditLog são débitos técnicos obrigatórios futuros;
- `FlagsJson` é aceito como MVP técnico, com débito técnico controlado;
- esta fase não implementa nada.

## 3. Arquivos inspecionados

### Domain

- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`
- `backend/src/Togo.Domain/Entities/Patient.cs`
- `backend/src/Togo.Domain/Entities/Attendance.cs`

### Application

- `backend/src/Togo.Application/Attendances/Contracts/AttendanceListItemResponse.cs`
- `backend/src/Togo.Application/Attendances/Contracts/AttendanceResponse.cs`
- `backend/src/Togo.Application/Attendances/Contracts/CloseAttendanceRequest.cs`
- `backend/src/Togo.Application/Attendances/Contracts/CreateAttendanceRequest.cs`
- `backend/src/Togo.Application/Attendances/Repositories/IAttendanceRepository.cs`
- `backend/src/Togo.Application/Attendances/UseCases/CancelAttendanceUseCase.cs`
- `backend/src/Togo.Application/Attendances/UseCases/CloseAttendanceUseCase.cs`
- `backend/src/Togo.Application/Attendances/UseCases/CreateAttendanceUseCase.cs`
- `backend/src/Togo.Application/Attendances/UseCases/GetAttendanceByIdUseCase.cs`
- `backend/src/Togo.Application/Attendances/UseCases/ListAttendancesUseCase.cs`
- `backend/src/Togo.Application/Attendances/Validators/AttendanceNumberUniqueValidator.cs`
- `backend/src/Togo.Application/Attendances/Validators/AttendancePatientExistsValidator.cs`
- `backend/src/Togo.Application/Attendances/Validators/OpenAttendanceValidator.cs`

### Infrastructure

- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`
- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`
- `backend/src/Togo.Infrastructure/Migrations/20260428200839_AddClinicalCoreEntities.cs`
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `backend/src/Togo.Infrastructure/Repositories/AttendanceRepository.cs`

### API

- `backend/src/Togo.Api/Controllers/AttendancesController.cs`

### Tests

- `backend/src/Togo.Domain.Tests/*`
- `backend/src/Togo.Application.Tests/*`
- `backend/src/Togo.Infrastructure.Tests/*`
- `backend/src/Togo.Api.Tests/*`

### Docs

- `docs/clinical-core/PHASE_05_00_00_CLINICAL_RECORD_COMPLIANCE_GUIDELINES.md`
- `docs/clinical-core/PHASE_05_00_01_MEDICAL_RECORD_IMPLEMENTATION_PLANNING.md`
- `docs/clinical-core/PHASE_04_06_01_ATTENDANCE_FINAL_AUDIT.md`
- `docs/DEVELOPMENT_GUIDELINES.md`

## 4. Auditoria da camada Domain

### 4.1 Estado atual de `MedicalRecord`

Auditoria de `MedicalRecord.cs`:

- classe **existe**;
- namespace: `Togo.Domain.Entities`;
- propriedades: `Id`, `PatientId`, `GeneralNotes`, `FlagsJson`, `UpdatedAt`;
- possui construtor privado sem parâmetros para EF;
- possui construtor privado com validações;
- possui factory `Create(long patientId, string? generalNotes, string? flagsJson, DateTime updatedAt)`;
- possui método `UpdateNotes(string? generalNotes, string? flagsJson, DateTime updatedAt)`;
- valida `PatientId` (`id > 0`);
- valida `UpdatedAt` (`date != default`);
- normaliza `GeneralNotes` (`Trim` e `null` quando branco);
- normaliza `FlagsJson` (`Trim` e `null` quando branco);
- **não possui `CreatedAt`**;
- **não possui controle de autoria** (`CreatedBy`, `UpdatedBy`, etc.);
- **não possui soft delete** (`IsDeleted`, `DeletedAt`, etc.);
- **não possui audit trail**.

### 4.2 Avaliação técnica

- A entidade está adequada como MVP mínimo para iniciar a vertical.
- Não há bloqueio técnico obrigatório para iniciar implementação a partir da entidade atual.
- `CreatedAt` deve ser considerado em fase futura de maturidade/auditoria clínica.
- `UpdatedAt` sozinho é aceitável para MVP atual, com débito técnico explícito.
- A entidade respeita a decisão arquitetural de que **prontuário não é atendimento** (modelos separados).

## 5. Auditoria da configuração EF Core

Auditoria de `MedicalRecordConfiguration.cs`:

- tabela: `MedicalRecords`;
- chave primária: `Id`;
- `Id` com `ValueGeneratedOnAdd`;
- `PatientId` obrigatório;
- `GeneralNotes` mapeado como `text`;
- `FlagsJson` mapeado como `longtext`;
- `UpdatedAt` obrigatório;
- relacionamento com `Patient` por FK `PatientId`;
- `DeleteBehavior` atual: `Cascade`;
- índice em `PatientId`.

Avaliação técnica:

- `DeleteBehavior.Cascade` traz risco para entidade clínica (apagamento indireto do prontuário ao excluir `Patient`).
- O índice em `PatientId` **não é único**, portanto não garante “um prontuário por paciente” no nível do banco.
- Há necessidade futura de avaliar e planejar índice único para `PatientId`.
- `longtext` em `FlagsJson` é flexível para MVP, mas pode aumentar risco de desorganização sem governança.
- Não há limitação explícita de tamanho para `GeneralNotes`/`FlagsJson` na configuração.
- Configuração é aderente ao MVP técnico da fase atual.

## 6. Auditoria do AppDbContext

Auditoria de `AppDbContext.cs`:

- existe `DbSet<MedicalRecord> MedicalRecords`;
- `ApplyConfigurationsFromAssembly` está sendo usado;
- `MedicalRecord` já está integrado ao contexto;
- não há pendência para integração inicial de mapeamento.

## 7. Auditoria da migration existente

Auditoria de `20260428200839_AddClinicalCoreEntities.cs` e `AppDbContextModelSnapshot.cs`:

- tabela `MedicalRecords` já é criada na migration existente;
- colunas criadas: `Id`, `PatientId`, `GeneralNotes`, `FlagsJson`, `UpdatedAt`;
- tipos observados: `bigint`, `text`, `longtext`, `datetime(6)`;
- FK para `Patients` em `PatientId`;
- comportamento de delete na FK: `Cascade`;
- índice em `PatientId` existe;
- índice em `PatientId` não é único;
- alterar essas definições sem migration planejada representa risco de divergência entre modelo e schema aplicado.

## 8. Auditoria da camada Application

Estado de MedicalRecord na Application:

- pasta `MedicalRecords`: **não encontrada**;
- contracts de MedicalRecord: **não encontrados**;
- `IMedicalRecordRepository`: **não encontrado**;
- validators de MedicalRecord: **não encontrados**;
- use cases de MedicalRecord: **não encontrados**;
- padrão `ApplicationResult<T>`: **existe no projeto e é reutilizável** (base Attendance);
- padrão de organização de Attendance/Pet: **existe e pode ser reutilizado**.

Comparação com padrão Attendance:

- Attendance já possui estrutura completa de `Contracts`, `Repositories`, `Validators` e `UseCases`.
- MedicalRecord ainda não possui esta vertical na Application.

Próximas criações esperadas (fases futuras):

- pasta `Togo.Application/MedicalRecords`;
- contracts de create/update/response;
- interface `IMedicalRecordRepository`;
- validators de existência de patient/unicidade/existência de prontuário;
- use cases de create/get by patient/update.

## 9. Auditoria da camada Infrastructure

Estado de MedicalRecord na Infrastructure:

- `MedicalRecordRepository` concreto: **não existe**;
- padrão de `AttendanceRepository`: **existe e é reutilizável como referência**;
- métodos esperados (futuro): `GetByIdAsync`, `GetByPatientIdAsync`, `ExistsByPatientIdAsync`, `AddAsync`, `UpdateAsync`;
- leituras devem usar `AsNoTracking`;
- escritas devem usar `SaveChangesAsync`;
- testes esperados para repository devem usar SQLite in-memory, seguindo padrão já aplicado.

## 10. Auditoria da camada API

Estado de MedicalRecord na API:

- `MedicalRecordsController`: **não existe**;
- rotas de MedicalRecord: **não encontradas**;
- não há registro observável de vertical MedicalRecord em `Program.cs` dentro do escopo desta fase de auditoria documental;
- `AttendancesController` fornece padrão reutilizável (Authorize, controller fino, mapeamento `ApplicationResult` para HTTP);
- decisão planejada de rota aninhada por `patientId` permanece aderente ao plano (`/api/patients/{patientId}/medical-record`).

## 11. Auditoria dos testes

### Domain.Tests

- testes específicos de MedicalRecord: **não encontrados**.

### Application.Tests

- testes específicos de MedicalRecord: **não encontrados**.

### Infrastructure.Tests

- testes específicos de MedicalRecord repository: **não encontrados**.

### Api.Tests

- testes específicos de controller/rotas de MedicalRecord: **não encontrados**.

Cobertura necessária futura:

- domínio (invariantes e normalização);
- application (validators e use cases);
- infrastructure (repository com SQLite in-memory);
- api (status codes e contratos HTTP).

## 12. Auditoria documental

Alinhamento com documentos-base:

- **Fase 5.0.0**: auditoria confirma as diretrizes centrais (prontuário ≠ atendimento, sensibilidade clínica, Soft Delete/AuditLog como débitos futuros, `FlagsJson` como MVP controlado).
- **Fase 5.0.1**: auditoria confirma o diagnóstico de base pronta em Domain/EF/migration e ausência das camadas de implementação.
- **Fase 4.6.1**: auditoria confirma que Attendance é padrão maduro de referência para replicação arquitetural incremental.
- **Development Guidelines**: estado atual está aderente aos padrões de entidade, separação de camadas e nomenclatura em inglês.

Contradições encontradas:

- não foram encontradas contradições diretas entre 5.1.1 e as decisões anteriores.

Riscos documentais derivados:

- apesar de alinhado ao plano, a regra “um prontuário por paciente” ainda não está garantida no banco (índice não único), devendo ser tratada como risco técnico até fase específica.

## 13. Lacunas identificadas

| Camada | Lacuna | Impacto | Próxima fase recomendada |
|---|---|---|---|
| Domain.Tests | Ausência de testes de domínio de MedicalRecord | Risco de regressão em invariantes e normalização | Fase 5.2.1 |
| Application | Ausência de contracts de MedicalRecord | Sem contrato de entrada/saída para casos de uso | Fase 5.3.x |
| Application | Ausência de `IMedicalRecordRepository` | Sem abstração para persistência | Fase 5.3.x |
| Infrastructure | Ausência de repository concreto de MedicalRecord | Sem persistência operacional da vertical | Fase 5.4.x |
| Application | Ausência de validators | Regras de negócio sem camada dedicada | Fase 5.3.x |
| Application | Ausência de use cases | API não pode orquestrar fluxo clínico corretamente | Fase 5.3.x |
| API | Ausência de controller/rotas de MedicalRecord | Vertical indisponível via HTTP | Fase 5.5.x |
| Api.Tests | Ausência de testes API de MedicalRecord | Sem garantia de contratos/status code | Fase 5.5.x |
| Infrastructure/DB | Índice `PatientId` não único em MedicalRecords | Pode haver duplicidade de prontuário por paciente | Fase de migration planejada |
| Infrastructure/DB | `DeleteBehavior.Cascade` na FK com Patient | Risco de exclusão em cascata de dado clínico | Fase de revisão de integridade clínica |
| Cross-cutting | Ausência de Soft Delete | Baixa maturidade para retenção e preservação clínica | Fase específica de Soft Delete |
| Cross-cutting | Ausência de AuditLog | Sem trilha de autoria/alterações clínicas | Fase específica de auditoria |
| Security | Ausência de roles/permissões finas para prontuário | Risco de acesso excessivo a dados sensíveis | Fase de segurança/autorização |

## 14. Riscos técnicos identificados

- duplicidade de prontuário por `Patient`;
- apagamento em cascata de prontuário via exclusão de `Patient`;
- `MedicalRecord` virar campo gigante de observações;
- `FlagsJson` virar “lixeira” sem governança;
- logs exporem dados clínicos sensíveis;
- implementação avançar sem testes de domínio;
- avanço para API antes de consolidar Application;
- criação de migration sem auditoria prévia de schema;
- mistura conceitual entre `MedicalRecord` e `Attendance`;
- não preparar unicidade por `PatientId`.

## 15. Decisões confirmadas pela auditoria

- `MedicalRecord` já possui base mínima no Domain.
- `MedicalRecord` já possui configuração EF.
- `MedicalRecords` já existe em migration.
- A vertical ainda não está implementada nas camadas Application, Infrastructure e API.
- A próxima etapa segura é iniciar testes de domínio.
- Não há implementação nova nesta fase.

## 16. Recomendação para próxima fase

### Fase 5.2.1 — Testes de domínio de MedicalRecord

Objetivo:

Criar testes unitários de domínio para validar o comportamento atual da entidade `MedicalRecord` antes de ajustar ou expandir qualquer camada.

Testes esperados na próxima fase:

- Create válido;
- `PatientId` inválido;
- `UpdatedAt` default;
- normalização de `GeneralNotes`;
- normalização de `FlagsJson`;
- campos opcionais nulos;
- campos opcionais vazios normalizados para `null`;
- `UpdateNotes` válido;
- `UpdateNotes` com `GeneralNotes = null`;
- `UpdateNotes` com `FlagsJson = null`;
- `UpdateNotes` com `UpdatedAt = default`;
- preservação de `PatientId`;
- preservação de `Id` quando aplicável ao comportamento da entidade.

## 17. Critérios de aceite da Fase 5.1.1

A fase é considerada concluída quando esta auditoria:

- audita Domain;
- audita EF Core;
- audita AppDbContext;
- audita migration/snapshot;
- audita Application;
- audita Infrastructure;
- audita API;
- audita Tests;
- audita documentação;
- lista lacunas;
- lista riscos;
- confirma próximas fases;
- não altera código;
- não altera testes;
- não altera migrations;
- não altera banco;
- não executa database update.

## 18. Fora do escopo

Esta fase não implementa:

- entidade;
- ajuste de entidade;
- testes;
- contracts;
- repository;
- validators;
- use cases;
- controller;
- migration;
- database update;
- Soft Delete;
- AuditLog;
- roles/permissões;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## 19. Validações obrigatórias no final

Validações executadas:

- `git branch --show-current`
- `git status --short`
- `git diff --check`

Observação:

- se `dotnet` não estiver disponível no ambiente, a limitação deve ser registrada; nesta fase documental não há execução obrigatória de build/test.
