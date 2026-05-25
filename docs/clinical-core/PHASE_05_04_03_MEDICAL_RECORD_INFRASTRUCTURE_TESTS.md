# TOGO — Fase 5.4.3: Testes de Infrastructure de MedicalRecord

## Resumo da Subfase 5.4

Subfase 5.4 — Infrastructure MedicalRecord

Planejamento:
- 5.4.1 — Repository EF Core de MedicalRecord.
- 5.4.2 — Registro de DI/Program.cs para MedicalRecord.
- 5.4.3 — Testes de Infrastructure de MedicalRecord.
- 5.4.4 — Validação EF/AppDbContext/Migration existente.
- 5.4.5 — Documentação final da camada Infrastructure.
- 5.4.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Validar o `MedicalRecordRepository` concreto com EF Core em ambiente de teste, cobrindo persistência, consultas, atualização e `AsNoTracking`, antes de avançar para validações de `AppDbContext`/migration e API.

## Contexto

- O `MedicalRecordRepository` foi criado na fase 5.4.1.
- O registro de DI para MedicalRecord foi concluído na fase 5.4.2.
- Nesta fase, o foco é comprovar persistência real da camada Infrastructure com banco de teste.
- Os testes não utilizam banco real.
- Esta fase não cria migration.
- Esta fase não implementa API/controller.

## Testes criados

Arquivo criado:
- `backend/src/Togo.Infrastructure.Tests/Repositories/MedicalRecordRepositoryTests.cs`

Cenários cobertos:
- `AddAsync_ShouldPersistMedicalRecord`: valida persistência de `PatientId`, `GeneralNotes`, `FlagsJson` e `UpdatedAt`.
- `GetByIdAsync_ShouldReturnMedicalRecord_WhenExists`: valida retorno completo por `Id`.
- `GetByIdAsync_ShouldReturnNull_WhenNotFound`: valida retorno `null` para `Id` inexistente.
- `GetByPatientIdAsync_ShouldReturnMedicalRecord_WhenExists`: valida consulta por `PatientId`.
- `GetByPatientIdAsync_ShouldReturnNull_WhenNotFound`: valida retorno `null` para `PatientId` inexistente.
- `ExistsByPatientIdAsync_ShouldReturnTrue_WhenExists`: valida existência quando há prontuário para o paciente.
- `ExistsByPatientIdAsync_ShouldReturnFalse_WhenNotFound`: valida inexistência quando não há prontuário.
- `UpdateAsync_ShouldPersistChanges`: valida atualização de `GeneralNotes`, `FlagsJson` e `UpdatedAt` via `UpdateNotes`.
- `ReadMethods_ShouldUseAsNoTracking`: valida que `GetByIdAsync` e `GetByPatientIdAsync` não deixam entidades `MedicalRecord` rastreadas no `ChangeTracker`.

## Decisões técnicas

- Testes implementados na camada Infrastructure.
- Execução com EF Core em ambiente de teste (`SQLite in-memory`, via factory existente no projeto).
- Sem uso de MySQL real.
- Sem uso de API/controller.
- Sem `database update`.
- Sem criação de migration.
- Entidades `MedicalRecord` criadas com `MedicalRecord.Create`.
- Testes validam o comportamento do repository concreto.
- Regras da camada Application não foram testadas aqui.

## Segurança e privacidade

- Dados clínicos usados nos testes são fictícios.
- `GeneralNotes` e `FlagsJson` de teste não representam dados reais.
- Não há logging de payload clínico real nos testes.

## Pontos de atenção

- Unicidade por `PatientId` ainda não é constraint física de banco (até confirmação contrária na fase 5.4.4).
- `DeleteBehavior.Cascade` será avaliado na fase 5.4.4.
- Soft Delete e AuditLog continuam fora do escopo.
- `CancellationToken` ainda não é propagado pela interface de repository.
- API/controller ainda não existem nesta vertical.

## Critérios de aceite

A fase foi definida para concluir quando:
- `MedicalRecordRepositoryTests.cs` for criado.
- `AddAsync` for testado.
- `GetByIdAsync` for testado.
- `GetByPatientIdAsync` for testado.
- `ExistsByPatientIdAsync` for testado.
- `UpdateAsync` for testado.
- `AsNoTracking` for testado.
- `dotnet build backend/Togo.sln` passar.
- `dotnet test backend/Togo.sln` passar.
- `git diff --check` não apontar problemas.
- nenhuma migration for criada.
- nenhum banco real for alterado.
- nenhuma API/controller for criada.
- documentação da fase for criada.

## Fora do escopo

Esta fase não implementa:
- controller;
- API;
- endpoints;
- migration;
- database update;
- banco real;
- alteração de Program.cs;
- alteração de AppDbContext;
- alteração de EF Configuration;
- Soft Delete;
- AuditLog;
- roles/permissões;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## Próxima fase recomendada

**Fase 5.4.4 — Validação EF/AppDbContext/Migration existente.**

Objetivo:
Validar que `AppDbContext`, `MedicalRecordConfiguration`, migration existente e snapshot estão coerentes com a implementação de `MedicalRecordRepository`, incluindo análise de índice em `PatientId`, `DeleteBehavior.Cascade` e necessidade futura de migration para índice único/Soft Delete.

## Validações obrigatórias

Comandos obrigatórios desta fase:
- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`
