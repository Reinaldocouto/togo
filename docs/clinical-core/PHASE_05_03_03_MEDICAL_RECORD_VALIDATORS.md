# TOGO — Fase 5.3.3: Validators de MedicalRecord

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

Criar validators na camada Application para preparar os futuros use cases de MedicalRecord, sem implementar use cases nesta fase.

## Contexto

- Os contracts de MedicalRecord foram criados na Fase 5.3.1.
- A interface `IMedicalRecordRepository` foi criada na Fase 5.3.2.
- O fluxo de MedicalRecord precisa validar se o `Patient` existe.
- Um `Patient` deve ter no máximo um `MedicalRecord` principal.
- Consultas e updates precisam validar existência prévia de prontuário.
- Validators não devem conhecer HTTP.
- Validators não devem acessar EF Core diretamente.

## Validators criados

### 1) MedicalRecordPatientExistsValidator

- **Finalidade:** validar existência do `Patient` antes de criar, consultar ou atualizar prontuário.
- **Dependências:** `IPetRepository` e `ILogger<MedicalRecordPatientExistsValidator>`.
- **Método criado:** `ValidateAsync(long patientId, CancellationToken cancellationToken)`.
- **Retorno:** `Task<ApplicationResult<bool>>`.
- **Regra protegida:** `patientId` precisa ser maior que zero e o paciente precisa existir no repositório.
- **Uso futuro esperado:** use cases poderão mapear retorno de validação/not-found sem acoplamento a HTTP.

### 2) MedicalRecordUniquenessValidator

- **Finalidade:** impedir duplicidade lógica de prontuário por paciente antes da criação.
- **Dependências:** `IMedicalRecordRepository` e `ILogger<MedicalRecordUniquenessValidator>`.
- **Método criado:** `ValidateAsync(long patientId, CancellationToken cancellationToken)`.
- **Retorno:** `Task<ApplicationResult<bool>>`.
- **Regra protegida:** valida `patientId > 0` e verifica `ExistsByPatientIdAsync(patientId)`.
- **Uso futuro esperado:** use cases poderão mapear conflito de unicidade para resposta de conflito (409) na camada de API.

### 3) MedicalRecordExistsValidator

- **Finalidade:** garantir que o prontuário do paciente exista antes de consulta/update.
- **Dependências:** `IMedicalRecordRepository` e `ILogger<MedicalRecordExistsValidator>`.
- **Método criado:** `ValidateAsync(long patientId, CancellationToken cancellationToken)`.
- **Retorno:** `Task<ApplicationResult<bool>>`.
- **Regra protegida:** valida `patientId > 0` e verifica existência via `ExistsByPatientIdAsync(patientId)`.
- **Uso futuro esperado:** use cases poderão mapear ausência para not-found (404) na camada de API.

## Decisões técnicas

- Validators ficam na camada Application.
- Validators não acessam `AppDbContext`.
- Validators não dependem de EF Core.
- Validators não dependem de ASP.NET Core.
- Validators não retornam `IActionResult`.
- Validators não implementam status HTTP.
- Validators preparam integração com futuros `ApplicationResult` nos use cases.
- `MedicalRecordPatientExistsValidator` segue o padrão de `AttendancePatientExistsValidator`.
- `MedicalRecordUniquenessValidator` prepara conflito lógico para futuro mapeamento de Conflict 409.
- `MedicalRecordExistsValidator` prepara ausência lógica para futuro mapeamento de NotFound 404.

## Segurança e privacidade

- Validators não devem logar `GeneralNotes`.
- Validators não devem logar `FlagsJson`.
- Validators operam por IDs e metadados de validação.
- A validação evita vazamento de dados clínicos sensíveis.

## Pontos de atenção

- A unicidade por `PatientId` ainda não está garantida no banco.
- Validators reduzem risco lógico, mas não substituem constraint/migration futura.
- O repository concreto ainda não existe.
- Os use cases ainda não existem.
- Testes de Application serão criados em fase futura.
- Soft Delete e AuditLog continuam fora do escopo.

## Critérios de aceite

A fase será considerada concluída se:

- A pasta `MedicalRecords/Validators` existir.
- `MedicalRecordPatientExistsValidator` for criado.
- `MedicalRecordUniquenessValidator` for criado.
- `MedicalRecordExistsValidator` for criado.
- Validators seguirem padrão dos validators existentes.
- Validators não dependerem de EF Core.
- Validators não acessarem `AppDbContext`.
- Validators não conhecerem HTTP/IActionResult.
- Validators não implementarem use cases.
- Documentação da fase for criada.
- Nenhuma implementação concreta de repository for criada.
- Nenhuma migration for criada.
- Nenhuma camada Infrastructure/API for alterada.
- `git diff --check` não apontar problemas.
- Build/test forem executados se SDK estiver disponível, ou a limitação for registrada.

## Fora do escopo

Esta fase não implementa:

- use cases;
- controller;
- API;
- repository concreto;
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

**Fase 5.3.4 — Use cases de MedicalRecord.**

Objetivo:
Criar os casos de uso da camada Application para criar, consultar por `patientId` e atualizar `MedicalRecord`, usando contracts, repository interface e validators criados nas fases anteriores.

Use cases esperados para avaliação na próxima fase:

- `CreateMedicalRecordUseCase`;
- `GetMedicalRecordByPatientIdUseCase`;
- `UpdateMedicalRecordUseCase`.

## Validações obrigatórias

Executar:

- `git branch --show-current`
- `git status --short`
- `git diff --check`

Se dotnet estiver disponível, executar:

- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Se dotnet não estiver disponível, registrar a limitação sem inventar resultados.
