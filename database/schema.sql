-- schema.sql: base schema. Idempotent. Run through masterSQL.sql (see README).
-- ids: AUTOINCREMENT, never reused (same as IDENTITY in SQL Server).
-- Money: TEXT to keep exact values (REAL has rounding errors). In SQL Server: DECIMAL(18,2).
-- Dates: DATETIME stored as ISO 8601 text, in UTC.
-- FKs without ON DELETE CASCADE: accounts and transactions are never deleted in cascade.

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
-- Account number is unique within its branch.
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

-- SQLite does not create an index for foreign keys.
CREATE INDEX IF NOT EXISTS IX_Cuentas_idCliente ON Cuentas(idCliente);

-- Movimientos ---------------------------------------------------------------------------
-- tipo: free text. Types are not restricted in the database; they are validated in code.
-- importe: always positive; tipo defines whether it adds to or subtracts from the balance.
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

-- Account transactions ordered by date. Also covers the foreign key.
CREATE INDEX IF NOT EXISTS IX_Movimientos_idCuenta_fecha ON Movimientos(idCuenta, fecha);