Store the SQL Server connection string in user secrets for development

This project has a UserSecretsId set in the project file. To store your development-only SQL Server connection string locally, run the following in the backend project folder:

```bash
# run from ShiftsLoggerV2.RyanW84 (backend) folder
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ShiftsLoggerDb;User Id=sa;Password=YOUR_SA_PASSWORD;TrustServerCertificate=True;"
```

Notes:
- Replace YOUR_SA_PASSWORD with your SA password.
- Ensure ASPNETCORE_ENVIRONMENT=Development when running locally so the User Secrets configuration is loaded automatically.
- To inspect the stored secret:

```bash
dotnet user-secrets list
```

- To remove it:

```bash
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"
```

Security:
- Do not commit secrets to source control.
- For CI or production, use environment variables or a secure secret store (Key Vault, etc.).
