# PHASE 06.02.05 — MedicalRecord Authorization Test Evidence

## 1. Contexto da Fase 6.2
A Fase 6.2 consolida a autorização granular da vertical **MedicalRecord** em etapas incrementais, mantendo o foco em permissões mínimas, explícitas e testáveis para operações de prontuário clínico.

Esta fase sucede a implementação de enforcement real feita na Fase 6.2.4 e concentra-se em evidências de teste, matriz perfil x permissão e documentação para preparar o encerramento formal da Fase 6.2.

## 2. Referência explícita ao MR-DEBT-003
A Fase 6.2.5 continua o tratamento do débito técnico **MR-DEBT-003 — Roles/permissões finas ausentes**.

O objetivo desta fase não é ampliar regra funcional, mas demonstrar, por testes diretos e testes de controller, que a autorização granular MedicalRecord está protegida contra acessos sem autenticação, acessos autenticados sem permissão, tokens sem profile e profiles inválidos.

## 3. Relação com as fases 6.2.1, 6.2.2, 6.2.3 e 6.2.4
- **6.2.1:** planejou a autorização granular MedicalRecord e delimitou uma abordagem incremental para MR-DEBT-003.
- **6.2.2:** criou constantes centralizadas para permissões e policies MedicalRecord, sem enforcement funcional.
- **6.2.3:** introduziu perfis mínimos de usuário, a claim `togo:profile` e a emissão do profile no JWT.
- **6.2.4:** registrou as policies MedicalRecord no pipeline ASP.NET Core Authorization e aplicou policies por operação no `MedicalRecordsController`.
- **6.2.5:** consolida as evidências automatizadas da matriz perfil x permissão e documenta explicitamente os cenários permitidos, `401 Unauthorized`, `403 Forbidden`, token sem profile e profile inválido.

## 4. Objetivo da fase
Consolidar e ampliar as evidências de teste da autorização granular MedicalRecord, garantindo que a matriz perfil x permissão esteja protegida por testes suficientes antes da preparação do encerramento da Fase 6.2 e da futura atualização do registro vivo de débitos.

## 5. Escopo da fase
O escopo desta fase foi limitado a:
- criar testes diretos para `MedicalRecordAuthorization.HasPermission`;
- consolidar a cobertura da matriz perfil x permissão;
- revisar os testes existentes do `MedicalRecordsController`;
- confirmar cobertura explícita para `401 Unauthorized`;
- confirmar cobertura explícita para `403 Forbidden`;
- confirmar cobertura explícita para acessos permitidos;
- confirmar bloqueio de token sem claim `togo:profile`;
- confirmar bloqueio de profile vazio ou inválido;
- documentar a matriz de evidências.

Não houve alteração intencional de regra de negócio, comportamento funcional, frontend ou infraestrutura.

## 6. Arquivos criados/alterados
- `backend/src/Togo.Api.Tests/Security/MedicalRecordAuthorizationTests.cs` — novo teste direto da autorização MedicalRecord.
- `docs/clinical-core/PHASE_06_02_05_MEDICAL_RECORD_AUTHORIZATION_TEST_EVIDENCE.md` — documentação desta fase e matriz de evidências.

Os seguintes arquivos foram revisados como base da evidência, sem necessidade de alteração funcional nesta fase:
- `backend/src/Togo.Api/Security/MedicalRecordAuthorization.cs`
- `backend/src/Togo.Api/Security/MedicalRecordPolicies.cs`
- `backend/src/Togo.Application/Security/MedicalRecordPermissions.cs`
- `backend/src/Togo.Domain/Security/UserProfiles.cs`
- `backend/src/Togo.Domain/Security/TogoClaimTypes.cs`
- `backend/src/Togo.Api/Controllers/MedicalRecordsController.cs`
- `backend/src/Togo.Api.Tests/MedicalRecords/MedicalRecordsControllerTests.cs`

## 7. Matriz de evidência perfil x permissão
A matriz esperada da Fase 6.2.4 permanece inalterada e agora possui teste direto em `MedicalRecordAuthorizationTests`.

| Perfil / entrada | `MedicalRecord.Read` | `MedicalRecord.Create` | `MedicalRecord.Update` | Evidência principal |
| --- | --- | --- | --- | --- |
| `Admin` | Permitido | Permitido | Permitido | `HasPermission_ShouldReturnTrue_WhenProfileHasMedicalRecordPermission` |
| `Veterinarian` | Permitido | Permitido | Permitido | `HasPermission_ShouldReturnTrue_WhenProfileHasMedicalRecordPermission` |
| `Assistant` | Permitido | Bloqueado | Bloqueado | `HasPermission_ShouldReturnTrue_WhenProfileHasMedicalRecordPermission` e `HasPermission_ShouldReturnFalse_WhenProfileDoesNotHaveMedicalRecordPermission` |
| `Reception` | Bloqueado | Bloqueado | Bloqueado | `HasPermission_ShouldReturnFalse_WhenProfileDoesNotHaveMedicalRecordPermission` |
| `ReadOnly` | Bloqueado | Bloqueado | Bloqueado | `HasPermission_ShouldReturnFalse_WhenProfileDoesNotHaveMedicalRecordPermission` |
| Token sem `togo:profile` | Bloqueado | Bloqueado | Bloqueado | `HasPermission_ShouldReturnFalseForEveryMedicalRecordPermission_WhenProfileClaimIsMissing` |
| Claim `togo:profile` vazia | Bloqueado | Bloqueado | Bloqueado | `HasPermission_ShouldReturnFalseForEveryMedicalRecordPermission_WhenProfileClaimIsEmpty` |
| Claim `togo:profile` inválida | Bloqueado | Bloqueado | Bloqueado | `HasPermission_ShouldReturnFalseForEveryMedicalRecordPermission_WhenProfileClaimIsInvalid` |
| `veterinarian` com casing diferente | Permitido | Permitido | Permitido | `HasPermission_ShouldNormalizeProfileClaimCasing` |
| Permissão desconhecida | Bloqueado | Bloqueado | Bloqueado | `HasPermission_ShouldReturnFalse_WhenPermissionIsUnknown` |

## 8. Evidências de 401 Unauthorized
A cobertura de `401 Unauthorized` permanece nos testes do `MedicalRecordsController` para requisições sem token:
- `Get_ShouldReturnUnauthorized_WithoutToken`;
- `Post_ShouldReturnUnauthorized_WithoutToken`;
- `Put_ShouldReturnUnauthorized_WithoutToken`.

Esses testes demonstram que a API exige autenticação antes de avaliar as policies MedicalRecord.

## 9. Evidências de 403 Forbidden
A cobertura de `403 Forbidden` permanece nos testes do `MedicalRecordsController` para usuários autenticados, porém sem permissão suficiente:
- `Get_ShouldReturnForbidden_WithTokenWithoutProfileClaim`;
- `Get_ShouldReturnForbidden_WhenProfileCannotRead`;
- `Post_ShouldReturnForbidden_WhenProfileCannotCreate`;
- `Post_ShouldReturnForbidden_WithTokenWithoutProfileClaim`;
- `Put_ShouldReturnForbidden_WhenProfileCannotUpdate`;
- `Put_ShouldReturnForbidden_WithTokenWithoutProfileClaim`.

Esses testes cobrem perfis bloqueados, ausência de claim de profile e operações de leitura, criação e atualização.

## 10. Evidências de acessos permitidos
A cobertura de acessos permitidos permanece nos testes do `MedicalRecordsController` e foi reforçada pelos testes diretos de autorização:
- `Get_ShouldReturnOk_WhenMedicalRecordExists` usa cliente autenticado como `Veterinarian`;
- `Get_ShouldReturnOk_WhenProfileIsAssistant` confirma leitura permitida para `Assistant`;
- `Post_ShouldReturnCreated_WhenValid` usa cliente autenticado como `Veterinarian`;
- `Post_ShouldReturnCreated_WhenProfileIsAdmin` confirma criação permitida para `Admin`;
- `Put_ShouldReturnOk_WhenUpdated` usa cliente autenticado como `Veterinarian`;
- `Put_ShouldReturnOk_WhenProfileCanUpdate` confirma atualização permitida para `Admin` e `Veterinarian`;
- `HasPermission_ShouldReturnTrue_WhenProfileHasMedicalRecordPermission` confirma diretamente a matriz permitida para `Admin`, `Veterinarian` e leitura de `Assistant`.

## 11. Evidências de token sem profile
O bloqueio de token autenticado sem claim `togo:profile` está coberto em duas camadas:
- teste direto: `HasPermission_ShouldReturnFalseForEveryMedicalRecordPermission_WhenProfileClaimIsMissing`;
- testes de controller: `Get_ShouldReturnForbidden_WithTokenWithoutProfileClaim`, `Post_ShouldReturnForbidden_WithTokenWithoutProfileClaim` e `Put_ShouldReturnForbidden_WithTokenWithoutProfileClaim`.

## 12. Evidências de profile inválido
O bloqueio de profile inválido está coberto diretamente por `HasPermission_ShouldReturnFalseForEveryMedicalRecordPermission_WhenProfileClaimIsInvalid`.

A claim vazia também está coberta por `HasPermission_ShouldReturnFalseForEveryMedicalRecordPermission_WhenProfileClaimIsEmpty`, garantindo que entradas sem valor útil não sejam normalizadas para um perfil válido.

## 13. Confirmação de que não houve RBAC complexo
Esta fase não introduziu RBAC complexo.

Não foram criadas tabelas de roles, tabelas de permissões, agregados de autorização, entidades persistidas de perfil/permissão ou mecanismos dinâmicos de concessão de autorização.

A matriz permanece explícita e mínima em `MedicalRecordAuthorization`, conforme a decisão técnica da Fase 6.2.4.

## 14. Confirmação de que não houve endpoint administrativo de perfil
Esta fase não criou endpoint administrativo de perfil, role ou permissão.

A autorização continua baseada na claim `togo:profile` emitida no fluxo existente e avaliada pelas policies MedicalRecord.

## 15. Confirmação de que não houve alteração de frontend/infra
Esta fase não alterou frontend, Docker, Redis, RabbitMQ, Kubernetes, CI/CD ou qualquer componente de infraestrutura.

As mudanças ficaram restritas a testes automatizados de autorização e documentação clínica/técnica da fase.

## 16. Critérios de aceite
A Fase 6.2.5 atende aos critérios de aceite quando:
- há testes diretos da matriz de autorização MedicalRecord;
- há evidência de bloqueio para token sem profile;
- há evidência de bloqueio para profile inválido;
- há evidência de `401 Unauthorized` para usuário não autenticado;
- há evidência de `403 Forbidden` para usuário autenticado sem permissão;
- há evidência de acesso permitido para perfis autorizados;
- os testes existentes do controller continuam válidos;
- nenhuma regra de negócio é alterada sem necessidade;
- não há RBAC complexo;
- não há endpoint administrativo de perfil/permissão;
- não há alteração de frontend ou infraestrutura;
- a documentação da fase existe;
- `dotnet build backend/Togo.sln` passa;
- `dotnet test backend/Togo.sln` passa.

## 17. Fora do escopo
Permanecem fora do escopo desta fase:
- alterar a matriz de autorização definida na Fase 6.2.4;
- criar RBAC completo;
- criar tabelas persistidas de role/permissão;
- criar endpoint administrativo de perfil/permissão;
- alterar frontend;
- alterar infraestrutura;
- alterar emissão de JWT além do que já foi definido na Fase 6.2.3;
- tratar MR-DEBT-002, MR-DEBT-004 ou outros débitos fora do MR-DEBT-003;
- atualizar o registro vivo de débitos técnicos, reservado para a próxima fase.

## 18. Próxima fase recomendada
**Fase 6.2.6 — Atualização do registro vivo, evidências finais e encerramento da Fase 6.2.**

Objetivo recomendado: atualizar o registro vivo de débitos técnicos da Fase 6.1.1, registrar que **MR-DEBT-003** foi tratado pela sequência **6.2.1 até 6.2.5**, anexar evidências de implementação/testes e encerrar formalmente a Fase 6.2.

## 19. Decisão da fase
**Opção A — Evidências de autorização granular MedicalRecord aprovadas como base para preparação do encerramento do MR-DEBT-003.**
