# TOGO — Plano do Núcleo Clínico Patient/Pet

## 1. Objetivo

Este documento registra a análise técnica da Fase 3.1 do núcleo clínico Patient/Pet antes do início de qualquer implementação de CRUD.

O objetivo é revisar o modelo atual de domínio e persistência para confirmar se a estrutura existente está adequada para iniciar, nas próximas fases, um CRUD público de Pet/Patient sem alterar código, banco de dados, migrations, testes, frontend, configurações ou pipeline nesta etapa.

Esta fase é exclusivamente documental e não implementa controller, use case, repository, validator, migration, database update, alterações de frontend ou mudanças de infraestrutura.

## 2. Contexto do núcleo clínico

O TOGO é um backend em .NET 8, ASP.NET Core, Entity Framework Core e MySQL para um ERP veterinário. O projeto já possui separação em camadas, CRUD de Tutor, autenticação JWT, Swagger com Bearer Token, logs com `ILogger`, middleware global de exceções, testes automatizados, CI com GitHub Actions, branch protection e documentação técnica iniciada.

A retomada do núcleo clínico segue a cadeia conceitual:

```text
Tutor
↓
Patient/Pet
↓
Attendance
↓
MedicalRecord / Prontuário
```

Neste desenho, `Patient` representa a base clínica que será referenciada por atendimento, prontuário e histórico clínico. `Pet` representa a especialização veterinária desse paciente e conecta o paciente clínico ao seu responsável (`Tutor`).

A análise dos arquivos atuais indica que o modelo Patient/Pet já contém a estrutura mínima para seguir para as próximas fases, desde que o CRUD público seja desenhado como CRUD de Pet e trate a criação/atualização de `Patient` como detalhe interno da aplicação.

## 3. Estado atual da entidade Patient

A entidade `Patient` é a entidade clínica base do domínio. Ela centraliza os dados comuns necessários para identificar e acompanhar um paciente ao longo do fluxo clínico.

### Propriedades atuais

- `Id`: identificador técnico do paciente clínico, gerado pelo banco.
- `Type`: tipo do paciente, baseado em `PatientType`.
- `Name`: nome do paciente, obrigatório e normalizado com `Trim()`.
- `BirthDate`: data de nascimento opcional.
- `Status`: situação do paciente, atualmente representada como `string` obrigatória.
- `CreatedAt`: data/hora obrigatória de criação.
- `UpdatedAt`: data/hora opcional da última atualização.

### Comportamento de domínio

A entidade possui factory method `Create(...)` e método `Update(...)`. Ambos validam os campos obrigatórios antes de alterar o estado interno:

- `Name` não pode ser vazio ou composto apenas por espaços.
- `Status` não pode ser vazio ou composto apenas por espaços.
- `CreatedAt` e `UpdatedAt`, quando usados nos fluxos de criação/atualização, não podem receber `DateTime` default.
- `Name` e `Status` são normalizados com `Trim()`.

### Uso de PatientType

O enum `PatientType` possui atualmente os valores:

- `Pet = 1`.
- `Other = 2`.

A presença de `PatientType` é adequada para a evolução do domínio porque permite que `Patient` continue sendo uma entidade clínica base e, no futuro, aceite outras especializações além de `Pet`, se o produto precisar.

Para o CRUD veterinário planejado, o fluxo de criação de Pet deve criar internamente um `Patient` com `Type = PatientType.Pet`.

### Mapeamento atual

Na configuração EF Core, `Patient` é mapeado para a tabela `Patients` com:

- chave primária em `Id`;
- `Id` com geração automática;
- `Type` obrigatório convertido para `string`;
- `Name` obrigatório com tamanho máximo de 120;
- `BirthDate` opcional;
- `Status` obrigatório com tamanho máximo de 20;
- `CreatedAt` obrigatório;
- `UpdatedAt` opcional;
- índice em `Status`.

### Avaliação técnica

A entidade está adequada para atuar como base clínica inicial porque contém identificação, classificação por tipo, dados temporais e status. O modelo é simples e coerente com a cadeia clínica planejada, especialmente porque `Attendance` e `MedicalRecord` dependem de `PatientId`.

O principal ponto de atenção é que `Status` ainda é uma `string`. Isso é aceitável para a fase atual, mas deve ser observado nas próximas fases porque um enum de status pode reduzir inconsistências de escrita, facilitar validações e melhorar a semântica do domínio. Não se recomenda alterar isso nesta fase.

## 4. Estado atual da entidade Pet

A entidade `Pet` representa a especialização veterinária de `Patient`. Ela contém dados específicos de animais de companhia e conecta o paciente clínico ao responsável (`Tutor`).

### Propriedades atuais

- `PatientId`: identificador do paciente clínico associado. Atua como chave primária da tabela `Pets` e também como chave estrangeira para `Patients`.
- `TutorId`: identificador do tutor responsável pelo pet.
- `Species`: espécie do animal, obrigatória e normalizada com `Trim()`.
- `Breed`: raça, opcional e normalizada para `null` quando vazia.
- `Sex`: sexo do pet, baseado em `PetSex`.
- `WeightKg`: peso em quilogramas, opcional, mas quando informado deve ser maior que zero.
- `Microchip`: identificação por microchip, opcional e normalizada para `null` quando vazia.

### Comportamento de domínio

A entidade possui factory method `Create(...)` e método `UpdateProfile(...)`. As validações atuais garantem que:

- `PatientId` deve ser maior que zero.
- `TutorId` deve ser maior que zero.
- `Species` é obrigatória.
- `WeightKg`, quando informado, deve ser maior que zero.
- `Breed` e `Microchip` são opcionais e normalizados para `null` se vierem vazios ou apenas com espaços.

### Uso de PetSex

O enum `PetSex` possui atualmente os valores:

- `Male = 1`.
- `Female = 2`.
- `NotInformed = 3`.

O uso de enum para sexo é adequado porque reduz variações textuais e melhora a previsibilidade das entradas e saídas da API futura.

### Mapeamento atual

Na configuração EF Core, `Pet` é mapeado para a tabela `Pets` com:

- chave primária em `PatientId`;
- `PatientId` com `ValueGeneratedNever()`, pois o valor vem do `Patient` previamente criado;
- `TutorId` obrigatório;
- `Species` obrigatório com tamanho máximo de 40;
- `Breed` opcional com tamanho máximo de 60;
- `Sex` obrigatório convertido para `string`;
- `WeightKg` com precisão decimal `(6, 2)`;
- `Microchip` opcional com tamanho máximo de 40;
- índice em `TutorId`;
- índice em `Microchip`.

### Avaliação técnica

A entidade está adequada como especialização veterinária de `Patient`. O desenho atual indica que `Pet` não é uma entidade independente com identificador próprio; ele depende do ciclo de vida e da identidade clínica de `Patient`.

Essa decisão simplifica a navegação clínica, pois o identificador do pet perante o núcleo clínico é o próprio `PatientId`. Em contrapartida, exige que todos os fluxos de criação, atualização e exclusão sejam desenhados considerando simultaneamente `Patient` e `Pet`.

## 5. Relacionamento Patient/Pet

O relacionamento atual entre `Patient` e `Pet` deve ser entendido como uma especialização 1:1:

- `Patient` é a entidade clínica base.
- `Pet` é uma especialização veterinária vinculada a `Patient`.
- `Pet.PatientId` é simultaneamente chave primária de `Pets` e chave estrangeira para `Patients.Id`.
- O relacionamento `Patient → Pet` é 1:1.
- `Pet.PatientId` não é gerado automaticamente; ele deve receber o `Id` de um `Patient` já criado.
- A configuração atual permite que a exclusão de `Patient` exclua o `Pet` por cascade.

A migration do núcleo clínico confirma essa modelagem ao criar `Pets` com `PatientId` como chave primária e FK para `Patients`, usando exclusão em cascata.

Essa decisão precisa ser tratada com cuidado nas próximas fases. O cascade de `Patient` para `Pet` é coerente do ponto de vista estrutural, mas pode ter impacto relevante quando atendimento, prontuário, evoluções clínicas e prescrições estiverem em uso. Excluir fisicamente um `Patient` pode afetar histórico clínico e rastreabilidade. Por isso, antes de implementar exclusão definitiva no CRUD de Pet, deve-se avaliar se o comportamento correto será exclusão física ou inativação lógica.

## 6. Relacionamento Tutor/Pet

`Tutor` representa o responsável pelo animal. No modelo atual, `Pet` possui `TutorId`, estabelecendo o vínculo operacional entre o paciente veterinário e seu responsável.

A relação esperada é:

- Um `Tutor` pode ter vários `Pets`.
- Cada `Pet` deve possuir um `TutorId` válido.
- O relacionamento é essencial para o CRUD de Pet, pois a criação de um pet deve validar previamente se o tutor existe.
- A exclusão de `Tutor` deve ser restrita quando houver `Pets` vinculados.

A configuração atual do EF Core define `Pet → Tutor` com `OnDelete(DeleteBehavior.Restrict)`, o que está alinhado à regra esperada: não permitir a exclusão de um tutor enquanto houver pets associados a ele.

Essa restrição é importante para preservar consistência referencial e evitar pets sem responsável. No fluxo futuro de criação, atualização e exclusão de Pet, o `TutorId` deve ser tratado como dado obrigatório e validado antes da persistência.

## 7. Decisão recomendada para o CRUD

A recomendação técnica é implementar, nas próximas fases, um CRUD público de Pet, e não um CRUD público separado de Patient neste momento.

Motivo principal: no contexto veterinário, o usuário final pensa e opera em termos de “Pet”, não em termos de “Patient”. `Patient` deve permanecer como entidade clínica base do domínio, mas sua criação e atualização devem acontecer internamente no fluxo da aplicação.

### Modelo recomendado para futuro `POST /api/pets`

O fluxo conceitual recomendado é:

1. Receber uma requisição pública de criação de pet.
2. Validar se o `TutorId` informado existe.
3. Criar um `Patient` com `Type = PatientType.Pet`.
4. Criar um `Pet` usando o `PatientId` gerado.
5. Vincular o `Pet` ao `Tutor` por meio de `TutorId`.
6. Persistir `Patient` e `Pet` de forma transacional.
7. Retornar uma resposta consolidada com dados clínicos e veterinários.

### Request conceitual futuro

```http
POST /api/pets
```

```json
{
  "tutorId": 1,
  "name": "Thor",
  "birthDate": "2021-05-10",
  "status": "Active",
  "species": "Dog",
  "breed": "Labrador",
  "sex": "Male",
  "weightKg": 18.5,
  "microchip": "ABC123"
}
```

### Resposta conceitual futura

```json
{
  "patientId": 10,
  "tutorId": 1,
  "name": "Thor",
  "birthDate": "2021-05-10",
  "status": "Active",
  "species": "Dog",
  "breed": "Labrador",
  "sex": "Male",
  "weightKg": 18.5,
  "microchip": "ABC123"
}
```

Essa abordagem mantém o modelo clínico correto e oferece uma API alinhada à linguagem do usuário e ao domínio veterinário.

## 8. Rotas futuras planejadas

As rotas abaixo são recomendadas para fases futuras. Elas não devem ser implementadas nesta fase.

```text
GET    /api/pets
GET    /api/pets/{patientId}
POST   /api/pets
PUT    /api/pets/{patientId}
DELETE /api/pets/{patientId}
```

O identificador público inicial deve ser `patientId`, porque `Pet` não possui `Id` próprio no modelo atual. O `PatientId` é a chave primária de `Pets` e também o identificador clínico que será usado por atendimento e prontuário.

Caso futuramente o produto exija um identificador próprio de Pet separado do identificador clínico, essa decisão deverá ser analisada em uma fase específica, com migration planejada e avaliação de impacto. Não há recomendação para alterar esse desenho agora.

## 9. Pontos de atenção

Antes da implementação do CRUD público de Pet, os seguintes pontos devem ser considerados:

- `Pet` não possui `Id` próprio; usa `PatientId` como chave primária e FK.
- Criar `Pet` exige criar `Patient` antes.
- A criação de `Patient` + `Pet` deve ser transacional.
- Não pode sobrar `Patient` órfão se a criação de `Pet` falhar.
- `Microchip` possui índice, mas não há unicidade configurada atualmente; pode ser necessário definir regra de unicidade quando informado.
- `Tutor` precisa existir antes de criar `Pet`.
- A exclusão de `Patient/Pet` precisa ser pensada com cuidado por causa de atendimento, prontuário e histórico clínico futuros.
- Futuramente, delete físico talvez precise virar inativação lógica.
- `Status` de `Patient` talvez precise virar enum futuramente, mas isso não deve ser alterado agora.
- `PatientType` deve permanecer para permitir evolução futura do núcleo clínico.
- O cascade de `Patient` para `Pet` deve ser reavaliado quando `Attendance`, `MedicalRecord`, evoluções clínicas e prescrições passarem a compor fluxos reais de operação.
- A resposta da API deve evitar expor detalhes internos desnecessários, mas deve retornar `patientId` por ser o identificador operacional atual.
- Logs futuros devem usar identificadores e flags, como `PatientId`, `TutorId` e `HasMicrochip`, evitando payload completo e dados sensíveis.

## 10. Fora do escopo desta fase

Nesta fase não será decidido nem implementado:

- PDV.
- Financeiro.
- Estoque.
- Vacinas.
- Prontuário completo.
- Atendimento completo.
- Upload de arquivos.
- Relatórios.
- OpenTelemetry.
- Serilog.
- Docker.
- CRUD de Pet.
- Controller de Pet.
- Use cases de Pet.
- Repository de Pet.
- Validators de Pet.
- Migrations.
- Database update.
- Alterações em testes.
- Alterações em frontend.
- Alterações em `Program.cs`.
- Alterações em `appsettings`.
- Instalação de pacotes.
- Alterações de CI ou branch protection.

## 11. Plano das próximas fases

### Fase 3.2 — Testar domínio Patient/Pet

- Criar testes unitários para `Patient`.
- Criar testes unitários para `Pet`.
- Validar criação, atualização e regras básicas.

### Fase 3.3 — Criar contratos

- `CreatePetRequest`.
- `UpdatePetRequest`.
- `PetResponse`.
- `PetListItemResponse`.

### Fase 3.4 — Criar interface de repository

- Definir `IPetRepository`.
- Definir métodos necessários para listar, buscar, criar, atualizar e remover `Pet/Patient`.

### Fase 3.5 — Implementar repository EF com transação

- Criar `Patient` e `Pet` de forma transacional.
- Evitar `Patient` órfão.

### Fase 3.6 — Criar validators

- `TutorExistsValidator`.
- `PetMicrochipUniquenessValidator`, se microchip for informado.

### Fase 3.7 — Criar use cases

- `CreatePetUseCase`.
- `ListPetsUseCase`.
- `GetPetByIdUseCase`.
- `UpdatePetUseCase`.
- `DeletePetUseCase`.

### Fase 3.8 — Criar controller

- `PetsController`.
- Rotas protegidas com `Authorize`.
- Conversão de `ApplicationResult` para HTTP status.

### Fase 3.9 — Aplicar logs

- `ILogger` no controller, use cases e validators.
- Não logar payload completo.
- Não logar dados sensíveis.
- Usar `PatientId`, `TutorId` e `HasMicrochip`.

### Fase 3.10 — Criar testes

- Testes de domínio.
- Testes de use cases quando a estrutura estiver pronta.
- Testes de API se necessário.

### Fase 3.11 — Testar manualmente

- Swagger/Postman.
- Validar `200`, `201`, `400`, `401`, `404`, `409` e `204`.

### Fase 3.12 — Documentar fechamento

- Registrar decisões, endpoints, testes e validações.


## Fase 3.2 — Testes de domínio Patient/Pet

A Fase 3.2 foi executada como etapa de validação do domínio clínico antes da implementação do CRUD público de Pet. A fase foi dividida em criação dos testes de `Patient`, criação dos testes de `Pet` e validação local por build/test.

### 3.2.1 — PatientTests

Foram criados testes unitários de domínio em `backend/src/Togo.Domain.Tests/PatientTests.cs` para validar o comportamento da entidade `Patient`.

Os testes cobrem:

- criação válida;
- normalização de `Name` com `Trim()`;
- normalização de `Status` com `Trim()`;
- `BirthDate` preenchido;
- `BirthDate` `null`;
- `CreatedAt` obrigatório;
- `UpdatedAt` `null` inicialmente;
- atualização válida;
- atualização com `BirthDate` `null`;
- preservação de `Type`;
- preservação de `CreatedAt`;
- validação de `Name` obrigatório;
- validação de `Status` obrigatório;
- validação de `UpdatedAt` obrigatório.

### 3.2.2 — PetTests

Foram criados testes unitários de domínio em `backend/src/Togo.Domain.Tests/PetTests.cs` para validar o comportamento da entidade `Pet`.

Os testes cobrem:

- criação válida;
- normalização de `Species`, `Breed` e `Microchip`;
- campos opcionais `null`;
- campos opcionais vazios normalizados para `null`;
- `PatientId` obrigatório maior que zero;
- `TutorId` obrigatório maior que zero;
- `Species` obrigatório;
- `WeightKg` zero inválido;
- `WeightKg` negativo inválido;
- `UpdateProfile` válido;
- `UpdateProfile` com campos opcionais `null`;
- `UpdateProfile` normalizando campos opcionais vazios;
- preservação de `PatientId`;
- preservação de `TutorId`.

### 3.2.3 — Validação local

A validação local foi executada manualmente com os seguintes comandos:

```bash
dotnet build backend/Togo.sln
dotnet test backend/Togo.sln
```

Resultado registrado:

- build: sucesso;
- testes: 45 total, 45 passaram, 0 falharam, 0 ignorados.

Também foi registrado que:

- nenhum código de produção foi alterado nesta fase documental;
- nenhuma migration foi criada;
- nenhum `database update` foi executado;
- nenhum pacote foi instalado;
- os testes são de domínio puro;
- os testes não acessam banco;
- os testes não dependem de EF Core;
- os testes não criam mocks;
- os testes validam o comportamento das entidades antes da implementação do CRUD.

Conclusão da fase: a Fase 3.2 está concluída, com `Patient` e `Pet` cobertos por testes unitários de domínio suficientes para avançar para a Fase 3.3.

Próxima fase recomendada: Fase 3.3 — Criar contratos de API/Application para Pet.

## Fase 3.3 — Contratos de API/Application para Pet

A Fase 3.3 consolidou os contratos de entrada e saída que serão usados futuramente pelo CRUD público de Pet. Esta etapa mantém a decisão arquitetural de expor Pet como recurso público da API, enquanto `Patient` continua sendo criado e atualizado internamente pela aplicação.

### 3.3.1 — Contratos criados

Foram criados os seguintes contratos na camada Application:

- `CreatePetRequest`.
- `UpdatePetRequest`.
- `PetResponse`.
- `PetListItemResponse`.

#### CreatePetRequest

`CreatePetRequest` será usado futuramente pelo endpoint `POST /api/pets`. Ele contém os dados necessários para criar internamente um `Patient` e um `Pet` no mesmo fluxo de aplicação.

Este contrato não possui `PatientId` no body, pois o `Patient` será criado internamente e seu identificador será gerado durante a persistência.

Campos previstos:

- `TutorId`.
- `Name`.
- `BirthDate`.
- `Status`.
- `Species`.
- `Breed`.
- `Sex`.
- `WeightKg`.
- `Microchip`.

#### UpdatePetRequest

`UpdatePetRequest` será usado futuramente pelo endpoint `PUT /api/pets/{patientId}`. Ele não possui `PatientId` no body, porque o identificador público do recurso virá pela rota como `patientId`.

O contrato contém os mesmos campos editáveis do create, permitindo atualizar os dados consolidados de `Patient` e `Pet` sem duplicar o identificador no payload.

#### PetResponse

`PetResponse` representa a resposta detalhada consolidada de `Patient` + `Pet`. Ele usa `PatientId` como identificador público inicial do pet, refletindo a modelagem atual em que `Pet` não possui `Id` próprio.

A resposta detalhada também contém `CreatedAt` e `UpdatedAt` vindos de `Patient`, preservando os metadados temporais da entidade clínica base.

#### PetListItemResponse

`PetListItemResponse` representa a resposta resumida para listagem de pets. Ele também usa `PatientId` como identificador público inicial.

Por ser um item de listagem, este contrato não traz dados de Tutor, prontuário, atendimentos ou outros dados pesados. O objetivo é manter a listagem leve e adequada para telas e consultas resumidas.

### 3.3.2 — Decisões técnicas dos contratos

As decisões técnicas registradas para os contratos de Pet são:

- Os contratos ficam no namespace `Togo.Application.Pets.Contracts`.
- Os contratos ficam na camada Application.
- Os contratos não possuem validação.
- As validações serão implementadas futuramente em validators e/ou use cases.
- Os contratos não dependem de EF Core.
- Os contratos não representam tabelas do banco de dados.
- Os contratos não possuem comportamento de domínio.
- Os contratos são DTOs de entrada e saída da aplicação/API.
- O campo `Sex` usa `PetSex` do domínio, e não `string`.
- `PatientId` não entra em `CreatePetRequest` nem em `UpdatePetRequest`.
- `PatientId` entra nas responses, como `PetResponse` e `PetListItemResponse`.
- No update, get by id e delete futuros, o `patientId` deverá ser recebido pela rota.

Essa decisão evita inconsistência entre body e rota, mantém a criação de `Patient` encapsulada no fluxo de aplicação e preserva `PatientId` como identificador público inicial enquanto `Pet` não tiver um identificador próprio.

### Pontos de atenção para próximas fases

Nas próximas fases, os seguintes pontos devem ser observados:

- Os use cases deverão mapear `CreatePetRequest` para a criação interna de `Patient` + `Pet`.
- O repository deverá persistir `Patient` + `Pet` de forma transacional.
- Os validators deverão validar se o Tutor informado existe.
- Os validators deverão validar microchip duplicado, se `Microchip` for informado e a regra de unicidade for adotada.
- O controller deverá receber `patientId` pela rota nos fluxos de update, delete e get by id.
- Logs futuros devem evitar payload completo.
- Logs futuros devem evitar microchip completo, preferindo identificadores, flags e dados mínimos necessários.
- A conversão de resultados para HTTP deve manter consistência com `ApplicationResult`.

### Conclusão da Fase 3.3

A Fase 3.3 está concluída do ponto de vista de contratos. Os DTOs de entrada e saída para o futuro CRUD público de Pet foram criados e a decisão técnica foi documentada: requests não carregam `PatientId`, responses usam `PatientId`, e o `patientId` será fornecido pela rota nos endpoints que operam sobre um pet existente.

É seguro avançar para a próxima etapa planejada:

Fase 3.4 — Criar interface de repository para Pet/Patient.

## 12. Conclusão técnica

A recomendação é seguir com CRUD público de Pet, criando e atualizando `Patient` internamente, mantendo `Patient` como entidade clínica base e `Pet` como especialização veterinária.

O modelo atual está adequado para iniciar as próximas fases, desde que a implementação futura respeite estas decisões:

- a API pública deve expor recursos de `Pet`;
- `Patient` deve ser criado e atualizado internamente;
- `PatientType.Pet` deve ser usado no fluxo de criação de pets;
- `PatientId` deve ser o identificador público inicial do pet;
- criação de `Patient` + `Pet` deve ser transacional;
- exclusões devem ser avaliadas cuidadosamente por causa do histórico clínico futuro;
- regras como unicidade de microchip e status tipado devem ser avaliadas nas fases seguintes, sem alteração nesta etapa.

Próxima fase recomendada: Fase 3.3 — Criar contratos de API/Application para Pet.
