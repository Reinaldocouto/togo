# Fase 6.3.3.1 — Hotfix dos defaults de autoria de MedicalRecord

## 1. Contexto da Fase 6.3.3

A Fase 6.3.3 adicionou autoria clínica mínima em `MedicalRecord`, incluindo os campos `CreatedByUserId`, `CreatedAt`, `UpdatedByUserId` e `UpdatedAt`.

Como `CreatedAt`, `CreatedByUserId` e `UpdatedByUserId` foram adicionados como colunas obrigatórias em uma tabela já existente, a migration `20260601120000_AddMedicalRecordAuthorship` precisou usar defaults técnicos para permitir a alteração do schema em bancos com dados legados.

## 2. Motivo do hotfix

Os defaults técnicos eram necessários apenas durante a criação das colunas obrigatórias. Depois da migration, eles não devem permanecer como defaults persistentes no banco, porque a autoria clínica deve ser preenchida pela aplicação e não por valores automáticos de fallback do schema.

Este hotfix cria uma migration posterior para remover esses defaults em bancos que já aplicaram a migration anterior.

## 3. Por que editar a migration antiga não resolve bancos já migrados

Migrations do EF Core são controladas pela tabela `__EFMigrationsHistory`. Quando a migration `20260601120000_AddMedicalRecordAuthorship` já foi executada, seu identificador fica registrado nessa tabela.

Por isso, alterações feitas posteriormente no arquivo dessa migration antiga não são executadas de novo nesses ambientes. Mesmo que comandos `DROP DEFAULT` existam no arquivo antigo, bancos que executaram a versão anterior da migration não receberão esses comandos automaticamente.

A correção segura é criar uma nova migration com timestamp posterior, garantindo que o EF Core aplique o hotfix nos ambientes já migrados.

## 4. Migration criada

Foi criada a migration `20260603120000_DropMedicalRecordAuthorshipDefaults`.

No `Up`, a migration remove os defaults persistentes das colunas de autoria técnica que ainda poderiam existir em bancos previamente migrados.

No `Down`, a migration restaura os defaults anteriores apenas para manter reversibilidade.

## 5. Colunas afetadas

A migration afeta exclusivamente as seguintes colunas da tabela `MedicalRecords`:

- `CreatedAt`;
- `CreatedByUserId`;
- `UpdatedByUserId`.

A coluna `UpdatedAt` não é alterada neste hotfix, pois não fazia parte dos defaults persistentes a remover nesta fase.

## 6. Itens explicitamente não alterados

Este hotfix não altera:

- domínio;
- entidade `MedicalRecord`;
- use cases;
- controllers;
- JWT;
- autorização;
- regras de negócio;
- implementação de `AuditLog`.

## 7. Riscos mitigados

A nova migration mitiga os seguintes riscos:

- permanência de `CreatedAt` com data sentinela por default em novos registros criados diretamente no banco;
- permanência de `CreatedByUserId` com `Guid.Empty` por default;
- permanência de `UpdatedByUserId` com `Guid.Empty` por default;
- falsa sensação de correção ao editar uma migration antiga já registrada em `__EFMigrationsHistory`;
- divergência entre ambientes novos e ambientes que aplicaram a migration antes da correção.

## 8. Critérios de aceite

A fase é considerada concluída quando:

- uma nova migration posterior à `20260601120000_AddMedicalRecordAuthorship` existe;
- o `Up` remove os defaults de `CreatedAt`, `CreatedByUserId` e `UpdatedByUserId`;
- o `Down` restaura os defaults anteriores para reversibilidade;
- não há alteração em `MedicalRecord`;
- não há alteração em use cases;
- não há alteração em controllers;
- não há alteração em JWT ou autorização;
- não há implementação de `AuditLog`;
- a documentação da fase existe;
- `git diff --check` passa;
- `dotnet build backend/Togo.sln` passa;
- `dotnet test backend/Togo.sln` passa;
- `dotnet build backend/Togo.sln --configuration Release` passa;
- `dotnet test backend/Togo.sln --configuration Release` passa.

## 9. Próxima fase recomendada

Após este hotfix estar mergeado na `main` com CI verde, a próxima fase recomendada é a **Fase 6.3.4 — Implementação mínima de AuditLog clínico**.

Não avançar para a Fase 6.3.4 antes da conclusão deste hotfix.
