# TOGO — Fase 5.2.2: Ajustes de domínio de MedicalRecord

## Resumo da Subfase 5.2

Subfase 5.2 — Domain MedicalRecord

Planejamento:
- 5.2.1 — Testes de domínio de MedicalRecord.
- 5.2.2 — Ajustes de domínio, se os testes revelarem necessidade.
- 5.2.3 — Documentação final do domínio MedicalRecord.
- 5.2.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Verificar se os testes de domínio criados na Fase 5.2.1 exigem algum ajuste pontual na entidade `MedicalRecord`.

## Contexto

- `MedicalRecord` já existe no Domain.
- `MedicalRecordTests.cs` foi criado na Fase 5.2.1.
- Os testes cobrem `Create` e `UpdateNotes`.
- A entidade ainda não possui `CreatedAt`, Soft Delete ou AuditLog.
- Esses pontos continuam fora do escopo desta fase.

## Validação executada

Comandos executados nesta fase:

- `git branch --show-current`
- `git status --short`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Limitação de ambiente registrada:

- O comando `dotnet` não está disponível no ambiente (`/bin/bash: line 1: dotnet: command not found`).
- Portanto, build/test não puderam ser executados nesta fase.

## Resultado da análise

Cenário aplicado: **Validação limitada por ausência do SDK**.

Resultado técnico:

- Foi realizada revisão estática de `MedicalRecord.cs` e `MedicalRecordTests.cs`.
- As regras de domínio observadas (`PatientId > 0`, `UpdatedAt != default`, normalização de opcionais com `Trim` e `null` para vazio/branco, preservação de `PatientId` e `Id` em `UpdateNotes`) permanecem aderentes aos testes da Fase 5.2.1.
- Não foi identificada inconsistência objetiva que justificasse ajuste em `MedicalRecord.cs` nesta fase.

## Alterações realizadas

- `MedicalRecord.cs` **não foi alterado**.
- `MedicalRecordTests.cs` **não foi alterado**.
- Nenhuma migration foi criada.
- Nenhuma camada fora de **Domain.Tests/docs** foi alterada.
- Foi criado o documento desta fase para registrar o resultado confirmatório com limitação de SDK.

## Decisões técnicas

- Não implementar `CreatedAt` nesta fase.
- Não implementar Soft Delete nesta fase.
- Não implementar AuditLog nesta fase.
- Não alterar estrutura de `FlagsJson` nesta fase.
- Manter foco em comportamento mínimo de domínio.
- Não avançar para Application antes de fechar Domain.

## Pontos de atenção remanescentes

- `CreatedAt` continua ausente.
- Soft Delete continua pendente.
- AuditLog continua pendente.
- Unicidade por `PatientId` ainda não é garantida no banco.
- `DeleteBehavior.Cascade` ainda precisa de revisão futura.
- `FlagsJson` continua como débito técnico controlado.

## Critérios de aceite

A fase é considerada concluída porque:

- os testes da Fase 5.2.1 foram analisados estaticamente;
- a limitação do ambiente para build/test foi registrada sem inventar resultado;
- `MedicalRecord.cs` foi mantido sem alteração por ausência de evidência de falha real;
- nenhuma alteração fora do escopo foi feita;
- nenhuma migration foi criada;
- nenhuma camada Application/Infrastructure/API foi alterada;
- a documentação da fase foi criada;
- a próxima fase foi recomendada.

## Fora do escopo

Esta fase não implementa:

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

## Próxima fase recomendada

**Fase 5.2.3 — Documentação final do domínio MedicalRecord.**

Objetivo:

Consolidar o estado final da subfase 5.2, documentando testes de domínio, eventuais ajustes ou confirmação de que nenhum ajuste foi necessário, pontos pendentes e autorização para avançar para a Fase 5.3 — Application MedicalRecord.
