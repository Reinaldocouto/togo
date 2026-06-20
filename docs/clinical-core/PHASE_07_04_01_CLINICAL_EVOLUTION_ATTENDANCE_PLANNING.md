# Fase 7.4.1 — Planejamento técnico da integração ClinicalEvolution com Attendance

## 1. Objetivo

A Fase 7.4.1 cria o planejamento técnico para integrar `ClinicalEvolution` de forma segura à vertical `Attendance`.

Esta fase é exclusivamente documental. Ela mapeia o estado atual de `ClinicalEvolution`, sua relação com `Attendance`, a separação em relação a `MedicalRecord`, lacunas de autorização, autoria e auditoria, riscos clínicos e técnicos, decisões recomendadas e o fracionamento incremental da Fase 7.4.

Não foram implementados código C#, testes, migrations, schema, endpoints, policies, permissions, repositories, use cases, DTOs, audit log, frontend ou infraestrutura nesta fase.

## 2. Contexto

A Fase 7 é a expansão clínica e operacional após o hardening de `MedicalRecord` encerrado na Fase 6. A Fase 7.0 recomendou `Attendance` como eixo operacional inicial porque `Attendance` já existia no domínio, aplicação, infraestrutura e API, enquanto `ClinicalEvolution` e `Prescription` já apontavam para `Attendance` como vínculo natural.

A Fase 7.1 planejou a vertical `Attendance` pós-hardening de `MedicalRecord`. A Fase 7.2 encerrou o hardening mínimo de autorização granular de `Attendance`. A Fase 7.3 encerrou autoria e auditoria mínima de eventos críticos de `Attendance`.

Com `Attendance` mais protegido, a próxima expansão segura é planejar `ClinicalEvolution` como registro clínico pertencente a um episódio de atendimento, sem inflar `MedicalRecord` e sem expor texto clínico sensível antes de contratos, autorização, autoria e auditoria mínimos.

## 3. Fontes consultadas

### 3.1 Documentos de contexto consultados

- `docs/clinical-core/PHASE_07_00_CLINICAL_OPERATIONAL_EXPANSION_PLANNING.md`.
- `docs/clinical-core/PHASE_07_01_ATTENDANCE_TECHNICAL_PLANNING.md`.
- `docs/clinical-core/PHASE_07_02_04_ATTENDANCE_AUTHORIZATION_CLOSURE.md`.
- `docs/clinical-core/PHASE_07_03_06_ATTENDANCE_AUTHORSHIP_AUDIT_CLOSURE.md`.
- `docs/clinical-core/PHASE_06_06_05_MEDICAL_RECORD_PHASE_06_CLOSURE.md`.

### 3.2 Arquivos técnicos consultados

- `backend/src/Togo.Domain/Entities/ClinicalEvolution.cs`.
- `backend/src/Togo.Domain/Entities/Attendance.cs`.
- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalEvolutionConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/AttendanceConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`.

### 3.3 Inspeções obrigatórias executadas

Foram executadas as inspeções obrigatórias:

```bash
rg -n "class ClinicalEvolution|ClinicalEvolution|Evolution|AttendanceId|RegisteredAt|Text|Type" backend/src docs/clinical-core
rg -n "ClinicalEvolutionConfiguration|HasOne|Attendance|DeleteBehavior|ClinicalEvolution" backend/src/Togo.Infrastructure backend/src/Togo.Domain
rg -n "ClinicalEvolution" backend/src/Togo.Application backend/src/Togo.Api backend/src/*Tests || true
rg -n "Attendance" backend/src/Togo.Domain/Entities/ClinicalEvolution.cs backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalEvolutionConfiguration.cs
rg -n "CreatedByUserId|UpdatedByUserId|IClinicalAuditLogWriter|ClinicalAuditLog|CurrentUser" backend/src/Togo.Domain backend/src/Togo.Application backend/src/Togo.Infrastructure
```

## 4. Relação com Attendance

`Attendance` representa o episódio clínico e operacional. Ele está vinculado diretamente a `Patient` por `PatientId`, possui ciclo de vida com `Open`, `Closed` e `Canceled`, autoria mínima para criação, fechamento e cancelamento, e audit log mínimo para eventos críticos conforme a Fase 7.3.

`ClinicalEvolution` representa o registro clínico textual dentro desse episódio. Portanto, a integração segura deve manter `ClinicalEvolution` vinculada a `Attendance` por `AttendanceId`.

A entidade atual de `ClinicalEvolution` já possui `AttendanceId`, e a configuração EF já define relacionamento com `Attendance` por foreign key e `DeleteBehavior.Restrict`. Essa decisão é compatível com a diretriz clínica de não remover evoluções por cascade ao remover um atendimento.

Diretriz recomendada:

```text
ClinicalEvolution deve permanecer vinculada a Attendance por AttendanceId.
```

A superfície pública futura deve ser orientada pelo episódio:

```text
Começar com criação e listagem por AttendanceId, não com listagem global.
```

## 5. Relação com MedicalRecord

`MedicalRecord` permanece a memória clínica longitudinal consolidada do paciente. Ele não deve ser inflado com listas de evoluções clínicas, atendimentos, prescrições, anexos, agenda, estoque ou financeiro.

A evolução clínica pertence ao episódio `Attendance`; indiretamente, ela compõe a história do paciente porque `Attendance` aponta para `Patient`. Nesta fase, não se recomenda vínculo direto novo entre `ClinicalEvolution` e `MedicalRecord`.

Resposta explícita à pergunta de arquitetura:

```text
Não. MedicalRecord permanece memória longitudinal. Attendance é o episódio. ClinicalEvolution pertence ao episódio Attendance.
```

## 6. Estado atual de ClinicalEvolution

`ClinicalEvolution` já existe como entidade de domínio.

### 6.1 Propriedades atuais

A entidade possui:

- `Id`.
- `AttendanceId`.
- `RegisteredAt`.
- `Type`.
- `Text`.

### 6.2 Vínculo atual com Attendance

O vínculo é feito por `AttendanceId`. A entidade não possui navigation property explícita para `Attendance`, mas a configuração EF usa `HasOne<Attendance>()`, `WithMany()`, `HasForeignKey(e => e.AttendanceId)` e `OnDelete(DeleteBehavior.Restrict)`.

### 6.3 Timestamps atuais

`ClinicalEvolution` possui `RegisteredAt` como timestamp clínico do registro da evolução.

Não possui atualmente:

- `CreatedAt`.
- `UpdatedAt`.
- `DeletedAt`.
- `RegisteredByUserId`.
- `CreatedByUserId`.
- `UpdatedByUserId`.

### 6.4 Tipo de evolução

`ClinicalEvolution` possui `Type` do enum `EvolutionType`. A configuração EF persiste o enum como string.

### 6.5 Texto clínico

`ClinicalEvolution` possui `Text` obrigatório, armazenado como coluna `text` na configuração EF. O domínio normaliza o texto com `Trim()` na criação e na atualização.

### 6.6 Validações existentes

A entidade valida:

- `attendanceId > 0`.
- `registeredAt` diferente de `default`.
- `text` obrigatório e não composto apenas por espaços.

Não foram identificadas validações de:

- limite máximo de tamanho para `Text`.
- `DateTimeKind.Utc` para `RegisteredAt`.
- profissional responsável.
- atendimento aberto versus fechado/cancelado.
- existência de `Attendance` no domínio.
- autorização/autoria/auditoria no domínio.

### 6.7 Métodos de domínio existentes

A entidade possui:

- `Create(long attendanceId, DateTime registeredAt, EvolutionType type, string text)`.
- `UpdateText(string text)`.

Não possui métodos para soft delete, retificação, assinatura clínica, anexos ou alteração de tipo.

## 7. Arquivos encontrados

Foram encontrados arquivos diretamente relacionados a `ClinicalEvolution` no domínio, EF e testes de persistência transversal:

- `backend/src/Togo.Domain/Entities/ClinicalEvolution.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalEvolutionConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`, com `DbSet<ClinicalEvolution>`.
- `backend/src/Togo.Infrastructure.Tests/Persistence/ClinicalCascadeDeleteBehaviorTests.cs`, validando `DeleteBehavior.Restrict` para `ClinicalEvolution.AttendanceId` e bloqueio de remoção física de `Attendance` com evolução vinculada.

## 8. Arquivos ausentes

Não foram encontrados os seguintes artefatos específicos de `ClinicalEvolution`:

- `backend/src/Togo.Application/ClinicalEvolutions`.
- `backend/src/Togo.Api/Controllers/ClinicalEvolutionsController.cs`.
- `backend/src/Togo.Application.Tests/ClinicalEvolutions`.
- `backend/src/Togo.Infrastructure.Tests/Repositories/ClinicalEvolutionRepositoryTests.cs`.
- `backend/src/Togo.Api.Tests/Controllers/ClinicalEvolutionsControllerTests.cs`.

Também não foram identificados, nas buscas obrigatórias, contracts, use cases, repository interface, repository implementation, controller ou testes específicos de controller/use case/repository para `ClinicalEvolution`.

## 9. Relacionamento atual com Attendance

O relacionamento atual é:

```text
ClinicalEvolution.AttendanceId -> Attendance.Id
DeleteBehavior.Restrict
```

A configuração também define índices em `AttendanceId` e `RegisteredAt`.

Esse estado confirma que o projeto já modela `ClinicalEvolution` como filha lógica de `Attendance`, não como filha direta de `MedicalRecord`.

## 10. Superfície pública atual

Não foi encontrado controller público de `ClinicalEvolution`.

Consequentemente, não há endpoint público atual para:

- criar evolução clínica;
- listar evoluções por atendimento;
- buscar evolução por id;
- atualizar texto clínico;
- excluir evolução clínica.

Essa ausência reduz risco imediato de exposição HTTP, mas também significa que qualquer implementação futura deve começar por contratos mínimos e autorização antes de expor texto clínico sensível.

## 11. Lacunas de autorização

Não foram identificadas permissões, policies ou matriz por profile específicas para `ClinicalEvolution`.

Como não há controller atual, também não há `[Authorize]` específico para endpoints de evolução clínica.

Decisão recomendada:

```text
ClinicalEvolution deve ter autorização granular antes de endpoint público.
```

Permissões candidatas futuras, sem implementação nesta fase:

```text
ClinicalEvolution.Read
ClinicalEvolution.Create
ClinicalEvolution.Update
ClinicalEvolution.Delete
```

Recomendação conservadora inicial:

- não criar listagem global de evoluções;
- limitar leitura a `AttendanceId` ou detalhe por id com validação do vínculo;
- discutir se `Delete` deve existir antes de política de soft delete/retificação;
- evitar expor `Text` em listagens amplas.

## 12. Lacunas de autoria

`ClinicalEvolution` não possui autoria mínima própria.

Ausências atuais:

- `CreatedByUserId`.
- `CreatedAt`.
- `UpdatedByUserId`.
- `UpdatedAt`.
- `RegisteredByUserId`.

Decisão recomendada:

```text
ClinicalEvolution deve ter autoria mínima desde a primeira implementação pública.
```

Campos candidatos para a fase técnica futura:

```text
RegisteredByUserId
RegisteredAt
UpdatedByUserId
UpdatedAt
```

Justificativa: `RegisteredAt` já existe como timestamp clínico do registro. `RegisteredByUserId` explicitaria o profissional/usuário responsável pelo registro clínico. `UpdatedByUserId` e `UpdatedAt` seriam necessários se `UpdateText` for exposto publicamente. Alternativamente, a fase 7.4.2 pode avaliar alinhamento com o padrão `CreatedByUserId`, `CreatedAt`, `UpdatedByUserId`, `UpdatedAt` usado em `Attendance` e `MedicalRecord`.

Nenhum campo deve ser adicionado nesta fase documental.

## 13. Lacunas de audit log

Existe infraestrutura transversal de `ClinicalAuditLog`, mas não foram identificadas actions específicas de `ClinicalEvolution`, como:

- `ClinicalEvolution.Created`.
- `ClinicalEvolution.Updated`.
- `ClinicalEvolution.Deleted`.

Decisão recomendada:

```text
AuditLog mínimo futuro para ClinicalEvolution.Created e ClinicalEvolution.Updated.
```

`ClinicalEvolution.Deleted` deve permanecer fora até haver decisão explícita sobre delete, soft delete, retificação ou bloqueio de exclusão.

Recomendação obrigatória para privacidade:

```text
Não registrar texto clínico completo em audit log.
```

O audit log futuro deve registrar metadados mínimos, como entidade, id, `AttendanceId`, ação, usuário e timestamp, evitando conteúdo clínico textual completo.

## 14. Riscos de segurança, privacidade e integridade clínica

Riscos mapeados:

- exposição de texto clínico sensível;
- criação de endpoint sem autorização granular;
- ausência de autoria;
- ausência de audit log;
- vincular evolução ao paciente errado via `AttendanceId` incorreto;
- permitir evolução em atendimento fechado sem decisão de negócio;
- permitir evolução em atendimento cancelado sem decisão de negócio;
- permitir evolução sem profissional responsável;
- permitir update/delete de texto clínico sem rastreabilidade;
- retornar texto clínico em listagens amplas;
- misturar evolução clínica com prescrição;
- acoplar evolução clínica ao prontuário longitudinal indevidamente;
- criar migration antes de decidir contrato mínimo;
- criar endpoint antes de decidir autorização, autoria e minimização de response;
- registrar texto clínico completo em audit log;
- não definir limites de tamanho para texto clínico antes de exposição pública;
- não definir se `RegisteredAt` deve ser fornecido pelo cliente, pelo servidor ou por ambos com restrições.

## 15. Decisões técnicas recomendadas

### 15.1 Vínculo

Manter `ClinicalEvolution` vinculada a `Attendance` por `AttendanceId`.

### 15.2 MedicalRecord

Não vincular diretamente `ClinicalEvolution` a `MedicalRecord` nesta trilha. `MedicalRecord` permanece memória longitudinal; `Attendance` permanece episódio; `ClinicalEvolution` pertence ao episódio.

### 15.3 Superfície inicial futura

Começar por:

- criação de evolução para um `AttendanceId` específico;
- listagem de evoluções por `AttendanceId`;
- sem listagem global;
- sem endpoint de exclusão no primeiro corte;
- update apenas se houver autoria e audit log mínimos desde o início.

### 15.4 Autorização

Definir permissões granulares antes de endpoint público. Permissões candidatas:

- `ClinicalEvolution.Read`.
- `ClinicalEvolution.Create`.
- `ClinicalEvolution.Update`.
- `ClinicalEvolution.Delete`.

A permissão `Delete` deve ser planejada com cautela e pode ficar reservada até decisão de soft delete/retificação.

### 15.5 Autoria

Adicionar autoria mínima antes da primeira exposição pública. A Fase 7.4.2 deve decidir entre:

- padrão clínico orientado a registro: `RegisteredByUserId`, `RegisteredAt`, `UpdatedByUserId`, `UpdatedAt`;
- padrão transversal já usado em `Attendance` e `MedicalRecord`: `CreatedByUserId`, `CreatedAt`, `UpdatedByUserId`, `UpdatedAt`.

Como `RegisteredAt` já existe, a recomendação inicial é avaliar `RegisteredByUserId` como par semântico de `RegisteredAt`, mantendo compatibilidade com `UpdatedByUserId` e `UpdatedAt` para alterações.

### 15.6 AuditLog

Planejar audit log mínimo para:

- `ClinicalEvolution.Created`;
- `ClinicalEvolution.Updated`, se update for exposto.

Não registrar o texto clínico completo no audit log. Usar metadados mínimos.

### 15.7 Status do Attendance

Antes de criar evolução, validar decisão de negócio para status do atendimento:

- permitido em `Open`;
- proibido ou restrito em `Closed`;
- proibido em `Canceled`, salvo decisão clínica formal futura.

A recomendação inicial é permitir criação apenas em atendimento `Open` até que haja regra de retificação/registro tardio.

### 15.8 Contratos e minimização

Os contratos futuros devem minimizar dados:

- request de criação com `AttendanceId` contextual ou em rota, `Type`, `Text` e talvez `RegisteredAt` se definido;
- response de detalhe com texto apenas quando necessário;
- listagem por atendimento com cuidado para não virar listagem ampla de texto clínico sensível;
- sem incluir dados completos de paciente, tutor ou prontuário na response de evolução.

## 16. Subfases futuras recomendadas

Fracionamento recomendado da Fase 7.4:

```text
7.4.1 — Planejamento técnico da integração ClinicalEvolution com Attendance
7.4.2 — Contratos/base técnica para ClinicalEvolution vinculada a Attendance
7.4.3 — Implementação mínima de ClinicalEvolution com autorização granular
7.4.4 — Autoria e AuditLog mínimos de ClinicalEvolution
7.4.5 — Testes e evidências da integração ClinicalEvolution
7.4.6 — Encerramento da trilha ClinicalEvolution
```

Para evitar PR grande, a Fase 7.4.2 deve permanecer focada em contratos/base e decisões, sem endpoint público e sem implementação ampla de persistência nova além do que for estritamente planejado.

## 17. Fora do escopo desta fase

Ficaram fora do escopo:

- alteração de entidade C#;
- criação de DTO/contract;
- repository de `ClinicalEvolution`;
- use case de `ClinicalEvolution`;
- controller de `ClinicalEvolution`;
- endpoint público;
- migration;
- alteração de schema;
- permission/policy;
- autorização;
- autoria;
- audit log;
- validação nova;
- testes;
- frontend;
- vínculo com prescrição;
- upload/anexo;
- assinatura clínica;
- histórico longitudinal no `MedicalRecord`;
- Docker, Redis, RabbitMQ ou Kubernetes.

## 18. Critérios de aceite da Fase 7.4.1

A fase é considerada concluída quando:

- este documento é criado;
- o estado atual de `ClinicalEvolution` é inspecionado;
- a relação com `Attendance` é documentada;
- a relação com `MedicalRecord` é documentada;
- lacunas de autorização são mapeadas;
- lacunas de autoria são mapeadas;
- lacunas de audit log são mapeadas;
- riscos de segurança e privacidade são registrados;
- decisões técnicas iniciais são propostas;
- subfases futuras são recomendadas;
- a próxima fase 7.4.2 é recomendada;
- nenhuma implementação é feita;
- nenhuma migration é criada;
- somente documentação é alterada;
- `git diff --check` passa.

## 19. Próxima fase recomendada

Próxima fase recomendada:

```text
Fase 7.4.2 — Contratos/base técnica para ClinicalEvolution vinculada a Attendance
```

Objetivo sugerido:

```text
Definir os contratos mínimos, permissões planejadas e estruturas base para ClinicalEvolution vinculada a Attendance, sem ainda expor endpoint público e sem persistir nova regra além do que já existir.
```
