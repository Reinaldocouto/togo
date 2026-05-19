# TOGO — Fase 4.1.2: Decisão de nomenclatura técnica Attendance vs Atendimento

## 1. Objetivo

Esta fase confirma oficialmente a nomenclatura técnica que será usada na Fase 4 — Atendimento.

A decisão formaliza o padrão para evitar mistura de nomes em português e inglês no código, reduzindo ambiguidades entre domínio de negócio e implementação técnica.

## 2. Contexto

A Fase 4.1.1 identificou que já existe estrutura parcial de Atendimento no projeto com nomenclatura técnica em inglês:

- Attendance;
- AttendanceStatus;
- AttendanceType;
- AttendanceConfiguration;
- DbSet<Attendance>;
- tabela Attendances.

Também foi identificado que ainda não existe vertical completa de Attendance nas camadas Application/API/Tests, incluindo repository, use cases, contracts, validators, controller e testes específicos.

## 3. Decisão principal

Fica registrada oficialmente a seguinte convenção de nomenclatura para a vertical de Atendimento:

- Código-fonte: usar Attendance.
- API/rotas: usar attendances.
- Controller: AttendancesController.
- Repository: IAttendanceRepository / AttendanceRepository.
- Use cases: CreateAttendanceUseCase, ListAttendancesUseCase, GetAttendanceByIdUseCase, UpdateAttendanceUseCase, CancelAttendanceUseCase ou CloseAttendanceUseCase, conforme decisão futura.
- Contracts: CreateAttendanceRequest, UpdateAttendanceRequest, AttendanceResponse, AttendanceListItemResponse.
- Testes: usar Attendance no nome das classes de teste.
- Documentação de negócio: usar Atendimento.

## 4. Justificativa

A decisão é baseada nos seguintes pontos:

- o projeto já usa nomes técnicos em inglês;
- Patient, Pet, MedicalRecord e Attendance já seguem esse padrão;
- manter inglês no código melhora consistência entre entidades, camadas e rotas;
- português será mantido na documentação para clareza de negócio com stakeholders;
- misturar Atendimento no código com Attendance em outras camadas aumentaria ambiguidade e custo de manutenção.

## 5. Glossário

| Termo de negócio | Nome técnico |
|---|---|
| Atendimento | Attendance |
| Atendimentos | Attendances |
| Status do atendimento | AttendanceStatus |
| Tipo de atendimento | AttendanceType |
| Prontuário | MedicalRecord |
| Paciente/Pet atendido | Patient/Pet |

## 6. Convenções para próximas fases

As próximas fases devem seguir as convenções abaixo:

- pasta/namespace em inglês;
- classes em inglês;
- endpoints em inglês;
- documentação explicando o equivalente em português;
- commits e PRs preferencialmente em inglês técnico ou formato já usado no projeto.

Exemplos esperados de endpoints:

- GET /api/attendances
- GET /api/attendances/{id}
- POST /api/attendances
- PUT /api/attendances/{id}
- DELETE /api/attendances/{id}

## 7. Fora do escopo

Esta fase não implementa:

- entidade;
- enum;
- repository;
- use cases;
- contracts;
- validators;
- controller;
- testes;
- migration;
- alteração de banco;
- alteração de Program.cs;
- alteração de appsettings;
- alteração de workflow;
- Docker;
- Redis;
- RabbitMQ;
- Kubernetes.

## 8. Próxima fase recomendada

Fase 4.1.3 — Definir modelo mínimo de Atendimento.

Objetivo:
Revisar a entidade Attendance existente e documentar qual será o modelo mínimo necessário para iniciar a vertical de Atendimento sem acoplar prematuramente Prontuário, Financeiro, Redis ou RabbitMQ.
