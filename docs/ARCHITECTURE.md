# TOGO Architecture (Fase 0)

## Objetivo
Definir a base arquitetural oficial antes de evoluir para novos CRUDs, endpoints e casos de uso.

## Arquitetura em camadas

### Togo.Domain
Responsabilidades:
- Entidades de domínio.
- Enums.
- Regras básicas de negócio.

Diretrizes:
- Não depende de EF Core.
- Não usa DataAnnotations de banco.
- Não contém lógica de API.

### Togo.Application
Responsabilidades:
- Interfaces.
- Use cases.
- Contratos de entrada/saída.
- Orquestração das regras de negócio.

Diretrizes:
- Não deve depender da API.
- Não deve depender diretamente de detalhes de persistência.
- Depende de abstrações (interfaces), não de implementações concretas.

### Togo.Infrastructure
Responsabilidades:
- EF Core.
- AppDbContext.
- Configurações de mapeamento.
- Repositories.
- Implementações de serviços externos.
- Migrations.

Diretrizes:
- Concentra detalhes de persistência e integração técnica.
- Implementa interfaces definidas em camadas superiores.

### Togo.Api
Responsabilidades:
- Controllers.
- Configuração da Web API.
- Injeção de dependência.
- Swagger.
- Autenticação/autorização.

Diretrizes:
- Recebe requisições HTTP e chama use cases.
- Não concentra regra de negócio.
- Não deve acessar persistência diretamente.

## Próxima fase planejada
**Fase 1.1 — CRUD de Tutor**

Objetivo:
- Validar a arquitetura ponta a ponta com uma entidade simples antes de avançar para Patient/Pet.

Escopo futuro:
- Interface de repository para Tutor.
- Implementação EF Core do repository.
- Use cases de Tutor.
- DTOs/Requests/Responses.
- TutorController.
- Testes via Swagger/Postman.
