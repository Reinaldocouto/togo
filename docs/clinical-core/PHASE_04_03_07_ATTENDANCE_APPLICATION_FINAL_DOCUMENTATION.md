# TOGO — Fase 4.3.7: Documentação Final da Application Attendance

## 1. Objetivo

Esta fase consolida a camada **Application** da vertical Attendance, registrando de forma unificada:

- contracts de entrada/saída;
- interface de repositório;
- validators;
- use cases;
- testes e fake repository;
- validações e decisões técnicas;
- incidentes/aprendizados operacionais;
- próximos passos para evolução da vertical.

## 2. Contexto

- O domínio de Attendance foi fechado na **Fase 4.2**.
- A **Fase 4.3** implementou e estabilizou a camada Application de Attendance.
- Nesta etapa, **não** houve implementação de Infrastructure/API.
- Com os artefatos atuais, a vertical Attendance possui base suficiente para avançar para repository concreto e exposição por API em fase posterior.

## 3. Escopo consolidado da Fase 4.3

### 4.3.1 — Contracts
- **Objetivo:** criar DTOs de request/response da Application para Attendance.
- **Arquivos principais:** contracts em `Attendances/Contracts`.
- **Decisão técnica:** separar DTO de Application da entidade de domínio.
- **Resultado:** contratos estáveis para create/get/list/close.

### 4.3.2 — IAttendanceRepository
- **Objetivo:** definir contrato de persistência/consulta sem acoplar à infraestrutura.
- **Arquivos principais:** `IAttendanceRepository.cs`.
- **Decisão técnica:** Application depende de abstração, não de EF/banco.
- **Resultado:** base para use cases e testes por fake.

### 4.3.3 — Validators
- **Objetivo:** validar pré-condições de negócio antes de criar/alterar Attendance.
- **Arquivos principais:** `AttendancePatientExistsValidator`, `AttendanceNumberUniqueValidator`, `OpenAttendanceValidator`.
- **Decisão técnica:** retorno padronizado via `ApplicationResult<bool>`.
- **Resultado:** validações reutilizáveis e testáveis isoladamente.

### 4.3.3.1 — FakeAttendanceRepository fix
- **Objetivo:** corrigir comportamento incorreto de fake com entidades `Id = 0`.
- **Arquivos principais:** `FakeAttendanceRepository.cs` e testes do fake.
- **Decisão técnica:** armazenamento em lista, não indexado apenas por Id default.
- **Resultado:** fake confiável para cenários de create/list/update.

### 4.3.4 — CreateAttendanceUseCase
- **Objetivo:** implementar fluxo de criação de Attendance.
- **Arquivos principais:** `CreateAttendanceUseCase.cs` e testes correspondentes.
- **Decisão técnica:** encadear validators e mapear entidade para DTO de response.
- **Resultado:** criação com validações, persistência e mapeamento padronizados.

### 4.3.4.1 — Exception handling fix
- **Objetivo:** corrigir tratamento de exceções no create.
- **Arquivos principais:** `CreateAttendanceUseCase.cs` + testes.
- **Decisão técnica:** capturar `ArgumentException` (base) para mapear erro de domínio esperado.
- **Resultado:** conversão consistente para `ValidationError`.

### 4.3.5 — Get/List use cases
- **Objetivo:** implementar leitura por id e listagem.
- **Arquivos principais:** `GetAttendanceByIdUseCase.cs`, `ListAttendancesUseCase.cs` + testes.
- **Decisão técnica:** validação de id e mapeamento explícito para DTOs.
- **Resultado:** consultas estáveis com retorno `NotFound`/`Success`.

### 4.3.6 — Close/Cancel use cases
- **Objetivo:** implementar transições de estado da entidade.
- **Arquivos principais:** `CloseAttendanceUseCase.cs`, `CancelAttendanceUseCase.cs` + testes.
- **Decisão técnica:** estado é alterado no domínio, Application orquestra e persiste.
- **Resultado:** fechamento/cancelamento com conflitos e validações tratados.

### 4.3.6.1 — Incidentes e aprendizados
- **Objetivo:** consolidar lições operacionais/técnicas da fase.
- **Arquivos principais:** documento da fase 4.3.6.1.
- **Decisão técnica:** registrar explicitamente riscos recorrentes de fluxo e qualidade.
- **Resultado:** guia prático para evitar reincidência em próximas fases.

## 4. Contracts criados

### CreateAttendanceRequest

Campos:
- `PatientId`;
- `AttendanceNumber`;
- `OpenedAt`;
- `Type`.

Não recebe `Status` nem `ClosedAt`, porque a criação é governada pelo domínio: Attendance nasce como **Open** e `ClosedAt` inicia `null`.

### AttendanceResponse

Campos:
- `Id`;
- `PatientId`;
- `AttendanceNumber`;
- `OpenedAt`;
- `ClosedAt`;
- `Status`;
- `Type`.

### AttendanceListItemResponse

Campos:
- `Id`;
- `PatientId`;
- `AttendanceNumber`;
- `OpenedAt`;
- `ClosedAt`;
- `Status`;
- `Type`.

### CloseAttendanceRequest

Campo:
- `ClosedAt`.

Não foi criado `CancelAttendanceRequest`, pois o cancelamento nesta fase não recebe payload de entrada.

## 5. Repository interface

A interface `IAttendanceRepository` foi documentada com os métodos:

- `GetByIdAsync`;
- `ListAsync`;
- `ListByPatientIdAsync`;
- `AddAsync`;
- `UpdateAsync`;
- `ExistsByAttendanceNumberAsync`;
- `HasOpenAttendanceForPatientAsync`.

Consolidação técnica:
- a Application depende exclusivamente dessa abstração;
- a Infrastructure ainda não foi implementada na Fase 4.3;
- use cases não têm acesso direto a EF Core/banco.

## 6. Validators criados

### AttendancePatientExistsValidator
- valida `PatientId`;
- utiliza `IPetRepository.GetByPatientIdAsync`;
- retorna `ValidationError`, `NotFound` ou `Success`.

### AttendanceNumberUniqueValidator
- valida obrigatoriedade de `AttendanceNumber`;
- normaliza com `Trim()`;
- usa `IAttendanceRepository.ExistsByAttendanceNumberAsync`;
- retorna `Conflict` quando número já existe.

### OpenAttendanceValidator
- valida `PatientId`;
- usa `IAttendanceRepository.HasOpenAttendanceForPatientAsync`;
- bloqueia múltiplos atendimentos **Open** para o mesmo paciente.

## 7. Use cases criados

### CreateAttendanceUseCase

Fluxo:
1. valida existência do Patient;
2. valida unicidade de `AttendanceNumber`;
3. valida existência prévia de Attendance Open;
4. cria `Attendance` via domínio;
5. persiste com `AddAsync`;
6. retorna `AttendanceResponse`.

Erros:
- falhas de validator interrompem o fluxo e são propagadas;
- `ArgumentException` do domínio é convertido em `ValidationError`.

### GetAttendanceByIdUseCase

Fluxo:
1. valida id;
2. busca via `GetByIdAsync`;
3. retorna `NotFound` se não existir;
4. retorna `AttendanceResponse` se existir.

### ListAttendancesUseCase

Fluxo:
1. consulta via `ListAsync`;
2. mapeia para `AttendanceListItemResponse`;
3. retorna lista, inclusive vazia.

### CloseAttendanceUseCase

Fluxo:
1. valida id;
2. busca Attendance;
3. retorna `NotFound` quando aplicável;
4. executa `Attendance.Close`;
5. persiste com `UpdateAsync`;
6. retorna `AttendanceResponse`.

Erros:
- `ArgumentException` -> `ValidationError`;
- `InvalidOperationException` -> `Conflict`.

### CancelAttendanceUseCase

Fluxo:
1. valida id;
2. busca Attendance;
3. retorna `NotFound` quando aplicável;
4. executa `Attendance.Cancel`;
5. persiste com `UpdateAsync`;
6. retorna `AttendanceResponse`.

Erros:
- `InvalidOperationException` -> `Conflict`.

## 8. Testes criados

### Validators

Cobertura consolidada:
- `AttendancePatientExistsValidatorTests`: id inválido, paciente inexistente, sucesso;
- `AttendanceNumberUniqueValidatorTests`: número vazio, duplicado, inexistente, trim antes da verificação;
- `OpenAttendanceValidatorTests`: id inválido, paciente com atendimento aberto, paciente sem atendimento aberto.

### Fake repository

Cobertura consolidada:
- `AddAsync` não sobrescreve entidades com Id default;
- `AddAttendanceForLookup` permite `GetByIdAsync` por id artificial;
- `UpdateAsync` incrementa `UpdateCallsCount`;
- `UpdateAsync` não colapsa a lista indevidamente.

### CreateAttendanceUseCaseTests

Cenários consolidados:
- sucesso;
- patient inexistente;
- attendance number duplicado;
- patient já possui Attendance Open;
- attendance number inválido;
- trim de attendance number;
- domínio lança `ArgumentException`.

### GetAttendanceByIdUseCaseTests

Cenários:
- id inválido;
- not found;
- success.

### ListAttendancesUseCaseTests

Cenários:
- lista vazia;
- lista preenchida.

### CloseAttendanceUseCaseTests

Cenários:
- id inválido;
- not found;
- success;
- `ClosedAt` default;
- `ClosedAt` anterior a `OpenedAt`;
- already closed;
- canceled.

### CancelAttendanceUseCaseTests

Cenários:
- id inválido;
- not found;
- success;
- closed;
- already canceled.

## 9. FakeAttendanceRepository

O fake existe para dar suporte a testes unitários de Application sem dependência de infraestrutura real.

Comportamentos suportados:
- `AddAsync`;
- `UpdateAsync`;
- `ListAsync`;
- `GetByIdAsync`;
- `ListByPatientIdAsync`;
- `ExistsByAttendanceNumberAsync`;
- `HasOpenAttendanceForPatientAsync`;
- `AddAttendanceForLookup`;
- contadores de chamada;
- proteção para cenários com `Id = 0`.

Registro importante:
- o fake é infraestrutura de teste;
- não representa um repository real de produção;
- o repository real será implementado na camada Infrastructure em fase futura.

## 10. Decisões técnicas consolidadas

- Application trabalha com DTOs próprios.
- Interface de repository retorna entidade de domínio.
- Use cases fazem mapeamento explícito para response.
- Regras internas de estado permanecem no domínio.
- Application orquestra validação e persistência.
- Sem eventos/RabbitMQ.
- Sem Redis/cache.
- Sem Docker/Kubernetes.
- Sem repository concreto.
- Sem controller/API nesta fase.
- Sem migration.
- Sem banco.

## 11. Incidentes e aprendizados da Fase 4.3

Consolidação operacional da fase 4.3.6.1:

- atenção a `default(DateTime)` vs `default` em nullable;
- cuidado com `Attendance.Id = 0` antes da persistência real;
- asserts devem refletir mensagens reais lançadas pelo domínio;
- validar branch correta antes de editar arquivos;
- limitação de ambiente Codex sem `dotnet` deve ser registrada claramente quando ocorrer;
- CI deve ser tratado como gate obrigatório;
- executar `git diff --check` para higiene final;
- fake repository deve evoluir com teste junto;
- evitar PR auxiliar contra branch errada;
- revisar conflitos documentais para remover marcadores residuais.

## 12. Validação local/CI

Conforme validação humana posterior à PR 87:

- `dotnet build backend/Togo.sln` passou localmente;
- `dotnet test backend/Togo.sln` passou localmente;
- total informado: **154** testes;
- **154** passaram;
- **0** falhas;
- **0** ignorados;
- cobertura próxima de **80%**, conforme validação no Visual Studio.

Também registrado:

- CI da PR 87 passou após correções;
- nesta fase documental, a verificação mínima obrigatória inclui ao menos `git diff --check`.

## 13. Estado final da camada Application Attendance

Ao final da Fase 4.3 existem:

- contracts;
- interface de repository;
- validators;
- use cases de create/get/list/close/cancel;
- testes unitários de Application;
- fake repository estabilizado;
- documentação técnica e aprendizados.

Ainda não existem nesta fase:

- repository concreto;
- controller/API;
- endpoints;
- DI em `Program.cs`;
- migrations novas;
- atualização de banco;
- integração com eventos;
- integração com cache.

## 14. Fora do escopo

- Domain;
- Infrastructure;
- API;
- Controllers;
- Endpoints;
- `Program.cs`;
- `AppDbContext`;
- EF configuration;
- Migrations;
- database update;
- Redis;
- RabbitMQ;
- Docker;
- Kubernetes;
- Frontend.

## 15. Próxima fase recomendada

**Fase 4.4 — Implementar Infrastructure de Attendance.**

Sugestão de início:

**Fase 4.4.1 — Criar AttendanceRepository concreto na Infrastructure.**

Objetivo:
Implementar a interface `IAttendanceRepository` usando EF Core/`AppDbContext`, respeitando os contratos definidos na Application e sem alterar regras de domínio.
