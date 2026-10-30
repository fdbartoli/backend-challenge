-- masterSQL.sql: creates or updates the database. Run from the repo root (see README).

.bail on
PRAGMA foreign_keys = ON;

-- 1. Base schema
.read database/schema.sql

-- 2. Versioned changes (conditional ALTERs; see README for the SQL Server pattern)

-- 3. Seed data
.read database/seed.sql