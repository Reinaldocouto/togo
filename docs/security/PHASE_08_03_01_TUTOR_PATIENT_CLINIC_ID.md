# Fase 8.3.1 — ClinicId em Tutor e Patient

## Objetivo

Introduzir `ClinicId` como escopo primário obrigatório em `Tutor` e `Patient`, atualizando domínio, persistência, migration e fluxos mínimos de criação de tutor e pet/patient. Esta fase é incremental e não implementa autorização contextual completa.

## Escopo implementado

- `Tutor` passou a possuir `ClinicId` obrigatório no domínio.
- `Patient` passou a possuir `ClinicId` obrigatório no domínio.
- As factories `Tutor.Create` e `Patient.Create` passaram a exigir `clinicId`.
- O fluxo de criação de tutor recebe `ClinicId` no request e persiste o valor informado.
- O fluxo de criação de pet recebe `ClinicId` no request e o propaga para o `Patient` criado internamente.
- `Pet` permanece sem `ClinicId` direto.
- A validação de tutor na criação de pet confirma, quando possível, que o tutor pertence à mesma clínica informada.
- EF Core mapeia `ClinicId` em `Tutors` e `Patients` com FKs para `Clinics` e `DeleteBehavior.Restrict`.
- Foram adicionados índices mínimos por `ClinicId`.

## Arquivos alterados

- Domínio: `Tutor`, `Patient` e testes de domínio relacionados.
- Application: contratos e use cases de criação de tutor e pet; validações de documento e tutor.
- Infrastructure: configurações EF Core, repositórios e migration.
- Testes: ajustes em fakes e testes de domínio/application impactados.

## Mudanças em Domain

- `Tutor.ClinicId` é obrigatório, sem setter público, validado como maior que zero e definido na factory.
- `Patient.ClinicId` é obrigatório, sem setter público, validado como maior que zero e definido na factory.
- Updates de `Patient` não alteram `ClinicId`.

## Mudanças em Infrastructure

- `TutorConfiguration` mapeia `ClinicId`, FK `Tutors.ClinicId -> Clinics.Id`, `DeleteBehavior.Restrict`, índice em `ClinicId` e índice composto não único em `(ClinicId, Document)`.
- `PatientConfiguration` mapeia `ClinicId`, FK `Patients.ClinicId -> Clinics.Id`, `DeleteBehavior.Restrict`, índice em `ClinicId` e índice composto não único em `(ClinicId, Name)`.
- Não foi criado índice único global de `Document`.

## Migration criada

- `20260629120000_AddTutorPatientClinicId`.
- A migration adiciona `ClinicId` em `Tutors` e `Patients`, faz backfill controlado, torna as colunas obrigatórias, cria FKs para `Clinics` e cria índices mínimos.
- A migration não adiciona `ClinicId` em `Pets`, `Attendances` ou entidades clínicas futuras.

## Estratégia para dados existentes

Para reduzir risco em bancos de desenvolvimento/teste com dados existentes, a migration adiciona `ClinicId` inicialmente como nullable, cria uma organização/clínica transitória de compatibilidade somente quando houver dados em `Tutors` ou `Patients` e não existir nenhuma clínica, realiza backfill com a primeira clínica disponível e então torna as colunas obrigatórias.

Essa clínica de compatibilidade é transitória e não representa governança real de produção. Ambientes produtivos devem revisar e ajustar a atribuição clínica antes de considerar o isolamento contextual completo.

## ClinicId transitório em request

`ClinicId` em `CreateTutorRequest` e `CreatePetRequest` é transitório nesta fase. Ele serve para persistir o escopo mínimo exigido pelo modelo, mas não é autorização e não comprova acesso do usuário à clínica. A autorização contextual real permanece para as fases 8.4, 8.5 e 8.6.

## Decisão sobre documento por clínica

A validação de duplicidade de documento de tutor passou a considerar `ClinicId`. Foi criado índice composto não único em `(ClinicId, Document)` para apoiar consultas/validação, evitando unicidade global e evitando depender de índice único filtrado com documento nulo no provider MySQL/EF. A regra amigável permanece na Application.

## Testes criados/executados

- Testes de domínio foram atualizados para criação válida com `ClinicId`, rejeição de zero/negativo e preservação das validações existentes.
- Testes de criação/validação de pet foram atualizados para `ClinicId` válido, `ClinicId` inválido e tutor de outra clínica.
- Testes de update de pet garantem que `UpdatePetRequest` não expõe `ClinicId` e que a validação de tutor usa o `ClinicId` existente do `Patient`.
- Testes de Infrastructure impactados por `Tutor`/`Patient` passaram a criar `Organization`/`Clinic` explicitamente por meio de `ClinicalScopeTestData`, sem seed global.
- Durante o PR 199 houve falhas intermediárias de build/testes; o fluxo foi corrigido e o CI final do PR 199 passou com sucesso.
- Nesta revisão pós-merge, os comandos de validação foram tentados no ambiente disponível; `dotnet` não está instalado no container atual, portanto a reexecução local de build/testes depende de ambiente local/CI.

## Riscos remanescentes

- Ainda não há autorização contextual real por usuário/clínica.
- `ClinicId` informado no payload pode ser falsificado até as fases de autorização contextual.
- Ainda não há validação amigável de existência da `Clinic` na Application; a integridade é garantida pela FK obrigatória para `Clinics`, e a validação amigável fica como dívida para fase posterior.
- O backfill da migration usa a primeira clínica disponível ou uma clínica transitória de compatibilidade; isso precisa ser revisado para dados reais.
- O `Down` da migration remove FKs, índices e colunas `ClinicId`, mas não remove automaticamente a organização/clínica transitória eventualmente criada para compatibilidade, para evitar apagar dados que podem ter sido reutilizados após o rollback.
- Listagens ainda não filtram por contexto clínico.

## Fora do escopo desta fase

Esta fase não implementa:

- `ClinicId` em `Pet`.
- `ClinicId` em `Attendance`.
- `ClinicId` em `MedicalRecord`.
- `ClinicId` em `ClinicalEvolution`.
- `ClinicId` em `Prescription`.
- Alterações em `PrescriptionItem`.
- Alterações em `ClinicalAuditLog`.
- `CurrentClinicalContext`.
- `UserClinicAccess`.
- Autorização contextual.
- Filtros por contexto.
- Auditoria contextual.
- `ClinicUnitId` como escopo.
- Tutor multi-clínica.
- Front-end.

## Revisão pós-merge — Fase 8.3.1.1

A revisão técnica pós-merge confirmou que a Fase 8.3.1 permaneceu restrita a `Tutor` e `Patient`:

- Não houve relaxamento de FKs: `ClinicId` segue obrigatório no modelo final e as FKs para `Clinics` usam `DeleteBehavior.Restrict`.
- `Pet` permanece sem `ClinicId` direto; projeções de detalhes de pet expõem o `ClinicId` vindo de `Patient`.
- `UpdatePetRequest` não expõe `ClinicId`; updates usam o escopo clínico já persistido no `Patient`.
- Não houve avanço para `Attendance`, autorização contextual, `CurrentClinicalContext`, `UserClinicAccess`, filtros globais ou front-end.
- A correção de build em `UpdatePetUseCase` e os ajustes de testes de Infrastructure com `ClinicalScopeTestData` fazem parte do estado final validado no PR 199.

## Próxima fase recomendada

Fase 8.3.2 — Introdução de `ClinicId` em `Attendance`, somente após esta revisão 8.3.1.1.
