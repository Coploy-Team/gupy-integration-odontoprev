# gupy-integration-odontoprev

Integração Coploy / Gupy para a Odontoprev.

- Sincroniza vagas do Gupy com etapa "Entrevista Coploy" para o Firebase
- Envia convite por email quando candidatos são movidos para essa etapa

## Configuração

1. Copie `appsettings.example.json` para `appsettings.json` e preencha as variáveis
2. Configure `firebase.json` com as credenciais do Firebase (não versionado)

## Deploy (GitHub Actions)

Secrets necessários no repositório:

| Secret | Descrição |
|--------|-----------|
| `GCP_SERVICE_ACCOUNT_KEY` | JSON da service account do GCP (usado também para Firebase) |
| `GCP_PROJECT_ID` | ID do projeto no GCP |
