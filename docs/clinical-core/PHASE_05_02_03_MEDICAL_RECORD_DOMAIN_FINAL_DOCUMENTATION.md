# TOGO — Fase 5.2.3: Documentação final do domínio MedicalRecord

## Resumo da Subfase 5.2

Subfase 5.2 — Domain MedicalRecord

Planejamento:
- 5.2.1 — Testes de domínio de MedicalRecord.
- 5.2.2 — Ajustes de domínio, se os testes revelarem necessidade.
- 5.2.3 — Documentação final do domínio MedicalRecord.
- 5.2.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Esta fase consolida o estado final da subfase 5.2 — Domain MedicalRecord, documentando:

- entidade atual;
- comportamento validado;
- testes criados;
- ausência de ajustes necessários;
- débitos técnicos remanescentes;
- decisão de encerramento da subfase 5.2;
- autorização para avançar para Application.

## Contexto

- `MedicalRecord` representa o prontuário principal longitudinal do `Patient`.
- Prontuário não é Atendimento.
- `Attendance` representa episódio/visita/evento clínico.
- `MedicalRecord` já existia antes da Fase 5.2.
- A Fase 5.2.1 criou testes de domínio.
- A Fase 5.2.2 confirmou que nenhum ajuste de domínio foi necessário.
- Esta fase não implementa código.

## Estado final da entidade MedicalRecord

Estado atual consolidado da entidade:

- namespace: `Togo.Domain.Entities`;
- propriedades:
  - `Id`;
  - `PatientId`;
  - `GeneralNotes`;
  - `FlagsJson`;
  - `UpdatedAt`;
- construtor privado para EF;
- factory `Create`;
- método `UpdateNotes`;
- validação de `PatientId` maior que zero;
- validação de `UpdatedAt` diferente de `default`;
- normalização de `GeneralNotes`;
- normalização de `FlagsJson`;
- tratamento de `null`/vazio/branco como `null`;
- preservação de `PatientId` em `UpdateNotes`;
- preservação de `Id` em `UpdateNotes`.

## Comportamento validado por testes

Comportamentos cobertos por `MedicalRecordTests.cs`:

- criação válida;
- `PatientId` inválido;
- `UpdatedAt` default no `Create`;
- normalização de `GeneralNotes` no `Create`;
- normalização de `FlagsJson` no `Create`;
- campos opcionais `null` no `Create`;
- campos opcionais vazios/brancos virando `null` no `Create`;
- `UpdateNotes` válido;
- normalização de `GeneralNotes` no `UpdateNotes`;
- normalização de `FlagsJson` no `UpdateNotes`;
- `UpdateNotes` com `null`;
- `UpdateNotes` com vazio/branco;
- `UpdatedAt` default no `UpdateNotes`;
- preservação de `PatientId`;
- preservação de `Id`.

## Resultado da Fase 5.2.2

- `MedicalRecord.cs` não foi alterado.
- `MedicalRecordTests.cs` não foi alterado.
- Não houve evidência objetiva de inconsistência na entidade.
- Não houve migration.
- Não houve alteração em Application, Infrastructure ou API.
- A validação com `dotnet` não pôde ser executada no ambiente por ausência do SDK.
- O resultado da 5.2.2 foi confirmatório.

## Decisões técnicas finais da subfase 5.2

- manter `MedicalRecord` como entidade mínima de domínio para MVP;
- não adicionar `CreatedAt` nesta subfase;
- não implementar Soft Delete nesta subfase;
- não implementar AuditLog nesta subfase;
- não alterar `FlagsJson` nesta subfase;
- não implementar unicidade por `PatientId` no domínio nesta subfase;
- deixar unicidade por `PatientId` para Application/Infrastructure/migration futura;
- avançar para Application somente após este fechamento.

## Débitos técnicos remanescentes

| Débito | Motivo | Risco | Fase futura recomendada |
|---|---|---|---|
| CreatedAt | Entidade MVP manteve apenas `UpdatedAt` | Perda de referência explícita de criação | Fase de evolução de domínio/clínico |
| Soft Delete | Fora do escopo da 5.2 | Risco de exclusão física e perda de histórico | Fase dedicada de Soft Delete |
| AuditLog | Fora do escopo da 5.2 | Falta de trilha de auditoria de alterações | Fase dedicada de auditoria clínica |
| Unicidade por PatientId | Regra não implementada no domínio/banco nesta subfase | Duplicidade de prontuário por paciente | Application + Infrastructure + migration |
| Revisão de DeleteBehavior.Cascade | Configuração atual precisa revisão clínica futura | Exclusão em cascata de dados clínicos | Fase de integridade de dados/migration |
| Normalização futura de FlagsJson | Modelo flexível aceito como MVP | Inconsistência estrutural de dados | Fase de refino de contrato/modelagem |
| Validação estrutural de FlagsJson | Não implementada nesta subfase | Entrada de conteúdo sem padrão mínimo | Fase de validators/Application |
| Controle de autoria | Não implementado nesta subfase | Falta de responsabilização por alteração | Fase de segurança/auditoria |
| Roles/permissões finas | Fora do escopo da subfase de domínio | Acesso clínico com granularidade insuficiente | Fase de segurança/API |

## Riscos aceitos temporariamente

- `UpdatedAt` sem `CreatedAt`;
- prontuário sem trilha de autoria;
- prontuário sem Soft Delete;
- `FlagsJson` flexível demais;
- unicidade por `PatientId` ainda não garantida no banco;
- `DeleteBehavior.Cascade` ainda existente;
- validação build/test pendente em ambiente com SDK.

Esses riscos não bloqueiam o avanço para Application nesta etapa, mas permanecem rastreados como passivos técnicos que exigem tratamento em fases futuras.

## Critérios de aceite da subfase 5.2

A subfase 5.2 será considerada concluída se:

- `MedicalRecordTests.cs` existir;
- testes cobrirem `Create`;
- testes cobrirem `UpdateNotes`;
- testes cobrirem validações;
- testes cobrirem normalização;
- testes cobrirem `null`/vazio/branco;
- testes cobrirem preservação de `PatientId`/`Id`;
- 5.2.2 confirmar se houve ou não necessidade de ajuste;
- documentação final do domínio for criada;
- nenhum código fora do domínio/testes for alterado;
- nenhuma migration for criada;
- a próxima fase 5.3 for recomendada.

## Fora do escopo

Esta fase não implementa:

- código;
- testes;
- `CreatedAt`;
- Soft Delete;
- AuditLog;
- contracts;
- repository;
- validators;
- use cases;
- controller;
- migration;
- database update;
- API;
- frontend;
- Redis;
- RabbitMQ;
- Docker.

## Decisão final da subfase 5.2

**Opção A — Subfase 5.2 aprovada para encerramento.**

Justificativa:
A entidade `MedicalRecord` possui comportamento mínimo adequado para MVP, está coberta por testes unitários de domínio, não exigiu ajuste imediato e está pronta para servir de base à camada Application.

## Próxima fase recomendada

**Fase 5.3.1 — Contracts de MedicalRecord.**

Objetivo:
Iniciar a subfase 5.3 — Application MedicalRecord criando os contratos de entrada e saída necessários para `Create`, `Update` e `Response` do prontuário, respeitando a decisão de rotas orientadas por `patientId`.

Também deve registrar o planejamento da subfase 5.3 no início da próxima fase.

Planejamento sugerido da subfase 5.3:

- 5.3.1 — Contracts de MedicalRecord.
- 5.3.2 — Interface IMedicalRecordRepository.
- 5.3.3 — Validators de MedicalRecord.
- 5.3.4 — Use cases de MedicalRecord.
- 5.3.5 — Testes de Application.
- 5.3.6 — Documentação final da camada Application.
- 5.3.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Validações obrigatórias

Comandos executados nesta fase:

- `git branch --show-current`
- `git status --short`
- `git diff --check`

Comandos de validação .NET:

- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Situação de ambiente:

- `dotnet` indisponível no ambiente (`command not found`), portanto build/test não puderam ser executados aqui.

## Encerramento da entrega

Entrega desta fase:

- criação de `docs/clinical-core/PHASE_05_02_03_MEDICAL_RECORD_DOMAIN_FINAL_DOCUMENTATION.md`;
- atualização exclusivamente documental;
- sem alteração de código, testes, migration, banco, Application, Infrastructure, API ou workflow.
