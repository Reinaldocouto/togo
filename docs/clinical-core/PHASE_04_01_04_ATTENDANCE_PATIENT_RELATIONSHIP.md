# TOGO — Fase 4.1.4: Relacionamento entre Attendance e Patient/Pet

## 1. Objetivo

Esta fase define o relacionamento entre Atendimento e Patient/Pet antes da implementação da vertical nas camadas Application/API/Tests.

Também fica explícito que esta fase é exclusivamente documental e não implementa código.

## 2. Contexto

- A Fase 4.1.1 identificou estrutura parcial de Attendance já existente no domínio e na infraestrutura.
- A Fase 4.1.2 confirmou a nomenclatura técnica para código/API (`Attendance` / `attendances`) e nomenclatura de negócio em português (`Atendimento`).
- A Fase 4.1.3 definiu o modelo mínimo com os campos `Id`, `PatientId`, `AttendanceNumber`, `OpenedAt`, `ClosedAt`, `Status` e `Type`.
- Agora, a Fase 4.1.4 define as regras de vínculo entre Attendance e Patient/Pet antes de criar repository, use cases, controller e testes da vertical.

## 3. Conceito de Patient/Pet no TOGO

Com base no estado atual do projeto:

- `Patient` é a entidade clínica base com identidade própria (`Id`) e dados gerais de paciente (tipo, nome, status e datas de controle).
- `Pet` é uma entidade especializada que usa `PatientId` como chave primária e se relaciona 1:1 com `Patient`, concentrando atributos veterinários do animal (espécie, raça, sexo, peso, microchip) e referência de `TutorId`.

No domínio veterinário do TOGO, o entendimento operacional fica:

- `Pet` representa o animal;
- `Patient` representa o paciente clínico;
- `Attendance` deve se vincular ao `Patient` por `PatientId`.

Assim, o atendimento referencia o paciente clínico e os dados do animal são acessados indiretamente via relação `Patient -> Pet`.

## 4. Relacionamento principal

Fica registrado oficialmente:

- Attendance pertence a um Patient.
- Um Patient pode ter múltiplos Attendances.
- Attendance deve possuir `PatientId` obrigatório.
- Attendance não deve duplicar dados de Patient/Pet.
- Dados de Patient/Pet devem ser consultados por relacionamento/projeção, não copiados para Attendance.

## 5. Cardinalidade

Cardinalidade esperada:

- Patient 1:N Attendance.
- Attendance N:1 Patient.

Essa cardinalidade representa o histórico de atendimentos do paciente ao longo do tempo, permitindo múltiplos episódios clínicos para o mesmo paciente.

## 6. Regras mínimas para criação de Attendance

Regras mínimas esperadas para criação:

- `PatientId` deve ser obrigatório.
- `PatientId` deve ser maior que zero.
- `Patient` deve existir.
- `AttendanceNumber` deve ser obrigatório e único.
- `OpenedAt` deve ser obrigatório.
- `Status` inicial preferencial deve ser `Open`.
- `Type` deve ser obrigatório.
- `ClosedAt` deve ser nulo na criação de um atendimento aberto.

Decisão sobre criação já fechada:

- **Não permitir como fluxo padrão inicial** criar atendimento já fechado.
- Fluxo inicial recomendado: criar como `Open` e encerrar posteriormente por operação explícita de fechamento.
- Exceções retroativas/migração de legado, se necessárias no futuro, devem ser tratadas por fluxo administrativo específico, fora da primeira versão.

## 7. Regras sobre Patient inexistente ou inválido

A camada Application deve validar a existência do `Patient` antes de criar `Attendance`.

Essa regra não deve depender apenas da FK do banco, porque:

- melhora a experiência da API com erro de negócio claro;
- evita retorno de erro genérico de persistência;
- facilita rastreabilidade de regra funcional no use case/validator.

Resultado esperado para API: erro explícito de validação/domínio para `Patient` inexistente ou inválido.

## 8. Regra sobre múltiplos atendimentos abertos

Opções avaliadas:

- Opção A: permitir múltiplos atendimentos abertos por `Patient`.
- Opção B: bloquear mais de um atendimento `Open` por `Patient`.
- Opção C: não bloquear agora e postergar decisão.

Decisão inicial conservadora:

- **Adotar a Opção B como regra de negócio alvo**: bloquear mais de um `Attendance` em status `Open` para o mesmo `Patient`.

Justificativa:

- reduz ambiguidade clínica/operacional sobre qual episódio está em andamento;
- simplifica fechamento, evolução e consistência da jornada do atendimento;
- evita crescimento de dívida de reconciliação entre atendimentos concorrentes.

Observação de implementação:

- caso a vertical ainda não tenha suporte imediato, a regra deve ficar registrada para implementação em validator/use case (`OpenAttendanceValidator`) na fase de codificação.

## 9. Relacionamento com Tutor

Nesta fase, `Attendance` não se relaciona diretamente com `Tutor`.

`Tutor` pode ser acessado indiretamente pela cadeia atual do domínio (`Attendance -> Patient -> Pet -> Tutor`).

Não será criado `TutorId` em `Attendance` agora.

## 10. Relacionamento com MedicalRecord/Prontuário

`Attendance` não substitui `MedicalRecord`.

Nesta definição:

- `Attendance` representa episódio/visita;
- `MedicalRecord` representa histórico longitudinal;
- evoluções clínicas e prescrições podem referenciar `Attendance` futuramente;
- nenhuma alteração em `MedicalRecord` será feita nesta fase.

## 11. Relacionamento com Financeiro

Nesta fase, `Attendance` não terá vínculo financeiro direto.

Não haverá:

- cobrança;
- pagamento;
- item financeiro;
- evento financeiro;
- RabbitMQ.

Registro permitido para evolução futura: um `Attendance` fechado poderá originar fluxo financeiro em etapa posterior.

## 12. Validações futuras esperadas

Validadores/regras prováveis para próximas fases:

- `AttendancePatientExistsValidator`;
- `OpenAttendanceValidator`;
- `AttendanceNumberUniqueValidator`;
- `AttendanceStatusTransitionValidator`;
- `AttendanceBelongsToPatientValidator` (se necessário em operações futuras).

Esta fase apenas documenta essas necessidades; não implementa validators.

## 13. Impacto esperado nas próximas camadas

### Application

- criação de `IAttendanceRepository`;
- validação de existência do `Patient`;
- use cases de `Create/List/Get/Update/Close/Cancel`.

### Infrastructure

- implementação de `AttendanceRepository`;
- consultas por `PatientId`;
- consulta de atendimento aberto por `PatientId`;
- verificação de `AttendanceNumber` único.

### API

- endpoint de criação de `Attendance` exigindo `PatientId`;
- endpoints de listagem com filtro por `PatientId` em evolução futura;
- respostas com dados mínimos de Patient/Pet por projeções futuras.

### Tests

- testes de criação com `PatientId` válido;
- testes de erro com `PatientId` inválido;
- testes de regra de atendimento aberto;
- testes de listagem por `PatientId`.

## 14. Riscos e cuidados

Riscos desta etapa:

- duplicar dados de Patient/Pet em `Attendance`;
- criar dependência direta com `Tutor` antes da hora;
- deixar somente o banco validar `Patient` inexistente;
- permitir atendimentos abertos ilimitados sem decisão;
- acoplar Atendimento com Prontuário cedo demais;
- acoplar Atendimento com Financeiro cedo demais;
- criar projection grande demais na primeira versão.

## 15. Decisões finais

Decisões finais desta fase:

- `Attendance` se relaciona diretamente com `Patient` por `PatientId`.
- `Patient` pode ter múltiplos `Attendances`.
- `Attendance` não duplica dados de Patient/Pet.
- `Attendance` não terá `TutorId`.
- `Attendance` não terá vínculo financeiro direto.
- Application deve validar existência de `Patient`.
- Regra de atendimento aberto por `Patient` fica definida como decisão inicial (bloqueio de múltiplos `Open` por paciente).
- Próximas implementações devem respeitar essas regras.

## 16. Próxima fase recomendada

**Fase 4.1.5 — Documentar decisões e riscos antes de codar.**

Objetivo:
Consolidar as decisões da Fase 4.1 antes de iniciar implementação da vertical Attendance nas camadas Domain/Application/Infrastructure/API/Tests.

## 17. Fora do escopo

Esta fase não implementa:

- alteração da entidade `Attendance`;
- alteração da entidade `Patient`;
- alteração da entidade `Pet`;
- alteração da entidade `Tutor`;
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
