When initializing DB for the first time, comment data seeding when doing dotnet ef database update.
Then use it again after the first migration is applied. Otherwise, possible EF + PG enums issue.

Install EF tools: ```dotnet tool install --global dotnet-ef```<br />
Update EF tools: ```dotnet tool update --global dotnet-ef```<br />
Add migration: ```dotnet ef migrations add Initial```<br />
Update database: ```dotnet ef database update```<br />

Currently, there's a problem with EF migrations when adding Postgres enums
and using them in the same transaction: https://github.com/npgsql/efcore.pg/issues/3389. <br />
In this project, it's workarounded with <br /> ```migrationBuilder.Sql("SELECT 1;", suppressTransaction: true);``` <br />
Probably, this should be solved with Dotnet 10. <br />
For now, this workaround is used in the migration files and warning for this operation is ok.