# TOGO — Diário de Bordo Técnico e Evolução do Projeto — Fase 01

> **Documento de evolução — Fase 01**  
> Período coberto: fundação técnica do backend, domínio clínico inicial, CRUD de Tutor, segurança JWT, refatoração SOLID e testes iniciais.  
> Próximas fases deverão ser documentadas em arquivos separados para evitar perda de histórico e reduzir risco de conflitos em documentos muito grandes.

---

## 1. Objetivo deste documento

Este documento registra a evolução técnica inicial do projeto **TOGO**, funcionando como um diário de bordo da construção do backend em C#/.NET.

O objetivo não é apenas listar commits ou funcionalidades, mas registrar:

- planejamento técnico;
- decisões arquiteturais;
- fases executadas;
- uso de IA como apoio;
- uso do Codex como executor controlado;
- revisão humana;
- validações locais;
- testes manuais;
- testes automatizados;
- problemas encontrados;
- correções aplicadas;
- aprendizados;
- próximos passos.

Além de registrar a evolução do sistema, este documento também registra a evolução do desenvolvedor no processo de construção do produto, com foco em análise, arquitetura, segurança, banco de dados, validação, manutenção e evolução contínua.

Este documento cobre a **Fase 01 documental**. A partir da próxima etapa, novos documentos deverão ser criados para cada bloco relevante de evolução.

---

## 2. Visão geral do projeto

O **TOGO** é um sistema de gestão veterinária inspirado no MVP **TOGO 1**. O backend está sendo reconstruído em **C#/.NET**, com o objetivo de criar uma base igual ou superior ao MVP inicial, porém mais organizada, escalável, modular, testável e sustentável.

### 2.1 Stack atual

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- MySQL
- JWT
- Swagger
- xUnit
- Git/GitHub
- Arquitetura em camadas:
  - `Togo.Domain`
  - `Togo.Application`
  - `Togo.Infrastructure`
  - `Togo.Api`

### 2.2 Objetivo técnico

Criar uma base backend capaz de sustentar o fluxo principal do sistema veterinário:

```text
Login
↓
Dashboard
↓
Tutores/Clientes
↓
Pacientes/Pets
↓
Agenda
↓
Atendimento
↓
Prontuário
↓
Evolução clínica / Prescrição
```

Os módulos de vacinas, produtos, estoque, PDV, financeiro e relatórios serão evoluídos depois que o fluxo clínico principal estiver sólido.

---

## 3. Papel da IA, Codex e revisão humana

A evolução do projeto foi apoiada por IA como ferramenta de produtividade técnica, mas mantendo a responsabilidade final de análise, validação e decisão no desenvolvedor humano.

### 3.1 ChatGPT

O ChatGPT foi usado para:

- análise arquitetural;
- planejamento técnico;
- quebra de problemas grandes em fases menores;
- revisão conceitual;
- geração de prompts técnicos para o Codex;
- interpretação de erros;
- análise de aderência a SOLID/Clean Architecture;
- apoio na tomada de decisão;
- revisão de diffs e PRs;
- validação conceitual dos próximos passos.

Versão usada na fase de análise:

```text
ChatGPT GPT-5.5 Thinking
```

### 3.2 Codex

O Codex foi usado para:

- executar alterações controladas no repositório;
- criar arquivos e estruturas;
- implementar fases pequenas;
- abrir PRs;
- atualizar documentação quando solicitado;
- realizar tarefas orientadas por prompts específicos.

O uso do Codex foi sempre orientado por escopos pequenos, para reduzir risco de alterações grandes demais e difíceis de revisar.

### 3.3 Revisão humana

A revisão humana permaneceu como etapa obrigatória. O desenvolvedor humano executou:

- revisão do resumo do Codex;
- conferência de código no GitHub;
- revisão de diffs;
- execução de build local;
- execução de testes locais;
- validação manual no Postman;
- validação via Swagger;
- inspeção do banco no MySQL Workbench;
- correção de problemas locais;
- decisão final sobre avançar ou bloquear uma fase.

A responsabilidade técnica final é **humana**.

### 3.4 Ponto importante sobre IA e validação

O Codex acelerou a implementação, mas falhas reais só foram encontradas na revisão local humana, por exemplo:

- dependências JWT faltando no projeto correto;
- incompatibilidade entre Swagger/OpenAPI e versão do Swashbuckle;
- endpoint respondendo `200 OK` sem token;
- projeto de teste criado em `net10.0` quando o padrão era `net8.0`;
- testes falhando por comparação frágil de `ArgumentException.Message`.

Esse ponto é importante porque demonstra que IA/Codex não substitui o processo de engenharia. O uso correto é:

```text
IA apoia análise e execução
↓
Codex acelera implementação
↓
Humano revisa, valida, corrige e decide
```

---

## 4. Ferramentas utilizadas

### 4.1 ChatGPT

Usado para análise, planejamento, revisão conceitual, geração de prompts e apoio na interpretação de erros.

### 4.2 Codex

Usado para execução controlada de tarefas no repositório.

### 4.3 GitHub

Usado para:

- versionamento;
- PRs;
- merge no branch `main`;
- revisão de diffs;
- histórico de evolução.

### 4.4 Git CLI / Terminal

Comandos utilizados:

```bash
git status
git diff
git add
git commit
git push
git pull --rebase
dotnet build
dotnet test
dotnet run
dotnet ef migrations add
dotnet ef database update
dotnet add package
```

### 4.5 .NET CLI

Usado para:

- restaurar dependências;
- compilar solução;
- executar API localmente;
- gerar migrations;
- aplicar migrations;
- rodar testes;
- gerenciar pacotes.

### 4.6 dotnet-ef

Usado para:

- gerar migrations EF Core;
- remover migration incorreta;
- aplicar migrations no banco local.

### 4.7 MySQL Workbench

Usado para:

- inspeção visual do banco;
- conferência do EER;
- validação das tabelas criadas;
- análise das tabelas legadas;
- conferência da migration aplicada.

### 4.8 Postman

Usado para testes manuais de:

- login;
- token;
- GET/POST/PUT/DELETE;
- status HTTP;
- duplicidade de documento;
- endpoint sem token;
- endpoint com token.

### 4.9 Swagger

Usado para:

- inspeção visual de endpoints;
- validação do botão `Authorize`;
- testes autenticados com Bearer Token.

### 4.10 Visual Studio

Usado como apoio para:

- solução .NET;
- estrutura dos projetos;
- depuração;
- análise de testes.

### 4.11 VS Code

Usado como apoio para:

- edição de arquivos;
- terminal integrado;
- inspeção de estrutura;
- apoio futuro ao frontend.

### 4.12 Navegador

Usado para:

- validação rápida de endpoints GET;
- inspeção de retorno JSON;
- acesso ao Swagger.

---

## 5. Linha do tempo da Fase 01

> A numeração das fases segue a evolução real do projeto e parte do planejamento anterior. Algumas fases aparecem como “Fase 2” antes da “Fase 1.1” porque o domínio clínico foi preparado antes do CRUD de Tutor.

---

### 5.1 Fase inicial — Análise do MVP TOGO 1

O MVP TOGO 1 foi usado como referência funcional e visual.

Foram identificados os principais módulos:

- tutores/clientes;
- pets;
- agenda;
- atendimentos;
- vacinas;
- produtos;
- PDV;
- financeiro;
- relatórios;
- configurações.

Decisão tomada:

```text
O TOGO C# será inspirado no MVP TOGO 1,
mas com backend mais robusto, organizado, escalável e testável.
```

---

### 5.2 Fase 0 — Organização antes de crescer

Antes de criar novas telas e endpoints, foi definida a base técnica do projeto.

Documentos criados:

- `docs/ARCHITECTURE.md`
- `docs/DATABASE_GUIDELINES.md`
- `docs/DEVELOPMENT_GUIDELINES.md`

Decisões registradas:

- manter tabelas antigas em português apenas como referência;
- não usar tabelas antigas em código novo;
- padronizar código novo em inglês;
- padronizar banco novo em inglês;
- usar EF Core migrations como padrão obrigatório;
- usar MySQL Workbench apenas para inspeção/análise visual;
- evitar criação manual de tabelas para o núcleo novo.

---

### 5.3 Fase 2 — Criação do domínio clínico inicial

Foram criadas as entidades clínicas iniciais no projeto `Togo.Domain`.

Entidades criadas:

- `Tutor`
- `Patient`
- `Pet`
- `MedicalRecord`
- `Attendance`
- `ClinicalEvolution`
- `Prescription`
- `PrescriptionItem`

Enums criados:

- `PatientType`
- `PetSex`
- `AttendanceStatus`
- `AttendanceType`
- `EvolutionType`

Decisões da fase:

- entidades clínicas usam `long`;
- `User` permanece com `Guid`, por compatibilidade com autenticação existente;
- Domain não depende de EF Core;
- propriedades usam `private set`;
- entidades usam métodos de comportamento como `Create` e `Update`;
- validações básicas foram concentradas nas entidades quando fazem sentido para o domínio.

---

### 5.4 Fase 2.1 — Ajuste das entidades para IDs gerados pelo banco

Foi ajustado o modelo para que os IDs principais não fossem recebidos nos métodos `Create`.

Decisão:

```text
O MySQL/EF Core será responsável por gerar IDs via AUTO_INCREMENT.
```

Ajustes realizados:

- remoção do `Id` dos métodos `Create` principais;
- manutenção de `private set` para compatibilidade com EF Core;
- preservação das validações de FKs como `> 0`;
- `Pet` permaneceu vinculado por `PatientId` e `TutorId`.

---

### 5.5 Fase 2.3 — Mapeamento EF Core

O `AppDbContext` passou a mapear as entidades clínicas.

Foram criadas configurações separadas por entidade, seguindo a camada `Infrastructure`.

Principais decisões:

- configs EF Core separadas por entidade;
- `UserConfiguration` preservando comportamento da autenticação;
- enums mapeados como string;
- tabelas novas em inglês;
- Domain sem dependência de EF Core;
- `PrescriptionItem.ProductId` mantido como campo simples, sem relacionamento neste momento.

---

### 5.6 Fase 2.4 — Migration do núcleo clínico

Foi criada a migration:

```text
AddClinicalCoreEntities
```

Problema encontrado:

- a primeira migration saiu vazia.

Causa:

- repositório local estava desatualizado em relação ao `main` remoto.

Correção:

- sincronizar com o `main`;
- remover a migration incorreta;
- gerar novamente a migration;
- revisar o conteúdo antes de aplicar no banco.

Aprendizado:

```text
Migration precisa ser revisada antes de ser aplicada.
Build passar não basta.
Migration vazia ou errada pode comprometer o banco.
```

---

### 5.7 Fase 2.5 — Aplicação da migration no MySQL local

A migration correta foi aplicada no banco local com:

```bash
dotnet ef database update
```

Tabelas criadas:

- `Patients`
- `Tutors`
- `Pets`
- `MedicalRecords`
- `Attendances`
- `ClinicalEvolutions`
- `Prescriptions`
- `PrescriptionItems`

Validações realizadas:

- conferência no MySQL Workbench;
- validação visual do EER;
- conferência do registro em `__EFMigrationsHistory`.

---

### 5.8 Fase 1.1 — CRUD de Tutor

Foi implementado o CRUD de Tutor ponta a ponta.

Arquivos/estruturas principais:

- `TutorsController`
- `ITutorRepository`
- `TutorRepository`
- `ApplicationResult<T>`
- DTOs/Requests/Responses
- `CreateTutorUseCase`
- `UpdateTutorUseCase`
- `GetTutorByIdUseCase`
- `ListTutorsUseCase`
- `DeleteTutorUseCase`

Endpoints criados:

```http
GET    /api/tutors
GET    /api/tutors/{id}
POST   /api/tutors
PUT    /api/tutors/{id}
DELETE /api/tutors/{id}
```

Decisões da fase:

- controller fino;
- use cases na camada Application;
- repository concreto na Infrastructure;
- API sem acesso direto ao `AppDbContext`;
- use cases sem dependência de `IActionResult`;
- regra de documento duplicado retornando `409 Conflict`.

---

### 5.9 Fase 1.1.1 — Segurança JWT dos endpoints de Tutor

Durante teste manual, foi identificado que:

```http
GET /api/tutors
```

retornava:

```text
200 OK
```

mesmo sem token.

Diagnóstico:

```text
O CRUD estava funcional, mas não estava protegido.
```

Solução adotada:

- criação do `JwtTokenService`;
- migração de token em memória para JWT real;
- configuração de `AddAuthentication`;
- configuração de `AddJwtBearer`;
- validação de issuer, audience, lifetime e signing key;
- `[Authorize]` aplicado em `TutorsController`;
- `[AllowAnonymous]` aplicado ao login;
- Swagger configurado com botão `Authorize`.

Testes esperados:

```text
POST /api/auth/login sem token → 200 OK + JWT
GET /api/tutors sem token → 401 Unauthorized
GET /api/tutors com token → 200 OK
POST /api/tutors sem token → 401 Unauthorized
POST /api/tutors com token → 201 Created
Swagger Authorize → funcionando
```

Observação:

```text
Em produção, a secret JWT não deve ficar hardcoded.
Ela deve vir de variável de ambiente ou secret manager.
```

---

### 5.10 Fase 1.1.1.1 — Correções pós-build da autenticação JWT

O Codex implementou a solução JWT, mas não conseguiu executar build por ausência de `.NET CLI` no ambiente.

O build local humano encontrou erros reais.

Problemas encontrados:

1. `JwtTokenService` estava em `Togo.Infrastructure`, mas pacotes JWT tinham sido adicionados inicialmente apenas em `Togo.Api`.
2. Foi necessário adicionar `System.IdentityModel.Tokens.Jwt` em `Togo.Infrastructure`.
3. Foi necessário adicionar `Microsoft.Extensions.Configuration.Binder` em `Togo.Infrastructure` para uso de `GetValue`.
4. `Swashbuckle.AspNetCore 10.0.1` apresentou incompatibilidade com o padrão de configuração OpenAPI usado.
5. Foi feito ajuste para `Swashbuckle.AspNetCore 6.6.2`, mantendo `Microsoft.OpenApi.Models`.

Resultado:

```text
dotnet build passou localmente.
```

Aprendizado:

```text
Código gerado por IA precisa de validação local.
Build local é obrigatório.
Revisão humana detecta problemas que o Codex não consegue validar sem ambiente completo.
```

Esse ponto foi considerado essencial no processo de evolução do projeto.

---

### 5.11 Fase 1.1.2 — Refatoração SOLID da regra de documento único do Tutor

Durante revisão humana, foi identificado que a regra de documento único estava repetida em:

- `CreateTutorUseCase`
- `UpdateTutorUseCase`

Discussão arquitetural:

```text
Nem todo if deve virar classe.
Mas regras repetidas em 2 ou mais use cases,
com potencial de evolução e reuso,
devem ser centralizadas.
```

Solução:

- criação de `TutorDocumentUniquenessValidator`;
- `CreateTutorUseCase` passou a usar o validator;
- `UpdateTutorUseCase` passou a usar o validator com `ignoreTutorId`.

Comportamento preservado:

```text
Documento duplicado continua retornando 409 Conflict.
Documento vazio/nulo continua permitido.
Update ignora o próprio Tutor ao validar duplicidade.
```

Ganhos:

- melhora do Single Responsibility Principle;
- redução de duplicação;
- centralização da regra;
- manutenção mais simples;
- maior escalabilidade da regra.

---

### 5.12 Fase 1.1.3 — Testes unitários do domínio Tutor

Foi criado o projeto:

```text
backend/src/Togo.Domain.Tests
```

O projeto foi incluído na solution.

Testes criados para `Tutor`:

- criação válida;
- nome vazio;
- nome com whitespace;
- data default;
- trim de nome;
- documento vazio vira `null`;
- documento com whitespace é trimado;
- update de contato;
- update de nome;
- validações de `updatedAt`.

Problema inicial:

```text
O projeto de testes foi criado em net10.0,
mas o padrão do projeto TOGO é net8.0.
```

Correção:

- `TargetFramework` ajustado para `net8.0`;
- `UnitTest1.cs` removido.

Aprendizado:

```text
Após criar projeto via template, revisar imediatamente TargetFramework e arquivos gerados automaticamente.
```

---

### 5.13 Fase 1.1.3.1 — Correção dos testes de ArgumentException

Os testes falharam porque comparavam a mensagem completa da exceção:

```csharp
Assert.Equal("Name is required", exception.Message);
```

Problema:

No .NET, `ArgumentException` adiciona automaticamente o nome do parâmetro na mensagem:

```text
Name is required (Parameter 'name')
Date is required (Parameter 'createdAt')
Date is required (Parameter 'updatedAt')
```

Correção:

```csharp
Assert.StartsWith("Name is required", exception.Message);
Assert.StartsWith("Date is required", exception.Message);
Assert.Equal("name", exception.ParamName);
Assert.Equal("createdAt", exception.ParamName);
Assert.Equal("updatedAt", exception.ParamName);
```

Aprendizado:

```text
Teste bom deve validar comportamento estável.
A mensagem completa da exception pode variar.
ParamName é mais confiável para validar o parâmetro.
```

A regra de domínio estava correta. O problema estava no teste.

---

## 6. Testes manuais realizados

### 6.1 API local e HTTPS

Validado:

- API subiu localmente;
- certificado HTTPS local foi ajustado;
- endpoints ficaram acessíveis via localhost.

### 6.2 Login e JWT

Validado:

- login retornando `200 OK`;
- token retornado em formato JWT real;
- token com três partes (`header.payload.signature`);
- uso no Postman com Bearer Token.

### 6.3 Segurança sem token

Validação crítica:

```http
GET /api/tutors
```

sem token deve retornar:

```text
401 Unauthorized
```

Esse teste se tornou obrigatório após identificação de retorno `200 OK` sem autenticação.

### 6.4 CRUD Tutor

Validações realizadas:

- `GET /api/tutors` lista registros;
- `POST /api/tutors` cria tutor;
- `GET /api/tutors/{id}` busca por id;
- `PUT /api/tutors/{id}` atualiza tutor;
- `DELETE /api/tutors/{id}` remove tutor;
- `GET` após delete retorna `404`;
- documento duplicado retorna `409`.

### 6.5 Swagger

Validado:

- botão `Authorize`;
- uso de Bearer Token;
- teste autenticado via Swagger.

### 6.6 Banco de dados

Validado:

- persistência dos dados;
- criação das tabelas clínicas;
- registros conferidos;
- Workbench usado para inspeção.

---

## 7. Testes automatizados

Foi criado o projeto:

```text
Togo.Domain.Tests
```

Framework:

```text
xUnit
```

Foco inicial:

```text
Entidade Tutor
```

Objetivos:

- proteger regras de domínio;
- evitar regressões;
- validar comportamento de `Create`;
- validar comportamento de `Update`;
- validar normalização de campos opcionais;
- validar exceções de domínio.

Aprendizados:

- `ArgumentException.Message` pode incluir detalhes automáticos;
- `ParamName` deve ser validado separadamente;
- testes unitários devem evitar acoplamento frágil com detalhes instáveis.

---

## 8. Decisões arquiteturais consolidadas

### 8.1 Nomenclatura

- código novo em inglês;
- banco novo em inglês;
- tabelas legadas em português apenas como referência.

### 8.2 Banco e migrations

- MySQL como banco oficial;
- EF Core migrations obrigatórias;
- Workbench apenas para inspeção;
- toda migration deve ser revisada antes de aplicar.

### 8.3 Camadas

- `Domain` sem EF Core;
- `Application` com use cases, contracts, validators e interfaces;
- `Infrastructure` com EF Core e implementações concretas;
- `Api` com controllers, DI, Swagger e autenticação.

### 8.4 SOLID

Decisões aplicadas:

- controller fino;
- use case como orquestrador;
- validators/rules para regras reutilizáveis;
- repository via interface;
- implementação EF isolada;
- Domain independente de banco/API.

Regra prática adotada:

```text
Nem todo if deve virar classe.
Extrair regra quando houver repetição, reuso, relevância de negócio ou potencial de evolução.
```

### 8.5 Segurança

- endpoints sensíveis exigem JWT;
- login deve permanecer público;
- validar sempre com token e sem token;
- token inválido deve retornar `401`;
- secret JWT em produção deve vir de variável de ambiente ou secret manager.

### 8.6 Testes

- testes unitários devem começar pelo Domain;
- regras críticas devem ser testadas antes de novas fases;
- build local é obrigatório;
- teste manual de API ainda é necessário, especialmente em segurança e persistência.

---

## 9. Problemas encontrados e aprendizados

### 9.1 Ambiente Codex sem .NET CLI

Problema:

```text
Codex não conseguiu executar dotnet build/test.
```

Causa:

```text
Limitação do ambiente.
```

Correção:

```text
Build e testes executados localmente pelo desenvolvedor.
```

Aprendizado:

```text
Entrega assistida por IA precisa de validação local.
```

---

### 9.2 Build local falhou por DLL bloqueada

Problema:

```text
Build falhou porque arquivos DLL estavam bloqueados.
```

Causa:

```text
Processo Togo.Api ainda estava em execução.
```

Correção:

```text
Encerrar processo antes do build.
```

Aprendizado:

```text
Garantir ambiente limpo antes de compilar.
```

---

### 9.3 Migration vazia

Problema:

```text
Migration criada sem alterações.
```

Causa:

```text
Repositório local desatualizado.
```

Correção:

```text
git pull --rebase
remover migration incorreta
gerar migration novamente
```

Aprendizado:

```text
Migration precisa ser revisada antes de aplicar.
```

---

### 9.4 Push rejeitado por main remoto à frente

Problema:

```text
git push rejeitado.
```

Causa:

```text
main remoto tinha commits que o local ainda não possuía.
```

Correção:

```bash
git pull --rebase origin main
git push origin main
```

Aprendizado:

```text
Sincronizar antes de commitar reduz retrabalho.
```

---

### 9.5 Arquivo indevido `caramelo.cs`

Problema:

```text
Arquivo não planejado entrou no fluxo de commit.
```

Correção:

```text
Remover do commit antes do push.
```

Aprendizado:

```text
Revisar git status e git diff antes de commitar.
```

---

### 9.6 HTTPS/certificado local

Problema:

```text
Chamadas locais HTTPS tiveram bloqueio inicial.
```

Correção:

```text
Ajustar ambiente local/certificado para permitir testes.
```

Aprendizado:

```text
Nem todo erro de chamada HTTP é erro de código.
Ambiente local também precisa ser validado.
```

---

### 9.7 Endpoint funcionando sem autenticação

Problema:

```text
GET /api/tutors retornava 200 sem token.
```

Correção:

```text
JWT + [Authorize] + testes sem token.
```

Aprendizado:

```text
Testar com token não prova que a rota exige token.
É obrigatório testar também sem token.
```

---

### 9.8 Dependências JWT no projeto errado

Problema:

```text
Pacotes JWT adicionados inicialmente apenas em Togo.Api.
```

Causa:

```text
JwtTokenService estava em Togo.Infrastructure.
```

Correção:

```text
Adicionar dependências no projeto correto.
```

Aprendizado:

```text
Pacote deve estar no projeto onde o código compila/executa.
```

---

### 9.9 Falta de `System.IdentityModel.Tokens.Jwt`

Problema:

```text
JwtSecurityTokenHandler e SymmetricSecurityKey não encontrados.
```

Correção:

```text
Adicionar System.IdentityModel.Tokens.Jwt em Togo.Infrastructure.
```

---

### 9.10 Falta de `Microsoft.Extensions.Configuration.Binder`

Problema:

```text
IConfiguration.GetValue não encontrado.
```

Correção:

```text
Adicionar Microsoft.Extensions.Configuration.Binder em Togo.Infrastructure.
```

---

### 9.11 Incompatibilidade com `Swashbuckle.AspNetCore 10.0.1`

Problema:

```text
Conflito com configuração OpenAPI usada no projeto.
```

Correção:

```text
Ajuste para Swashbuckle.AspNetCore 6.6.2.
```

Aprendizado:

```text
Nem sempre a versão mais nova é a melhor para o contexto atual.
```

---

### 9.12 Projeto de testes criado em `net10.0`

Problema:

```text
Projeto de testes desalinhado com o padrão do projeto.
```

Correção:

```text
Alterar TargetFramework para net8.0.
```

---

### 9.13 `UnitTest1.cs` gerado por template

Problema:

```text
Arquivo padrão sem valor de negócio.
```

Correção:

```text
Remover UnitTest1.cs.
```

---

### 9.14 Testes falhando por `Assert.Equal` em `ArgumentException.Message`

Problema:

```text
Falso negativo nos testes.
```

Causa:

```text
ArgumentException adiciona ParamName automaticamente à mensagem.
```

Correção:

```text
Usar Assert.StartsWith em Message e Assert.Equal em ParamName.
```

---

### 9.15 Importância da revisão humana após Codex

Problema:

```text
Confiar somente na geração automática mascara defeitos.
```

Correção:

```text
Institucionalizar revisão humana: build, testes e validação manual.
```

Aprendizado:

```text
IA acelera. Qualidade final e responsabilidade técnica continuam humanas.
```

---

## 10. Fluxo de trabalho adotado

Fluxo consolidado nesta fase:

1. Análise da necessidade.
2. Quebra em problemas menores.
3. Definição da fase.
4. Geração de prompt específico para Codex.
5. Codex executa escopo pequeno.
6. Revisão do resumo do Codex.
7. Revisão do diff/código.
8. Build local.
9. Testes locais.
10. Testes manuais em Postman/Swagger.
11. Inspeção de banco quando necessário.
12. Correções humanas.
13. PR/merge.
14. Atualização do diário de bordo.
15. Avanço somente sem pendências críticas.

Princípio adotado:

```text
Não avançar para próxima fase deixando rabo.
```

---

## 11. Estado atual ao fim da Fase 01

Ao final desta fase documental, o projeto possui:

- backend em camadas;
- núcleo clínico inicial modelado;
- banco local com tabelas clínicas;
- migration clínica aplicada;
- CRUD de Tutor implementado;
- JWT implementado;
- `TutorsController` protegido com `[Authorize]`;
- login público com `[AllowAnonymous]`;
- Swagger com `Authorize`;
- regra de documento único centralizada em `TutorDocumentUniquenessValidator`;
- testes unitários iniciais de `Tutor`;
- projeto de testes alinhado com `net8.0`;
- asserts de `ArgumentException` corrigidos;
- documentação técnica de arquitetura, banco e desenvolvimento;
- diário de bordo técnico iniciado.

Validações locais esperadas antes da próxima fase:

```bash
dotnet build backend/Togo.sln
dotnet test backend/Togo.sln
```

Validações manuais esperadas:

```text
POST /api/auth/login → 200 OK + JWT
GET /api/tutors sem token → 401 Unauthorized
GET /api/tutors com token → 200 OK
POST /api/tutors com token → 201 Created
Documento duplicado → 409 Conflict
Swagger Authorize → funcionando
```

---

## 12. Próximas fases planejadas

A próxima fase deve ser registrada em novo documento, para evitar que este arquivo fique grande demais e reduza a chance de perda de informações.

Sugestão de próximo documento:

```text
docs/PROJECT_EVOLUTION_PHASE_02.md
```

### 12.1 Fase 02 — Fluxo Patient/Pet

Fatiamento sugerido:

- Fase 2.1 — CRUD básico de `Patient`;
- Fase 2.2 — CRUD de `Pet` vinculado a `Tutor` e `Patient`;
- Fase 2.3 — Validação do fluxo `Tutor → Patient → Pet`;
- Fase 2.4 — Testes de domínio/use cases;
- Fase 2.5 — Atualização do diário da Fase 02.

### 12.2 Fases posteriores

- MedicalRecord básico;
- Attendance básico;
- ClinicalEvolution;
- Prescription;
- Vacinas;
- Produtos;
- Estoque;
- PDV;
- Financeiro;
- Relatórios;
- roles/perfis/autorização avançada;
- soft delete;
- auditoria.

---

## 13. Regras para próximos documentos de evolução

Para evitar documentos muito grandes e risco de perda de informação, a partir daqui a evolução será documentada em arquivos separados por fase.

### 13.1 Padrão de nomenclatura sugerido

```text
docs/PROJECT_EVOLUTION_PHASE_01.md
docs/PROJECT_EVOLUTION_PHASE_02.md
docs/PROJECT_EVOLUTION_PHASE_03.md
```

### 13.2 O que cada documento deve registrar

Cada documento de fase deve registrar:

- objetivo da fase;
- escopo;
- decisões tomadas;
- prompts relevantes usados;
- arquivos criados/alterados;
- validações humanas;
- testes manuais;
- testes automatizados;
- problemas encontrados;
- correções aplicadas;
- aprendizados;
- critérios para avançar;
- próximos passos.

### 13.3 Regra de manutenção

Toda fase relevante deve ser documentada com transparência.

Falhas, erros de ambiente, decisões erradas e correções também devem ser registradas. Isso não enfraquece o projeto; pelo contrário, mostra maturidade técnica e rastreabilidade.

---

## 14. Encerramento da Fase 01 documental

Esta Fase 01 consolidou a fundação técnica do backend TOGO.

Foram estabelecidos:

- domínio clínico inicial;
- mapeamento EF Core;
- migration clínica;
- CRUD de Tutor;
- autenticação JWT;
- proteção de endpoint sensível;
- refatoração SOLID;
- testes unitários iniciais;
- processo de revisão humana;
- fluxo de trabalho com IA/Codex;
- padrão de documentação evolutiva.

A próxima etapa deverá continuar a partir de um novo documento de evolução, iniciando pela implementação do fluxo **Patient/Pet**.

---

**Fim do documento — Fase 01.**
