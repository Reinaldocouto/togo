# TOGO — Fase 3.6: Validators de Pet

## 1. Objetivo da fase

A Fase 3.6 criou validators de aplicação para regras reutilizáveis do fluxo Pet/Patient. O objetivo foi preparar a camada Application para centralizar validações que dependem de persistência e que serão orquestradas por use cases futuros.

Esses validators permanecem na camada Application porque representam regras de aplicação e dependem de consultas via abstrações de repository. Essa decisão mantém o padrão SOLID, evita dependência direta de Infrastructure nos fluxos de aplicação e reduz o risco de espalhar condicionais repetidas nos futuros use cases de Pet.

## 2. Arquivos criados

A Fase 3.6.1 criou os seguintes arquivos:

- `backend/src/Togo.Application/Pets/Validators/PetTutorExistsValidator.cs`
- `backend/src/Togo.Application/Pets/Validators/PetMicrochipUniquenessValidator.cs`

## 3. PetTutorExistsValidator

O `PetTutorExistsValidator` é responsável por validar se o Tutor informado para o fluxo de Pet/Patient existe antes da continuidade da operação de aplicação.

Responsabilidades principais:

- validar se o Tutor informado existe;
- validar `tutorId` inválido;
- consultar `IPetRepository.TutorExistsAsync`;
- retornar `ApplicationResult<bool>`;
- retornar `ValidationError` quando `tutorId <= 0`;
- retornar `NotFound` quando o tutor não existe;
- retornar `Success(true)` quando o tutor existe.

Comportamento esperado:

- quando `tutorId <= 0`, a validação deve falhar como erro de validação, sem consultar detalhes de Tutor;
- quando `tutorId` é válido, o validator consulta a existência via `IPetRepository.TutorExistsAsync`;
- quando o Tutor não existe, a validação retorna `NotFound`;
- quando o Tutor existe, a validação retorna sucesso com `true`.

Logs e segurança:

- usa `ILogger<PetTutorExistsValidator>`;
- os logs usam apenas `TutorId`;
- o validator não carrega o Tutor completo;
- o validator não registra dados pessoais do tutor;
- o validator não acessa a camada Infrastructure diretamente.

## 4. PetMicrochipUniquenessValidator

O `PetMicrochipUniquenessValidator` é responsável por validar a unicidade de microchip quando esse dado é informado no fluxo de Pet/Patient.

Responsabilidades principais:

- validar unicidade de microchip quando informado;
- ignorar validação quando `microchip` for `null`, vazio ou composto apenas por whitespace;
- normalizar o microchip com `Trim` antes de consultar;
- consultar `IPetRepository.MicrochipExistsAsync`;
- retornar `Conflict` quando o microchip já existe;
- retornar `Success(true)` quando o microchip não existe.

Regra principal:

- se o microchip não for informado, não há validação de duplicidade;
- se o microchip for informado, ele não pode duplicar um microchip já existente.

Comportamento esperado:

- quando `microchip` é `null`, vazio ou whitespace, o validator considera que não há microchip a validar e retorna sucesso;
- quando `microchip` é informado, o valor é normalizado com `Trim` antes da consulta;
- quando a consulta indica que o microchip já existe, o validator retorna `Conflict`;
- quando a consulta indica que o microchip não existe, o validator retorna `Success(true)`.

Segurança de logs:

- não loga o microchip completo;
- não loga microchip parcial;
- os logs usam apenas `HasMicrochip` e `IgnorePatientId`;
- a mensagem de erro não retorna o valor do microchip.

## 5. Por que não criar PetRequiredFieldsValidator agora

A Fase 3.6 não criou um `PetRequiredFieldsValidator` por decisão técnica de separação de responsabilidades.

Motivos:

- campos obrigatórios e invariantes simples pertencem às entidades de domínio `Patient` e `Pet`;
- `PatientTests` e `PetTests` já validam regras de domínio;
- os validators desta fase tratam regras de aplicação que dependem de repository;
- criar `PetRequiredFieldsValidator` agora duplicaria responsabilidade e poderia ferir o princípio da responsabilidade única, SRP.

Portanto, a Fase 3.6 ficou limitada a validators de aplicação que consultam persistência por meio de abstrações, sem duplicar invariantes já protegidas pelo domínio.

## 6. Registro no Dependency Injection

A Fase 3.6.2 registrou os validators no `Program.cs` da API.

Using adicionado:

```csharp
using Togo.Application.Pets.Validators;
```

Registros adicionados:

```csharp
builder.Services.AddScoped<PetTutorExistsValidator>();
builder.Services.AddScoped<PetMicrochipUniquenessValidator>();
```

Características do registro:

- os registros foram feitos com `AddScoped`;
- os registros foram adicionados na seção `DEPENDENCY INJECTION`;
- os registros ficaram próximos ao `TutorDocumentUniquenessValidator`, mantendo o agrupamento de validators de aplicação;
- nenhum endpoint, use case ou controller foi criado nessa fase.

## 7. Validação local

A validação local foi executada pelo desenvolvedor humano com os seguintes comandos:

```bash
dotnet build backend/Togo.sln
```

Resultado registrado:

- build concluído com sucesso.

```bash
dotnet test backend/Togo.sln
```

Resultado registrado:

- testes concluídos com sucesso;
- total de 45 testes;
- 45 passaram;
- 0 falharam;
- 0 ignorados.

## 8. Decisões técnicas importantes

Decisões consolidadas na Fase 3.6:

- validators ficam na camada Application;
- validators dependem de `IPetRepository`;
- validators não acessam `AppDbContext`;
- validators não usam EF Core diretamente;
- validators não retornam entidades;
- validators retornam `ApplicationResult<bool>`;
- validators seguem o padrão do `TutorDocumentUniquenessValidator`;
- logs usam placeholders estruturados;
- microchip é tratado como dado sensível para logs.

## 9. Pontos de atenção futuros

Pontos para fases posteriores:

- `ApplicationResult` ainda está em namespace relacionado a Tutors e futuramente pode ser movido para `Application/Common`;
- use cases de Pet deverão orquestrar esses validators;
- `CreatePetUseCase` deverá usar `PetTutorExistsValidator` e `PetMicrochipUniquenessValidator`;
- `UpdatePetUseCase` deverá usar `PetTutorExistsValidator` e `PetMicrochipUniquenessValidator` com `ignorePatientId`;
- controller ainda não existe;
- endpoints ainda não existem;
- testes específicos de validators/use cases podem ser criados em fase posterior.

## 10. Fechamento da Fase 3.6

A Fase 3.6 está concluída com:

- validators criados;
- validators registrados no DI;
- build/test local validado;
- nenhuma migration criada;
- nenhum database update executado;
- nenhum endpoint público criado ainda.

Próxima fase recomendada:

**Fase 3.7 — Criar use cases de Pet.**
