# Banco API

## Requisitos

- **Con Docker:** solo Docker. El SDK de .NET y sqlite3 se usan dentro de la imagen durante el build.
- **Sin Docker:** .NET SDK 10 y `sqlite3` en el PATH.

## Base de datos

### Orden de ejecución

`database/masterSQL.sql` es el único script que se ejecuta. Llama, en este orden, a:

1. `schema.sql`: crea tablas e índices.
2. Cambios versionados (ALTERs condicionales). Hoy no hay ninguno.
3. `seed.sql`: carga los datos iniciales.

### Comando

Desde la raíz del repositorio:

```bash
sqlite3 src/Banco.Api/banco.db ".read database/masterSQL.sql"
```

El script es idempotente: se puede ejecutar varias veces sin errores ni datos duplicados.

### Patrón en SQL Server

SQLite soporta `CREATE TABLE/INDEX IF NOT EXISTS`, pero no ALTERs condicionales. En SQL Server, los cambios de estructura se agregan en la sección 2 del master con un chequeo de catálogo:

```sql
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Clientes') AND name = N'email'
)
BEGIN
    ALTER TABLE dbo.Clientes ADD email VARCHAR(255) NULL;
END;
```

También se puede usar `INFORMATION_SCHEMA.COLUMNS` en lugar de `sys.columns`.

## Ejecución

```bash
cd src/Banco.Api
dotnet run
```

La API queda en http://localhost:5245 (Swagger en http://localhost:5245/swagger).

## Docker

Construir la imagen, desde la raíz del repositorio:

```bash
docker build -t banco-api .
```

Correr el contenedor:

```bash
docker run -d --name banco-api -p 8080:8080 banco-api
```

La API queda en http://localhost:8080. La base de datos se crea durante el build con `masterSQL.sql`.

Para ver Swagger dentro del contenedor, levantarlo en entorno Development:

```bash
docker run -d --name banco-api -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development banco-api
```

Swagger queda en http://localhost:8080/swagger.

La configuración se pasa por variables de entorno:

```bash
docker run -d --name banco-api -p 8080:8080 \
  -e Database__ConnectionString="Data Source=/app/data/banco.db;Mode=ReadWrite;Foreign Keys=True" \
  -e Bcra__BaseUrl="https://api.bcra.gob.ar/centraldedeudores/v1.0/" \
  -e Cache__BcraExpirationSeconds=300 \
  banco-api
```

## Datos de ejemplo

| CUIT | Cliente | Situación BCRA |
|---|---|---|
| 30610252334 | Arcos Dorados Argentina S.A. | Con datos (CUIT real) |
| 27990000021 | Lucía Fernández | Sin datos (CUIT ficticio) |
| 20990000019 | Martín Gómez | Sin datos |
| 20990000035 | Sofía Ramírez | Sin datos, sin cuentas |

Cuentas: ids 1 a 4. La colección de Postman (`postman/`) tiene un request por cada caso.

## Postman

Importar `postman/Banco.postman_collection.json`. La variable de colección `baseUrl` apunta a `http://localhost:5245` (Visual Studio); para Docker, cambiarla a `http://localhost:8080`.

## Fallback ante fallas del BCRA

Si el BCRA no responde (error, timeout o circuito abierto), `GET /api/customers/{cuit}` devuelve **200** con los datos del cliente y sus cuentas, y la situación crediticia marcada como no disponible:

```json
"bcra": {
  "status": "Unavailable",
  "period": null,
  "worstSituation": null,
  "totalAmount": null,
  "entities": []
}
```
