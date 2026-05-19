# TOGO — Fase 4.3.4.1: Correção de tratamento de exceções no CreateAttendanceUseCase

## 1. Objetivo

Esta fase corrige o tratamento de exceções de domínio no `CreateAttendanceUseCase`, garantindo que erros esperados do domínio sejam mapeados corretamente para `ApplicationResult.ValidationError`.

## 2. Problema identificado

Durante a PR 84:
- o CI inicialmente falhou com erro C# `CS0160`;
- `ArgumentOutOfRangeException` herda de `ArgumentException`;
- `catch` de exceção base antes de exceção derivada torna o `catch` da derivada inalcançável;
- a correção inicial compilou, mas deixou apenas `ArgumentOutOfRangeException` no `CreateAttendanceUseCase`.

## 3. Correção aplicada

Foi feita a substituição para `catch (ArgumentException ex)` no `CreateAttendanceUseCase`.

Motivo: capturar os erros esperados de domínio baseados em `ArgumentException`, incluindo exceções derivadas como `ArgumentOutOfRangeException`, sem duplicação de `catch` quando o tratamento é idêntico.

## 4. Teste criado ou reaproveitado

Foi criado um teste adicional no use case para garantir que uma `ArgumentException` de domínio comum também seja convertida em `ApplicationResult.ValidationError`.

## 5. Nota técnica para documentação futura

- Em C#, blocos `catch` devem ir do mais específico para o mais genérico.
- Se o tratamento for idêntico, preferir apenas o `catch` genérico.
- Essa regra deve ser lembrada em futuros use cases.

## 6. Fora do escopo

- Domain;
- Infrastructure;
- API;
- contratos;
- validators;
- repository;
- migrations;
- banco;
- workflow.

## 7. Próxima fase recomendada

Fase 4.3.5 — Criar `GetAttendanceByIdUseCase` e `ListAttendancesUseCase`.
