# Fase 7.5.1 — Planejamento técnico da integração Prescription com Attendance

## 1. Objetivo

Criar o planejamento técnico da integração segura de `Prescription` com `Attendance`, sem implementar código, testes, migrations, endpoints, contratos, autorização, autoria, AuditLog, estoque, frontend ou infraestrutura.

Esta fase é exclusivamente documental. O objetivo é registrar o estado real encontrado para `Prescription`, `PrescriptionItem`, `Attendance` e `Product`/estoque, mapear lacunas e riscos, propor decisões técnicas iniciais e recomendar a próxima subfase incremental da trilha 7.5.

## 2. Contexto

A Fase 7 é a expansão clínica e operacional pós-hardening de `MedicalRecord`. A Fase 6 consolidou `MedicalRecord` como vertical clínica mais madura, com autorização granular, autoria, AuditLog mínimo, Soft Delete, revisão de cascades, integridade física e governança documental.

A Fase 7.0 recomendou tratar `Attendance` como eixo operacional antes de evoluções clínicas e prescrições. A Fase 7.1 confirmou que `ClinicalEvolution` e `Prescription` já apontavam para `Attendance` como vínculo natural. As Fases 7.2 e 7.3 endureceram `Attendance` com autorização granular, autoria mínima e eventos de auditoria para criação, fechamento e cancelamento. A Fase 7.4 integrou `ClinicalEvolution` com `Attendance` por uma superfície mínima, sem listagem global e com minimização de dados clínicos sensíveis.

A Fase 7.5 deve seguir o mesmo padrão incremental: antes de expor uma prescrição clínica, é necessário planejar contratos, autorização, autoria, auditoria, vínculo ao episódio correto e limites explícitos com estoque, venda, financeiro, `MedicalRecord` e `ClinicalEvolution`.

## 3. Fontes consultadas

### 3.1 Documentos de contexto consultados

- `docs/clinical-core/PHASE_07_00_CLINICAL_OPERATIONAL_EXPANSION_PLANNING.md`.
- `docs/clinical-core/PHASE_07_01_ATTENDANCE_TECHNICAL_PLANNING.md`.
- `docs/clinical-core/PHASE_07_02_04_ATTENDANCE_AUTHORIZATION_CLOSURE.md`.
- `docs/clinical-core/PHASE_07_03_06_ATTENDANCE_AUTHORSHIP_AUDIT_CLOSURE.md`.
- `docs/clinical-core/PHASE_07_04_06_CLINICAL_EVOLUTION_CLOSURE.md`.
- `docs/clinical-core/PHASE_06_06_05_MEDICAL_RECORD_PHASE_06_CLOSURE.md`.

### 3.2 Arquivos técnicos consultados

- `backend/src/Togo.Domain/Entities/Prescription.cs`.
- `backend/src/Togo.Domain/Entities/PrescriptionItem.cs`.
- `backend/src/Togo.Domain/Entities/Attendance.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/PrescriptionConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/PrescriptionItemConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/AttendanceConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`.

### 3.3 Inspeções executadas

Foram executadas as buscas obrigatórias:

```bash
rg -n "class Prescription|PrescriptionItem|Prescription|AttendanceId|ProductId|Dosage|Frequency|Duration|Instructions|Quantity" backend/src docs/clinical-core
rg -n "PrescriptionConfiguration|PrescriptionItemConfiguration|HasOne|Attendance|Product|DeleteBehavior|Prescription" backend/src/Togo.Infrastructure backend/src/Togo.Domain
rg -n "Prescription" backend/src/Togo.Application backend/src/Togo.Api backend/src/*Tests || true
rg -n "Product|Stock|Inventory|Quantity|Unit|Medication|Medicine" backend/src/Togo.Domain backend/src/Togo.Application backend/src/Togo.Infrastructure docs/clinical-core || true
rg -n "CreatedByUserId|UpdatedByUserId|IClinicalAuditLogWriter|ClinicalAuditLog|CurrentUser" backend/src/Togo.Domain backend/src/Togo.Application backend/src/Togo.Infrastructure
```

## 4. Relação com Attendance

`Attendance` representa o episódio clínico/operacional. O estado atual de `Prescription` já confirma `AttendanceId` obrigatório, e a configuração EF atual vincula `Prescription` a `Attendance` com `DeleteBehavior.Restrict`.

Decisão recomendada: `Prescription` deve permanecer vinculada a `Attendance` por `AttendanceId`. A criação futura deve ocorrer em superfície orientada por atendimento, por exemplo criação para um `AttendanceId` específico e listagem por `AttendanceId`, evitando listagem global inicial.

Motivos:

- a prescrição é uma ordem/conduta emitida dentro de um episódio clínico;
- `Attendance` já aponta para `Patient`, permitindo derivar o paciente indiretamente;
- a Fase 7.4 já consolidou o padrão de superfície mínima por atendimento para `ClinicalEvolution`;
- uma listagem global de prescrições aumentaria risco de exposição ampla de dado clínico sensível.

Regras futuras a decidir em fases posteriores:

- se criação deve ser permitida apenas para `AttendanceStatus.Open`;
- se prescrição em atendimento fechado deve ser bloqueada, retificada ou criada apenas por fluxo especial;
- se atendimento cancelado deve impedir qualquer criação/alteração de prescrição;
- como tratar correções sem apagar histórico.

## 5. Relação com ClinicalEvolution

`ClinicalEvolution` e `Prescription` devem permanecer conceitos separados. A evolução clínica registra texto/evento clínico narrativo do episódio; a prescrição representa uma ordem/conduta clínica estruturada, potencialmente composta por itens.

A integração de `Prescription` não deve exigir acoplamento direto com `ClinicalEvolution` nesta fase. Não se recomenda criar vínculo obrigatório `Prescription -> ClinicalEvolution` agora, pois isso anteciparia decisões sobre conduta, retificação, assinatura, versionamento e rastreabilidade cruzada.

Decisão recomendada: manter `ClinicalEvolution` como trilha já encerrada na Fase 7.4 e iniciar `Prescription` como trilha própria, vinculada ao mesmo eixo `Attendance`, mas sem dependência direta entre as duas entidades.

## 6. Relação com MedicalRecord

`MedicalRecord` permanece a memória clínica longitudinal consolidada e não deve ser inflado com coleções diretas de prescrições.

A prescrição deve compor o histórico clínico por meio de `Attendance`, que por sua vez se relaciona a `Patient`. Não se recomenda criar vínculo direto novo entre `Prescription` e `MedicalRecord` nesta fase.

Essa decisão preserva as premissas das Fases 6 e 7: `MedicalRecord` não deve virar agregado genérico de atendimento, evolução, prescrição, agenda, estoque e financeiro.

## 7. Estado atual de Prescription

`Prescription` existe como entidade de domínio.

### 7.1 Propriedades atuais

A entidade possui:

```text
Id
AttendanceId
IssuedAt
Notes
```

`AttendanceId` é obrigatório no domínio e na configuração EF. `IssuedAt` é obrigatório no domínio e na configuração EF. `Notes` é opcional, normalizado para `null` quando vazio ou whitespace.

### 7.2 Vínculo com Attendance

O vínculo atual é por `AttendanceId`. A configuração EF usa `HasOne<Attendance>().WithMany().HasForeignKey(p => p.AttendanceId).OnDelete(DeleteBehavior.Restrict)`.

### 7.3 Vínculo com PrescriptionItem

A entidade `Prescription` não expõe coleção de `PrescriptionItem` no domínio atual. O vínculo é configurado do lado de `PrescriptionItem`, por `PrescriptionId`, com `DeleteBehavior.Cascade`.

### 7.4 Timestamps e status

`Prescription` possui `IssuedAt`, que representa data/hora de emissão clínica/operacional. Não possui `CreatedAt`, `UpdatedAt`, `PrescribedAt`, `CanceledAt` ou status próprio.

Não foi encontrado status de prescrição, como `Active`, `Canceled`, `Draft`, `Signed` ou equivalente.

### 7.5 Instruções clínicas

`Prescription` possui apenas `Notes` como texto opcional no nível da prescrição. Não há campo específico de instruções clínicas gerais além de `Notes`.

### 7.6 Validações existentes

Validações atuais no domínio:

- `AttendanceId` deve ser maior que zero;
- `IssuedAt` não pode ser `default`;
- `Notes` é normalizado para `null` quando vazio.

### 7.7 Métodos de domínio existentes

Métodos atuais:

- `Create(long attendanceId, DateTime issuedAt, string? notes)`;
- `UpdateNotes(string? notes)`.

Não há métodos de domínio para cancelamento, assinatura, adição/remoção de itens, autoria, auditoria, soft delete, versionamento ou retificação.

## 8. Estado atual de PrescriptionItem

`PrescriptionItem` existe como entidade de domínio.

### 8.1 Propriedades atuais

A entidade possui:

```text
Id
PrescriptionId
ProductId
Quantity
Unit
Dosage
DurationDays
```

### 8.2 Vínculo com Prescription

`PrescriptionId` é obrigatório no domínio e na configuração EF. A configuração EF vincula `PrescriptionItem` a `Prescription` com `DeleteBehavior.Cascade`.

### 8.3 Vínculo com Product

`ProductId` existe como `long?`, portanto é opcional no modelo de domínio atual. Contudo, não foi encontrada entidade `Product`, `ProductConfiguration` ou `DbSet<Product>` no estado atual consultado.

A configuração EF atual apenas cria propriedade e índice para `ProductId`; não há relacionamento `HasOne<Product>()` configurado.

### 8.4 Campos clínicos e operacionais

Campos atuais:

- `Quantity`: quantidade decimal obrigatória, com precisão EF `12,3`;
- `Unit`: unidade obrigatória, `MaxLength(20)`;
- `Dosage`: dose/posologia obrigatória, `MaxLength(200)`;
- `DurationDays`: duração opcional em dias.

Não foram encontrados campos específicos `Frequency` ou `Instructions` em `PrescriptionItem`.

### 8.5 Validações existentes

Validações atuais no domínio:

- `PrescriptionId` deve ser maior que zero;
- `ProductId`, quando informado, deve ser maior que zero;
- `Quantity` deve ser maior que zero;
- `Unit` é obrigatório;
- `Dosage` é obrigatório;
- `DurationDays`, quando informado, deve ser maior que zero.

### 8.6 Métodos existentes

Métodos atuais:

- `Create(long prescriptionId, long? productId, decimal quantity, string unit, string dosage, int? durationDays)`;
- `Update(decimal quantity, string unit, string dosage, int? durationDays)`.

Não há método de domínio para troca de produto, cancelamento de item, assinatura, trilha de alteração ou baixa/reserva de estoque.

## 9. Estado atual de Product/estoque

Não foi encontrado arquivo `backend/src/Togo.Domain/Entities/Product.cs`.

Não foi encontrado arquivo `backend/src/Togo.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`.

Não foi encontrado `DbSet<Product>` em `AppDbContext`.

Não foram encontrados indícios de vertical implementada de estoque/inventário associada a `PrescriptionItem.ProductId`. Portanto, `ProductId` deve ser tratado como referência técnica planejada/latente, não como integração real com catálogo ou estoque já disponível.

Decisão obrigatória recomendada: não implementar baixa ou reserva de estoque na primeira implementação clínica de `Prescription`. A trilha inicial deve modelar e persistir a prescrição clínica. Estoque exige decisão separada sobre produto, disponibilidade, lote, validade, venda, financeiro, rastreabilidade e autorização operacional.

## 10. Arquivos encontrados

Arquivos obrigatórios encontrados:

- `backend/src/Togo.Domain/Entities/Prescription.cs`.
- `backend/src/Togo.Domain/Entities/PrescriptionItem.cs`.
- `backend/src/Togo.Domain/Entities/Attendance.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/PrescriptionConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/PrescriptionItemConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/AttendanceConfiguration.cs`.
- `backend/src/Togo.Infrastructure/Persistence/AppDbContext.cs`.

## 11. Arquivos ausentes

Arquivos/diretórios citados para consulta que não existem no estado atual inspecionado:

- `backend/src/Togo.Domain/Entities/Product.cs`.
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`.
- `backend/src/Togo.Application/Prescriptions`.
- `backend/src/Togo.Api/Controllers/PrescriptionsController.cs`.
- `backend/src/Togo.Application.Tests/Prescriptions`.
- `backend/src/Togo.Infrastructure.Tests/Repositories/PrescriptionRepositoryTests.cs`.
- `backend/src/Togo.Api.Tests/Controllers/PrescriptionsControllerTests.cs`.

## 12. Superfície pública atual

Não foi encontrado controller de `Prescription`.

Não foram encontrados use cases, contracts, repository interface, repository implementation ou testes específicos de `Prescription` no estado atual consultado.

Conclusão: `Prescription` e `PrescriptionItem` existem como domínio/configuração de persistência, mas ainda não possuem superfície pública operacional própria.

## 13. Lacunas de autorização

Não foram encontradas permissões, policies, controller com `[Authorize]` ou matriz por profile específica para `Prescription`.

Lacuna: qualquer endpoint futuro de prescrição deve ser precedido por autorização granular. Não se recomenda expor endpoint público de prescrição apenas com autenticação genérica ou reaproveitando permissões de `Attendance` sem decisão explícita.

Permissões candidatas futuras:

```text
Prescription.Read
Prescription.Create
Prescription.Update
Prescription.Cancel
```

Evitar inicialmente:

```text
Prescription.Delete
```

Motivo: exclusão física de prescrição clínica é sensível e pode apagar histórico relevante. Cancelamento/retificação auditável tende a ser caminho mais seguro.

## 14. Lacunas de autoria

`Prescription` não possui atualmente:

```text
CreatedByUserId
CreatedAt
UpdatedByUserId
UpdatedAt
PrescribedByUserId
PrescribedAt
CanceledByUserId
CanceledAt
```

`PrescriptionItem` também não possui campos próprios de autoria.

Lacuna: a primeira implementação pública de prescrição deve incluir autoria mínima desde o início ou, no mínimo, a subfase de contratos/base deve deixar a decisão explícita antes de expor endpoint público.

Campos candidatos para implementação futura:

```text
CreatedByUserId
CreatedAt
UpdatedByUserId
UpdatedAt
```

Avaliação futura: `PrescribedByUserId` e `PrescribedAt` podem ser úteis se o domínio diferenciar emissão clínica de criação técnica. Contudo, deve-se evitar duplicar semântica sem necessidade. No modelo atual, `IssuedAt` já representa timestamp clínico/operacional de emissão, enquanto `CreatedAt` futuro representaria persistência técnica.

## 15. Lacunas de AuditLog

Não foram encontradas actions específicas de auditoria para:

```text
Prescription.Created
Prescription.Updated
Prescription.Canceled
Prescription.Deleted
```

Não foram encontrados use cases de prescrição usando `IClinicalAuditLogWriter`.

Recomendação futura: iniciar com AuditLog mínimo para `Prescription.Created` quando a criação pública for implementada. `Prescription.Updated` e `Prescription.Canceled` devem ser planejados conforme houver endpoints reais de alteração/cancelamento. `Prescription.Deleted` não deve ser priorizado.

Metadata futura deve ser mínima e não deve conter dose, frequência, duração, instruções clínicas, `Notes`, `Dosage`, texto completo da prescrição ou payload do request.

## 16. Riscos clínicos e operacionais

Riscos obrigatórios mapeados:

- exposição de prescrição medicamentosa sensível;
- criação de endpoint sem autorização granular;
- prescrição sem autoria clínica/técnica;
- prescrição sem AuditLog;
- prescrição sem vínculo com atendimento correto;
- prescrição criada ou alterada em atendimento fechado/cancelado sem decisão formal;
- alteração de prescrição sem rastreabilidade;
- exclusão de prescrição sem soft delete, cancelamento ou auditoria;
- retorno de itens completos em listagens amplas;
- listagem global de prescrições sem necessidade operacional mínima;
- registrar dose, duração, posologia, instruções ou observações clínicas em metadata de AuditLog;
- confundir `IssuedAt` com `CreatedAt` futuro;
- ausência de status/cancelamento dificultar correção segura;
- acoplamento indevido com `MedicalRecord`;
- acoplamento prematuro com `ClinicalEvolution`;
- uso de `ProductId` como se houvesse catálogo/estoque real implementado.

## 17. Riscos de estoque

Riscos específicos de estoque/produto:

- baixa automática indevida de estoque a partir de uma prescrição clínica;
- reserva de estoque sem decisão de negócio, lote, validade ou disponibilidade;
- mistura entre prescrição clínica e venda/financeiro;
- uso de `ProductId` opcional sem entidade `Product` real configurada;
- pressupor medicamento cadastrado quando o modelo atual permite item sem produto;
- divergência entre quantidade prescrita, quantidade dispensada e quantidade vendida;
- falta de rastreabilidade para dispensação, estorno, lote e validade;
- exposição de dados comerciais/financeiros junto a dado clínico sensível.

Decisão recomendada: estoque deve ser trilha separada ou subfase posterior específica, não parte da primeira implementação clínica de `Prescription`.

## 18. Decisões técnicas recomendadas

### 18.1 Vínculo

`Prescription` deve permanecer vinculada a `Attendance` por `AttendanceId`.

### 18.2 PrescriptionItem

`Prescription` deve possuir itens de prescrição. O estado atual modela `PrescriptionItem` com `PrescriptionId` obrigatório e `ProductId` opcional. Como não há entidade `Product` implementada, a próxima fase não deve inventar integração real com produto/estoque.

### 18.3 Superfície inicial futura

Começar com superfície orientada por atendimento:

```text
POST /api/attendances/{attendanceId}/prescriptions
GET /api/attendances/{attendanceId}/prescriptions
```

A rota exata deve ser confirmada na Fase 7.5.2/7.5.3, mas o princípio recomendado é não criar listagem global inicial.

### 18.4 Autorização

`Prescription` deve ter autorização granular antes de qualquer endpoint público.

Permissões candidatas:

```text
Prescription.Read
Prescription.Create
Prescription.Update
Prescription.Cancel
```

Não criar `Prescription.Delete` inicialmente.

### 18.5 Autoria

`Prescription` deve ter autoria mínima desde a primeira implementação pública.

Campos candidatos:

```text
CreatedByUserId
CreatedAt
UpdatedByUserId
UpdatedAt
```

Avaliar em fase posterior se `PrescribedByUserId`/`PrescribedAt` agregam valor além de `CreatedByUserId`/`IssuedAt`.

### 18.6 AuditLog

Planejar AuditLog mínimo futuro para `Prescription.Created` na criação pública. `Prescription.Updated` e `Prescription.Canceled` devem vir apenas quando houver operações reais correspondentes.

### 18.7 Estoque

Não implementar baixa/reserva de estoque na primeira implementação clínica de `Prescription`.

### 18.8 Metadata de AuditLog

Não registrar dose, frequência, duração, instruções clínicas, `Notes`, `Dosage`, texto completo da prescrição ou payload completo em metadata de AuditLog.

Metadata candidata futura para `Prescription.Created` deve ser mínima, por exemplo apenas identificadores técnicos e status/contagem não sensível, após decisão formal:

```json
{
  "AttendanceId": 123,
  "ItemCount": 2
}
```

Mesmo essa metadata deve ser validada para não carregar conteúdo clínico sensível.

## 19. Subfases futuras propostas

Fracionamento recomendado da Fase 7.5:

```text
7.5.1 — Planejamento técnico da integração Prescription com Attendance
7.5.2 — Contratos/base técnica para Prescription vinculada a Attendance
7.5.3 — Implementação mínima de Prescription vinculada a Attendance
7.5.4 — Autoria e AuditLog mínimos de Prescription
7.5.5 — Testes e evidências da integração Prescription
7.5.6 — Encerramento da trilha Prescription
```

A fase 7.5.2 pode preparar permissões, contracts e audit actions necessários para a trilha, sem expor endpoint público de criação de Prescription.

Se a implementação mínima ficar grande, recomenda-se dividir 7.5.3 em subfases menores, mas sem expor endpoint público de criação antes de autoria e AuditLog mínimos.

Opção segura de fracionamento:

```text
7.5.3.1 — Repository/use case mínimo interno de Prescription, sem endpoint público de criação
7.5.3.2 — Autoria e AuditLog mínimos de Prescription aplicados ao fluxo de criação
7.5.3.3 — Controller mínimo por AttendanceId com autorização granular, expondo POST/GET somente após autoria e AuditLog estarem disponíveis
```

Alternativamente, manter 7.5.3 e 7.5.4 como fases separadas, desde que a 7.5.3 não exponha POST público de criação antes da 7.5.4.

Prescrição clínica é dado sensível. Não deve haver superfície pública de criação de Prescription sem autoria técnica e AuditLog mínimo desde o primeiro endpoint público.

A divisão deve preservar escopo pequeno e evitar misturar contratos, endpoints, autoria, auditoria, estoque e cancelamento em uma única PR. Caso a implementação seja dividida, o controller público deve vir junto com autoria/AuditLog ou depois deles.

## 20. Fora do escopo desta fase

Ficam explicitamente fora do escopo:

- alterações em código C#;
- testes;
- migrations;
- schema;
- banco;
- entities;
- controllers;
- use cases;
- repositories;
- contracts/DTOs;
- autorização;
- autoria;
- AuditLog;
- estoque;
- frontend;
- Docker, Redis, RabbitMQ, Kubernetes;
- endpoint de prescrição;
- repository de `Prescription`;
- use case de `Prescription`;
- DTO/contract novo;
- policy/permission;
- baixa/reserva de estoque;
- validação real de produto;
- vínculo direto com evolução clínica;
- impressão/PDF/receituário;
- assinatura clínica;
- integração com venda/financeiro.

## 21. Critérios de aceite

A Fase 7.5.1 é considerada concluída quando:

- este documento for criado;
- o estado atual de `Prescription` for inspecionado;
- o estado atual de `PrescriptionItem` for inspecionado;
- o estado atual de `Product`/estoque for inspecionado;
- a relação com `Attendance` for documentada;
- a relação com `ClinicalEvolution` for documentada;
- a relação com `MedicalRecord` for documentada;
- lacunas de autorização forem mapeadas;
- lacunas de autoria forem mapeadas;
- lacunas de AuditLog forem mapeadas;
- riscos clínicos/operacionais forem registrados;
- riscos de estoque forem registrados;
- decisões técnicas iniciais forem propostas;
- subfases futuras forem recomendadas;
- a próxima fase 7.5.2 for recomendada;
- nenhuma implementação for feita;
- nenhuma migration for criada;
- somente documentação for alterada;
- `git diff --check` passar.

## 22. Próxima fase recomendada

Próxima fase recomendada:

```text
Fase 7.5.2 — Contratos/base técnica para Prescription vinculada a Attendance
```

Objetivo sugerido:

```text
Definir contratos mínimos, permissões planejadas e ações de auditoria base para Prescription vinculada a Attendance, sem ainda expor endpoint público, sem persistir nova regra e sem movimentar estoque.
```

A Fase 7.5.2 deve continuar preservando escopo pequeno, preferencialmente preparando nomes de contracts, permissões e audit actions sem criar endpoint público amplo e sem assumir integração real com produto/estoque.
