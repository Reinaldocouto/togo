# TOGO — Fase 6.4.4: Revisão de DeleteBehavior.Cascade clínico em MedicalRecord

## 1. Contexto da Fase 6.4

A Fase 6.4 trata débitos P1 restantes de persistência clínica segura da vertical `MedicalRecord`.

Os débitos acompanhados nesta trilha são:

- `MR-DEBT-001` — Soft Delete ausente.
- `MR-DEBT-005` — Política de retenção não implementada.
- `MR-DEBT-006` — `DeleteBehavior.Cascade` pendente de revisão.

Esta fase executa a revisão do comportamento de exclusão relacional para reduzir o risco de exclusão física indireta de histórico clínico.

## 2. Referência explícita ao MR-DEBT-006

A Fase 6.4.4 endereça diretamente o `MR-DEBT-006`.

O problema analisado é a existência de relacionamentos clínicos configurados com `DeleteBehavior.Cascade`, capazes de apagar fisicamente dependentes clínicos quando uma entidade principal é removida por engano, manutenção manual, teste inadequado ou futura feature fora das regras de preservação clínica.

## 3. Relação com as fases 6.4.1, 6.4.2 e 6.4.3

- A Fase 6.4.1 planejou a persistência clínica segura e separou Soft Delete, retenção e revisão de cascades como preocupações distintas.
- A Fase 6.4.2 implementou a base de Soft Delete em `MedicalRecord`.
- A Fase 6.4.3 consolidou filtros explícitos para impedir que fluxos clínicos padrão retornem `MedicalRecord` com `IsDeleted = true`.
- A Fase 6.4.4 complementa essas medidas removendo cascades perigosos nas constraints clínicas críticas.

## 4. Objetivo da fase

Mapear, revisar e ajustar comportamentos `DeleteBehavior.Cascade` em relações clínicas que poderiam causar perda física indireta de histórico clínico.

A meta não é criar uma funcionalidade de exclusão física, e sim tornar o modelo relacional mais seguro caso uma remoção física seja tentada em nível de infraestrutura.

## 5. Relações clínicas mapeadas

Foram mapeadas as configurações EF atuais, o snapshot de modelo e migrations históricas.

Relações priorizadas:

| Relação | Entidade principal | Entidade dependente | Status antes da fase | Decisão da Fase 6.4.4 |
| --- | --- | --- | --- | --- |
| `Patient -> MedicalRecord` | `Patient` | `MedicalRecord` | `Cascade` | Alterar para `Restrict` |
| `Patient -> Attendance` | `Patient` | `Attendance` | `Cascade` | Alterar para `Restrict` |
| `Patient -> Pet` | `Patient` | `Pet` | `Cascade` | Manter `Cascade` |
| `Attendance -> ClinicalEvolution` | `Attendance` | `ClinicalEvolution` | `Cascade` | Alterar para `Restrict` |
| `Attendance -> Prescription` | `Attendance` | `Prescription` | `Cascade` | Alterar para `Restrict` |
| `Prescription -> PrescriptionItem` | `Prescription` | `PrescriptionItem` | `Cascade` | Manter `Cascade` |

## 6. Cascades encontrados

Foram encontrados `DeleteBehavior.Cascade` nas seguintes configurações clínicas:

- `AttendanceConfiguration`: `Patient -> Attendance`.
- `ClinicalEvolutionConfiguration`: `Attendance -> ClinicalEvolution`.
- `MedicalRecordConfiguration`: `Patient -> MedicalRecord`.
- `PetConfiguration`: `Patient -> Pet`.
- `PrescriptionConfiguration`: `Attendance -> Prescription`.
- `PrescriptionItemConfiguration`: `Prescription -> PrescriptionItem`.

As migrations históricas mantêm o histórico de criação original com `ReferentialAction.Cascade`. Elas não foram editadas retroativamente. A mudança foi aplicada em uma nova migration.

## 7. Decisão por relação

### 7.1 `Patient -> MedicalRecord`

**Decisão:** alterar para `DeleteBehavior.Restrict`.

**Justificativa:** `MedicalRecord` contém histórico clínico direto. A remoção física de `Patient` não deve apagar prontuário por efeito colateral. A preservação deve ser governada por Soft Delete e por política futura de retenção.

### 7.2 `Patient -> Attendance`

**Decisão:** alterar para `DeleteBehavior.Restrict`.

**Justificativa:** `Attendance` representa atendimento clínico e pode estar relacionado a evolução clínica e prescrição. Apagar atendimentos por cascata a partir de `Patient` comprometeria rastreabilidade clínica.

### 7.3 `Patient -> Pet`

**Decisão:** manter `DeleteBehavior.Cascade`.

**Justificativa:** no modelo atual, `Pet` é uma especialização/detalhe 1:1 de `Patient`, usando a própria chave `PatientId` como PK/FK. A entidade `Pet` não foi tratada nesta fase como histórico clínico independente. A exclusão física de `Patient` continua não sendo um fluxo clínico público introduzido por esta fase, e os vínculos de histórico clínico (`MedicalRecord` e `Attendance`) agora bloqueiam remoções físicas de pacientes com histórico associado.

### 7.4 `Attendance -> ClinicalEvolution`

**Decisão:** alterar para `DeleteBehavior.Restrict`.

**Justificativa:** `ClinicalEvolution` registra evolução clínica vinculada ao atendimento. A remoção física de `Attendance` não deve apagar evoluções por cascata.

### 7.5 `Attendance -> Prescription`

**Decisão:** alterar para `DeleteBehavior.Restrict`.

**Justificativa:** `Prescription` contém histórico de prescrição e pode possuir itens. A remoção física de `Attendance` não deve apagar prescrições por cascata.

### 7.6 `Prescription -> PrescriptionItem`

**Decisão:** manter `DeleteBehavior.Cascade`.

**Justificativa:** `PrescriptionItem` é composição estrita de `Prescription` e não possui sentido clínico isolado fora da prescrição. O risco de perda indireta por remoção de `Attendance` foi reduzido porque `Attendance -> Prescription` passou a ser `Restrict`. Esta fase não criou endpoint nem fluxo de exclusão física de prescrição.

## 8. Alterações realizadas em EF

Foram alteradas as configurações EF para usar `DeleteBehavior.Restrict` em relações clínicas críticas:

- `MedicalRecordConfiguration`: `Patient -> MedicalRecord`.
- `AttendanceConfiguration`: `Patient -> Attendance`.
- `ClinicalEvolutionConfiguration`: `Attendance -> ClinicalEvolution`.
- `PrescriptionConfiguration`: `Attendance -> Prescription`.

Foram mantidas como `Cascade`, com justificativa documentada:

- `PetConfiguration`: `Patient -> Pet`.
- `PrescriptionItemConfiguration`: `Prescription -> PrescriptionItem`.

## 9. Migration criada

Foi criada a migration:

- `ReviewClinicalCascadeDeleteBehavior`

A migration remove e recria as FKs críticas com `ReferentialAction.Restrict`:

- `FK_Attendances_Patients_PatientId`.
- `FK_ClinicalEvolutions_Attendances_AttendanceId`.
- `FK_MedicalRecords_Patients_PatientId`.
- `FK_Prescriptions_Attendances_AttendanceId`.

O `AppDbContextModelSnapshot` foi atualizado para refletir as novas decisões.

## 10. Testes criados/alterados

Foi criado o arquivo de testes de infraestrutura:

- `backend/src/Togo.Infrastructure.Tests/Persistence/ClinicalCascadeDeleteBehaviorTests.cs`

Cenários cobertos:

- O modelo EF expõe `Restrict` para relações clínicas críticas e mantém `Cascade` em `Prescription -> PrescriptionItem`.
- Remover fisicamente `Patient` com `MedicalRecord` associado falha por FK e preserva o prontuário.
- Remover fisicamente `Patient` com `Attendance` e `MedicalRecord` não apaga prontuário por cascata.
- Remover fisicamente `Attendance` com `ClinicalEvolution` falha por FK e preserva a evolução.
- Remover fisicamente `Attendance` com `Prescription` e `PrescriptionItem` falha por FK e preserva prescrição e item.
- Soft Delete de `MedicalRecord` continua persistindo o registro e queries padrão do repositório continuam ignorando `IsDeleted = true`.

## 11. Impacto em MedicalRecord

`MedicalRecord` passa a ser protegido contra exclusão física indireta via remoção de `Patient`.

Essa mudança não altera a entidade de domínio nem a semântica de Soft Delete. `IsDeleted`, `DeletedAt` e `DeletedByUserId` continuam sendo os mecanismos mínimos já implementados para remoção lógica.

## 12. Impacto em Patient, Attendance, Prescription e ClinicalEvolution

- `Patient`: tentativas de exclusão física passam a ser bloqueadas quando houver `MedicalRecord` ou `Attendance` associado.
- `Attendance`: tentativas de exclusão física passam a ser bloqueadas quando houver `ClinicalEvolution` ou `Prescription` associada.
- `ClinicalEvolution`: deixa de ser removida fisicamente por cascata a partir de `Attendance`.
- `Prescription`: deixa de ser removida fisicamente por cascata a partir de `Attendance`.
- `PrescriptionItem`: continua dependente estrito de `Prescription`, com cascade mantido e justificado.

## 13. Confirmação de que Soft Delete continua funcionando

Soft Delete de `MedicalRecord` continua funcionando sem alteração estrutural adicional.

Os testes de infraestrutura validam que um `MedicalRecord` marcado com `IsDeleted = true` permanece fisicamente persistido e deixa de ser retornado pelos métodos padrão do repositório.

## 14. Confirmação de que retenção não foi implementada

Esta fase não implementou política de retenção clínica.

Não foram criados expurgo, arquivamento, job, scheduler, worker, endpoint administrativo ou automação de remoção física.

## 15. Riscos remanescentes

- Ainda não existe política formal de retenção clínica; o débito `MR-DEBT-005` permanece para fase futura.
- `Prescription -> PrescriptionItem` permanece com `Cascade` por composição estrita. O risco é mitigado pela restrição em `Attendance -> Prescription`, mas uma exclusão física direta de `Prescription` ainda removeria seus itens.
- `Patient -> Pet` permanece com `Cascade` por modelagem 1:1 de especialização. Caso `Pet` passe a carregar histórico clínico independente no futuro, essa decisão deve ser reavaliada.
- Os testes usam SQLite em memória; o provider principal do projeto é MySQL/Pomelo. A migration foi criada para alterar FKs relacionais em MySQL, mas a validação local de runtime usa a infraestrutura de testes existente com SQLite.

## 16. Critérios de aceite

Critérios atendidos pela fase:

- Cascades clínicos relevantes foram mapeados.
- Relações clínicas críticas foram revisadas.
- Cascades perigosos foram alterados para `Restrict`.
- Cascades mantidos têm justificativa documentada.
- Migration `ReviewClinicalCascadeDeleteBehavior` foi criada.
- `AppDbContextModelSnapshot` foi atualizado.
- Testes de infraestrutura validam bloqueio de exclusão relacional.
- Soft Delete de `MedicalRecord` continua funcionando.
- Queries padrão continuam respeitando `IsDeleted`.
- Não foi criado endpoint DELETE público.
- Não foi implementada retenção clínica.
- Não foi criado frontend.
- Não foram alterados JWT, User/Profile ou autorização granular.

## 17. Fora do escopo

Permaneceu fora do escopo:

- Retenção clínica.
- Expurgo físico.
- Job, scheduler ou worker.
- Endpoint DELETE público.
- Frontend.
- Consulta administrativa de registros deletados.
- Restauração de registros.
- Alterações em JWT.
- Alterações em User/Profile.
- Autorização granular.
- Auditoria de leitura.
- Auditoria de acesso negado.
- Docker, Redis, RabbitMQ ou Kubernetes.

## 18. Próxima fase recomendada

**Fase 6.4.5 — Planejamento e decisão de política de retenção clínica.**

Objetivo recomendado: formalizar a política inicial de retenção clínica de `MedicalRecord`, definindo preservação, arquivamento e eventual expurgo sem implementar automação prematura.
