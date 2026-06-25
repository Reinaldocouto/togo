# Fase 8.1 — Modelo mínimo de Organização, Clínica e Unidade

## Objetivo

Implementar o modelo mínimo de domínio para `Organization`, `Clinic` e `ClinicUnit`, preparando o TOGO para isolamento clínico-operacional futuro sem alterar fluxos clínicos existentes, persistência, API ou autorização contextual.

## Escopo implementado

- Criação das entidades puras de domínio `Organization`, `Clinic` e `ClinicUnit`.
- Definição de identificadores, nome, status ativo/inativo e timestamps conforme o padrão atual do domínio.
- Definição dos vínculos hierárquicos mínimos:
  - `Clinic` possui `OrganizationId` obrigatório.
  - `ClinicUnit` possui `ClinicId` obrigatório.
- Implementação de invariantes simples para nomes, identificadores e datas.
- Implementação de métodos simples de ativação e inativação.
- Criação de testes unitários de domínio para criação válida, entradas inválidas e inativação.

## Arquivos alterados

- `backend/src/Togo.Domain/Entities/Organization.cs`
- `backend/src/Togo.Domain/Entities/Clinic.cs`
- `backend/src/Togo.Domain/Entities/ClinicUnit.cs`
- `backend/src/Togo.Domain.Tests/OrganizationTests.cs`
- `backend/src/Togo.Domain.Tests/ClinicTests.cs`
- `backend/src/Togo.Domain.Tests/ClinicUnitTests.cs`
- `docs/security/PHASE_08_01_ORGANIZATION_CLINIC_UNIT_MODEL.md`

## Entidades criadas

### Organization

Representa o agrupador administrativo superior. Nesta fase, contém somente:

- `Id`
- `Name`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

### Clinic

Representa o escopo primário de isolamento clínico-operacional. Nesta fase, contém somente:

- `Id`
- `OrganizationId`
- `Name`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

### ClinicUnit

Representa uma unidade física, filial ou subdivisão operacional da clínica. Nesta fase, atua como escopo complementar, sem regras avançadas. Contém somente:

- `Id`
- `ClinicId`
- `Name`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

## Regras de domínio implementadas

- Nome é obrigatório em todas as entidades.
- Nome não pode ser vazio ou composto apenas por whitespace.
- Nome é normalizado com `Trim()` na criação.
- `Organization` inicia ativa.
- `Clinic` inicia ativa e deve possuir `OrganizationId` maior que zero.
- `ClinicUnit` inicia ativa e deve possuir `ClinicId` maior que zero.
- Todas as entidades exigem `CreatedAt` válido na criação.
- Todas as entidades permitem ativação e inativação controladas com `updatedAt` válido.
- As entidades mantêm setters privados e construtor privado para compatibilidade futura com ferramentas de materialização, sem acoplar persistência ao domínio.

## Testes criados

- `OrganizationTests`
  - cria organização válida ativa;
  - rejeita nome vazio;
  - rejeita nome whitespace;
  - permite inativar.
- `ClinicTests`
  - cria clínica válida vinculada a uma organização;
  - rejeita `OrganizationId` inválido;
  - rejeita nome vazio;
  - rejeita nome whitespace;
  - permite inativar.
- `ClinicUnitTests`
  - cria unidade válida vinculada a uma clínica;
  - rejeita `ClinicId` inválido;
  - rejeita nome vazio;
  - rejeita nome whitespace;
  - permite inativar.

## Decisões tomadas

- `Clinic` será tratada como escopo primário de isolamento clínico-operacional.
- `Organization` será tratada como agrupador administrativo superior.
- `ClinicUnit` será tratada como escopo complementar, sem regras avançadas nesta fase.
- O status foi modelado como `IsActive`, evitando a criação prematura de enum específico.
- As entidades seguem o estilo atual do domínio: factory estática `Create`, construtores privados, validações com exceptions padrão e timestamps explícitos.
- A Fase 8.1 não implementa persistência.
- A Fase 8.1 não altera fluxos clínicos existentes.
- A Fase 8.1 não propaga `ClinicId` para entidades clínicas.
- A Fase 8.1 não implementa `UserClinicAccess`.
- A validação de acesso do usuário à clínica será tratada em fase posterior.
- A persistência será tratada na Fase 8.2.

## Decisões adiadas

- Estratégia de mapeamento EF Core, índices e migrations.
- Se `IsActive` permanecerá como booleano ou evoluirá para enum de status.
- Regras avançadas de unidade, como unidade principal, endereço, capacidade operacional ou permissões específicas.
- Vínculo de usuários a clínicas e unidades por meio de `UserClinicAccess` ou estrutura equivalente.
- Origem e validação do contexto clínico ativo por requisição.
- Propagação de `ClinicId` para pacientes, tutores, atendimentos, prontuários, prescrições e evoluções clínicas.

## Riscos remanescentes

- As novas entidades ainda não são persistidas e, portanto, não exercem isolamento real em runtime.
- Os fluxos clínicos continuam sem `ClinicId` até fases posteriores.
- A autorização atual segue baseada em identidade/permissão, sem validação contextual de clínica.
- Listagens e consultas clínicas ainda não possuem filtro por escopo clínico.
- A auditoria clínica ainda não registra organização, clínica ou unidade.

## O que não foi feito nesta fase

- Não foram criadas migrations.
- `AppDbContext` não foi alterado.
- Nenhum `DbSet` foi criado.
- Nenhuma configuração EF Core foi criada.
- Repositories não foram alterados.
- API, controllers e DTOs públicos não foram alterados.
- Autenticação e autorização não foram alteradas.
- `CurrentClinicalContext` não foi criado.
- `ClinicId` não foi propagado para `Patient`, `Tutor`, `Attendance`, `MedicalRecord`, `Prescription` ou `ClinicalEvolution`.
- Nenhum seed foi criado.
- `UserClinicAccess` não foi implementado.

## Próxima fase recomendada

A próxima fase recomendada é a **Fase 8.2 — Persistência do escopo clínico**, responsável por mapear as entidades no banco de dados, criar migrations e definir índices mínimos sem ainda ampliar indevidamente autorização ou fluxos clínicos além do escopo aprovado para a subfase.
