# TOGO — Fase 6.5.3: Validação estrutural inicial de FlagsJson em MedicalRecord

## 1. Objetivo

Implementar a validação estrutural inicial de `MedicalRecord.FlagsJson`, mantendo o campo opcional e impedindo que novos fluxos de criação ou atualização persistam JSON inválido, arrays na raiz ou valores escalares na raiz.

## 2. Contexto da Fase 6.5

A Fase 6.5 trata integridade e evolução estrutural da vertical `MedicalRecord`. As fases anteriores desta trilha já cobriram o planejamento de integridade, a unicidade física de `MedicalRecords.PatientId` e o tratamento do conflito concorrente da constraint física.

## 3. Referência ao MR-DEBT-009

Esta fase trata o débito `MR-DEBT-009 — FlagsJson flexível`, registrado como risco de modelagem/validação porque o campo aceitava string livre sem validação estrutural de JSON.

## 4. Estado anterior de FlagsJson

Antes desta fase, `FlagsJson` era uma `string?` normalizada apenas por ausência (`null`, vazio ou whitespace) e `Trim` para valores não vazios. Strings livres, JSON malformado, arrays e escalares JSON poderiam chegar ao domínio e ser persistidos como texto.

## 5. Decisão técnica

A decisão adotada foi validar `FlagsJson` no domínio, usando `System.Text.Json` do .NET 8, sem pacote externo, sem migration e sem alteração do tipo da coluna.

## 6. Regra estrutural adotada

`FlagsJson` é opcional. Quando informado com conteúdo não vazio, deve ser JSON válido e o elemento raiz deve ser um objeto JSON. O valor persistido é a string aparada nas extremidades, preservando a formatação interna original.

## 7. Camada responsável pela validação

A validação fica em `Togo.Domain.Entities.MedicalRecord`, porque `FlagsJson` compõe o estado da entidade e a regra deve valer igualmente para qualquer fluxo de criação ou atualização, sem depender de ASP.NET Core, EF Core ou provider de banco.

## 8. Valores aceitos

São aceitos:

- `null`;
- string vazia;
- whitespace;
- `{}`;
- objeto simples, como `{"risk":true}`;
- objeto aninhado;
- objeto com arrays como valores;
- objeto JSON com espaços externos, persistido sem esses espaços externos.

## 9. Valores rejeitados

São rejeitados:

- JSON malformado;
- `{`;
- objeto incompleto;
- array na raiz;
- string JSON na raiz;
- número na raiz;
- booleano na raiz;
- literal JSON `null`;
- trailing comma;
- comentários JSON;
- qualquer sintaxe recusada pelo parser JSON estrito padrão.

## 10. Comportamento de null, vazio e whitespace

`null`, string vazia e whitespace continuam representando ausência de flags e são normalizados para `null`.

## 11. Garantia de atomicidade no update

O update normaliza e valida os valores em variáveis locais antes de alterar a entidade. Se `FlagsJson` for inválido, `GeneralNotes`, `FlagsJson`, `UpdatedByUserId`, `UpdatedAt`, `CreatedByUserId`, `CreatedAt`, `PatientId` e `Id` permanecem inalterados.

## 12. Impacto em create

Na criação, `MedicalRecord.Create` passa a validar estruturalmente `FlagsJson` antes de produzir uma entidade válida. Falhas geram `ArgumentException` com mensagem genérica e segura.

## 13. Impacto em update

Na atualização, `UpdateNotes` usa a mesma regra de criação. Falhas impedem mutação em memória e, consequentemente, impedem persistência e auditoria nos casos de uso.

## 14. Comportamento HTTP 400

Os casos de uso continuam convertendo `ArgumentException` de domínio para `ApplicationResultType.ValidationError`. O controller mantém o mapeamento existente de `ValidationError` para `400 Bad Request`.

## 15. Segurança e minimização de logs

A mensagem de erro é estável e genérica: `FlagsJson must be a valid JSON object.`. O payload recebido não é incluído na exceção, nos logs ou na resposta. Logs seguem limitados a dados operacionais mínimos, como `PatientId`.

## 16. Ausência de payload no AuditLog

O AuditLog permanece com metadata mínima de `PatientId`. `FlagsJson` e `GeneralNotes` não são copiados para `MetadataJson`. Em falha de validação, nenhum evento `MedicalRecord.Created` ou `MedicalRecord.Updated` é escrito.

## 17. Testes criados e alterados

Foram adicionadas e ajustadas coberturas de domínio, Application e API para:

- aceitação de ausência e de objeto JSON válido;
- rejeição de JSON inválido, arrays e escalares na raiz;
- preservação de estado em update inválido;
- conversão para `ValidationError`/HTTP 400;
- ausência de chamada de persistência em falha;
- ausência de AuditLog em falha;
- ausência de eco de payload sensível em mensagens e logs;
- manutenção dos fluxos válidos existentes.

## 18. Confirmação de ausência de migration

Nenhuma migration foi criada ou alterada nesta fase.

## 19. Confirmação de ausência de alteração de schema

Não houve alteração em `AppDbContext`, `MedicalRecordConfiguration`, repositories, tipo de coluna, índice único ou constraints físicas. A coluna segue persistindo `FlagsJson` como texto.

## 20. Riscos remanescentes

Permanecem fora da mitigação desta fase:

- ausência de schema semântico;
- ausência de allowlist de chaves;
- dados legados inválidos não saneados automaticamente;
- ausência de normalização relacional;
- ausência de indexação interna do JSON;
- propriedades duplicadas não tratadas especificamente;
- interoperabilidade futura dependente de contrato clínico mais estável.

## 21. Fora do escopo

Não foram implementados JSON Schema, nova entidade, nova tabela, mudança de coluna, canonicalização, reserialização, ordenação de propriedades, endpoint novo, alteração de repository, alteração de índice único, frontend, Docker, Redis, RabbitMQ ou Kubernetes.

## 22. Critérios de aceite

A fase é aceita quando `null`/vazio/whitespace viram `null`, objetos JSON válidos são aceitos, JSON inválido e raízes não objeto são rejeitados, create e update usam a mesma regra, update inválido é atômico, falhas não persistem nem auditam, POST/PUT inválidos retornam 400, não há migration/schema change/pacote externo e os testes Debug/Release são executados.

## 23. Decisão final sobre MR-DEBT-009

`MR-DEBT-009` fica resolvido tecnicamente no escopo inicial de validação estrutural de `FlagsJson`. A solução mitiga a persistência de novos valores estruturalmente inválidos, mantendo riscos remanescentes documentados para evolução posterior.

## 24. Próxima fase recomendada

A próxima fase recomendada é `6.5.4 — Evidências finais, atualização do registro vivo e encerramento da Fase 6.5`.
