# Fase 8.6.1 — Filtros contextuais em Tutor, Patient e Pet

## Objetivo

Implementar filtros explícitos por `ClinicId` autorizado nos fluxos clínicos de Tutor e Pet/Patient, reduzindo risco de consulta, listagem, alteração ou deleção cross-clinic.

## Escopo implementado

- Tutor: `Get`, `List`, `Update` e `Delete` passam a buscar registros por `Id + ClinicId` ou listar por `ClinicId`.
- Tutor: `Create` passa a persistir o `ClinicId` do contexto clínico autorizado, não o valor do payload.
- Patient/Pet: `List`, `Get`, `Update` e `Delete` passam a filtrar por `Patient.ClinicId`.
- Patient/Pet: `Create` passa a criar `Patient.ClinicId` com o `ClinicId` autorizado.
- Validações de tutor/documento e tutor do pet operam dentro da clínica autorizada.
- `Pet` continua sem `ClinicId` direto; o escopo é herdado via `Patient.ClinicId`.

## Arquivos alterados

- `backend/src/Togo.Application/Tutors/ITutorRepository.cs`
- `backend/src/Togo.Application/Tutors/UseCases/CreateTutorUseCase.cs`
- `backend/src/Togo.Application/Tutors/UseCases/GetTutorByIdUseCase.cs`
- `backend/src/Togo.Application/Tutors/UseCases/ListTutorsUseCase.cs`
- `backend/src/Togo.Application/Tutors/UseCases/UpdateTutorUseCase.cs`
- `backend/src/Togo.Application/Tutors/UseCases/DeleteTutorUseCase.cs`
- `backend/src/Togo.Application/Pets/IPetRepository.cs`
- `backend/src/Togo.Application/Pets/UseCases/CreatePetUseCase.cs`
- `backend/src/Togo.Application/Pets/UseCases/GetPetByIdUseCase.cs`
- `backend/src/Togo.Application/Pets/UseCases/ListPetsUseCase.cs`
- `backend/src/Togo.Application/Pets/UseCases/UpdatePetUseCase.cs`
- `backend/src/Togo.Application/Pets/UseCases/DeletePetUseCase.cs`
- `backend/src/Togo.Application/Pets/Validators/PetTutorExistsValidator.cs`
- `backend/src/Togo.Infrastructure/Repositories/TutorRepository.cs`
- `backend/src/Togo.Infrastructure/Repositories/PetRepository.cs`
- testes/fakes de Application e API impactados por contratos.

## Decisão sobre filtros explícitos

A fase mantém a decisão da Fase 8.6.0: filtros explícitos e incrementais em use cases/repositories. Os use cases resolvem o contexto atual e chamam `IClinicalContextAuthorizationService.EnsureCanAccessCurrentClinicAsync`; os repositories recebem `clinicId` em métodos protegidos.

## Decisão sobre EF Core Global Query Filters

Não foram implementados filtros globais do EF Core nesta fase. Isso evita efeitos colaterais em fluxos ainda não migrados e mantém o rollout incremental.

## Mudanças em Tutor

- `ITutorRepository.GetByIdAsync` agora recebe `clinicId`.
- `ITutorRepository.ListAsync` agora recebe `clinicId`.
- `TutorRepository.GetByIdAsync` filtra por `Tutor.Id` e `Tutor.ClinicId`.
- `TutorRepository.ListAsync` filtra por `Tutor.ClinicId`.
- `CreateTutorUseCase` persiste o `ClinicId` autorizado e rejeita divergência positiva entre payload e contexto.
- `UpdateTutorUseCase` não altera `ClinicId`.
- Busca de tutor de outra clínica retorna `NotFound` no fluxo de Application.

## Mudanças em Patient/Pet

- `IPetRepository.ListAsync`, `GetByPatientIdAsync`, `UpdateAsync` e `DeleteAsync` recebem `clinicId` nos fluxos protegidos.
- `PetRepository` filtra o escopo por `Patient.ClinicId` via join/consulta de `Patient`.
- `CreatePetUseCase` cria `Patient` com `ClinicId` autorizado.
- `UpdatePetUseCase` valida o novo tutor dentro da clínica autorizada e não altera `Patient.ClinicId`.
- `DeletePetUseCase` remove somente Patient/Pet da clínica autorizada.

## Como o ClinicId autorizado é obtido

Os use cases usam `ICurrentClinicalContext.GetRequiredClinicId()`. A ausência de contexto continua resultando em `MissingClinicalContextException`.

## Como UserClinicAccess é validado

Os use cases chamam `IClinicalContextAuthorizationService.EnsureCanAccessCurrentClinicAsync`, que valida o usuário atual e vínculo ativo em `UserClinicAccess`.

## Decisão sobre request.ClinicId transitório

`CreateTutorRequest.ClinicId` e `CreatePetRequest.ClinicId` permanecem por compatibilidade/transição. Quando informado com valor positivo divergente do contexto autorizado, o use case retorna `ValidationError`. Quando ausente/zero, o contexto autorizado é usado.

## Decisão sobre microchip

A unicidade de microchip foi mantida global nesta fase para evitar ampliar escopo e semântica de negócio. A mensagem de conflito permanece genérica: não informa clínica, tutor ou paciente associado. Risco remanescente: a existência de um microchip em outra clínica ainda pode bloquear cadastro e indicar indiretamente duplicidade global.

## Testes criados/executados

Foram ajustados testes e fakes de Application/API para os novos contratos e para uso de contexto clínico falso. A validação local do ambiente não executou testes porque o SDK `dotnet` não está instalado no contêiner.

## Riscos remanescentes

- Fluxos Attendance, MedicalRecord, ClinicalEvolution e Prescription ainda exigem subfases próprias de filtros contextuais.
- Microchip permanece com unicidade global.
- Padronização HTTP para ausência de contexto/acesso negado ainda depende do pipeline global.
- Alguns fluxos não migrados usam métodos de compatibilidade documentados no `IPetRepository` até suas fases específicas.

## O que não foi implementado

- Nenhum filtro em Attendance.
- Nenhum filtro em MedicalRecord.
- Nenhum filtro em ClinicalEvolution.
- Nenhum filtro em Prescription.
- Nenhum `ClinicId` direto em `Pet`.
- Nenhuma migration.
- Nenhum EF Core Global Query Filter.
- Nenhuma auditoria contextual completa ou auditoria de leitura.
- Nenhuma alteração de front-end.

## Próxima fase recomendada

Fase 8.6.2 — Filtros em Attendance.
