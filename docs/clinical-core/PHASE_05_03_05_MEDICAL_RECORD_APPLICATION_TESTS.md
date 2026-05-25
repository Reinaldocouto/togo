# TOGO — Fase 5.3.5: Testes de Application de MedicalRecord

Subfase 5.3 — Application MedicalRecord

## Resumo da Subfase 5.3

Subfase 5.3 — Application MedicalRecord

Planejamento:
- 5.3.1 — Contracts de MedicalRecord.
- 5.3.2 — Interface IMedicalRecordRepository.
- 5.3.3 — Validators de MedicalRecord.
- 5.3.4 — Use cases de MedicalRecord.
- 5.3.5 — Testes de Application.
- 5.3.6 — Documentação final da camada Application.
- 5.3.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Criar testes unitários para validators e use cases de MedicalRecord na camada Application, cobrindo fluxos positivos e negativos antes do avanço para Infrastructure/API.

## Contexto

- Os contracts de MedicalRecord já existem.
- A interface de repositório (`IMedicalRecordRepository`) já existe.
- Os validators de MedicalRecord já existem.
- Os use cases de MedicalRecord já existem.
- O Domain de MedicalRecord já foi validado em fase anterior.
- Esta fase não implementa Infrastructure/API.
- O PR 115 incluiu correção de build removendo catch redundante de `ArgumentOutOfRangeException` onde `ArgumentException` já cobria o cenário.

## Testes criados

### Validators

Arquivos:
- `backend/src/Togo.Application.Tests/MedicalRecords/Validators/MedicalRecordPatientExistsValidatorTests.cs`
- `backend/src/Togo.Application.Tests/MedicalRecords/Validators/MedicalRecordUniquenessValidatorTests.cs`
- `backend/src/Togo.Application.Tests/MedicalRecords/Validators/MedicalRecordExistsValidatorTests.cs`

Cobertura:
- `MedicalRecordPatientExistsValidator`
  - `patientId` inválido → `ValidationError`.
  - paciente inexistente → `NotFound`.
  - paciente existente → `Success`.
- `MedicalRecordUniquenessValidator`
  - `patientId` inválido → `ValidationError`.
  - paciente já com prontuário → `Conflict`.
  - paciente sem prontuário → `Success`.
- `MedicalRecordExistsValidator`
  - `patientId` inválido → `ValidationError`.
  - prontuário inexistente → `NotFound`.
  - prontuário existente → `Success`.

### Use cases

Arquivos:
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/CreateMedicalRecordUseCaseTests.cs`
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/GetMedicalRecordByPatientIdUseCaseTests.cs`
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/UpdateMedicalRecordUseCaseTests.cs`
- suporte: `backend/src/Togo.Application.Tests/MedicalRecords/Fakes/FakeMedicalRecordRepository.cs`

Cobertura:
- `CreateMedicalRecordUseCase`
  - fluxo válido com `Success` + response.
  - `patientId` inválido → `ValidationError`.
  - paciente inexistente → `NotFound`.
  - duplicidade → `Conflict`.
  - `AddAsync` chamado somente quando válido.
  - mapeamento de `GeneralNotes`, `FlagsJson`, `PatientId`, `UpdatedAt`.
  - proteção para não expor `GeneralNotes`/`FlagsJson` em logs do use case.
- `GetMedicalRecordByPatientIdUseCase`
  - fluxo válido com `Success` + response.
  - `patientId` inválido → `ValidationError`.
  - paciente inexistente → `NotFound`.
  - prontuário inexistente → `NotFound`.
  - retorno defensivo `NotFound` quando repositório devolve `null` após validações.
  - mapeamento da response.
- `UpdateMedicalRecordUseCase`
  - fluxo válido com `Success` + response atualizada.
  - `patientId` inválido → `ValidationError`.
  - paciente inexistente → `NotFound`.
  - prontuário inexistente → `NotFound`.
  - retorno defensivo `NotFound` quando repositório devolve `null` após validações.
  - `UpdateAsync` chamado somente quando válido.
  - normalização de `GeneralNotes`/`FlagsJson` pelo domínio (trim).

## Decisões técnicas

- Testes implementados como testes unitários da camada Application.
- Não usam banco real.
- Não usam EF Core.
- Não usam API/controller.
- Não dependem de Infrastructure.
- Usam fakes/mocks manuais de repositório.
- Validam `ApplicationResult` (`Type`, `IsSuccess`, `Error`, `Data` quando aplicável).
- Cobrem fluxos positivos e negativos.
- Não alteram regra de negócio existente.
- Não implementam repositório concreto.

## Segurança e privacidade

- Testes não usam dados clínicos reais.
- `GeneralNotes` e `FlagsJson` usados em testes são fictícios.
- Logs de produção não devem expor payload clínico sensível.
- O foco dos testes é fluxo e resultado, não conteúdo clínico real.

## Pontos de atenção

- A unicidade por `PatientId` ainda não é constraint de banco.
- O repositório concreto ainda não existe.
- API/controller ainda não existem para MedicalRecord.
- Soft Delete e AuditLog permanecem fora do escopo.
- `CancellationToken` ainda não está propagado na interface `IMedicalRecordRepository`.

## Critérios de aceite

A fase é considerada concluída com:
- testes de validators criados;
- testes de use cases criados;
- fluxos positivos cobertos;
- fluxos negativos cobertos;
- validação de chamadas `AddAsync`/`UpdateAsync` quando aplicável;
- validação de `ApplicationResult`;
- `dotnet build backend/Togo.sln` passando;
- `dotnet test backend/Togo.sln` passando;
- `git diff --check` sem problemas;
- nenhuma migration criada;
- nenhuma alteração em Infrastructure/API;
- documentação da fase criada.

## Fora do escopo

Esta fase não implementa:
- repository concreto;
- controller;
- API;
- Infrastructure;
- migration;
- database update;
- Program.cs/DI;
- Soft Delete;
- AuditLog;
- roles/permissões;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## Próxima fase recomendada

**Fase 5.3.6 — Documentação final da camada Application MedicalRecord.**

Objetivo:
Consolidar contracts, repository interface, validators, use cases, testes de Application, correções aplicadas e autorização para avançar para a Fase 5.4 — Infrastructure MedicalRecord.

## Validações obrigatórias

Executar e registrar:
- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`
