# Fase 7.4.4 — Autoria e AuditLog mínimos de ClinicalEvolution

## 1. Objetivo

Implementar autoria técnica persistida e AuditLog mínimo para `ClinicalEvolution`, mantendo a superfície pública mínima introduzida na Fase 7.4.3.

## 2. Contexto da Fase 7.4

A Fase 7.4 integra `ClinicalEvolution` com `Attendance` de forma segura e incremental. Esta fase adiciona apenas autoria técnica e auditoria de criação.

## 3. Referências às fases anteriores

- Fase 7.4.1: planejamento técnico da integração `ClinicalEvolution` com `Attendance`.
- Fase 7.4.2: contratos e base técnica, incluindo `ClinicalEvolutionAuditActions`.
- Fase 7.4.3: implementação mínima dos endpoints de listagem por atendimento e criação.

## 4. Campos de autoria adicionados

Foram adicionados à entidade `ClinicalEvolution`:

- `CreatedByUserId`;
- `CreatedAt`;
- `UpdatedByUserId`;
- `UpdatedAt`.

## 5. Decisão sobre `RegisteredAt` vs `CreatedAt`

`RegisteredAt` permanece como timestamp clínico/operacional informado para a evolução. `CreatedAt` é o timestamp técnico de criação/persistência no sistema e deve ser preenchido em UTC pelo fluxo de aplicação.

## 6. Decisão sobre não criar `RegisteredByUserId`

`RegisteredByUserId` não foi criado nesta fase. O projeto já usa `CreatedByUserId`/`CreatedAt` como padrão transversal em `Attendance` e `MedicalRecord`, e `CreatedByUserId` representa o usuário autenticado responsável pelo registro inicial. Criar `RegisteredByUserId` agora duplicaria semântica sem necessidade.

## 7. Evento auditado

Somente o evento `ClinicalEvolution.Created` é gravado após persistência bem-sucedida da evolução clínica.

## 8. Eventos não auditados

Não são gravados nesta fase:

- `ClinicalEvolution.Updated`;
- `ClinicalEvolution.Deleted`;
- `ClinicalEvolution.Read`;
- `ClinicalEvolution.AccessDenied`.

## 9. Metadata permitida

A metadata do evento de criação é mínima e usa PascalCase:

```json
{
  "AttendanceId": 123,
  "Type": "ClinicalNote"
}
```

## 10. Metadata proibida

A auditoria não deve incluir:

- `Text`;
- payload completo do request;
- dados de paciente;
- dados de tutor;
- dados de prontuário;
- evolução clínica textual;
- prescrição;
- observações clínicas completas.

## 11. Arquivos alterados/criados

- `backend/src/Togo.Domain/Entities/ClinicalEvolution.cs`;
- `backend/src/Togo.Application/ClinicalEvolutions/UseCases/CreateClinicalEvolutionUseCase.cs`;
- `backend/src/Togo.Infrastructure/Persistence/Configurations/ClinicalEvolutionConfiguration.cs`;
- `backend/src/Togo.Infrastructure/Migrations/20260621120000_AddClinicalEvolutionAuthorship.cs`;
- `backend/src/Togo.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`;
- testes de domínio, aplicação e infraestrutura relacionados a `ClinicalEvolution`.

## 12. Impacto em domínio

`ClinicalEvolution.Create` passou a exigir usuário criador e timestamp técnico. `UpdateText` foi ajustado para receber usuário e timestamp de atualização para preservar consistência futura, embora não exista fluxo público de update nesta fase.

## 13. Impacto em use case

`CreateClinicalEvolutionUseCase` passou a resolver o usuário atual via `ICurrentUserService`, preencher autoria técnica com `DateTime.UtcNow` e gravar `ClinicalEvolution.Created` via `IClinicalAuditLogWriter` após `AddAsync` bem-sucedido.

## 14. Impacto em EF/migration

A configuração EF marca os quatro campos de autoria como obrigatórios. A migration `AddClinicalEvolutionAuthorship` adiciona os campos à tabela `ClinicalEvolutions`.

## 15. Impacto em testes

Foram ajustados testes de domínio para validar autoria e `UpdateText`; testes de aplicação para validar uso do usuário atual e AuditLog; e testes de infraestrutura para validar persistência/materialização dos campos.

## 16. Ausência de update/delete/listagem global

Não foi implementado update público, delete, soft delete, retificação ou listagem global de `ClinicalEvolution`.

## 17. Ausência de endpoint novo

Nenhum endpoint novo foi criado. Permanecem apenas os endpoints mínimos da Fase 7.4.3.

## 18. Riscos remanescentes

- Ainda não há fluxo público de retificação/assinatura clínica.
- Ainda não há saneamento automático de autoria histórica real para registros legados.
- A auditoria cobre apenas criação.

## 19. Limitações para registros legados

A migration usa `Guid.Empty` como marcador técnico de autoria desconhecida e `1970-01-01T00:00:00Z` como data técnica transitória para registros já existentes. Isso não representa autoria histórica real. A aplicação/domínio impede `Guid.Empty` em novos registros; caso existam dados reais legados, recomenda-se saneamento futuro.

## 20. Fora do escopo

Ficaram fora do escopo: update, delete, soft delete, retificação, assinatura clínica, anexos, listagem global, novos endpoints, alterações de autorização, auditoria de leitura/acesso negado, frontend e infraestrutura operacional.

## 21. Critérios de aceite

- Autoria mínima persistida em `ClinicalEvolution`;
- `CreateClinicalEvolutionUseCase` usa `ICurrentUserService`;
- `ClinicalEvolution.Created` é gravado após criação bem-sucedida;
- AuditLog não inclui `Text` nem payload clínico sensível;
- Migration criada;
- Testes ajustados para domínio, aplicação e infraestrutura;
- Ausência de update/delete/listagem global e endpoint novo.

## 22. Próxima fase recomendada

Fase 7.4.5 — Testes e evidências da integração ClinicalEvolution.
