# Azure SQL para o ambiente de desenvolvimento

O aplicativo exige `ConnectionStrings:DefaultConnection`. Ele não cria mais um SQLite local quando essa configuração está ausente.

## Configuração local

Armazene a conexão somente nos User Secrets do projeto:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<conexão Azure SQL>"
```

Não use `appsettings*.json` nem variáveis versionadas para credenciais. Para remover a configuração local, execute `dotnet user-secrets remove "ConnectionStrings:DefaultConnection"`.

## Migração do banco antigo

Faça primeiro a checagem do backup exportado do App Service:

```powershell
dotnet run -- --verify-sqlite "C:\caminho\Fabrica.db"
```

Depois de aplicar a migração SQL Server em um banco Azure SQL vazio, faça a importação. O comando interrompe se a origem estiver vazia ou se o destino já tiver dados; a operação é transacional e preserva os IDs.

```powershell
dotnet run -- --import-sqlite "C:\caminho\Fabrica.db"
```

Backups antigos com `RaioMm` são convertidos para `DiametroMm` durante a importação.

## Smoke test

O smoke test não usa a conexão dev. Crie e inicie uma instância descartável com outra base Azure SQL e defina a conexão apenas no ambiente do terminal:

```powershell
$env:TEST_CONNECTION_STRING = "<conexão Azure SQL de teste>"
$env:ConnectionStrings__DefaultConnection = $env:TEST_CONNECTION_STRING
dotnet run --urls http://localhost:5001
```

Em outro terminal com a mesma `TEST_CONNECTION_STRING`, execute `./tests/ux-smoke.ps1`. Nunca aponte o teste para a base compartilhada `fabrica-aluminio-dev-db`.
