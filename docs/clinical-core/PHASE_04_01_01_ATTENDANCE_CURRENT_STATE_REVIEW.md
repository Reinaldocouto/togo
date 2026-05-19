# TOGO — Fase 4.1.1: Inspeção do estado atual de Atendimento/Attendance

## 1. Objetivo

Esta fase tem como objetivo inspecionar o estado atual do projeto TOGO antes de qualquer implementação nova de Atendimento, para evitar retrabalho, sobreposição de componentes e decisões prematuras de arquitetura.

## 2. Contexto

O projeto concluiu a Fase 3 com a vertical de Pet/Patient (entidade, repository, use cases, contracts, controller e testes).

Com isso, inicia-se a Fase 4 clínica focada em Atendimento.

A Fase 4.0.1 (documentada em `docs/architecture/PHASE_04_00_INFRA_CACHE_MESSAGING_DECISION.md`) definiu que Docker, Redis, RabbitMQ e Kubernetes fazem parte do roadmap futuro, mas não entram no escopo imediato desta etapa.

## 3. Resultado da inspeção

### Domain

**Encontrado (parcial):**

- Entidade `Attendance` já existente em domínio (`Togo.Domain.Entities.Attendance`).
- Enums já existentes:
  - `AttendanceStatus` (`Open`, `Closed`, `Canceled`);
  - `AttendanceType` (`Consultation`, `Emergency`, `Return`, `Procedure`, `Exam`, `Other`).
- Entidades clínicas relacionadas já referenciam `AttendanceId`:
  - `ClinicalEvolution`;
  - `Prescription`.

**Leitura técnica:** existe base de modelagem de domínio para Atendimento, ainda sem camada de aplicação/API correspondente.

### Application

**Não encontrado (para Attendance):**

- Não foram encontrados use cases de Attendance.
- Não foram encontrados contratos request/response de Attendance.
- Não foram encontrados validators de Attendance.
- Não foram encontrados mapeamentos/application services de Attendance.

**Observação:** o padrão completo existe para `Pets` e pode ser reaproveitado.

### Infrastructure

**Encontrado (parcial):**

- Configuração EF Core de `Attendance` já existente (`AttendanceConfiguration`).
- `DbSet<Attendance>` já existente em `AppDbContext`.
- Migração `20260428200839_AddClinicalCoreEntities` já contém tabela `Attendances` e índices.

**Não encontrado (para Attendance):**

- Não foi encontrado `AttendanceRepository` (implementação concreta).
- Não foi encontrada interface de repositório específica para Attendance na camada de aplicação (análogo a `IPetRepository`).

### API

**Não encontrado (para Attendance):**

- Não foi encontrado `AttendancesController`/`AttendanceController`.
- Não foram encontradas rotas `/api/attendances` implementadas.

### Tests

**Não encontrado (para Attendance):**

- Não foram encontrados testes de domínio específicos de `Attendance`.
- Não foram encontrados testes de aplicação (use cases/validators) para Attendance.
- Não foram encontrados testes de API para endpoints de Attendance.

### Documentation

**Encontrado:**

- Há referências arquiteturais e de planejamento sobre Atendimento em:
  - `docs/architecture/PHASE_04_00_INFRA_CACHE_MESSAGING_DECISION.md`;
  - `docs/CLINICAL_CORE_PLAN.md`;
  - `docs/PROJECT_EVOLUTION*.md`.
- Há diretrizes de evolução clínica mencionando Atendimento/Prontuário antes de mudanças de política de exclusão de Pet.

**Leitura técnica:** a documentação de roadmap existe, mas a vertical funcional de Attendance ainda não foi implementada ponta a ponta.

## 4. Referências existentes para reutilização

A vertical de `Pet/Patient` deve ser usada como referência arquitetural direta para Atendimento:

- **Padrão de entidade (Domain):** construtor privado + factory estático + validações de invariantes no domínio.
- **Padrão de repository (Application + Infrastructure):**
  - interface em `Togo.Application` (ex.: `IPetRepository`);
  - implementação EF Core em `Togo.Infrastructure/Repositories`.
- **Padrão de use case (Application):** use cases específicos por operação (`Create`, `Update`, `Delete`, `GetById`, `List`).
- **Padrão de contracts (Application):** requests/responses/projections separados em pasta `Contracts`.
- **Padrão de controller (API):** controller REST dedicado por agregado, com endpoints CRUD e payloads tipados.
- **Padrão de validação (Application):** validators desacoplados do domínio para regras de integração e consistência.
- **Padrão de testes:**
  - domínio em `Togo.Domain.Tests`;
  - aplicação em `Togo.Application.Tests`;
  - API em `Togo.Api.Tests`.
- **Padrão de documentação:** sequência de fases em `docs/clinical-core` com decisões, riscos e critérios de aceite explícitos.

## 5. Decisão sobre nomenclatura inicial

O estado atual do código mostra predominância de nomenclatura técnica em inglês para entidades, enums, tabelas e rotas (`Attendance`, `Patient`, `Pet`, etc.).

Recomendação inicial (sem implementar nesta fase):

- **Código/API em inglês:** `Attendance`;
- **Documentação e comunicação de negócio em português:** `Atendimento`.

Essa abordagem mantém consistência com o código existente e reduz ambiguidades para o time funcional.

## 6. Riscos identificados

Riscos principais antes de iniciar implementação:

1. Modelar Atendimento grande demais logo no início, elevando acoplamento e custo de mudança.
2. Misturar Atendimento com Prontuário precocemente, sem fronteira clara entre episódio clínico e histórico longitudinal.
3. Misturar Atendimento com Financeiro antes da hora, sem casos de uso clínicos estabilizados.
4. Introduzir RabbitMQ/eventos prematuramente, contrariando a decisão de escopo da Fase 4.0.1.
5. Criar campos clínicos em excesso antes de validar modelo mínimo operacional.
6. Não definir claramente relacionamento e regras de ciclo de vida com `Patient`/`Pet`.
7. Reaproveitar parcialmente o padrão de Pet/Patient e gerar inconsistência entre verticais.

## 7. Recomendação para próxima fase

Com base na inspeção, a nomenclatura técnica já está praticamente estabelecida no código como `Attendance`.

**Próxima fase recomendada:**

- **Fase 4.1.3 — Definir modelo mínimo de Atendimento.**

Observação: a Fase 4.1.2 (nomenclatura) pode ser registrada de forma rápida/confirmatória, mas não parece bloqueadora neste momento dado o padrão existente.

## 8. Fora do escopo

Nesta fase, ficou explicitamente fora do escopo:

- criação de entidade;
- criação de enum;
- criação de contratos;
- criação de repository;
- criação de use cases;
- criação de validators;
- criação de controller;
- criação de testes;
- criação de migration;
- alteração de banco;
- alteração de DI;
- alteração de `Program.cs`;
- alteração de `appsettings`;
- alteração de workflow;
- qualquer implementação de Docker, Redis, RabbitMQ ou Kubernetes.
