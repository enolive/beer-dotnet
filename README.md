# Beer Service

simple CRUD application written in ASP.NET Core which manages beers.

it stores all data inside a Postgres database using json for the entities.

## Development

```bash
docker compose up -d
docker run --project BeerService
``` 

## Testing

The tests are solely relying on TestContainers, so no need to start a Postgres container for that.

```bash
dotnet test
```

## Tech Stack

- DotNetCore 10
- ASP.NET Core 10
- Postgres
- TestContainers
- XUnit v3
- FsCheck
- Verify.NET