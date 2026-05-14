# TOGO — Fase 3.10: Testes automatizados do fluxo Pet

## 1. Objetivo da fase

A Fase 3.10 teve como objetivo automatizar testes da camada Application para o fluxo Pet/Patient, validando validators e use cases reais sem depender de banco de dados, EF Core, controller, endpoint HTTP ou API em execução.

A fase complementa a Fase 3.9, que validou manualmente o CRUD Pet/Patient com Postman/Swagger, API local e banco real. Enquanto a Fase 3.9 confirmou o comportamento ponta a ponta do fluxo exposto pela API, a Fase 3.10 fortaleceu a cobertura automatizada das regras e da orquestração na camada Application.

## 2. Estratégia adotada

A estratégia da Fase 3.10 foi criar uma suíte de testes automatizados isolada da infraestrutura externa, mantendo o foco em regras de negócio, validações e orquestração dos use cases.

Foram adotadas as seguintes decisões:

- criação de um projeto específico chamado `Togo.Application.Tests`;
- uso de `FakePetRepository` em memória;
- uso de `TestLogger<T>` para capturar e inspecionar logs;
- uso dos validators reais da Application;
- uso dos use cases reais da Application;
- ausência de Moq;
- ausência de NSubstitute;
- ausência de EF Core;
- ausência de banco de dados;
- ausência de controller/API nesta fase;
- ausência de chamadas HTTP.

Com isso, os testes passaram a validar o comportamento da camada Application sem acoplar a suíte ao banco, ao provider EF Core, à serialização HTTP ou ao pipeline ASP.NET Core.

## 3. Projeto de testes criado

Na subfase 3.10.1 foi criado o projeto de testes:

```text
backend/src/Togo.Application.Tests/Togo.Application.Tests.csproj
```

Características documentadas do projeto:

- `TargetFramework` definido como `net8.0`;
- referência de projeto para `Togo.Application`;
- referência de projeto para `Togo.Domain`;
- pacotes mínimos de teste:
  - `Microsoft.NET.Test.Sdk`;
  - `xunit`;
  - `xunit.runner.visualstudio`;
- projeto adicionado à solution:

```text
backend/Togo.sln
```

Nenhum teste foi criado na subfase 3.10.1. Essa etapa preparou apenas a estrutura necessária para as subfases seguintes.

## 4. Infraestrutura fake de testes

### 4.1. FakePetRepository

Na subfase 3.10.2 foi criado o arquivo:

```text
backend/src/Togo.Application.Tests/Pets/Fakes/FakePetRepository.cs
```

O `FakePetRepository` foi implementado como fake em memória de `IPetRepository`, permitindo testar validators e use cases sem EF Core, sem banco de dados e sem framework de mock.

Características do fake:

- implementa `IPetRepository`;
- usa armazenamento em memória;
- possui helper `AddExistingTutor`;
- possui helper `AddPet`;
- possui helper `AddDeleteConflict`;
- permite testar fluxos de List, Get, Create, Update e Delete;
- permite testar validação de Tutor existente/inexistente;
- permite testar validação de microchip existente/inexistente;
- simula conflito de delete lançando `InvalidOperationException`;
- não usa EF Core;
- não usa banco real;
- não usa Moq ou NSubstitute.

O fake tornou possível validar a camada Application com reprodutibilidade e baixo custo de execução, mantendo os cenários independentes de infraestrutura externa.

### 4.2. TestLogger<T>

Na subfase 3.10.3 foi criado o arquivo:

```text
backend/src/Togo.Application.Tests/Pets/Fakes/TestLogger.cs
```

O `TestLogger<T>` foi criado para validar logs emitidos por validators e use cases sem depender de framework de mock.

Características do logger de teste:

- implementa `ILogger<T>`;
- captura `LogLevel`;
- captura `Message`;
- captura `Exception`;
- permite verificar logs de sucesso;
- permite verificar logs de conflito;
- permite validar que logs não expõem dados sensíveis;
- foi usado especialmente para verificar que microchips não são registrados nos logs.

## 5. Testes dos validators de Pet

Na subfase 3.10.3 foram criados testes para os validators do fluxo Pet.

Arquivos documentados:

```text
backend/src/Togo.Application.Tests/Pets/Validators/PetTutorExistsValidatorTests.cs
backend/src/Togo.Application.Tests/Pets/Validators/PetMicrochipUniquenessValidatorTests.cs
```

### 5.1. PetTutorExistsValidator

Cobertura criada:

- `TutorId` inválido;
- Tutor inexistente;
- Tutor existente.

Esses testes verificam se o validator identifica corretamente entradas inválidas, bloqueia tutores inexistentes e aprova tutores conhecidos pelo fake em memória.

### 5.2. PetMicrochipUniquenessValidator

Cobertura criada:

- microchip `null`;
- microchip vazio;
- microchip com whitespace;
- microchip inexistente;
- microchip duplicado;
- uso de `ignorePatientId`;
- aplicação de `Trim` antes da checagem;
- garantia de não logar microchip sensível.

Esses testes reforçam a regra de unicidade de microchip e a exigência de logs seguros.

## 6. Testes do CreatePetUseCase

Na subfase 3.10.4 foi criado o arquivo:

```text
backend/src/Togo.Application.Tests/Pets/UseCases/CreatePetUseCaseTests.cs
```

Cobertura criada para `CreatePetUseCase`:

- criação válida;
- persistência no `FakePetRepository`;
- Tutor inexistente;
- microchip duplicado;
- microchip `null`;
- garantia de não logar microchip sensível;
- log de sucesso.

Esses testes validam a orquestração de criação de Pet/Patient na camada Application, incluindo validação de Tutor, validação de microchip e persistência em memória.

## 7. Testes de List/Get/Delete Pet use cases

Na subfase 3.10.5 foram criados testes para listagem, busca e exclusão de Pet.

Arquivos documentados:

```text
backend/src/Togo.Application.Tests/Pets/UseCases/ListPetsUseCaseTests.cs
backend/src/Togo.Application.Tests/Pets/UseCases/GetPetByIdUseCaseTests.cs
backend/src/Togo.Application.Tests/Pets/UseCases/DeletePetUseCaseTests.cs
```

### 7.1. ListPetsUseCase

Cobertura criada:

- lista vazia;
- listagem ordenada por `Name`;
- mapping de `PetListItemResponse`;
- log de sucesso.

Esses testes validam o comportamento esperado da listagem sem depender de query EF Core ou endpoint HTTP.

### 7.2. GetPetByIdUseCase

Cobertura criada:

- `PatientId` inválido;
- pet inexistente;
- pet existente;
- mapping completo de `PetResponse`;
- log de pet encontrado.

Esses testes validam a busca por identificador público do Patient e o mapeamento completo da resposta da Application.

### 7.3. DeletePetUseCase

Cobertura criada:

- `PatientId` inválido;
- pet inexistente;
- delete com sucesso;
- remoção real no `FakePetRepository`;
- conflito simulado com `AddDeleteConflict`;
- log de sucesso;
- log de conflito.

Esses testes validam a exclusão na camada Application, incluindo o caminho feliz, validações de entrada, inexistência do Pet e conflito simulado pelo fake.

## 8. Testes do UpdatePetUseCase

Na subfase 3.10.6 foi criado o arquivo:

```text
backend/src/Togo.Application.Tests/Pets/UseCases/UpdatePetUseCaseTests.cs
```

Cobertura criada para `UpdatePetUseCase`:

- `PatientId` inválido;
- pet inexistente;
- Tutor inexistente;
- microchip duplicado em outro pet;
- update válido;
- persistência no `FakePetRepository`;
- reutilização do mesmo microchip para o mesmo `PatientId`;
- microchip `null`;
- garantia de não logar microchip sensível;
- log de sucesso;
- preservação de `CreatedAt`;
- preenchimento de `UpdatedAt`.

Esses testes validam a atualização de Pet/Patient na camada Application, incluindo regras de unicidade, validação de Tutor, persistência em memória e consistência de datas.

## 9. Logs e segurança

A Fase 3.10 incluiu validações automatizadas de logs para garantir observabilidade mínima sem exposição indevida de dados sensíveis.

Os testes validam:

- logs de sucesso;
- logs de conflito;
- logs de listagem;
- logs de busca;
- ausência de microchip sensível nos logs de validators;
- ausência de microchip sensível nos logs de create;
- ausência de microchip sensível nos logs de update.

Os logs podem conter:

- `PatientId`;
- `TutorId`;
- `HasMicrochip`;
- `Count`.

Os logs não devem conter:

- microchip completo;
- microchip parcial;
- payload completo;
- token JWT;
- senha.

Essa regra preserva a utilidade operacional dos logs sem registrar dados sensíveis do paciente/pet ou credenciais de autenticação.

## 10. Resultado final dos testes

Após a PR 64, o desenvolvedor humano informou a seguinte validação local.

Comando executado:

```bash
dotnet build backend/Togo.sln
```

Resultado:

- build com sucesso.

Comando executado:

```bash
dotnet test backend/Togo.sln
```

Resultado:

- total: 96 testes;
- falhou: 0;
- bem-sucedido: 96;
- ignorado: 0.

Nesta subfase documental 3.10.7, `dotnet build` e `dotnet test` não foram reexecutados porque o escopo foi exclusivamente documental.

## 11. Decisões técnicas

Decisões técnicas registradas para a Fase 3.10:

- os testes ficaram na camada Application;
- os testes usam fake em memória;
- os validators testados são os validators reais da Application;
- os use cases testados são os use cases reais da Application;
- não foram criados testes de controller nesta fase;
- não foram criados testes de integração nesta fase;
- não foi usado banco real;
- não foi usado EF Core;
- não foi usado framework de mock;
- os testes validam regras e orquestração;
- os testes não validam transporte HTTP;
- os testes não validam serialização/deserialização de API;
- os testes não validam middleware, autenticação ou autorização.

Essas decisões mantêm a suíte rápida, determinística e focada no comportamento da Application.

## 12. Limites da fase

Ficaram fora do escopo da Fase 3.10:

- testes de controller;
- testes com `WebApplicationFactory`;
- testes de integração com banco;
- Testcontainers;
- coverage;
- testes de performance;
- testes E2E;
- frontend;
- migrations;
- database update;
- instalação de pacotes além dos pacotes mínimos do projeto de testes;
- alterações em código de produção;
- alterações em endpoints HTTP.

## 13. Status final da Fase 3.10

Status final registrado:

- Fase 3.10 concluída com sucesso;
- validators de Pet cobertos;
- use cases de Pet cobertos;
- fluxo Application de Pet mais protegido;
- build local com sucesso, conforme validação informada pelo desenvolvedor humano;
- 96/96 testes passando, conforme validação informada pelo desenvolvedor humano;
- nenhum código de produção alterado nesta fase documental;
- nenhum teste alterado nesta fase documental;
- nenhuma migration criada nesta fase documental;
- nenhum database update executado nesta fase documental.

A Fase 3.10 adiciona uma camada importante de proteção automatizada sobre o fluxo Pet/Patient, reduzindo risco de regressões nas regras de Application.

## 14. Próxima fase recomendada

Próxima fase recomendada:

**Fase 3.11 — Testes manuais finais e revisão de fechamento do CRUD Pet/Patient**

Sugestão de escopo da Fase 3.11:

- rodar build/test final;
- subir API local;
- repetir checklist manual principal de Pet;
- conferir logs;
- validar CI no GitHub Actions;
- documentar fechamento da Fase 3.

A Fase 3.11 deve atuar como revisão final do CRUD Pet/Patient, combinando validação automatizada, validação manual e revisão de fechamento antes da continuidade para novos módulos clínicos.
