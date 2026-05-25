# TOGO — Fase 5.5.4: Validação Swagger/HTTP manual de MedicalRecord

## Subfase 5.5 — API MedicalRecord

Planejamento:
- 5.5.1 — Planejamento da API MedicalRecord.
- 5.5.2 — Controller MedicalRecord / rotas orientadas por Patient.
- 5.5.3 — Testes de API MedicalRecord.
- 5.5.4 — Validação Swagger/HTTP manual.
- 5.5.5 — Documentação final da API MedicalRecord.
- 5.5.x.y — Correções pontuais, bugs, ajustes complementares ou documentação adicional, quando necessário.

## Objetivo

Esta fase documenta a validação manual dos endpoints de `MedicalRecord` via Swagger, Postman, Insomnia ou cliente HTTP (`curl`), complementando os testes automatizados da fase 5.5.3.

O objetivo é fornecer um roteiro reproduzível para validar:
- autenticação;
- criação de prontuário;
- consulta de prontuário;
- atualização de prontuário;
- duplicidade;
- patient inexistente;
- prontuário inexistente;
- payload nulo;
- payload em branco;
- proteção de endpoints sem token.

## Contexto

- O controller `MedicalRecordsController` já existe.
- Os testes de API de `MedicalRecord` já existem.
- O pipeline de CI da fase anterior foi considerado aprovado para avanço da subfase.
- Os endpoints são protegidos por `[Authorize]`.
- `MedicalRecord` pertence ao `Patient`.
- `patientId` é recebido pela rota.
- Dados clínicos são sensíveis.
- Esta fase não implementa código; é exclusivamente documental.

## Pré-requisitos para validação manual

- aplicação rodando localmente;
- banco local configurado;
- migrations aplicadas no ambiente local, se necessário;
- usuário autenticável disponível;
- token JWT válido;
- Swagger acessível, se habilitado;
- alternativa com Postman/Insomnia/curl;
- pelo menos um `Patient/Pet` existente para testar fluxo positivo.

> Observação: os comandos exatos (subir API, autenticar, seed local, etc.) podem variar conforme o ambiente local.

## Endpoints a validar

Endpoints do MVP:
- `GET /api/patients/{patientId}/medical-record`
- `POST /api/patients/{patientId}/medical-record`
- `PUT /api/patients/{patientId}/medical-record`

Endpoints que **não existem** no MVP:
- `GET /api/medical-records/{id}`
- `GET /api/medical-records`
- `DELETE /api/patients/{patientId}/medical-record`

## Payloads de exemplo

> Todos os exemplos abaixo são fictícios e não representam dado clínico real.

### POST válido

```json
{
  "generalNotes": "Paciente apresenta histórico clínico inicial fictício.",
  "flagsJson": "{\"risk\":false,\"requiresFollowUp\":true}"
}
```

### POST com campos nulos

```json
{
  "generalNotes": null,
  "flagsJson": null
}
```

### POST/PUT com campos em branco

```json
{
  "generalNotes": "   ",
  "flagsJson": "   "
}
```

### PUT válido

```json
{
  "generalNotes": "Prontuário atualizado com observação fictícia.",
  "flagsJson": "{\"risk\":true,\"requiresFollowUp\":true}"
}
```

## Roteiro positivo de validação

1. Autenticar usuário e obter token JWT.
2. Criar ou identificar um `Patient/Pet` válido.
3. Confirmar `patientId`.
4. Executar `POST` para criar `MedicalRecord`.
5. Validar status `201`.
6. Validar `Location` header, se retornado.
7. Validar corpo com `Id`, `PatientId`, `GeneralNotes`, `FlagsJson`, `UpdatedAt`.
8. Executar `GET` para consultar `MedicalRecord`.
9. Validar status `200`.
10. Executar `PUT` para atualizar `MedicalRecord`.
11. Validar status `200`.
12. Executar `GET` novamente.
13. Confirmar dados atualizados.
14. Executar segundo `POST` para o mesmo `patientId`.
15. Confirmar status `409 Conflict`.

## Roteiro negativo de validação

- `GET` sem token -> `401`.
- `POST` sem token -> `401`.
- `PUT` sem token -> `401`.
- `GET` com `patientId = 0` -> `400`.
- `POST` com `patientId = 0` -> `400`.
- `PUT` com `patientId = 0` -> `400`.
- `GET` com patient inexistente -> `404`.
- `POST` com patient inexistente -> `404`.
- `PUT` com patient inexistente -> `404`.
- `GET` com patient existente sem prontuário -> `404`.
- `PUT` com patient existente sem prontuário -> `404`.
- `POST` duplicado para patient com prontuário -> `409`.
- `POST` com campos nulos -> `201`.
- `POST` com campos em branco -> `201` com valores normalizados para `null`.
- `PUT` com campos em branco -> `200` com valores normalizados para `null`.

## Respostas esperadas

| Cenário | Método | Endpoint | Status esperado | Observação |
|---|---|---|---|---|
| Buscar prontuário existente | GET | `/api/patients/{patientId}/medical-record` | 200 | Retorna `MedicalRecordResponse` |
| Criar prontuário válido | POST | `/api/patients/{patientId}/medical-record` | 201 | Pode retornar `Location` |
| Atualizar prontuário existente | PUT | `/api/patients/{patientId}/medical-record` | 200 | Retorna dados atualizados |
| Sem token (GET) | GET | `/api/patients/{patientId}/medical-record` | 401 | Endpoint protegido por `[Authorize]` |
| Sem token (POST) | POST | `/api/patients/{patientId}/medical-record` | 401 | Endpoint protegido por `[Authorize]` |
| Sem token (PUT) | PUT | `/api/patients/{patientId}/medical-record` | 401 | Endpoint protegido por `[Authorize]` |
| `patientId = 0` (GET) | GET | `/api/patients/0/medical-record` | 400 | Validação de entrada |
| `patientId = 0` (POST) | POST | `/api/patients/0/medical-record` | 400 | Validação de entrada |
| `patientId = 0` (PUT) | PUT | `/api/patients/0/medical-record` | 400 | Validação de entrada |
| Patient inexistente (GET) | GET | `/api/patients/{patientId}/medical-record` | 404 | Patient não encontrado |
| Patient inexistente (POST) | POST | `/api/patients/{patientId}/medical-record` | 404 | Patient não encontrado |
| Patient inexistente (PUT) | PUT | `/api/patients/{patientId}/medical-record` | 404 | Patient não encontrado |
| Prontuário inexistente para patient existente (GET) | GET | `/api/patients/{patientId}/medical-record` | 404 | MedicalRecord não encontrado |
| Prontuário inexistente para patient existente (PUT) | PUT | `/api/patients/{patientId}/medical-record` | 404 | MedicalRecord não encontrado |
| POST duplicado para patient com prontuário | POST | `/api/patients/{patientId}/medical-record` | 409 | Regra de unicidade lógica por patient |
| POST com campos nulos | POST | `/api/patients/{patientId}/medical-record` | 201 | Campos aceitos como `null` |
| POST com campos em branco | POST | `/api/patients/{patientId}/medical-record` | 201 | Campos normalizados para `null` |
| PUT com campos em branco | PUT | `/api/patients/{patientId}/medical-record` | 200 | Campos normalizados para `null` |

## Validação de segurança

Checklist:
- [ ] endpoint sem token retorna `401`;
- [ ] endpoint com token inválido retorna `401`;
- [ ] nenhum endpoint é público;
- [ ] não há listagem ampla de prontuários;
- [ ] não há endpoint de delete;
- [ ] response não retorna dados fora do contrato `MedicalRecordResponse`;
- [ ] logs não devem expor `GeneralNotes`/`FlagsJson`.

## Validação de privacidade

- usar apenas dados fictícios;
- não usar dados clínicos reais;
- não usar CPF, e-mail real, telefone real ou dados reais de tutores;
- não copiar payloads reais para documentação;
- não registrar token JWT em documentação;
- não salvar prints com dados sensíveis.

## Validação Swagger/OpenAPI

Checklist:
- [ ] controller aparece no Swagger;
- [ ] rota GET aparece corretamente;
- [ ] rota POST aparece corretamente;
- [ ] rota PUT aparece corretamente;
- [ ] parâmetros de route aparecem como `patientId`;
- [ ] request bodies aparecem corretamente;
- [ ] status codes aparecem conforme `ProducesResponseType`;
- [ ] endpoints mostram cadeado/autorização, se Swagger suportar;
- [ ] não há exemplos sensíveis.

## Registro de evidências manuais

Modelo sugerido:

- Data:
- Ambiente:
- Branch/commit:
- Usuário de teste:
- PatientId usado:
- Endpoint:
- Método:
- Payload:
- Status obtido:
- Resultado esperado:
- Resultado real:
- Aprovado? Sim/Não
- Observações:

> Tokens e dados sensíveis devem ser mascarados antes de registrar evidências.

## Pontos de atenção

- validação manual não substitui testes automatizados;
- testes de API já existem;
- roles/permissões finas seguem fora do escopo;
- Soft Delete e AuditLog seguem fora do escopo;
- índice único em `PatientId` ainda não existe;
- validação real depende de dados locais corretos;
- Swagger pode variar conforme ambiente.

## Critérios de aceite da Fase 5.5.4

A fase será considerada concluída se:
- [x] documento de validação manual for criado;
- [x] endpoints forem listados;
- [x] payloads de exemplo forem definidos;
- [x] roteiro positivo for definido;
- [x] roteiro negativo for definido;
- [x] respostas esperadas forem documentadas;
- [x] checklist de segurança for definido;
- [x] checklist de privacidade for definido;
- [x] checklist Swagger/OpenAPI for definido;
- [x] modelo de evidência for criado;
- [x] nenhuma implementação for feita;
- [x] nenhum código/teste/migration/banco for alterado.

## Fora do escopo

Esta fase não implementa:
- código;
- testes;
- controller;
- novos endpoints;
- Swagger attributes;
- migration;
- database update;
- banco real;
- Soft Delete;
- AuditLog;
- índice único;
- roles/permissões finas;
- Redis;
- RabbitMQ;
- Docker;
- frontend.

## Próxima fase recomendada

**Fase 5.5.5 — Documentação final da API MedicalRecord.**

Objetivo:
Consolidar planejamento, controller, testes de API, validação manual Swagger/HTTP, riscos, débitos técnicos e autorização para encerrar a subfase 5.5.

## Validações obrigatórias

Executar nesta fase:
- `git branch --show-current`
- `git status --short`
- `git diff --check`

Se `dotnet` estiver disponível:
- `dotnet build backend/Togo.sln`
- `dotnet test backend/Togo.sln`

Não inferir resultados: registrar apenas saída real dos comandos no ambiente executado.
