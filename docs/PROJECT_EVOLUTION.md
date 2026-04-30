# TOGO — Project Evolution Log

## 1. Objetivo do documento

Este documento registra a evolução técnica do projeto TOGO ao longo das fases de reconstrução do backend em C#/.NET. O objetivo é manter um histórico vivo de decisões arquiteturais, uso de IA/Codex, validações humanas, testes, PRs, problemas encontrados, aprendizados e próximos passos.

A proposta é permitir rastreabilidade técnica e facilitar a continuidade do projeto por qualquer desenvolvedor do time.

## 2. Visão geral do projeto

O TOGO é um sistema de gestão veterinária inspirado no MVP TOGO 1, reconstruído com foco em qualidade de engenharia, organização e escalabilidade no backend.

Stack e base técnica atual:
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- MySQL
- Arquitetura em camadas:
  - `Togo.Domain`
  - `Togo.Application`
  - `Togo.Infrastructure`
  - `Togo.Api`

Objetivo de engenharia: evoluir para uma base igual ou superior ao MVP inicial, com maior modularidade, manutenção sustentável e melhor separação de responsabilidades.

## 3. Papel da IA, Codex e revisão humana

A evolução do projeto foi apoiada por IA como ferramenta de produtividade técnica, mantendo responsabilidade final de análise e decisão no desenvolvimento humano.

- ChatGPT foi usado como apoio para análise, planejamento técnico, revisão arquitetural, geração de prompts técnicos e validação conceitual.
- A versão utilizada na fase de análise foi **ChatGPT GPT-5.5 Thinking**.
- Codex foi usado para executar alterações controladas no repositório, com prompts específicos e escopos pequenos.
- O desenvolvedor humano revisou entregas, conferiu logs, analisou PRs, executou comandos locais, validou banco, testou endpoints, identificou problemas e tomou decisões finais.
- A responsabilidade técnica final permanece humana.

Princípio adotado:

> O objetivo da formação em Sistemas de Informação não é apenas “programar/codar”, mas construir sistemas com análise, arquitetura, banco de dados, segurança, manutenção, validação, documentação e evolução contínua.

## 4. Ferramentas utilizadas no processo

### ChatGPT
Usado para:
- análise arquitetural;
- decomposição do problema em fases menores;
- geração de prompts técnicos;
- revisão conceitual;
- apoio na interpretação de erros;
- planejamento de próximos passos.

### Codex
Usado para:
- gerar alterações controladas no repositório;
- criar entidades;
- criar mapeamentos EF Core;
- criar documentação;
- implementar CRUD de Tutor;
- abrir/organizar PRs.

### GitHub
Usado para:
- versionamento;
- PRs;
- merge no branch `main`;
- revisão de diffs;
- histórico de evolução.

### Git CLI / Terminal
Usado para:
- `dotnet build`;
- `dotnet ef migrations add`;
- `dotnet ef database update`;
- `git add`;
- `git commit`;
- `git pull --rebase`;
- `git push`;
- correção de conflitos/sincronização.

### .NET CLI
Usado para:
- restaurar dependências;
- compilar solução;
- gerar migrations;
- aplicar migrations;
- executar API localmente.

### dotnet-ef
Usado para:
- gerar migrations EF Core;
- remover migration incorreta;
- aplicar migrations no banco local.

### MySQL Workbench
Usado para:
- inspeção visual do banco;
- conferência do EER;
- validação das tabelas criadas;
- análise das tabelas legadas em português e novas tabelas em inglês.

### Postman
Usado para:
- testes manuais dos endpoints;
- validação de status HTTP;
- validação de headers;
- envio de token Bearer;
- testes de CRUD;
- testes de regra de negócio.

### Swagger
Usado ou previsto para:
- exposição visual dos endpoints;
- validação rápida da API;
- inspeção de rotas disponíveis.

### Visual Studio
Usado ou previsto para:
- apoio ao desenvolvimento backend;
- depuração;
- execução local;
- análise de solução .NET.

### VS Code
Usado ou previsto para:
- edição de código;
- terminal integrado;
- apoio ao frontend;
- análise de estrutura do projeto.

### Navegador
Usado para:
- validação direta de endpoints GET;
- inspeção rápida de retorno JSON.

## 5. Linha do tempo das fases executadas

### Fase inicial — Análise do MVP TOGO 1

- O MVP TOGO 1 foi usado como referência funcional e visual.
- Foram identificados módulos como tutores/clientes, pets, agenda, atendimentos, vacinas, produtos, PDV, financeiro, relatórios e configurações.
- Foi decidido que o TOGO C# usaria o MVP como inspiração, com arquitetura mais robusta.

### Fase 0 — Organização antes de crescer

- Criação de documentação técnica base.
- Definição da arquitetura em camadas.
- Decisão sobre tratamento de tabelas antigas em português.
- Definição do padrão oficial: código novo e banco novo em inglês.
- EF Core migrations como padrão obrigatório de evolução estrutural.
- MySQL Workbench definido apenas para inspeção/análise visual.

Referências:
- `docs/ARCHITECTURE.md`
- `docs/DATABASE_GUIDELINES.md`
- `docs/DEVELOPMENT_GUIDELINES.md`

### Fase 2 — Criação do domínio clínico inicial

Criação das entidades clínicas no Domain:
- `Tutor`
- `Patient`
- `Pet`
- `MedicalRecord`
- `Attendance`
- `ClinicalEvolution`
- `Prescription`
- `PrescriptionItem`

Decisões da fase:
- Entidades clínicas usam `long`.
- IDs principais gerados pelo MySQL via `AUTO_INCREMENT`.
- `User` permanece com `Guid` por compatibilidade com autenticação existente.
- Domain sem dependência de EF Core.
- Encapsulamento com `private set` e métodos de comportamento (`Create`/`Update`).

### Fase 2.3 — Mapeamento EF Core

- `AppDbContext` passou a mapear entidades clínicas.
- Configurações EF Core foram separadas por entidade.
- `UserConfiguration` preservou comportamento existente.
- Enums mapeados como string.
- Tabelas novas criadas em inglês.

### Fase 2.4 — Migration do núcleo clínico

- Criação da migration `AddClinicalCoreEntities`.
- Problema inicial: primeira migration saiu vazia devido ao repositório local desatualizado.
- Falha identificada em revisão humana.
- Migration vazia removida.
- Migration regenerada corretamente.
- Aprendizado consolidado: revisar migration antes de aplicar no banco.

### Fase 2.5 — Aplicação da migration no MySQL local

- Migration correta aplicada localmente com `dotnet ef database update`.
- Tabelas criadas:
  - `Patients`
  - `Tutors`
  - `Pets`
  - `MedicalRecords`
  - `Attendances`
  - `ClinicalEvolutions`
  - `Prescriptions`
  - `PrescriptionItems`
- `__EFMigrationsHistory` registrou a migration aplicada.
- Conferência visual validada via MySQL Workbench (EER).

### Fase 1.1 — CRUD de Tutor

Implementação ponta a ponta do CRUD de Tutor, incluindo:
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

Diretrizes consolidadas nesta fase:
- Registro de dependências no `Program.cs`.
- Controller fino orquestrando use cases.
- Repositório EF Core isolado na Infrastructure.
- Use cases sem dependência de `IActionResult`.

## 6. Testes manuais realizados no backend

Esta etapa é crítica porque valida comportamento real da feature além de compilação e build.

### 6.1 API subindo localmente

Validado que:
- API subiu corretamente;
- ambiente HTTPS local foi resolvido;
- certificado local deixou de bloquear os testes;
- endpoints ficaram acessíveis via localhost.

### 6.2 Autenticação/token

Validado que:
- login/token local estava disponível para os testes;
- Postman foi configurado com Bearer Token;
- requisições foram enviadas com header `Authorization`.

Ponto de atenção:
- testar com Bearer Token não comprova sozinho exigência de autenticação;
- para validar segurança real, é necessário testar removendo header `Authorization`;
- se responder 200 sem token, rota está funcional, porém não protegida;
- essa validação deve ser tratada como melhoria de segurança, se ainda não implementada.

### 6.3 GET `/api/tutors`

Validado:
- retorno `200 OK`;
- lista vazia `[]` quando não havia dados;
- após criação, lista retornou dados persistidos.

### 6.4 POST `/api/tutors`

Validado:
- criação de tutor com body JSON;
- retorno `201 Created`;
- retorno com campos:
  - `id`
  - `name`
  - `document`
  - `email`
  - `phone`
  - `createdAt`
  - `updatedAt` como `null`

Exemplo de body testado:

```json
{
  "name": "Reinaldo Couto",
  "document": "12345678900",
  "email": "reinaldo@email.com",
  "phone": "11999999999"
}
```

### 6.5 Regra de documento duplicado

Validado:
- tentativa de cadastro com mesmo `document` retornou `409 Conflict`;
- mensagem retornada: `A tutor with this document already exists.`
- regra de unicidade validada na camada Application/Repository.

### 6.6 GET `/api/tutors/{id}`

Validado:
- tutor existente retornou `200 OK`;
- tutor removido/inexistente retornou `404 Not Found`;
- mensagem retornada: `Tutor not found.`

### 6.7 PUT `/api/tutors/{id}`

Validado:
- atualização de tutor existente retornou `200 OK`;
- campos atualizados foram persistidos;
- `updatedAt` passou a ser preenchido após update.

### 6.8 DELETE `/api/tutors/{id}`

Validado:
- delete físico retornou `204 No Content`;
- após delete, GET por id retornou `404 Not Found`.

Ponto de atenção:
- delete físico é aceitável nesta fase;
- para produto real, avaliar soft delete (`IsActive` ou `DeletedAt`) antes de cenários com vínculos fortes (`Pet`/`Patient`).

## 7. Decisões arquiteturais importantes

- Código novo em inglês.
- Banco novo em inglês.
- Tabelas legadas em português não são usadas em código novo.
- EF Core migrations são obrigatórias para mudanças estruturais.
- Controllers devem ser finos.
- Use cases ficam na Application.
- Repositories concretos ficam na Infrastructure.
- Domain não depende de banco/API.
- Toda migration deve ser revisada antes de aplicar.
- Endpoints devem ser validados com Swagger/Postman.
- Regra de negócio deve ser testada além de status 200.
- Segurança deve ser validada com e sem token.

## 8. Problemas encontrados e aprendizados

1. **Ambiente Codex sem dotnet CLI disponível.**
   - Solução: execução local dos comandos dotnet.

2. **Build local falhou por DLL bloqueada.**
   - Causa: processo `Togo.Api` em execução.
   - Solução: encerrar processo antes do build.

3. **Migration vazia.**
   - Causa: repositório local atrasado em relação ao `main` remoto.
   - Solução: `git pull --rebase`, remover migration incorreta e gerar novamente.

4. **Push rejeitado por `main` remoto à frente.**
   - Solução: sincronizar com `pull --rebase` e fazer push após ajuste.

5. **Arquivo indevido `caramelo.cs`.**
   - Solução: remover do commit antes do push.

6. **HTTPS/certificado local.**
   - Solução: ajustar ambiente local para permitir chamadas HTTPS no Postman/navegador.

7. **Testes de API exigiram validação em Postman.**
   - Aprendizado: feature pronta não é apenas código compilado; é endpoint respondendo, regra de negócio funcionando e dados persistidos.

## 9. Fluxo de trabalho adotado

1. Conversa e análise técnica.
2. Quebra do problema em fases menores.
3. Geração de prompt específico para Codex.
4. Codex executa alteração controlada.
5. Revisão humana do resumo/diff.
6. Build/testes locais quando necessário.
7. Correção de problemas.
8. PR.
9. Merge.
10. Validação no banco/API.
11. Registro no documento de evolução.

## 10. Estado atual do projeto

- Backend com arquitetura em camadas.
- Núcleo clínico inicial modelado.
- Banco local com tabelas clínicas novas.
- CRUD de Tutor implementado.
- API validada manualmente via Postman.
- GET, POST, PUT, DELETE e regra de documento duplicado testados.
- Projeto pronto para avançar para Patient/Pet.
- Segurança dos endpoints deve ser validada explicitamente com teste sem token e, se necessário, protegida com autorização.

## 11. Próximas fases planejadas

### Fase 1.2 — CRUD de Patient/Pet

Objetivo:
- Criar fluxo de cadastro de paciente/pet vinculado ao tutor.

### Fase 1.3 — MedicalRecord básico

Objetivo:
- Criar/consultar prontuário do paciente.

### Fase 1.4 — Attendance básico

Objetivo:
- Abrir e encerrar atendimento.

### Fase 1.5 — ClinicalEvolution e Prescription

Objetivo:
- Registrar evolução clínica e prescrição.

### Fase futura — Vacinas, estoque, PDV e financeiro

Objetivo:
- Evoluir para módulos equivalentes ou superiores ao MVP TOGO 1.

### Fase futura — Segurança e autorização

Objetivo:
- Validar e proteger endpoints sensíveis.
- Aplicar autorização quando necessário.
- Testar acesso com token válido, token inválido e sem token.
- Definir roles/perfis futuramente.

## 12. Como atualizar este documento

- Toda fase relevante deve adicionar uma nova entrada.
- Toda decisão arquitetural importante deve ser registrada.
- Toda correção crítica deve ser documentada.
- Toda validação manual importante deve ser descrita.
- Evitar excesso de detalhe irrelevante.
- Registrar o suficiente para outro desenvolvedor entender o histórico.

---

Este documento deve crescer junto com o projeto e ser atualizado a cada avanço técnico relevante.
