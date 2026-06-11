# Fase 6.5.2.1 — Tratamento de conflito de unicidade concorrente de MedicalRecord

## 1. Objetivo

Tratar a janela de concorrência remanescente da Fase 6.5.2 para que uma violação física esperada do índice único de `MedicalRecords.PatientId` seja retornada como conflito de negócio, preservando `ApplicationResultType.Conflict` e HTTP 409.

## 2. Contexto pós-PR 156

A PR 156 concluiu a Fase 6.5.2 e implementou unicidade física total de `MedicalRecord` por `Patient`, com índice único em `MedicalRecords.PatientId`, Soft Delete contando para unicidade, validator alinhado à constraint e testes de Application/Infrastructure.

## 3. Problema de concorrência identificado

O validator lógico consulta a existência antes da persistência, mas não é uma trava transacional. Em concorrência, duas requisições podem passar pelo validator e uma delas falhar apenas no `SaveChangesAsync` por violar a constraint física.

## 4. Cenário das duas requisições simultâneas

1. Requisição A consulta unicidade e não encontra prontuário.
2. Requisição B consulta unicidade no mesmo intervalo e também não encontra prontuário.
3. A persiste o `MedicalRecord`.
4. B tenta persistir o mesmo `PatientId`.
5. O banco rejeita B por `IX_MedicalRecords_PatientId`.
6. A falha de B deve virar conflito de negócio, não HTTP 500.

## 5. Decisão arquitetural

A tradução do erro físico fica na Infrastructure, onde EF Core e providers de banco são detalhes conhecidos. A Application recebe somente uma exceção própria de domínio de aplicação, sem depender de EF Core, MySQL ou SQLite.

## 6. Separação Application x Infrastructure

A Application não captura `DbUpdateException` e não adiciona `using Microsoft.EntityFrameworkCore`. O repository da Infrastructure captura `DbUpdateException` apenas para classificar a violação específica de unicidade de `MedicalRecords.PatientId`.

## 7. Exceção específica criada

Foi criada `MedicalRecordAlreadyExistsException`, independente de provider, representando exclusivamente a regra 1:1 total entre `MedicalRecord` e `Patient`. A exceção preserva `PatientId`, usa mensagem segura e aceita `InnerException` sem expor SQL ou payload clínico ao cliente.

## 8. Detecção restrita da constraint

Foi criado um detector interno de Infrastructure para reconhecer somente a violação de `MedicalRecords.PatientId`/`IX_MedicalRecords_PatientId`:

- SQLite em testes: código de constraint única e mensagem contendo `MedicalRecords.PatientId`;
- MySQL/Pomelo em produção: erro de duplicidade e referência ao índice `IX_MedicalRecords_PatientId` ou à tabela/coluna esperadas.

Outras `DbUpdateException`, como FK inválida, timeout, conexão, erro de coluna ou outros índices únicos, continuam sendo relançadas sem conversão para conflito.

## 9. Tradução para ApplicationResultType.Conflict

O `CreateMedicalRecordUseCase` captura `MedicalRecordAlreadyExistsException`, registra warning apenas com `PatientId` e retorna `ApplicationResultType.Conflict` com a mensagem `Patient already has a medical record.`.

## 10. Garantia de HTTP 409

O controller já mapeia `ApplicationResultType.Conflict` para `Conflict(...)`, resultando em HTTP 409. A fase não altera o middleware global e não transforma toda `DbUpdateException` em 409.

## 11. Garantia de ausência de AuditLog em falha

A ordem permanece: persistir `MedicalRecord` primeiro e escrever `MedicalRecord.Created` somente após sucesso. Se o insert falha por conflito de unicidade, o use case retorna conflito e nenhum evento de auditoria é escrito.

## 12. Testes criados

Foram adicionados/ajustados testes para cobrir:

- conflito durante `AddAsync` no use case;
- ausência de `ClinicalAuditLog` quando `AddAsync` falha;
- propagação de exceção de persistência não relacionada;
- tradução da duplicidade de `PatientId` para `MedicalRecordAlreadyExistsException`;
- preservação de `PatientId` e `InnerException`;
- manutenção do bloqueio com Soft Delete;
- não tradução de FK inválida como conflito;
- permanência do índice único e permissão para pacientes diferentes.

## 13. Arquivos alterados

- `backend/src/Togo.Application/MedicalRecords/Exceptions/MedicalRecordAlreadyExistsException.cs`;
- `backend/src/Togo.Application/MedicalRecords/UseCases/CreateMedicalRecordUseCase.cs`;
- `backend/src/Togo.Application/MedicalRecords/Validators/MedicalRecordUniquenessValidator.cs`;
- `backend/src/Togo.Application/Togo.Application.csproj`;
- `backend/src/Togo.Infrastructure/Repositories/MedicalRecordRepository.cs`;
- `backend/src/Togo.Infrastructure/Repositories/MedicalRecords/MedicalRecordUniqueConstraintDetector.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/MedicalRecordConfiguration.cs`;
- testes de Application e Infrastructure;
- documentação clínica da Fase 6.5.

## 14. Riscos remanescentes

- Ambientes com dados legados duplicados ainda precisam de saneamento prévio antes da migration da Fase 6.5.2.
- O detector depende de formatos conhecidos de SQLite e MySQL/Pomelo; novos providers exigirão classificação explícita.
- Não há endpoint administrativo para visualizar/restaurar prontuários deletados.

## 15. Fora do escopo

Ficam fora do escopo: nova migration, alteração de schema, alteração do índice único, saneamento automático de dados, validação/normalização de `FlagsJson`, endpoint DELETE, restore, consultas administrativas, mudanças de autorização/JWT/frontend/infraestrutura e tratamento global de toda `DbUpdateException`.

## 16. Critérios de aceite

A fase é aceita quando duplicidade concorrente retorna conflito/HTTP 409, a Application permanece desacoplada de EF Core, somente a constraint de `MedicalRecords.PatientId` é traduzida, erros não relacionados propagam, AuditLog não é escrito após falha, testes passam e nenhuma migration/alteração funcional de `FlagsJson` é criada.

## 17. Decisão final

A Fase 6.5.2.1 complementa `MR-DEBT-007` sem reabri-lo: o índice único continua sendo a garantia final de integridade e a aplicação agora traduz a violação concorrente esperada para conflito de negócio seguro.

## 18. Próxima fase recomendada

**6.5.3 — Validação estrutural inicial de FlagsJson.**
