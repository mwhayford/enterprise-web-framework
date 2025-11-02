# Debugging CI Container Crashes

This guide explains where to find information about container crashes in GitHub Actions CI/CD pipelines.

## Where to Find Crash Information

### 1. GitHub Actions Workflow Logs

The primary place to see crash details is in the **GitHub Actions workflow logs**:

1. Go to your GitHub repository
2. Click the **Actions** tab
3. Select the failed workflow run (usually the most recent one)
4. Click on the failed job (e.g., "E2E Tests")
5. Expand the step that failed (usually "Wait for containers to initialize" or "Start Docker services")

### 2. Container Logs Output

The workflow automatically captures container logs in these steps:

- **"Capture container logs immediately"**: Shows logs right after `docker compose up`
- **"Wait for containers to initialize"**: Shows logs after a 60-second wait
- **"Show backend logs if health check fails"**: Shows detailed logs if the health check fails

Look for sections marked with:
- `=== Container Status ===` - Shows which containers are running/exited
- `=== Full Backend Container Logs ===` - Complete stdout/stderr from the backend container
- `=== Backend Container Exit Code ===` - Exit code (139 = segmentation fault)
- `[DEBUG]` prefixed messages - Our debug logging output

### 3. Debug Log Messages

Our application logs debug messages at key startup points:

```
[DEBUG] Kafka configuration check:
[DEBUG]   BootstrapServers: '...'
[DEBUG]   KafkaEnabled: ...
[DEBUG] Starting application build...
[DEBUG] Application build completed successfully
[DEBUG] About to start web server (app.Run())...
```

**The last `[DEBUG]` message before the crash tells you where it failed:**

- If you see `[DEBUG] Starting application build...` but not `[DEBUG] Application build completed successfully` → Crash during `app.Build()`
- If you see build completed but not `[DEBUG] About to start web server...` → Crash during middleware setup
- If you see `[DEBUG] About to start web server...` but exit code 139 → Crash during `app.Run()`

### 4. Container Exit Codes

- **Exit Code 139**: Segmentation fault (SIGSEGV) - Native code crash (Kafka, Elasticsearch, etc.)
- **Exit Code 1**: General application error
- **Exit Code 0**: Success (container stopped normally)

### 5. Docker Compose Logs

To view logs locally (if running Docker Compose on your machine):

```bash
# View all container logs
docker compose -f docker-compose.ci.yml logs

# View only backend logs
docker compose -f docker-compose.ci.yml logs backend

# Follow logs in real-time
docker compose -f docker-compose.ci.yml logs -f backend

# View last 100 lines
docker compose -f docker-compose.ci.yml logs --tail=100 backend
```

### 6. Container Inspection

If you need to inspect a crashed container locally:

```bash
# Check container status
docker ps -a | grep rentalmanager-backend-ci

# Inspect container details
docker inspect rentalmanager-backend-ci

# View exit code
docker inspect rentalmanager-backend-ci --format='{{.State.ExitCode}}'

# View environment variables
docker inspect rentalmanager-backend-ci --format='{{range .Config.Env}}{{println .}}{{end}}'
```

## Common Crash Scenarios

### Exit Code 139 (Segmentation Fault)

This indicates a native library crash. Common causes:

1. **Kafka client initialization** - Fixed by disabling Kafka in CI
2. **Elasticsearch client initialization** - Fixed by disabling Elasticsearch in CI
3. **Native SSL/TLS libraries** - Can crash if certificates are missing
4. **OpenTelemetry native components** - Wrapped in try-catch
5. **Hangfire PostgreSQL driver** - Fixed by disabling Hangfire in CI

### Missing Debug Logs

If you don't see any `[DEBUG]` messages:
- Container crashed before reaching our debug logging code
- Check Docker build logs for compilation errors
- Check if the Dockerfile `CMD` or `ENTRYPOINT` is correct

### Container Starts Then Immediately Exits

If the container starts but exits immediately:
- Check the exit code (139 = native crash, 1 = app error)
- Look for the last log line before exit
- Check if all required environment variables are set
- Verify database/Redis connection strings are valid

## Enabling More Verbose Logging

To see more detailed logs, you can:

1. **Set ASPNETCORE environment variables**:
   ```yaml
   environment:
     - ASPNETCORE_ENVIRONMENT=Development
     - Logging__LogLevel__Default=Debug
     - Logging__LogLevel__Microsoft=Information
   ```

2. **Add more Console.WriteLine in Program.cs**:
   ```csharp
   Console.WriteLine($"[DEBUG] Before Kafka registration...");
   // ... code ...
   Console.WriteLine($"[DEBUG] After Kafka registration...");
   ```

3. **Enable .NET runtime logging**:
   ```yaml
   environment:
     - DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP2FLOWCONTROL_DISABLED=1
     - DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP2FLOWCONTROL_INITIALWINDOWSIZE=65536
   ```

## Workflow Improvements

The workflow has been enhanced with automatic log capture:

1. **Immediate log capture** - Logs are captured right after `docker compose up`
2. **Detailed container inspection** - Shows exit codes, status, and environment
3. **All container logs** - Shows logs from all services for context
4. **Failure-specific logs** - Enhanced logging when health checks fail

## Next Steps After Identifying Crash Point

Once you identify where the crash occurs:

1. **During app.Build()**: Check service registration (Kafka, Elasticsearch, Hangfire)
2. **During middleware setup**: Check authentication/authorization configuration
3. **During app.Run()**: Check Kestrel server configuration, HTTPS settings
4. **After startup**: Check database initialization, seeding, or hosted services

See the main README for more debugging information.
