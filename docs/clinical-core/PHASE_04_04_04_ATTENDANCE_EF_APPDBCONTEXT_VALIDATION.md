# TOGO — Fase 4.4.4: Validação EF/AppDbContext de Attendance

## 1. Objetivo

Esta fase revisa e documenta a configuração EF Core/AppDbContext de `Attendance` na camada Infrastructure, verificando consistência de mapeamento, relacionamento, índices e estado de migrations, sem evolução de schema nesta etapa.

## 2. Contexto

- A Application de Attendance foi consolidada na Fase 4.3.
- O `AttendanceRepository` concreto foi implementado na Fase 4.4.1.
- Os testes de Infrastructure para AttendanceRepository foram criados na Fase 4.4.2.
- O projeto de testes foi integrado à solution na Fase 4.4.2.1.
- O DI (`IAttendanceRepository` -> `AttendanceRepository`) foi registrado na Fase 4.4.3.
- Esta fase valida o mapeamento EF antes de seguir para o fechamento da Infrastructure.

## 3. AppDbContext

Validação realizada em `AppDbContext`:

- Existe `DbSet<Attendance> Attendances`.
- Existe aplicação automática de configurations por assembly via `modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);`.

Conclusão:

- O `AppDbContext` está coerente para carregar o mapeamento de `Attendance` e manter centralização de configurações EF.

## 4. AttendanceConfiguration

Validação realizada em `AttendanceConfiguration`:

- **Tabela:** `Attendances`.
- **Chave primária:** `HasKey(a => a.Id)`.
- **Id gerado por banco:** `ValueGeneratedOnAdd()`.
- **PatientId obrigatório:** `Property(a => a.PatientId).IsRequired()`.
- **AttendanceNumber obrigatório:** `Property(a => a.AttendanceNumber).IsRequired()`.
- **Tamanho máximo AttendanceNumber:** `HasMaxLength(30)`.
- **OpenedAt obrigatório:** `Property(a => a.OpenedAt).IsRequired()`.
- **ClosedAt opcional:** `Property(a => a.ClosedAt)` (nullable no domínio e sem `IsRequired`).
- **Status obrigatório:** `Property(a => a.Status).IsRequired()`.
- **Type obrigatório:** `Property(a => a.Type).IsRequired()`.
- **Conversão de enum:** `Status` e `Type` usam `HasConversion<string>()`.
- **Relacionamento com Patient:** `HasOne<Patient>().WithMany().HasForeignKey(a => a.PatientId)`.
- **Delete behavior:** `OnDelete(DeleteBehavior.Cascade)`.
- **Índices:** `PatientId`, `AttendanceNumber` (único), `OpenedAt`.
- **Unicidade de AttendanceNumber:** garantida por `HasIndex(a => a.AttendanceNumber).IsUnique()`.

Conclusão:

- A configuração EF de `Attendance` está tecnicamente consistente com os requisitos definidos para esta fase.

## 5. Relacionamento Attendance/Patient

- `Attendance` possui `PatientId` no domínio e no mapeamento EF.
- A FK aponta para `Patient` (`FK_Attendances_Patients_PatientId` em migration existente).
- A cardinalidade implementada é compatível com decisão de negócio **Patient 1:N Attendance**.
- Não existe `TutorId` em `Attendance`.
- O acesso a tutor permanece indireto, via agregados relacionais já existentes (Patient/Pet/Tutor), quando necessário.
- O `DeleteBehavior.Cascade` remove attendances quando o paciente for removido; isso é coerente tecnicamente, porém deve ser acompanhado por regra funcional de retenção histórica.

## 6. Índices e performance

Índices atualmente configurados:

- `IX_Attendances_PatientId`.
- `IX_Attendances_AttendanceNumber` (unique).
- `IX_Attendances_OpenedAt`.

Impacto para queries atuais do repository:

- `GetByIdAsync`: atende pela PK (`Id`).
- `ExistsByAttendanceNumberAsync`: beneficia-se diretamente do índice único em `AttendanceNumber`.
- `ListByPatientIdAsync`: beneficia-se de índice em `PatientId`, porém pode exigir ordenação adicional por `OpenedAt`/`Id`.
- `HasOpenAttendanceForPatientAsync`: usa filtro por `PatientId` + `Status`; atualmente não há índice composto para esse padrão.

Recomendação futura (documental, sem implementação nesta fase):

- Avaliar índice composto `(PatientId, Status)` para otimizar `HasOpenAttendanceForPatientAsync`.
- Avaliar índice composto `(PatientId, OpenedAt)` para otimizar listagem por paciente com ordenação temporal.

## 7. Enums no banco

`AttendanceStatus` e `AttendanceType` estão mapeados com `HasConversion<string>()`.

Vantagens:

- Maior legibilidade no banco.
- Menor risco de interpretação incorreta de valores numéricos de enum após evolução de código.

Riscos/impactos:

- Maior uso de armazenamento e tamanho de índices comparado a inteiros.
- Potencial impacto de performance em cenários de alto volume (aceitável no estágio atual).

## 8. Migration

- **Migration criada nesta fase:** não.
- **Database update executado nesta fase:** não.
- Já existem migrations com `Attendance` (`20260428200839_AddClinicalCoreEntities`) e snapshot contendo tabela/colunas/índices principais.
- Com base na inspeção estática de código e migrations existentes, não há evidência de divergência que exija migration imediata.
- Limitação do ambiente: `dotnet` indisponível (`dotnet: command not found`), impossibilitando validação por EF tooling em tempo de execução.

## 9. Cobertura por testes de Infrastructure

Coberturas identificadas em `AttendanceRepositoryTests`:

- Persistência (`AddAsync`).
- Consulta por Id (`GetByIdAsync`).
- Listagem global ordenada (`ListAsync`).
- Listagem por paciente (`ListByPatientIdAsync`).
- Verificação por número (`ExistsByAttendanceNumberAsync`).
- Verificação de atendimento aberto (`HasOpenAttendanceForPatientAsync`) incluindo casos aberto/sem atendimento/fechado-cancelado.
- Atualização para estados `Closed` e `Canceled` (`UpdateAsync`).

Cobertura indireta de FK:

- Todos os cenários de criação de attendance usam `Patient` previamente persistido, evidenciando uso coerente da FK em fluxo positivo.

Lacunas:

- Não há teste explícito de violação de unicidade de `AttendanceNumber` (constraint unique no banco).
- Não há teste explícito de violação de FK (inserção com `PatientId` inexistente).
- Não há teste específico para comportamento de cascade delete em `Patient -> Attendance`.

## 10. Riscos e decisões

Riscos técnicos registrados:

- Diferenças de comportamento entre SQLite in-memory (teste) e MySQL real (produção), sobretudo em constraints, collations e planos de índice.
- `DeleteBehavior.Cascade` pode conflitar com políticas de retenção/auditoria clínica se a remoção de paciente for permitida funcionalmente.
- `AttendanceNumber` único globalmente pode ser restritivo dependendo da estratégia futura de numeração por clínica/tenant.
- Ausência atual de índices compostos pode afetar performance em crescimento de volume.
- Não houve geração de migration nesta fase; validação de drift de schema fica pendente de ambiente com SDK/EF tooling.
- Enum como string melhora legibilidade, com custo potencial de índice/armazenamento.

Decisão da fase:

- Manter configuração atual sem alteração de código, registrando recomendações para evolução futura guiada por métricas e necessidade funcional.

## 11. Fora do escopo

- Domain
- Application
- API
- Controller
- Endpoints
- Program.cs/DI
- AttendanceRepository (salvo correção real)
- testes (salvo correção real)
- migrations
- database update
- RabbitMQ
- Redis
- Docker
- Kubernetes
- Frontend

## 12. Validação

Comandos executados:

- `git branch --show-current`
- `git status`
- `git diff --check`
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Resultados observados nesta execução:

- Branch inicial: `work`.
- Branch de trabalho da fase: `docs/phase-4-4-4-attendance-ef-appdbcontext-validation`.
- `git status` inicial sem alterações pendentes.
- `git diff --check`: sem saída (sem problemas de whitespace).
- `dotnet build backend/Togo.sln`: `dotnet: command not found`.
- `dotnet test backend/Togo.sln`: `dotnet: command not found`.

## 13. Próxima fase recomendada

**Fase 4.4.5 — Documentar fechamento da Infrastructure Attendance.**

Objetivo:

Consolidar a camada Infrastructure de Attendance, documentando repository concreto, testes de Infrastructure, integração à solution, DI, configuração EF/AppDbContext, riscos e pendências antes de avançar para API.
