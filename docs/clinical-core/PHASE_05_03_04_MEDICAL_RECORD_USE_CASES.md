# TOGO — Fase 5.3.4: Use cases de MedicalRecord

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

Criar os use cases da camada Application para criar, consultar por `patientId` e atualizar `MedicalRecord`, preparando a API futura sem implementar controller.

## Contexto

- contracts já existem;
- `IMedicalRecordRepository` já existe;
- validators já existem;
- `MedicalRecord` já possui domínio testado;
- prontuário não é atendimento;
- `patientId` vem pela rota;
- dados clínicos são sensíveis;
- esta fase não implementa API/Infrastructure.

## Use cases criados

### 1) CreateMedicalRecordUseCase

- **Finalidade:** criar prontuário para um `patientId` da rota usando `CreateMedicalRecordRequest`.
- **Dependências:** `IMedicalRecordRepository`, `MedicalRecordPatientExistsValidator`, `MedicalRecordUniquenessValidator`, `ILogger<CreateMedicalRecordUseCase>`.
- **Assinatura:** `Task<ApplicationResult<MedicalRecordResponse>> ExecuteAsync(long patientId, CreateMedicalRecordRequest request, CancellationToken cancellationToken)`.
- **Fluxo:** loga início com `PatientId`; valida paciente; valida unicidade; cria entidade com `MedicalRecord.Create(..., DateTime.UtcNow)`; persiste com `AddAsync`; retorna `Success` com `MedicalRecordResponse`; captura `ArgumentException`/`ArgumentOutOfRangeException` como `ValidationError`.
- **Resultados esperados:** sucesso com payload mapeado; falhas de validação/not-found/conflict convertidas de `ApplicationResult<bool>` para `ApplicationResult<MedicalRecordResponse>`.
- **Regras protegidas:** paciente precisa existir, prontuário deve ser único por paciente, domínio valida dados de entrada.
- **Logs seguros:** não loga `GeneralNotes` nem `FlagsJson`.

### 2) GetMedicalRecordByPatientIdUseCase

- **Finalidade:** consultar prontuário pelo `patientId` da rota.
- **Dependências:** `IMedicalRecordRepository`, `MedicalRecordPatientExistsValidator`, `MedicalRecordExistsValidator`, `ILogger<GetMedicalRecordByPatientIdUseCase>`.
- **Assinatura:** `Task<ApplicationResult<MedicalRecordResponse>> ExecuteAsync(long patientId, CancellationToken cancellationToken)`.
- **Fluxo:** loga início com `PatientId`; valida paciente; valida existência do prontuário; busca por `GetByPatientIdAsync`; retorna `NotFound` defensivo se vier `null`; retorna `Success` com response mapeado.
- **Resultados esperados:** sucesso com prontuário; erros de validação/not-found mapeados por validator; fallback defensivo de not-found pós-validação.
- **Regras protegidas:** somente paciente existente e com prontuário existente segue para leitura.
- **Logs seguros:** não loga `GeneralNotes` nem `FlagsJson`.

### 3) UpdateMedicalRecordUseCase

- **Finalidade:** atualizar `GeneralNotes` e `FlagsJson` do prontuário pelo `patientId` da rota.
- **Dependências:** `IMedicalRecordRepository`, `MedicalRecordPatientExistsValidator`, `MedicalRecordExistsValidator`, `ILogger<UpdateMedicalRecordUseCase>`.
- **Assinatura:** `Task<ApplicationResult<MedicalRecordResponse>> ExecuteAsync(long patientId, UpdateMedicalRecordRequest request, CancellationToken cancellationToken)`.
- **Fluxo:** loga início com `PatientId`; valida paciente; valida existência do prontuário; busca por `GetByPatientIdAsync`; retorna `NotFound` defensivo se `null`; executa `UpdateNotes(..., DateTime.UtcNow)`; persiste com `UpdateAsync`; retorna `Success`; captura `ArgumentException`/`ArgumentOutOfRangeException` como `ValidationError`.
- **Resultados esperados:** sucesso com estado atualizado; falhas de validação/not-found mapeadas; proteção defensiva em inconsistência de leitura.
- **Regras protegidas:** paciente deve existir e prontuário deve existir antes de atualizar; atualização respeita validações do domínio.
- **Logs seguros:** não loga `GeneralNotes` nem `FlagsJson`.

## Decisões técnicas

- use cases ficam na Application;
- use cases retornam `ApplicationResult<MedicalRecordResponse>`;
- use cases não retornam `IActionResult`;
- use cases não conhecem HTTP;
- use cases não acessam EF Core;
- use cases não acessam `AppDbContext`;
- use cases usam validators;
- use cases usam `IMedicalRecordRepository`;
- use cases usam `DateTime.UtcNow` para `UpdatedAt`;
- use cases não logam `GeneralNotes`/`FlagsJson`;
- use cases não implementam Soft Delete/AuditLog;
- use cases não alteram estrutura de `FlagsJson`;
- repository concreto virá na Infrastructure.

## Segurança e privacidade

- `GeneralNotes` e `FlagsJson` são dados clínicos sensíveis;
- logs devem usar `PatientId`, `MedicalRecordId`, operação e status;
- payload clínico não deve ser logado;
- erros técnicos não devem expor conteúdo clínico.

## Pontos de atenção

- build/test não puderam rodar se `dotnet` indisponível;
- `IMedicalRecordRepository` ainda não propaga `CancellationToken`;
- unicidade por `PatientId` ainda não é constraint de banco;
- repository concreto ainda não existe;
- controller ainda não existe;
- testes de Application virão na 5.3.5;
- Soft Delete e AuditLog continuam fora do escopo.

## Critérios de aceite

A fase será considerada concluída se:

- pasta `MedicalRecords/UseCases` existir;
- `CreateMedicalRecordUseCase` for criado;
- `GetMedicalRecordByPatientIdUseCase` for criado;
- `UpdateMedicalRecordUseCase` for criado;
- use cases usarem `ApplicationResult`;
- use cases usarem validators;
- use cases usarem `IMedicalRecordRepository`;
- use cases não acessarem `AppDbContext`;
- use cases não dependerem de EF Core;
- use cases não retornarem `IActionResult`;
- use cases não logarem `GeneralNotes`/`FlagsJson`;
- documentação da fase for criada;
- nenhuma implementação concreta de repository for criada;
- nenhuma migration for criada;
- nenhuma camada Infrastructure/API for alterada;
- `git diff --check` não apontar problemas;
- build/test forem executados se SDK disponível, ou limitação for registrada.

## Fora do escopo

Esta fase não implementa:

- controller;
- API;
- repository concreto;
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

**Fase 5.3.5 — Testes de Application de MedicalRecord.**

Objetivo:
Criar testes unitários da camada Application para validators e use cases de MedicalRecord, cobrindo fluxos positivos e negativos antes de avançar para Infrastructure/API.

Cenários esperados para avaliação na próxima fase:

Validators:
- patientId inválido;
- Patient inexistente;
- Patient existente;
- MedicalRecord duplicado;
- MedicalRecord inexistente;
- MedicalRecord existente.

Use cases:
- create válido;
- create com patient inválido;
- create com patient inexistente;
- create duplicado;
- get por patient válido;
- get patient inexistente;
- get prontuário inexistente;
- update válido;
- update patient inexistente;
- update prontuário inexistente;
- update com domínio inválido, se aplicável.

## Validações obrigatórias

Executar:

- `git branch --show-current`
- `git status --short`
- `git diff --check`

Se dotnet estiver disponível, executar:

- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Se dotnet não estiver disponível, registrar a limitação.

Não inventar resultado de build/test.
