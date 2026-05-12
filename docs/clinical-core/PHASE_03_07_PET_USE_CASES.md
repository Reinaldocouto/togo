# TOGO — Fase 3.7: Use Cases de Pet

## 1. Objetivo da fase

A Fase 3.7 criou os use cases de Pet na camada Application, consolidando a orquestração das operações de criação, listagem, busca por identificador, atualização e remoção de Pet/Patient.

Os use cases passam a centralizar o fluxo de aplicação para Pet, orquestrando:

- contratos de entrada;
- validators;
- repository;
- mappings;
- ApplicationResult;
- logs seguros.

Esta fase ainda não criou controller nem endpoints públicos para Pet. A exposição HTTP da funcionalidade deve ser tratada em fase posterior.

## 2. Arquivos criados

A Fase 3.7 criou os seguintes use cases na camada Application:

- `backend/src/Togo.Application/Pets/UseCases/CreatePetUseCase.cs`;
- `backend/src/Togo.Application/Pets/UseCases/ListPetsUseCase.cs`;
- `backend/src/Togo.Application/Pets/UseCases/GetPetByIdUseCase.cs`;
- `backend/src/Togo.Application/Pets/UseCases/UpdatePetUseCase.cs`;
- `backend/src/Togo.Application/Pets/UseCases/DeletePetUseCase.cs`.

A fase também criou/atualizou o arquivo de mapeamentos explícitos:

- `backend/src/Togo.Application/Pets/PetMappings.cs`.

## 3. CreatePetUseCase

`CreatePetUseCase` é responsável por criar um Pet/Patient a partir do contrato de entrada de criação.

Responsabilidades principais:

- criar Pet/Patient;
- validar a existência do Tutor com `PetTutorExistsValidator`;
- validar a unicidade do microchip com `PetMicrochipUniquenessValidator`;
- usar `ignorePatientId = null` na criação, pois ainda não existe PatientId próprio a ser ignorado na regra de unicidade;
- montar `CreatePetRepositoryData`;
- fixar `PatientType.Pet` no dado enviado ao repository;
- chamar `IPetRepository.CreateAsync`;
- mapear `PetDetailsProjection` para `PetResponse`;
- retornar `ApplicationResult<PetResponse>`.

Logs adotados:

- registra log inicial seguro com `TutorId` e `HasMicrochip`;
- registra log de sucesso com `PatientId`, `TutorId` e `HasMicrochip`;
- não loga microchip completo;
- não loga microchip parcial;
- não loga payload completo da requisição.

## 4. ListPetsUseCase

`ListPetsUseCase` é responsável por listar pets existentes por meio do repository.

Responsabilidades principais:

- listar pets;
- chamar `IPetRepository.ListAsync`;
- mapear `PetListItemProjection` para `PetListItemResponse`;
- retornar `ApplicationResult<IReadOnlyList<PetListItemResponse>>`.

Logs adotados:

- registra log inicial de listagem;
- registra log de sucesso com `Count`;
- não loga a lista completa de pets;
- não loga microchip.

Paginação e filtros ainda não fazem parte desta fase. A listagem permanece simples e deve ser evoluída em fase posterior, quando houver decisão funcional sobre critérios de busca e volume esperado.

## 5. GetPetByIdUseCase

`GetPetByIdUseCase` é responsável por buscar um Pet usando o `PatientId` como identificador público.

Responsabilidades principais:

- buscar Pet por `PatientId`;
- validar `patientId <= 0`;
- retornar `ValidationError` para `PatientId` inválido;
- chamar `IPetRepository.GetByPatientIdAsync`;
- retornar `NotFound` quando não encontrar o Pet;
- mapear `PetDetailsProjection` para `PetResponse`;
- retornar `Success` quando encontrar o Pet.

Logs adotados:

- usa apenas `PatientId`;
- não loga microchip;
- não loga payload.

## 6. UpdatePetUseCase

`UpdatePetUseCase` é responsável por atualizar os dados de um Pet/Patient existente.

Responsabilidades principais:

- atualizar Pet/Patient;
- validar `patientId <= 0`;
- verificar a existência do Pet antes das validações de negócio;
- validar Tutor com `PetTutorExistsValidator`;
- validar microchip com `ignorePatientId = patientId`;
- montar `UpdatePetRepositoryData`;
- chamar `IPetRepository.UpdateAsync`;
- manter fallback `NotFound` se o repository retornar `null` durante a atualização;
- mapear o resultado para `PetResponse`;
- retornar `ApplicationResult<PetResponse>`.

A ordem segura adotada é:

1. validar `patientId`;
2. verificar se o Pet existe;
3. validar Tutor;
4. validar microchip ignorando o próprio `PatientId`;
5. atualizar.

Essa ordem evita retornar conflito de microchip ou erro de Tutor para um Pet inexistente. Assim, quando o `PatientId` não representa um Pet existente, o retorno preserva a semântica correta de `NotFound` antes de avaliar regras de negócio relacionadas ao payload.

Logs adotados:

- usa `PatientId`, `TutorId` e `HasMicrochip`;
- não loga microchip completo;
- não loga microchip parcial;
- não loga payload completo.

## 7. DeletePetUseCase

`DeletePetUseCase` é responsável por remover um Pet/Patient por `PatientId`.

Responsabilidades principais:

- remover Pet/Patient por `PatientId`;
- validar `patientId <= 0`;
- retornar `ValidationError` para `PatientId` inválido;
- chamar `IPetRepository.DeleteAsync`;
- retornar `NotFound` quando `DeleteAsync` retornar `false`;
- retornar `Success(true)` quando remover;
- capturar apenas `InvalidOperationException` como `Conflict`;
- não capturar `Exception` genérica.

Decisão provisória sobre remoção:

- delete físico segue provisório nesta fase;
- soft delete não foi implementado;
- essa decisão será reavaliada quando Atendimento/Prontuário existirem, pois vínculos clínicos podem exigir preservação histórica e regras mais rígidas de exclusão.

Logs adotados:

- usa apenas `PatientId`;
- não loga microchip;
- não loga payload;
- não loga dados clínicos detalhados.

## 8. PetMappings

`PetMappings` fica na camada Application e concentra os mapeamentos explícitos entre projections retornadas pelo repository e contracts de resposta retornados pelos use cases.

Responsabilidades principais:

- converter `PetDetailsProjection` para `PetResponse`;
- converter `PetListItemProjection` para `PetListItemResponse`;
- não usar AutoMapper;
- não instalar pacote adicional;
- manter mapeamento explícito e simples.

A decisão por mapeamento explícito preserva clareza, reduz dependências e mantém a transformação de dados visível no código da camada Application.

## 9. Registro no Dependency Injection

A Fase 3.7.6 registrou os use cases de Pet no `Program.cs` da API com o using:

```csharp
using Togo.Application.Pets.UseCases;
```

Também foram adicionados os registros:

```csharp
builder.Services.AddScoped<CreatePetUseCase>();
builder.Services.AddScoped<ListPetsUseCase>();
builder.Services.AddScoped<GetPetByIdUseCase>();
builder.Services.AddScoped<UpdatePetUseCase>();
builder.Services.AddScoped<DeletePetUseCase>();
```

Observações sobre o registro:

- os registros foram feitos com `AddScoped`;
- os registros foram adicionados na seção `DEPENDENCY INJECTION`;
- nenhum endpoint foi criado;
- nenhum controller foi criado;
- middleware, JWT, Swagger e CORS não foram alterados por esta fase.

## 10. Decisões técnicas importantes

Decisões consolidadas nesta fase:

- use cases ficam na camada Application;
- use cases não acessam `AppDbContext` diretamente;
- use cases dependem de `IPetRepository`;
- use cases dependem de validators quando necessário;
- use cases retornam `ApplicationResult`;
- use cases não retornam entidades de domínio;
- use cases não criam transação diretamente;
- transação fica no `PetRepository`;
- `PatientId` segue como identificador público;
- `PetId` não foi criado nem usado;
- microchip é tratado como dado sensível para logs.

## 11. Validação local

Nenhuma validação local executada pelo desenvolvedor humano foi informada neste fechamento documental.

Como esta tarefa é exclusivamente documental, a validação esperada para a base segue sendo:

- `dotnet build backend/Togo.sln`;
- `dotnet test backend/Togo.sln`;
- expectativa atual: 45 testes passando.

O resultado acima deve ser lido como validação esperada, não como execução local confirmada pelo desenvolvedor humano neste documento.

## 12. Pontos de atenção futuros

Pontos a acompanhar nas próximas fases:

- `ApplicationResult` ainda está em namespace relacionado a Tutors e futuramente pode ser movido para `Application/Common`;
- controller de Pet ainda não existe;
- endpoints de Pet ainda não existem;
- testes específicos de use cases ainda podem ser criados em fase posterior;
- paginação/filtros ainda não foram implementados;
- delete físico deve ser reavaliado antes de Atendimento/Prontuário;
- logs do futuro controller devem manter o mesmo cuidado com microchip e payload completo.

## 13. Fechamento da Fase 3.7

A Fase 3.7 está concluída com:

- `CreatePetUseCase` criado;
- `ListPetsUseCase` criado;
- `GetPetByIdUseCase` criado;
- `UpdatePetUseCase` criado;
- `DeletePetUseCase` criado;
- `PetMappings` criado/atualizado;
- use cases registrados no DI;
- nenhum endpoint público criado ainda;
- nenhuma migration criada;
- nenhum database update executado.

Próxima fase recomendada:

**Fase 3.8 — Criar PetsController.**
