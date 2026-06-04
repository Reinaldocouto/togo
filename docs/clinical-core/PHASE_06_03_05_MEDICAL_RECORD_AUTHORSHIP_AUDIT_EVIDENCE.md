# Fase 6.3.5 — Evidências finais de autoria e AuditLog clínico em MedicalRecord

## 1. Objetivo

Consolidar as evidências técnicas, documentais e de testes do encerramento da **Fase 6.3 — Auditoria e autoria clínica** da vertical `MedicalRecord`, comprovando que os débitos P1 de rastreabilidade clínica foram tratados no escopo incremental planejado:

- **MR-DEBT-004 — Controle de autoria ausente**;
- **MR-DEBT-002 — AuditLog ausente**.

Esta fase é majoritariamente documental e de validação. As únicas alterações de teste adicionadas foram complementares para lacunas reais de evidência sobre minimização de `MetadataJson` e perfil de usuário opcional no AuditLog.

## 2. Contexto da Fase 6.3

A Fase 6.3 sucede a Fase 6.2, que resolveu tecnicamente o débito de autorização granular mínima de `MedicalRecord`. O foco desta etapa foi rastreabilidade clínica mínima sem ampliar o produto para consulta pública de auditoria, frontend de auditoria, auditoria de leitura, auditoria de acesso negado ou garantias transacionais avançadas.

A trilha da Fase 6.3 seguiu a ordem técnica planejada:

1. planejar autoria e auditoria;
2. criar contratos de usuário atual e auditoria;
3. persistir autoria mínima em `MedicalRecord`;
4. remover defaults persistentes temporários das colunas de autoria;
5. persistir AuditLog clínico mínimo para criação e atualização;
6. consolidar evidências finais nesta fase.

## 3. Relação com MR-DEBT-004 — Controle de autoria ausente

O débito **MR-DEBT-004** foi tratado tecnicamente pela autoria clínica mínima implementada na Fase 6.3.3.

O tratamento adotado foi incremental e suficiente para o escopo planejado porque:

- `MedicalRecord` passou a possuir `CreatedByUserId`;
- `MedicalRecord` passou a possuir `CreatedAt`;
- `MedicalRecord` passou a possuir `UpdatedByUserId`;
- `MedicalRecord` passou a possuir `UpdatedAt`;
- o fluxo de criação preenche autoria de criação e de última atualização com o usuário autenticado atual;
- o fluxo de atualização preserva autoria original de criação;
- o fluxo de atualização altera somente a autoria da última modificação;
- a resolução do usuário atual vem de claims estáveis do usuário autenticado;
- não foi introduzida autoria fake ou hardcoded nos fluxos de produção.

## 4. Relação com MR-DEBT-002 — AuditLog ausente

O débito **MR-DEBT-002** foi tratado tecnicamente pelo AuditLog clínico mínimo implementado na Fase 6.3.4.

O tratamento adotado foi incremental e suficiente para o escopo planejado porque:

- foi criada a entidade persistente `ClinicalAuditLog`;
- foi criada a tabela `ClinicalAuditLogs` por migration;
- os eventos `MedicalRecord.Created` e `MedicalRecord.Updated` são registrados internamente pela Application;
- cada evento registra `UserId`, `UserProfile` quando disponível, `EntityName`, `EntityId`, `Action`, `OccurredAt` em UTC e `MetadataJson` mínimo;
- `MetadataJson` permanece restrito a metadados mínimos, atualmente `PatientId`;
- `MetadataJson` não registra `GeneralNotes` completo;
- `MetadataJson` não registra `FlagsJson` completo;
- não foi criado endpoint público de AuditLog;
- não foi criada tela frontend de AuditLog.

## 5. Resumo das fases 6.3.1 a 6.3.4

### 5.1 Fase 6.3.1 — Planejamento de autoria e auditoria

A Fase 6.3.1 definiu a estratégia incremental para tratar primeiro identidade/autoria e somente depois AuditLog persistido. A decisão evitou criar trilha de auditoria sem autor confiável.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_01_MEDICAL_RECORD_AUTHORSHIP_AUDIT_PLANNING.md`.

### 5.2 Fase 6.3.2 — Contratos de usuário atual e auditoria

A Fase 6.3.2 introduziu os contratos mínimos de usuário atual e auditoria:

- `CurrentUserInfo`;
- `ICurrentUserService`;
- `IClinicalAuditLogWriter`;
- `ClinicalAuditEvent`;
- constantes de ações em `MedicalRecordAuditActions`.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_02_CURRENT_USER_AND_AUDIT_CONTRACTS.md`.

### 5.3 Fase 6.3.3 — Implementação de autoria clínica mínima

A Fase 6.3.3 adicionou autoria ao domínio, configuração EF, use cases e testes de `MedicalRecord`.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_03_MEDICAL_RECORD_AUTHORSHIP_IMPLEMENTATION.md`.

### 5.4 Fase 6.3.3.1 — Hotfix de defaults persistentes

A Fase 6.3.3.1 removeu defaults persistentes que haviam sido úteis apenas para compatibilizar a migration de inclusão das colunas obrigatórias. A decisão final evita que novos registros recebam autoria artificial por default de banco.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_03_01_MEDICAL_RECORD_AUTHORSHIP_DEFAULTS_HOTFIX.md`.

### 5.5 Fase 6.3.4 — Implementação de AuditLog clínico mínimo

A Fase 6.3.4 adicionou `ClinicalAuditLog`, writer EF e escrita interna dos eventos `MedicalRecord.Created` e `MedicalRecord.Updated`.

Documento relacionado:

- `docs/clinical-core/PHASE_06_03_04_MEDICAL_RECORD_CLINICAL_AUDIT_LOG_IMPLEMENTATION.md`.

## 6. Evidências de autoria clínica

### 6.1 Campos de autoria no domínio

`MedicalRecord` possui os campos de autoria mínimos:

- `CreatedByUserId`;
- `CreatedAt`;
- `UpdatedByUserId`;
- `UpdatedAt`.

A criação valida `createdByUserId` e `createdAt`, normaliza os campos clínicos opcionais, atribui autoria de criação e inicializa autoria de atualização com os mesmos valores de criação.

A atualização valida `updatedByUserId` e `updatedAt`, atualiza apenas notas/flags e autoria de última modificação, sem alterar `CreatedByUserId` e `CreatedAt`.

Arquivos de evidência:

- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`.

### 6.2 Autoria na criação

O fluxo `CreateMedicalRecordUseCase` resolve o usuário atual por `ICurrentUserService`, cria o prontuário com `currentUser.UserId` e `DateTime.UtcNow`, persiste o registro e em seguida escreve o evento `MedicalRecord.Created`.

Evidências:

- criação preenche `CreatedByUserId`;
- criação preenche `CreatedAt`;
- criação preenche `UpdatedByUserId` com o mesmo usuário;
- criação preenche `UpdatedAt` com o mesmo instante inicial do domínio;
- falha de validação antes da criação não escreve AuditLog.

Arquivos de evidência:

- `backend/src/Togo.Application/MedicalRecords/UseCases/CreateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/CreateMedicalRecordUseCaseTests.cs`.

### 6.3 Autoria na atualização

O fluxo `UpdateMedicalRecordUseCase` valida paciente e prontuário, carrega o `MedicalRecord`, resolve o usuário atual, chama `UpdateNotes` com `currentUser.UserId` e `DateTime.UtcNow`, persiste a alteração e escreve o evento `MedicalRecord.Updated`.

Evidências:

- atualização preserva `CreatedByUserId`;
- atualização preserva `CreatedAt`;
- atualização troca `UpdatedByUserId` para o usuário da modificação;
- atualização troca `UpdatedAt` para o instante da modificação;
- falha de validação antes da atualização não escreve AuditLog.

Arquivos de evidência:

- `backend/src/Togo.Application/MedicalRecords/UseCases/UpdateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/UpdateMedicalRecordUseCaseTests.cs`.

### 6.4 Usuário atual estável e ausência de autoria fake

`HttpContextCurrentUserService` obtém o identificador do usuário autenticado a partir de `ClaimTypes.NameIdentifier` ou `sub`, valida que é `Guid` não vazio e retorna o perfil a partir da claim `togo:profile` quando disponível.

Não há fallback de produção para GUID fixo, usuário fake ou autoria hardcoded nos fluxos de criação/atualização. Os fakes existentes são restritos a projetos de teste.

Arquivos de evidência:

- `backend/src/Togo.Application/Security/ICurrentUserService.cs`;
- `backend/src/Togo.Api/Security/HttpContextCurrentUserService.cs`;
- `backend/src/Togo.Application.Tests/Security/Fakes/FakeCurrentUserService.cs`.

## 7. Evidências de AuditLog clínico

### 7.1 Entidade e tabela

A entidade `ClinicalAuditLog` possui campos mínimos persistidos:

- `Id`;
- `EntityName`;
- `EntityId`;
- `Action`;
- `UserId`;
- `UserProfile` opcional;
- `OccurredAt`;
- `MetadataJson` opcional.

A configuração EF mapeia a entidade para a tabela `ClinicalAuditLogs` e define índices simples por entidade, ação, instante e usuário.

Arquivos de evidência:

- `backend/src/Togo.Domain/Entities/ClinicalAuditLog.cs`;
- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalAuditLogConfiguration.cs`.

### 7.2 Eventos registrados

Os fluxos de escrita de `MedicalRecord` registram os dois eventos mínimos previstos:

- `MedicalRecord.Created` no fluxo de criação;
- `MedicalRecord.Updated` no fluxo de atualização.

Cada evento usa:

- `EntityName = nameof(MedicalRecord)`;
- `EntityId = medicalRecord.Id.ToString()`;
- `Action` com constante de `MedicalRecordAuditActions`;
- `UserId = currentUser.UserId`;
- `UserProfile = currentUser.Profile` quando disponível;
- `OccurredAt = DateTime.UtcNow`;
- `MetadataJson` mínimo com `PatientId`.

Arquivos de evidência:

- `backend/src/Togo.Application/Auditing/ClinicalAuditEvent.cs`;
- `backend/src/Togo.Application/Auditing/MedicalRecordAuditActions.cs`;
- `backend/src/Togo.Application/Auditing/IClinicalAuditLogWriter.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/CreateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/UpdateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Infrastructure/Auditing/EfClinicalAuditLogWriter.cs`.

## 8. Evidências de minimização de dados

A decisão de minimização é explícita: o AuditLog clínico registra contexto operacional mínimo e não armazena payload clínico sensível.

Evidências técnicas:

- `CreateMetadataJson` serializa somente `{ PatientId = patientId }` nos fluxos de criação e atualização;
- testes de criação garantem que o `MetadataJson` não contém o conteúdo de `GeneralNotes`, não contém o conteúdo de `FlagsJson` e também não contém os nomes `GeneralNotes`/`FlagsJson`;
- testes de atualização garantem que o `MetadataJson` não contém o conteúdo de `GeneralNotes`, não contém o conteúdo de `FlagsJson` e também não contém os nomes `GeneralNotes`/`FlagsJson`;
- teste de persistência EF confirma que o writer persiste `MetadataJson` sem `GeneralNotes` e sem `FlagsJson` quando recebe metadados mínimos;
- logs técnicos de aplicação continuam sem payload clínico sensível.

Arquivos de evidência:

- `backend/src/Togo.Application/MedicalRecords/UseCases/CreateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/UpdateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/CreateMedicalRecordUseCaseTests.cs`;
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/UpdateMedicalRecordUseCaseTests.cs`;
- `backend/src/Togo.Infrastructure.Tests/Auditing/EfClinicalAuditLogWriterTests.cs`.

## 9. Evidências de ausência de endpoint público

Não foi criado controller, rota pública ou endpoint para consultar AuditLog.

Evidências de revisão:

- `backend/src/Togo.Api/Controllers` contém controllers de `Attendances`, `Auth`, `MedicalRecords`, `Pets`, `Tutors` e `User`, sem controller de AuditLog;
- busca por termos de auditoria no projeto API retornou apenas registros de DI (`IClinicalAuditLogWriter`/`EfClinicalAuditLogWriter`) e não rotas públicas;
- o writer de AuditLog é registrado para uso interno da Application, não exposto por controller.

Comandos de revisão usados nesta fase:

- `find backend/src/Togo.Api -type f | sort`;
- `rg -n "Audit|AuditLog|ClinicalAudit|clinical-audit|audit-log" backend/src/Togo.Api frontend/togo-frontend/src -S`.

## 10. Evidências de testes

### 10.1 Testes já existentes revisados

Foram revisados testes de domínio, aplicação, API e infraestrutura relacionados a autoria e AuditLog:

- domínio de `MedicalRecord` cobre criação, validação de usuário, datas e preservação da autoria original em atualização;
- use case de criação cobre autoria, evento `MedicalRecord.Created`, `UserId`, `UserProfile`, `OccurredAt` UTC, metadata mínima e falhas sem escrita de AuditLog;
- use case de atualização cobre preservação de autoria original, troca da última autoria, evento `MedicalRecord.Updated`, `UserId`, `UserProfile`, `OccurredAt` UTC, metadata mínima e falhas sem escrita de AuditLog;
- testes de `HttpContextCurrentUserService` cobrem resolução do usuário autenticado por identificador estável;
- testes de infraestrutura cobrem persistência de `ClinicalAuditLog` via EF.

### 10.2 Lacunas complementares tratadas nesta fase

Foram adicionadas validações pontuais, sem criar feature nova:

- `MedicalRecord.Created` agora também tem asserção explícita de que `MetadataJson` não contém os nomes dos campos `GeneralNotes` e `FlagsJson`;
- `MedicalRecord.Updated` agora também tem asserção explícita de que `MetadataJson` não contém os nomes dos campos `GeneralNotes` e `FlagsJson`;
- persistência de `ClinicalAuditLog` agora cobre o caso em que `UserProfile` e `MetadataJson` são nulos.

Esses testes complementam evidências reais solicitadas pela fase e evitam ampliar o escopo funcional.

## 11. Evidências de migrations

Migrations relevantes revisadas:

- `20260601120000_AddMedicalRecordAuthorship` adiciona as colunas de autoria de `MedicalRecord`;
- `20260603120000_DropMedicalRecordAuthorshipDefaults` remove defaults persistentes das colunas de autoria;
- `20260604120000_AddClinicalAuditLogs` cria a tabela `ClinicalAuditLogs` e índices mínimos.

Arquivos de evidência:

- `backend/src/Togo.Infrastructure/Migrations/20260601120000_AddMedicalRecordAuthorship.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260603120000_DropMedicalRecordAuthorshipDefaults.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260604120000_AddClinicalAuditLogs.cs`;
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`.

## 12. Arquivos relevantes

### Documentação

- `docs/clinical-core/PHASE_06_03_01_MEDICAL_RECORD_AUTHORSHIP_AUDIT_PLANNING.md`;
- `docs/clinical-core/PHASE_06_03_02_CURRENT_USER_AND_AUDIT_CONTRACTS.md`;
- `docs/clinical-core/PHASE_06_03_03_MEDICAL_RECORD_AUTHORSHIP_IMPLEMENTATION.md`;
- `docs/clinical-core/PHASE_06_03_03_01_MEDICAL_RECORD_AUTHORSHIP_DEFAULTS_HOTFIX.md`;
- `docs/clinical-core/PHASE_06_03_04_MEDICAL_RECORD_CLINICAL_AUDIT_LOG_IMPLEMENTATION.md`;
- `docs/clinical-core/PHASE_06_01_01_MEDICAL_RECORD_TECH_DEBT_REGISTER.md`.

### Código de domínio, aplicação, API e infraestrutura

- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`;
- `backend/src/Togo.Domain/Entities/ClinicalAuditLog.cs`;
- `backend/src/Togo.Application/Security/ICurrentUserService.cs`;
- `backend/src/Togo.Application/Auditing/IClinicalAuditLogWriter.cs`;
- `backend/src/Togo.Application/Auditing/ClinicalAuditEvent.cs`;
- `backend/src/Togo.Application/Auditing/MedicalRecordAuditActions.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/CreateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/UpdateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Api/Security/HttpContextCurrentUserService.cs`;
- `backend/src/Togo.Infrastructure/Auditing/EfClinicalAuditLogWriter.cs`;
- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalAuditLogConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Migrations`.

### Testes

- `backend/src/Togo.Domain.Tests/MedicalRecordTests.cs`;
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/CreateMedicalRecordUseCaseTests.cs`;
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/UpdateMedicalRecordUseCaseTests.cs`;
- `backend/src/Togo.Api.Tests/Security/HttpContextCurrentUserServiceTests.cs`;
- `backend/src/Togo.Infrastructure.Tests/Auditing/EfClinicalAuditLogWriterTests.cs`.

## 13. O que foi considerado resolvido

### 13.1 MR-DEBT-004

**Resolvido tecnicamente no escopo incremental da Fase 6.3.3.**

Critérios atendidos:

- autoria de criação persistida;
- data de criação persistida;
- autoria de última atualização persistida;
- data de última atualização persistida;
- criação e atualização usam usuário autenticado resolvido pelo contrato `ICurrentUserService`;
- atualização preserva autoria original de criação;
- não há default persistente final que injete autoria artificial em novos registros.

### 13.2 MR-DEBT-002

**Resolvido tecnicamente no escopo incremental da Fase 6.3.4.**

Critérios atendidos:

- tabela persistente de AuditLog criada;
- eventos mínimos de criação e atualização registrados;
- usuário, perfil quando disponível, entidade, identificador, ação e instante UTC registrados;
- metadata restrita a metadados mínimos;
- ausência de payload clínico sensível no AuditLog;
- ausência de endpoint público e tela frontend de auditoria.

## 14. O que continua fora do escopo

Permanecem explicitamente fora do escopo desta fase e não devem ser interpretados como resolvidos:

- auditoria de leitura de prontuário;
- auditoria de acesso negado;
- endpoint público de consulta de AuditLog;
- tela frontend de AuditLog;
- transação única atômica entre operação principal e AuditLog;
- política de retenção/expurgo;
- soft delete;
- revisão de `DeleteBehavior.Cascade`;
- validação estrutural de `FlagsJson`;
- índices físicos adicionais além dos já criados para AuditLog;
- mecanismos externos como Docker, Redis, RabbitMQ ou Kubernetes.

## 15. Riscos remanescentes

Riscos conhecidos após o encerramento da Fase 6.3:

- se a escrita do AuditLog falhar depois da operação principal, não há transação única garantindo atomicidade total entre `MedicalRecord` e `ClinicalAuditLog`;
- leituras clínicas ainda não geram trilha de auditoria;
- tentativas negadas por autorização ainda não geram trilha de auditoria clínica;
- não há endpoint operacional para consulta controlada de AuditLog por administradores;
- não há política implementada de retenção, expurgo ou arquivamento de logs;
- MedicalRecord ainda permanece bloqueado para produção real enquanto outros P1 da Fase 6.4 estiverem abertos, especialmente Soft Delete, retenção e revisão de cascade.

Esses riscos não foram resolvidos nesta fase por decisão explícita de escopo. Eles podem ser convertidos em débitos futuros, se necessário.

## 16. Recomendações futuras

Recomendações para fases posteriores:

- avaliar auditoria de leitura de prontuário com política clara de volume, privacidade e retenção;
- avaliar auditoria de acesso negado em camada de autorização/API;
- definir consulta administrativa segura para AuditLog, com paginação, filtros, autorização forte e minimização de dados;
- avaliar transação única ou padrão outbox para operação clínica + AuditLog;
- definir política de retenção/expurgo para dados clínicos e trilhas de auditoria;
- concluir débitos P1 da Fase 6.4 antes de recomendar produção real com dados clínicos sensíveis.

## 17. Critérios finais de aceite

| Critério | Status | Evidência |
|---|---|---|
| `MedicalRecord` possui `CreatedByUserId` | Atendido | Entidade e configuração EF |
| `MedicalRecord` possui `CreatedAt` | Atendido | Entidade e configuração EF |
| `MedicalRecord` possui `UpdatedByUserId` | Atendido | Entidade e configuração EF |
| `MedicalRecord` possui `UpdatedAt` | Atendido | Entidade e configuração EF |
| Criação preenche autoria de criação e atualização | Atendido | Use case e testes de criação |
| Atualização preserva autoria original de criação | Atendido | Domínio/use case e testes de atualização |
| Atualização troca apenas autoria da última modificação | Atendido | Domínio/use case e testes de atualização |
| `CurrentUserService` usa identificador estável do usuário autenticado | Atendido | `NameIdentifier`/`sub` validados como `Guid` |
| Não existe autoria fake/hardcoded em produção | Atendido | Fakes restritos aos testes |
| `ClinicalAuditLogs` existe como tabela de auditoria | Atendido | DbSet, configuração EF e migration |
| `MedicalRecord.Created` é registrado | Atendido | Use case e testes de criação |
| `MedicalRecord.Updated` é registrado | Atendido | Use case e testes de atualização |
| AuditLog registra `UserId` | Atendido | Entidade, evento, writer e testes |
| AuditLog registra `UserProfile` quando disponível | Atendido | Evento e testes de criação/atualização |
| `UserProfile` pode ser nulo | Atendido | Teste complementar de persistência EF |
| AuditLog registra `EntityName` | Atendido | Evento, writer e testes |
| AuditLog registra `EntityId` | Atendido | Evento, writer e testes |
| AuditLog registra `Action` | Atendido | Constantes, evento, writer e testes |
| AuditLog registra `OccurredAt` em UTC | Atendido | Use cases, entidade e testes |
| `MetadataJson` contém apenas metadados mínimos | Atendido | `PatientId` somente |
| `MetadataJson` não contém `GeneralNotes` completo | Atendido | Use cases e testes |
| `MetadataJson` não contém `FlagsJson` completo | Atendido | Use cases e testes |
| Não há endpoint público de AuditLog | Atendido | Revisão de controllers e busca por rotas |
| Não há tela frontend de AuditLog | Atendido | Busca em `frontend/togo-frontend/src` |
| Auditoria de leitura permanece fora do escopo | Confirmado | Documentado como risco remanescente |
| Auditoria de acesso negado permanece fora do escopo | Confirmado | Documentado como risco remanescente |

## 18. Decisão final da Fase 6.3

**Decisão:** Fase 6.3 encerrada com **MR-DEBT-004** e **MR-DEBT-002** tratados tecnicamente no escopo incremental planejado.

A vertical `MedicalRecord` agora possui autoria clínica mínima e AuditLog clínico mínimo para criação e atualização, sem payload clínico sensível e sem exposição pública de logs. A decisão não libera automaticamente produção real com dados clínicos sensíveis, pois outros débitos P1 continuam fora do escopo desta fase e devem ser endereçados nas fases posteriores de hardening.
