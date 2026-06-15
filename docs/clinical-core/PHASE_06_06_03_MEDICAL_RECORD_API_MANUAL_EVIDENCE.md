# Fase 6.6.3 — Evidências manuais versionadas de API/Swagger MedicalRecord

## 1. Objetivo

Versionar um roteiro de evidência manual da API `MedicalRecord`, com cenários Swagger/HTTP, requests e responses esperados, dados sintéticos e observações de segurança. Esta fase formaliza a rastreabilidade manual esperada para a vertical sem alterar implementação, contracts, testes, banco, migrations ou infraestrutura.

## 2. Contexto da Fase 6.6

A Fase 6.6 faz parte do hardening operacional da vertical `MedicalRecord` e vem após avanços incrementais de:

- segurança/autorização granular por perfil;
- autoria de criação e atualização;
- `AuditLog` clínico mínimo;
- `Soft Delete`;
- decisão de retenção clínica inicial;
- unicidade física de prontuário por paciente;
- validação estrutural de `FlagsJson`;
- propagação de `CancellationToken` até repository/EF Core.

A fase anterior direta foi a 6.6.2, documentada em `docs/clinical-core/PHASE_06_06_02_MEDICAL_RECORD_CANCELLATION_TOKEN_PROPAGATION.md`.

## 3. Referência ao MR-DEBT-011

Débito tratado:

```text
MR-DEBT-011 — Evidências manuais Swagger não versionadas formalmente
```

## 4. Natureza da evidência

Este documento é um **roteiro versionado de evidência manual com resultados esperados** para Swagger/HTTP. A API não foi executada manualmente nesta fase; portanto, este documento não declara execução real em ambiente local.

A evidência:

- não substitui testes automatizados;
- complementa testes de API, Application, Domain e Infrastructure;
- usa somente dados fictícios e sanitizados;
- usa placeholders para tokens JWT;
- registra responses esperados e sanitizados;
- documenta limitações de ambiente e variações possíveis de headers, timestamps e envelopes de erro.

## 5. Endpoints cobertos

Endpoints confirmados no controller `MedicalRecordsController`:

```http
GET /api/patients/{patientId}/medical-record
POST /api/patients/{patientId}/medical-record
PUT /api/patients/{patientId}/medical-record
```

Não há endpoint público `DELETE` no controller atual. Delete/restore/consulta administrativa permanecem fora do escopo desta evidência.

Arquivo solicitado ausente durante a inspeção:

```text
backend/src/Togo.Domain/Security/MedicalRecordPermissions.cs
```

A implementação atual de permissões foi encontrada em:

```text
backend/src/Togo.Application/Security/MedicalRecordPermissions.cs
```

## 6. Autorização

Todos os endpoints do controller exigem autenticação (`[Authorize]`). Além disso, cada operação exige policy específica:

| Operação | Policy | Perfis autorizados confirmados | Perfis não autorizados confirmados |
| --- | --- | --- | --- |
| GET | `MedicalRecord.Read` | `Admin`, `Veterinarian`, `Assistant` | `Reception`, `ReadOnly`, token sem claim de profile |
| POST | `MedicalRecord.Create` | `Admin`, `Veterinarian` | `Assistant`, `Reception`, `ReadOnly`, token sem claim de profile |
| PUT | `MedicalRecord.Update` | `Admin`, `Veterinarian` | `Assistant`, `Reception`, `ReadOnly`, token sem claim de profile |

Comportamentos esperados:

- sem header `Authorization`: `401 Unauthorized`;
- token autenticado sem permissão/profile válido: `403 Forbidden`;
- token autenticado sem claim `togo:profile`: `403 Forbidden`.

Placeholders permitidos nos exemplos:

```http
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_ADMIN>
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_VETERINARIAN>
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_ASSISTANT>
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_RECEPTION>
Authorization: Bearer <JWT_VALIDO_SEM_PROFILE_CLAIM>
```

Nenhum token real deve ser registrado neste documento ou em evidências futuras.

## 7. Dados fictícios usados

Todos os dados abaixo são sintéticos:

| Dado | Valor fictício |
| --- | --- |
| `PatientId` válido com prontuário | `1001` |
| `PatientId` válido sem prontuário | `1002` |
| `PatientId` inexistente | `999999` |
| `PatientId` inválido | `0` |
| Tutor | `Tutor Demo` |
| Paciente | `Pet Demo` |
| Usuário | `Usuario Demo` |
| Id de prontuário | `5001` |
| Timestamp base | `2026-01-01T12:00:00Z` |

## 8. Cenários mínimos obrigatórios

### Cenário 1 — GET prontuário existente

Pré-condição: paciente `1001` existe e possui prontuário ativo.

```http
GET /api/patients/1001/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_VETERINARIAN>
Accept: application/json
```

Status esperado:

```http
HTTP/1.1 200 OK
Content-Type: application/json
```

Response esperado sanitizado:

```json
{
  "id": 5001,
  "patientId": 1001,
  "generalNotes": "Paciente demo estavel para evidencia manual.",
  "flagsJson": "{\"risk\":\"low\"}",
  "updatedAt": "2026-01-01T12:00:00Z"
}
```

### Cenário 2 — GET paciente existente sem prontuário

Pré-condição: paciente `1002` existe, mas não possui prontuário ativo.

```http
GET /api/patients/1002/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_VETERINARIAN>
Accept: application/json
```

Status esperado:

```http
HTTP/1.1 404 Not Found
Content-Type: application/json
```

Response esperado:

```json
{
  "message": "Medical record not found."
}
```

### Cenário 3 — GET paciente inexistente

```http
GET /api/patients/999999/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_VETERINARIAN>
Accept: application/json
```

Status esperado:

```http
HTTP/1.1 404 Not Found
Content-Type: application/json
```

Response esperado:

```json
{
  "message": "Patient not found."
}
```

### Cenário 4 — GET com PatientId inválido

```http
GET /api/patients/0/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_VETERINARIAN>
Accept: application/json
```

Status esperado:

```http
HTTP/1.1 400 Bad Request
Content-Type: application/json
```

Response esperado:

```json
{
  "message": "Patient id is invalid."
}
```

### Cenário 5 — POST válido

Pré-condição: paciente `1002` existe e ainda não possui prontuário.

```http
POST /api/patients/1002/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_VETERINARIAN>
Content-Type: application/json
Accept: application/json
```

```json
{
  "generalNotes": "Paciente demo estavel para evidencia manual.",
  "flagsJson": "{\"risk\":\"low\"}"
}
```

Status esperado:

```http
HTTP/1.1 201 Created
Location: /api/patients/1002/medical-record
Content-Type: application/json
```

Response esperado:

```json
{
  "id": 5002,
  "patientId": 1002,
  "generalNotes": "Paciente demo estavel para evidencia manual.",
  "flagsJson": "{\"risk\":\"low\"}",
  "updatedAt": "2026-01-01T12:00:00Z"
}
```

Observação: `FlagsJson` informado com espaços externos deve retornar normalizado, sem espaços externos, mantendo objeto JSON válido.

### Cenário 6 — POST com `FlagsJson` inválido

Pré-condição: paciente `1003` existe e ainda não possui prontuário.

```http
POST /api/patients/1003/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_VETERINARIAN>
Content-Type: application/json
Accept: application/json
```

```json
{
  "generalNotes": "Nota sintetica que nao deve ser ecoada em erro.",
  "flagsJson": "{\"risk\":\"low\""
}
```

Status esperado:

```http
HTTP/1.1 400 Bad Request
Content-Type: application/json
```

Response esperado sanitizado:

```json
{
  "message": "FlagsJson must be a valid JSON object."
}
```

A resposta não deve ecoar `generalNotes`, `flagsJson` malformado, payload clínico ou qualquer valor sensível.

### Cenário 7 — POST com array na raiz de `FlagsJson`

Pré-condição: paciente `1004` existe e ainda não possui prontuário.

```http
POST /api/patients/1004/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_VETERINARIAN>
Content-Type: application/json
Accept: application/json
```

```json
{
  "generalNotes": "Paciente demo sem flags validas.",
  "flagsJson": "[]"
}
```

Status esperado:

```http
HTTP/1.1 400 Bad Request
Content-Type: application/json
```

Response esperado sanitizado:

```json
{
  "message": "FlagsJson must be a valid JSON object."
}
```

### Cenário 8 — POST duplicado

Pré-condição: paciente `1001` já possui prontuário ativo.

```http
POST /api/patients/1001/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_VETERINARIAN>
Content-Type: application/json
Accept: application/json
```

```json
{
  "generalNotes": "Tentativa duplicada sintetica.",
  "flagsJson": "{\"risk\":\"low\"}"
}
```

Status esperado:

```http
HTTP/1.1 409 Conflict
Content-Type: application/json
```

Response esperado:

```json
{
  "message": "Patient already has a medical record."
}
```

### Cenário 9 — PUT válido

Pré-condição: paciente `1001` possui prontuário ativo.

```http
PUT /api/patients/1001/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_ADMIN>
Content-Type: application/json
Accept: application/json
```

```json
{
  "generalNotes": "Paciente demo atualizado para evidencia manual.",
  "flagsJson": "{\"risk\":\"medium\",\"review\":true}"
}
```

Status esperado:

```http
HTTP/1.1 200 OK
Content-Type: application/json
```

Response esperado:

```json
{
  "id": 5001,
  "patientId": 1001,
  "generalNotes": "Paciente demo atualizado para evidencia manual.",
  "flagsJson": "{\"risk\":\"medium\",\"review\":true}",
  "updatedAt": "2026-01-01T12:30:00Z"
}
```

Confirmações esperadas: `GeneralNotes`, `FlagsJson` e `UpdatedAt` são atualizados; `FlagsJson` permanece objeto JSON válido e normalizado quanto a espaços externos.

### Cenário 10 — PUT com `FlagsJson` inválido

Pré-condição: paciente `1001` possui prontuário ativo com estado anterior conhecido.

```http
PUT /api/patients/1001/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_ADMIN>
Content-Type: application/json
Accept: application/json
```

```json
{
  "generalNotes": "Nota sintetica invalida que nao deve ser ecoada.",
  "flagsJson": "{"
}
```

Status esperado:

```http
HTTP/1.1 400 Bad Request
Content-Type: application/json
```

Response esperado sanitizado:

```json
{
  "message": "FlagsJson must be a valid JSON object."
}
```

A resposta não deve ecoar payload sensível. O estado anterior do prontuário deve ser preservado, sem mutação de `GeneralNotes`, `FlagsJson` ou `UpdatedAt`.

### Cenário 11 — PUT paciente existente sem prontuário

Pré-condição: paciente `1002` existe e não possui prontuário ativo.

```http
PUT /api/patients/1002/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_ADMIN>
Content-Type: application/json
Accept: application/json
```

```json
{
  "generalNotes": "Tentativa de atualizacao sem prontuario.",
  "flagsJson": "{\"risk\":\"low\"}"
}
```

Status esperado:

```http
HTTP/1.1 404 Not Found
Content-Type: application/json
```

Response esperado:

```json
{
  "message": "Medical record not found."
}
```

### Cenário 12 — 401 sem token

```http
GET /api/patients/1001/medical-record
Accept: application/json
```

Status esperado:

```http
HTTP/1.1 401 Unauthorized
```

O mesmo comportamento é esperado para `POST` e `PUT` quando o header `Authorization` estiver ausente.

### Cenário 13 — 403 com perfil sem permissão

Exemplo de leitura com perfil sem permissão:

```http
GET /api/patients/1001/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_RECEPTION>
Accept: application/json
```

Status esperado:

```http
HTTP/1.1 403 Forbidden
```

Exemplo de criação/atualização com perfil sem permissão:

```http
POST /api/patients/1002/medical-record
Authorization: Bearer <JWT_VALIDO_COM_PROFILE_ASSISTANT>
Content-Type: application/json
Accept: application/json
```

```json
{
  "generalNotes": "Tentativa sintetica sem permissao.",
  "flagsJson": "{\"risk\":\"low\"}"
}
```

Status esperado:

```http
HTTP/1.1 403 Forbidden
```

### Cenário 14 — Token sem profile claim

```http
GET /api/patients/1001/medical-record
Authorization: Bearer <JWT_VALIDO_SEM_PROFILE_CLAIM>
Accept: application/json
```

Status esperado confirmado pelos testes de controller:

```http
HTTP/1.1 403 Forbidden
```

O mesmo comportamento é esperado para `POST` e `PUT` quando o token é autenticado, mas não contém claim de profile válida.

## 9. Formato dos exemplos

Os exemplos deste documento usam blocos HTTP/JSON com:

- método e path do endpoint;
- headers relevantes;
- body JSON quando aplicável;
- status HTTP esperado;
- response JSON esperado e sanitizado.

IDs, nomes e timestamps são fictícios. Headers adicionais, casing de `Content-Type`, campos de tracing e detalhes de erro podem variar conforme ambiente, middleware, configuração ASP.NET Core e Swagger.

## 10. Segurança e sanitização

Regras obrigatórias para esta evidência e evidências futuras:

- nenhum dado real foi usado;
- nenhum token real deve ser versionado;
- nenhum segredo, e-mail, telefone, CPF, endereço, log real ou payload clínico real deve ser registrado;
- dados sensíveis devem ser mascarados ou substituídos por placeholders;
- exemplos são sintéticos e não representam paciente/tutor real;
- prints, se futuramente adicionados, devem ser revisados antes de commit;
- erros de validação de `FlagsJson` não devem ecoar payload de entrada sensível.

## 11. Relação com testes automatizados

Os cenários manuais são apoiados conceitualmente por testes automatizados existentes, sem substituí-los nem copiá-los integralmente:

```text
backend/src/Togo.Api.Tests/MedicalRecords/MedicalRecordsControllerTests.cs
backend/src/Togo.Application.Tests/MedicalRecords
backend/src/Togo.Domain.Tests/MedicalRecordTests.cs
backend/src/Togo.Infrastructure.Tests/Repositories/MedicalRecordRepositoryTests.cs
```

Coberturas relacionadas incluem GET/POST/PUT, validação de `PatientId`, validação de `FlagsJson`, normalização de campos, conflito de duplicidade, 401 sem token, 403 por perfil sem permissão e 403 para token sem claim de profile.

## 12. Limitações

- Evidência manual não substitui teste automatizado.
- Este documento é roteiro com resultados esperados; não houve execução manual real da API nesta fase.
- Ambiente real pode variar em headers, timestamps, host, `Location`, casing e formato exato de erro.
- Não cobre performance.
- Não cobre carga.
- Não cobre observabilidade.
- Não cobre frontend.
- Não cobre dados reais.
- Não cobre endpoint público `DELETE`, pois ele não existe no controller atual.
- Novos endpoints futuros exigirão nova evidência versionada.

## 13. Fora do escopo

Esta fase não implementa nem altera:

- código C#;
- testes;
- migrations;
- endpoint novo;
- collection Postman/Insomnia;
- customização de Swagger;
- autorização/policies;
- JWT/autenticação;
- frontend;
- banco;
- Docker, Redis, RabbitMQ ou Kubernetes;
- `MedicalRecordListItemResponse`.

## 14. Critérios de aceite

Critérios atendidos por esta fase:

- documento de evidência criado em `docs/clinical-core`;
- `MR-DEBT-011` referenciado explicitamente;
- endpoints reais do controller documentados;
- cenários mínimos GET/POST/PUT, validação, conflito, 401 e 403 cobertos;
- requests e responses sanitizados;
- nenhum token real registrado;
- nenhum dado real registrado;
- limitações registradas;
- relação com testes automatizados documentada;
- registro vivo atualizado;
- nenhuma implementação realizada;
- nenhuma migration criada;
- escopo de alteração restrito a `docs/clinical-core`;
- `git diff --check` deve passar nas validações finais.

## 15. Decisão final

MR-DEBT-011 fica resolvido tecnicamente por criação de evidência manual versionada e sanitizada da API MedicalRecord.

Como não houve execução real da API nesta fase, a resolução é documental/governança: um roteiro versionado com resultados esperados substitui a dependência exclusiva de prints ou validações informais não versionadas.

## 16. Próxima fase recomendada

```text
6.6.4 — Decisão sobre MedicalRecordListItemResponse
```
