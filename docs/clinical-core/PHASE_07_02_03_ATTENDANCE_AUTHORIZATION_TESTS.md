# Fase 7.2.3 — Testes de autorização granular de Attendance

## 1. Objetivo

Validar, por cobertura automatizada, a autorização granular de `Attendance` implementada na Fase 7.2.2, garantindo que permissões, policies, matriz por profile e atributos do controller estejam alinhados ao planejamento técnico da Fase 7.2.1.

Esta fase é exclusivamente de segurança/autorização e não altera comportamento funcional dos use cases.

## 2. Contexto da Fase 7.2

A Fase 7.2 trata o hardening operacional mínimo da vertical `Attendance`, reduzindo o risco de endpoints protegidos apenas por autenticação genérica e preparando a vertical para expansão clínica/operacional posterior.

## 3. Referência à Fase 7.2.1

A Fase 7.2.1 definiu a matriz técnica de autorização granular de `Attendance`, incluindo permissões esperadas, policies por operação e perfis autorizados/negados para leitura, criação, fechamento e cancelamento.

Documento de referência:

- `docs/clinical-core/PHASE_07_02_01_ATTENDANCE_AUTHORIZATION_PLANNING.md`

## 4. Referência à Fase 7.2.2

A Fase 7.2.2 implementou:

- `AttendancePermissions`;
- `AttendancePolicies`;
- `AttendanceAuthorization`;
- registro de `AddAttendancePolicies()` no bootstrap da API;
- policies por action no `AttendancesController`.

Documento de referência:

- `docs/clinical-core/PHASE_07_02_02_ATTENDANCE_AUTHORIZATION_IMPLEMENTATION.md`

## 5. Matriz testada

| Profile | Read | Create | Close | Cancel |
| --- | --- | --- | --- | --- |
| Admin | Permitido | Permitido | Permitido | Permitido |
| Veterinarian | Permitido | Permitido | Permitido | Permitido |
| Assistant | Permitido | Permitido | Negado | Negado |
| Reception | Permitido | Permitido | Negado | Permitido |
| ReadOnly | Negado | Negado | Negado | Negado |
| Sem `togo:profile` | Negado | Negado | Negado | Negado |
| `togo:profile` vazio | Negado | Negado | Negado | Negado |
| `togo:profile` inválido | Negado | Negado | Negado | Negado |

Também foi testado que o casing do profile é normalizado conforme o comportamento atual de `UserProfiles.Normalize`.

## 6. Arquivos de teste criados

- `backend/src/Togo.Application.Tests/Security/AttendancePermissionsTests.cs`
- `backend/src/Togo.Api.Tests/Security/AttendancePoliciesTests.cs`
- `backend/src/Togo.Api.Tests/Security/AttendanceAuthorizationTests.cs`

## 7. Arquivos de teste alterados

- `backend/src/Togo.Api.Tests/Controllers/AttendancesControllerTests.cs`

A alteração adiciona testes por reflection para confirmar `[Authorize]` no controller e policies específicas nas actions atuais, sem alterar os testes funcionais existentes.

## 8. Cobertura de permissões

`AttendancePermissionsTests` valida que:

- `AttendancePermissions.Read` é `Attendance.Read`;
- `AttendancePermissions.Create` é `Attendance.Create`;
- `AttendancePermissions.Close` é `Attendance.Close`;
- `AttendancePermissions.Cancel` é `Attendance.Cancel`;
- todas as permissões são não vazias;
- todas as permissões são únicas.

## 9. Cobertura de policies

`AttendancePoliciesTests` valida que:

- `AttendancePolicies.Read` aponta para `AttendancePermissions.Read`;
- `AttendancePolicies.Create` aponta para `AttendancePermissions.Create`;
- `AttendancePolicies.Close` aponta para `AttendancePermissions.Close`;
- `AttendancePolicies.Cancel` aponta para `AttendancePermissions.Cancel`;
- todas as policies são não vazias;
- todas as policies são únicas.

## 10. Cobertura de matriz por profile

`AttendanceAuthorizationTests` valida a matriz completa implementada em `AttendanceAuthorization.HasPermission`, cobrindo:

- perfis autorizados por operação;
- perfis negados por operação;
- `ReadOnly` negado em todas as operações;
- usuário autenticado sem claim `togo:profile`;
- claim `togo:profile` vazia;
- claim `togo:profile` inválida;
- normalização de casing do profile;
- permissão desconhecida negada.

## 11. Cobertura de attributes no controller

`AttendancesControllerTests` passa a validar que:

- `AttendancesController` mantém `[Authorize]` no nível da classe;
- `List` exige `AttendancePolicies.Read`;
- `GetById` exige `AttendancePolicies.Read`;
- `Create` exige `AttendancePolicies.Create`;
- `Close` exige `AttendancePolicies.Close`;
- `Cancel` exige `AttendancePolicies.Cancel`.

## 12. Cobertura ou limitação de 401/403 HTTP real

Nesta fase, a cobertura de autorização foi feita pelo padrão unitário/reflection já usado para o hardening de `MedicalRecord` e pela avaliação direta de `AttendanceAuthorization.HasPermission`.

A infraestrutura atual de testes de `Attendance` é baseada em testes diretos de controller/use cases e não possui, para essa vertical, um pipeline HTTP integrado com autenticação fake/JWT reutilizável para validar 401/403 reais sem ampliar significativamente o escopo.

Assim:

- cenários equivalentes a 403 foram cobertos por `HasPermission` para profiles negados, ausência de claim, claim vazia e claim inválida;
- cenários equivalentes a exigência de autenticação/policy foram cobertos por reflection dos atributos `[Authorize]`;
- testes HTTP completos de 401/403 para `Attendance` ficam recomendados para evidência/fase futura de integração de API, sem criação de token real ou uso de secret real nesta fase.

## 13. Confirmação de ausência de mudança de regra de negócio

Nenhuma regra funcional de `Attendance` foi alterada. Use cases, validators, repositories, contratos, entidades de domínio e fluxo de ciclo de vida permanecem inalterados.

## 14. Confirmação de ausência de endpoint novo

Nenhum endpoint novo foi criado. As rotas existentes de `AttendancesController` permanecem as mesmas.

## 15. Confirmação de ausência de migration/schema

Nenhuma migration foi criada e não houve alteração de schema, `AppDbContext` ou configurações EF.

## 16. Riscos remanescentes

- A validação HTTP real de 401/403 da vertical `Attendance` ainda depende de infraestrutura de integração com autenticação fake/JWT testável.
- A cobertura atual garante a matriz e os atributos, mas não exercita o middleware de autenticação/autorização ponta a ponta para esses endpoints.

## 17. Fora do escopo

Permaneceram fora do escopo:

- novas permissões ou profiles;
- novos endpoints;
- autoria/auditoria;
- Soft Delete;
- retenção;
- evolução clínica;
- prescrição;
- evidência manual final;
- alterações de JWT, banco, schema, migrations, frontend, Docker, Redis, RabbitMQ ou Kubernetes.

## 18. Critérios de aceite

Critérios atendidos:

- testes de `AttendancePermissions` criados;
- testes de `AttendancePolicies` criados;
- testes de `AttendanceAuthorization` cobrindo a matriz completa;
- cobertura de profile ausente, vazio e inválido;
- cobertura de attributes/policies do `AttendancesController`;
- testes funcionais existentes preservados;
- nenhuma regra de negócio alterada;
- nenhum endpoint novo criado;
- nenhuma migration/schema criada;
- documentação da fase criada.

## 19. Próxima fase recomendada

Próxima fase recomendada:

```text
Fase 7.2.4 — Evidências e fechamento do hardening mínimo de autorização de Attendance
```
