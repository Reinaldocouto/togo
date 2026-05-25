# TOGO — Fase 5.3.6: Documentação final da camada Application MedicalRecord

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

Esta fase consolida o estado final da camada Application de MedicalRecord, documentando:

- contratos criados;
- repository interface;
- validators;
- use cases;
- testes unitários;
- correção 5.3.4.1;
- decisões técnicas;
- débitos remanescentes;
- autorização para avançar para Infrastructure.

## Contexto

- A subfase 5.2 Domain foi encerrada.
- MedicalRecord já possui domínio testado.
- A subfase 5.3 implementou a camada Application.
- A Application ainda não possui repository concreto.
- Infrastructure/API ainda estão fora do escopo.
- Prontuário não é atendimento.
- `patientId` vem pela rota.
- Dados clínicos são sensíveis.

## 5. Entregas consolidadas da subfase 5.3

### 5.1 Contracts

Contracts consolidados:

- `CreateMedicalRecordRequest`;
- `UpdateMedicalRecordRequest`;
- `MedicalRecordResponse`;
- `MedicalRecordListItemResponse`.

Consolidação funcional:

- Requests não possuem `PatientId`.
- `PatientId` virá pela rota.
- Response detalhada pode conter `GeneralNotes`/`FlagsJson`.
- List item evita conteúdo clínico completo.

### 5.2 Repository Interface

Interface consolidada:

- `IMedicalRecordRepository`.

Métodos consolidados:

- `GetByIdAsync`;
- `GetByPatientIdAsync`;
- `ExistsByPatientIdAsync`;
- `AddAsync`;
- `UpdateAsync`.

Decisões de desenho:

- Interface fica na Application.
- Implementação concreta virá na Infrastructure.
- Não expõe EF Core, `DbSet` ou `IQueryable`.
- Não possui `DeleteAsync` por decisão clínica.
- Não possui `ListAsync` por não haver listagem MVP.

### 5.3 Validators

Validators consolidados:

- `MedicalRecordPatientExistsValidator`;
- `MedicalRecordUniquenessValidator`;
- `MedicalRecordExistsValidator`.

Consolidação funcional:

- Validam existência de `Patient`.
- Validam unicidade lógica de prontuário por `Patient`.
- Validam existência de `MedicalRecord` para consulta/update.
- Retornam `ApplicationResult<bool>`.
- Não conhecem HTTP.
- Não acessam EF Core diretamente.

### 5.4 Use cases

Use cases consolidados:

- `CreateMedicalRecordUseCase`;
- `GetMedicalRecordByPatientIdUseCase`;
- `UpdateMedicalRecordUseCase`.

Consolidação funcional:

- Retornam `ApplicationResult<MedicalRecordResponse>`.
- Usam validators.
- Usam `IMedicalRecordRepository`.
- Não retornam `IActionResult`.
- Não conhecem HTTP.
- Não acessam `AppDbContext`.
- Não dependem de EF Core.
- Usam `DateTime.UtcNow` para `UpdatedAt`.
- Não logam `GeneralNotes`/`FlagsJson`.

### 5.5 Testes de Application

Estrutura de testes consolidada:

- `FakeMedicalRecordRepository`;
- `MedicalRecordPatientExistsValidatorTests`;
- `MedicalRecordUniquenessValidatorTests`;
- `MedicalRecordExistsValidatorTests`;
- `CreateMedicalRecordUseCaseTests`;
- `GetMedicalRecordByPatientIdUseCaseTests`;
- `UpdateMedicalRecordUseCaseTests`.

Consolidação de cobertura:

- Testes são unitários.
- Não usam banco real.
- Não usam EF Core.
- Não usam API/controller.
- Validam `ApplicationResult`.
- Cobrem fluxos positivos e negativos.
- Cobrem chamadas `AddAsync`/`UpdateAsync`.
- Cobrem proteção de logs sensíveis onde aplicável.

## 6. Correção 5.3.4.1

Correção pontual registrada:

- O PR 115 inicialmente falhou por CS0160.
- Havia `catch` de `ArgumentException` antes de `catch` de `ArgumentOutOfRangeException`.
- `ArgumentOutOfRangeException` herda de `ArgumentException`.
- Os catches redundantes foram removidos.
- A correção não alterou regra de negócio.
- Build/test passaram posteriormente em ambiente com SDK/CI.

## 7. Segurança e privacidade

- `GeneralNotes` e `FlagsJson` são dados clínicos sensíveis.
- Use cases não devem logar esses campos.
- Testes usam dados fictícios.
- Listagens futuras devem preferir `MedicalRecordListItemResponse`.
- Erros técnicos não devem expor payload clínico.
- A camada Application opera por IDs/metadados quando possível.

## 8. Decisões técnicas finais da subfase 5.3

- Application está pronta para avançar para Infrastructure.
- `PatientId` não entra no body dos requests.
- Prontuário é orientado por `Patient`.
- Delete físico não faz parte do MVP.
- Repository concreto será criado na Fase 5.4.
- API/controller serão criados em fase posterior.
- Soft Delete e AuditLog continuam fora do escopo.
- Unicidade por `PatientId` ainda precisa de reforço em Infrastructure/DB.
- `CancellationToken` ainda não é propagado por `IMedicalRecordRepository` e permanece como ponto de atenção.

## 9. Débitos técnicos remanescentes

| Débito | Motivo | Risco | Fase futura recomendada |
|---|---|---|---|
| Repository concreto | A interface existe, mas sem implementação | Fluxos não executáveis fora de testes unitários | 5.4.1 |
| Registro em DI/Program.cs | Dependências de MedicalRecord ainda não registradas | Falha de resolução em runtime da API | 5.4.2 |
| Infrastructure tests | Ainda não há validação do repository real | Regressões de persistência podem passar despercebidas | 5.4.3 |
| Controller/API | Endpoints ainda não implementados | Vertical indisponível para consumo HTTP | 5.5.x |
| Testes de API | Ausência de cobertura de contrato HTTP | Quebras de integração sem detecção automática | 5.5.x |
| Unicidade por PatientId no banco | Regra ainda lógica (Application) | Possível duplicidade em cenário de concorrência | 5.4.4 |
| Revisão de DeleteBehavior.Cascade | Revisão clínica ainda pendente | Exclusão em cascata indesejada | 5.4.4 |
| Soft Delete | Fora do escopo MVP atual | Risco de perda de histórico clínico | Fase futura dedicada |
| AuditLog | Fora do escopo MVP atual | Falta de trilha formal de auditoria | Fase futura dedicada |
| Roles/permissões finas | Segurança granular ainda não modelada | Acesso além do necessário | Fase futura de segurança/API |
| Propagação de CancellationToken no IMedicalRecordRepository | Interface não recebe token | Menor capacidade de cancelamento cooperativo | Evolução Application/Infrastructure |
| Validação estrutural futura de FlagsJson | Campo permanece flexível | Inconsistência de estrutura sem padrão formal | Evolução de validators/domain |

## 10. Riscos aceitos temporariamente

- Ausência de repository concreto.
- Ausência de API/controller.
- Unicidade por `PatientId` ainda lógica, não física.
- `DeleteBehavior.Cascade` ainda pendente.
- Soft Delete pendente.
- AuditLog pendente.
- `FlagsJson` ainda flexível.
- `CancellationToken` ainda não propagado na interface de repository.

Esses riscos não bloqueiam o avanço para Infrastructure, mas precisam permanecer rastreados até tratamento nas fases correspondentes.

## 11. Critérios de aceite da subfase 5.3

A subfase 5.3 será considerada concluída se:

- contracts existirem;
- `IMedicalRecordRepository` existir;
- validators existirem;
- use cases existirem;
- testes de Application existirem;
- build/test passarem no CI ou em ambiente local;
- documentação final for criada;
- nenhuma migration tiver sido criada;
- nenhuma Infrastructure/API tiver sido implementada antes da fase correta;
- próxima fase 5.4 for recomendada.

## 12. Fora do escopo

Esta fase não implementa:

- código;
- testes;
- repository concreto;
- controller;
- API;
- Infrastructure;
- migration;
- database update;
- Program.cs/DI;
- Soft Delete;
- AuditLog;
- roles/permissões;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## 13. Decisão final da subfase 5.3

**Opção A — Subfase 5.3 aprovada para encerramento.**

Justificativa:
A camada Application de MedicalRecord possui contracts, repository interface, validators, use cases, testes unitários e documentação suficiente para avançar para a camada Infrastructure.

## 14. Próxima fase recomendada

Recomendação:

**Fase 5.4.1 — Repository EF Core de MedicalRecord.**

Como esta será a abertura da subfase maior 5.4, o próximo documento deve iniciar com:

Subfase 5.4 — Infrastructure MedicalRecord

Planejamento sugerido:
- 5.4.1 — Repository EF Core de MedicalRecord.
- 5.4.2 — Registro de DI/Program.cs para MedicalRecord.
- 5.4.3 — Testes de Infrastructure de MedicalRecord.
- 5.4.4 — Validação EF/AppDbContext/Migration existente.
- 5.4.5 — Documentação final da camada Infrastructure.
- 5.4.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

Objetivo da próxima fase:
Criar a implementação concreta de `IMedicalRecordRepository` na Infrastructure usando EF Core/AppDbContext, respeitando `AsNoTracking` em leituras, `SaveChangesAsync` em escrita e sem criar migration prematura.

## 15. Validações obrigatórias

Comandos obrigatórios desta fase:

- `git branch --show-current`
- `git status --short`
- `git diff --check`

Comandos condicionais (se `dotnet` disponível):

- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Registro de execução deve refletir o resultado real, sem inventar sucesso/falha.
