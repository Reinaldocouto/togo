# Fase 6.3.3 — Implementação de autoria clínica mínima em MedicalRecord

## 1. Contexto da Fase 6.3

A Fase 6.3 trata incrementalmente dois débitos P1 da vertical clínica `MedicalRecord`:

- **MR-DEBT-004 — Controle de autoria ausente**;
- **MR-DEBT-002 — AuditLog ausente**.

Esta subfase trata diretamente **MR-DEBT-004**. O escopo permanece deliberadamente limitado à autoria mínima persistida no prontuário, sem antecipar a implementação de AuditLog.

## 2. Relação com as Fases 6.3.1 e 6.3.2

A Fase 6.3.1 (`PHASE_06_03_01_MEDICAL_RECORD_AUTHORSHIP_AUDIT_PLANNING.md`) definiu o sequenciamento: resolver usuário autenticado, persistir autoria mínima e somente depois implementar AuditLog clínico.

A Fase 6.3.2 (`PHASE_06_03_02_CURRENT_USER_AND_AUDIT_CONTRACTS.md`) criou `CurrentUserInfo`, `ICurrentUserService`, `CurrentUserResolutionException` e a implementação HTTP `HttpContextCurrentUserService`. A presente fase consome essa base técnica nos casos de uso de criação e atualização.

## 3. Objetivo

Persistir quem criou o prontuário e quem realizou sua última alteração, com timestamps UTC claros, usando exclusivamente o identificador estável do usuário autenticado resolvido por `ICurrentUserService`.

## 4. Decisão técnica adotada

Foi implementada a **Opção A — Autoria clínica mínima em MedicalRecord implementada com usuário autenticado atual, tratando tecnicamente MR-DEBT-004 sem antecipar AuditLog**.

A regra foi aplicada na camada Application, pois o projeto já possui casos de uso próprios para criação e atualização de `MedicalRecord`. Não foi criada arquitetura paralela e o controller permaneceu responsável apenas pela adaptação HTTP.

## 5. Campos adicionados

A entidade `MedicalRecord` agora persiste:

| Campo | Tipo | Obrigatório | Semântica |
|---|---|---|---|
| `CreatedByUserId` | `Guid` | Sim | Identificador estável do usuário autenticado que criou o prontuário. |
| `CreatedAt` | `DateTime` | Sim | Instante UTC de criação do prontuário. |
| `UpdatedByUserId` | `Guid` | Sim | Identificador estável do usuário autenticado responsável pela última alteração. |
| `UpdatedAt` | `DateTime` | Sim | Instante UTC da criação inicial ou da última alteração. |

Não foram adicionados nome, e-mail, profile ou quaisquer outros dados pessoais como autoria.

## 6. Estratégia de resolução do usuário atual

`CreateMedicalRecordUseCase` e `UpdateMedicalRecordUseCase` recebem `ICurrentUserService` por injeção de dependência e chamam `GetCurrentUser()` imediatamente antes da mutação persistida.

- Na criação, `CreatedByUserId` e `UpdatedByUserId` recebem o mesmo `currentUser.UserId`; `CreatedAt` e `UpdatedAt` recebem o mesmo `DateTime.UtcNow`.
- Na atualização, somente `UpdatedByUserId` e `UpdatedAt` são alterados; `CreatedByUserId` e `CreatedAt` permanecem preservados.
- Se a identidade atual não puder ser resolvida, `CurrentUserResolutionException` interrompe o fluxo antes da persistência. A falha é fechada e não há autoria fake, hardcoded ou vazia para novas gravações.
- A entidade rejeita `Guid.Empty` tanto na criação quanto na atualização.

## 7. Impacto em domínio e Application

A factory `MedicalRecord.Create` passou a exigir autoria de criação e instante de criação. O método `UpdateNotes` passou a exigir autoria da atualização e instante da alteração.

Os casos de uso continuam usando `DateTime.UtcNow`. A resposta HTTP existente não foi ampliada com dados de autoria nesta fase; a autoria é persistida internamente para rastreabilidade clínica mínima sem ampliar exposição de contrato público.

## 8. Impacto em EF Core e migration

`MedicalRecordConfiguration` marca os quatro campos como obrigatórios. A migration `AddMedicalRecordAuthorship` adiciona as três colunas que ainda não existiam fisicamente: `CreatedByUserId`, `CreatedAt` e `UpdatedByUserId`. A coluna `UpdatedAt` existente é preservada e sua semântica passa a ser explicitamente a data da última alteração.

### 8.1 Registros históricos

O banco pode conter prontuários criados antes desta fase. Como sua autoria real não foi registrada, a migration não inventa usuários históricos:

1. adiciona `CreatedAt` de forma migrável e copia o valor histórico disponível em `UpdatedAt`;
2. adiciona `CreatedByUserId` e `UpdatedByUserId` obrigatórios com `Guid.Empty` como **sentinela explícita apenas para registros legados**;
3. exige saneamento posterior dos registros legados quando uma fonte confiável de autoria histórica existir.

Essa limitação é intencional e documentada. Novos fluxos de domínio recusam `Guid.Empty`, portanto a sentinela não é aceita como autoria válida para novas criações ou atualizações.

## 9. Arquivos alterados e criados

### Domínio e Application

- `backend/src/Togo.Domain/Entities/MedicalRecord.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/CreateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/UpdateMedicalRecordUseCase.cs`.

### API e resolução de usuário

- `backend/src/Togo.Api/Security/HttpContextCurrentUserService.cs`;
- `backend/src/Togo.Api.Tests/Security/HttpContextCurrentUserServiceTests.cs`.

A implementação HTTP também passou a rejeitar `Guid.Empty` como identificador autenticado inválido.

### Persistência

- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260601120000_AddMedicalRecordAuthorship.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260601120000_AddMedicalRecordAuthorship.Designer.cs`;
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`.

### Testes

- testes de domínio de `MedicalRecord`;
- testes de aplicação dos casos de uso e validators;
- fake explícito de `ICurrentUserService` para testes de aplicação;
- testes API de `MedicalRecordsController`;
- testes de infraestrutura de `MedicalRecordRepository`.

## 10. Impacto em testes

A cobertura foi ajustada para verificar:

- criação com `CreatedByUserId` e `UpdatedByUserId` iguais ao usuário autenticado atual;
- criação com `CreatedAt` e `UpdatedAt` preenchidos com o mesmo instante inicial;
- atualização com troca de `UpdatedByUserId` e avanço de `UpdatedAt`;
- preservação de `CreatedByUserId` e `CreatedAt` na atualização;
- rejeição de `Guid.Empty` no domínio;
- falha segura de criação e atualização quando `ICurrentUserService` não resolve o usuário;
- persistência EF dos campos mínimos;
- manutenção das regras existentes de autorização API.

Os fakes de teste usam GUIDs explícitos e não vazios. Nenhum teste afrouxa autorização.

## 11. Riscos remanescentes

- Registros históricos permanecem sinalizados com `Guid.Empty` até saneamento confiável; esse valor não representa autoria real.
- `CreatedAt` histórico usa o melhor timestamp já disponível (`UpdatedAt`), podendo representar a última alteração anterior à migration, e não a criação original.
- **MR-DEBT-002 — AuditLog ausente** continua aberto.
- Não há nesta fase trilha imutável de eventos clínicos; apenas autoria mínima no estado corrente do prontuário.

## 12. Confirmação de escopo

Esta fase **não** implementa `AuditLog`, tabela de auditoria, writer de auditoria, eventos persistidos `MedicalRecord.Created` ou `MedicalRecord.Updated`, endpoint de auditoria, auditoria de leitura, auditoria de acesso negado, soft delete, retenção ou revisão de cascatas.

## 13. Critérios de aceite

- [x] `MedicalRecord` persiste `CreatedByUserId`, `CreatedAt`, `UpdatedByUserId` e `UpdatedAt`.
- [x] Criação e atualização usam o usuário autenticado atual via `ICurrentUserService`.
- [x] Criação e atualização falham de forma segura sem usuário resolvido.
- [x] Atualização preserva `CreatedByUserId` e `CreatedAt`.
- [x] Não há autoria fake, hardcoded ou vazia aceita em novos fluxos.
- [x] Não há armazenamento desnecessário de nome ou e-mail.
- [x] Migration mínima foi criada com limitação histórica documentada.
- [x] Testes foram ajustados para criação, atualização, persistência e falha segura.
- [x] AuditLog não foi implementado nesta fase.

## 14. Fora do escopo

Permanece fora do escopo tudo que pertence a AuditLog clínico completo, auditoria de leitura ou negação, retenção, soft delete, revisão de `DeleteBehavior.Cascade`, frontend e infraestrutura externa como Docker, Redis, RabbitMQ ou Kubernetes.

## 15. Próxima fase recomendada

**Fase 6.3.4 — Implementação mínima de AuditLog clínico.**

Objetivo recomendado: implementar persistência mínima dos eventos `MedicalRecord.Created` e `MedicalRecord.Updated`, sem payload clínico sensível, tratando **MR-DEBT-002** incrementalmente.
