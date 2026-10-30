-- schema.sql: estructura base. Idempotente. Se ejecuta desde masterSQL.sql (ver README).
-- ids: AUTOINCREMENT, nunca se reutilizan (como IDENTITY en SQL Server).
-- Dinero: TEXT, valor exacto (REAL tiene errores de redondeo). En SQL Server: DECIMAL(18,2).
-- Fechas: DATETIME como texto ISO 8601, en UTC.
-- FKs sin ON DELETE CASCADE: no se borran cuentas ni movimientos en cascada.

PRAGMA foreign_keys = ON;

-- Clientes ------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Clientes (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre      VARCHAR(255) NOT NULL,
    apellido    VARCHAR(255) NOT NULL,
    cuit        VARCHAR(11)  NOT NULL,
    created_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT UQ_Clientes_cuit UNIQUE (cuit),
    CONSTRAINT CK_Clientes_cuit CHECK (length(cuit) = 11 AND cuit NOT GLOB '*[^0-9]*')
);

-- Cuentas -------------------------------------------------------------------------------
-- El número de cuenta es único dentro de su sucursal.
CREATE TABLE IF NOT EXISTS Cuentas (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    idCliente    INTEGER     NOT NULL,
    numero       VARCHAR(12) NOT NULL,
    codSucursal  VARCHAR(4)  NOT NULL,
    saldo        TEXT        NOT NULL DEFAULT '0.00',
    created_at   DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at   DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Cuentas_Clientes FOREIGN KEY (idCliente) REFERENCES Clientes(id),
    CONSTRAINT UQ_Cuentas_sucursal_numero UNIQUE (codSucursal, numero),
    CONSTRAINT CK_Cuentas_codSucursal CHECK (length(codSucursal) = 4 AND codSucursal NOT GLOB '*[^0-9]*')
);

-- En SQLite la FK no crea índice.
CREATE INDEX IF NOT EXISTS IX_Cuentas_idCliente ON Cuentas(idCliente);

-- Movimientos ---------------------------------------------------------------------------
-- tipo: texto libre. No se restringen los tipos en la base, se validan en el código.
-- importe: siempre positivo; el tipo define si suma o resta.
CREATE TABLE IF NOT EXISTS Movimientos (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    idCuenta     INTEGER     NOT NULL,
    fecha        DATETIME    NOT NULL,
    tipo         VARCHAR(50) NOT NULL,
    descripcion  VARCHAR(50),
    importe      TEXT        NOT NULL,
    created_at   DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at   DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Movimientos_Cuentas FOREIGN KEY (idCuenta) REFERENCES Cuentas(id)
);

-- Movimientos de una cuenta ordenados por fecha. También cubre la FK.
CREATE INDEX IF NOT EXISTS IX_Movimientos_idCuenta_fecha ON Movimientos(idCuenta, fecha);