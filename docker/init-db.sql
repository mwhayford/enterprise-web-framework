-- Initialize Core Database
-- This script runs when the PostgreSQL container starts for the first time

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create database if it doesn't exist (handled by POSTGRES_DB env var)
-- The database CoreDb_Dev will be created automatically

-- Create additional schemas if needed
-- CREATE SCHEMA IF NOT EXISTS audit;
-- CREATE SCHEMA IF NOT EXISTS search;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE "CoreDb_Dev" TO postgres;

-- Create any initial tables or data here
-- Note: Entity Framework will handle table creation through migrations
