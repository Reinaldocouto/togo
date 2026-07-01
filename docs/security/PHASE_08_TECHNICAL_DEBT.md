# Débitos técnicos da Fase 8

## Contexto

Este arquivo concentra riscos e débitos identificados durante a implementação de segurança contextual, escopo clínico e governança multiunidade no projeto TOGO. Ele deve ser mantido como arquivo vivo durante as próximas fases da Fase 8, especialmente antes de produção, porque a persistência de `ClinicId` ainda não equivale a isolamento de acesso sem `CurrentClinicalContext`, autorização contextual e filtros por contexto.

## Débitos registrados

### 1. Migrations orientadas a MySQL

- **Descrição:** As migrations de backfill das entidades clínicas derivadas usam `UPDATE ... INNER JOIN`. Isso é adequado ao MySQL, mas não é portável para SQLite ou outros providers. A migration inicial de `Tutor`/`Patient` usa estratégia transitória baseada na primeira clínica existente para compatibilizar dados legados, conforme decisão anterior.
- **Impacto:** Execução de migrations pode falhar ou exigir reescrita em ambientes de teste/produção que não usem MySQL ou que usem provider diferente do alvo esperado.
- **Fase sugerida para tratamento:** Pré-produção da Fase 8 ou fase de hardening de migrations.
- **Severidade:** média
- **Status:** aberto

### 2. Backfill antes de produção

- **Descrição:** Revisar dados órfãos antes de aplicar migrations, garantindo consistência entre `Tutor`, `Patient`, `Attendance`, `MedicalRecord`, `ClinicalEvolution` e `Prescription`. Definir estratégia operacional de backup e rollback.
- **Impacto:** Dados órfãos ou inconsistentes podem interromper migrations intencionalmente ou gerar indisponibilidade durante implantação.
- **Fase sugerida para tratamento:** Pré-produção, antes da aplicação de migrations em banco real.
- **Severidade:** alta
- **Status:** aberto

### 3. Scope readers / repositories

- **Descrição:** Avaliar criação de `ClinicalScopeReader`, `PatientScopeRepository` ou equivalente para evitar uso semântico inadequado de `IPetRepository` como fonte de escopo de `Patient` e padronizar obtenção de `ClinicId` em fluxos clínicos.
- **Impacto:** Reuso semântico de repositories pode aumentar acoplamento, dificultar manutenção e gerar padrões divergentes para resolver escopo clínico.
- **Fase sugerida para tratamento:** Fase 8.4 ou 8.5, antes de filtros globais por contexto.
- **Severidade:** média
- **Status:** aberto

### 4. Projections de escopo clínico

- **Descrição:** Padronizar projections como `AttendancePatientScope`, `MedicalRecordPatientScope` e equivalentes para evitar duplicação de padrões divergentes.
- **Impacto:** Divergências entre projections podem introduzir inconsistência de escopo e aumentar custo de revisão de autorização contextual.
- **Fase sugerida para tratamento:** Fase 8.4 ou 8.5.
- **Severidade:** média
- **Status:** aberto

### 5. Validação amigável de Clinic

- **Descrição:** Hoje alguns fluxos podem depender de FK para rejeitar `ClinicId` inexistente. Avaliar validação amigável de existência/atividade de `Clinic` na Application. Isso não substitui autorização contextual.
- **Impacto:** Erros de FK podem chegar ao usuário como falhas menos claras; além disso, clínica inativa pode não ser bloqueada em todos os fluxos.
- **Fase sugerida para tratamento:** Fase 8.5, junto da autorização contextual.
- **Severidade:** média
- **Status:** aberto

### 6. Responses expondo ClinicId

- **Descrição:** `AttendanceResponse`, `MedicalRecordResponse`, `ClinicalEvolutionResponse` e `PrescriptionResponse` expõem `ClinicId` para rastreabilidade. Antes da autorização contextual completa, isso não representa permissão de acesso. Revisar contratos públicos antes de produção.
- **Impacto:** Consumidores podem interpretar incorretamente `ClinicId` como autorização ou isolamento já aplicado.
- **Fase sugerida para tratamento:** Revisão de contratos antes de produção e durante a Fase 8.5.
- **Severidade:** baixa
- **Status:** aberto

### 7. CurrentClinicalContext ainda ausente

- **Descrição:** `ClinicId` persistido não é isolamento de acesso. A proteção real depende das próximas fases: `CurrentClinicalContext`, autorização contextual e filtros por contexto.
- **Impacto:** Sem contexto clínico ativo, consultas e comandos ainda não conseguem validar automaticamente o escopo permitido do usuário.
- **Fase sugerida para tratamento:** Fase 8.4 — `CurrentClinicalContext`.
- **Severidade:** alta
- **Status:** aberto

### 8. UserClinicAccess ainda ausente

- **Descrição:** Ainda não existe validação de vínculo usuário-clínica. Isso será necessário antes de ambiente real.
- **Impacto:** Mesmo com `ClinicId` persistido, o sistema ainda não comprova se o usuário pode operar naquela clínica.
- **Fase sugerida para tratamento:** Fase 8.5 — autorização contextual.
- **Severidade:** alta
- **Status:** aberto

### 9. Filtros globais por contexto ainda ausentes

- **Descrição:** Listagens e consultas ainda podem não estar filtradas por clínica. A Fase 8.6 deve tratar isso.
- **Impacto:** Risco de exposição cruzada em consultas/listagens até que filtros por contexto sejam implementados e testados.
- **Fase sugerida para tratamento:** Fase 8.6 — filtros por contexto.
- **Severidade:** alta
- **Status:** aberto

### 10. Auditoria contextual completa ainda ausente

- **Descrição:** Metadata de criação foi enriquecido, mas ainda não há auditoria contextual completa. Leitura e acesso negado ainda não são auditados como capacidade transversal da Fase 8.
- **Impacto:** Investigações de acesso e rastreabilidade contextual permanecem incompletas até a evolução do modelo de auditoria.
- **Fase sugerida para tratamento:** Fases 8.7 e 8.8.
- **Severidade:** média
- **Status:** aberto
