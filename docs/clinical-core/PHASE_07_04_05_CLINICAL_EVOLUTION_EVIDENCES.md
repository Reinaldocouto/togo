# Fase 7.4.5 — Testes e evidências da integração ClinicalEvolution

## 1. Objetivo

Consolidar as evidências de testes, contratos, segurança, autoria, AuditLog, infraestrutura e API da integração segura de `ClinicalEvolution` com `Attendance`, sem implementar funcionalidade clínica nova.

## 2. Contexto da Fase 7.4

A Fase 7.4 integra `ClinicalEvolution` a `Attendance` por uma superfície mínima, orientada por atendimento, com criação e listagem restritas ao contexto do atendimento.

A Fase 7.4.5 revisou o estado produzido pelas fases anteriores e registrou evidências de que a integração preserva o escopo seguro definido para a trilha.

## 3. Referências às fases anteriores

- `PHASE_07_04_01_CLINICAL_EVOLUTION_ATTENDANCE_PLANNING.md`: planejamento técnico da integração.
- `PHASE_07_04_02_CLINICAL_EVOLUTION_CONTRACTS.md`: contratos/base técnica.
- `PHASE_07_04_03_CLINICAL_EVOLUTION_MINIMAL_IMPLEMENTATION.md`: endpoints mínimos `GET` e `POST` por atendimento.
- `PHASE_07_04_04_CLINICAL_EVOLUTION_AUTHORSHIP_AUDIT_IMPLEMENTATION.md`: autoria técnica e AuditLog mínimo para `ClinicalEvolution.Created`.

## 4. Arquivos revisados

### Domínio

- `backend/src/Togo.Domain/Entities/ClinicalEvolution.cs`
- `backend/src/Togo.Domain.Tests/ClinicalEvolutionTests.cs`

### Application

- `backend/src/Togo.Application/ClinicalEvolutions/UseCases/CreateClinicalEvolutionUseCase.cs`
- `backend/src/Togo.Application/ClinicalEvolutions/UseCases/ListClinicalEvolutionsByAttendanceUseCase.cs`
- `backend/src/Togo.Application.Tests/ClinicalEvolutions/CreateClinicalEvolutionUseCaseTests.cs`
- `backend/src/Togo.Application.Tests/ClinicalEvolutions/ListClinicalEvolutionsByAttendanceUseCaseTests.cs`
- `backend/src/Togo.Application.Tests/ClinicalEvolutions/Fakes/FakeClinicalEvolutionRepository.cs`

### Segurança

- `backend/src/Togo.Application/Security/ClinicalEvolutionPermissions.cs`
- `backend/src/Togo.Api/Security/ClinicalEvolutionPolicies.cs`
- `backend/src/Togo.Api/Security/ClinicalEvolutionAuthorization.cs`
- `backend/src/Togo.Application.Tests/Security/ClinicalEvolutionPermissionsTests.cs`
- `backend/src/Togo.Api.Tests/Security/ClinicalEvolutionPoliciesTests.cs`
- `backend/src/Togo.Api.Tests/Security/ClinicalEvolutionAuthorizationTests.cs`

### Auditoria

- `backend/src/Togo.Application/Auditing/ClinicalEvolutionAuditActions.cs`
- `backend/src/Togo.Application.Tests/Auditing/ClinicalEvolutionAuditActionsTests.cs`

### Infraestrutura

- `backend/src/Togo.Infrastructure/Repositories/ClinicalEvolutionRepository.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalEvolutionConfiguration.cs`
- `backend/src/Togo.Infrastructure/Migrations/20260621120000_AddClinicalEvolutionAuthorship.cs`
- `backend/src/Togo.Infrastructure.Tests/Repositories/ClinicalEvolutionRepositoryTests.cs`

### API

- `backend/src/Togo.Api/Controllers/ClinicalEvolutionsController.cs`
- `backend/src/Togo.Api.Tests/Controllers/ClinicalEvolutionsControllerTests.cs`

## 5. Arquivos alterados

- `backend/src/Togo.Api.Tests/Controllers/ClinicalEvolutionsControllerTests.cs`: adicionada evidência por reflexão para ausência de endpoints `PUT`, `PATCH` e `DELETE` no controller de evoluções clínicas.
- `docs/clinical-core/PHASE_07_04_05_CLINICAL_EVOLUTION_EVIDENCES.md`: documento de evidências da fase.

## 6. Evidências de domínio

Os testes de domínio confirmam:

- criação válida com `AttendanceId`, `RegisteredAt`, `Type`, `Text`, `CreatedByUserId` e `CreatedAt`;
- rejeição de `AttendanceId` inválido;
- rejeição de `RegisteredAt` default;
- rejeição de `Text` vazio ou whitespace;
- trim de `Text` na criação;
- preenchimento de `CreatedByUserId` e `CreatedAt`;
- inicialização de `UpdatedByUserId` e `UpdatedAt` com os dados de criação;
- rejeição de `Guid.Empty` para autoria;
- rejeição de `createdAt` default;
- separação entre `RegisteredAt` clínico e `CreatedAt` técnico;
- `UpdateText` preparado para autoria futura, validando texto, usuário e timestamp de atualização.

## 7. Evidências de contracts

Os contratos revisados confirmam:

- `CreateClinicalEvolutionRequest` contém o texto clínico somente para criação;
- `ClinicalEvolutionResponse` expõe `Text` no retorno de criação/detalhe mínimo;
- `ClinicalEvolutionListItemResponse` não possui propriedade `Text`;
- a listagem por atendimento retorna somente `Id`, `AttendanceId`, `RegisteredAt` e `Type`.

## 8. Evidências de autorização

Os testes de segurança confirmam:

- permissões `ClinicalEvolution.Read`, `ClinicalEvolution.Create` e `ClinicalEvolution.Update` existem como constantes;
- não há permissão `ClinicalEvolution.Delete`;
- policies de API espelham as permissões de application;
- matriz por profile autoriza `Admin` e `Veterinarian` para leitura/criação/update e `Assistant` somente para leitura;
- `Reception` e `ReadOnly` não recebem permissões de ClinicalEvolution;
- profile ausente, vazio ou inválido falha fechado;
- casing do profile é normalizado;
- permissão desconhecida retorna negado.

## 9. Evidências de criação

Os testes do use case de criação confirmam:

- sucesso quando o atendimento existe e está `Open`;
- divergência entre `attendanceId` de rota e corpo retorna validação;
- `AttendanceId` inválido na rota ou no corpo retorna validação;
- atendimento inexistente retorna `NotFound`;
- atendimento `Closed` ou `Canceled` retorna conflito;
- `RegisteredAt` default, `Text` vazio e enum inválido retornam validação;
- usuário atual é resolvido por `ICurrentUserService`;
- falha de resolução do usuário interrompe o fluxo sem persistir nem auditar;
- repository é chamado apenas em sucesso;
- `ClinicalEvolution.Created` é escrito apenas em sucesso.

## 10. Evidências de listagem

Os testes e o repositório confirmam:

- listagem é sempre filtrada por `AttendanceId`;
- lista vazia é retornada quando o atendimento existe e não possui evoluções;
- `AttendanceId` inválido retorna validação sem consultar evoluções;
- atendimento inexistente retorna `NotFound` sem consultar evoluções;
- infraestrutura ordena por `RegisteredAt` e, em seguida, por `Id`;
- resposta de listagem não expõe `Text`.

## 11. Evidências de autoria

As evidências confirmam:

- `ClinicalEvolution.Create` exige `CreatedByUserId` e `CreatedAt`;
- criação inicializa `UpdatedByUserId` e `UpdatedAt` com os mesmos valores técnicos;
- use case usa o usuário atual para autoria;
- infraestrutura persiste e materializa os campos de autoria;
- falha de resolução do usuário impede persistência e AuditLog.

## 12. Evidências de AuditLog

As evidências confirmam:

- existe ação `ClinicalEvolution.Created`;
- existe constante `ClinicalEvolution.Updated` reservada, sem fluxo real nesta fase;
- não existem ações `ClinicalEvolution.Deleted`, `ClinicalEvolution.Read` ou `ClinicalEvolution.AccessDenied`;
- criação bem-sucedida grava evento com entidade `ClinicalEvolution`, id da evolução, usuário, profile e metadata mínima;
- metadata contém somente `AttendanceId` e `Type`;
- metadata não contém `Text` nem payload completo do request.

## 13. Evidências de infraestrutura

Os testes de repositório confirmam:

- `AddAsync` persiste a evolução clínica;
- campos de autoria são persistidos e materializados;
- `ListByAttendanceIdAsync` retorna apenas evoluções do atendimento solicitado;
- ordenação ocorre por `RegisteredAt` e `Id`;
- evoluções de outro atendimento não retornam na listagem filtrada.

## 14. Evidências de API

Os testes por reflexão confirmam:

- controller possui `[Authorize]`;
- rota base é `api/attendances/{attendanceId:long}/clinical-evolutions`;
- `GET` usa policy `ClinicalEvolution.Read`;
- `POST` usa policy `ClinicalEvolution.Create`;
- não há métodos HTTP `PUT`, `PATCH` ou `DELETE` no controller;
- list item não expõe `Text`;
- response de criação mantém `Text` apenas no contrato de criação/detalhe mínimo.

## 15. Confirmação de ausência de `Text` em listagem

Confirmado por contrato e testes: `ClinicalEvolutionListItemResponse` não declara `Text`, e a projeção de listagem constrói itens sem texto clínico.

## 16. Confirmação de ausência de `Text` em AuditLog

Confirmado por implementação e testes: `CreateMetadataJson` serializa somente `AttendanceId` e `Type`, e os testes verificam que metadata não contém a chave `Text` nem o valor sensível usado no request.

## 17. Confirmação de ausência de update/delete/listagem global

Confirmado por inspeção e teste por reflexão:

- não há endpoint `PUT`, `PATCH` ou `DELETE` para `ClinicalEvolution`;
- não há use case real de update/delete;
- não há listagem global de evoluções clínicas;
- a única listagem disponível é por `AttendanceId`.

## 18. Confirmação de ausência de endpoint novo

Nenhum endpoint novo foi criado nesta fase. A superfície permanece limitada a:

- `GET /api/attendances/{attendanceId}/clinical-evolutions`;
- `POST /api/attendances/{attendanceId}/clinical-evolutions`.

## 19. Confirmação de ausência de migration/schema nova

Nenhuma migration nova, alteração de schema, alteração de `AppDbContext` ou configuração EF foi criada nesta fase.

## 20. Lacunas remanescentes

- Não há auditoria de leitura, por decisão de escopo.
- Não há auditoria de acesso negado, por decisão de escopo.
- Não há fluxo real de `ClinicalEvolution.Updated`, embora a constante exista para preparação futura.
- Não há endpoint de detalhe individual; a trilha atual mantém apenas criação e listagem por atendimento.

## 21. Riscos remanescentes

- A evolução futura de update/retificação deve preservar autoria, rastreabilidade e ausência de texto sensível em AuditLog.
- A eventual criação de endpoints globais deve ser tratada como mudança de escopo e reavaliada do ponto de vista de privacidade clínica.
- Se houver auditoria de leitura em fase futura, ela deve ser desenhada sem registrar conteúdo clínico sensível.

## 22. Fora do escopo

Permanecem fora do escopo desta fase:

- novo endpoint;
- update/delete/soft delete;
- retificação;
- assinatura clínica;
- anexos;
- listagem global;
- nova migration ou alteração de schema;
- nova audit action em fluxo real;
- auditoria de leitura ou acesso negado;
- alterações em `Attendance`, `MedicalRecord`, `Prescription`, frontend ou infraestrutura operacional.

## 23. Critérios de aceite

Atendidos nesta fase:

- evidências de domínio, contratos, autorização, criação, listagem, autoria, AuditLog, infraestrutura e API documentadas;
- ausência de `Text` em listagem confirmada;
- ausência de `Text` em AuditLog confirmada;
- ausência de update/delete/listagem global confirmada;
- ausência de endpoint novo confirmada;
- ausência de migration/schema nova confirmada;
- lacunas e riscos remanescentes registrados;
- documento da fase criado.

## 24. Próxima fase recomendada

Fase 7.4.6 — Encerramento da trilha ClinicalEvolution.
