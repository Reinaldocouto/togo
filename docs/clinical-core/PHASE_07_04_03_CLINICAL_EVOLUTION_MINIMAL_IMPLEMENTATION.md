# Fase 7.4.3 — Implementação mínima de ClinicalEvolution vinculada a Attendance

## 1. Objetivo

Implementar a superfície mínima de criação e listagem de `ClinicalEvolution` vinculada a um `AttendanceId` específico, com autorização granular e sem expandir para autoria, auditoria, update, delete ou listagem global.

## 2. Contexto da Fase 7.4

A Fase 7.4 integra evoluções clínicas ao episódio operacional de atendimento. `ClinicalEvolution` pertence ao episódio `Attendance`; `MedicalRecord` permanece como memória clínica longitudinal e não foi alterado.

## 3. Referências às Fases 7.4.1 e 7.4.2

Esta implementação segue o planejamento de `PHASE_07_04_01_CLINICAL_EVOLUTION_ATTENDANCE_PLANNING.md` e reutiliza os contratos, permissões, policies e audit action constants preparados em `PHASE_07_04_02_CLINICAL_EVOLUTION_CONTRACTS.md`.

## 4. Endpoints criados

- `GET /api/attendances/{attendanceId}/clinical-evolutions`
- `POST /api/attendances/{attendanceId}/clinical-evolutions`

Não foi criado endpoint de listagem global.

## 5. Policies aplicadas

- GET usa `ClinicalEvolution.Read`.
- POST usa `ClinicalEvolution.Create`.
- O controller exige `[Authorize]` em nível de classe.

## 6. Use cases criados

- `CreateClinicalEvolutionUseCase`
- `ListClinicalEvolutionsByAttendanceUseCase`

## 7. Repository criado

- Interface: `IClinicalEvolutionRepository`
- Implementação EF Core: `ClinicalEvolutionRepository`

A interface expõe apenas `AddAsync` e `ListByAttendanceIdAsync`. Não há `ListAsync` global, update ou delete.

## 8. Contracts utilizados

- `CreateClinicalEvolutionRequest`
- `ClinicalEvolutionResponse`
- `ClinicalEvolutionListItemResponse`

## 9. Regras de validação

Criação valida `attendanceId` da rota, `request.AttendanceId`, consistência rota/body, existência de `Attendance`, status aberto, `RegisteredAt` não default e `Text` não vazio. Listagem valida `attendanceId`, existência de `Attendance` e retorna lista vazia quando não há evoluções.

## 10. Criação apenas para Attendance.Open

A criação é permitida somente quando `Attendance.Status == Open`. Atendimentos fechados ou cancelados retornam conflito.

## 11. Text não retornado em listagem

`ClinicalEvolutionListItemResponse` não possui `Text`, reduzindo exposição de conteúdo clínico sensível em listagens.

## 12. Ausência de autoria nesta fase

Nenhum `ICurrentUserService` foi usado nos use cases de `ClinicalEvolution` e nenhum campo de autoria foi persistido nesta fase.

## 13. Ausência de AuditLog nesta fase

Nenhum `IClinicalAuditLogWriter` foi usado nos use cases de `ClinicalEvolution`. As constants de audit action permanecem apenas como base técnica preparada na fase anterior.

## 14. Ausência de migration/schema

Nenhuma migration ou alteração de schema foi criada, pois a tabela `ClinicalEvolutions` e seu vínculo com `Attendances` já existiam.

## 15. Testes criados/alterados

Foram adicionados testes de domínio para `ClinicalEvolution`, testes de aplicação para criação/listagem, testes de infraestrutura para repository e testes de API por reflexão para controller, rotas e policies.

## 16. Riscos remanescentes

- Criação ainda não registra autoria persistida.
- Criação ainda não grava audit log clínico.
- Não há endpoint de detalhe dedicado além da resposta de criação.

## 17. Fora do escopo

Update, delete, soft delete, retificação, assinatura clínica, anexos, autoria persistida, audit log, migration/schema, alterações em `MedicalRecord`, `Attendance` além de consulta/validação, `Prescription`, frontend e infraestrutura externa.

## 18. Critérios de aceite

A fase atende aos critérios ao criar repository, use cases, controller, DI, policies, validações de `Attendance`, listagem sem `Text`, ausência de listagem global/update/delete/autoria/audit log/migration e testes cobrindo domínio, aplicação, infraestrutura e API.

## 19. Próxima fase recomendada

Fase 7.4.4 — Autoria e AuditLog mínimos de ClinicalEvolution.
