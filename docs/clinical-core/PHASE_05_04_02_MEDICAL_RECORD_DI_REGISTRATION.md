# TOGO — Fase 5.4.2: Registro de DI/Program.cs para MedicalRecord

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

Registrar as dependências da vertical MedicalRecord no container de DI para permitir a resolução em runtime de repository, validators e use cases da camada Application, sem expor endpoint HTTP nesta fase.

## Contexto

- A Application de MedicalRecord já está encerrada (subfase 5.3).
- `MedicalRecordRepository` foi criado na fase 5.4.1.
- O repository concreto ainda não estava registrado no container de DI.
- Validators e use cases de MedicalRecord precisam ser resolvidos pelo container.
- Esta fase não implementa controller/API.
- Esta fase não cria migration.

## Registros realizados

### Repository

- `IMedicalRecordRepository` -> `MedicalRecordRepository`.

### Validators

- `MedicalRecordPatientExistsValidator`.
- `MedicalRecordUniquenessValidator`.
- `MedicalRecordExistsValidator`.

### Use cases

- `CreateMedicalRecordUseCase`.
- `GetMedicalRecordByPatientIdUseCase`.
- `UpdateMedicalRecordUseCase`.

## Decisões técnicas

- Os registros seguem o lifetime padrão já utilizado no projeto (`AddScoped`).
- O DI permanece no ponto de composição atual (`Program.cs`).
- Nenhuma nova arquitetura de DI foi criada.
- Nenhuma extension nova de DI foi criada.
- Controller/API permanecem fora do escopo.
- Migrations permanecem fora do escopo.
- O repository concreto já existia e apenas passou a ser resolvível.
- Validators e use cases passam a ser resolvíveis em runtime.

## Segurança e privacidade

- Registros de DI não alteram política de acesso.
- API/roles/autorização ficam para fase posterior.
- Nenhum dado clínico é logado por DI.
- `GeneralNotes` e `FlagsJson` permanecem sensíveis.

## Pontos de atenção

- Controller/API ainda não existem.
- Testes de Infrastructure serão realizados na 5.4.3.
- Validação de AppDbContext/migration existente será realizada na 5.4.4.
- Unicidade por `PatientId` ainda não é constraint física no banco.
- Soft Delete e AuditLog continuam fora do escopo.
- `CancellationToken` ainda não é propagado pelo `IMedicalRecordRepository`.

## Critérios de aceite

A fase é considerada concluída se:

- `IMedicalRecordRepository` estiver registrado com `MedicalRecordRepository`.
- Os 3 validators de MedicalRecord estiverem registrados.
- Os 3 use cases de MedicalRecord estiverem registrados.
- O lifetime seguir o padrão existente.
- Build passar.
- Testes passarem.
- Nenhuma migration for criada.
- Nenhum controller/API for criado.
- `AppDbContext` não for alterado.
- A documentação da fase for criada.
- `git diff --check` não apontar problemas.

## Fora do escopo

Esta fase não implementa:

- controller;
- API;
- endpoints;
- testes de Infrastructure;
- migration;
- database update;
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

**Fase 5.4.3 — Testes de Infrastructure de MedicalRecord.**

Objetivo:
Criar testes de Infrastructure para validar `MedicalRecordRepository` com EF Core/SQLite in-memory (ou padrão equivalente já usado no projeto), cobrindo `AddAsync`, `UpdateAsync`, `GetByIdAsync`, `GetByPatientIdAsync`, `ExistsByPatientIdAsync` e comportamento `AsNoTracking`.

## Validações obrigatórias

Comandos executados nesta fase:

- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`
