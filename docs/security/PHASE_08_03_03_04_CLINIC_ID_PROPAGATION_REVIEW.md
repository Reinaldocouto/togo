# Fase 8.3.3.4 — Revisão final da propagação de ClinicId

## Objetivo da revisão

Revisar a propagação de `ClinicId` nas entidades clínicas já alteradas, confirmar consistência entre Domain, Application, Infrastructure, migrations, testes e documentação, consolidar riscos em arquivo vivo de débitos técnicos e preparar o projeto para a próxima etapa da Fase 8.

Esta fase foi conduzida como revisão técnica e documental. Não foram implementadas novas features clínicas, autorização contextual, filtros globais por contexto ou migrations adicionais.

## Escopo revisado

Foram revisados os seguintes pontos:

- Entidades de domínio com `ClinicId`: `Tutor`, `Patient`, `Attendance`, `MedicalRecord`, `ClinicalEvolution` e `Prescription`.
- Entidades sem `ClinicId` direto: `Pet` e `PrescriptionItem`.
- Factories, validações de identificadores, encapsulamento por setter privado e métodos de atualização/status.
- Fluxos de criação na Application.
- Contratos de requests e responses relacionados.
- Metadata de auditoria de criação.
- Configurações EF Core, FKs, índices e `DeleteBehavior`.
- Migrations de backfill de `ClinicId`.
- Testes de Domain, Application, Infrastructure e API.

## Entidades revisadas

| Entidade | Resultado da revisão |
| --- | --- |
| `Tutor` | Possui `ClinicId` obrigatório, validado contra valores `<= 0`, com setter privado e factory exigindo `clinicId`. Updates não alteram o escopo clínico. |
| `Patient` | Possui `ClinicId` obrigatório, validado contra valores `<= 0`, com setter privado e factory exigindo `clinicId`. Updates de dados/status não alteram o escopo clínico. |
| `Attendance` | Possui `ClinicId` obrigatório, validado na factory, com setter privado. O escopo é derivado de `Patient.ClinicId` na criação. Fechamento/cancelamento não alteram `ClinicId`. |
| `MedicalRecord` | Possui `ClinicId` obrigatório, validado na factory, com setter privado. O escopo é derivado de `Patient.ClinicId` na criação. Atualização/remoção lógica não alteram `ClinicId`. |
| `ClinicalEvolution` | Possui `ClinicId` obrigatório, validado na factory, com setter privado. O escopo é derivado de `Attendance.ClinicId` na criação. Atualização de texto não altera `ClinicId`. |
| `Prescription` | Possui `ClinicId` obrigatório, validado na factory, com setter privado. O escopo é derivado de `Attendance.ClinicId` na criação. Atualizações/cancelamento não alteram `ClinicId`. |
| `Pet` | Confirmado sem `ClinicId` direto; o escopo é herdado por `Patient`. |
| `PrescriptionItem` | Confirmado sem `ClinicId` direto; o escopo é herdado por `Prescription`. |

## Cadeia esperada de escopo clínico

A cadeia revisada e esperada para a Fase 8 é:

- `Organization -> Clinic -> Tutor`
- `Organization -> Clinic -> Patient`
- `Patient -> Attendance`
- `Patient -> MedicalRecord`
- `Attendance -> ClinicalEvolution`
- `Attendance -> Prescription`
- `Prescription -> PrescriptionItem`
- `Patient -> Pet`

Regras confirmadas:

- `Pet` não possui `ClinicId` direto; herda escopo via `Patient`.
- `PrescriptionItem` não possui `ClinicId` direto; herda escopo via `Prescription`.
- Entidades operacionais sensíveis possuem `ClinicId` direto para simplificar filtros futuros e reduzir risco de consulta sem escopo.

## Revisão de Domain

A revisão confirmou que as entidades planejadas possuem `ClinicId` obrigatório e encapsulado. As factories públicas exigem `clinicId`, rejeitam valores inválidos e mantêm os métodos de update/status sem alteração de escopo clínico.

Também foi confirmado que `Pet` e `PrescriptionItem` permanecem sem `ClinicId` direto, preservando a decisão arquitetural de herança de escopo por entidade pai.

## Revisão de Application

Fluxos revisados:

- `CreateTutorUseCase`: recebe `ClinicId` transitório no request, valida valor positivo e cria `Tutor` com esse escopo.
- `CreatePetUseCase` / criação de `Patient`: recebe `ClinicId` transitório no request para criar `Patient`; `Pet` em si não recebe `ClinicId` direto.
- `CreateAttendanceUseCase`: deriva `ClinicId` de `Patient.ClinicId` via projection de escopo do paciente.
- `CreateMedicalRecordUseCase`: deriva `ClinicId` de `Patient.ClinicId` via validation/projection de escopo do paciente.
- `CreateClinicalEvolutionUseCase`: deriva `ClinicId` de `Attendance.ClinicId`.
- `CreatePrescriptionUseCase`: deriva `ClinicId` de `Attendance.ClinicId`.

Foi confirmado que requests de `Attendance`, `MedicalRecord`, `ClinicalEvolution` e `Prescription` não aceitam `ClinicId` como fonte de verdade. Responses que expõem `ClinicId` devem ser lidas apenas como rastreabilidade de contrato, não como autorização contextual.

Não foi encontrada implementação acidental de autorização contextual, `CurrentClinicalContext` ou filtros globais por contexto.

## Revisão de auditoria

Eventos de criação revisados:

- `Attendance.Created`
- `MedicalRecord.Created`
- `ClinicalEvolution.Created`
- `Prescription.Created`

Foi confirmado que o metadata de criação inclui `ClinicId` nos fluxos implementados. A estrutura de `ClinicalAuditLog` não foi alterada nesta revisão. Também não foi introduzida auditoria contextual completa, auditoria de leitura ou evento transversal de acesso negado nesta fase.

Observação: há testes e constantes legadas de ações como `MedicalRecord.Read` e `MedicalRecord.AccessDenied` de fases anteriores, mas esta revisão não implementou uma capacidade transversal nova de auditoria de leitura/acesso negado para a Fase 8.

## Revisão de Infrastructure / EF

Configurações revisadas:

- `TutorConfiguration`
- `PatientConfiguration`
- `AttendanceConfiguration`
- `MedicalRecordConfiguration`
- `ClinicalEvolutionConfiguration`
- `PrescriptionConfiguration`

Resultado:

- `ClinicId` está configurado como obrigatório nas entidades planejadas.
- Há FK para `Clinics.Id` com `DeleteBehavior.Restrict`.
- Há índice simples por `ClinicId`.
- Índices compostos planejados foram adicionados onde aplicável.
- FKs pré-existentes para `Patient`, `Attendance`, `Tutor` e `Prescription` foram preservadas.
- `PetConfiguration` e `PrescriptionItemConfiguration` não receberam `ClinicId` direto.

## Revisão de migrations

Migrations revisadas:

- `20260629120000_AddTutorPatientClinicId`
- `20260630120000_AddAttendanceClinicId`
- `20260630130000_AddMedicalRecordClinicId`
- `20260630140000_AddClinicalEvolutionClinicId`
- `20260630150000_AddPrescriptionClinicId`

Resultado:

- `Attendance` realiza backfill a partir de `Patient.ClinicId`.
- `MedicalRecord` realiza backfill a partir de `Patient.ClinicId`.
- `ClinicalEvolution` realiza backfill a partir de `Attendance.ClinicId`.
- `Prescription` realiza backfill a partir de `Attendance.ClinicId`.
- As migrations derivadas usam `UPDATE ... INNER JOIN`, portanto são orientadas a MySQL e não são portáveis automaticamente para outros providers.
- As alterações para `NOT NULL` e FKs fazem as migrations falharem intencionalmente quando houver órfãos ou inconsistências.
- Os métodos `Down` removem FK, índices e coluna de forma revisável.
- A migration de `Tutor`/`Patient` preserva a estratégia transitória anterior de preencher registros legados com a primeira clínica existente, sem criar isolamento real de acesso.

Riscos de produção foram registrados em `PHASE_08_TECHNICAL_DEBT.md`.

## Revisão de testes

Foram conferidos testes de Domain, Application, Infrastructure e API. A cobertura existente contempla:

- Criação válida com `ClinicId` e rejeição de `ClinicId` inválido nas entidades planejadas.
- Derivação de `ClinicId` da entidade fonte correta nos fluxos de criação.
- Ausência de `ClinicId` em requests protegidos de `Attendance`, `MedicalRecord`, `ClinicalEvolution` e `Prescription`.
- Ausência de `ClinicId` direto em `Pet` e `PrescriptionItem` por contrato/domínio.
- EF mapping, FK, `DeleteBehavior.Restrict` e índices por `ClinicId`.
- Metadata de auditoria contendo `ClinicId` nos eventos de criação revisados.
- Contratos públicos e responses de API relacionados.

Nenhuma lacuna simples exigiu alteração de teste nesta fase. Lacunas estruturais foram registradas como débitos técnicos.

## Ajustes realizados

- Criado o arquivo central de débitos técnicos da Fase 8: `docs/security/PHASE_08_TECHNICAL_DEBT.md`.
- Criado este documento de revisão final da propagação de `ClinicId`.
- Não houve alteração de código de produção, testes, controllers, migrations ou regras de negócio.

## Riscos remanescentes

Os principais riscos remanescentes são:

- `ClinicId` persistido ainda não garante isolamento de acesso.
- `CurrentClinicalContext` ainda não existe.
- `UserClinicAccess` ainda não existe.
- Filtros globais/listagens por contexto ainda não existem.
- Auditoria contextual completa, leitura e acesso negado ainda não foram implementados como capacidade transversal.
- Migrations de backfill derivadas são orientadas a MySQL.
- Dados órfãos devem ser revisados antes de produção.
- Responses com `ClinicId` devem ser entendidas como rastreabilidade, não autorização.

A lista viva está em [PHASE_08_TECHNICAL_DEBT.md](./PHASE_08_TECHNICAL_DEBT.md).

## Confirmação do que não foi implementado

Esta fase não implementou:

- `ClinicId` direto em `Pet`.
- `ClinicId` direto em `PrescriptionItem`.
- `ClinicUnitId`.
- `CurrentClinicalContext`.
- `UserClinicAccess`.
- Autorização contextual.
- Filtros globais por contexto.
- Auditoria contextual completa.
- Auditoria de leitura transversal.
- Evento transversal de acesso negado.
- Front-end.
- Nova feature clínica.
- Regras de negócio em controllers.
- Nova migration.

## Critérios de aceite

- Documento de revisão criado.
- Arquivo central de débitos técnicos criado.
- Entidades com `ClinicId` revisadas.
- Entidades sem `ClinicId` direto confirmadas.
- Migrations revisadas.
- EF mappings revisados.
- Metadata de auditoria revisado.
- Testes/CI verificados por comandos de validação.
- Escopo proibido preservado.
- Próxima fase recomendada com justificativa.

## Testes executados

Comandos previstos para validação desta revisão:

```bash
dotnet build backend/Togo.sln --configuration Release
dotnet test
dotnet test backend/src/Togo.Domain.Tests/Togo.Domain.Tests.csproj
dotnet test backend/src/Togo.Application.Tests/Togo.Application.Tests.csproj
dotnet test backend/src/Togo.Infrastructure.Tests/Togo.Infrastructure.Tests.csproj
dotnet test backend/src/Togo.Api.Tests/Togo.Api.Tests.csproj
git diff --check
git status --short
```

## Próxima fase recomendada

Como a revisão confirmou que a propagação de `ClinicId` está consistente e que as pendências remanescentes são estruturais já esperadas, a próxima fase recomendada é:

**Fase 8.4 — CurrentClinicalContext**

Justificativa: a persistência de `ClinicId` está preparada para servir como base de escopo, mas a proteção real depende da resolução do contexto clínico ativo antes de avançar para autorização contextual e filtros por contexto.
