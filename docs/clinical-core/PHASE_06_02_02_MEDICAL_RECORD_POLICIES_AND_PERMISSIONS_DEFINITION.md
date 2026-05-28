# PHASE 06.02.02 — MedicalRecord Policies and Permissions Definition

## 1. Contexto da Fase 6.2
A Fase 6.2 trata da evolução da autorização da vertical **MedicalRecord** com implantação incremental, visando reduzir risco técnico e evitar mudanças acopladas em autenticação, domínio e API no mesmo passo.

## 2. Referência explícita ao MR-DEBT-003
Esta fase endereça o débito técnico **MR-DEBT-003 — roles/permissões finas ausentes**, iniciando a base de nomenclatura estável para autorização granular futura.

## 3. Relação com a Fase 6.2.1
A Fase 6.2.1 (planejamento) definiu a abordagem incremental. A Fase 6.2.2 implementa apenas a fundação técnica de nomes centralizados de permissões e policies, sem ativar enforcement.

## 4. Objetivo da fase
Criar definição centralizada e explícita de permissões e policies de MedicalRecord para evitar strings mágicas e preparar as fases seguintes (6.2.3+), sem alterar fluxo atual de autenticação/autorização.

## 5. Decisão técnica adotada
**Opção A aprovada:** centralização de constantes em classes estáticas separadas por camada:
- `Togo.Application.Security.MedicalRecordPermissions`
- `Togo.Api.Security.MedicalRecordPolicies`

A decisão preserva separação de responsabilidades e evita dependência circular.

## 6. Arquivos criados
- `backend/src/Togo.Application/Security/MedicalRecordPermissions.cs`
- `backend/src/Togo.Api/Security/MedicalRecordPolicies.cs`
- `backend/src/Togo.Application.Tests/Security/MedicalRecordPermissionsTests.cs`
- `backend/src/Togo.Api.Tests/Security/MedicalRecordPoliciesTests.cs`
- `docs/clinical-core/PHASE_06_02_02_MEDICAL_RECORD_POLICIES_AND_PERMISSIONS_DEFINITION.md`

## 7. Permissões definidas
Em `MedicalRecordPermissions`:
- `Read = "MedicalRecord.Read"`
- `Create = "MedicalRecord.Create"`
- `Update = "MedicalRecord.Update"`

## 8. Policies definidas
Em `MedicalRecordPolicies`:
- `Read = "MedicalRecord.Read"`
- `Create = "MedicalRecord.Create"`
- `Update = "MedicalRecord.Update"`

## 9. O que ainda não foi implementado
- Sem alteração em `User`.
- Sem alteração em `JwtTokenService`.
- Sem alteração em `Program.cs` (sem registro de policies).
- Sem alteração em `MedicalRecordsController` (sem `[Authorize(Policy = ...)]`).
- Sem claims/perfis reais.
- Sem mudança de banco/migração.
- Sem novos endpoints.

## 10. Impacto esperado nas próximas fases
- 6.2.3: habilita evolução de perfil/claim mínima com nomes já padronizados.
- 6.2.4: permite aplicação de policies no controller com baixo risco de typo.
- 6.2.5: facilita escrita de testes de autorização granular baseados em constantes estáveis.
- 6.2.6: simplifica rastreabilidade documental e evidências.

## 11. Critérios de aceite
- `MedicalRecordPermissions` criado com `Read/Create/Update`.
- `MedicalRecordPolicies` criado com `Read/Create/Update`.
- Nomes explícitos e estáveis.
- Sem alteração em User/JWT/Program.cs/Controller/banco.
- Sem migration.
- Sem endpoint novo.
- Testes simples criados para validar valores, não vazio e distinção.
- Build e test suite devem permanecer válidos.

## 12. Fora do escopo
- Implementação de autorização granular real.
- Criação de RBAC completo.
- Mudança de autenticação/token.
- Mudanças em frontend, infraestrutura (Docker/Redis/RabbitMQ/Kubernetes) ou configuração de ambiente.

## 13. Próxima fase recomendada
**Fase 6.2.3 — Evolução de User/JWT para suportar perfil/claim mínima**, preparando avaliação futura das policies sem RBAC prematuro.
