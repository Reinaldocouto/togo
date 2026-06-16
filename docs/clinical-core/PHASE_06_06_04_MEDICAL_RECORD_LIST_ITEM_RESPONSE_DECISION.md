# Fase 6.6.4 — Decisão sobre MedicalRecordListItemResponse

## 1. Objetivo

Decidir formalmente o destino de `MedicalRecordListItemResponse` na vertical `MedicalRecord`, encerrando a ambiguidade sobre um contrato existente sem uso funcional imediato e registrando uma decisão técnica orientada por segurança, minimização de dados e clareza arquitetural.

## 2. Contexto da Fase 6.6

Esta fase integra a trilha de qualidade operacional e evidências finais da Fase 6 da vertical `MedicalRecord`.

Ela vem após:

- planejamento operacional da Fase 6.6.1;
- propagação de `CancellationToken` da Fase 6.6.2;
- evidência manual versionada da API/Swagger da Fase 6.6.3;
- correção documental da mensagem de conflito duplicado da Fase 6.6.3.1.

## 3. Referência ao MR-DEBT-012

Débito tratado:

```text
MR-DEBT-012 — MedicalRecordListItemResponse ainda não usado
```

## 4. Estado atual

A inspeção confirmou o seguinte estado:

- o contract `MedicalRecordListItemResponse` existe em `backend/src/Togo.Application/MedicalRecords/Contracts/MedicalRecordListItemResponse.cs`;
- o contract contém apenas `Id`, `PatientId`, `UpdatedAt`, `HasGeneralNotes` e `HasFlags`;
- o contract não é usado atualmente em use case, repository, controller ou teste de `MedicalRecord`;
- não existe endpoint público de listagem de prontuários;
- o controller atual expõe apenas operações por paciente em `GET`, `POST` e `PUT` na rota `api/patients/{patientId:long}/medical-record`;
- `IMedicalRecordRepository` e `MedicalRecordRepository` não possuem método de listagem ampla, `GetAll`, consulta paginada ou projection de list item;
- os testes de API cobrem `GET`, `POST`, `PUT`, autenticação, autorização e validações, sem cenário de listagem;
- a documentação anterior já registrava que listagens futuras devem ser tratadas com cuidado e não devem ser criadas automaticamente;
- a API evita listagem ampla nesta fase.

## 5. Risco original

O risco original de `MR-DEBT-012` combina ruído técnico e risco de privacidade futura:

- contract morto gera ruído técnico e pode confundir a leitura arquitetural;
- a existência do contract pode induzir uma implementação futura sem decisão explícita de privacidade;
- listagem de prontuários é uma superfície sensível, pois expõe associações entre pacientes e existência de dados clínicos;
- uma lista ampla pode expor dados clínicos, indícios de conteúdo e timestamps mesmo quando o payload parece minimizado;
- sem política clara, uma implementação futura pode criar vazamento funcional por autorização insuficiente, ausência de paginação, filtros amplos ou inclusão indevida de payload clínico.

## 6. Opções analisadas

### Opção A — Remover o contract

Vantagens:

- elimina código morto;
- reduz ruído;
- evita falsa intenção de listagem.

Desvantagens:

- pode apagar uma intenção futura válida;
- exigiria recriar contract quando listagem segura for planejada;
- não resolve a discussão de privacidade futura.

### Opção B — Manter como reservado documentado

Vantagens:

- preserva intenção futura;
- evita endpoint prematuro;
- registra explicitamente que não deve ser usado sem fase própria;
- permite futura listagem minimizada, paginada e autorizada.

Desvantagens:

- mantém um contract sem uso imediato;
- exige disciplina documental para não virar ruído permanente.

### Opção C — Implementar listagem segura agora

Vantagens:

- usa o contract;
- elimina débito formalmente por implementação.

Desvantagens:

- amplia superfície sensível;
- exige autorização específica;
- exige paginação, filtros, minimização e testes;
- mistura escopo;
- não foi planejado para esta fase;
- pode antecipar decisão de produto.

## 7. Decisão adotada

Decisão adotada:

```text
Opção B — Manter MedicalRecordListItemResponse como contrato reservado para futura listagem segura.
```

Esta decisão estabelece que:

- nenhum endpoint será criado nesta fase;
- o contract não deve ser usado sem fase própria;
- uma futura listagem deve exigir autorização explícita;
- a futura resposta deve ser minimizada;
- deve haver paginação obrigatória;
- deve evitar payload clínico sensível;
- deve evitar `FlagsJson` completo, salvo decisão futura justificada;
- deve possuir testes de autorização e privacidade;
- deve ter evidência manual versionada.

A decisão mantém a intenção de um payload reduzido para eventual listagem, mas impede que a simples existência do contract seja interpretada como autorização para expor uma lista ampla de prontuários.

## 8. Critérios mínimos para futura listagem segura

Uma futura listagem só deve avançar se houver:

- decisão de produto;
- caso de uso claro;
- autorização específica;
- paginação obrigatória;
- filtros mínimos;
- ordenação explícita;
- resposta minimizada;
- ausência de `GeneralNotes` completo;
- ausência de `FlagsJson` completo por padrão;
- testes de `401 Unauthorized` e `403 Forbidden`;
- testes de minimização;
- testes de paginação;
- documentação Swagger/manual evidence;
- validação de privacidade.

## 9. Impacto técnico

Impacto técnico desta fase:

- nenhum código alterado;
- nenhum endpoint criado;
- nenhum teste alterado;
- nenhuma migration criada;
- registro vivo atualizado;
- dívida resolvida por decisão técnica formal, não por implementação.

## 10. Fora do escopo

Esta fase não implementa:

- listagem;
- endpoint;
- repository query;
- use case;
- controller;
- testes;
- migration;
- frontend;
- alteração de autorização;
- alteração de contracts existentes;
- alteração de Swagger.

## 11. Riscos remanescentes

Riscos remanescentes:

- o contract continua sem uso imediato;
- se não for revisitado futuramente, pode voltar a virar ruído;
- implementação futura ainda precisa de decisão de privacidade;
- qualquer listagem futura pode ampliar superfície de exposição clínica;
- o débito é encerrado por decisão explícita, não por entrega funcional de listagem.

## 12. Critérios de aceite

A fase será considerada concluída se:

- uso atual do contract for inspecionado;
- ausência ou presença de uso for documentada;
- ausência de endpoint de listagem for confirmada;
- opções A/B/C forem analisadas;
- decisão formal for registrada;
- riscos de privacidade forem documentados;
- critérios para futura listagem segura forem definidos;
- registro vivo for atualizado;
- nenhuma implementação for feita;
- nenhuma migration for criada;
- somente `docs/clinical-core` for alterado;
- `git diff --check` passar.

## 13. Decisão final sobre MR-DEBT-012

```text
MR-DEBT-012 fica resolvido tecnicamente por decisão formal de manter MedicalRecordListItemResponse como contrato reservado para futura listagem segura, sem criação de endpoint nesta fase.
```

## 14. Próxima fase recomendada

```text
6.6.5 — Evidências finais, atualização do registro vivo e encerramento da Fase 6
```
