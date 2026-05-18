# TOGO — Fase 4.0: Decisão técnica sobre Infraestrutura, Cache e Mensageria

## 1. Objetivo da decisão

Esta fase registra decisões arquiteturais antes do início da Fase 4 — Atendimento.

Docker, Redis e RabbitMQ fazem parte do roadmap técnico do TOGO, mas só devem ser aplicados quando houver encaixe técnico real com os fluxos de negócio e com a maturidade da plataforma.

Kubernetes fica fora do escopo atual e não será aplicado nesta etapa.

## 2. Contexto do projeto

O TOGO já possui:

- backend em C#/.NET;
- ASP.NET Core;
- EF Core;
- API REST;
- autenticação JWT;
- Git/GitHub;
- GitHub Actions;
- branch protection;
- testes automatizados;
- logs com ILogger;
- CRUD Pet/Patient validado na Fase 3.

Com essa base consolidada, o projeto agora avança para a Fase 4 — Atendimento.

## 3. Separação em duas trilhas

Foi definida a separação do roadmap em duas trilhas para reduzir acoplamento entre evolução clínica e evolução de infraestrutura.

### Trilha Clínica

- Fase 4 — Atendimento
- Fase 5 — Prontuário / Evolução clínica
- Fase 6 — Financeiro inicial

### Trilha Infra/DevOps

- Infra 1 — Docker
- Infra 2 — Health Check
- Infra 3 — Redis cache
- Infra 4 — RabbitMQ eventos
- Infra 5 — Kubernetes lab, futuramente

Essa separação evita misturar regra de negócio clínica com decisões de infraestrutura.

## 4. Docker

Docker será a primeira ferramenta da trilha Infra/DevOps.

Objetivo futuro:

- padronizar ambiente local;
- facilitar onboarding;
- reduzir problema de "na minha máquina funciona";
- permitir subir API + banco + serviços auxiliares de forma previsível.

Cenário futuro esperado:

- API .NET
- MySQL
- Redis
- RabbitMQ

Via:

- Dockerfile
- docker-compose.yml

Docker não será implementado agora na Fase 4.0.1.

O momento ideal para adoção é antes ou durante a preparação de ambiente integrado, Health Check ou testes de integração.

## 5. Redis

Redis será usado futuramente para cache de leitura.

Cenário inicial preferido:

- cache do endpoint GET /api/pets.

Cenários futuros possíveis:

- GET /api/pets/{patientId}
- GET /api/attendances
- GET /api/attendances/{id}
- dashboards clínicos/operacionais

Estratégia futura:

- cache-aside;
- TTL curto;
- invalidação após POST/PUT/DELETE;
- não cachear dados sensíveis sem critério.

Exemplo conceitual:

- GET /api/pets consulta cache;
- se não existir, consulta banco;
- salva no cache por tempo controlado;
- Create/Update/Delete limpa cache relacionado.

Redis não será implementado agora na Fase 4.0.1.

Redis só deve entrar quando os endpoints estiverem estáveis.

## 6. RabbitMQ

RabbitMQ será usado futuramente para mensageria/eventos entre módulos.

Cenário principal:

- Atendimento conversar com Financeiro.

Exemplo futuro:

- Atendimento finalizado publica evento AttendanceClosed ou AttendanceFinalized;
- Financeiro consome o evento;
- Financeiro cria cobrança pendente ou movimento financeiro.

RabbitMQ não deve entrar no início da Fase 4, porque ainda não existe Attendance consolidado nem CloseAttendanceUseCase/FinalizeAttendanceUseCase.

RabbitMQ só deve ser aplicado quando existir pelo menos:

- Attendance;
- CloseAttendanceUseCase ou FinalizeAttendanceUseCase;
- algum fluxo financeiro inicial para consumir evento.

A abordagem mais próxima de produção deve considerar futuramente o padrão Outbox para evitar o problema:

- banco salvou;
- publicação no RabbitMQ falhou.

No primeiro momento, pode haver uma implementação didática/simples, mas o caminho mais robusto deve evoluir para Outbox.

## 7. Kubernetes

Kubernetes não será aplicado agora.

Conceitualmente:

- Docker empacota e executa containers.
- Kubernetes orquestra containers.

Kubernetes fica como laboratório futuro, somente depois de:

- Docker funcionando;
- Health Check implementado;
- API estável;
- configuração por ambiente;
- logs mais maduros.

Kubernetes sai do escopo imediato.

## 8. Impacto na Fase 4 — Atendimento

A Fase 4 deve continuar focada em Atendimento.

A Fase 4 não deve começar já com Docker, Redis ou RabbitMQ.

Atendimento deve nascer limpo, seguindo o padrão usado em Pet/Patient:

- modelo;
- contrato;
- repository;
- validator;
- use case;
- controller;
- testes;
- validação;
- documentação.

A Fase 4 deve preparar terreno para eventos futuros, mas não publicar eventos ainda no começo.

Exemplo:

- ao pensar no Atendimento, considerar que futuramente um atendimento finalizado poderá disparar evento financeiro.

Mas sem implementar RabbitMQ agora.

## 9. Impacto futuro em Financeiro e Prontuário

- Prontuário dependerá de Atendimento bem modelado;
- Financeiro poderá ser acionado futuramente por eventos de Atendimento;
- RabbitMQ será útil quando houver comunicação assíncrona real;
- Redis poderá ajudar leituras frequentes;
- Docker facilitará ambiente com múltiplos serviços.

## 10. O que fica fora do escopo imediato

Está fora do escopo da Fase 4.0.1:

- implementação de Dockerfile;
- implementação de docker-compose.yml;
- configuração de Redis;
- configuração de RabbitMQ;
- configuração de Kubernetes;
- alteração de Program.cs;
- alteração de appsettings;
- instalação de pacotes;
- criação de serviços de cache;
- criação de produtores/consumidores de fila;
- criação de workers;
- criação de Health Check;
- alteração do CI;
- alteração de banco;
- migrations;
- database update;
- qualquer código de produção;
- qualquer teste.

## 11. Roadmap recomendado

1. Fase 4.0 — Decisão técnica de Infraestrutura, Cache e Mensageria.
2. Fase 4.1 — Revisar e documentar Atendimento.
3. Fase 4.x — Implementar Atendimento.
4. Infra 1 — Docker.
5. Infra 2 — Health Check.
6. Infra 3 — Redis cache.
7. Fase 6 — Financeiro inicial.
8. Infra 4 — RabbitMQ eventos entre Atendimento e Financeiro.
9. Infra 5 — Kubernetes lab futuramente.

## 12. Decisão final

- Docker: aprovado para roadmap, mas não agora.
- Redis: aprovado para cache de leitura, começando por GET /api/pets futuramente.
- RabbitMQ: aprovado para eventos entre Atendimento e Financeiro, somente quando houver Attendance/Close ou Finalize use case.
- Kubernetes: adiado para laboratório futuro.

## 13. Próxima fase recomendada

Fase 4.1 — Revisar e documentar Atendimento.

Objetivo:

Inspecionar o estado atual do projeto, verificar se já existe alguma entidade ou documento relacionado a Atendimento/Attendance e definir o modelo inicial antes de qualquer implementação.
