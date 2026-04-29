# Database Guidelines (Fase 0)

## Banco oficial
- Banco oficial do projeto: **MySQL**.
- Desenvolvimento local: **MySQL local**.

## Destino das tabelas antigas (legado em português)
As tabelas abaixo devem ser **mantidas por enquanto**:
- tutor
- paciente
- pet
- prontuario
- atendimento
- evolucao
- prescricao
- prescricao_item
- estoque_produto
- estoque_movimento
- venda
- venda_item
- financeiro_lancamento

Diretrizes obrigatórias:
- Essas tabelas são referência histórica/técnica para análise e migração futura.
- Não remover sem planejamento explícito.
- Não usar em código novo.
- Não usar em migrations novas, repositories, use cases ou controllers futuros.
- Todo desenvolvimento novo deve usar o modelo oficial em inglês.
- A migração de dados legado deverá ser planejada separadamente.

## Modelo oficial (novo)
O modelo oficial deve seguir tabelas em inglês criadas por EF Core migrations, incluindo:
- Tutors
- Patients
- Pets
- MedicalRecords
- Attendances
- ClinicalEvolutions
- Prescriptions
- PrescriptionItems

## Padrão de alterações estruturais
- Toda alteração estrutural deve ser feita via **EF Core Migrations**.
- Não criar tabelas manualmente via MySQL Workbench para o núcleo novo.
- MySQL Workbench pode ser usado apenas para inspeção, conferência e análise visual.
- Revisar migrations antes de executar `database update`.

Fluxo recomendado:

```bash
dotnet build backend/Togo.sln

dotnet ef migrations add NomeDaMigration --project backend/src/Togo.Infrastructure/Togo.Infrastructure.csproj --startup-project backend/src/Togo.Api/Togo.Api.csproj

# revisar migration gerada

dotnet ef database update --project backend/src/Togo.Infrastructure/Togo.Infrastructure.csproj --startup-project backend/src/Togo.Api/Togo.Api.csproj
```
