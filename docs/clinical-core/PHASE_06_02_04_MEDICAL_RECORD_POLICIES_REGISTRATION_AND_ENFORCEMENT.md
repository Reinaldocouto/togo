# PHASE 06.02.04 — MedicalRecord Policies Registration and Enforcement

## 1. Contexto da Fase 6.2
A Fase 6.2 evolui a autorização da vertical **MedicalRecord** por etapas, com foco em reduzir o débito de autorização fina sem introduzir um RBAC completo antes de existir necessidade operacional validada.

## 2. Referência explícita ao MR-DEBT-003
Esta fase continua o tratamento do débito técnico **MR-DEBT-003 — roles/permissões finas ausentes**. O objetivo é transformar a base criada nas fases anteriores em enforcement real no pipeline da API e nas operações sensíveis de prontuário clínico.

## 3. Relação com as Fases 6.2.1, 6.2.2 e 6.2.3
- **6.2.1:** documentou o planejamento de autorização granular para MedicalRecord e delimitou a implantação incremental.
- **6.2.2:** criou constantes centralizadas de permissões e policies (`MedicalRecord.Read`, `MedicalRecord.Create`, `MedicalRecord.Update`) sem ativar enforcement.
- **6.2.3:** adicionou perfis mínimos de usuário, a claim própria `togo:profile` e emissão dessa claim no JWT.
- **6.2.4:** registra as policies no ASP.NET Core Authorization e aplica cada policy na operação correspondente do `MedicalRecordsController`.

## 4. Objetivo da fase
Registrar e aplicar policies MedicalRecord de forma granular por operação, usando a claim `togo:profile` como fonte mínima de autorização e mantendo a solução simples, explícita e sem RBAC completo.

## 5. Decisão técnica adotada
Foi adotada a **Opção A — Policies MedicalRecord registradas e aplicadas por operação, com autorização granular mínima baseada em perfil JWT**.

A implementação adiciona `MedicalRecordAuthorization` na API para centralizar:
- o registro das policies MedicalRecord;
- a relação explícita entre perfis mínimos e permissões MedicalRecord;
- a avaliação da claim `TogoClaimTypes.Profile` (`togo:profile`) contra a permissão exigida.

A abordagem evita strings mágicas no `Program.cs`, não cria tabelas de roles/permissões, não cria endpoints administrativos e não emite permissões detalhadas no token.

## 6. Arquivos alterados/criados
- `backend/src/Togo.Api/Security/MedicalRecordAuthorization.cs`
- `backend/src/Togo.Api/Program.cs`
- `backend/src/Togo.Api/Controllers/MedicalRecordsController.cs`
- `backend/src/Togo.Api.Tests/MedicalRecords/MedicalRecordsControllerTests.cs`
- `docs/clinical-core/PHASE_06_02_04_MEDICAL_RECORD_POLICIES_REGISTRATION_AND_ENFORCEMENT.md`

## 7. Policies registradas
As seguintes policies MedicalRecord foram registradas no pipeline de autorização da API:

| Policy | Permissão avaliada | Uso esperado |
| --- | --- | --- |
| `MedicalRecord.Read` | `MedicalRecord.Read` | Leitura de prontuário clínico |
| `MedicalRecord.Create` | `MedicalRecord.Create` | Criação de prontuário clínico |
| `MedicalRecord.Update` | `MedicalRecord.Update` | Atualização de prontuário clínico |

Todas exigem usuário autenticado e profile válido na claim `togo:profile`.

## 8. Matriz de perfil x permissão aplicada
A matriz inicial aplicada é conservadora:

| Perfil | Read | Create | Update | Justificativa |
| --- | --- | --- | --- | --- |
| `Admin` | Sim | Sim | Sim | Perfil administrativo com acesso operacional completo nesta fase mínima. |
| `Veterinarian` | Sim | Sim | Sim | Perfil clínico responsável por consultar, criar e atualizar prontuários. |
| `Assistant` | Sim | Não | Não | Apoia operação clínica com leitura, sem permissão de escrita em prontuário. |
| `Reception` | Não | Não | Não | Perfil administrativo de recepção não recebe acesso a dados clínicos sensíveis nesta fase. |
| `ReadOnly` | Não | Não | Não | Decisão conservadora: sem auditoria específica implementada, o perfil ReadOnly não acessa prontuário clínico. |

A decisão de negar MedicalRecord ao `ReadOnly` evita conceder leitura clínica ampla antes de existir auditoria operacional específica e critérios formais para esse perfil.

## 9. Impacto em `Program.cs`
`Program.cs` deixa de registrar autorização de forma genérica sem policies e passa a registrar as policies MedicalRecord via helper centralizado:

- `builder.Services.AddAuthorization(options => options.AddMedicalRecordPolicies());`

Com isso, as policies ficam disponíveis para uso por atributos `[Authorize(Policy = ...)]` sem espalhar lógica de perfil/permissão pelo bootstrap da aplicação.

## 10. Impacto em `MedicalRecordsController`
O controller mantém `[Authorize]` como camada base de autenticação, mas cada ação sensível passa a exigir policy explícita:

| Operação | Policy aplicada |
| --- | --- |
| `GET /api/patients/{patientId}/medical-record` | `MedicalRecordPolicies.Read` |
| `POST /api/patients/{patientId}/medical-record` | `MedicalRecordPolicies.Create` |
| `PUT /api/patients/{patientId}/medical-record` | `MedicalRecordPolicies.Update` |

Assim, a proteção deixa de depender apenas de `[Authorize]` genérico e passa a refletir a operação clínica executada.

## 11. Impacto em testes
Os testes do `MedicalRecordsController` foram ajustados para registrar as mesmas policies MedicalRecord usadas pela API e para emitir a claim `togo:profile` no handler de autenticação fake quando o cenário exige acesso permitido.

Foram cobertos cenários de:
- usuário não autenticado recebendo `401 Unauthorized`;
- usuário autenticado sem claim `togo:profile` recebendo `403 Forbidden`;
- `Reception` bloqueado para leitura, criação e atualização;
- `ReadOnly` bloqueado para leitura, criação e atualização;
- `Assistant` autorizado para leitura e bloqueado para criação/atualização;
- `Veterinarian` autorizado para leitura, criação e atualização;
- `Admin` autorizado para criação e atualização;
- preservação dos comportamentos funcionais já existentes quando o perfil possui permissão.

## 12. O que ainda não foi implementado
- RBAC completo.
- Tabelas `UserRole`, `RolePermission` ou equivalentes.
- Endpoints administrativos de perfil/permissão.
- Auditoria operacional específica para concessão futura de leitura ao perfil `ReadOnly`.
- Integração com AuditLog.
- Alterações de frontend.
- Alterações de Docker, Redis, RabbitMQ ou Kubernetes.
- Correção de outros débitos fora do MR-DEBT-003, incluindo MR-DEBT-002 e MR-DEBT-004.

## 13. Critérios de aceite
A fase atende aos critérios definidos quando:
- as policies MedicalRecord estão registradas no pipeline da API;
- `MedicalRecordsController` usa policies por operação;
- a autorização deixou de ser apenas `[Authorize]` genérico;
- a claim `togo:profile` é usada na autorização;
- os perfis são mapeados explicitamente para permissões;
- os testes cobrem acessos permitidos e bloqueados;
- os testes cobrem `401` e `403`;
- não há RBAC complexo;
- não há endpoints administrativos de perfil;
- a documentação da fase existe;
- build e test suite devem permanecer válidos no ambiente com SDK .NET disponível.

## 14. Fora do escopo
- Criar RBAC completo.
- Criar tabelas de roles/permissões.
- Criar endpoints administrativos de perfil.
- Alterar frontend.
- Alterar Docker, Redis, RabbitMQ ou Kubernetes.
- Expor dados clínicos sensíveis em logs, testes ou documentação.
- Misturar esta fase com AuditLog.
- Resolver MR-DEBT-002 ou MR-DEBT-004.

## 15. Próxima fase recomendada
**Fase 6.2.5 — Testes de autorização granular e evidências finais de segurança MedicalRecord.**

Objetivo recomendado: consolidar evidências de teste para MR-DEBT-003, ampliar a rastreabilidade dos cenários 401/403/permitidos e preparar o encerramento documentado da Fase 6.2.
