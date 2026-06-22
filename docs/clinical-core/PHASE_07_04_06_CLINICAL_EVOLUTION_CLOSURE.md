# Fase 7.4.6 — Encerramento da trilha ClinicalEvolution

## 1. Objetivo

A Fase 7.4.6 encerra formalmente a trilha `ClinicalEvolution` dentro da Fase 7, consolidando planejamento, contratos, autorização, superfície mínima, vínculo com `Attendance`, autoria, AuditLog, testes, evidências, limites, riscos remanescentes, decisão final e próxima fase recomendada.

Esta fase é exclusivamente documental. Ela não implementa código, testes, migrations, schema, endpoints, policies, repositories, use cases, contracts, entidades, frontend ou infraestrutura.

## 2. Contexto

A Fase 7 é a expansão clínica e operacional pós-hardening da vertical `MedicalRecord`. A Fase 6 consolidou `MedicalRecord` como memória clínica longitudinal mais madura, com autorização granular, autoria, AuditLog mínimo, persistência conservadora e governança documental.

Antes da integração de `ClinicalEvolution`, `Attendance` já havia sido endurecido em autorização granular, autoria e auditoria mínima. Assim, `Attendance` passou a ser um eixo operacional mais seguro para receber registros clínicos associados a um episódio de atendimento.

`ClinicalEvolution` agora foi integrada a `Attendance` por uma superfície mínima e orientada por episódio. `ClinicalEvolution.Text` é dado clínico sensível e, por isso, a trilha buscou exposição mínima, ausência de listagem global, contracts reduzidos para listagem e rastreabilidade básica de criação.

## 3. Subfases consolidadas

- 7.4.1 — Planejamento técnico da integração ClinicalEvolution com Attendance.
- 7.4.2 — Contratos/base técnica para ClinicalEvolution vinculada a Attendance.
- 7.4.3 — Implementação mínima de ClinicalEvolution vinculada a Attendance.
- 7.4.4 — Autoria e AuditLog mínimos de ClinicalEvolution.
- 7.4.5 — Testes e evidências da integração ClinicalEvolution.
- 7.4.6 — Encerramento da trilha ClinicalEvolution.

## 4. Artefatos documentais consolidados

Documentos da trilha 7.4 consolidados:

- `docs/clinical-core/PHASE_07_04_01_CLINICAL_EVOLUTION_ATTENDANCE_PLANNING.md`.
- `docs/clinical-core/PHASE_07_04_02_CLINICAL_EVOLUTION_CONTRACTS.md`.
- `docs/clinical-core/PHASE_07_04_03_CLINICAL_EVOLUTION_MINIMAL_IMPLEMENTATION.md`.
- `docs/clinical-core/PHASE_07_04_04_CLINICAL_EVOLUTION_AUTHORSHIP_AUDIT_IMPLEMENTATION.md`.
- `docs/clinical-core/PHASE_07_04_05_CLINICAL_EVOLUTION_EVIDENCES.md`.
- `docs/clinical-core/PHASE_07_04_06_CLINICAL_EVOLUTION_CLOSURE.md`.

Documentos de contexto consultados:

- `docs/clinical-core/PHASE_07_03_06_ATTENDANCE_AUTHORSHIP_AUDIT_CLOSURE.md`.
- `docs/clinical-core/PHASE_07_02_04_ATTENDANCE_AUTHORIZATION_CLOSURE.md`.
- `docs/clinical-core/PHASE_07_01_ATTENDANCE_TECHNICAL_PLANNING.md`.
- `docs/clinical-core/PHASE_07_00_CLINICAL_OPERATIONAL_EXPANSION_PLANNING.md`.
- `docs/clinical-core/PHASE_06_06_05_MEDICAL_RECORD_PHASE_06_CLOSURE.md`.

## 5. Artefatos técnicos consolidados

Arquivos técnicos principais consolidados:

- `backend/src/Togo.Domain/Entities/ClinicalEvolution.cs`.
- `backend/src/Togo.Application/ClinicalEvolutions/Contracts/CreateClinicalEvolutionRequest.cs`.
- `backend/src/Togo.Application/ClinicalEvolutions/Contracts/ClinicalEvolutionResponse.cs`.
- `backend/src/Togo.Application/ClinicalEvolutions/Contracts/ClinicalEvolutionListItemResponse.cs`.
- `backend/src/Togo.Application/ClinicalEvolutions/UseCases/CreateClinicalEvolutionUseCase.cs`.
- `backend/src/Togo.Application/ClinicalEvolutions/UseCases/ListClinicalEvolutionsByAttendanceUseCase.cs`.
- `backend/src/Togo.Application/ClinicalEvolutions/Repositories/IClinicalEvolutionRepository.cs`.
- `backend/src/Togo.Infrastructure/Repositories/ClinicalEvolutionRepository.cs`.
- `backend/src/Togo.Api/Controllers/ClinicalEvolutionsController.cs`.
- `backend/src/Togo.Application/Security/ClinicalEvolutionPermissions.cs`.
- `backend/src/Togo.Api/Security/ClinicalEvolutionPolicies.cs`.
- `backend/src/Togo.Api/Security/ClinicalEvolutionAuthorization.cs`.
- `backend/src/Togo.Application/Auditing/ClinicalEvolutionAuditActions.cs`.
- `backend/src/Togo.Infrastructure/Migrations/20260621120000_AddClinicalEvolutionAuthorship.cs`.

Arquivos técnicos de contexto consultados:

- `backend/src/Togo.Domain/Entities/Attendance.cs`.

Todos os arquivos técnicos principais e testes obrigatórios citados para esta fase estavam presentes no repositório durante a inspeção.

## 6. Relação consolidada com Attendance

- `ClinicalEvolution` pertence ao episódio `Attendance` por `AttendanceId`.
- Toda criação pública de `ClinicalEvolution` é feita por rota contendo `AttendanceId`.
- Toda listagem pública de `ClinicalEvolution` é feita por rota contendo `AttendanceId`.
- Não há listagem global de evoluções clínicas.
- A criação exige `Attendance` existente.
- A criação exige `Attendance` aberto (`AttendanceStatus.Open`).
- A listagem exige `Attendance` existente.
- O repository lista evoluções clínicas por `AttendanceId` via `ListByAttendanceIdAsync`.
- A interface do repository expõe apenas adição e listagem por atendimento, sem update, delete ou listagem global.

## 7. Relação consolidada com MedicalRecord

`MedicalRecord` permanece memória longitudinal e não foi inflado com coleção direta de `ClinicalEvolution`.

A evolução clínica compõe o histórico clínico por meio do episódio `Attendance`, que por sua vez está associado ao paciente. A trilha 7.4 não criou vínculo direto novo entre `ClinicalEvolution` e `MedicalRecord` e não alterou `MedicalRecord`.

## 8. Contracts consolidados

Contracts consolidados:

- `CreateClinicalEvolutionRequest`.
- `ClinicalEvolutionResponse`.
- `ClinicalEvolutionListItemResponse`.

`CreateClinicalEvolutionRequest` contém os dados mínimos para criação vinculada ao atendimento: `AttendanceId`, `RegisteredAt`, `Type` e `Text`.

`ClinicalEvolutionResponse` é usado como resposta de criação e contém `Text`, pois representa o resultado direto do POST e não uma listagem ampla.

`ClinicalEvolutionListItemResponse` contém `Id`, `AttendanceId`, `RegisteredAt` e `Type`, mas não expõe `Text`. Essa minimização reduz exposição de conteúdo clínico sensível em listagens.

## 9. Autorização consolidada

Permissões consolidadas:

- `ClinicalEvolution.Read`.
- `ClinicalEvolution.Create`.
- `ClinicalEvolution.Update`.

Não foi criada permissão `ClinicalEvolution.Delete`.

A matriz por profile foi consolidada de forma conservadora:

| Profile | Read | Create | Update |
| --- | ---: | ---: | ---: |
| `Admin` | Sim | Sim | Sim |
| `Veterinarian` | Sim | Sim | Sim |
| `Assistant` | Sim | Não | Não |
| `Reception` | Não | Não | Não |
| `ReadOnly` | Não | Não | Não |
| Sem profile/profile inválido | Não | Não | Não |

Aplicação nos endpoints atuais:

- `GET /api/attendances/{attendanceId}/clinical-evolutions` usa `ClinicalEvolution.Read`.
- `POST /api/attendances/{attendanceId}/clinical-evolutions` usa `ClinicalEvolution.Create`.
- `ClinicalEvolution.Update` existe como permissão/policy preparada, mas update não possui endpoint ainda.

## 10. Endpoints consolidados

Endpoints existentes:

```http
GET /api/attendances/{attendanceId}/clinical-evolutions
POST /api/attendances/{attendanceId}/clinical-evolutions
```

Não há:

```http
GET /api/clinical-evolutions
PUT /api/...
PATCH /api/...
DELETE /api/...
```

A ausência de endpoint global, update e delete preserva a superfície mínima definida para a trilha 7.4.

## 11. Autoria mínima consolidada

Campos persistidos em `ClinicalEvolution`:

```text
CreatedByUserId
CreatedAt
UpdatedByUserId
UpdatedAt
```

Semântica consolidada:

- `RegisteredAt`: timestamp clínico/operacional da evolução, informado no request.
- `CreatedAt`: timestamp técnico de persistência/criação no sistema.
- `CreatedByUserId`: usuário autenticado responsável pela criação.
- `UpdatedAt`/`UpdatedByUserId`: campos preparados para alteração futura, inicializados com os dados da criação.

`RegisteredByUserId` não foi criado. A trilha manteve o padrão transversal já usado em `Attendance` e `MedicalRecord`, no qual o usuário autenticado é persistido em `CreatedByUserId` para o registro inicial.

## 12. AuditLog consolidado

- `ClinicalEvolution.Created` é gravado no sucesso da criação.
- O fluxo usa `ClinicalAuditEvent`.
- O fluxo usa `IClinicalAuditLogWriter`.
- O fluxo usa `ClinicalEvolutionAuditActions.Created`.
- A metadata contém apenas `AttendanceId` e `Type`.
- A metadata não inclui `Text`.

`ClinicalEvolution.Updated` existe como constante preparada, mas não há fluxo real de update ainda. Não existem fluxos reais de `ClinicalEvolution.Deleted`, `ClinicalEvolution.Read` ou `ClinicalEvolution.AccessDenied` nesta trilha.

## 13. Metadata consolidada

Metadata permitida para `ClinicalEvolution.Created`:

```json
{
  "AttendanceId": 123,
  "Type": "ClinicalNote"
}
```

Metadata proibida:

- `Text`.
- Payload completo.
- Dados de paciente.
- Dados de tutor.
- Dados de prontuário.
- Evolução textual.
- Prescrição.
- Observações clínicas completas.

## 14. Evidências de testes

Evidências consolidadas:

- Domínio: criação válida, validação de `AttendanceId`, `RegisteredAt`, `Text`, autoria inicial e preparação de `UpdateText`.
- Contracts: `ClinicalEvolutionListItemResponse` não expõe `Text`; response de criação expõe `Text` apenas no retorno direto do POST.
- Autorização: permissões, policies e matriz por profile cobertas por testes.
- Criação: valida rota/body, existência de `Attendance`, status aberto, `RegisteredAt`, `Type`, `Text`, autoria e AuditLog.
- Listagem: exige `Attendance` existente, lista por `AttendanceId` e retorna list item sem `Text`.
- Autoria: criação usa `ICurrentUserService` e persiste `CreatedByUserId`, `CreatedAt`, `UpdatedByUserId` e `UpdatedAt`.
- AuditLog: criação grava `ClinicalEvolution.Created` com `ClinicalAuditEvent`, `IClinicalAuditLogWriter` e metadata mínima sem `Text`.
- Infraestrutura: repository adiciona e lista por `AttendanceId`, ordenando por `RegisteredAt` e `Id`.
- API/reflection: controller expõe apenas GET e POST por atendimento e não expõe PUT, PATCH ou DELETE.
- Ausência de update/delete: inspeções e testes confirmam que não há endpoints públicos de alteração ou exclusão de `ClinicalEvolution`.
- Ausência de `Text` em listagem: tests por contract/reflection confirmam que o list item não possui propriedade `Text`.
- Ausência de `Text` em AuditLog: testes verificam metadata sem campo `Text` e sem conteúdo clínico sensível.

Testes principais consolidados:

- `backend/src/Togo.Domain.Tests/ClinicalEvolutionTests.cs`.
- `backend/src/Togo.Application.Tests/ClinicalEvolutions/CreateClinicalEvolutionUseCaseTests.cs`.
- `backend/src/Togo.Application.Tests/ClinicalEvolutions/ListClinicalEvolutionsByAttendanceUseCaseTests.cs`.
- `backend/src/Togo.Application.Tests/ClinicalEvolutions/Fakes/FakeClinicalEvolutionRepository.cs`.
- `backend/src/Togo.Infrastructure.Tests/Repositories/ClinicalEvolutionRepositoryTests.cs`.
- `backend/src/Togo.Api.Tests/Controllers/ClinicalEvolutionsControllerTests.cs`.
- `backend/src/Togo.Application.Tests/Security/ClinicalEvolutionPermissionsTests.cs`.
- `backend/src/Togo.Api.Tests/Security/ClinicalEvolutionPoliciesTests.cs`.
- `backend/src/Togo.Api.Tests/Security/ClinicalEvolutionAuthorizationTests.cs`.
- `backend/src/Togo.Application.Tests/Auditing/ClinicalEvolutionAuditActionsTests.cs`.

## 15. Limitações conhecidas

- Não há update/retificação.
- Não há delete/soft delete.
- Não há assinatura clínica.
- Não há anexos.
- Não há auditoria de leitura.
- Não há auditoria de acesso negado.
- `ClinicalEvolution.Updated` ainda não possui fluxo real.
- Não há endpoint de detalhe individual.
- Registros legados podem conter autoria técnica transitória da migration.
- Estratégia de transação/outbox/retry não foi redesenhada.

## 16. Riscos remanescentes

- Futuras alterações de texto clínico precisam de retificação, autoria e auditoria rigorosas.
- Auditoria de leitura pode gerar ruído operacional e deve ser planejada antes de implementação.
- Exposição global de texto clínico deve continuar proibida sem decisão formal de produto, segurança, compliance e arquitetura.
- Saneamento de legados pode ser necessário se houver dados reais com autoria técnica transitória.
- Eventual assinatura clínica exige desenho próprio, incluindo semântica de assinatura, bloqueio, retificação e trilha de auditoria.

## 17. Fora do escopo da trilha 7.4

Ficaram fora da trilha 7.4:

- update;
- delete;
- soft delete;
- retificação;
- assinatura clínica;
- anexos;
- listagem global;
- auditoria de leitura/acesso negado;
- frontend;
- integrações com prescrição;
- mudanças em `MedicalRecord`.

## 18. Critérios finais de aceite

A trilha é considerada encerrada porque:

- documentos 7.4.1 a 7.4.5 foram consolidados;
- endpoints mínimos foram documentados;
- vínculo com `Attendance` foi documentado;
- separação de `MedicalRecord` foi documentada;
- autoria mínima foi documentada;
- AuditLog mínimo foi documentado;
- metadata permitida/proibida foi documentada;
- limitações e riscos foram registrados;
- próxima fase foi recomendada;
- nenhuma implementação nova foi feita nesta fase;
- somente documentação foi alterada;
- `git diff --check` passou nas validações finais.

## 19. Decisão final da Fase 7.4

A Fase 7.4 fica encerrada como a trilha de integração segura de `ClinicalEvolution` com `Attendance`, oferecendo criação e listagem por episódio, autorização granular, autoria técnica persistida e AuditLog mínimo de criação, sem listagem global, sem update/delete, sem exposição de texto clínico em listagens e sem texto clínico em metadata de auditoria.

O encerramento da Fase 7.4 não significa que `ClinicalEvolution` esteja completa em termos de retificação clínica, assinatura, anexos, auditoria de leitura/acesso negado ou fluxos de update/delete; esses temas permanecem para fases futuras.

## 20. Próxima fase recomendada

Próxima frente recomendada:

```text
Fase 7.5 — Integração segura de Prescription com Attendance
```

Fracionamento sugerido:

- 7.5.1 — Planejamento técnico da integração Prescription com Attendance.
- 7.5.2 — Contratos/base técnica para Prescription vinculada a Attendance.
- 7.5.3 — Implementação mínima de Prescription vinculada a Attendance.
- 7.5.4 — Autoria e AuditLog mínimos de Prescription.
- 7.5.5 — Testes e evidências da integração Prescription.
- 7.5.6 — Encerramento da trilha Prescription.

Nada da Fase 7.5 foi implementado nesta fase.
