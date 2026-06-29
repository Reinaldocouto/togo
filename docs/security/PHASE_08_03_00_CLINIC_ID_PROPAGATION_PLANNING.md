# Fase 8.3.0 — Planejamento técnico da propagação de ClinicId nos fluxos clínicos

## 1. Objetivo da Fase 8.3

A Fase 8.3 prepara os registros clínicos do TOGO para pertencerem explicitamente a uma clínica específica por meio de `ClinicId` nas entidades clínicas relevantes.

Esta preparação deve permitir que fases posteriores apliquem isolamento clínico-operacional com menor risco, especialmente em consultas, auditoria, relatórios e validações de acesso cruzado.

Importante: a Fase 8.3 ainda não implementa autorização contextual completa, não ativa filtros globais por contexto clínico e não substitui as permissões já existentes por escopo clínico. Ela cria a base persistida necessária para que as fases futuras façam isso de forma confiável.

## 2. Problema que a Fase 8.3 resolve

Hoje o projeto já possui persistência mínima de `Organization`, `Clinic` e `ClinicUnit`, mas as entidades clínicas existentes ainda não carregam `ClinicId`.

Sem `ClinicId` persistido nas entidades clínicas relevantes, fases futuras não conseguirão aplicar de forma confiável:

- filtros por clínica em listagens e buscas sensíveis;
- auditoria contextual indicando a clínica afetada;
- proteção contra acesso cruzado entre clínicas;
- relatórios operacionais por clínica;
- validações de consistência entre tutor, paciente, atendimento, prontuário, evolução e prescrição;
- investigação de incidentes com recorte clínico-operacional claro.

A ausência de `ClinicId` também aumenta o risco de regras futuras dependerem apenas de joins indiretos, o que pode gerar consultas sem escopo por engano, especialmente em endpoints de listagem, repositórios e relatórios.

## 3. Decisão sobre escopo primário

A decisão recomendada para a Fase 8.3 é manter `ClinicId` como escopo primário de isolamento clínico-operacional.

Decisões de escopo:

- `ClinicId` será o campo operacional usado para isolar registros clínicos.
- `OrganizationId` poderá ser derivado por meio de `Clinic`, evitando duplicação prematura nas entidades clínicas.
- `ClinicUnitId` deve permanecer opcional e futuro, salvo se surgir necessidade clara em uma fase posterior.
- A Fase 8.3 não deve tentar resolver isolamento por unidade interna antes de consolidar o isolamento por clínica.

Essa decisão segue o desenho da Fase 8.2, em que `Clinic` já foi registrada como escopo primário planejado para isolamento clínico-operacional.

## 4. Estratégia recomendada para ClinicId por entidade

### 4.1 Tutor

Estado atual analisado:

- `Tutor` possui `Id`, dados de contato e datas de criação/atualização.
- Não há `ClinicId` no domínio atual.
- O tutor é base para relacionamento com pacientes/pets.

Recomendação para o TOGO neste momento:

- Adicionar `ClinicId` direto em `Tutor` na próxima subfase aplicável.
- Tornar `Tutor` pertencente a uma única clínica no MVP.
- Não implementar relacionamento tutor multi-clínica agora.

Justificativa:

- Reduz complexidade do MVP.
- Facilita filtros por clínica em cadastro de tutores.
- Evita que a proteção dependa apenas de vínculos indiretos com pacientes ou pets.
- Permite validar que um paciente/pet criado para um tutor pertence à mesma clínica.

Trade-off aceito:

- Um mesmo tutor que exista em múltiplas clínicas poderá precisar de registros separados nesta etapa.
- Um modelo normalizado de tutor multi-clínica, com tabela associativa, fica fora do escopo atual.

### 4.2 Patient

Estado atual analisado:

- `Patient` possui `Id`, tipo, nome, data de nascimento, status e datas de criação/atualização.
- Não há `ClinicId` no domínio atual.
- `Patient` é referenciado por `Pet`, `Attendance` e `MedicalRecord`.

Recomendação:

- Adicionar `ClinicId` direto em `Patient`.
- Validar que `Patient.ClinicId` seja igual ao `Tutor.ClinicId` quando houver vínculo direto ou indireto via `Pet`.
- Usar `Patient.ClinicId` como uma das principais âncoras de escopo para registros clínicos derivados.

Justificativa:

- Paciente é uma entidade central do fluxo clínico.
- Prontuários, atendimentos e histórico clínico derivam do paciente.
- A presença de `ClinicId` em `Patient` simplifica consultas sensíveis por paciente dentro de uma clínica.

### 4.3 Pet

Estado atual analisado:

- O projeto possui `Pet` como entidade separada.
- `Pet` possui `PatientId` e `TutorId`.
- `Pet` funciona como extensão/representação veterinária vinculada a `Patient` e `Tutor`.

Recomendação:

- Não adicionar `ClinicId` direto em `Pet` inicialmente.
- Herdar o escopo de `Patient` e validar consistência com `Tutor`.
- Reavaliar `ClinicId` direto em `Pet` apenas se consultas frequentes por pet sem join com paciente demonstrarem necessidade técnica.

Justificativa:

- `Pet` já depende de `PatientId` e `TutorId`.
- Duplicar `ClinicId` em `Pet` aumentaria risco de inconsistência entre `Pet`, `Patient` e `Tutor`.
- Para o MVP, `Patient.ClinicId` deve ser suficiente como escopo clínico primário do pet.

Risco a monitorar:

- Se endpoints de pets fizerem listagens extensas por clínica, os repositórios precisarão garantir joins seguros com `Patient` ou `Tutor` enquanto `Pet` não tiver `ClinicId` direto.

### 4.4 Attendance

Estado atual analisado:

- `Attendance` possui `PatientId`, número de atendimento, datas de abertura/fechamento/cancelamento, status, tipo e autoria.
- Não há `ClinicId` no domínio atual.
- É unidade operacional central para evolução clínica e prescrição.

Recomendação:

- Adicionar `ClinicId` direto em `Attendance`.
- Validar que `Attendance.ClinicId` seja igual ao `Patient.ClinicId`.
- Criar índice em `ClinicId` e avaliar índices compostos para listagens por clínica/status/data.

Justificativa:

- `Attendance` é a unidade operacional central.
- Consultas, auditoria, relatórios e segurança frequentemente partem do atendimento.
- Carregar `ClinicId` diretamente reduz risco de consultas operacionais sem escopo e sem join adequado.

### 4.5 MedicalRecord

Estado atual analisado:

- `MedicalRecord` possui `PatientId`, notas, flags JSON, autoria, datas de atualização e soft delete.
- Não há `ClinicId` no domínio atual.
- Já existe preocupação com unicidade por paciente, exclusão lógica, autoria e auditoria.

Opções avaliadas:

1. Herdar `ClinicId` apenas de `Patient`.
   - Vantagem: evita redundância.
   - Desvantagem: consultas e auditorias exigem join com `Patient` para escopo.

2. Armazenar `ClinicId` direto em `MedicalRecord`.
   - Vantagem: simplifica filtros protegidos, auditoria e relatórios sensíveis.
   - Desvantagem: duplica escopo e exige validação de consistência com `Patient.ClinicId`.

Decisão final recomendada para o próximo PR aplicável:

- Adicionar `ClinicId` direto em `MedicalRecord`.
- Validar que `MedicalRecord.ClinicId` seja igual ao `Patient.ClinicId` no fluxo de criação.
- Manter índice por `ClinicId` e revisar índices de unicidade para considerar dados ativos e escopo clínico quando necessário.

Justificativa:

- Prontuário é uma das entidades mais sensíveis do sistema.
- Consultas de prontuário tendem a ser frequentes e críticas.
- O padrão da Fase 8 prioriza consultas protegidas sem depender de joins complexos em todos os caminhos.

### 4.6 ClinicalEvolution

Estado atual analisado:

- `ClinicalEvolution` possui `AttendanceId`, data de registro, tipo, texto e autoria.
- Não há `ClinicId` no domínio atual.
- A evolução clínica é fortemente dependente do atendimento.

Opções avaliadas:

1. Herdar escopo de `Attendance`.
   - Vantagem: evita redundância.
   - Desvantagem: toda consulta protegida exige join com `Attendance`.

2. Armazenar `ClinicId` direto em `ClinicalEvolution`.
   - Vantagem: simplifica filtros, auditoria e proteção contra listagens sem join.
   - Desvantagem: exige validação de consistência com `Attendance.ClinicId`.

Recomendação inicial:

- Adicionar `ClinicId` direto em `ClinicalEvolution`, desde que a Fase 8 mantenha o padrão de consultas protegidas sem joins complexos.
- Validar que `ClinicalEvolution.ClinicId` seja igual ao `Attendance.ClinicId`.

Justificativa:

- Evoluções são dados clínicos sensíveis.
- Listagens por atendimento e futuras consultas por clínica devem ser seguras por padrão.
- O custo de duplicação é aceitável se houver validação clara no Application/Infrastructure.

### 4.7 Prescription

Estado atual analisado:

- `Prescription` possui `AttendanceId`, data de emissão e observações.
- Não há `ClinicId` no domínio atual.
- `PrescriptionItem` depende de `Prescription`.

Opções avaliadas:

1. Herdar escopo de `Attendance`.
   - Vantagem: evita redundância.
   - Desvantagem: consultas, auditoria e futuras emissões exigem join com atendimento.

2. Armazenar `ClinicId` direto em `Prescription`.
   - Vantagem: simplifica filtros, auditoria, emissão e futuras integrações.
   - Desvantagem: exige validação de consistência com `Attendance.ClinicId`.

Recomendação inicial:

- Adicionar `ClinicId` direto em `Prescription`.
- Validar que `Prescription.ClinicId` seja igual ao `Attendance.ClinicId`.

Justificativa:

- Prescrições são dados clínicos e operacionais sensíveis.
- Futuras emissões, relatórios e auditorias se beneficiam de escopo direto.
- Reduz risco de consulta sem escopo ao listar prescrições.

### 4.8 PrescriptionItem

Estado atual analisado:

- `PrescriptionItem` possui `PrescriptionId`, produto opcional, quantidade, unidade, dosagem e duração.
- É entidade filha de `Prescription`.

Recomendação:

- Não adicionar `ClinicId` direto em `PrescriptionItem` inicialmente.
- Herdar escopo de `Prescription`.
- Garantir que consultas de itens partam de uma prescrição já escopada ou façam join seguro com `Prescription`.

Justificativa:

- `PrescriptionItem` não é raiz de escopo clínico.
- Duplicar `ClinicId` em itens aumentaria redundância sem benefício claro no MVP.
- A integridade deve ser garantida pelo relacionamento com `Prescription`.

### 4.9 ClinicalAuditLog

Estado atual analisado:

- `ClinicalAuditLog` registra entidade, id da entidade, ação, usuário, perfil, data e metadados JSON.
- Ainda não carrega escopo contextual estruturado como `ClinicId`.

Recomendação para esta fase:

- Não alterar `ClinicalAuditLog` agora.
- Registrar impacto futuro para a Fase 8.7.
- Planejar que auditoria contextual futura inclua `ClinicId` de forma estruturada ou por metadados controlados, evitando depender apenas de texto livre.

Justificativa:

- A auditoria contextual depende de `ClinicId` já propagado nas entidades clínicas.
- Alterar auditoria antes da propagação pode criar dados incompletos ou inconsistentes.

## 5. Ordem incremental recomendada

A Fase 8.3 deve ser dividida em subfases pequenas, testáveis e reversíveis em revisão.

Divisão recomendada:

- 8.3.0 — Planejamento técnico da propagação de `ClinicId`.
- 8.3.1 — Introdução de `ClinicId` em `Tutor` e `Patient`.
- 8.3.2 — Introdução de `ClinicId` em `Attendance`.
- 8.3.3 — Introdução de `ClinicId` em `MedicalRecord`, `ClinicalEvolution` e `Prescription`.
- 8.3.4 — Ajustes finais de persistência, testes e documentação da propagação.

Justificativa da ordem:

- `Tutor` e `Patient` são a base cadastral e clínica inicial.
- `Attendance` depende de `Patient` e deve vir depois para validar consistência de escopo.
- `MedicalRecord`, `ClinicalEvolution` e `Prescription` dependem de `Patient` ou `Attendance` e devem ser ajustados depois da base estar clara.
- `PrescriptionItem` e `Pet` devem preferencialmente herdar escopo nesta etapa, sem campo próprio.

## 6. Impacto em Domain

### Entidades que provavelmente precisarão de novo campo `ClinicId`

- `Tutor`.
- `Patient`.
- `Attendance`.
- `MedicalRecord`.
- `ClinicalEvolution`.
- `Prescription`.

### Entidades que não devem receber `ClinicId` direto inicialmente

- `Pet`, que deve herdar escopo de `Patient` e validar consistência com `Tutor`.
- `PrescriptionItem`, que deve herdar escopo de `Prescription`.
- `ClinicalAuditLog`, que fica para impacto futuro.

### Alterações esperadas por entidade

| Entidade | Campo novo | Factory `Create` | Validações | Testes unitários | Regra de consistência esperada |
| --- | --- | --- | --- | --- | --- |
| `Tutor` | `ClinicId` | Sim | `ClinicId > 0` | Sim | Tutor pertence a uma clínica no MVP. |
| `Patient` | `ClinicId` | Sim | `ClinicId > 0` | Sim | Se vinculado a tutor, clínica deve coincidir. |
| `Pet` | Não inicialmente | Não inicialmente | Validar por serviços/use cases | Sim, se fluxo for afetado | `Patient.ClinicId == Tutor.ClinicId`. |
| `Attendance` | `ClinicId` | Sim | `ClinicId > 0` | Sim | `Attendance.ClinicId == Patient.ClinicId`. |
| `MedicalRecord` | `ClinicId` | Sim | `ClinicId > 0` | Sim | `MedicalRecord.ClinicId == Patient.ClinicId`. |
| `ClinicalEvolution` | `ClinicId` | Sim | `ClinicId > 0` | Sim | `ClinicalEvolution.ClinicId == Attendance.ClinicId`. |
| `Prescription` | `ClinicId` | Sim | `ClinicId > 0` | Sim | `Prescription.ClinicId == Attendance.ClinicId`. |
| `PrescriptionItem` | Não | Não | Não | Apenas se consultas mudarem | Herda escopo de `Prescription`. |
| `ClinicalAuditLog` | Não agora | Não agora | Não agora | Futuro | Fase 8.7 deve receber contexto clínico. |

Regras de domínio a considerar:

- criar método privado de validação de id positivo para `ClinicId`, reaproveitando padrão já existente em entidades com ids obrigatórios;
- evitar aceitar `ClinicId` default/zero;
- manter construtores privados e factories como ponto principal de criação;
- não misturar regra de autorização no domínio nesta fase;
- tratar consistência entre entidades preferencialmente em use cases/validators, pois exige leitura de agregados relacionados.

## 7. Impacto em Infrastructure

Configurações EF esperadas em fases posteriores:

- adicionar coluna `ClinicId` nas tabelas clínicas selecionadas;
- configurar FK para `Clinics.Id` nas entidades com `ClinicId` direto;
- configurar `DeleteBehavior.Restrict` em todos os relacionamentos com `Clinic`;
- criar índice simples em `ClinicId` para filtros por clínica;
- avaliar índices compostos por padrões de consulta;
- criar migration de alteração das tabelas clínicas apenas em subfase de implementação.

Índices compostos candidatos:

- `Tutors`: `ClinicId + Document`, se a unicidade/busca por documento passar a ser por clínica.
- `Patients`: `ClinicId + Name`, se houver listagens por nome dentro da clínica.
- `Attendances`: `ClinicId + Status`, `ClinicId + OpenedAt`, ou `ClinicId + Status + OpenedAt`.
- `MedicalRecords`: revisar unicidade por `PatientId`; considerar `ClinicId + PatientId` apenas se necessário para consulta ou integridade.
- `ClinicalEvolutions`: `ClinicId + AttendanceId`, possivelmente `ClinicId + RegisteredAt` para relatórios.
- `Prescriptions`: `ClinicId + AttendanceId`, possivelmente `ClinicId + IssuedAt` para relatórios.

Cuidados obrigatórios:

- não usar cascade delete de `Clinic` para registros clínicos;
- não criar migration nesta Fase 8.3.0;
- validar impacto em snapshots apenas nas subfases de implementação;
- garantir testes de metadados EF para FK, índices e delete behavior seguro.

## 8. Impacto em Application

Use cases provavelmente afetados nas próximas subfases:

- criação de `Tutor`;
- criação de `Patient`;
- criação/atualização de `Pet`, apenas para validação de consistência entre paciente e tutor;
- criação de `Attendance`;
- criação de `MedicalRecord`;
- criação de `ClinicalEvolution`;
- criação de `Prescription`;
- consultas e listagens sensíveis, como impacto futuro, para receber filtros por contexto clínico.

Pontos de atenção:

- validators que hoje verificam existência de `Patient`, `Tutor`, `Attendance` ou `MedicalRecord` precisarão considerar escopo clínico em fases futuras;
- repositórios poderão precisar de métodos escopados por `ClinicId`;
- use cases não devem tratar `ClinicId` informado no payload como autorização;
- enquanto `CurrentClinicalContext` não existir, qualquer passagem temporária de `ClinicId` deve ser documentada e validada posteriormente;
- permissões atuais por perfil/claim não devem ser confundidas com autorização contextual por clínica.

## 9. Impacto em API/DTOs

Decisão recomendada:

- Evitar aceitar `ClinicId` livremente no payload público como fonte confiável.
- Planejar que `ClinicId` venha futuramente de um `CurrentClinicalContext` validado.
- Enquanto `CurrentClinicalContext` ainda não existir, qualquer inclusão temporária de `ClinicId` em request deve ser documentada como transitória.
- Reforçar que payload informado pelo cliente não é autorização.

Implicações para fases futuras:

- DTOs públicos podem precisar receber `ClinicId` temporariamente apenas para desbloquear persistência em ambiente controlado.
- Essa solução temporária deve ser removida ou endurecida quando houver contexto clínico autenticado.
- Controllers não devem inferir autorização apenas porque o cliente enviou um `ClinicId`.
- Documentação Swagger/OpenAPI deve deixar claro quando um campo for transitório.

Não haverá alteração de DTOs nesta Fase 8.3.0.

## 10. Compatibilidade e dados existentes

A introdução de `ClinicId` obrigatório afeta dados já existentes em desenvolvimento/testes. A decisão deve ser cuidadosa para evitar migrations quebradas ou dados órfãos.

Opções analisadas:

### Opção A — `ClinicId` obrigatório com backfill para clínica default de desenvolvimento

Vantagens:

- Modelo final já nasce mais rígido.
- Evita período com registros clínicos sem clínica.
- Facilita testes de integridade.

Desvantagens:

- Exige definir ou criar uma clínica default.
- Pode misturar dados históricos em uma clínica artificial.
- Requer cuidado para não levar seed indevido a ambientes reais.

### Opção B — `ClinicId` nullable temporário com posterior endurecimento

Vantagens:

- Migration inicial menos arriscada.
- Permite transição gradual.
- Reduz risco de quebra imediata em dados existentes.

Desvantagens:

- Cria período com escopo incompleto.
- Exige segunda migration para tornar obrigatório.
- Pode atrasar filtros seguros se consultas precisarem lidar com nulos.

### Opção C — Migration em duas etapas

Etapa 1:

- adicionar `ClinicId` nullable;
- preencher dados existentes com uma clínica controlada;
- ajustar aplicação e testes.

Etapa 2:

- tornar `ClinicId` obrigatório;
- criar FKs e índices definitivos quando seguro;
- remover caminhos transitórios.

Vantagens:

- É a abordagem mais segura para evolução de schema.
- Permite validação incremental.
- Reduz risco de indisponibilidade por dados legados.

Desvantagens:

- Exige mais subfases e mais disciplina de documentação.

### Opção D — Seed controlado de Organization/Clinic default apenas se necessário

Vantagens:

- Ajuda em ambientes de desenvolvimento/testes.
- Permite backfill previsível.

Desvantagens:

- Deve ser claramente limitado por ambiente.
- Não deve esconder ausência de governança real em produção.

Recomendação preliminar para a próxima subfase:

- Evitar decisão apressada sobre obrigatoriedade imediata.
- Para `Tutor` e `Patient`, avaliar uma migration em duas etapas se houver dados existentes relevantes.
- Em ambiente de desenvolvimento/testes, considerar clínica default controlada apenas se necessária para backfill.
- Documentar explicitamente se `ClinicId` nascerá obrigatório ou temporariamente nullable em cada subfase de implementação.

## 11. Riscos técnicos

Riscos identificados:

- Quebrar construtores/factories existentes ao adicionar parâmetro obrigatório em várias entidades.
- Quebrar testes atuais de domínio, application, infrastructure e API.
- Criar `ClinicId` obrigatório sem backfill e tornar migration inaplicável em bancos existentes.
- Aceitar `ClinicId` do cliente sem validação e tratar indevidamente como autorização.
- Duplicar `ClinicId` em entidades derivadas e gerar inconsistência entre paciente, atendimento, evolução, prescrição e prontuário.
- Esquecer índice em consultas futuras e degradar performance de listagens por clínica.
- Permitir cascade delete perigoso a partir de `Clinic` para dados clínicos.
- Misturar escopo clínico com autorização contextual antes da Fase 8.5.
- Criar filtros parciais em alguns repositórios e deixar outros sem escopo.
- Endurecer DTOs antes de existir `CurrentClinicalContext` e criar contrato público difícil de remover.
- Usar `OrganizationId` de forma duplicada nas entidades clínicas sem necessidade imediata.
- Antecipar `ClinicUnitId` e aumentar complexidade antes de haver caso de uso validado.

Mitigações planejadas:

- Subfases pequenas e focadas.
- Testes unitários de domínio para validação de `ClinicId`.
- Testes de persistência para FK, índices e `DeleteBehavior.Restrict`.
- Documentação explícita de qualquer campo transitório em request.
- Validação de consistência no Application antes de persistir entidades derivadas.
- Revisão de migrations antes de aplicá-las a ambientes compartilhados.

## 12. Fora do escopo da Fase 8.3.0

Esta subfase não implementa:

- migrations;
- alterações nas entidades;
- alterações em DTOs;
- alterações em use cases;
- alterações em repositories;
- alterações em controllers;
- alterações em configurações EF Core;
- alterações no `AppDbContext`;
- `CurrentClinicalContext`;
- `UserClinicAccess`;
- autorização contextual;
- filtros por contexto;
- auditoria contextual;
- seeds obrigatórios;
- front-end;
- regras de `ClinicUnitId`;
- vínculo tutor multi-clínica.

## 13. Critérios de aceite da Fase 8.3.0

A Fase 8.3.0 só deve ser considerada concluída se:

- o documento `PHASE_08_03_00_CLINIC_ID_PROPAGATION_PLANNING.md` for criado;
- todas as entidades clínicas relevantes forem analisadas;
- houver recomendação explícita para cada entidade;
- houver proposta de subfases 8.3.x;
- houver estratégia preliminar para dados existentes;
- houver riscos técnicos documentados;
- houver definição clara do que não será implementado ainda;
- a próxima subfase recomendada estiver indicada;
- nenhum código produtivo for alterado;
- nenhuma migration for criada.

## 14. Próxima fase recomendada

A próxima fase recomendada é:

**Fase 8.3.1 — Introdução de `ClinicId` em `Tutor` e `Patient`.**

A análise confirma que `Tutor` e `Patient` são o melhor primeiro corte porque formam a base cadastral e clínica inicial. A partir deles, será possível validar consistência de escopo em `Pet`, `Attendance`, `MedicalRecord`, `ClinicalEvolution` e `Prescription` nas subfases seguintes.

## Resumo das decisões recomendadas

| Entidade | Decisão recomendada | Subfase provável |
| --- | --- | --- |
| `Tutor` | Receber `ClinicId` direto; tutor multi-clínica fora do escopo atual. | 8.3.1 |
| `Patient` | Receber `ClinicId` direto; deve coincidir com tutor quando aplicável. | 8.3.1 |
| `Pet` | Não receber `ClinicId` direto inicialmente; herdar de `Patient`. | 8.3.1/8.3.4 validações |
| `Attendance` | Receber `ClinicId` direto; deve coincidir com paciente. | 8.3.2 |
| `MedicalRecord` | Receber `ClinicId` direto; trade-off aceito para simplificar segurança e auditoria. | 8.3.3 |
| `ClinicalEvolution` | Receber `ClinicId` direto se mantido o padrão de consultas protegidas sem joins complexos. | 8.3.3 |
| `Prescription` | Receber `ClinicId` direto para reduzir risco de consulta sem escopo. | 8.3.3 |
| `PrescriptionItem` | Não receber `ClinicId` direto; herdar de `Prescription`. | 8.3.3/8.3.4 |
| `ClinicalAuditLog` | Não alterar agora; planejar impacto futuro na Fase 8.7. | Futuro |

