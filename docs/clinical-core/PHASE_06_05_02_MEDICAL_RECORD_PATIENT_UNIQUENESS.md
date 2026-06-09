# TOGO — Fase 6.5.2: Unicidade física de MedicalRecord por Patient

## 1. Contexto da Fase 6.5

A Fase 6.5 trata o hardening estrutural de `MedicalRecord`, com foco em integridade física e evolução segura de schema. A Fase 6.5.1 foi documental e registrou dois débitos principais:

- `MR-DEBT-007` — índice único em `MedicalRecords.PatientId` ausente;
- `MR-DEBT-009` — `FlagsJson` flexível.

A Fase 6.5.2 trata diretamente `MR-DEBT-007` e não altera `FlagsJson`.

## 2. Referência explícita ao MR-DEBT-007

`MR-DEBT-007` existia porque a relação clínica esperada entre `Patient` e `MedicalRecord` era 1:1 na aplicação, mas não havia constraint física no banco. A proteção anterior dependia de validator/use case e podia falhar em concorrência extrema ou escrita fora da aplicação.

## 3. Relação com a Fase 6.5.1

A Fase 6.5.1 registrou a tensão entre duas alternativas:

- manter o comportamento documentado na Fase 6.4.3, no qual um registro logicamente deletado não bloqueava a criação de novo prontuário;
- adotar índice único simples em `PatientId`, tratando `MedicalRecord` como 1:1 total por paciente.

A Fase 6.5.2 resolve essa decisão e implementa a alternativa conservadora.

## 4. Decisão final de unicidade

A decisão final é:

> Um `Patient` deve possuir no máximo um `MedicalRecord` total, independentemente de `IsDeleted`.

Consequências diretas:

- `MedicalRecord` é 1:1 total por `Patient`;
- Soft Delete não libera criação de novo `MedicalRecord` para o mesmo `PatientId`;
- registro logicamente deletado continua contando como histórico clínico do paciente;
- `MedicalRecords.PatientId` passa a ter índice único físico simples.

## 5. Justificativa clínica/técnica

A regra conservadora foi adotada porque:

- prontuário logicamente deletado ainda representa histórico clínico preservado;
- a retenção inicial de `MedicalRecord` é indefinida;
- Soft Delete deve remover registros dos fluxos clínicos padrão, mas não apagar a existência histórica;
- índice único simples é mais seguro, previsível e auditável;
- múltiplos prontuários para o mesmo paciente criariam ambiguidade clínica e operacional.

## 6. Tensão resolvida entre Soft Delete e índice único

A Fase 6.4.3 documentou que `ExistsByPatientIdAsync` ignorava `IsDeleted = true`, permitindo criação quando o paciente tivesse apenas registro deletado. Esse comportamento foi substituído para criação: a validação de unicidade agora consulta existência total, incluindo deletados.

As consultas clínicas padrão continuam preservando o comportamento da Fase 6.4.3:

- `GetByIdAsync` ignora deletados;
- `GetByPatientIdAsync` ignora deletados;
- `ExistsByPatientIdAsync` continua representando existência ativa.

A diferença é que criação usa a nova verificação total.

## 7. Alterações em repository/validator/use cases

Alterações realizadas:

- `IMedicalRecordRepository` recebeu `ExistsIncludingDeletedByPatientIdAsync(long patientId)`;
- `MedicalRecordRepository` implementa a nova verificação sem filtro de `IsDeleted`;
- `MedicalRecordUniquenessValidator` passou a usar a verificação total;
- `CreateMedicalRecordUseCase` mantém a mesma orquestração, mas passa a bloquear recriação após Soft Delete por meio do validator;
- fakes de testes foram ajustados para diferenciar existência ativa e existência total.

## 8. Alterações em EF/migration

Alterações estruturais realizadas:

- `MedicalRecordConfiguration` configura `HasIndex(m => m.PatientId).IsUnique()`;
- migration `AddUniqueIndexToMedicalRecordPatientId` remove o índice não único anterior e recria `IX_MedicalRecords_PatientId` como único;
- a migration preserva a FK `FK_MedicalRecords_Patients_PatientId` com `Restrict`;
- `AppDbContextModelSnapshot` foi atualizado para refletir o índice único.

Ambientes com dados reais devem validar duplicidades antes de aplicar a migration. A fase não implementa saneamento automático destrutivo.

## 9. Testes criados/alterados

Testes de Infrastructure:

- configuração EF marca `PatientId` como índice único;
- banco bloqueia dois `MedicalRecords` ativos para o mesmo `PatientId`;
- banco bloqueia novo `MedicalRecord` quando o anterior do mesmo paciente está logicamente deletado;
- banco permite `MedicalRecords` para pacientes diferentes;
- verificação total retorna `true` para registro deletado;
- queries padrão continuam ignorando registros deletados.

Testes de Application:

- criação válida sem registro anterior continua permitida;
- criação com registro ativo anterior continua bloqueada;
- criação com registro deletado anterior agora retorna conflito de unicidade.

## 10. Impacto em criação após Soft Delete

O comportamento anterior de recriação após Soft Delete deixa de ser permitido. Quando existe qualquer `MedicalRecord` para o `PatientId`, ativo ou deletado, o use case de criação retorna conflito com a mensagem já existente: `Patient already has a medical record.`

## 11. Confirmação sobre queries padrão

A fase preserva o comportamento clínico padrão de leitura:

- registros deletados não são retornados por `GetByIdAsync`;
- registros deletados não são retornados por `GetByPatientIdAsync`;
- `ExistsByPatientIdAsync` continua ignorando deletados e representa existência ativa.

## 12. Confirmação de escopo não implementado

Não foram criados:

- endpoint novo;
- endpoint DELETE;
- consulta administrativa de registros deletados;
- restore;
- remoção de Soft Delete;
- alteração de AuditLog;
- alteração de autorização;
- alteração de JWT/User/Profile;
- alteração de frontend;
- alteração de Docker, Redis, RabbitMQ ou Kubernetes;
- retenção ou expurgo.

## 13. Riscos remanescentes

Riscos remanescentes:

- ambientes com dados legados duplicados falharão ao aplicar a migration até que sejam saneados manualmente;
- não há consulta administrativa para visualizar ou restaurar prontuários deletados;
- não há política de expurgo ou retenção operacional além da decisão de retenção indefinida inicial;
- `MR-DEBT-009` permanece aberto para validação estrutural de `FlagsJson`.

## 14. Critérios de aceite

A Fase 6.5.2 atende aos critérios quando:

- a decisão de unicidade total está documentada;
- `MedicalRecord` é tratado como 1:1 total por `Patient`;
- existe índice único físico em `MedicalRecords.PatientId`;
- migration foi criada;
- validator/use case bloqueiam criação quando há registro deletado do mesmo paciente;
- banco bloqueia duplicidade fora da validação lógica;
- testes de Infrastructure cobrem duplicidade ativa e duplicidade com registro deletado;
- testes de Application cobrem criação bloqueada após Soft Delete;
- queries padrão continuam ignorando registros deletados;
- `MR-DEBT-007` foi atualizado no registro vivo;
- `MR-DEBT-009` permaneceu fora do escopo.

## 15. Fora do escopo

Permanecem fora do escopo desta fase:

- validação JSON;
- normalização de `FlagsJson`;
- resolução de `MR-DEBT-009`;
- endpoints novos;
- restore;
- consultas administrativas;
- retenção ou expurgo;
- alterações de segurança/autorização/frontend/infraestrutura.

## 16. Próxima fase recomendada

Próxima fase recomendada:

**Fase 6.5.3 — Validação estrutural inicial de FlagsJson.**

Objetivo recomendado: rejeitar JSON inválido e preservar `null`/vazio como ausência de flags, sem normalização prematura.

## 17. Atualização do registro vivo

`MR-DEBT-007` passa a ser considerado resolvido tecnicamente por índice único físico em `MedicalRecords.PatientId`.

Evidências:

- índice único físico em `PatientId`;
- validator alinhado com unicidade total;
- testes de infraestrutura bloqueando duplicidade ativa e duplicidade após Soft Delete;
- testes de Application bloqueando criação após Soft Delete.

Risco original mitigado: duplicidade física em concorrência extrema ou escrita fora da aplicação.

Mitigação atual: constraint física + validação lógica coerente.

Fase futura recomendada para este débito: tratado na Fase 6.5.2; evidências finais podem ser consolidadas em Fase 6.5.x de encerramento.
