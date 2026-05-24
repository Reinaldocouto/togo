# TOGO — Fase 5.2.1: Testes de domínio de MedicalRecord

## Resumo da Subfase 5.2

Subfase 5.2 — Domain MedicalRecord

Planejamento:
- 5.2.1 — Testes de domínio de MedicalRecord.
- 5.2.2 — Ajustes de domínio, se os testes revelarem necessidade.
- 5.2.3 — Documentação final do domínio MedicalRecord.
- 5.2.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Criar testes unitários para validar o comportamento atual da entidade `MedicalRecord` antes de qualquer ajuste de domínio ou avanço para a camada Application.

## Contexto

- `MedicalRecord` já existe no Domain.
- `MedicalRecord` possui `Create`.
- `MedicalRecord` possui `UpdateNotes`.
- `MedicalRecord` valida `PatientId`.
- `MedicalRecord` valida `UpdatedAt`.
- `MedicalRecord` normaliza `GeneralNotes` e `FlagsJson`.
- A auditoria 5.1.1 recomendou iniciar pela cobertura de domínio.
- Esta fase não altera a entidade.

## Testes criados

- `Create_ShouldCreateMedicalRecord_WhenDataIsValid`: valida criação, `PatientId`, campos opcionais, `UpdatedAt` e `Id` inicial/default.
- `Create_ShouldThrowArgumentOutOfRangeException_WhenPatientIdIsInvalid`: valida `PatientId = 0` e negativo.
- `Create_ShouldThrowArgumentException_WhenUpdatedAtIsDefault`: valida falha para `UpdatedAt = default`.
- `Create_ShouldNormalizeGeneralNotes_WhenHasLeadingAndTrailingSpaces`: valida trim de `GeneralNotes`.
- `Create_ShouldNormalizeFlagsJson_WhenHasLeadingAndTrailingSpaces`: valida trim de `FlagsJson`.
- `Create_ShouldAllowNullOptionalFields`: valida `null` em opcionais.
- `Create_ShouldConvertEmptyOrWhitespaceOptionalFieldsToNull`: valida vazio/branco para `null`.
- `UpdateNotes_ShouldUpdateFields_WhenDataIsValid`: valida atualização de `GeneralNotes`, `FlagsJson` e `UpdatedAt`.
- `UpdateNotes_ShouldNormalizeGeneralNotes_WhenHasLeadingAndTrailingSpaces`: valida trim de `GeneralNotes`.
- `UpdateNotes_ShouldNormalizeFlagsJson_WhenHasLeadingAndTrailingSpaces`: valida trim de `FlagsJson`.
- `UpdateNotes_ShouldAllowNullOptionalFields`: valida `null` em opcionais.
- `UpdateNotes_ShouldConvertEmptyOrWhitespaceOptionalFieldsToNull`: valida vazio/branco para `null`.
- `UpdateNotes_ShouldThrowArgumentException_WhenUpdatedAtIsDefault`: valida falha para `UpdatedAt = default`.
- `UpdateNotes_ShouldPreservePatientId`: valida preservação de `PatientId`.
- `UpdateNotes_ShouldPreserveId_WhenApplyingUpdates`: valida preservação de `Id` quando aplicável.

## Decisões técnicas

- Testes unitários de domínio puro.
- Sem acesso a banco.
- Sem uso de EF Core.
- Sem uso de mocks.
- Sem necessidade de migrations.
- Foco em validar comportamento atual.
- Nenhum ajuste em `MedicalRecord` foi feito nesta fase.

## Pontos de atenção

- `MedicalRecord` ainda não possui `CreatedAt`.
- `MedicalRecord` ainda não possui Soft Delete.
- `MedicalRecord` ainda não possui AuditLog.
- `MedicalRecord` ainda não garante unicidade por `PatientId` no domínio/banco.
- Esses pontos permanecem como débitos técnicos planejados.

## Critérios de aceite

A fase será considerada concluída se:

- `MedicalRecordTests.cs` for criado;
- testes cobrirem `Create`;
- testes cobrirem `UpdateNotes`;
- testes cobrirem `PatientId` inválido;
- testes cobrirem `UpdatedAt` default;
- testes cobrirem normalização de `GeneralNotes`;
- testes cobrirem normalização de `FlagsJson`;
- testes cobrirem `null`/vazio/branco;
- testes validarem preservação de `PatientId`;
- testes validarem preservação de `Id` quando aplicável;
- `dotnet build` passar, se o SDK estiver disponível;
- `dotnet test` passar, se o SDK estiver disponível;
- `git diff --check` não apontar problemas;
- nenhuma alteração de produção for feita;
- nenhuma migration for criada.

## Fora do escopo

Esta fase não implementa:

- ajuste de entidade;
- `CreatedAt`;
- Soft Delete;
- AuditLog;
- contracts;
- repository;
- validators;
- use cases;
- controller;
- migration;
- database update;
- API;
- frontend;
- Redis;
- RabbitMQ;
- Docker.

## Próxima fase recomendada

**Fase 5.2.2 — Ajustes de domínio de MedicalRecord, se necessários.**

Se todos os testes passarem sem necessidade de alteração, a Fase 5.2.2 poderá ser documental/confirmatória. Se os testes revelarem inconsistência, a Fase 5.2.2 deverá corrigir o domínio de forma pontual e testável.
