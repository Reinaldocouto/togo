# Fase 6.3.4 — Implementação mínima de AuditLog clínico em MedicalRecord

## 1. Contexto da Fase 6.3

A Fase 6.3 trata os débitos P1 de rastreabilidade clínica da vertical `MedicalRecord`:

- **MR-DEBT-004 — Controle de autoria ausente**;
- **MR-DEBT-002 — AuditLog ausente**.

Após a implementação de autoria mínima e o hotfix de defaults persistentes, o débito **MR-DEBT-004** está tecnicamente tratado. Esta fase atua sobre o débito ainda aberto, **MR-DEBT-002**, por meio de um mecanismo mínimo e incremental de AuditLog clínico.

## 2. Referência explícita ao MR-DEBT-002

O débito **MR-DEBT-002 — AuditLog ausente** indicava a inexistência de trilha persistida para eventos clínicos relevantes de `MedicalRecord`.

A Fase 6.3.4 trata tecnicamente esse débito de forma inicial, registrando eventos de criação e atualização do prontuário sem armazenar conteúdo clínico sensível.

## 3. Relação com as Fases 6.3.1, 6.3.2, 6.3.3 e 6.3.3.1

- A **Fase 6.3.1** planejou a sequência de rastreabilidade clínica: resolver usuário atual, persistir autoria mínima e só então implementar AuditLog.
- A **Fase 6.3.2** criou os contratos de usuário atual (`CurrentUserInfo` e `ICurrentUserService`) e as constantes de ações de auditoria em `MedicalRecordAuditActions`.
- A **Fase 6.3.3** implementou autoria mínima em `MedicalRecord` com `CreatedByUserId`, `CreatedAt`, `UpdatedByUserId` e `UpdatedAt`.
- A **Fase 6.3.3.1** removeu defaults persistentes das colunas de autoria, garantindo que novas gravações dependam da aplicação e do usuário autenticado atual.

Esta fase consome essa base para escrever eventos clínicos mínimos associados ao usuário autenticado atual.

## 4. Objetivo da fase

Implementar a **Opção A — AuditLog clínico mínimo para criação e atualização de MedicalRecord, sem payload clínico sensível**, registrando inicialmente:

- `MedicalRecord.Created`;
- `MedicalRecord.Updated`.

## 5. Decisão técnica adotada

Foi criada uma entidade persistente `ClinicalAuditLog` no domínio, um contrato de escrita `IClinicalAuditLogWriter` na camada Application e uma implementação EF Core `EfClinicalAuditLogWriter` na Infrastructure.

A decisão mantém a auditoria como escrita interna da aplicação, sem endpoint público, tela frontend ou exposição de consulta de logs nesta fase. A tabela não possui foreign key obrigatória para usuário, preservando eventos históricos mesmo se dados de usuário forem alterados futuramente.

## 6. Arquivos criados/alterados

### Domínio

- `backend/src/Togo.Domain/Entities/ClinicalAuditLog.cs`.

### Application

- `backend/src/Togo.Application/Auditing/ClinicalAuditEvent.cs`;
- `backend/src/Togo.Application/Auditing/IClinicalAuditLogWriter.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/CreateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/UpdateMedicalRecordUseCase.cs`.

### Infrastructure

- `backend/src/Togo.Infrastructure/Auditing/EfClinicalAuditLogWriter.cs`;
- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalAuditLogConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260604120000_AddClinicalAuditLogs.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260604120000_AddClinicalAuditLogs.Designer.cs`;
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`.

### API/DI

- `backend/src/Togo.Api/Program.cs`.

### Testes

- `backend/src/Togo.Application.Tests/MedicalRecords/Fakes/FakeClinicalAuditLogWriter.cs`;
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/CreateMedicalRecordUseCaseTests.cs`;
- `backend/src/Togo.Application.Tests/MedicalRecords/UseCases/UpdateMedicalRecordUseCaseTests.cs`;
- `backend/src/Togo.Infrastructure.Tests/Auditing/EfClinicalAuditLogWriterTests.cs`;
- `backend/src/Togo.Api.Tests/MedicalRecords/MedicalRecordsControllerTests.cs`.

## 7. Entidade/tabela de auditoria criada

A entidade `ClinicalAuditLog` possui os campos mínimos:

| Campo | Tipo | Obrigatório | Semântica |
|---|---|---|---|
| `Id` | `long` | Sim | Identificador técnico do evento de auditoria. |
| `EntityName` | `string` | Sim | Nome da entidade auditada, inicialmente `MedicalRecord`. |
| `EntityId` | `string` | Sim | Identificador da entidade auditada convertido para string. |
| `Action` | `string` | Sim | Ação de auditoria, por exemplo `MedicalRecord.Created`. |
| `UserId` | `Guid` | Sim | Usuário autenticado atual responsável pela operação. |
| `UserProfile` | `string?` | Não | Perfil resolvido no contexto atual, quando disponível. |
| `OccurredAt` | `DateTime` | Sim | Instante UTC do evento de auditoria. |
| `MetadataJson` | `string?` | Não | Metadados mínimos sem conteúdo clínico sensível. |

A tabela criada é `ClinicalAuditLogs`, sem relacionamentos complexos nesta fase.

## 8. Eventos implementados

Foram implementados os eventos:

- `MedicalRecord.Created`: registrado após persistência bem-sucedida da criação do prontuário;
- `MedicalRecord.Updated`: registrado após persistência bem-sucedida da atualização do prontuário.

Ambos usam:

- `EntityName = "MedicalRecord"`;
- `EntityId = medicalRecord.Id.ToString()`;
- `UserId = currentUser.UserId`;
- `UserProfile = currentUser.Profile`;
- `OccurredAt = DateTime.UtcNow`.

## 9. Política de minimização de dados

`MetadataJson` contém apenas metadado mínimo de identificação operacional, atualmente `PatientId`.

Não são gravados:

- `GeneralNotes` completo;
- `FlagsJson` completo;
- request body;
- response body;
- conteúdo anterior e novo do prontuário;
- notas clínicas completas;
- flags clínicas completas.

## 10. Confirmação de ausência de payload clínico sensível

A implementação de auditoria não copia conteúdo clínico de `GeneralNotes` ou `FlagsJson` para `ClinicalAuditLog`.

Os testes de Application validam que os metadados dos eventos de criação e atualização não contêm as notas clínicas nem o JSON de flags enviados na requisição.

## 11. Impacto em use cases

`CreateMedicalRecordUseCase` passou a receber `IClinicalAuditLogWriter` por injeção de dependência e escreve `MedicalRecord.Created` somente depois de `AddAsync` concluir com sucesso.

`UpdateMedicalRecordUseCase` passou a receber `IClinicalAuditLogWriter` por injeção de dependência e escreve `MedicalRecord.Updated` somente depois de `UpdateAsync` concluir com sucesso.

Falhas de validação não disparam auditoria. Falhas de resolução do usuário atual continuam interrompendo o fluxo antes da persistência principal.

## 12. Impacto em banco/migration

A migration `AddClinicalAuditLogs` cria a tabela `ClinicalAuditLogs` e índices simples para consulta operacional futura por entidade, ação, usuário e instante.

Não foram criadas foreign keys obrigatórias para `Users` nesta fase, pois o AuditLog deve preservar o histórico mesmo diante de alteração ou remoção futura de usuários.

## 13. Testes criados/alterados

A cobertura foi ampliada para validar:

- criação de `MedicalRecord` gera evento `MedicalRecord.Created`;
- atualização de `MedicalRecord` gera evento `MedicalRecord.Updated`;
- `EntityName`, `EntityId`, `UserId`, `UserProfile` e `OccurredAt` UTC são preenchidos corretamente;
- `MetadataJson` contém apenas metadados mínimos;
- `MetadataJson` não contém `GeneralNotes` completo;
- `MetadataJson` não contém `FlagsJson` completo;
- falhas de validação na criação não escrevem AuditLog;
- falhas de validação na atualização não escrevem AuditLog;
- o writer EF persiste `ClinicalAuditLog` em banco de teste.

## 14. O que ainda não foi implementado

Ainda não foram implementados:

- auditoria de leitura;
- auditoria de acesso negado;
- endpoint público de consulta de AuditLog;
- tela frontend de auditoria;
- retenção/expurgo de logs;
- soft delete;
- revisão de `DeleteBehavior.Cascade`;
- auditoria de outras entidades clínicas;
- garantias transacionais distribuídas entre operação principal e auditoria.

## 15. Riscos remanescentes

- A operação principal e a escrita de auditoria ainda ocorrem em chamadas de persistência separadas, seguindo o desenho atual dos repositories. Se a escrita de auditoria falhar após a persistência principal, será necessário avaliar estratégia transacional em fase futura.
- A tabela é conceitualmente append-only, mas ainda não há bloqueios técnicos contra alterações diretas por administradores de banco.
- Ainda não há política de retenção ou consulta controlada dos eventos.
- Auditoria de leitura e acesso negado continuam fora do escopo desta etapa.

## 16. Critérios de aceite

A fase atende aos critérios quando:

- existe mecanismo mínimo de AuditLog clínico;
- eventos `MedicalRecord.Created` são registrados;
- eventos `MedicalRecord.Updated` são registrados;
- eventos são associados ao usuário autenticado atual;
- eventos são associados à entidade `MedicalRecord`;
- AuditLog não armazena `GeneralNotes` completo;
- AuditLog não armazena `FlagsJson` completo;
- AuditLog não armazena payload de request/response;
- não há endpoint público de auditoria;
- tabela/migration de auditoria existe;
- testes cobrem criação e atualização auditadas;
- testes cobrem ausência de payload clínico sensível;
- testes existentes de autoria continuam preservados;
- documentação da fase existe;
- build e test suite devem passar em ambiente com SDK .NET disponível.

## 17. Fora do escopo

Permaneceu fora do escopo:

- leitura auditada;
- acesso negado auditado;
- autorização granular nova;
- JWT;
- `User/Profile`;
- frontend;
- endpoint público de AuditLog;
- Docker, Redis, RabbitMQ ou Kubernetes;
- soft delete;
- retenção;
- revisão de cascades.

## 18. Próxima fase recomendada

A próxima fase recomendada é a **Fase 6.3.5 — Testes finais e evidências de autoria/auditoria MedicalRecord**.

Seu objetivo deve ser consolidar evidências finais de autoria clínica e AuditLog, cobrindo **MR-DEBT-004** e **MR-DEBT-002** antes de atualizar o registro vivo e encerrar formalmente a Fase 6.3.
