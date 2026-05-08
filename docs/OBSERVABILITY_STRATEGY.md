# TOGO — Estratégia de Observabilidade

## 1. Objetivo

O objetivo da observabilidade no TOGO é permitir entender o comportamento da aplicação em execução, especialmente no backend .NET 8 organizado nas camadas `Togo.Domain`, `Togo.Application`, `Togo.Infrastructure` e `Togo.Api`.

Nesta Fase 2.0, o foco é preparar o projeto conceitualmente antes de implementar logs, métricas, traces, monitoramento ou alertas. A intenção é estabelecer uma base técnica clara para que as próximas alterações sejam simples, incrementais, seguras e alinhadas à arquitetura existente.

A observabilidade deve ajudar a responder perguntas como:

- O que aconteceu?
- Quando aconteceu?
- Onde aconteceu?
- Qual endpoint foi afetado?
- Qual usuário ou operação estava envolvida?
- Quanto tempo demorou?
- O erro foi isolado ou recorrente?
- O sistema está saudável?

Para o TOGO, isso significa evoluir a capacidade de diagnóstico do backend sem alterar o comportamento funcional da API e sem expor dados sensíveis.

## 2. O que é observabilidade

Observabilidade é a capacidade de compreender o estado interno de um sistema a partir dos sinais que ele gera externamente.

Em uma aplicação backend, esses sinais são produzidos durante a execução da API, dos casos de uso, dos acessos ao banco de dados, da autenticação e das integrações com infraestrutura. Eles permitem investigar problemas, acompanhar a saúde do sistema e entender como os usuários e operações interagem com a aplicação.

É importante deixar claro que:

- Observabilidade não é apenas log.
- Logs fazem parte da observabilidade.
- Observabilidade também envolve métricas, traces, monitoramento e alertas.

No TOGO, a estratégia deve começar simples, usando recursos nativos do ecossistema .NET, e evoluir conforme o projeto ganhar necessidade real de análise, volume, automação e integração com ferramentas externas.

## 3. Pilares da observabilidade

### 3.1 Logs

Logs são registros de eventos relevantes que acontecem durante a execução do sistema. Eles ajudam a entender decisões tomadas pela aplicação, falhas esperadas, erros inesperados e operações importantes do negócio.

Exemplos de eventos que podem gerar logs no TOGO:

- login realizado;
- falha de login;
- tutor criado;
- tutor atualizado;
- documento duplicado;
- token inválido;
- erro inesperado;
- falha de banco.

Os níveis de log mais comuns no .NET são:

- `Trace`: detalhes extremamente granulares, normalmente usados apenas em diagnósticos específicos.
- `Debug`: informações técnicas úteis durante desenvolvimento ou investigação.
- `Information`: eventos relevantes do fluxo normal da aplicação.
- `Warning`: situações esperadas, mas que merecem atenção.
- `Error`: falhas inesperadas em uma operação específica.
- `Critical`: falhas graves que podem comprometer a disponibilidade ou integridade do sistema.

Logs precisam ser úteis, mas também seguros. Portanto, logs no TOGO não devem conter:

- senha;
- token JWT completo;
- CPF/documento completo;
- dados sensíveis;
- payloads inteiros sem necessidade.

### 3.2 Métricas

Métricas são números acompanhados ao longo do tempo. Elas permitem observar tendências, degradação de desempenho e comportamentos anormais.

Exemplos de métricas úteis para o TOGO:

- quantidade de requisições;
- tempo médio de resposta;
- quantidade de erros 500;
- quantidade de logins inválidos;
- uso de CPU/memória;
- número de tutores cadastrados;
- número de conflitos por documento duplicado.

Enquanto logs ajudam a explicar eventos individuais, métricas ajudam a enxergar o comportamento agregado do sistema. Por exemplo, um único erro 500 pode ser investigado por log, mas o aumento da taxa de erros 500 ao longo de uma hora é melhor percebido por métricas.

### 3.3 Traces / Rastreamento

Um trace representa o caminho de uma requisição dentro do sistema. Ele mostra as etapas percorridas por uma operação, permitindo entender onde ela gastou tempo, onde falhou ou quais dependências foram acionadas.

Exemplo conceitual no TOGO:

```text
POST /api/tutors
→ TutorsController
→ CreateTutorUseCase
→ TutorDocumentUniquenessValidator
→ TutorRepository
→ MySQL
```

Traces ajudam a descobrir onde uma operação ficou lenta ou falhou. Em arquiteturas maiores, eles se tornam ainda mais importantes para rastrear chamadas entre serviços. No estágio atual do TOGO, o conceito deve ser compreendido e planejado, mas sua implementação pode ficar para uma fase futura.

### 3.4 Monitoramento

Monitoramento acompanha a saúde do sistema por meio de sinais coletados continuamente. Ele busca responder se a aplicação está disponível, está respondendo dentro do esperado e está operando sem degradação relevante.

Exemplos de monitoramento para o TOGO:

- API online;
- banco respondendo;
- tempo médio de resposta aceitável;
- taxa de erro controlada;
- autenticação funcionando.

Monitoramento não se limita a identificar erros. Ele também ajuda a perceber lentidão, indisponibilidade parcial, aumento de uso de recursos e mudanças de comportamento ao longo do tempo.

### 3.5 Alertas

Alertas notificam quando algo foge do esperado. Eles são acionados com base em regras, limites ou padrões definidos a partir de logs, métricas, traces ou verificações de saúde.

Exemplos de alertas futuros para o TOGO:

- muitos erros 500;
- API indisponível;
- tempo médio de resposta alto;
- falhas de login fora do padrão;
- banco indisponível.

Alertas serão tratados em fase futura. Neste momento, o objetivo é definir a estratégia e preparar a base conceitual, não implementar notificações automáticas.

## 4. Observabilidade no ecossistema .NET

### 4.1 ILogger

`ILogger` é o mecanismo nativo de logging do .NET. Ele é integrado ao ASP.NET Core, funciona com injeção de dependência e permite registrar mensagens em diferentes níveis, como `Information`, `Warning` e `Error`.

Para o TOGO, `ILogger` deve ser o primeiro passo. Ele é simples, já faz parte do ecossistema .NET e é suficiente para iniciar a padronização dos logs sem adicionar bibliotecas externas.

Exemplo conceitual:

```csharp
_logger.LogInformation("Tutor criado com sucesso. TutorId: {TutorId}", tutor.Id);
_logger.LogWarning("Documento duplicado ao criar Tutor. DocumentHash: {DocumentHash}", documentHash);
_logger.LogError(ex, "Erro inesperado ao criar Tutor");
```

Esse exemplo é apenas documental. Nenhum código do projeto deve ser alterado nesta Fase 2.0.

### 4.2 Microsoft.Extensions.Logging

`Microsoft.Extensions.Logging` é a infraestrutura base de logging do .NET. Ela define abstrações como `ILogger`, `ILoggerFactory` e providers de logging.

Essa infraestrutura permite plugar diferentes providers sem acoplar o código da aplicação a uma ferramenta específica. Ela pode ser usada com console, debug, Serilog, Application Insights e outros destinos de logs.

No TOGO, essa abstração favorece uma evolução incremental: começar com o padrão nativo e, se necessário, adicionar ferramentas mais robustas posteriormente.

### 4.3 Serilog

Serilog é uma biblioteca popular para logs estruturados no ecossistema .NET. Ela permite registrar eventos com propriedades nomeadas e enviá-los para diferentes destinos, como console, arquivo, Seq, Elastic, Grafana e outros.

Apesar de ser uma opção forte, Serilog não deve ser adotado imediatamente nesta fase. Ele deve ser considerado em uma etapa futura, depois que o padrão de logging com `ILogger` estiver definido e validado no TOGO.

### 4.4 NLog

NLog é uma alternativa ao Serilog e também é bastante usada no ecossistema .NET. Ele oferece recursos de configuração, roteamento e armazenamento de logs em múltiplos destinos.

No contexto do TOGO, NLog será apenas registrado como ferramenta conhecida. Não há decisão de adotá-lo agora.

### 4.5 Application Insights

Application Insights é um serviço da Microsoft/Azure voltado para telemetria de aplicações. Ele pode coletar logs, métricas, traces, dados de performance e erros.

Essa ferramenta pode ser útil caso o TOGO seja executado em ambiente cloud/Azure ou precise de integração gerenciada para diagnóstico e monitoramento. Porém, sua adoção envolve decisões de ambiente, custo, privacidade e configuração operacional.

Application Insights não será implementado agora.

### 4.6 OpenTelemetry

OpenTelemetry é um padrão aberto para observabilidade. Ele suporta logs, métricas e traces, além de permitir exportar telemetria para várias ferramentas e plataformas.

É uma abordagem poderosa e flexível, especialmente para sistemas distribuídos, mas também traz maior complexidade de configuração, operação e análise.

Para o TOGO, OpenTelemetry deve ficar para uma fase futura, quando houver necessidade clara de métricas e traces padronizados.

## 5. Estratégia inicial para o TOGO

O TOGO deve começar simples. A estratégia inicial é criar uma base de logging útil e segura antes de adicionar ferramentas mais complexas.

Fase inicial:

- usar `ILogger`;
- adicionar logs úteis em pontos críticos;
- não adicionar biblioteca externa de observabilidade ainda;
- não implementar OpenTelemetry ainda;
- não criar alertas ainda;
- não gerar dashboards ainda.

Objetivo inicial:

- tornar Auth e Tutor observáveis;
- registrar eventos importantes;
- registrar erros inesperados;
- evitar vazamento de dados sensíveis;
- criar padrão para logs futuros.

Essa abordagem reduz risco, evita complexidade prematura e permite evoluir com base em necessidades reais do projeto.

## 6. Eventos candidatos para logging no TOGO

Os eventos abaixo são candidatos para logging nas próximas fases. Nem todos precisam ser implementados imediatamente. Esta lista serve como mapa para orientar a Fase 2.1 e as etapas seguintes.

### Auth/Login

- tentativa de login;
- login realizado com sucesso;
- falha de login;
- usuário não encontrado;
- senha inválida;
- geração de token;
- erro inesperado no login.

### Tutors

- listagem de tutores;
- busca por tutor;
- tutor criado;
- tutor atualizado;
- tutor removido;
- tutor não encontrado;
- documento duplicado;
- conflito ao remover tutor;
- erro inesperado.

### Segurança

- acesso sem token;
- token inválido;
- tentativa de acesso não autorizada.

## 7. Regras básicas de composição dos logs

As regras abaixo devem orientar a escrita de logs no TOGO:

1. Log deve explicar o evento.
2. Log deve ter nível adequado.
3. Log deve usar placeholders estruturados.
4. Log não deve concatenar string manualmente.
5. Log não deve expor senha.
6. Log não deve expor token JWT completo.
7. Log não deve expor CPF/documento completo.
8. Log não deve expor payload completo sem necessidade.
9. Log de erro deve incluir exception.
10. Log deve ser útil para diagnóstico.

Exemplo bom:

```csharp
_logger.LogInformation("Tutor criado com sucesso. TutorId: {TutorId}", tutor.Id);
```

Exemplo ruim:

```csharp
_logger.LogInformation("deu certo");
```

Exemplo perigoso:

```csharp
_logger.LogInformation("Login com senha {Password}", password);
```

A regra geral é registrar contexto suficiente para diagnóstico, sem transformar o log em vazamento de informação sensível.

## 8. Níveis de log recomendados

### Information

Use `Information` para eventos normais importantes:

- tutor criado;
- login bem-sucedido;
- operação concluída.

Esse nível deve representar acontecimentos esperados e relevantes para entender o fluxo da aplicação.

### Warning

Use `Warning` para situações esperadas, mas que merecem atenção:

- documento duplicado;
- tutor não encontrado;
- login inválido;
- tentativa sem token.

Esse nível não significa necessariamente erro técnico, mas indica um evento que pode ser importante para segurança, suporte ou análise de comportamento.

### Error

Use `Error` para falhas inesperadas:

- exceção no banco;
- erro inesperado no use case;
- falha não tratada.

Logs de erro devem incluir a exception quando disponível, preservando detalhes técnicos para diagnóstico sem expor dados sensíveis ao usuário final.

### Critical

Use `Critical` para falhas graves:

- API indisponível;
- falha estrutural;
- recurso crítico fora do ar.

Esse nível deve ser raro e reservado para situações que comprometem seriamente a operação do sistema.

### Debug/Trace

Use `Debug` e `Trace` para detalhes técnicos temporários ou muito detalhados, preferencialmente em desenvolvimento.

Esses níveis devem ser usados com cautela em produção para evitar excesso de ruído, custo de armazenamento e risco de exposição de informações internas.

## 9. Cuidados com dados sensíveis

O TOGO lida com dados pessoais e pode evoluir para lidar com informações clínicas veterinárias. Por isso, observabilidade precisa ser tratada também como uma preocupação de segurança e privacidade.

Regras mínimas:

- não logar senha;
- não logar token JWT;
- não logar documento completo;
- não logar dados clínicos completos;
- preferir IDs internos;
- quando necessário, mascarar dados.

Exemplos de cuidado:

- CPF/documento: registrar apenas final ou hash, se necessário.
- Token: nunca registrar completo.
- Erros: registrar exception técnica sem expor dado sensível na resposta HTTP.

O log deve ajudar a diagnosticar o problema, mas não deve se tornar uma nova superfície de exposição de dados.

## 10. Fase 2.2 — Tratamento global de exceções inesperadas

A Fase 2.2 define o padrão planejado para tratamento global de exceções inesperadas na API do TOGO. Esta seção é documental e não implementa middleware, não altera `Program.cs` e não modifica fluxos de aplicação existentes.

### 10.1 Objetivo

O objetivo do tratamento global de exceções é criar um ponto central da API para lidar com falhas técnicas não previstas no fluxo normal da aplicação.

Esse tratamento deverá:

- capturar erros inesperados em um ponto central da API;
- registrar o erro técnico com `ILogger`;
- retornar uma resposta HTTP 500 segura e padronizada;
- evitar exposição de stack trace;
- evitar vazamento de detalhes internos;
- melhorar o diagnóstico e a observabilidade em desenvolvimento e produção.

O tratamento global deve atuar como uma proteção técnica para exceções não tratadas, sem substituir os mecanismos já existentes para erros conhecidos de negócio ou aplicação.

### 10.2 O que este tratamento resolve

Um middleware global de exceções ajuda a reduzir problemas comuns em APIs quando ocorre uma falha inesperada, como:

- respostas inconsistentes para erros inesperados;
- repetição de blocos `try/catch` em controllers;
- exposição acidental de detalhes internos da aplicação;
- falta de log centralizado para exceções não tratadas;
- dificuldade de investigar falhas em produção ou desenvolvimento.

Esse padrão não elimina a necessidade de boas validações, tratamento correto de regras de negócio e testes. Ele apenas centraliza o comportamento para falhas técnicas que escaparem do fluxo esperado.

### 10.3 O que este tratamento não deve fazer

O tratamento global de exceções inesperadas não deve ser usado como substituto para regras de negócio, validações ou resultados esperados da aplicação.

Portanto, ele não deve:

- substituir o uso de `ApplicationResult`;
- transformar erro de negócio em exception;
- alterar status codes de erros esperados;
- capturar validações comuns como se fossem falhas técnicas;
- expor stack trace na resposta HTTP;
- retornar a mensagem técnica da exception diretamente ao cliente;
- criar exceptions customizadas nesta fase.

Erros esperados devem continuar sendo tratados explicitamente pelos fluxos atuais da aplicação. O middleware global deve tratar somente exceções inesperadas.

### 10.4 Separação entre erro esperado e erro inesperado

O TOGO já utiliza `ApplicationResult` para representar erros esperados de negócio ou aplicação. Esse padrão deve continuar sendo usado para validações, conflitos, recursos não encontrados, autenticação inválida e demais regras previstas.

Exemplos de erros esperados que continuam com `ApplicationResult`:

| Situação | Resposta esperada |
| --- | --- |
| Tutor não encontrado | 404 |
| Nome obrigatório | 400 |
| Documento duplicado | 409 |
| Login inválido | 401 |

Exemplos de erros inesperados que serão tratados pelo middleware global na implementação futura:

| Situação | Resposta esperada |
| --- | --- |
| `NullReferenceException` | 500 |
| Falha inesperada no banco | 500 |
| Exception não tratada | 500 |

A diferença principal é que erros esperados fazem parte do fluxo conhecido da aplicação, enquanto erros inesperados indicam falhas técnicas não previstas que precisam ser registradas para diagnóstico.

### 10.5 Resposta padronizada planejada

A resposta planejada para exceções inesperadas deve ser simples, segura e consistente.

Exemplo de resposta HTTP 500:

```json
{
  "message": "An unexpected error occurred.",
  "traceId": "..."
}
```

Regras para a resposta:

- `message` deve ser uma mensagem genérica e segura;
- `traceId` deve ajudar a correlacionar a resposta recebida pelo cliente com os logs do servidor;
- detalhes técnicos devem ficar apenas no log;
- stack trace não deve ser exposto;
- `exception.Message` não deve ser retornada diretamente ao cliente.

Esse formato busca apoiar o diagnóstico sem revelar detalhes internos da aplicação, infraestrutura, banco de dados ou regras internas.

### 10.6 Logging esperado

Na implementação futura, o middleware deverá usar `ILogger<GlobalExceptionHandlingMiddleware>` para registrar exceções inesperadas no servidor.

Exemplo conceitual de log esperado:

```csharp
_logger.LogError(
    exception,
    "Unhandled exception occurred. TraceId: {TraceId}",
    traceId);
```

Regras para o logging do middleware:

- logar a exception técnica no servidor;
- incluir o `traceId` para correlação;
- não logar payload completo;
- não logar senha;
- não logar token JWT;
- não logar documento/CPF;
- não retornar `exception.Message` diretamente ao cliente.

O log deve conter informação técnica suficiente para diagnóstico interno, mas sem ampliar a superfície de exposição de dados sensíveis.

### 10.7 Local planejado na arquitetura

O middleware global deverá ficar na camada de API, pois sua responsabilidade estará ligada ao pipeline HTTP do ASP.NET Core.

Local planejado:

```text
backend/src/Togo.Api
```

Estrutura planejada:

```text
backend/src/Togo.Api/
  Middlewares/
    GlobalExceptionHandlingMiddleware.cs
```

Caso seja necessário criar um modelo específico para a resposta de erro, a estrutura planejada poderá ser:

```text
backend/src/Togo.Api/
  Models/
    ErrorResponse.cs
```

Nesta Fase 2.2.1, esses arquivos não devem ser criados. O objetivo é apenas documentar o padrão planejado.

### 10.8 Ordem planejada no pipeline HTTP

Na futura implementação, o middleware deverá ser registrado no `Program.cs` em posição inicial do pipeline HTTP, antes dos endpoints/controllers, para capturar exceções não tratadas pelos componentes seguintes.

O registro efetivo foi realizado na Fase 2.2.3 logo após `builder.Build()`, mantendo os componentes seguintes do pipeline protegidos pelo tratamento global de exceções inesperadas.

### 10.9 Plano de implementação da Fase 2.2

#### Fase 2.2.1 — Documentar padrão de erro global

- Documentar objetivo.
- Documentar diferença entre erro esperado e inesperado.
- Documentar resposta padronizada.
- Documentar regras de segurança.

#### Fase 2.2.2 — Criar GlobalExceptionHandlingMiddleware

- Criar middleware na camada API.
- Capturar exceptions inesperadas.
- Logar com `ILogger`.
- Retornar 500 padronizado.
- Não expor stack trace.

#### Fase 2.2.3 — Registrar middleware no Program.cs

- Registrar middleware no pipeline.
- Validar ordem de execução.
- Garantir build.

#### Fase 2.2.4 — Testar erro inesperado e revisar logs

- Validar comportamento em erro inesperado.
- Confirmar resposta 500 segura.
- Confirmar log com exception.
- Confirmar ausência de dados sensíveis.
- Não criar endpoint fake de produção sem planejamento.

### 10.10 Critérios para avançar da Fase 2.2.1

A Fase 2.2.1 estará concluída quando:

- `docs/OBSERVABILITY_STRATEGY.md` explicar o tratamento global de exceções;
- estiver claro que `ApplicationResult` continua para erros esperados;
- estiver claro que o middleware trata somente erros inesperados;
- estiver definido o formato planejado da resposta 500;
- estiver definido o uso de `traceId`;
- estiver definido que stack trace não deve ser exposto;
- estiver definido que não haverá exceptions customizadas nesta fase;
- nenhuma alteração de código tiver sido feita.

### 10.11 Fase 2.2.2 — Criar GlobalExceptionHandlingMiddleware

- Middleware criado na camada API.
- Captura exceções inesperadas que escaparem do fluxo normal.
- Usa `ILogger` para `LogError` com exception e `traceId`.
- Retorna resposta 500 segura com `message` genérica e `traceId`.
- Não expõe stack trace nem `exception.Message` na resposta HTTP.
- Durante a Fase 2.2.2, ainda não havia sido registrado no `Program.cs`; o registro foi realizado na Fase 2.2.3.
- `ApplicationResult` continua responsável por erros esperados.

### 10.12 Próxima tarefa planejada

A próxima tarefa planejada será:

**Fase 2.2.4 — Testar erro inesperado e revisar logs**

Escopo previsto:

- testar erro inesperado em ambiente controlado;
- revisar logs gerados pelo middleware;
- confirmar resposta 500 segura;
- confirmar ausência de dados sensíveis;
- garantir build.

### 10.13 Fase 2.2.3 — Registro do middleware no Program.cs

- `GlobalExceptionHandlingMiddleware` foi registrado no pipeline HTTP.
- O registro foi feito logo após `app.Build()`.
- O objetivo é capturar exceções inesperadas dos componentes seguintes do pipeline.
- `ApplicationResult` continua responsável por erros esperados.
- Controllers, use cases e repositories não foram alterados.
- Nenhum endpoint fake de erro foi criado.
- A próxima etapa será Fase 2.2.4 — Testar erro inesperado e revisar logs.

## Conclusão da Fase 2.2 — Tratamento global de exceções inesperadas

A Fase 2.2 foi concluída com o objetivo de centralizar o tratamento de exceções inesperadas da API do TOGO, melhorar a observabilidade técnica e preservar respostas HTTP seguras para falhas não previstas.

O `GlobalExceptionHandlingMiddleware` foi implementado e registrado no pipeline HTTP para capturar exceções inesperadas, registrar o erro técnico com `ILogger.LogError` e retornar uma resposta 500 padronizada com `message` genérica e `traceId`. A resposta 500 permanece segura: não expõe `exception.Message`, stack trace, inner exception, tipo da exception, senha, hash, token JWT, documento completo, payloads internos ou outros dados sensíveis ao cliente.

A separação entre erros esperados e inesperados foi preservada. Erros esperados continuam sendo tratados pelos fluxos de aplicação com `ApplicationResult`, mantendo seus status codes próprios, como 401, 404 e 409. Esses erros esperados não foram transformados em respostas 500, e o middleware global permanece reservado para exceções inesperadas que escapem do fluxo normal da aplicação.

A validação automatizada da Fase 2.2.4.2 foi concluída com testes em memória para o middleware global. O cenário de exceção inesperada validou status 500, JSON seguro, mensagem genérica, `traceId`, ausência de detalhes técnicos na resposta e registro de log em nível `Error` com exception. O caminho feliz validou a preservação do status 204 e a ausência de logs de erro.

A validação manual da Fase 2.2.4.3 também foi concluída, confirmando que os fluxos esperados de autenticação e tutores continuam funcionando sem serem convertidos em erro 500. Nenhum endpoint fake foi criado para esta validação.

Também foram executados localmente:

- `dotnet build backend/Togo.sln`: concluído com sucesso.
- `dotnet test backend/Togo.sln`: concluído com sucesso.

Resultado dos testes automatizados:

- Total de testes: 17.
- Testes passando: 17.
- Testes falhando: 0.
- Testes ignorados: 0.

Permanece registrado como débito técnico futuro um warning relacionado a conflito de versões do `Microsoft.EntityFrameworkCore.Relational` entre `8.0.13` e `8.0.22`, associado ao ecossistema EF Core/Pomelo. Esse warning não bloqueou build nem testes e não faz parte da correção da Fase 2.2.

Checklist final da validação:

| Validação | Resultado |
|---|---|
| Login válido | 200 OK |
| Login inválido | 401 Unauthorized |
| GET tutors sem token | 401 Unauthorized |
| GET tutors com token | 200 OK |
| GET tutor inexistente | 404 Not Found |
| POST tutor válido | 201 Created |
| POST tutor duplicado | 409 Conflict |
| DELETE tutor inexistente | 404 Not Found |
| Build local | Sucesso |
| Testes automatizados | 17/17 passando |

Com isso, a Fase 2.2 — Tratamento global de exceções inesperadas fica documentada como concluída.

## 11. Roadmap de implementação

### Fase 2.0 — Documentação da estratégia de observabilidade

- Criar este documento.
- Definir conceitos.
- Definir limites.
- Definir próximos passos.

### Fase 2.1 — Logging básico com ILogger

- Adicionar logs em AuthController.
- Adicionar logs em TutorsController ou use cases de Tutor.
- Adicionar logs em TutorDocumentUniquenessValidator.
- Validar build.
- Validar que comportamento da API não mudou.

### Fase 2.2 — Tratamento global de exceções inesperadas

- Criar middleware de exceções.
- Logar erros inesperados.
- Padronizar resposta de erro.
- Evitar stack trace na resposta.

### Fase 2.3 — Correlation ID

- Adicionar identificador por requisição.
- Usar header `X-Correlation-Id`.
- Incluir correlation id nos logs.

### Fase 2.4 — Serilog

- Avaliar adoção.
- Configurar logs estruturados.
- Avaliar sink para console/arquivo.

### Fase 2.5 — Métricas e traces

- Avaliar OpenTelemetry.
- Avaliar exportadores.
- Avaliar dashboards.

### Fase 2.6 — Monitoramento e alertas

- Definir ambiente alvo.
- Definir métricas críticas.
- Definir alertas mínimos.

## 12. Relação entre observabilidade e testes

Testes e observabilidade têm papéis diferentes e complementares.

- Testes validam se o comportamento esperado está correto.
- Observabilidade ajuda a entender o sistema em execução.
- Testes respondem: “o código deveria funcionar?”
- Observabilidade responde: “o sistema está funcionando agora e, se falhou, onde e por quê?”

Observabilidade não substitui testes, e testes não substituem observabilidade. O TOGO deve manter os testes como mecanismo de validação do comportamento esperado e usar observabilidade para melhorar diagnóstico, operação e suporte em tempo de execução.

## 13. Critérios para avançar da Fase 2.0

A Fase 2.0 será considerada concluída quando:

- `docs/OBSERVABILITY_STRATEGY.md` existir;
- o documento explicar os pilares da observabilidade;
- o documento definir `ILogger` como primeiro passo;
- o documento definir Serilog, OpenTelemetry e Application Insights como fases futuras;
- o documento estabelecer regras de segurança para logs;
- o documento definir roadmap incremental;
- nenhuma alteração de código tiver sido feita.

## 14. Próxima tarefa planejada

A próxima tarefa planejada será:

**Fase 2.2.4 — Testar erro inesperado e revisar logs**

Escopo previsto:

- testar erro inesperado em ambiente controlado;
- revisar logs gerados pelo middleware;
- confirmar resposta 500 segura para exceções inesperadas;
- confirmar ausência de dados sensíveis;
- garantir build.

Critérios esperados para essa próxima fase:

- comportamento de erro inesperado validado sem alterar regras de negócio;
- `ApplicationResult` preservado para erros esperados;
- ausência de dados sensíveis na resposta 500;
- logs revisados;
- build validado.

## Status de implementação

### Fase 2.1.1 — Logging básico no Auth/Login

- `ILogger<AuthController>` adicionado.
- Logs de tentativa, sucesso e falha de login.
- Nenhum dado sensível deve ser registrado.
- Serilog/OpenTelemetry ainda não foram adotados.

### Fase 2.1.2 — Logging básico no fluxo Tutor

- `ILogger` adicionado ao fluxo Tutor.
- Logs estruturados adicionados em controller, use cases e validator.
- Eventos de listagem, busca, criação, atualização, remoção, conflito e not found passaram a ser observáveis.
- Documento/CPF completo não deve ser registrado.
- Serilog/OpenTelemetry ainda não foram adotados.

### Fase 2.1.3 — Revisão dos logs em regras/validators de Tutor

- Logs do validator de unicidade de documento e dos use cases de criação/atualização de Tutor foram revisados.
- Confirmado que documento/CPF completo ou parcial não é registrado.
- Confirmado que payload completo não é registrado.
- Confirmado que os logs usam placeholders estruturados e não concatenação manual.
- O log de sucesso do validator foi ajustado de `Information` para `Debug` para reduzir ruído, mantendo `Warning` para falhas de regra e `Information` para eventos de operação dos use cases.
- Serilog/OpenTelemetry ainda não foram adotados.
