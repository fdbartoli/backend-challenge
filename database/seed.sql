-- seed.sql: initial data. Idempotent: can run N times without duplicating rows.
-- Each row is identified by its natural key and only inserted if it does not exist.
-- INSERT OR IGNORE is not used because it would also hide data errors (CHECK, NOT NULL).
-- Foreign keys are resolved with LEFT JOIN: if the parent is missing, NOT NULL fails
-- instead of silently skipping the row.
--
-- CUIT 30610252334 (Arcos Dorados) has data in the BCRA Central de Deudores.
-- The others are made up, with a valid check digit; BCRA returns 404 (no debts).

BEGIN TRANSACTION;

-- Clientes: natural key = cuit
WITH datos (nombre, apellido, cuit) AS (
    VALUES
        ('Arcos Dorados Argentina', 'S.A.',      '30610252334'),
        ('Lucía',                   'Fernández', '27990000021'),
        ('Martín',                  'Gómez',     '20990000019'),
        ('Sofía',                   'Ramírez',   '20990000035')   -- client with no accounts
)
INSERT INTO Clientes (nombre, apellido, cuit)
SELECT d.nombre, d.apellido, d.cuit
FROM datos d
WHERE NOT EXISTS (SELECT 1 FROM Clientes c WHERE c.cuit = d.cuit);

-- Cuentas: natural key = (codSucursal, numero). Each balance matches its transactions.
WITH datos (cuit, codSucursal, numero, saldo) AS (
    VALUES
        ('30610252334', '0001', '000100000001', '1244999.50'),
        ('30610252334', '0001', '000100000002', '2000000.00'),
        ('27990000021', '0002', '000200000001', '809211.33'),
        ('20990000019', '0003', '000300000001', '309549.60')
)
INSERT INTO Cuentas (idCliente, codSucursal, numero, saldo)
SELECT cl.id, d.codSucursal, d.numero, d.saldo
FROM datos d
LEFT JOIN Clientes cl ON cl.cuit = d.cuit
WHERE NOT EXISTS (
    SELECT 1 FROM Cuentas c WHERE c.codSucursal = d.codSucursal AND c.numero = d.numero
);

-- Movimientos: natural key = (idCuenta, fecha, tipo, importe). Amounts are always positive.
WITH datos (codSucursal, numero, fecha, tipo, descripcion, importe) AS (
    VALUES
        ('0001', '000100000001', '2026-09-01 09:00:00', 'TRANSFERENCIA RECIBIDA',                 'Cobro a proveedor',      '1500000.00'),
        ('0001', '000100000001', '2026-09-05 11:30:00', 'PAGO DE SERVICIOS (DEBITO AUTOMATICO)',  'Energía eléctrica',      '85000.50'),
        ('0001', '000100000001', '2026-09-10 15:45:00', 'TRANSFERENCIA EMITIDA',                  'Pago a proveedor',       '420000.00'),
        ('0001', '000100000001', '2026-09-20 10:00:00', 'DEPÓSITO EN EFECTIVO',                   'Recaudación local',      '250000.00'),
        ('0001', '000100000002', '2026-09-02 10:00:00', 'ACREDITACIÓN DE PLAZO FIJO',             'Vencimiento plazo fijo', '3000000.00'),
        ('0001', '000100000002', '2026-09-15 12:00:00', 'TRANSFERENCIA INMEDIATA SALIENTE (CI)',  'Transferencia interna',  '1000000.00'),
        ('0002', '000200000001', '2026-09-01 08:00:00', 'HABERES / SUELDO ACREDITADO',            'Sueldo agosto',          '950000.00'),
        ('0002', '000200000001', '2026-09-03 18:20:00', 'COMPRA TARJETA DE DÉBITO',               'Supermercado',           '45320.75'),
        ('0002', '000200000001', '2026-09-07 09:10:00', 'EXTRACCIÓN EN EFECTIVO (CAJERO)',        NULL,                     '100000.00'),
        ('0002', '000200000001', '2026-09-12 13:00:00', 'REINTEGRO TARJETA DE DÉBITO',            'Promoción supermercado', '4532.08'),
        ('0003', '000300000001', '2026-09-01 08:00:00', 'HABERES / SUELDO ACREDITADO',            'Sueldo agosto',          '720000.00'),
        ('0003', '000300000001', '2026-09-10 10:00:00', 'PAGO DE PRÉSTAMO (CUOTA)',               'Cuota 3 de 12',          '180000.00'),
        ('0003', '000300000001', '2026-09-25 09:30:00', 'DÉBITO AUTOMÁTICO TARJETA DE CRÉDITO',   'Resumen septiembre',     '230450.40')
)
INSERT INTO Movimientos (idCuenta, fecha, tipo, descripcion, importe)
SELECT cu.id, d.fecha, d.tipo, d.descripcion, d.importe
FROM datos d
LEFT JOIN Cuentas cu ON cu.codSucursal = d.codSucursal AND cu.numero = d.numero
WHERE NOT EXISTS (
    SELECT 1 FROM Movimientos m
    WHERE m.idCuenta = cu.id AND m.fecha = d.fecha AND m.tipo = d.tipo AND m.importe = d.importe
);

COMMIT;