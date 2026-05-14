# TOGO — Fase 3.11: Revisão final do CRUD Pet/Patient

## 1. Objetivo da fase

A Fase 3.11 teve como objetivo validar o CRUD Pet/Patient de ponta a ponta em ambiente local e no GitHub Actions, após as fases de implementação, testes manuais, testes automatizados e documentação.

Essa revisão funcionou como fechamento operacional do fluxo Pet/Patient, confirmando que as camadas e integrações construídas nas fases anteriores funcionam em conjunto:

```text
Domain → Application → Infrastructure → API → Banco → Postman → Logs → CI
```

## 2. Escopo da revisão

A revisão da Fase 3.11 cobriu os seguintes pontos:

- build local;
- testes automatizados locais;
- API local;
- autenticação;
- CRUD Pet/Patient via Postman;
- logs;
- GitHub Actions;
- branch protection.

## 3. Validação local de build/test

A validação local de build e testes foi executada pelo desenvolvedor humano com os comandos abaixo:

```bash
git pull origin main
dotnet build backend/Togo.sln
dotnet test backend/Togo.sln
git status
```

Resultado informado pelo desenvolvedor humano:

- build com sucesso;
- total: 96 testes;
- falhou: 0;
- bem-sucedido: 96;
- ignorado: 0;
- working tree clean.

Conclusão da subfase 3.11.1:

- build local aprovado;
- testes automatizados locais aprovados;
- nenhum teste falhando;
- repositório local sem alterações pendentes ao final da validação humana.

## 4. Checklist manual do CRUD Pet/Patient

O checklist manual principal do Pet foi executado no Postman contra a API local em:

```text
https://localhost:3003
```

Endpoint base validado:

```text
/api/pets
```

| Caso | Endpoint | Esperado | Resultado |
|---|---|---|---|
| Login válido | `POST /api/auth/login` | `200 OK` | OK |
| Pets sem token | `GET /api/pets` | `401 Unauthorized` | OK |
| Listar Pets com token | `GET /api/pets` | `200 OK` | OK |
| Criar Pet | `POST /api/pets` | `201 Created` | OK |
| Buscar Pet criado | `GET /api/pets/{patientId}` | `200 OK` | OK |
| Microchip duplicado | `POST /api/pets` | `409 Conflict` | OK |
| Tutor inexistente | `POST /api/pets` | `404 Not Found` | OK |
| Atualizar Pet | `PUT /api/pets/{patientId}` | `200 OK` | OK |
| Buscar inexistente | `GET /api/pets/999999` | `404 Not Found` | OK |
| Deletar inexistente | `DELETE /api/pets/999999` | `404 Not Found` | OK |
| Deletar existente | `DELETE /api/pets/{patientId}` | `204 No Content` | OK |
| Buscar após delete | `GET /api/pets/{patientId}` | `404 Not Found` | OK |

Mensagens esperadas também validadas nos cenários de erro:

- microchip duplicado: `A pet with this microchip already exists.`;
- Tutor inexistente: `Tutor not found.`;
- Pet inexistente: `Pet not found.`.

## 5. Resultado da validação manual

Resultado consolidado da validação manual da subfase 3.11.2:

- API real respondeu corretamente;
- autenticação funcionou;
- endpoints protegidos retornaram `401 Unauthorized` sem token;
- criação retornou `201 Created`;
- busca retornou `200 OK`;
- duplicidade de microchip retornou `409 Conflict`;
- Tutor inexistente retornou `404 Not Found`;
- atualização retornou `200 OK`;
- delete existente retornou `204 No Content`;
- busca após delete retornou `404 Not Found`;
- erros esperados foram validados;
- delete e busca pós-delete foram validados;
- nenhum bug novo foi identificado.

Conclusão da validação manual:

- CRUD Pet/Patient validado pela API real;
- autenticação validada;
- comportamento funcional aprovado para o escopo atual da Fase 3.

## 6. Revisão de logs

Os logs do fluxo manual foram revisados na subfase 3.11.3 e considerados adequados para a fase atual.

Os logs permitem rastrear os seguintes eventos do fluxo Pet/Patient:

- login;
- listagem;
- criação;
- busca;
- atualização;
- exclusão;
- falhas esperadas.

Itens considerados OK na revisão:

- API subiu corretamente;
- login registrado;
- listagem registrada;
- criação registrada;
- busca registrada;
- atualização registrada;
- delete registrado;
- erros esperados registrados como `Warning` ou `Information`, conforme aplicável;
- fluxo rastreável por `PatientId`, `TutorId`, `HasMicrochip` e `Count`.

Campos permitidos nos logs da aplicação:

- `PatientId`;
- `TutorId`;
- `HasMicrochip`;
- `Count`;
- mensagens de sucesso ou falha esperadas.

Dados que não devem aparecer nos logs próprios da aplicação ou em responses HTTP:

- senha;
- token JWT;
- payload completo;
- microchip completo ou parcial nos logs próprios da aplicação;
- stack trace em response HTTP.

Ponto de atenção futuro:

- o log de autenticação pode exibir e-mail em ambiente de desenvolvimento;
- isso é aceitável para a fase atual, mas deve ser reavaliado futuramente para produção.

Conclusão da revisão de logs:

- logs revisados;
- fluxo CRUD Pet/Patient rastreável;
- logs úteis e seguros para a fase atual;
- nenhum vazamento crítico identificado nos logs próprios da aplicação.

## 7. Validação do GitHub Actions / CI

A validação de CI foi realizada na subfase 3.11.4 para o workflow:

```text
TOGO Backend CI
```

Arquivo do workflow:

```text
.github/workflows/dotnet-ci.yml
```

Eventos validados:

- `push` na `main`;
- `pull_request`;
- `workflow_dispatch`.

Job principal validado:

```text
Restore, build and test
```

Steps validados no GitHub Actions:

- Checkout repository;
- Setup .NET;
- Restore;
- Build;
- Test;
- Complete job.

Resultado observado no CI:

- CI automático após merge da PR 65: sucesso;
- CI manual via `workflow_dispatch`: sucesso;
- job `Restore, build and test`: sucesso;
- Restore: sucesso;
- Build: sucesso;
- Test: sucesso;
- Build no CI com 0 warnings e 0 errors;
- total observado: 96 testes passando.

Projetos de teste observados nos logs do CI:

- `Togo.Domain.Tests`: 43 testes passando;
- `Togo.Api.Tests`: 2 testes passando;
- `Togo.Application.Tests`: 51 testes passando;
- total: 96 testes passando.

Conclusão da validação de CI:

- workflow existente e funcional;
- execução automática validada;
- execução manual validada;
- build e testes protegendo a branch principal.

## 8. Branch protection

A branch protection da `main` foi validada com as seguintes regras:

- branch protegida: `main`;
- pull request obrigatório antes de merge;
- status checks obrigatórios antes do merge;
- check obrigatório: `Restore, build and test`;
- force push desabilitado;
- delete branch desabilitado.

Essa configuração reduz o risco de uma PR quebrada ser incorporada na `main`, pois exige revisão via pull request e aprovação do status check principal de restore, build e testes antes do merge.

## 9. Warnings e débitos técnicos identificados

Foram registrados os seguintes warnings e débitos técnicos não bloqueantes:

- warning do GitHub Actions sobre depreciação de Node.js 20 em `actions/checkout@v4` e `actions/setup-dotnet@v4`;
- acompanhar atualização das actions/runner futuramente;
- e-mail em log de autenticação deve ser reavaliado para produção;
- delete físico de Pet/Patient deve ser reavaliado antes da implementação de Atendimento/Prontuário;
- falta paginação/filtros no `GET /api/pets`;
- testes de controller/API ainda não foram criados;
- testes de integração com banco real ainda não foram criados.

Esses pontos não bloquearam o fechamento da Fase 3.11, mas devem ser acompanhados nas próximas fases para reduzir riscos operacionais e preparar o fluxo clínico para evolução.

## 10. Status final da Fase 3.11

Status final consolidado:

- Fase 3.11 concluída com sucesso;
- build/test local aprovados;
- checklist manual aprovado;
- logs revisados;
- CI validado;
- branch protection validada;
- CRUD Pet/Patient considerado funcional e validado para o escopo atual.

Conclusão final:

- o fluxo Pet/Patient foi validado de ponta a ponta;
- a API real respondeu conforme esperado nos cenários principais e de erro;
- os logs foram considerados rastreáveis e seguros para a fase atual;
- o CI confirma build e testes automatizados com 96/96 testes passando;
- a branch `main` possui proteção mínima adequada para reduzir risco de regressão via PR.

## 11. Próxima fase recomendada

Próxima fase recomendada:

```text
Fase 3.12 — Fechamento macro da Fase 3
```

Objetivo da próxima fase:

- consolidar tudo que foi feito na Fase 3;
- registrar arquitetura aplicada;
- listar arquivos/fases criadas;
- registrar endpoints disponíveis;
- registrar testes automatizados;
- registrar validações manuais;
- registrar bugs encontrados e corrigidos;
- registrar débitos técnicos;
- preparar transição para a Fase 4 — Atendimento.

A Fase 3.12 deve funcionar como documentação macro de encerramento da Fase 3, preparando uma base clara para evolução do módulo clínico na Fase 4.
