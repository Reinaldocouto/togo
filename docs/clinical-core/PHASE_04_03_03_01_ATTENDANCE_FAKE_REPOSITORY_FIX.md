# TOGO — Fase 4.3.3.1: Correção do FakeAttendanceRepository

## 1. Objetivo

Esta fase corrige o fake de testes de Attendance antes da implementação do `CreateAttendanceUseCase`, evitando comportamento enganoso em cenários com entidades novas sem chave persistida.

## 2. Problema identificado

O fake armazenava itens por `Attendance.Id` em dicionário. Como entidades novas criadas por `Attendance.Create(...)` começam com `Id = 0`, múltiplos `AddAsync` sobrescreviam a mesma entrada e ocultavam problemas em testes futuros de listagem, duplicidade e atendimento aberto.

## 3. Correção aplicada

Foi adotada uma lista interna (`List<Attendance>`) para armazenamento no fake, removendo dependência do `Id` no `AddAsync`.

Ajustes realizados:
- `GetByIdAsync`: busca por `a.Id == id` na lista;
- `ListAsync`: retorna cópia da lista (`ToList()`);
- `ListByPatientIdAsync`: filtra a lista por `PatientId`;
- `AddAsync`: sempre adiciona novo item, sem sobrescrever;
- `UpdateAsync`: substitui por `Id` quando encontrado; caso não encontrado, adiciona o item.

## 4. Teste adicionado

Foi adicionado o teste:
- `AddAsync_ShouldNotOverwriteAttendances_WhenEntitiesHaveDefaultId`

O teste valida que dois `Attendance` recém-criados (com `Id` default) permanecem distintos após dois `AddAsync`, e que `ListAsync` retorna 2 itens.

## 5. Fora do escopo

Não houve alterações em:
- validators de produção;
- use cases;
- Domain;
- Infrastructure;
- API;
- migrations;
- banco;
- workflow.

## 6. Próxima fase recomendada

Fase 4.3.4 — Criar `CreateAttendanceUseCase`.
