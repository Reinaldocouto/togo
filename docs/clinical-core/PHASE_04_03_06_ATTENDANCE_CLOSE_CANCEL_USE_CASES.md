# PHASE 04.03.06 — Attendance Close/Cancel Use Cases (Audit Addendum)

## Notas de auditoria e estabilização dos testes

- PR 84 falhou por ordem incorreta de `catch` (exceção base antes da derivada), gerando tratamento inconsistente.
- PR 85 expôs cuidado com uso de `default` em parâmetro nullable: quando a intenção é `DateTime.MinValue`, o teste deve deixar isso explícito com `default(DateTime)` ou variável tipada.
- PR 86 expôs cuidado com `Attendance.Id = 0` em entidades criadas por factory (`Attendance.Create(...)`) e a necessidade de lookup artificial em testes de aplicação.
- PR 86 também mostrou que build/test não substituem `git diff --check`, que detecta trailing whitespace.
- PR 87 expôs desalinhamento entre mensagens esperadas nos testes e mensagens reais do domínio/use case.
- A correção definitiva desta trilha foi auditar testes, fake repository, mensagens esperadas, lookup artificial por id e tratamento de exceções esperado.

### Regras práticas para próximos testes

- Não usar `Id` não persistido como chave de lookup em testes de aplicação.
- Não usar `default` solto em nullable quando a intenção for `DateTime.MinValue`.
- Não escrever assertion de mensagem sem conferir a origem real (domínio, validator ou use case).
- Executar `git diff --check` antes do commit para prevenir whitespace residual.
- Rodar `dotnet build` e `dotnet test` localmente quando possível antes de abrir PR.
