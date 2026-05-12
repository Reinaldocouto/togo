# TOGO — Fase 3.9: Validação manual do fluxo Pet

## 1. Objetivo da fase

A Fase 3.9 teve como objetivo validar manualmente o CRUD de Pet/Patient via Postman/Swagger, usando a API local e conferindo o comportamento real do fluxo público exposto pelo `PetsController`.

A validação cobriu os seguintes pontos:

- autenticação;
- autorização;
- listagem;
- busca por PatientId;
- criação;
- validação de microchip duplicado;
- validação de Tutor inexistente;
- atualização;
- exclusão;
- comportamento após exclusão;
- logs seguros;
- funcionamento real com banco de dados.

## 2. Contexto da validação

A validação manual foi executada com a API rodando localmente. O endpoint de login foi usado para gerar um JWT válido, e os endpoints protegidos foram testados com `Bearer Token`.

Os testes foram feitos contra o banco local. Os bugs encontrados apareceram apenas durante a validação manual com EF Core executando queries contra banco real, não durante a validação de build/test automatizado já existente.

Comando usado para subir a API:

```bash
dotnet run --project backend/src/Togo.Api/Togo.Api.csproj
```

Endpoints do fluxo Pet/Patient validados:

- `GET /api/pets`;
- `GET /api/pets/{patientId}`;
- `POST /api/pets`;
- `PUT /api/pets/{patientId}`;
- `DELETE /api/pets/{patientId}`.

## 3. Validação local de build/test

O desenvolvedor humano executou localmente o build da solution:

```bash
dotnet build backend/Togo.sln
```

Resultado:

- build com sucesso.

Também foi executada a suíte de testes automatizados existente:

```bash
dotnet test backend/Togo.sln
```

Resultado:

- total: 45 testes;
- falhou: 0;
- bem-sucedido: 45;
- ignorado: 0.

## 4. Bug encontrado na listagem de Pets — PR 56

Endpoint afetado:

```http
GET /api/pets
```

Comportamento observado:

- com token válido, o endpoint retornava `500 Internal Server Error`.

Causa:

- erro de tradução LINQ/EF Core;
- o método `ListAsync` fazia ordenação sobre propriedade da projection `PetListItemProjection`;
- EF Core não conseguiu traduzir a query para SQL.

Correção aplicada na PR 56:

- ordenar por `patient.Name` antes da projection;
- manter `AsNoTracking`;
- manter `PetListItemProjection`;
- manter `PatientId` como identificador público;
- não introduzir `PetId`.

Arquivo corrigido:

```text
backend/src/Togo.Infrastructure/Repositories/PetRepository.cs
```

Resultado após correção:

- `GET /api/pets` passou a retornar `200 OK`.

## 5. Bug encontrado na busca por PatientId — PR 57

Endpoint afetado:

```http
GET /api/pets/{patientId}
```

Comportamento observado:

- `GET /api/pets/1` retornava `500 Internal Server Error`.

Causa:

- erro de tradução LINQ/EF Core;
- o método `GetByPatientIdAsync` filtrava sobre propriedade da projection `PetDetailsProjection`;
- EF Core não conseguiu traduzir o filtro para SQL.

Correção aplicada na PR 57:

- filtrar por `patient.Id` antes da projection;
- manter `AsNoTracking`;
- manter `PetDetailsProjection`;
- manter `PatientId` como identificador público;
- não introduzir `PetId`.

Arquivo corrigido:

```text
backend/src/Togo.Infrastructure/Repositories/PetRepository.cs
```

Resultado após correção:

- `GET /api/pets/1` passou a retornar `200 OK`.

## 6. Checklist manual executado

| Caso | Endpoint | Esperado | Resultado final | Status |
| --- | --- | --- | --- | --- |
| 1 | POST /api/auth/login | 200 OK e token JWT | 200 OK e token retornado | OK |
| 2 | GET /api/pets sem token | 401 Unauthorized | 401 Unauthorized | OK |
| 3 | GET /api/pets com token | 200 OK | 200 OK após PR 56 | OK |
| 4 | POST /api/pets com Tutor válido | 201 Created | 201 Created | OK |
| 5 | GET /api/pets/1 | 200 OK | 200 OK após PR 57 | OK |
| 6 | POST /api/pets com microchip duplicado | 409 Conflict | 409 Conflict | OK |
| 7 | POST /api/pets com Tutor inexistente | 404 Not Found | 404 Not Found | OK |
| 8 | PUT /api/pets/1 | 200 OK | 200 OK | OK |
| 9 | GET /api/pets/999999 | 404 Not Found | 404 Not Found | OK |
| 10 | DELETE /api/pets/999999 | 404 Not Found | 404 Not Found | OK |
| 11 | DELETE /api/pets/1 | 204 No Content | 204 No Content | OK |
| 12 | GET /api/pets/1 após delete | 404 Not Found | 404 Not Found | OK |

## 7. Resultados importantes observados

- Login válido retornou token JWT.
- `GET /api/pets` sem token retornou `401 Unauthorized`.
- `GET /api/pets` com token retornou lista.
- `POST /api/pets` criou Patient + Pet.
- `GET /api/pets/{patientId}` retornou o Pet correto após correção da PR 57.
- Microchip duplicado retornou `409 Conflict`.
- Tutor inexistente retornou `404 Not Found`.
- `PUT /api/pets/{patientId}` atualizou o Pet.
- `GET` de PatientId inexistente retornou `404 Not Found`.
- `DELETE` de PatientId inexistente retornou `404 Not Found`.
- `DELETE` de Pet existente retornou `204 No Content`.
- `GET` após delete retornou `404 Not Found`.

## 8. Logs e segurança

Os logs foram observados no terminal da API durante a validação manual.

Os logs esperados apareceram com:

- `PatientId`;
- `TutorId`;
- `HasMicrochip`;
- `Count`;
- comandos SQL parametrizados do EF Core.

Atenções importantes de segurança observadas:

- os logs próprios da aplicação não registraram payload completo;
- os logs próprios da aplicação não registraram token JWT;
- os logs próprios da aplicação não registraram senha;
- os logs próprios da aplicação não registraram microchip completo/parcial diretamente nos logs de aplicação.

Observação técnica:

O EF Core em ambiente Development registrou comandos SQL e parâmetros mascarados como `?`. Esse comportamento é aceitável em desenvolvimento, mas deverá ser reavaliado futuramente ao configurar logs de produção, Serilog e níveis de log.

## 9. Status final da Fase 3.9

A Fase 3.9 foi concluída com sucesso após duas correções técnicas:

- PR 56 corrigiu `ListAsync`;
- PR 57 corrigiu `GetByPatientIdAsync`.

O CRUD Pet/Patient está funcional ponta a ponta para:

- autenticação;
- autorização;
- listagem;
- criação;
- busca;
- validação de duplicidade;
- validação de Tutor inexistente;
- atualização;
- exclusão;
- busca após exclusão.

A fase encontrou bugs reais que build/test não capturaram, reforçando a importância da validação manual contra banco real.

## 10. Débitos técnicos e observações futuras

### 10.1. GetDetailsQuery

- O método privado `GetDetailsQuery` ainda existe no `PetRepository`.
- Após a correção do `GetByPatientIdAsync`, ele pode ter ficado sem uso.
- Não remover nesta fase.
- Avaliar em cleanup futuro.

### 10.2. Delete físico

- O delete de Pet/Patient está funcionando.
- Porém, delete físico continua sendo uma decisão provisória.
- Deve ser reavaliado antes da implementação de Atendimento/Prontuário.

### 10.3. Testes automatizados

- A validação manual passou.
- Ainda é recomendável criar testes automatizados para use cases/controller/repository em fase futura.

### 10.4. Logs de produção

- Logs em Development mostram SQL do EF Core.
- Em fase futura, avaliar configuração de Serilog/arquivo e nível de log por ambiente.

## 11. Fechamento

Fase 3.9 concluída.

Resultado:

- build local com sucesso;
- 45/45 testes passando;
- checklist manual completo aprovado;
- bugs encontrados foram corrigidos;
- endpoints de Pet funcionando.

Próxima fase recomendada:

**Fase 3.10 — Criar testes automatizados do fluxo Pet.**
