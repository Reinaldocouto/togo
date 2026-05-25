# TOGO — Fase 5.3.2: Interface IMedicalRecordRepository

## Resumo da Subfase 5.3

Subfase 5.3 — Application MedicalRecord

Planejamento:
- 5.3.1 — Contracts de MedicalRecord.
- 5.3.2 — Interface IMedicalRecordRepository.
- 5.3.3 — Validators de MedicalRecord.
- 5.3.4 — Use cases de MedicalRecord.
- 5.3.5 — Testes de Application.
- 5.3.6 — Documentação final da camada Application.
- 5.3.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Criar a abstração de persistência da camada Application para `MedicalRecord`, preparando validators e use cases futuros sem acoplar Application à Infrastructure/EF Core.

## Contexto

- `MedicalRecord` já possui domínio validado.
- Os contracts foram criados na Fase 5.3.1.
- A camada Application agora precisa de uma interface de persistência para a vertical de prontuário.
- A implementação concreta será criada somente na fase de Infrastructure.
- O prontuário é consultado principalmente por `patientId`.
- Um `Patient` deve ter no máximo um `MedicalRecord` principal.
- Delete físico de prontuário não faz parte do MVP.

## Interface criada

- Caminho: `backend/src/Togo.Application/MedicalRecords/Repositories/IMedicalRecordRepository.cs`
- Namespace: `Togo.Application.MedicalRecords.Repositories`
- Métodos criados:
  - `Task<MedicalRecord?> GetByIdAsync(long id);`
    - Responsabilidade: leitura por identificador técnico do prontuário.
  - `Task<MedicalRecord?> GetByPatientIdAsync(long patientId);`
    - Responsabilidade: leitura principal para fluxo orientado por paciente.
  - `Task<bool> ExistsByPatientIdAsync(long patientId);`
    - Responsabilidade: suporte à regra de unicidade de prontuário por paciente.
  - `Task AddAsync(MedicalRecord medicalRecord);`
    - Responsabilidade: persistência futura de criação.
  - `Task UpdateAsync(MedicalRecord medicalRecord);`
    - Responsabilidade: persistência futura de atualização.

Motivos de exclusão de métodos nesta fase:
- `ListAsync` não foi incluído porque não há endpoint de listagem planejado para o MVP de prontuário.
- `DeleteAsync` não foi incluído porque delete físico não é permitido neste momento e Soft Delete será tratado em fase futura.

## Decisões técnicas

- A interface permanece na camada Application.
- A implementação concreta será responsabilidade da camada Infrastructure.
- A interface depende apenas da entidade de domínio `MedicalRecord`.
- A interface não depende de EF Core.
- A interface não expõe `IQueryable`.
- A interface não expõe `DbSet`.
- A interface não conhece API/HTTP.
- A interface não resolve regra de negócio sozinha.
- `ExistsByPatientIdAsync` prepara a regra de unicidade por paciente.
- `GetByPatientIdAsync` suporta o fluxo da rota `/api/patients/{patientId}/medical-record`.
- `DeleteAsync` ficou fora por decisão clínica atual e evolução futura com Soft Delete.

## Pontos de atenção

- A unicidade por `PatientId` ainda não é garantida no banco.
- `DeleteBehavior.Cascade` ainda exige revisão em fase futura.
- Soft Delete e AuditLog continuam fora do escopo atual.
- O repository concreto ainda não existe.
- Validators e use cases ainda não existem.
- Na fase de Infrastructure, a implementação concreta será responsável por `AsNoTracking` em leitura e `SaveChangesAsync` em escrita.

## Critérios de aceite

A fase será considerada concluída se:

- A pasta `MedicalRecords/Repositories` existir.
- `IMedicalRecordRepository.cs` for criado.
- A interface possuir `GetByIdAsync`.
- A interface possuir `GetByPatientIdAsync`.
- A interface possuir `ExistsByPatientIdAsync`.
- A interface possuir `AddAsync`.
- A interface possuir `UpdateAsync`.
- A interface não possuir `DeleteAsync`.
- A interface não possuir `ListAsync`.
- A interface não depender de EF Core.
- A interface não expor `IQueryable`/`DbSet`.
- A documentação da fase for criada.
- Nenhuma implementação concreta for criada.
- Nenhuma migration for criada.
- Nenhuma camada Infrastructure/API for alterada.
- `git diff --check` não apontar problemas.
- Build/test forem executados se SDK estiver disponível, ou a limitação for registrada.

## Fora do escopo

Esta fase não implementa:

- `MedicalRecordRepository` concreto;
- validators;
- use cases;
- controller;
- API;
- Infrastructure;
- migration;
- database update;
- Soft Delete;
- AuditLog;
- roles/permissões;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## Próxima fase recomendada

**Fase 5.3.3 — Validators de MedicalRecord.**

Objetivo:
Criar validators da camada Application para validar existência de `Patient`, unicidade de `MedicalRecord` por `Patient` e existência do prontuário quando necessário, preparando a implementação dos use cases.

Validators esperados para avaliação na próxima fase:
- `MedicalRecordPatientExistsValidator`;
- `MedicalRecordUniquenessValidator`;
- `MedicalRecordExistsValidator`, se necessário.

## Validações obrigatórias

Comandos obrigatórios desta fase:

- `git branch --show-current`
- `git status --short`
- `git diff --check`

Comandos condicionais (se SDK .NET disponível):

- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Se `dotnet` não estiver disponível no ambiente, a limitação deve ser registrada sem inventar resultados.
