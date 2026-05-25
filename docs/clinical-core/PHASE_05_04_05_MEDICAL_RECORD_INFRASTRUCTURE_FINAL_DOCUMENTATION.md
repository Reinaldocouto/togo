# TOGO — Fase 5.4.5: Documentação final da camada Infrastructure MedicalRecord

## 1. Título

TOGO — Fase 5.4.5: Documentação final da camada Infrastructure MedicalRecord

## 2. Resumo da Subfase 5.4

Subfase 5.4 — Infrastructure MedicalRecord

Planejamento:
- 5.4.1 — Repository EF Core de MedicalRecord.
- 5.4.2 — Registro de DI/Program.cs para MedicalRecord.
- 5.4.3 — Testes de Infrastructure de MedicalRecord.
- 5.4.4 — Validação EF/AppDbContext/Migration existente.
- 5.4.5 — Documentação final da camada Infrastructure.
- 5.4.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## 3. Objetivo

Esta fase consolida a camada Infrastructure de MedicalRecord, documentando:
- repository concreto;
- registro de DI;
- testes de Infrastructure;
- correção 5.4.3.1;
- validação EF/AppDbContext/Migration;
- riscos remanescentes;
- débitos técnicos;
- autorização para avançar para API.

## 4. Contexto

- Domain MedicalRecord foi concluído na subfase 5.2.
- Application MedicalRecord foi concluída na subfase 5.3.
- Infrastructure MedicalRecord foi implementada na subfase 5.4.
- A vertical ainda não possui controller/API.
- Prontuário não é atendimento.
- Dados clínicos são sensíveis.
- Esta fase não implementa código.

## 5. Entregas consolidadas da subfase 5.4

### 5.1 Repository EF Core

Consolidação do arquivo `backend/src/Togo.Infrastructure/Repositories/MedicalRecordRepository.cs`:
- implementação de `IMedicalRecordRepository`;
- uso de `AppDbContext` para acesso à persistência;
- `GetByIdAsync` com `AsNoTracking`;
- `GetByPatientIdAsync` com `AsNoTracking`;
- `ExistsByPatientIdAsync` com `AnyAsync`;
- `AddAsync` com `SaveChangesAsync`;
- `UpdateAsync` com `SaveChangesAsync`;
- ausência de `DeleteAsync`;
- ausência de `ListAsync`.

### 5.2 Registro de DI

Consolidação dos registros em `backend/src/Togo.Api/Program.cs`:
- `IMedicalRecordRepository -> MedicalRecordRepository`;
- `MedicalRecordPatientExistsValidator`;
- `MedicalRecordUniquenessValidator`;
- `MedicalRecordExistsValidator`;
- `CreateMedicalRecordUseCase`;
- `GetMedicalRecordByPatientIdUseCase`;
- `UpdateMedicalRecordUseCase`.

Todos os itens acima usam `AddScoped`, seguindo o padrão atual.

### 5.3 Testes de Infrastructure

Consolidação de `backend/src/Togo.Infrastructure.Tests/Repositories/MedicalRecordRepositoryTests.cs`:
- uso de SQLite in-memory;
- `AddAsync` testado;
- `GetByIdAsync` testado;
- `GetByPatientIdAsync` testado;
- `ExistsByPatientIdAsync` testado;
- `UpdateAsync` testado;
- `AsNoTracking` testado;
- `Patient` real de suporte usado para respeitar FK;
- sem banco real.

### 5.4 Correção 5.4.3.1

Consolidação da correção:
- erro inicial de build por ausência de `using Microsoft.EntityFrameworkCore`;
- `AsNoTracking` e `SingleAsync` são extension methods do EF Core;
- correção aplicada no arquivo `MedicalRecordRepositoryTests.cs`;
- build/test passaram posteriormente.

### 5.5 Validação EF/AppDbContext/Migration

Consolidação dos achados da fase 5.4.4:
- `AppDbContext` possui `DbSet<MedicalRecord> MedicalRecords`;
- `ApplyConfigurationsFromAssembly` está sendo usado;
- `MedicalRecordConfiguration` mapeia `MedicalRecords`;
- migration existente cria `MedicalRecords`;
- snapshot está coerente;
- `PatientId` possui índice não único;
- `DeleteBehavior.Cascade` está presente;
- não houve migration nova.

## 6. Segurança e privacidade

- Infrastructure não deve logar `GeneralNotes`/`FlagsJson`.
- Repository não implementa logging de payload clínico.
- Dados de testes são fictícios.
- API/roles/autorização serão tratados na subfase 5.5 ou fase futura.
- Delete físico continua fora do fluxo de MedicalRecord.

## 7. Decisões técnicas finais da subfase 5.4

- Infrastructure está pronta para avançar para API.
- Repository concreto existe e está testado.
- DI está registrado.
- EF mapping está coerente com migration/snapshot.
- Não criar migration nesta subfase foi decisão correta.
- Índice único em `PatientId` não será implementado agora.
- `DeleteBehavior.Cascade` não será alterado agora.
- Soft Delete e AuditLog seguem fora do escopo.
- `CancellationToken` segue como débito técnico.

## 8. Débitos técnicos remanescentes

| Débito | Evidência | Risco | Fase futura recomendada |
|---|---|---|---|
| Índice único em `MedicalRecords.PatientId` | Existe apenas índice não único no mapeamento/migration | Duplicidade física em concorrência | Hardening de dados clínicos antes/durante 5.5 |
| Revisão de `DeleteBehavior.Cascade` | FK `MedicalRecords -> Patients` em cascade | Possível exclusão em cascata de dado clínico | Revisão de integridade clínica |
| Soft Delete | Requisito clínico já documentado, não implementado | Perda de histórico em delete físico | Fase dedicada de persistência clínica |
| AuditLog | Ausência de trilha de auditoria específica | Baixa rastreabilidade de alterações clínicas | Fase dedicada de auditoria |
| `CreatedAt` | MedicalRecord possui somente `UpdatedAt` | Perda de contexto temporal de criação | Evolução de schema clínico |
| Controle de autoria | Não há `CreatedBy`/`UpdatedBy` | Falta de responsabilização por alteração | Fase de segurança/auditoria |
| Validação estrutural de `FlagsJson` | Campo flexível sem contrato rígido | Inconsistência estrutural de dados | Evolução de domain/validation |
| `CancellationToken` no repository | Métodos async sem token | Menor controle de cancelamento cooperativo | Evolução Application/Infrastructure |
| Política de retenção | Diretriz existe sem implementação técnica | Risco regulatório/operacional | Fase de compliance clínica |
| Roles/permissões finas | API ainda não possui camada específica de prontuário | Acesso além do mínimo necessário | Fase 5.5 (API MedicalRecord) |
| Testes de API | Só há testes de Infrastructure nesta vertical | Cobertura HTTP inexistente | Fase 5.5.3 |
| Endpoints/controller | Vertical ainda sem controller/API | Sem superfície HTTP para uso clínico | Fase 5.5.2 |
| OpenAPI/Swagger responses | Contratos de resposta não detalhados para MedicalRecord | Ambiguidade de integração cliente/API | Fase 5.5.4/5.5.5 |

## 9. Riscos aceitos temporariamente

- `PatientId` não único fisicamente.
- Duplicidade em concorrência ainda possível.
- `DeleteBehavior.Cascade` em entidade clínica.
- Ausência de Soft Delete.
- Ausência de AuditLog.
- `FlagsJson` flexível.
- Ausência de `CreatedAt`.
- Ausência de controle de autoria.
- Ausência de API/controller.
- Ausência de roles específicas para prontuário.

Esses riscos não bloqueiam a abertura da API, mas devem permanecer rastreados ao longo da subfase 5.5 e fases futuras de hardening clínico.

## 10. Critérios de aceite da subfase 5.4

A subfase 5.4 será considerada concluída se:
- `MedicalRecordRepository` existir;
- `MedicalRecordRepository` implementar `IMedicalRecordRepository`;
- DI estiver registrado;
- testes de Infrastructure existirem;
- testes de Infrastructure cobrirem métodos principais;
- `AppDbContext` estiver validado;
- `MedicalRecordConfiguration` estiver validada;
- migration existente estiver validada;
- snapshot estiver validado;
- documentação final for criada;
- build/test passarem no CI ou ambiente local;
- nenhuma migration indevida tiver sido criada;
- nenhuma API/controller tiver sido implementada antes da fase correta.

## 11. Fora do escopo

Esta fase não implementa:
- código;
- testes;
- controller;
- API;
- endpoints;
- migration;
- database update;
- banco real;
- alteração de `AppDbContext`;
- alteração de EF Configuration;
- Soft Delete;
- AuditLog;
- índice único;
- roles/permissões;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## 12. Decisão final da subfase 5.4

**Opção A — Subfase 5.4 aprovada para encerramento.**

Justificativa:
A camada Infrastructure de MedicalRecord possui repository concreto, DI registrado, testes de Infrastructure, mapeamento EF validado, migration/snapshot coerentes e documentação suficiente para avançar para API.

## 13. Próxima fase recomendada

Recomendação:
**Fase 5.5.1 — Planejamento da API MedicalRecord.**

Como abertura da subfase maior 5.5, o próximo documento deve iniciar com:

Subfase 5.5 — API MedicalRecord

Planejamento sugerido:
- 5.5.1 — Planejamento da API MedicalRecord.
- 5.5.2 — Controller MedicalRecord / rotas orientadas por Patient.
- 5.5.3 — Testes de API MedicalRecord.
- 5.5.4 — Validação Swagger/HTTP manual.
- 5.5.5 — Documentação final da API MedicalRecord.
- 5.5.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

Objetivo da próxima fase:
Planejar endpoints, status codes, segurança, autorização, contratos HTTP, Swagger/OpenAPI e validação manual antes de implementar o controller.

Rotas planejadas:
- `GET /api/patients/{patientId}/medical-record`
- `POST /api/patients/{patientId}/medical-record`
- `PUT /api/patients/{patientId}/medical-record`

## 14. Validações obrigatórias

Comandos mandatórios executados nesta fase:
- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Resultados devem refletir execução real, sem inferência.
