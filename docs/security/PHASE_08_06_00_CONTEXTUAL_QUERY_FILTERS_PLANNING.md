# Fase 8.6.0 — Planejamento técnico dos filtros de consulta por contexto clínico

## 1. Objetivo da Fase 8.6

A Fase 8.6 tem como objetivo impedir que consultas, listagens, validações e operações clínicas retornem ou manipulem dados fora da clínica ativa e autorizada para o usuário corrente.

As fases anteriores criaram a base estrutural para isso, mas ainda não fecham o isolamento operacional:

- `ClinicId` persistido nas entidades clínicas é necessário, mas não basta: ele só cria a coluna/atributo de escopo.
- `ICurrentClinicalContext` é necessário, mas não basta: ele resolve a clínica ativa informada na requisição, mas não altera consultas automaticamente.
- `UserClinicAccess` é necessário, mas não basta: ele registra vínculos usuário-clínica, mas não filtra os dados retornados.
- `IClinicalContextAuthorizationService` é necessário, mas não basta: ele valida acesso ao contexto, mas as queries precisam usar esse contexto.

Conclusão técnica: as queries clínicas sensíveis precisam respeitar explicitamente o `ClinicId` autorizado antes de retornar, atualizar, remover ou validar dados clínicos.

## 2. Problema que esta fase resolve

O risco atual é que um usuário possa:

1. estar autenticado;
2. informar um header transitório `X-Clinic-Id` válido;
3. possuir vínculo ativo com uma clínica via `UserClinicAccess`;
4. ainda assim executar consultas/listagens que retornem dados de outra clínica, caso o método de repository/use case busque por `Id`, liste todos os registros ou valide existência sem aplicar `ClinicId`.

Esse risco é especialmente relevante em operações por identificador global (`GetByIdAsync(id)`), listagens sem parâmetros (`ListAsync()`), validações de existência/unicidade e consultas por entidades filhas que herdam escopo por relacionamento.

## 3. Princípio central

> Toda consulta clínica sensível deve obedecer ao `ClinicId` autorizado, exceto quando houver decisão explícita e documentada de consulta administrativa/global.

Implicações:

- listagens clínicas não devem ser globais por padrão;
- buscas por ID devem considerar `Id + ClinicId` sempre que possível;
- validações usadas em fluxos clínicos protegidos devem ser escopadas;
- exceções administrativas/globais precisam ser nomeadas, documentadas e testadas separadamente.

## 4. Estratégia recomendada

### Opção A — filtros explícitos nos repositories/use cases

Nesta abordagem, cada operação sensível recebe o `clinicId` autorizado ou usa serviços de contexto/autorização na camada de aplicação antes de chamar o repository.

Vantagens:

- filtro visível no contrato ou no fluxo do use case;
- menor risco de efeitos colaterais ocultos;
- mais fácil de revisar incrementalmente por módulo;
- permite testes unitários e de integração por operação;
- combina melhor com o estado atual do TOGO, em que os repositories têm métodos específicos e muitos use cases já organizam regras de negócio.

Desvantagens:

- exige alteração coordenada de assinaturas e chamadas;
- pode haver esquecimento em algum método se não houver checklist/testes;
- gera trabalho repetitivo até existirem padrões consolidados.

### Opção B — EF Core Global Query Filters

Nesta abordagem, filtros globais seriam configurados no `DbContext` para entidades com `ClinicId`.

Vantagens:

- reduz repetição em queries simples;
- diminui chance de listagens acidentalmente globais;
- centraliza parte do isolamento por entidade.

Riscos nesta fase:

- comportamento menos explícito e mais difícil de depurar;
- pode afetar includes, projections, testes de persistência e seeds;
- pode interferir em consultas administrativas e rotinas de manutenção;
- exige modelagem cuidadosa para entidades que herdam escopo (`Pet` via `Patient`, `PrescriptionItem` via `Prescription`);
- pode mascarar lacunas de autorização contextual se usado cedo demais.

### Decisão recomendada para o TOGO

Começar com filtros explícitos e incrementais em use cases/repositories clínicos sensíveis.

Não usar EF Core Global Query Filters agora. Global filters podem ser reavaliados em fase posterior, quando os limites entre consultas clínicas, administrativas e globais estiverem maduros e cobertos por testes.

## 5. Entidades com `ClinicId` direto

Estas entidades devem ser filtradas diretamente por `ClinicId` nas próximas subfases:

| Entidade | Observação de filtro |
| --- | --- |
| `Tutor` | Filtrar por `Tutor.ClinicId`. |
| `Patient` | Filtrar por `Patient.ClinicId`. |
| `Attendance` | Filtrar por `Attendance.ClinicId`. |
| `MedicalRecord` | Filtrar por `MedicalRecord.ClinicId`, preservando regra de soft delete. |
| `ClinicalEvolution` | Filtrar por `ClinicalEvolution.ClinicId` e/ou validar atendimento no mesmo contexto. |
| `Prescription` | Filtrar por `Prescription.ClinicId` e/ou validar atendimento no mesmo contexto. |

## 6. Entidades sem `ClinicId` direto

| Entidade/escopo | Estratégia de escopo |
| --- | --- |
| `Pet` | Herda escopo de `Patient`. Queries devem juntar/consultar `Patient.ClinicId`. |
| `PrescriptionItem` | Herda escopo de `Prescription`. Queries devem partir de prescription autorizada ou juntar por `Prescription.ClinicId`. |
| `ClinicUnit` | Ainda não é contexto obrigatório para autorização clínica nesta fase. Não introduzir `ClinicUnitId` na Fase 8.6. |
| `Organization` | Não é filtro operacional clínico primário nesta fase. Pode orientar governança futura, mas não substitui `ClinicId`. |
| `ClinicalAuditLog` | Não possui filtro operacional definido nesta subfase; mapear impacto futuro para auditoria contextual e leitura auditável. |

## 7. Consultas, use cases e repositories sensíveis mapeados

O mapeamento abaixo foi feito a partir dos contratos e implementações atuais de aplicação/infraestrutura. Ele não altera código produtivo nesta subfase.

### Tutors

Pontos atuais sensíveis:

- `CreateTutorUseCase` cria `Tutor` com `ClinicId` recebido na request e valida documento por clínica.
- `GetTutorByIdUseCase`, `UpdateTutorUseCase` e `DeleteTutorUseCase` usam busca por ID global no repository.
- `ListTutorsUseCase` lista tutores sem filtro contextual.
- `TutorDocumentUniquenessValidator` já recebe `clinicId`, mas a origem/autorização do `clinicId` precisa ser consolidada.
- `ITutorRepository.GetByIdAsync` e `ITutorRepository.ListAsync` não expressam `clinicId` no contrato atual.

### Patients/Pets

Pontos atuais sensíveis:

- `CreatePetUseCase` cria `Patient` com `ClinicId` da request e valida vínculo tutor-clínica.
- `GetPetByIdUseCase`, `UpdatePetUseCase` e `DeletePetUseCase` operam por `patientId` sem exigir filtro contextual no contrato.
- `ListPetsUseCase` lista pets sem filtro contextual.
- `PetRepository` possui consultas que juntam `Pet` e `Patient`, o que facilita filtrar por `Patient.ClinicId` nas próximas subfases.
- `PetMicrochipUniquenessValidator` usa unicidade global de microchip; é necessário decidir se o microchip é identificador global legítimo ou se deve ser escopado por clínica.

### Attendances

Pontos atuais sensíveis:

- `CreateAttendanceUseCase` deriva/recebe escopo clínico e usa validações de paciente, número e atendimento aberto.
- `GetAttendanceByIdUseCase`, `CloseAttendanceUseCase` e `CancelAttendanceUseCase` dependem de busca por ID.
- `ListAttendancesUseCase` e listagens por paciente precisam ser escopadas por `ClinicId`.
- `AttendancePatientExistsValidator`, `AttendanceNumberUniqueValidator` e `OpenAttendanceValidator` devem ser revisados para evitar validações globais em fluxos clínicos.
- `IAttendanceRepository` expõe `GetByIdAsync`, `ListAsync`, `ListByPatientIdAsync`, `ExistsByAttendanceNumberAsync` e `HasOpenAttendanceForPatientAsync` sem `clinicId` no contrato atual.

### MedicalRecords

Pontos atuais sensíveis:

- `CreateMedicalRecordUseCase` deve garantir que paciente e prontuário pertençam à clínica autorizada.
- `GetMedicalRecordByPatientIdUseCase` deve consultar por `patientId + clinicId` ou validar escopo antes de retornar.
- `UpdateMedicalRecordUseCase` e `SoftDeleteMedicalRecordUseCase` devem buscar registro dentro do contexto autorizado.
- `MedicalRecordPatientExistsValidator`, `MedicalRecordExistsValidator` e `MedicalRecordUniquenessValidator` devem ser escopados em fluxos protegidos.
- `IMedicalRecordRepository` expõe buscas/validações por `id` e `patientId` sem `clinicId` no contrato atual.

### ClinicalEvolutions

Pontos atuais sensíveis:

- `CreateClinicalEvolutionUseCase` cria evolução ligada a atendimento; deve validar atendimento dentro da clínica autorizada antes de criar.
- `ListClinicalEvolutionsByAttendanceUseCase` lista por `attendanceId`; deve filtrar por `ClinicalEvolution.ClinicId` e/ou validar que o atendimento pertence ao `ClinicId` autorizado.
- `IClinicalEvolutionRepository.ListByAttendanceIdAsync` não expressa `clinicId` no contrato atual.

### Prescriptions

Pontos atuais sensíveis:

- `CreatePrescriptionUseCase` cria prescrição e itens ligados a atendimento; deve validar atendimento dentro da clínica autorizada.
- `ListPrescriptionsByAttendanceUseCase` lista por `attendanceId`; deve filtrar por `Prescription.ClinicId` e garantir que itens retornados pertençam a prescrições autorizadas.
- `IPrescriptionRepository.ListByAttendanceIdAsync` não expressa `clinicId` no contrato atual.

### ClinicalAuditLog

Impacto futuro:

- a escrita de auditoria clínica registra eventos, mas a Fase 8.6.0 não define leitura auditável nem auditoria de leitura;
- quando houver endpoints de consulta de auditoria, eles precisarão de escopo contextual ou autorização administrativa explícita;
- deletes/soft deletes filtrados por clínica devem preservar metadados suficientes para auditoria contextual futura.

## 8. Classificação das operações

| Módulo | Operação | Entidade base | Fonte do ClinicId | Exige CurrentClinicalContext? | Exige autorização contextual? | Tipo de filtro | Subfase recomendada | Risco |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Tutors | Criar tutor | `Tutor` | preferencialmente contexto atual; request apenas como dado transitório legado | Sim | Sim | Validar contexto e persistir `Tutor.ClinicId` autorizado | 8.6.1 | Alto: payload pode apontar clínica não autorizada. |
| Tutors | Atualizar tutor | `Tutor` | contexto atual + `Tutor.ClinicId` existente | Sim | Sim | Buscar por `Id + ClinicId`; não alterar `ClinicId` | 8.6.1 | Alto: update por ID global. |
| Tutors | Buscar tutor por ID | `Tutor` | contexto atual | Sim | Sim | `Id + ClinicId` | 8.6.1 | Alto: exposição direta de tutor de outra clínica. |
| Tutors | Listar tutores | `Tutor` | contexto atual | Sim | Sim | `WHERE ClinicId = clinicId` | 8.6.1 | Alto: listagem global. |
| Tutors | Unicidade de documento | `Tutor` | contexto autorizado do fluxo | Sim nos fluxos clínicos | Sim nos fluxos clínicos | `ClinicId + Document`, sem aceitar clínica não autorizada | 8.6.1 | Médio: existência de documento pode vazar entre clínicas se origem do clinicId não for validada. |
| Patients/Pets | Criar pet/patient | `Patient` + `Pet` | contexto atual; tutor deve pertencer ao mesmo contexto | Sim | Sim | Persistir `Patient.ClinicId` autorizado; validar tutor por `TutorId + ClinicId` | 8.6.1 | Alto: criação vinculada a tutor de outra clínica. |
| Patients/Pets | Buscar pet por patientId | `Patient` + `Pet` | contexto atual via `Patient.ClinicId` | Sim | Sim | `Patient.Id + Patient.ClinicId` | 8.6.1 | Alto: consulta por ID global. |
| Patients/Pets | Listar pets | `Patient` + `Pet` | contexto atual via `Patient.ClinicId` | Sim | Sim | Join com `Patient` e filtro por `Patient.ClinicId` | 8.6.1 | Alto: listagem global. |
| Patients/Pets | Atualizar pet | `Patient` + `Pet` | contexto atual via `Patient.ClinicId` | Sim | Sim | Buscar `Patient/Pet` dentro do `ClinicId`; não mudar `ClinicId` | 8.6.1 | Alto: update cross-clinic. |
| Patients/Pets | Deletar pet | `Patient` + `Pet` | contexto atual via `Patient.ClinicId` | Sim | Sim | Remover apenas se `Patient.Id + ClinicId` | 8.6.1 | Alto: delete cross-clinic. |
| Patients/Pets | Validar tutor/paciente | `Tutor`/`Patient` | contexto autorizado | Sim | Sim | Existência por `Id + ClinicId` | 8.6.1 e 8.6.2 | Alto: vínculo indevido entre clínicas. |
| Patients/Pets | Unicidade de microchip | `Pet`/`Patient` | decisão de produto: global ou clínica via `Patient.ClinicId` | A decidir | A decidir | Tratamento especial; documentar política antes de alterar | 8.6.5 ou subfase específica | Médio: unicidade global pode revelar existência; unicidade por clínica pode permitir duplicidade global. |
| Attendances | Criar atendimento | `Attendance` | contexto atual + paciente autorizado | Sim | Sim | Validar paciente no `ClinicId` e persistir `Attendance.ClinicId` autorizado | 8.6.2 | Alto: atendimento em paciente de outra clínica. |
| Attendances | Buscar atendimento por ID | `Attendance` | contexto atual | Sim | Sim | `Id + ClinicId` | 8.6.2 | Alto: exposição por ID global. |
| Attendances | Listar atendimentos | `Attendance` | contexto atual | Sim | Sim | `WHERE ClinicId = clinicId` | 8.6.2 | Alto: listagem global. |
| Attendances | Listar por paciente | `Attendance` | contexto atual | Sim | Sim | `PatientId + ClinicId`; opcional validar `Patient.ClinicId` | 8.6.2 | Alto: enumeração de atendimentos por paciente externo. |
| Attendances | Fechar atendimento | `Attendance` | contexto atual | Sim | Sim | Buscar aberto por `Id + ClinicId` antes de fechar | 8.6.2 | Alto: mutação cross-clinic. |
| Attendances | Cancelar atendimento | `Attendance` | contexto atual | Sim | Sim | Buscar por `Id + ClinicId` antes de cancelar | 8.6.2 | Alto: mutação cross-clinic. |
| Attendances | Validar atendimento aberto | `Attendance` | contexto autorizado do paciente | Sim | Sim | `PatientId + ClinicId + Status` | 8.6.2 | Médio/alto: bloqueio indevido por atendimento de outra clínica. |
| Attendances | Unicidade de número | `Attendance` | contexto autorizado ou regra global explícita | Sim | Sim se por clínica | Decidir `ClinicId + AttendanceNumber` ou global documentado | 8.6.2 | Médio: regra atual pode bloquear/vazar globalmente. |
| MedicalRecords | Criar prontuário | `MedicalRecord` | contexto atual + paciente autorizado | Sim | Sim | Validar paciente por `Id + ClinicId`; persistir `ClinicId` autorizado | 8.6.3 | Alto: prontuário em paciente externo. |
| MedicalRecords | Buscar por patientId | `MedicalRecord` | contexto atual | Sim | Sim | `PatientId + ClinicId + !IsDeleted` | 8.6.3 | Alto: exposição por patientId. |
| MedicalRecords | Atualizar prontuário | `MedicalRecord` | contexto atual | Sim | Sim | Buscar por `Id + ClinicId + !IsDeleted`; não mudar `ClinicId` | 8.6.3 | Alto: update cross-clinic. |
| MedicalRecords | Soft delete | `MedicalRecord` | contexto atual | Sim | Sim | Buscar por `Id + ClinicId + !IsDeleted` | 8.6.3 | Alto: exclusão lógica cross-clinic. |
| MedicalRecords | Existência/unicidade | `MedicalRecord` | contexto autorizado do fluxo | Sim | Sim | `PatientId + ClinicId`, incluindo soft deleted quando necessário | 8.6.3 | Médio/alto: vazamento de existência e bloqueio indevido. |
| ClinicalEvolutions | Criar evolução | `ClinicalEvolution` | contexto atual + atendimento autorizado | Sim | Sim | Validar `Attendance.Id + ClinicId`; persistir `ClinicalEvolution.ClinicId` autorizado | 8.6.4 | Alto: evolução em atendimento externo. |
| ClinicalEvolutions | Listar por atendimento | `ClinicalEvolution` | contexto atual | Sim | Sim | `AttendanceId + ClinicId` ou validação prévia do atendimento + filtro direto | 8.6.4 | Alto: listagem por atendimento externo. |
| Prescriptions | Criar prescrição | `Prescription` + `PrescriptionItem` | contexto atual + atendimento autorizado | Sim | Sim | Validar `Attendance.Id + ClinicId`; persistir `Prescription.ClinicId` autorizado; itens herdam por prescrição | 8.6.4 | Alto: prescrição em atendimento externo. |
| Prescriptions | Listar por atendimento | `Prescription` + `PrescriptionItem` | contexto atual | Sim | Sim | `AttendanceId + Prescription.ClinicId`; contar itens apenas de prescrições filtradas | 8.6.4 | Alto: listagem por atendimento externo. |
| ClinicalAuditLog | Escrita de auditoria | `ClinicalAuditLog` | evento auditado; contexto futuro | Não nesta subfase | Não nesta subfase | Pode continuar sem filtro por enquanto; mapear extensão contextual futura | Futuro pós-8.6 | Médio: auditoria de leitura/contexto ainda não definida. |
| Consultas administrativas | Listagens globais deliberadas | Várias | autorização administrativa explícita | Não necessariamente | Sim, por política administrativa | Sem filtro clínico apenas se documentado e segregado | 8.6.5+ | Alto: confusão entre endpoint clínico e administrativo. |

## 9. Estratégia por tipo de operação

### Operações por ID

Padrão recomendado:

- buscar entidade por `Id + ClinicId` sempre que o registro tiver `ClinicId` direto;
- para entidades sem `ClinicId` direto, buscar por `Id` da entidade base + join/validação do pai escopado;
- evitar buscar por ID global e validar depois, quando possível;
- se a busca global for inevitável por limitação técnica, validar `ClinicId` antes de retornar qualquer dado e responder de forma indistinguível de não encontrado quando apropriado.

### Listagens

Padrão recomendado:

- sempre filtrar por `ClinicId` autorizado;
- não manter listagem global como comportamento padrão;
- criar métodos administrativos separados caso seja necessária visão global;
- testar que registros de outra clínica não aparecem no resultado.

### Criações

Padrão recomendado:

- derivar `ClinicId` do `ICurrentClinicalContext` autorizado;
- quando a entidade pai já possuir `ClinicId`, derivar dela após validar que ela pertence ao contexto atual;
- quando `ClinicId` vier do request por compatibilidade temporária, validar com `IClinicalContextAuthorizationService` antes de persistir;
- nunca tratar payload como prova de autorização.

### Atualizações

Padrão recomendado:

- buscar o registro dentro do `ClinicId` autorizado;
- não permitir mudança de `ClinicId` via update;
- validar entidades relacionadas dentro do mesmo contexto;
- retornar não encontrado/acesso negado conforme política de segurança definida, sem revelar existência cross-clinic desnecessariamente.

### Deletes/soft deletes

Padrão recomendado:

- buscar o registro dentro do `ClinicId` autorizado;
- aplicar delete/soft delete apenas no registro escopado;
- registrar débito para auditoria contextual futura, especialmente em soft deletes e exclusões físicas.

### Validações

Padrão recomendado:

- validadores de existência devem receber ou derivar `ClinicId` quando usados em fluxos protegidos;
- evitar validadores globais que retornem existência de dados de outra clínica;
- validadores de unicidade devem explicitar se a regra é global ou por clínica;
- mensagens de erro não devem revelar dados de outra clínica.

## 10. Subfases recomendadas

Divisão incremental recomendada:

1. **8.6.0 — Planejamento técnico dos filtros de consulta por contexto.** Criar este documento, mapear riscos e bloquear mudanças produtivas nesta subfase.
2. **8.6.1 — Filtros em Tutor/Patient/Pet.** Priorizar cadastros-base e entidades que sustentam validações posteriores; revisar origem de `ClinicId` em criação de tutor/pet.
3. **8.6.2 — Filtros em Attendance.** Escopar criação, busca, listagem, fechamento, cancelamento e validadores de atendimento aberto/número.
4. **8.6.3 — Filtros em MedicalRecord.** Escopar prontuários por `ClinicId`, preservando soft delete e unicidade por paciente.
5. **8.6.4 — Filtros em ClinicalEvolution e Prescription.** Escopar entidades filhas de atendimento e garantir que itens de prescrição herdem apenas prescrições autorizadas.
6. **8.6.5 — Revisão final dos filtros e guardrails.** Auditar contratos, testes de isolamento cross-clinic, exceções administrativas, microchip, mensagens de erro e documentação de qualquer operação global.

Essa ordem reduz risco porque começa pelas entidades-base (`Tutor`/`Patient`) antes de proteger fluxos dependentes (`Attendance`, prontuário, evolução e prescrição).

## 11. O que NÃO implementar na 8.6.0

Esta subfase não implementa:

- filtros reais em repositories;
- alterações em controllers;
- alterações em use cases;
- migrations;
- alterações em DTOs;
- alterações em queries produtivas;
- auditoria contextual;
- auditoria de leitura;
- acesso negado transversal;
- front-end;
- EF Core Global Query Filters;
- `ClinicUnitId`;
- multi-tenant SaaS completo;
- novas políticas administrativas globais.

## 12. Relação com débitos técnicos

O débito **filtros globais por contexto ainda ausentes** continua aberto até a implementação das subfases 8.6.x.

Débitos específicos associados a esta fase:

- risco de consultas por ID sem `ClinicId`;
- risco de validadores globais vazarem existência de dados de outra clínica;
- risco de listagens administrativas serem confundidas com listagens clínicas;
- risco de payload com `ClinicId` ser tratado como autorização;
- risco de entidades sem `ClinicId` direto serem filtradas incorretamente sem join/validação do pai;
- risco de regras de unicidade global, como microchip ou número de atendimento, revelarem dados ou bloquearem operações entre clínicas sem decisão explícita.

Este documento deve ser lido em conjunto com `docs/security/PHASE_08_TECHNICAL_DEBT.md`, que mantém o acompanhamento consolidado dos débitos da Fase 8.

## 13. Critérios de aceite da Fase 8.6.0

A Fase 8.6.0 só deve ser considerada concluída se:

- este documento for criado;
- consultas clínicas sensíveis forem mapeadas;
- operações forem classificadas;
- estratégia de filtros explícitos versus global filters for documentada;
- subfases 8.6.x forem propostas;
- riscos técnicos forem documentados;
- escopo proibido for respeitado;
- nenhuma query produtiva for alterada;
- nenhuma migration for criada;
- nenhum controller for alterado;
- próxima subfase recomendada for indicada.

## 14. Comandos de validação esperados

Para esta subfase documental, os comandos esperados são:

```bash
git diff --check
git status --short
```

Não é necessário executar `dotnet build` ou testes automatizados se houver apenas documentação. O CI pode rodar normalmente.

## 15. Próxima fase recomendada

A próxima fase recomendada é **8.6.1 — Filtros em Tutor/Patient/Pet**.

Objetivo inicial da 8.6.1:

- resolver `ClinicId` autorizado via `ICurrentClinicalContext` + `IClinicalContextAuthorizationService` nos fluxos de tutor/pet;
- alterar contratos mínimos de repository para buscas/listagens por `ClinicId`;
- filtrar `Tutor` diretamente por `Tutor.ClinicId`;
- filtrar `Pet` por `Patient.ClinicId`;
- revisar validadores de tutor, paciente e microchip com testes cross-clinic.
