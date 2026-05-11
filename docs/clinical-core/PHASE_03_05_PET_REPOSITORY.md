# TOGO — Fase 3.5: Repository EF Core de Pet/Patient

## 1. Objetivo da fase

A Fase 3.5 implementa a persistência concreta EF Core para Pet/Patient na camada Infrastructure, respeitando a interface `IPetRepository` definida na camada Application.

Essa implementação estabelece o ponto de acesso a dados para as operações de leitura, criação, atualização e remoção de Pet/Patient, sem acoplar a camada Application ao EF Core ou ao `AppDbContext`.

Esta fase ainda não expõe endpoint HTTP e ainda não registra a implementação no container de DI. Portanto, nenhuma rota pública passa a existir como resultado desta fase, e nenhuma alteração em `Program.cs` foi realizada.

## 2. Arquivo criado

Arquivo criado na Fase 3.5.1:

- `backend/src/Togo.Infrastructure/Repositories/PetRepository.cs`

O arquivo foi colocado em `Togo.Infrastructure/Repositories` porque esse é o padrão real já existente no projeto para repositories concretos da camada Infrastructure. A decisão mantém consistência com o `TutorRepository` e evita a criação de uma estrutura paralela desnecessária para repositories de Pet/Patient.

Essa organização reforça que a implementação EF Core pertence à Infrastructure, enquanto a Application permanece responsável apenas pelo contrato (`IPetRepository`) e pelos dados/projections necessários ao fluxo de aplicação.

## 3. Responsabilidade do PetRepository

O `PetRepository` é responsável por implementar `IPetRepository` usando `AppDbContext` como mecanismo concreto de persistência EF Core.

Responsabilidades documentadas:

- implementa `IPetRepository`;
- usa `AppDbContext` recebido via construtor;
- faz leitura consolidada de `Patient` + `Pet`;
- cria `Patient` + `Pet`;
- atualiza `Patient` + `Pet`;
- remove `Patient` por `PatientId`, confiando no cascade atual para remover o `Pet` associado;
- retorna projections para leitura;
- não retorna entidades de domínio para fora do repository;
- não expõe `IQueryable`, `DbSet` ou `AppDbContext` para a camada Application ou para consumidores externos.

Com isso, o repository atua como fronteira técnica entre a camada Application e os detalhes de persistência da Infrastructure.

## 4. Métodos implementados

### ListAsync

O método `ListAsync` lista pets com dados consolidados de `Patient` + `Pet`.

Comportamento documentado:

- usa `AsNoTracking` para leitura sem rastreamento;
- retorna `PetListItemProjection`;
- consolida os campos necessários de `Patient` e `Pet`;
- ordena o resultado por `Name`;
- não retorna tutor completo;
- não retorna prontuário;
- não retorna atendimento.

Esse método deve ser usado para listagem resumida, preservando o limite do contexto de Pet/Patient e evitando carregar agregados ou informações clínicas ainda não modeladas nesta etapa.

### GetByPatientIdAsync

O método `GetByPatientIdAsync` busca um Pet/Patient pelo identificador público `PatientId`.

Comportamento documentado:

- busca por `PatientId`;
- retorna `PetDetailsProjection` quando encontra o registro;
- retorna `null` se não encontrar;
- usa leitura consolidada de `Patient` + `Pet`.

A decisão mantém `PatientId` como identificador público da operação, sem introduzir `PetId` em contratos externos nesta fase.

### TutorExistsAsync

O método `TutorExistsAsync` verifica se existe um tutor com o `Id` informado.

Comportamento documentado:

- verifica existência de `Tutor` por `Id`;
- não carrega o `Tutor` completo;
- será usado futuramente por validator/use case para validar vínculo entre Pet e Tutor.

Esse método existe como apoio para regras futuras de aplicação, mantendo a verificação de existência eficiente e restrita à Infrastructure.

### MicrochipExistsAsync

O método `MicrochipExistsAsync` verifica duplicidade de microchip.

Comportamento documentado:

- verifica se já existe `Pet` com o microchip informado;
- permite `ignorePatientId` para cenários de update;
- não normaliza microchip no repository;
- a normalização do microchip será responsabilidade futura de validator/use case antes de chamar o repository.

Essa decisão evita misturar regra de entrada/normalização com persistência concreta, preservando o repository como componente de acesso a dados.

### CreateAsync

O método `CreateAsync` cria um registro de `Patient` e o respectivo registro de `Pet`.

Comportamento documentado:

- cria `Patient` + `Pet`;
- usa transação explícita;
- cria `Patient` com `Patient.Create`;
- salva `Patient` para gerar `Patient.Id`;
- cria `Pet` com `Pet.Create` usando o `PatientId` gerado;
- salva `Pet`;
- faz commit da transação;
- em caso de erro, faz rollback e relança a exceção com `throw`;
- evita `Patient` órfão se a criação de `Pet` falhar.

A transação explícita é a decisão técnica central deste método. Como a criação depende de duas tabelas relacionadas (`Patients` e `Pets`), a operação precisa ser atômica: ou ambos os registros são persistidos, ou nenhum deles permanece gravado.

### UpdateAsync

O método `UpdateAsync` atualiza os dados consolidados de `Patient` + `Pet`.

Comportamento documentado:

- atualiza `Patient` + `Pet`;
- usa transação explícita;
- busca `Patient` por `PatientId`;
- busca `Pet` por `PatientId`;
- retorna `null` se algum dos dois registros não existir;
- atualiza `Patient` com `patient.Update`;
- atualiza `Pet` com `pet.UpdateProfile`;
- salva as alterações;
- faz commit da transação;
- retorna `PetDetailsProjection`.

A transação explícita garante que a atualização de `Patient` e `Pet` permaneça consistente. Se ocorrer erro durante a operação, as alterações não devem ficar parcialmente persistidas.

### DeleteAsync

O método `DeleteAsync` remove um Pet/Patient a partir do `PatientId`.

Comportamento documentado:

- busca `Patient` por `PatientId`;
- retorna `false` se não encontrar;
- remove `Patient`;
- confia no cascade atual para remover o `Pet` associado;
- mantém delete físico nesta fase.

O delete físico foi mantido por enquanto porque ainda não há Atendimento/Prontuário integrados ao fluxo clínico. Essa decisão deve ser reavaliada antes da introdução de Atendimento/Prontuário, pois a existência de histórico clínico pode exigir soft delete, bloqueio de exclusão ou outra política de preservação de dados.

## 5. Decisões técnicas importantes

Decisões registradas nesta fase:

- `PatientId` continua sendo o identificador público para Pet/Patient;
- `PetId` não foi criado nem usado como identificador público;
- o repository não recebe `CreatePetRequest` nem `UpdatePetRequest`;
- o repository recebe `CreatePetRepositoryData` e `UpdatePetRepositoryData`;
- projections são usadas para leitura;
- EF Core fica restrito à camada Infrastructure;
- a camada Application continua desacoplada da Infrastructure;
- DI não foi registrado nesta fase;
- use cases ainda não existem;
- controller ainda não existe;
- validators ainda não existem.

Essas decisões preservam a separação entre contrato de aplicação, modelos de entrada HTTP e persistência EF Core. O repository trabalha com dados próprios da Application e projections, sem depender diretamente de requests de API.

## 6. Pontos de atenção encontrados

Pontos que devem ser observados nas próximas fases:

- `CreateAsync` depende de futuro use case enviar `PatientType.Pet` corretamente;
- microchip deve ser normalizado antes de chegar em `MicrochipExistsAsync`;
- `DeleteAsync` usa delete físico por enquanto;
- delete físico deve ser reavaliado antes de Atendimento/Prontuário;
- ainda falta registrar `IPetRepository`/`PetRepository` no DI em fase posterior;
- ainda falta criar validators para tutor existente e microchip duplicado;
- ainda falta criar use cases;
- ainda falta criar controller.

Esses pontos não bloqueiam a Fase 3.5.2, pois esta fase é documental e não altera comportamento de runtime.

## 7. Validação esperada

Após pull local, o desenvolvedor humano deve executar:

```bash
dotnet build backend/Togo.sln
dotnet test backend/Togo.sln
```

Resultado esperado:

- build com sucesso;
- testes existentes continuam passando;
- total esperado atual: 45 testes passando.

O resultado acima é a expectativa de validação local. Esta documentação não deve inventar resultado de build/test caso os comandos não tenham sido executados pelo Codex nesta tarefa.

Para esta Fase 3.5.2, a validação obrigatória executada pelo Codex é apenas:

```bash
git diff --check
```

## 8. Conclusão da fase

A Fase 3.5.1 implementou corretamente o repository EF Core de Pet/Patient na camada Infrastructure, respeitando a interface `IPetRepository` da camada Application e mantendo EF Core restrito à Infrastructure.

A Fase 3.5.2 documentou a decisão técnica, as responsabilidades do `PetRepository`, o comportamento de cada método implementado, as decisões arquiteturais relevantes e os pontos de atenção para as próximas fases.

Próxima fase recomendada:

**Fase 3.5.3 — Registrar IPetRepository/PetRepository no DI.**
