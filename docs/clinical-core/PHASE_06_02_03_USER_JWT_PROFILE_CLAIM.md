# PHASE 06.02.03 — User JWT Profile Claim

## 1. Contexto da Fase 6.2
A Fase 6.2 evolui a autorização da vertical **MedicalRecord** de forma incremental e rastreável, sem ativar autorização granular antes de existir uma base mínima de identidade autorizável.

## 2. Referência explícita ao MR-DEBT-003
Esta fase continua o tratamento do débito técnico **MR-DEBT-003 — roles/permissões finas ausentes**, adicionando uma informação mínima de perfil ao usuário autenticado e ao JWT.

## 3. Relação com as Fases 6.2.1 e 6.2.2
- **6.2.1:** definiu o planejamento para autorização granular de MedicalRecord e indicou a necessidade de preparar `User`/JWT.
- **6.2.2:** criou constantes centrais para permissões e policies de MedicalRecord, sem enforcement.
- **6.2.3:** adiciona perfil mínimo de usuário e claim de perfil no JWT, ainda sem aplicar policies em controllers.

## 4. Objetivo da fase
Definir e implementar uma estrutura mínima de perfil/claim para usuários autenticados, permitindo que tokens JWT carreguem contexto suficiente para avaliação futura das policies de MedicalRecord.

## 5. Decisão técnica adotada
**Opção A aprovada:** perfil/claim mínima como base para futura avaliação de policies MedicalRecord.

A implementação evita RBAC complexo e não cria tabelas de roles/permissões. O perfil é representado por uma string validada no domínio e persistida em `Users.Profile`.

## 6. Arquivos alterados/criados
- `backend/src/Togo.Domain/Security/UserProfiles.cs`
- `backend/src/Togo.Domain/Security/TogoClaimTypes.cs`
- `backend/src/Togo.Domain/Entities/User.cs`
- `backend/src/Togo.Infrastructure/Tokens/JwtTokenService.cs`
- `backend/src/Togo.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- `backend/src/Togo.Infrastructure/Migrations/20260528120000_AddUserProfile.cs`
- `backend/src/Togo.Infrastructure/Migrations/20260528120000_AddUserProfile.Designer.cs`
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `backend/src/Togo.Domain.Tests/UserTests.cs`
- `backend/src/Togo.Domain.Tests/Security/UserProfilesTests.cs`
- `backend/src/Togo.Infrastructure.Tests/Tokens/JwtTokenServiceTests.cs`
- `docs/clinical-core/PHASE_06_02_03_USER_JWT_PROFILE_CLAIM.md`

## 7. Modelo mínimo de perfis
Os perfis mínimos definidos são:
- `Admin`
- `Veterinarian`
- `Assistant`
- `Reception`
- `ReadOnly`

O perfil padrão conservador é `ReadOnly` para preservar compatibilidade com criações existentes sem perfil explícito.

## 8. Claim adotada
Foi adotada uma claim própria:

- `togo:profile`

A decisão evita confundir o perfil mínimo desta fase com RBAC completo ou com `ClaimTypes.Role`. Nenhuma permissão detalhada é emitida no JWT nesta fase.

## 9. Impacto em User
A entidade `User` passa a expor `Profile` e validar o valor via `UserProfiles.Normalize`.

Comportamentos adicionados:
- criação com perfil explícito válido;
- criação sem perfil explícito usando `ReadOnly`;
- rejeição de perfil vazio ou não suportado quando informado;
- atualização controlada de perfil por método de domínio.

## 10. Impacto em JWT
`JwtTokenService` passa a emitir a claim `togo:profile` com o perfil do usuário.

As claims existentes foram mantidas, incluindo:
- identificador do usuário (`sub` e `ClaimTypes.NameIdentifier`);
- nome;
- e-mail;
- `jti`.

`TryValidateToken` continua validando o token e extraindo `userId` da mesma forma, sem depender da nova claim.

## 11. Impacto em banco/migration
Como `User.Profile` precisa ser persistido para que logins futuros emitam tokens com perfil consistente, foi criada migration mínima `AddUserProfile`.

A migration adiciona a coluna obrigatória `Profile` em `Users`, com tamanho máximo 50 e `defaultValue: "ReadOnly"` para compatibilidade com registros existentes.

## 12. Testes criados/ajustados
Foram criados testes para:
- criação de usuário com perfil válido;
- perfil padrão `ReadOnly` quando criação existente não informa perfil;
- rejeição de perfil vazio/nulo;
- rejeição de perfil não suportado;
- normalização de perfil;
- emissão da claim `togo:profile` no JWT;
- manutenção da emissão de `sub`/e-mail;
- validação de token preservando extração de `userId`;
- rejeição de token vazio.

## 13. O que ainda não foi implementado
- Nenhuma policy foi registrada em `Program.cs`.
- Nenhuma policy foi aplicada em `MedicalRecordsController`.
- Nenhuma autorização granular real foi ativada.
- Nenhuma tabela complexa de roles/permissões foi criada.
- Nenhum relacionamento N:N `UserRole`/`Permission` foi criado.
- Nenhum endpoint administrativo de perfis foi criado.
- Nenhuma alteração de frontend foi realizada.

## 14. Critérios de aceite
- Existe modelo mínimo de perfis.
- `User` suporta perfil com validação e persistência mínima.
- `JwtTokenService` emite a claim `togo:profile`.
- O token continua emitindo e validando `userId` corretamente.
- Testes cobrem domínio e token.
- `MedicalRecordsController` não foi alterado.
- Policies ainda não foram aplicadas.
- Não há RBAC complexo.
- Documentação da fase foi criada.
- Build e test suite devem permanecer válidos.

## 15. Fora do escopo
- Aplicar `[Authorize(Policy = ...)]` em operações de MedicalRecord.
- Registrar policies MedicalRecord no pipeline da API.
- Mapear perfis para permissões detalhadas.
- Criar RBAC completo.
- Criar endpoints administrativos de role/perfil.
- Alterar frontend.
- Alterar Docker, Redis, RabbitMQ ou Kubernetes.

## 16. Próxima fase recomendada
**Fase 6.2.4 — Registro e aplicação das policies MedicalRecord.**

Objetivo recomendado: registrar policies no `Program.cs` e aplicar policies por operação no `MedicalRecordsController`, usando a base criada nas fases 6.2.2 e 6.2.3.
