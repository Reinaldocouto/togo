# TOGO — Fase 4.1.3: Modelo mínimo de Atendimento

## 1. Objetivo

Esta fase define o modelo mínimo de Atendimento antes de qualquer implementação da vertical nas camadas Application/API/Tests.

Também fica explícito que esta fase é exclusivamente documental e não implementa código.

## 2. Contexto

A Fase 4.1.1 identificou que já existe estrutura parcial de `Attendance` no projeto.

A Fase 4.1.2 confirmou a nomenclatura técnica em inglês (`Attendance`) para código e API, mantendo `Atendimento` na documentação de negócio.

Agora, a Fase 4.1.3 define o escopo mínimo do modelo de Atendimento para iniciar a vertical com baixo acoplamento.

Neste contexto, Atendimento é tratado como episódio clínico/operacional vinculado a um `Patient/Pet`.

## 3. Estado atual da entidade Attendance

Com base na inspeção do estado atual, a entidade `Attendance` já existe e contém os seguintes campos:

- `Id`;
- `PatientId`;
- `AttendanceNumber`;
- `OpenedAt`;
- `ClosedAt`;
- `Status`;
- `Type`.

Também já existem comportamentos de domínio mínimos:

- factory `Create`;
- método `Close`;
- validações básicas de id, texto obrigatório e data.

## 4. Decisão sobre modelo mínimo

Fica registrado oficialmente que, para a primeira versão da vertical de Atendimento, o modelo mínimo será baseado nos campos já existentes:

- `Id`;
- `PatientId`;
- `AttendanceNumber`;
- `OpenedAt`;
- `ClosedAt`;
- `Status`;
- `Type`.

Papel de cada campo no modelo mínimo:

- `Id`: identificador único técnico do atendimento.
- `PatientId`: identifica o paciente/pet atendido.
- `AttendanceNumber`: número único do atendimento.
- `OpenedAt`: data/hora de abertura do atendimento.
- `ClosedAt`: data/hora de encerramento; opcional enquanto o atendimento estiver aberto.
- `Status`: estado atual do atendimento.
- `Type`: tipo do atendimento.

## 5. Relacionamento com Patient/Pet

`Attendance` se relaciona diretamente com `Patient` por `PatientId`.

No domínio veterinário do TOGO, `Patient` representa o pet/paciente clínico.

Nesta fase, não será criado relacionamento direto com Tutor, Financeiro ou Usuário/Veterinário.

Esses vínculos podem ser avaliados futuramente conforme necessidade real dos fluxos clínicos e operacionais.

## 6. Ciclo de vida inicial

Ciclo de vida mínimo definido para o Atendimento:

- `Open`;
- `Closed`;
- `Canceled`.

Interpretação inicial de cada status:

- `Open`: atendimento criado e em andamento.
- `Closed`: atendimento finalizado.
- `Canceled`: atendimento cancelado antes da conclusão.

Transições mais complexas ficam para avaliação futura.

Exemplos fora do escopo imediato:

- `Scheduled`;
- `InProgress`;
- `WaitingPayment`;
- `NoShow`;
- `Reopened`.

## 7. Tipos de atendimento

Tipos já existentes considerados suficientes para o primeiro modelo mínimo:

- `Consultation`;
- `Emergency`;
- `Return`;
- `Procedure`;
- `Exam`;
- `Other`.

Especializações adicionais podem ser avaliadas em fases futuras, mas não entram agora.

## 8. O que NÃO entra no modelo mínimo

Não entram nesta fase:

- anamnese;
- evolução clínica;
- prescrição;
- exames detalhados;
- diagnóstico;
- conduta clínica detalhada;
- cobrança financeira;
- itens de venda;
- pagamento;
- fila/mensageria;
- evento RabbitMQ;
- cache Redis;
- upload de anexos;
- assinatura digital;
- vínculo obrigatório com profissional/veterinário;
- vínculo obrigatório com sala/unidade;
- dashboard;
- agendamento complexo.

Esses temas pertencem a fases futuras como Prontuário, Evolução Clínica, Prescrição, Financeiro ou Infraestrutura.

## 9. Decisão sobre Prontuário

Atendimento não deve ser confundido com Prontuário.

Nesta definição:

- Atendimento representa um episódio/visita.
- Prontuário (`MedicalRecord`) representa histórico longitudinal do paciente.
- Evoluções clínicas e prescrições podem se relacionar ao Atendimento, mas não serão implementadas nesta fase.

## 10. Decisão sobre Financeiro

Atendimento não deve acoplar Financeiro agora.

Futuramente, um atendimento fechado pode disparar fluxo financeiro.

Porém, nesta fase:

- não criar cobrança;
- não criar pagamento;
- não publicar evento;
- não criar RabbitMQ;
- não criar integração financeira.

## 11. Decisão sobre eventos e mensageria

A Fase 4.0.1 já definiu RabbitMQ como roadmap futuro.

No modelo mínimo atual, pode-se considerar que futuramente um atendimento fechado poderá gerar evento, por exemplo:

- `AttendanceClosed`;
- `AttendanceFinalized`.

Nenhum evento será implementado nesta fase.

## 12. Riscos e cuidados

Riscos principais desta etapa:

- inflar `Attendance` com campos de prontuário;
- misturar atendimento com financeiro;
- criar status demais antes de validar fluxo real;
- criar regras de transição complexas cedo demais;
- duplicar dados de `Patient/Pet`;
- ignorar o padrão já usado em `Pet/Patient`;
- alterar entidade/migration antes de consolidar decisão documental.

## 13. Decisões finais

Decisões finais desta fase:

- manter `Attendance` como agregado inicial de Atendimento;
- manter o modelo mínimo baseado nos campos já existentes;
- manter vínculo direto com `Patient`;
- não incluir Prontuário dentro de `Attendance`;
- não incluir Financeiro dentro de `Attendance`;
- não incluir RabbitMQ/Redis/Docker/Kubernetes agora;
- próxima implementação deve começar pela revisão/criação de testes de domínio ou pela documentação de relacionamento com `Patient/Pet`, conforme roadmap.

## 14. Próxima fase recomendada

**Fase 4.1.4 — Definir relacionamento com Patient/Pet.**

Objetivo:
Documentar com mais precisão como `Attendance` se relaciona com `Patient/Pet`, quais regras mínimas devem existir para criação de atendimento e quais validações serão necessárias antes de implementar repository, use cases e controller.

## 15. Fora do escopo

Esta fase não implementa:

- alteração da entidade `Attendance`;
- alteração dos enums;
- alteração do EF Core;
- alteração do `AppDbContext`;
- alteração de migration;
- alteração de banco;
- repository;
- use cases;
- contracts;
- validators;
- controller;
- testes;
- `Program.cs`;
- `appsettings`;
- workflow;
- Docker;
- Redis;
- RabbitMQ;
- Kubernetes.
