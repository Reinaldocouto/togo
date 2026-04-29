# Development Guidelines (Fase 0)

## Padrão oficial de nomes
- Código C# em inglês.
- Classes, entidades, métodos, DTOs, use cases e controllers em inglês.
- Banco novo em inglês.
- Tabelas novas em PascalCase/plural (ex.: `Tutors`, `Patients`, `Pets`, `Attendances`, `MedicalRecords`).
- Propriedades em PascalCase.
- Endpoints REST preferencialmente em kebab-case ou plural simples (definição final na evolução da API).
- Não misturar português e inglês no código novo.

## Padrão de entidade (Domain)
- Entidades em `Togo.Domain/Entities`.
- Enums em `Togo.Domain/Enums`.
- Usar `private set` quando aplicável.
- Evitar setters públicos livres.
- Usar métodos `Create/Update` quando fizer sentido.
- Validar regras básicas dentro da entidade.
- Não usar DataAnnotations de banco nas entidades.
- Não adicionar EF Core no projeto Domain.
- IDs `long` das entidades clínicas são gerados pelo banco via `AUTO_INCREMENT`.
- `User` permanece com `Guid` por compatibilidade com autenticação existente.

## Padrão de use case (Application)
- Use cases em `Togo.Application`.
- Cada use case representa uma ação clara do sistema.
- Nomenclatura recomendada:
  - `CreateTutorUseCase`
  - `UpdateTutorUseCase`
  - `GetTutorByIdUseCase`
  - `ListTutorsUseCase`
- Use cases devem depender de interfaces, não de implementações concretas.
- Repositories devem ser acessados por interfaces.
- Validações de entrada podem ocorrer antes de instanciar entidades.
- Use cases devem retornar resultados claros, sem acoplamento a `IActionResult`.

## Padrão de controller (API)
- Controllers em `Togo.Api`.
- Controllers devem ser finos.
- Não devem conter regra de negócio.
- Devem receber request, chamar use case e retornar response.
- Não devem acessar `AppDbContext` diretamente.
- Não devem montar SQL.
- Não devem conhecer detalhes de persistência.
- Usar rotas REST claras.

Exemplo futuro para Tutor:
- `GET /api/tutors`
- `GET /api/tutors/{id}`
- `POST /api/tutors`
- `PUT /api/tutors/{id}`
