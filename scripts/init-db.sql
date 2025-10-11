-- Database initialization script for Core application
-- This script runs when the PostgreSQL container starts for the first time

-- Create extensions that might be useful
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Set timezone
SET timezone = 'UTC';

-- Create any additional schemas if needed
-- CREATE SCHEMA IF NOT EXISTS audit;
-- CREATE SCHEMA IF NOT EXISTS reporting;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE "CoreDb" TO postgres;

-- Log successful initialization
DO $$
BEGIN
    RAISE NOTICE 'Core database initialized successfully';
END $$;

