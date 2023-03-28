# YAWA - Yet Another Web App

# Development

```
cd ./to-solution-folder

# This is required once after cloning the repo
dotnet tool restore

# Upgrade the dotnet-ef tool
dotnet tool update dotnet-ef

# Add a migration
dotnet ef migrations add 1 -s WebApp -c AppDbContext -v
dotnet ef migrations add 1 -s WebApp -c AppDbContext -v -- 'Server=127.0.0.1;Port=5432;Database=vanilla_tenants;Integrated Security=True;Include Error Detail=True;CommandTimeout=30;ConnectionTimeout=5;'

# Apply migrations
dotnet ef database update -s WebApp -c AppDbContext

# Apply/revert to a specific migration
dotnet ef database update MIGRATION -c AppDbContext -s WebApp

# Remove last migration, the migration cannot be applied to the database
dotnet ef migrations remove -c AppDbContext -p Cohub.Data.PostgreSQL -s WebApp

# Revert all migrations
dotnet ef database update 0 -s WebApp -c AppDbContext
```

# Dotnet Tools

Dotnet tools can be used on any operating system.

```
cd ./to-solution-folder

# Add a tool manifest
dotnet new tool-manifest

# Install a tool
dotnet tool install dotnet-ef

# This is required once after cloning the repo
dotnet tool restore

# Upgrade a tool
dotnet tool update dotnet-ef
```
