# TOGO — Fase 4.4.3: Registro de AttendanceRepository no DI

## 1. Objetivo

Esta fase registra `AttendanceRepository` como implementação concreta de `IAttendanceRepository` no container de injeção de dependência da aplicação.

## 2. Contexto

- A camada Application já depende de `IAttendanceRepository`.
- A camada Infrastructure já possui `AttendanceRepository`.
- Os testes de Infrastructure para AttendanceRepository já foram criados e integrados à solution.
- Esta fase conecta a abstração (`IAttendanceRepository`) à implementação concreta (`AttendanceRepository`) no DI.

## 3. Registro realizado

- **Arquivo alterado:** `backend/src/Togo.Api/Program.cs`
- **Registro adicionado (conceito):** mapeamento de interface para implementação no container.
- **Lifetime usado:** `Scoped`
- **Interface:** `IAttendanceRepository`
- **Implementação:** `AttendanceRepository`

Registro aplicado:

`builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();`

## 4. Padrão seguido

O padrão de registro já existente em `Program.cs` para repositories foi mantido:

- `IPetRepository` → `PetRepository` com `AddScoped`
- `ITutorRepository` → `TutorRepository` com `AddScoped`

Attendance seguiu exatamente o mesmo padrão:

- `IAttendanceRepository` → `AttendanceRepository` com `AddScoped`

## 5. Decisões técnicas

- Foi utilizado `Scoped`, alinhado ao padrão atual de repositories e ao ciclo de vida do `AppDbContext`.
- `AttendanceRepository` utiliza `AppDbContext`, portanto o lifetime `Scoped` é coerente para o contexto por requisição.
- Não houve criação de API/controller.
- Não houve migration/database update.
- Não houve alteração de regra de domínio.

## 6. Fora do escopo

- Domain
- Application use cases
- Infrastructure repository
- testes
- API controllers
- endpoints
- migrations
- database update
- RabbitMQ
- Redis
- Docker
- Kubernetes
- Frontend

## 7. Validação

Comandos executados:

- `git branch --show-current`
- `git status`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Resultados desta execução:

- `git branch --show-current`: `feat/phase-4-4-3-register-attendance-repository-di`
- `git status`: branch ativa sem alterações pendentes antes da implementação; após implementação, arquivos modificados/criados esperados
- `git diff --check`: sem saída (sem problemas de whitespace)
- `dotnet build backend/Togo.sln`: `dotnet: command not found`
- `dotnet test backend/Togo.sln`: `dotnet: command not found`

## 8. Próxima fase recomendada

**Fase 4.4.4 — Validar configuração EF/AppDbContext de Attendance.**

Objetivo:
Revisar se `Attendance` está corretamente configurado no `AppDbContext`/EF Core, incluindo DbSet, configuration, relacionamento com Patient, índices e necessidade ou não de migration.
