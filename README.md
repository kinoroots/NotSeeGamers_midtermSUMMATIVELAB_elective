# NotSeeGamers_midtermSUMMATIVELAB_elective

## Run the event registration prototype

The frontend is served by the ASP.NET Core backend so registration requests use the same origin.

1. Create or select a SQL Server database, then execute `database/schema.sql` against it once to create the tables. This script drops and recreates its tables, so do not rerun it on a database containing data you want to keep.
2. Run `database/seed-catalog-events.sql` to add the six events shown by the catalog. This seed script is safe to run more than once.
3. Configure the `DefaultConnection` connection string using the `ConnectionStrings__DefaultConnection` environment variable.
4. Start the application with `dotnet run --project backend/NotSeeGamers.Backend.csproj --urls http://localhost:5080`.
5. Open `http://localhost:5080` and submit the registration form.

The API validates the university email domain, creates a Student user when needed, and inserts the registration into `dbo.Registrations`. Duplicate registrations and full events are rejected. The browser confirmation is shown only after the API confirms the database write.