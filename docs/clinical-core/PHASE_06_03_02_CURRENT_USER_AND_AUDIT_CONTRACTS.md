# Fase 6.3.2 — Contratos de usuário atual e ações de auditoria clínica

## 1. Contexto da Fase 6.3

A Fase 6.3 prepara a evolução incremental da vertical `MedicalRecord` para autoria clínica e auditoria. Os débitos P1 envolvidos continuam sendo:

- **MR-DEBT-004 — Controle de autoria ausente**;
- **MR-DEBT-002 — AuditLog ausente**.

A Fase 6.2 encerrou o tratamento técnico de **MR-DEBT-003** com autorização granular mínima baseada em profile JWT e policies ASP.NET Core. A presente subfase não reabre autorização e não declara MR-DEBT-004 ou MR-DEBT-002 resolvidos.

## 2. Relação com a Fase 6.3.1

A Fase 6.3.1, documentada em `PHASE_06_03_01_MEDICAL_RECORD_AUTHORSHIP_AUDIT_PLANNING.md`, definiu o sequenciamento necessário para evitar improvisação: primeiro uma abstração mínima do usuário autenticado, depois autoria clínica e somente então persistência mínima de AuditLog.

A Fase 6.3.2 materializa apenas a primeira base técnica desse planejamento. Ela mantém o domínio e os casos de uso de `MedicalRecord` inalterados.

## 3. Objetivo

Criar contratos e abstrações mínimas para capturar o usuário autenticado atual por identificador estável e preparar o vocabulário de ações futuras de auditoria clínica, sem acoplar Application ao `HttpContext`, sem persistir autoria e sem criar AuditLog.

## 4. Decisão técnica adotada

Foi aprovada a **Opção A — Contratos e abstrações mínimas de usuário atual/auditoria aprovados como base técnica para implementação futura de autoria clínica e AuditLog**.

A implementação adota uma API simples e testável:

```csharp
CurrentUserInfo GetCurrentUser();
```

Não foi usado `ApplicationResult<T>` porque a resolução do contexto autenticado é uma pré-condição técnica de segurança, não um resultado esperado de regra de negócio. O serviço falha de forma fechada por meio de `CurrentUserResolutionException` quando a identidade não puder ser resolvida com segurança.

## 5. Arquivos criados e alterados

### Criados

- `backend/src/Togo.Application/Security/CurrentUserInfo.cs`;
- `backend/src/Togo.Application/Security/ICurrentUserService.cs`;
- `backend/src/Togo.Application/Security/CurrentUserResolutionException.cs`;
- `backend/src/Togo.Application/Auditing/MedicalRecordAuditActions.cs`;
- `backend/src/Togo.Api/Security/HttpContextCurrentUserService.cs`;
- `backend/src/Togo.Api.Tests/Security/HttpContextCurrentUserServiceTests.cs`;
- `backend/src/Togo.Api.Tests/Security/MedicalRecordAuditActionsTests.cs`;
- `docs/clinical-core/PHASE_06_03_02_CURRENT_USER_AND_AUDIT_CONTRACTS.md`.

### Alterado

- `backend/src/Togo.Api/Program.cs`.

## 6. Contrato de usuário atual

`CurrentUserInfo` representa somente os dados mínimos necessários para autoria futura:

| Campo | Tipo | Finalidade |
|---|---|---|
| `UserId` | `Guid` | Identificador estável do usuário autenticado. |
| `Profile` | `string?` | Profile disponível para contexto mínimo futuro; não identifica o usuário. |
| `IsAuthenticated` | `bool` | Indica que a resolução ocorreu a partir de uma identidade autenticada. |

O contrato deliberadamente não expõe nome, e-mail ou outros dados pessoais desnecessários.

`ICurrentUserService` vive em Application e não depende de ASP.NET Core nem de `HttpContext`. A leitura concreta da requisição fica na API por meio de `HttpContextCurrentUserService`.

## 7. Estratégia de resolução de claims

`HttpContextCurrentUserService` usa `IHttpContextAccessor` e executa a seguinte sequência:

1. obtém o `ClaimsPrincipal` associado ao `HttpContext` atual;
2. exige `Identity.IsAuthenticated == true`;
3. busca primeiro `ClaimTypes.NameIdentifier`;
4. usa a claim `sub` como fallback somente quando `ClaimTypes.NameIdentifier` não estiver presente;
5. valida que o valor selecionado seja um `Guid` válido;
6. captura `TogoClaimTypes.Profile` quando disponível;
7. retorna `CurrentUserInfo` autenticado.

### 7.1 Decisão sobre `ClaimTypes.NameIdentifier` e `sub`

`ClaimTypes.NameIdentifier` tem precedência para alinhar a abstração ao principal já processado pelo pipeline ASP.NET Core. A claim JWT padrão `sub` é aceita como fallback explícito para manter compatibilidade com principals que preservem o nome original da claim.

Se `ClaimTypes.NameIdentifier` estiver presente mas inválida, a resolução falha. O serviço não troca silenciosamente para `sub`, pois isso poderia mascarar uma identidade inconsistente.

### 7.2 Profile não é identificador

`TogoClaimTypes.Profile` é opcional no contrato de usuário atual. A resolução do usuário não depende de profile, nome ou e-mail. Isso preserva `UserId` como única identidade estável para autoria futura.

## 8. Falha fechada

A abstração não cria usuário fake, não usa identificador hardcoded e não retorna autoria artificial. `CurrentUserResolutionException` é lançada quando:

- não existe `HttpContext`;
- não existe identidade autenticada;
- nenhuma claim de identificador aceita está disponível;
- o identificador selecionado não é um `Guid` válido.

Essa decisão prepara a Fase 6.3.3 para rejeitar mutações clínicas quando não houver usuário autenticado resolvível.

## 9. Preparação de auditoria clínica

Foi criado `MedicalRecordAuditActions` com o vocabulário inicial:

| Constante | Valor |
|---|---|
| `Created` | `MedicalRecord.Created` |
| `Updated` | `MedicalRecord.Updated` |
| `Read` | `MedicalRecord.Read` |
| `AccessDenied` | `MedicalRecord.AccessDenied` |

As constantes não persistem eventos e ainda não são consumidas por controllers ou casos de uso.

Foi deliberadamente adiada a criação de `IClinicalAuditLogWriter`, `ClinicalAuditEvent`, entidade e repositório. Antecipar esses contratos exigiria decisões sobre formato, transação, metadados mínimos e política append-only que pertencem à Fase 6.3.4. Esse adiamento evita desenhar uma abstração de persistência prematuramente.

## 10. Impacto em `Program.cs` e DI

A API registra:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
```

Não foi registrado writer, repositório ou persistência de AuditLog.

## 11. Testes criados

Os testes unitários de `HttpContextCurrentUserService` cobrem:

- resolução por `ClaimTypes.NameIdentifier`;
- precedência de `ClaimTypes.NameIdentifier` sobre `sub`;
- fallback por `sub`;
- captura opcional de `TogoClaimTypes.Profile`;
- retorno autenticado após resolução válida;
- independência de nome, e-mail e profile;
- falha sem `HttpContext`;
- falha sem identidade autenticada;
- falha sem identificador;
- falha com identificador inválido.

Também foi criado teste para congelar os valores iniciais de `MedicalRecordAuditActions`.

## 12. O que ainda não foi implementado

Esta fase não implementa:

- autoria em `MedicalRecord`;
- `CreatedByUserId`, `CreatedAt` ou `UpdatedByUserId`;
- alteração de `UpdatedAt`;
- entidade, tabela, migration ou repositório de AuditLog;
- writer de auditoria;
- persistência de eventos;
- integração de autoria com controller ou casos de uso;
- endpoint de auditoria;
- alteração de JWT;
- alteração de regras clínicas.

## 13. Riscos remanescentes

| Risco | Situação após 6.3.2 | Tratamento recomendado |
|---|---|---|
| `MedicalRecord` continua sem autoria persistida | Aberto | Implementar na Fase 6.3.3 usando `ICurrentUserService`. |
| AuditLog clínico continua ausente | Aberto | Definir e persistir mecanismo mínimo na Fase 6.3.4. |
| Semântica HTTP futura para falha de resolução | Aberto | Definir ao integrar autoria às mutações; manter falha segura. |
| Auditoria de leitura | Aberto | Decidir explicitamente na Fase 6.3.4. |
| Auditoria de acesso negado | Aberto | Avaliar integração apropriada ao pipeline de autorização. |
| Retenção, soft delete e cascata | Aberto e fora do escopo | Tratar em fases próprias. |

## 14. Critérios de aceite

A Fase 6.3.2 é aceita quando:

- existe abstração clara para usuário atual;
- Application não depende diretamente de `HttpContext`;
- a API resolve `UserId` por `ClaimTypes.NameIdentifier` e aceita `sub` como fallback;
- `UserId` inválido não é aceito silenciosamente;
- usuário não resolvido não gera autoria fake ou hardcoded;
- profile é capturado quando disponível, mas não identifica o usuário;
- testes cobrem os cenários principais de resolução;
- `MedicalRecord` permanece inalterado;
- não existe migration ou alteração de banco;
- não existe AuditLog persistido;
- build e suíte de testes são validados no ambiente de execução disponível;
- esta documentação registra decisões, limites e riscos.

## 15. Fora do escopo

Permanecem fora do escopo desta fase:

- alterações em `MedicalRecord` e regras de prontuário;
- autoria clínica persistida;
- migrations e alterações de banco;
- tabela, persistência ou endpoint de AuditLog;
- alterações em `MedicalRecordsController` para aplicar autoria;
- frontend;
- Docker, Redis, RabbitMQ ou Kubernetes;
- soft delete, retenção ou `DeleteBehavior.Cascade`.

## 16. Próxima fase recomendada

**Fase 6.3.3 — Implementação de autoria clínica em MedicalRecord.**

A próxima fase deverá persistir autoria mínima de criação e alteração usando o usuário autenticado resolvido por `ICurrentUserService`, tratando diretamente **MR-DEBT-004** sem antecipar o AuditLog completo de **MR-DEBT-002**.
