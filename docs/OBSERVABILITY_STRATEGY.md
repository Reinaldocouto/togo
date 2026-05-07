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

## 10. Roadmap de implementação

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

### Fase 2.2 — Tratamento global de exceções

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

## 11. Relação entre observabilidade e testes

Testes e observabilidade têm papéis diferentes e complementares.

- Testes validam se o comportamento esperado está correto.
- Observabilidade ajuda a entender o sistema em execução.
- Testes respondem: “o código deveria funcionar?”
- Observabilidade responde: “o sistema está funcionando agora e, se falhou, onde e por quê?”

Observabilidade não substitui testes, e testes não substituem observabilidade. O TOGO deve manter os testes como mecanismo de validação do comportamento esperado e usar observabilidade para melhorar diagnóstico, operação e suporte em tempo de execução.

## 12. Critérios para avançar da Fase 2.0

A Fase 2.0 será considerada concluída quando:

- `docs/OBSERVABILITY_STRATEGY.md` existir;
- o documento explicar os pilares da observabilidade;
- o documento definir `ILogger` como primeiro passo;
- o documento definir Serilog, OpenTelemetry e Application Insights como fases futuras;
- o documento estabelecer regras de segurança para logs;
- o documento definir roadmap incremental;
- nenhuma alteração de código tiver sido feita.

## 13. Próxima tarefa planejada

A próxima tarefa planejada será:

**Fase 2.1 — Logging básico com ILogger**

Escopo previsto:

- adicionar `ILogger` em pontos controlados;
- começar por Auth/Login e Tutor;
- não alterar comportamento da API;
- não instalar Serilog ainda;
- não implementar OpenTelemetry ainda.

Critérios esperados para essa próxima fase:

- logs úteis e estruturados nos fluxos priorizados;
- ausência de dados sensíveis nos logs;
- build validado;
- comportamento funcional da API preservado.
