# Condubello

API REST (.NET 10) + frontend Angular 20 (Angular Material) para una escuela de conducción.
Se ejecuta directamente en el PC, sin Docker.

## Requisitos

- .NET SDK 10
- Node.js >= 20.19 y npm
- PostgreSQL local (puerto 5432, usuario `postgres`)

## 1. Base de datos

Crear una base de datos llamada **Condubello** (por ejemplo desde pgAdmin: Databases → Create → Database).

La conexión está en `src/Condubello.Api/appsettings.json`:

```
Host=localhost;Port=5432;Database=Condubello;Username=postgres;Password=12345
```

Ajusta la contraseña si la tuya es distinta. Las tablas se crean solas al iniciar la API en desarrollo (migraciones automáticas).

## 2. Ejecutar la API

```bash
cd src/Condubello.Api
dotnet run --launch-profile http
```

Queda en http://localhost:5086 (Swagger en http://localhost:5086/swagger).

## 3. Ejecutar el gateway

En otra terminal:

```bash
cd src/Condubello.Gateway
dotnet run --launch-profile http
```

Queda en http://localhost:5000. Es el único punto de entrada del frontend (YARP): reenvía `/api/*` a la API (http://localhost:5086), valida el JWT (solo `login` y `registro` son públicos), centraliza el CORS y registra cada petición en consola. `GET /health` comprueba que está activo.

- La clave `Jwt:Key` del gateway debe ser **idéntica** a la de la API (en desarrollo ya coinciden en `appsettings.Development.json`).
- La dirección de la API se cambia en `ReverseProxy:Clusters:api:Destinations` de `src/Condubello.Gateway/appsettings.json`.
- En producción solo debe exponerse el gateway; la API no.

## 4. Ejecutar el frontend

En otra terminal:

```bash
cd src/Condubello.Web
npm install      # solo la primera vez
npm start
```

Queda en http://localhost:4200 y consulta siempre al gateway (`src/Condubello.Web/src/app/core/config.ts`, `API_URL`).

Flujo: `Angular (4200) → Gateway (5000) → API (5086) → PostgreSQL`.

## Acceso inicial

- Correo: `admin@condubello.com`
- Contraseña: `Admin123*`

El usuario se crea automáticamente al primer arranque si no existe ningún administrativo.
