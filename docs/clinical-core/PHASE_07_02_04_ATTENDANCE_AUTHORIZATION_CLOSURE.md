# Fase 7.2.4 — Evidências e fechamento do hardening mínimo de autorização de Attendance

## 1. Objetivo

A Fase 7.2.4 encerra formalmente o hardening mínimo de autorização da vertical `Attendance`, consolidando as evidências documentais, técnicas e de testes produzidas nas Fases 7.2.1, 7.2.2 e 7.2.3.

Esta fase é exclusivamente documental e de governança. Ela não implementa código novo, não altera testes, não cria migrations, não altera policies, permissões, controllers, regras de negócio, contratos, infraestrutura ou frontend.

## 2. Contexto da Fase 7.2

A Fase 7 faz parte da expansão clínica e operacional pós-hardening de `MedicalRecord`. A Fase 7.0 definiu `Attendance` como eixo natural para a expansão operacional clínica, pois o atendimento já existe no domínio, aplicação, infraestrutura e API, e serve como vínculo operacional para futuras evoluções clínicas e prescrições.

A Fase 7.1 confirmou que `Attendance` já possuía ciclo de vida, use cases, repository, controller, contratos e testes, mas permanecia protegido apenas por `[Authorize]` genérico no controller. A Fase 7.2 tratou essa lacuna e moveu `Attendance` para autorização granular por operação, seguindo o padrão mínimo consolidado em `MedicalRecord`.

Com isso, `Attendance` deixou de depender apenas de autenticação genérica e passou a ter matriz explícita por profile para leitura, criação, fechamento e cancelamento.

## 3. Subfases consolidadas

| Subfase | Descrição |
| --- | --- |
| 7.2.1 | Planejamento técnico de autorização granular de `Attendance`. |
| 7.2.2 | Implementação de autorização granular de `Attendance`. |
| 7.2.3 | Testes de autorização granular de `Attendance`. |
| 7.2.4 | Evidências e fechamento do hardening mínimo de autorização de `Attendance`. |

## 4. Artefatos consolidados

### 4.1 Documentos consolidados

- `docs/clinical-core/PHASE_07_02_01_ATTENDANCE_AUTHORIZATION_PLANNING.md`.
- `docs/clinical-core/PHASE_07_02_02_ATTENDANCE_AUTHORIZATION_IMPLEMENTATION.md`.
- `docs/clinical-core/PHASE_07_02_03_ATTENDANCE_AUTHORIZATION_TESTS.md`.

### 4.2 Documentos de contexto consultados

- `docs/clinical-core/PHASE_07_01_ATTENDANCE_TECHNICAL_PLANNING.md`.
- `docs/clinical-core/PHASE_07_00_CLINICAL_OPERATIONAL_EXPANSION_PLANNING.md`.

### 4.3 Arquivos técnicos consolidados das fases anteriores

- `backend/src/Togo.Application/Security/AttendancePermissions.cs`.
- `backend/src/Togo.Api/Security/AttendancePolicies.cs`.
- `backend/src/Togo.Api/Security/AttendanceAuthorization.cs`.
- `backend/src/Togo.Api/Program.cs`.
- `backend/src/Togo.Api/Controllers/AttendancesController.cs`.
- `backend/src/Togo.Application.Tests/Security/AttendancePermissionsTests.cs`.
- `backend/src/Togo.Api.Tests/Security/AttendancePoliciesTests.cs`.
- `backend/src/Togo.Api.Tests/Security/AttendanceAuthorizationTests.cs`.
- `backend/src/Togo.Api.Tests/Controllers/AttendancesControllerTests.cs`.

Todos os arquivos técnicos obrigatórios consultados estavam presentes no repositório durante esta fase.

## 5. Matriz final de autorização

| Operação | Endpoint | Policy | Perfis autorizados | Perfis negados principais |
| --- | --- | --- | --- | --- |
| `ListAttendances` | `GET /api/attendances` | `Attendance.Read` | `Admin`, `Veterinarian`, `Assistant`, `Reception` | `ReadOnly`, sem profile, profile inválido |
| `GetAttendanceById` | `GET /api/attendances/{id}` | `Attendance.Read` | `Admin`, `Veterinarian`, `Assistant`, `Reception` | `ReadOnly`, sem profile, profile inválido |
| `CreateAttendance` | `POST /api/attendances` | `Attendance.Create` | `Admin`, `Veterinarian`, `Assistant`, `Reception` | `ReadOnly`, sem profile, profile inválido |
| `CloseAttendance` | `PATCH /api/attendances/{id}/close` | `Attendance.Close` | `Admin`, `Veterinarian` | `Assistant`, `Reception`, `ReadOnly`, sem profile, profile inválido |
| `CancelAttendance` | `PATCH /api/attendances/{id}/cancel` | `Attendance.Cancel` | `Admin`, `Veterinarian`, `Reception` | `Assistant`, `ReadOnly`, sem profile, profile inválido |

## 6. Evidências de implementação

As evidências técnicas consolidadas são:

- `AttendancePermissions` centraliza as permissões `Attendance.Read`, `Attendance.Create`, `Attendance.Close` e `Attendance.Cancel` na camada Application.
- `AttendancePolicies` centraliza os nomes das policies na API, usando os mesmos valores textuais das permissões.
- `AttendanceAuthorization` implementa a matriz de permissões por profile usando `Admin`, `Veterinarian`, `Assistant`, `Reception` e `ReadOnly`.
- `AttendanceAuthorization.HasPermission(...)` usa a claim `togo:profile`, rejeita profile ausente, vazio ou inválido, normaliza casing por `UserProfiles` e nega permissões desconhecidas.
- `AddAttendancePolicies()` registra as policies `Attendance.Read`, `Attendance.Create`, `Attendance.Close` e `Attendance.Cancel` com `RequireAuthenticatedUser()` e assertion baseada em `HasPermission(...)`.
- `Program.cs` mantém `AddMedicalRecordPolicies()` e adiciona `AddAttendancePolicies()` no registro de autorização.
- `AttendancesController` mantém `[Authorize]` no controller e aplica policies específicas por action:
  - `List` com `AttendancePolicies.Read`;
  - `GetById` com `AttendancePolicies.Read`;
  - `Create` com `AttendancePolicies.Create`;
  - `Close` com `AttendancePolicies.Close`;
  - `Cancel` com `AttendancePolicies.Cancel`.
- As rotas existentes foram preservadas:
  - `GET /api/attendances`;
  - `GET /api/attendances/{id}`;
  - `POST /api/attendances`;
  - `PATCH /api/attendances/{id}/close`;
  - `PATCH /api/attendances/{id}/cancel`.
- Nenhum endpoint novo foi criado nas fases de autorização granular.
- Nenhuma regra de negócio de `Attendance` foi alterada pela autorização granular; ciclo de vida, use cases e validações funcionais permanecem separados da autorização HTTP.

## 7. Evidências de testes

A Fase 7.2.3 consolidou cobertura automatizada para a autorização granular de `Attendance` por testes unitários e reflection:

- testes de `AttendancePermissions`, validando valores esperados, preenchimento e unicidade das permissões;
- testes de `AttendancePolicies`, validando equivalência com permissões, preenchimento e unicidade das policies;
- testes de `AttendanceAuthorization`, validando a matriz positiva por profile e permissão;
- testes de `AttendanceAuthorization`, validando matriz negativa para perfis sem permissão;
- cobertura de claim de profile ausente;
- cobertura de claim de profile vazia;
- cobertura de claim de profile inválida;
- cobertura de normalização de casing do profile;
- cobertura de permissão desconhecida;
- testes de `AttendancesController` por reflection, confirmando `[Authorize]` no controller e `[Authorize(Policy = AttendancePolicies.*)]` nas actions;
- preservação dos testes funcionais existentes do controller para resultados `Ok`, `CreatedAtAction`, `BadRequest`, `NotFound` e `Conflict`.

Essas evidências confirmam que a matriz documentada está refletida nos artefatos de autorização e nas attributes aplicadas ao controller.

## 8. Limitação conhecida sobre 401/403 HTTP real

A Fase 7.2.3 validou a matriz de autorização e as attributes aplicadas ao controller, mas não implementou pipeline HTTP real com autenticação fake/JWT reutilizável para `Attendance`.

A limitação registrada é:

- cenários equivalentes a `403 Forbidden` foram cobertos por `AttendanceAuthorization.HasPermission(...)`;
- exigência de autenticação e policies por action foi coberta por reflection em `AttendancesControllerTests`;
- não houve execução de testes HTTP reais de `401 Unauthorized` e `403 Forbidden` ponta a ponta para `Attendance`;
- testes HTTP reais de `401/403` podem ser planejados futuramente se houver infraestrutura integrada e reutilizável de autenticação de teste;
- essa limitação não bloqueia o encerramento da Fase 7.2, porque o escopo planejado para matriz, implementação, testes unitários/reflection e documentação foi atendido.

## 9. Decisões pendentes de produto/negócio

Permanecem pendentes para validação futura de produto/negócio:

- se `Assistant` deve realmente criar atendimento;
- se `Reception` deve realmente cancelar atendimento.

A Fase 7.2 implementou a matriz planejada e documentada, mas essas decisões podem ser revisitadas em uma fase futura de produto, negócio ou segurança operacional.

## 10. O que foi deliberadamente deixado fora

A Fase 7.2 não implementou:

- autoria de `Attendance`;
- AuditLog de `Attendance`;
- Soft Delete;
- retenção;
- justificativa de cancelamento;
- endpoint novo;
- mudanças de ciclo de vida;
- mudanças em `ClinicalEvolution`;
- mudanças em `Prescription`;
- frontend;
- infraestrutura;
- migrations;
- alterações de schema;
- alterações de banco;
- alterações de JWT;
- evidência Swagger executada.

## 11. Riscos remanescentes

Os riscos remanescentes após o encerramento da Fase 7.2 são:

- ausência de teste HTTP real `401/403` ponta a ponta para `Attendance`;
- decisões de produto pendentes sobre `Assistant` criar atendimento e `Reception` cancelar atendimento;
- autorização contextual futura pode ser necessária se houver regras por clínica, unidade, time, profissional responsável, vínculo profissional-paciente ou escopo multi-tenant;
- autoria e auditoria ainda não foram implementadas para eventos críticos de `Attendance`, especialmente criação, fechamento e cancelamento;
- listagem de atendimentos ainda precisa de cuidado com minimização, paginação, filtros seguros e avaliação de exposição operacional em fases futuras;
- fechamento e cancelamento permanecem eventos críticos que devem receber trilha de auditoria em fase posterior.

## 12. Critérios finais de aceite

A Fase 7.2 é considerada encerrada se:

- planejamento 7.2.1 estiver consolidado;
- implementação 7.2.2 estiver consolidada;
- testes 7.2.3 estiverem consolidados;
- matriz final estiver documentada;
- evidências técnicas estiverem registradas;
- riscos remanescentes estiverem documentados;
- nenhuma implementação nova for feita nesta fase;
- nenhuma migration for criada;
- somente documentação for alterada nesta fase;
- `git diff --check` passar.

## 13. Decisão final da Fase 7.2

A Fase 7.2 fica encerrada como hardening mínimo de autorização da vertical `Attendance`, substituindo a proteção efetiva apenas genérica por autorização granular por operação, com matriz por profile documentada, implementada e coberta por testes unitários/reflection.

O encerramento da Fase 7.2 não significa que `Attendance` esteja totalmente equivalente a `MedicalRecord` em autoria, auditoria, retenção e evidências HTTP ponta a ponta; esses temas permanecem para fases futuras.

## 14. Próxima fase recomendada

Próxima fase recomendada:

```text
Fase 7.3 — Autoria e auditoria de eventos críticos de Attendance
```

Fracionamento sugerido para a Fase 7.3:

| Subfase | Descrição |
| --- | --- |
| 7.3.1 | Planejamento técnico de autoria e auditoria de `Attendance`. |
| 7.3.2 | Contratos/base técnica de autoria e audit actions de `Attendance`. |
| 7.3.3 | Implementação de autoria mínima de `Attendance`. |
| 7.3.4 | Implementação de AuditLog para eventos críticos de `Attendance`. |
| 7.3.5 | Testes e evidências de autoria/auditoria de `Attendance`. |
| 7.3.6 | Encerramento da trilha de autoria/auditoria de `Attendance`. |

Nada da Fase 7.3 foi implementado nesta fase.
