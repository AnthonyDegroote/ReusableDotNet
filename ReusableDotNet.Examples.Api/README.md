# ReusableDotNet.Examples.Api

Exemple minimal ASP.NET Core pour tester `ReusableDotNet.Pagination` avec Swagger/OpenAPI.

## Démarrage

Depuis la racine du repo :

```powershell
dotnet run --project ReusableDotNet.Examples.Api/ReusableDotNet.Examples.Api.csproj
```

Swagger UI :

- `https://localhost:xxxx/swagger`
- `http://localhost:xxxx/swagger`

## Endpoints utiles

- `GET /api/pagination/config` : configuration actuelle.
- `POST /api/pagination/default-page-size/{pageSize}` : démonstration de la nouvelle API mutable `SetDefaultPageSize`.
- `GET /api/pagination/pages/{pageNumber}?pageSize=...` : récupère une page (et mémorise le curseur interne côté serveur).
- `POST /api/pagination/next` : page suivante depuis la dernière page mémorisée.
- `POST /api/pagination/previous` : page précédente depuis la dernière page mémorisée.
- `POST /api/pagination/next/external` : tente un `Next` avec un `PageResult` externe (doit échouer avec `InvalidOperationException`).

## Objectif pédagogique

Ce projet montre :

1. L’usage standard de `Paginator<T>`.
2. La mise à jour de la taille de page par défaut via `SetDefaultPageSize`.
3. La protection `Next/Previous` contre un `PageResult` créé hors du paginator courant.
