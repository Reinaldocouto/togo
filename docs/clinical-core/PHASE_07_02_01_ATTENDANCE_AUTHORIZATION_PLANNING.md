# Fase 7.2.1 — Planejamento técnico de autorização granular de Attendance

## 1. Objetivo

Esta fase planeja a autorização granular da vertical `Attendance`, sem implementar código. O objetivo é definir operações protegidas, perfis mínimos autorizados, perfis não autorizados, nomes futuros de permissões e policies, impactos esperados no controller e nos testes, riscos de segurança/privacidade e critérios de aceite para as próximas fases de implementação e validação.

A entrega desta fase é exclusivamente documental e deve servir como insumo direto para a Fase 7.2.2, na qual as permissões e policies poderão ser implementadas de forma incremental.

## 2. Contexto

A Fase 6 consolidou a vertical `MedicalRecord` como referência de maturidade clínica, com autorização granular por operação, autoria, auditoria, persistência segura, integridade, validação estrutural, qualidade operacional e evidências finais. O encerramento da Fase 6 registra que `MedicalRecord` passou a ter endpoints de leitura, criação e atualização protegidos por autenticação e autorização granular mínima.

A Fase 7.0 abriu a expansão clínica e operacional pós-hardening `MedicalRecord`. Ela definiu que `MedicalRecord` deve permanecer como memória clínica longitudinal consolidada e que `Attendance` deve ser tratado como evento/caso clínico independente, associado ao paciente e usado como eixo operacional para futuras evoluções clínicas e prescrições.

A Fase 7.1 escolheu `Attendance` como próximo eixo técnico. A inspeção documentada confirmou que `Attendance` já possui entidade de domínio, ciclo de vida `Open`, `Closed` e `Canceled`, use cases, repository, controller, contratos, testes e documentação histórica da Fase 4. A mesma fase registrou, porém, uma lacuna importante: `AttendancesController` está protegido apenas por `[Authorize]` genérico e não possui policies granulares equivalentes às de `MedicalRecord`.

Antes de evoluir `ClinicalEvolution` e `Prescription`, é necessário proteger explicitamente as operações atuais de `Attendance`, pois elas expõem dados clínico-operacionais sensíveis e controlam eventos que serão base para fluxos clínicos posteriores.

### Fracionamento recomendado da Fase 7.2

Para reduzir risco e manter governança incremental, a Fase 7.2 deve ser fracionada em:

| Fase | Descrição |
| --- | --- |
| 7.2.1 | Planejamento técnico de autorização granular de `Attendance`. |
| 7.2.2 | Implementação de autorização granular de `Attendance`. |
| 7.2.3 | Testes de autorização granular de `Attendance`. |
| 7.2.4 | Evidências e fechamento do hardening mínimo de autorização de `Attendance`. |

## 3. Estado atual da autorização

### 3.1. Padrão atual de MedicalRecord

O padrão de `MedicalRecord` é composto por:

- permissões centralizadas na camada Application em `MedicalRecordPermissions`, com constantes `MedicalRecord.Read`, `MedicalRecord.Create` e `MedicalRecord.Update`;
- nomes de policies centralizados na API em `MedicalRecordPolicies`, usando os mesmos valores textuais das permissões;
- registro das policies via extensão `AddMedicalRecordPolicies()` em `AuthorizationOptions`;
- matriz interna `PermissionsByProfile` baseada nos perfis reais `Admin`, `Veterinarian`, `Assistant`, `Reception` e `ReadOnly`;
- uso da claim `togo:profile` para resolver o perfil do usuário;
- normalização e validação do perfil por `UserProfiles`;
- retorno de `false` quando a claim de profile está ausente, vazia ou inválida;
- uso de `[Authorize(Policy = MedicalRecordPolicies.Read)]`, `[Authorize(Policy = MedicalRecordPolicies.Create)]` e `[Authorize(Policy = MedicalRecordPolicies.Update)]` nos endpoints do controller;
- testes cobrindo ausência de token, ausência de profile, perfis sem permissão e perfis autorizados.

A matriz de `MedicalRecord` encontrada no código é:

| Perfil | Leitura | Criação | Atualização |
| --- | --- | --- | --- |
| `Admin` | Sim | Sim | Sim |
| `Veterinarian` | Sim | Sim | Sim |
| `Assistant` | Sim | Não | Não |
| `Reception` | Não | Não | Não |
| `ReadOnly` | Não | Não | Não |

### 3.2. Estado atual de Attendance

`AttendancesController` possui `[Authorize]` aplicado no controller, mas não define `[Authorize(Policy = ...)]` por operação. Os endpoints atuais mapeados no controller são:

- `GET /api/attendances`;
- `GET /api/attendances/{id}`;
- `POST /api/attendances`;
- `PATCH /api/attendances/{id}/close`;
- `PATCH /api/attendances/{id}/cancel`.

Não foram encontrados arquivos equivalentes a `AttendancePermissions.cs`, `AttendancePolicies.cs` ou `AttendanceAuthorization.cs` no estado atual inspecionado. Também não há registro de `AddAttendancePolicies()` em `Program.cs`.

### 3.3. Diferença entre os padrões

| Dimensão | `MedicalRecord` | `Attendance` atual | Lacuna |
| --- | --- | --- | --- |
| Permissões centralizadas | Sim, em Application. | Não encontrado. | Criar `AttendancePermissions` em fase futura. |
| Policies centralizadas | Sim, na API. | Não encontrado. | Criar nomes e registro de policies em fase futura. |
| Matriz por perfil | Sim, em autorização da API. | Não encontrada. | Definir matriz por operação. |
| Controller | `[Authorize]` + policies por action. | Apenas `[Authorize]` no controller. | Substituir proteção genérica por policies por operação. |
| Testes 401/403 | Cobertos em `MedicalRecord`. | Testes atuais cobrem retorno funcional direto do controller, não autorização granular. | Adicionar testes de autorização em fase futura. |
| Claim de profile | `togo:profile`. | Apenas autenticação genérica no controller. | Reutilizar claim e perfis reais existentes. |

### 3.4. Riscos do `[Authorize]` genérico

O `[Authorize]` genérico garante autenticação, mas não diferencia o que cada perfil pode fazer. Isso cria riscos de acesso excessivo, especialmente porque `Attendance` controla abertura, fechamento e cancelamento de atendimentos. Um usuário autenticado sem função clínica/operacional adequada pode, dependendo da configuração de autenticação, acessar lista, detalhe, criação ou transições críticas se nenhuma policy granular for aplicada.

## 4. Endpoints e operações mapeadas

| Operação | Endpoint | Método no controller | Use case atual |
| --- | --- | --- | --- |
| `ListAttendances` | `GET /api/attendances` | `List` | `ListAttendancesUseCase` |
| `GetAttendanceById` | `GET /api/attendances/{id}` | `GetById` | `GetAttendanceByIdUseCase` |
| `CreateAttendance` | `POST /api/attendances` | `Create` | `CreateAttendanceUseCase` |
| `CloseAttendance` | `PATCH /api/attendances/{id}/close` | `Close` | `CloseAttendanceUseCase` |
| `CancelAttendance` | `PATCH /api/attendances/{id}/cancel` | `Cancel` | `CancelAttendanceUseCase` |

## 5. Matriz de autorização proposta

Perfis reais encontrados no projeto: `Admin`, `Veterinarian`, `Assistant`, `Reception` e `ReadOnly`.

A matriz abaixo é proposta para a implementação futura. Ela busca equilibrar o padrão restritivo de `MedicalRecord` com a natureza operacional de `Attendance`. Como `Reception` é um perfil real e operacional, sua participação é proposta para leitura, criação e cancelamento administrativo-operacional, mas não para fechamento clínico. Esta decisão deve ser validada com produto/negócio antes ou durante a Fase 7.2.2.

| Operação | Endpoint | Risco | Perfis autorizados | Perfis não autorizados | Policy sugerida | Permissão sugerida | Justificativa |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `ListAttendances` | `GET /api/attendances` | Exposição de volume operacional, vínculos paciente-atendimento, timestamps, status e fluxo da clínica. | `Admin`, `Veterinarian`, `Assistant`, `Reception` | `ReadOnly`; usuário sem claim `togo:profile`; profile inválido. | `AttendancePolicies.Read` | `AttendancePermissions.Read` | Listagem é necessária para operação clínica/recepção, mas não deve ser liberada apenas por autenticação genérica. |
| `GetAttendanceById` | `GET /api/attendances/{id}` | Exposição de caso clínico-operacional específico e vínculo com paciente. | `Admin`, `Veterinarian`, `Assistant`, `Reception` | `ReadOnly`; usuário sem claim `togo:profile`; profile inválido. | `AttendancePolicies.Read` | `AttendancePermissions.Read` | Detalhe usa a mesma permissão de leitura para manter simplicidade inicial e alinhamento com o padrão de permissões centralizadas. |
| `CreateAttendance` | `POST /api/attendances` | Abertura de caso clínico-operacional, possível duplicidade de atendimento aberto e impacto no fluxo da clínica. | `Admin`, `Veterinarian`, `Assistant`, `Reception` | `ReadOnly`; usuário sem claim `togo:profile`; profile inválido. | `AttendancePolicies.Create` | `AttendancePermissions.Create` | Criação pode ser ação clínica ou operacional; deve ser permitida apenas a perfis com função ativa na operação. |
| `CloseAttendance` | `PATCH /api/attendances/{id}/close` | Encerramento de caso, possível bloqueio de evolução/prescrição futura e alteração crítica do estado clínico-operacional. | `Admin`, `Veterinarian` | `Assistant`, `Reception`, `ReadOnly`; usuário sem claim `togo:profile`; profile inválido. | `AttendancePolicies.Close` | `AttendancePermissions.Close` | Fechamento é evento crítico com semântica clínica; deve ficar restrito a perfil clínico autorizado e administração. |
| `CancelAttendance` | `PATCH /api/attendances/{id}/cancel` | Alteração crítica de estado operacional, risco de ocultação indevida de fluxo e impacto em auditoria futura. | `Admin`, `Veterinarian`, `Reception` | `Assistant`, `ReadOnly`; usuário sem claim `togo:profile`; profile inválido. | `AttendancePolicies.Cancel` | `AttendancePermissions.Cancel` | Cancelamento é crítico, mas pode ser uma operação administrativa/recepção; `Assistant` fica excluído por padrão conservador até validação de negócio. |

### Decisão pendente

Validar com produto/negócio se `Reception` deve realmente cancelar atendimentos e se `Assistant` deve criar atendimentos. Caso a regra operacional seja mais restritiva, a Fase 7.2.2 deve ajustar a matriz antes da implementação.

## 6. Decisões técnicas

- Criar permissões centralizadas de `Attendance` em fase futura, preferencialmente em `backend/src/Togo.Application/Security/AttendancePermissions.cs`.
- Usar nomes textuais alinhados ao estilo real de `MedicalRecord`: `Attendance.Read`, `Attendance.Create`, `Attendance.Close` e `Attendance.Cancel`.
- Criar policies centralizadas na API em fase futura, preferencialmente em `backend/src/Togo.Api/Security/AttendancePolicies.cs`.
- Criar uma extensão de autorização equivalente a `AddMedicalRecordPolicies()`, possivelmente em `AttendanceAuthorization.cs`, se a implementação seguir exatamente o padrão atual de `MedicalRecordAuthorization`.
- Registrar as policies em `Program.cs` em fase futura, junto ao registro já existente de `AddMedicalRecordPolicies()`.
- Substituir o `[Authorize]` genérico por `[Authorize(Policy = ...)]` por action em fase futura, mantendo autenticação no controller se fizer sentido para o padrão da API.
- Manter o domínio sem dependência de autorização, claims, policies ou perfis.
- Manter regras de negócio de ciclo de vida nos use cases/domínio, sem misturá-las com autorização HTTP.
- Não criar roles/perfis novos nesta fase nem na implementação futura imediata, salvo decisão explícita de produto.
- Não criar endpoint novo nesta fase.
- Não alterar DTOs, migrations, banco, EF, JWT, frontend, Docker, Redis, RabbitMQ ou Kubernetes nesta fase.

## 7. Impacto previsto da 7.2.2

A Fase 7.2.2 deve implementar a autorização granular planejada, mantendo o comportamento funcional dos endpoints atuais. Arquivos provavelmente alterados ou criados:

| Arquivo | Tipo de impacto esperado |
| --- | --- |
| `backend/src/Togo.Application/Security/AttendancePermissions.cs` | Novo arquivo com constantes `Read`, `Create`, `Close` e `Cancel`, usando valores `Attendance.Read`, `Attendance.Create`, `Attendance.Close` e `Attendance.Cancel`. |
| `backend/src/Togo.Api/Security/AttendancePolicies.cs` | Novo arquivo com nomes de policies equivalentes às permissões. |
| `backend/src/Togo.Api/Security/AttendanceAuthorization.cs` | Novo arquivo provável, seguindo o padrão real de `MedicalRecordAuthorization`, com matriz `PermissionsByProfile`, `AddAttendancePolicies()` e `HasPermission()`. |
| `backend/src/Togo.Api/Program.cs` | Registro futuro das policies de `Attendance`, sem remover `AddMedicalRecordPolicies()`. |
| `backend/src/Togo.Api/Controllers/AttendancesController.cs` | Aplicação de `[Authorize(Policy = AttendancePolicies.Read/Create/Close/Cancel)]` nas actions atuais. |
| `backend/src/Togo.Api.Tests/Controllers/AttendancesControllerTests.cs` | Possível adaptação se os testes passarem a validar attributes ou se forem adicionados testes de integração. |
| `backend/src/Togo.Api.Tests/Security/AttendanceAuthorizationTests.cs` | Novo arquivo provável para validar matriz de permissões por perfil. |
| `backend/src/Togo.Api.Tests/Security/AttendancePoliciesTests.cs` | Novo arquivo provável para validar nomes e unicidade das policies. |
| `backend/src/Togo.Application.Tests/Security/AttendancePermissionsTests.cs` | Novo arquivo provável para validar nomes e unicidade das permissões. |

## 8. Impacto previsto da 7.2.3

A Fase 7.2.3 deve validar autorização granular sem alterar semântica funcional dos use cases. Cenários mínimos sugeridos:

- sem token em `GET /api/attendances` deve retornar `401`;
- sem token em `GET /api/attendances/{id}` deve retornar `401`;
- sem token em `POST /api/attendances` deve retornar `401`;
- sem token em `PATCH /api/attendances/{id}/close` deve retornar `401`;
- sem token em `PATCH /api/attendances/{id}/cancel` deve retornar `401`;
- token com perfil sem permissão deve retornar `403`;
- token sem claim `togo:profile` deve retornar `403`, seguindo o padrão atual de `MedicalRecord`;
- token com profile vazio ou inválido deve retornar `403`;
- perfil autorizado para listar deve obter `200` em `GET /api/attendances`;
- perfil autorizado para detalhe deve obter `200` em `GET /api/attendances/{id}` quando o atendimento existir;
- perfil autorizado para criar deve obter `201` em `POST /api/attendances` com request válido;
- perfil autorizado para fechar deve obter `200` em `PATCH /api/attendances/{id}/close` com request válido;
- perfil autorizado para cancelar deve obter `200` em `PATCH /api/attendances/{id}/cancel` quando a transição for válida;
- perfil autorizado para leitura, mas não para fechamento, deve obter `403` no fechamento;
- perfil autorizado para criação, mas não para cancelamento, deve obter `403` no cancelamento, se a matriz final mantiver essa separação;
- testes de segurança devem ser complementares aos testes funcionais já existentes de controller/use cases.

## 9. Fora do escopo

Esta fase não implementa:

- código C#;
- testes;
- policies;
- permissões;
- endpoints;
- migrations;
- alteração de controller;
- alteração de `Program.cs`;
- alteração de JWT;
- alteração de domínio;
- alteração de repository;
- alteração de validators;
- alteração de contracts/DTOs;
- alteração de frontend;
- alteração de Docker, Redis, RabbitMQ ou Kubernetes;
- autorização em `ClinicalEvolution`;
- autorização em `Prescription`;
- autoria ou auditoria de eventos de `Attendance`.

## 10. Riscos e cuidados

- Listagem ampla de atendimentos pode expor dados operacionais sensíveis, volume de atendimento, status e vínculos com pacientes.
- Detalhe de atendimento pode revelar caso clínico-operacional específico.
- Criação indevida pode abrir casos duplicados, impactar agenda/fluxo e confundir futuras evoluções/prescrições.
- Fechamento e cancelamento são eventos críticos e podem bloquear, distorcer ou ocultar fluxo clínico-operacional.
- Permissões mal definidas podem bloquear a operação real da clínica ou liberar acesso excessivo a dados sensíveis.
- Criar policies sem matriz formal aumenta retrabalho e risco de divergência entre código, testes e documentação.
- Não misturar autorização com autoria/auditoria nesta fase; autoria e auditoria devem ser planejadas em etapa própria.
- Não implementar `ClinicalEvolution` ou `Prescription` antes de endurecer minimamente `Attendance`.
- Se futuramente houver multi-tenant, clínica/unidade, escopo por equipe ou vínculo profissional-paciente, a autorização por perfil deverá ser complementada por autorização contextual.
- A matriz proposta inclui decisões pendentes sobre `Reception` e `Assistant`; essas decisões devem ser confirmadas antes de codificar a Fase 7.2.2.

## 11. Critérios de aceite

A Fase 7.2.1 será aceita se:

- este documento for criado em `docs/clinical-core/PHASE_07_02_01_ATTENDANCE_AUTHORIZATION_PLANNING.md`;
- os endpoints atuais de `Attendance` forem mapeados;
- o padrão de `MedicalRecord` for usado como referência;
- o estado atual de autorização de `Attendance` for descrito;
- a matriz de autorização for proposta com perfis reais do projeto;
- decisões técnicas forem registradas;
- o impacto previsto da Fase 7.2.2 for descrito;
- o impacto previsto da Fase 7.2.3 for descrito;
- riscos e fora do escopo forem documentados;
- nenhuma implementação for feita;
- nenhuma migration for criada;
- somente documentação for alterada;
- `git diff --check` passar.

## 12. Próxima fase recomendada

Próxima fase recomendada:

```text
Fase 7.2.2 — Implementação de autorização granular de Attendance
```

Objetivo sugerido:

```text
Implementar permissões e policies granulares para os endpoints atuais de Attendance, substituindo o [Authorize] genérico por policies por operação e mantendo o comportamento funcional existente.
```

## Fontes consultadas

Documentos obrigatórios consultados:

- `docs/clinical-core/PHASE_07_01_ATTENDANCE_TECHNICAL_PLANNING.md`;
- `docs/clinical-core/PHASE_07_00_CLINICAL_OPERATIONAL_EXPANSION_PLANNING.md`;
- `docs/clinical-core/PHASE_06_06_05_MEDICAL_RECORD_PHASE_06_CLOSURE.md`.

Padrão de autorização de `MedicalRecord` consultado:

- `backend/src/Togo.Application/Security/MedicalRecordPermissions.cs`;
- `backend/src/Togo.Api/Security/MedicalRecordPolicies.cs`;
- `backend/src/Togo.Api/Security/MedicalRecordAuthorization.cs`;
- `backend/src/Togo.Api/Controllers/MedicalRecordsController.cs`;
- `backend/src/Togo.Api.Tests/MedicalRecords/MedicalRecordsControllerTests.cs`;
- `backend/src/Togo.Api.Tests/Security/MedicalRecordAuthorizationTests.cs`;
- `backend/src/Togo.Api.Tests/Security/MedicalRecordPoliciesTests.cs`;
- `backend/src/Togo.Application.Tests/Security/MedicalRecordPermissionsTests.cs`.

Vertical `Attendance` consultada:

- `backend/src/Togo.Api/Controllers/AttendancesController.cs`;
- `backend/src/Togo.Application/Attendances/Contracts`;
- `backend/src/Togo.Application/Attendances/UseCases`;
- `backend/src/Togo.Api.Tests/Controllers/AttendancesControllerTests.cs`;
- `backend/src/Togo.Application.Tests/Attendances`.

Perfis e claims consultados:

- `backend/src/Togo.Application/Security`;
- `backend/src/Togo.Domain/Security/UserProfiles.cs`;
- `backend/src/Togo.Domain/Security/TogoClaimTypes.cs`.

Não foi identificada ausência dos arquivos obrigatórios consultados para esta fase.
