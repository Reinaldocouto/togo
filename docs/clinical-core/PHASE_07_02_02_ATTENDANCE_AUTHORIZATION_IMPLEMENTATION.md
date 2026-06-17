# Fase 7.2.2 — Implementação de autorização granular de Attendance

## 1. Objetivo

Implementar autorização granular para a vertical `Attendance`, substituindo a proteção efetiva exclusivamente genérica por `[Authorize]` por policies por operação nos endpoints existentes.

## 2. Contexto da Fase 7.2

A Fase 7.2 faz o hardening operacional mínimo de `Attendance` dentro da expansão clínica e operacional pós-hardening de `MedicalRecord`.

Antes desta fase, `AttendancesController` exigia autenticação, mas não diferenciava permissões por operação.

## 3. Referência à Fase 7.2.1

A implementação segue o planejamento técnico documentado em `docs/clinical-core/PHASE_07_02_01_ATTENDANCE_AUTHORIZATION_PLANNING.md`, que definiu permissões, policies e matriz por profile para `Attendance` usando o padrão já aplicado em `MedicalRecord`.

## 4. Matriz implementada

| Operação | Endpoint | Policy | Perfis autorizados |
| --- | --- | --- | --- |
| `ListAttendances` | `GET /api/attendances` | `Attendance.Read` | `Admin`, `Veterinarian`, `Assistant`, `Reception` |
| `GetAttendanceById` | `GET /api/attendances/{id}` | `Attendance.Read` | `Admin`, `Veterinarian`, `Assistant`, `Reception` |
| `CreateAttendance` | `POST /api/attendances` | `Attendance.Create` | `Admin`, `Veterinarian`, `Assistant`, `Reception` |
| `CloseAttendance` | `PATCH /api/attendances/{id}/close` | `Attendance.Close` | `Admin`, `Veterinarian` |
| `CancelAttendance` | `PATCH /api/attendances/{id}/cancel` | `Attendance.Cancel` | `Admin`, `Veterinarian`, `Reception` |

## 5. Arquivos criados

- `backend/src/Togo.Application/Security/AttendancePermissions.cs`
- `backend/src/Togo.Api/Security/AttendancePolicies.cs`
- `backend/src/Togo.Api/Security/AttendanceAuthorization.cs`
- `docs/clinical-core/PHASE_07_02_02_ATTENDANCE_AUTHORIZATION_IMPLEMENTATION.md`

## 6. Arquivos alterados

- `backend/src/Togo.Api/Program.cs`
- `backend/src/Togo.Api/Controllers/AttendancesController.cs`

## 7. Permissões criadas

As permissões centralizadas criadas foram:

- `Attendance.Read`
- `Attendance.Create`
- `Attendance.Close`
- `Attendance.Cancel`

## 8. Policies criadas

As policies usam os mesmos valores textuais das permissões:

- `Attendance.Read`
- `Attendance.Create`
- `Attendance.Close`
- `Attendance.Cancel`

## 9. Registro em `Program.cs`

`Program.cs` passou a registrar as policies de `Attendance` no mesmo bloco de autorização que mantém as policies de `MedicalRecord`.

O registro de `AddMedicalRecordPolicies()` foi preservado e `AddAttendancePolicies()` foi adicionado sem alteração na autenticação, JWT ou profiles.

## 10. Aplicação das policies no controller

`AttendancesController` mantém `[Authorize]` no controller e adiciona `[Authorize(Policy = ...)]` nas actions existentes:

- `List` usa `AttendancePolicies.Read`;
- `GetById` usa `AttendancePolicies.Read`;
- `Create` usa `AttendancePolicies.Create`;
- `Close` usa `AttendancePolicies.Close`;
- `Cancel` usa `AttendancePolicies.Cancel`.

## 11. Perfis autorizados por operação

- Leitura/listagem: `Admin`, `Veterinarian`, `Assistant`, `Reception`.
- Criação: `Admin`, `Veterinarian`, `Assistant`, `Reception`.
- Fechamento: `Admin`, `Veterinarian`.
- Cancelamento: `Admin`, `Veterinarian`, `Reception`.

## 12. Perfis negados por operação

São negados:

- `ReadOnly` em todas as operações;
- usuário autenticado sem claim `togo:profile`;
- profile vazio;
- profile inválido;
- qualquer profile não suportado por `UserProfiles`.

Além disso:

- `Assistant` não pode fechar nem cancelar atendimento;
- `Reception` não pode fechar atendimento.

## 13. Decisões pendentes de produto/negócio

A matriz implementa exatamente a decisão planejada na Fase 7.2.1.

Permanecem como validação futura com produto/negócio:

- permissão de `Assistant` para criar atendimento;
- permissão de `Reception` para cancelar atendimento.

## 14. Confirmação de ausência de endpoint novo

Nenhum endpoint novo foi criado nesta fase.

## 15. Confirmação de ausência de migration/schema

Nenhuma migration foi criada e nenhum schema foi alterado.

## 16. Confirmação de ausência de alteração de regra de negócio

Nenhuma regra de negócio, use case, contrato, entidade de domínio ou repositório foi alterado.

## 17. Impacto em testes

A fase não adiciona a suíte ampla de testes 401/403. Essa cobertura fica para a Fase 7.2.3.

Os testes existentes continuam exercitando o comportamento funcional dos endpoints e a chamada direta ao controller não é impactada pelos attributes de autorização.

## 18. Riscos remanescentes

- Falta a bateria dedicada de testes de autorização granular por profile e endpoint.
- As decisões de produto/negócio sobre `Assistant` criar e `Reception` cancelar ainda precisam de validação futura.

## 19. Fora do escopo

Ficaram fora do escopo:

- nova suíte extensa de testes 401/403;
- endpoint novo;
- evolução clínica;
- prescrição;
- nova entidade;
- migration;
- alteração de schema;
- alteração de JWT;
- criação de profile novo;
- frontend;
- Docker, Redis, RabbitMQ e Kubernetes.

## 20. Critérios de aceite

A implementação atende aos critérios da fase ao criar permissões, policies, matriz por profile, registro em `Program.cs`, aplicação por operação em `AttendancesController`, preservação dos endpoints atuais e ausência de migration/schema ou alteração de regra de negócio.

## 21. Próxima fase recomendada

Fase 7.2.3 — Testes de autorização granular de Attendance.
