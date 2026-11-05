# Condubello

API REST (.NET 10) + frontend Angular 20 (Angular Material) para una escuela de conducción.

## Con Docker (todo el proyecto)

```bash
cp .env.example .env        # edita POSTGRES_PASSWORD, JWT_KEY (>= 32 caracteres) y SEED_ADMIN_PASSWORD
docker compose up -d --build
```

- Web: http://localhost:8080 (nginx sirve el SPA y reenvía `/api` a la API)
- API directa: http://localhost:5086
- Usuario inicial: `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD`
- Las migraciones de EF se aplican al arrancar (`Database__MigrateOnStartup=true`).

Servicios: `postgres`, `api` (`docker/api.Dockerfile`), `web` (`docker/web.Dockerfile` + `docker/nginx.conf`).

## Desarrollo local

```bash
docker compose up -d postgres
dotnet run --project src/Condubello.Api --launch-profile http   # http://localhost:5086
cd src/Condubello.Web && npm install && npm start               # http://localhost:4200 (proxy /api -> 5086)
```

Angular 20 requiere Node >= 20.19. Credenciales de desarrollo: `admin@condubello.com` / `Admin123*`.
