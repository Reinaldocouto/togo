# TOGO — Fase 5.0.0: Diretrizes clínicas, regulatórias e arquiteturais para Prontuário Veterinário

## 1) Objetivo

Este documento estabelece diretrizes oficiais **antes da implementação** da Fase 5 — MedicalRecord / Prontuário do TOGO. O foco é orientar decisões técnicas e de produto relacionadas a:

- prontuário veterinário;
- dados clínicos;
- histórico do paciente;
- segurança;
- privacidade;
- rastreabilidade;
- retenção de dados;
- Soft Delete;
- limites regulatórios;
- débitos técnicos assumidos conscientemente.

## 2) Contexto do TOGO

O TOGO é um **ERP veterinário**. Não é, nesta fase, um sistema médico humano homologado.

A cadeia clínica do projeto segue a estrutura:

Tutor  
↓  
Patient/Pet  
↓  
Attendance  
↓  
MedicalRecord / Prontuário  
↓  
ClinicalEvolution / Prescription / Exams / Attachments / AuditLog (futuros)

A arquitetura do TOGO é inspirada em sistemas clínicos reais, mantendo foco explícito em domínio veterinário.

## 3) Declaração central: Prontuário não é Atendimento

> **Prontuário não é atendimento.**

Diretrizes conceituais:

- **Attendance** representa episódio, visita, consulta ou evento clínico específico.
- **MedicalRecord** representa o histórico longitudinal principal do paciente.
- **ClinicalEvolution** representa registros clínicos dentro ou relacionados a um atendimento.
- **Prescription** representa prescrição vinculada ao atendimento.
- **Exam/DiagnosticReport** (futuros) representam evidências diagnósticas.
- O prontuário não deve virar um campo gigante de observação.
- O atendimento não deve substituir o prontuário.
- O prontuário deve organizar a visão clínica histórica do paciente.

**Decisão arquitetural registrada:**

- Um paciente deve possuir apenas **um prontuário principal ativo**.

## 4) Escopo veterinário e limite humano

O TOGO permanece um ERP veterinário e não deve ser anunciado/documentado como PEP humano legalmente homologado.

Caso haja adaptação futura para atendimento humano, será necessária trilha própria de compliance com, entre outros:

- CFM/CRM;
- SBIS/S-RES;
- LGPD avançada;
- assinatura digital/eletrônica;
- auditoria clínica;
- controle de autoria;
- retenção documental;
- rastreabilidade;
- interoperabilidade;
- padrões como HL7/FHIR;
- RNDS;
- TISS/TUSS (quando houver saúde suplementar).

Referências de saúde humana podem inspirar decisões técnicas, mas **não** significam conformidade humana plena.

## 5) Referências normativas e técnicas consideradas

A documentação e a arquitetura do TOGO consideram referências técnicas e regulatórias como base de maturidade:

### 5.1) CFMV / CRMVs

- órgão responsável por fiscalizar, orientar, supervisionar e disciplinar a atuação médico-veterinária;
- Código de Ética do Médico-Veterinário;
- obrigação ética de elaboração de prontuário médico-veterinário para casos individuais;
- dever de sigilo profissional sobre prontuários, relatórios e documentos clínicos.

### 5.2) LGPD / ANPD

- dados de tutores/clientes são dados pessoais;
- informações clínicas veterinárias podem estar vinculadas a pessoas naturais responsáveis pelo animal;
- aplicar princípios de finalidade, necessidade, segurança, prevenção, transparência e responsabilização;
- não logar dados clínicos completos;
- não expor payloads sensíveis;
- adotar privacy by design.

### 5.3) HL7/FHIR

- referência internacional para estruturação e interoperabilidade de informações de saúde;
- uso como referência conceitual nesta etapa;
- FHIR não será implementado nesta fase.

### 5.4) ISO/TC 215

- referência internacional em health informatics;
- considerar conceitos de segurança, privacidade, interoperabilidade, rastreabilidade e qualidade.

### 5.5) SBIS/S-RES

- referência técnica brasileira para sistemas eletrônicos em saúde humana;
- usar como inspiração de maturidade técnica, não como obrigação direta do ERP veterinário nesta fase.

## 6) Dados clínicos são sensíveis

> **Dados clínicos são sensíveis sempre.**

Mesmo em contexto veterinário, o sistema lida com:

- dados do tutor;
- dados do cliente;
- histórico do animal;
- condutas clínicas;
- prescrições;
- exames;
- evoluções;
- possíveis evidências legais/contratuais.

Regras de proteção:

- não logar conteúdo completo de prontuário;
- não logar `GeneralNotes`;
- não logar `FlagsJson` completo;
- não logar evolução clínica completa;
- não logar prescrição completa;
- não expor dados clínicos em erros técnicos;
- usar IDs, flags e metadados nos logs;
- aplicar controle de acesso;
- preservar rastreabilidade.

## 7) Soft Delete como requisito arquitetural

> **Soft Delete é requisito indispensável para maturidade clínica do TOGO.**

Fundamentação:

- delete físico de paciente com histórico clínico é perigoso;
- paciente com prontuário, atendimento, evolução, prescrição ou exame não deve ser apagado fisicamente;
- a aplicação deve evoluir para inativação lógica;
- preservar histórico clínico é essencial para rastreabilidade, responsabilidade profissional e segurança jurídica;
- exclusões físicas podem comprometer auditoria, histórico e evidências clínicas.

Débito técnico formal:

- Soft Delete ainda não será implementado neste documento, mas deve ser tratado como débito técnico obrigatório antes da conclusão madura do núcleo clínico.

Campos sugeridos para fase específica:

- `IsDeleted`;
- `DeletedAt`;
- `DeletedByUserId`;
- `DeletionReason`;
- `IsActive`;
- `InactivatedAt`;
- `InactivatedByUserId`;
- `InactivationReason`.

A decisão final entre modelo `IsDeleted` e `IsActive` deve ser tomada em fase própria.

## 8) Política futura de retenção e preservação

O TOGO deve adotar política futura de retenção de dados clínicos considerando:

- obrigações éticas e profissionais;
- LGPD;
- retenção por obrigação legal/regulatória;
- direito do titular, quando aplicável;
- relação com tutor/cliente;
- histórico clínico do animal;
- segurança jurídica;
- auditoria;
- anonimização em cenários futuros;
- impossibilidade de apagar histórico clínico crítico sem critério formal.

Não há definição de prazo legal específico nesta fase. O prazo de retenção deve ser validado futuramente com análise jurídica/profissional.

## 9) FlagsJson como débito técnico controlado

> **FlagsJson dá flexibilidade, mas pode virar bagunça.**

Na Fase 5, o uso de `FlagsJson` pode ser aceito como MVP técnico, desde que documentado como débito técnico controlado.

Estrutura esperada inicial (exemplo):

```json
{
  "allergies": ["penicillin"],
  "chronicConditions": ["diabetes"],
  "behaviorNotes": ["aggressive_when_handled"],
  "riskAlerts": ["requires_muzzle"],
  "specialCare": ["fasting_before_exams"]
}
```

Limites obrigatórios:

- não armazenar qualquer coisa sem padrão;
- não usar `FlagsJson` para substituir entidade futura;
- não armazenar logs, prescrições, evoluções ou exames dentro de `FlagsJson`;
- não usar `FlagsJson` como “lixeira de campos”;
- não expor integralmente em logs;
- validar tamanho máximo futuramente;
- validar estrutura mínima futuramente.

Plano futuro:

- criar estrutura tipada;
- criar entidades próprias para alergias, alertas, condições crônicas ou marcadores clínicos, se o domínio exigir;
- migrar `FlagsJson` para modelo normalizado quando houver maturidade funcional.

## 10) Auditoria e rastreabilidade futura

O prontuário deve preparar terreno para auditoria como evolução futura:

- `AuditLog`;
- histórico de alterações;
- quem criou;
- quem alterou;
- quando alterou;
- motivo da alteração;
- IP/origem opcional;
- usuário autenticado;
- diferenciação entre update administrativo e registro clínico;
- não sobrescrever evolução clínica sem rastreabilidade.

Em contexto clínico, nem todo update deve apagar o passado.

## 11) Controle de acesso e segurança

A Fase 5 deve permanecer protegida por JWT e evoluir para roles/permissões granulares.

Regras futuras:

- somente usuários autorizados devem acessar prontuário;
- perfis diferentes devem ter permissões diferentes;
- prontuário deve ter acesso mais restrito que cadastro básico;
- listagens devem evitar dados clínicos pesados;
- endpoints devem retornar apenas o necessário;
- Swagger não deve expor exemplos com dados reais sensíveis;
- erros não devem vazar conteúdo clínico.

## 12) Decisões técnicas iniciais para a Fase 5

- MedicalRecord será tratado como prontuário principal do `Patient`.
- Um `Patient` deve ter no máximo um `MedicalRecord` principal.
- `Attendance` não substitui `MedicalRecord`.
- `MedicalRecord` não deve armazenar tudo.
- `ClinicalEvolution`, `Prescription`, `Exams` e `Attachments` devem permanecer estruturas separadas.
- Soft Delete é obrigatório como evolução arquitetural.
- `FlagsJson` é aceito como MVP técnico, com débito técnico explícito.
- Dados clínicos não devem ser logados integralmente.
- O TOGO permanece veterinário.
- Padrões humanos são referência técnica, não declaração de conformidade humana.
- FHIR não será implementado agora.
- Redis/RabbitMQ não entram nesta fase documental.
- Docker/Health Check podem seguir em trilha de Infra separada.

## 13) Débitos técnicos assumidos

| Débito Técnico | Motivo | Risco se ignorado | Momento recomendado |
|---|---|---|---|
| Soft Delete | Preservar histórico e integridade clínica | Perda de evidência e quebra de rastreabilidade | Antes da maturidade clínica do núcleo |
| AuditLog | Garantir autoria e trilha de alteração | Alterações sem responsabilidade clara | Primeiras subfases após MedicalRecord base |
| Controle de roles/permissões finas | Restringir acesso por perfil | Vazamento/acesso indevido | Fase de endurecimento de segurança |
| Normalização de FlagsJson | Reduzir ambiguidade estrutural | Dívida de modelagem e inconsistência | Após estabilização funcional inicial |
| Política formal de retenção | Definir preservação e descarte | Risco jurídico e operacional | Com apoio jurídico/profissional |
| Revisão de cascade delete em entidades clínicas | Evitar apagamentos indevidos em cadeia | Perda de dados críticos | Antes de liberar operações destrutivas |
| Histórico de alterações clínicas | Preservar evolução longitudinal | “Apagar passado” clínico sem lastro | Em conjunto com auditoria |
| Criptografia em repouso | Aumentar proteção de dados | Exposição em incidente de infraestrutura | Trilha de segurança de dados |
| Padronização Swagger com `ProducesResponseType` | Contratos HTTP mais claros | Ambiguidade de integração e testes | Durante evolução da API clínica |
| WebApplicationFactory E2E | Validar fluxo HTTP real | Regressões não detectadas | Após endpoints principais da Fase 5 |
| Anonimização/exportação futura (quando aplicável) | Governança e direitos de dados | Baixa capacidade de resposta regulatória | Trilha de governança de dados |

## 14) Fora do escopo

Este documento **não** implementa:

- MedicalRecord;
- repository;
- controller;
- use cases;
- validators;
- migrations;
- database update;
- Soft Delete;
- AuditLog;
- roles/permissões;
- FHIR;
- Redis;
- RabbitMQ;
- Docker;
- Kubernetes;
- frontend;
- qualquer código de produção.

## 15) Critérios de aceite do documento

Este documento é considerado concluído se:

- deixar claro que prontuário não é atendimento;
- registrar TOGO como ERP veterinário;
- registrar limite de não conformidade humana plena;
- citar referências CFMV/CRMVs, LGPD/ANPD, HL7/FHIR, ISO/TC 215 e SBIS/S-RES como referências técnicas;
- documentar Soft Delete como débito técnico obrigatório;
- documentar FlagsJson como débito técnico controlado;
- reforçar proteção de dados clínicos e LGPD;
- orientar a Fase 5 MedicalRecord;
- não alterar código;
- não alterar testes;
- não alterar banco;
- não criar migration.

## 16) Próxima fase recomendada

**Fase 5.0.1 — Planejamento técnico da implementação de MedicalRecord / Prontuário.**

Objetivo da próxima fase:

- definir modelo mínimo;
- definir subfases de implementação;
- definir contracts;
- definir repository;
- definir validators;
- definir use cases;
- definir API;
- definir testes;
- definir validação HTTP E2E;
- definir documentação final.
