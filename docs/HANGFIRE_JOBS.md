# Hangfire Jobs Documentation

## Overview

RentalManager uses Hangfire for background job processing. Jobs are stored in PostgreSQL and executed by Hangfire workers.

## Access

- **Dashboard**: http://localhost:5111/hangfire
- **Storage**: PostgreSQL database (same as application database)
- **Queues**: `default`, `emails`, `data-processing`

## Recurring Jobs

Recurring jobs are automatically registered at application startup via `RecurringJobsService`.

### 1. cleanup-expired-sessions
- **Schedule**: Every hour (`0 * * * *`)
- **Service**: `DataProcessingService.CleanupExpiredSessionsAsync()`
- **Purpose**: Cleans up expired user sessions
- **Status**: Active

### 2. update-search-index
- **Schedule**: Every 6 hours (`0 */6 * * *`)
- **Service**: `DataProcessingService.UpdateSearchIndexAsync()`
- **Purpose**: Updates Elasticsearch search index
- **Status**: Active

### 3. payment-reconciliation
- **Schedule**: Daily at 2 AM (`0 2 * * *`)
- **Service**: `DataProcessingService.ProcessPaymentReconciliationAsync()`
- **Purpose**: Processes payment reconciliation tasks
- **Status**: Active

## One-Time/Enqueued Jobs

These jobs are enqueued on-demand when specific events occur.

### Application Notification Jobs

**Service**: `ApplicationNotificationJobs` (implements `IApplicationNotificationJobs`)

#### SendApplicationSubmittedEmailAsync
- **Trigger**: When a property application is submitted
- **Location**: `SubmitPropertyApplicationCommandHandler`
- **Retry**: 3 automatic retries
- **Purpose**: Sends confirmation email to applicant

#### SendApplicationFeePaymentConfirmationAsync
- **Trigger**: When application fee payment is confirmed
- **Location**: `ConfirmApplicationFeeCommandHandler`
- **Retry**: 3 automatic retries
- **Purpose**: Sends payment confirmation email to applicant

#### SendApplicationStatusUpdateEmailAsync
- **Trigger**: When application status changes (approved/rejected)
- **Location**: `ApproveApplicationCommandHandler`, `RejectApplicationCommandHandler`
- **Retry**: 3 automatic retries
- **Purpose**: Sends status update email to applicant

#### SendOwnerNewApplicationNotificationAsync
- **Trigger**: When a new application is submitted
- **Location**: `SubmitPropertyApplicationCommandHandler`
- **Retry**: 3 automatic retries
- **Purpose**: Notifies property owner of new application

### Email Jobs

#### Welcome Email
- **Service**: `EmailService.SendWelcomeEmailAsync()`
- **Trigger**: `SendWelcomeEmailCommandHandler`
- **Purpose**: Sends welcome email to new users

#### Payment Confirmation Email
- **Service**: `EmailService.SendPaymentConfirmationAsync()`
- **Trigger**: `SendPaymentConfirmationEmailCommandHandler`
- **Purpose**: Sends payment confirmation emails

### Data Processing Jobs

#### ProcessUserDataAsync
- **Service**: `DataProcessingService.ProcessUserDataAsync(Guid userId)`
- **Purpose**: Processes user data asynchronously

#### GenerateUserReportAsync
- **Service**: `DataProcessingService.GenerateUserReportAsync(Guid userId, DateTime fromDate, DateTime toDate)`
- **Purpose**: Generates user reports asynchronously

## Job Configuration

### Hangfire Server Configuration
```csharp
builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "default", "emails", "data-processing" };
    options.WorkerCount = Environment.ProcessorCount * 5;
});
```

### Storage Configuration
- **Provider**: PostgreSQL
- **Connection**: Uses `DefaultConnection` from configuration
- **Tables**: Automatically created by Hangfire in the database

### Retry Policy
- **Default**: 3 automatic retries for notification jobs
- **Retry Delay**: Exponential backoff
- **Failed Jobs**: Stored in database for manual review

## Monitoring

### Hangfire Dashboard Features
- View all jobs (recurring, scheduled, processing, succeeded, failed)
- Retry failed jobs
- Trigger recurring jobs manually
- View job history and execution logs
- Monitor job performance

### Accessing the Dashboard
1. Navigate to: http://localhost:5111/hangfire
2. Requires authentication (filtered by `HangfireAuthorizationFilter`)

## Adding New Jobs

### Recurring Job Example
```csharp
_backgroundJobService.Recurring<DataProcessingService>(
    "job-id",
    service => service.MyMethodAsync(),
    "0 * * * *"); // Cron expression
```

### Enqueued Job Example
```csharp
BackgroundJob.Enqueue<IApplicationNotificationJobs>(
    x => x.SendNotificationAsync(notificationId));
```

### Scheduled Job Example
```csharp
_backgroundJobService.Schedule<DataProcessingService>(
    service => service.ProcessLaterAsync(),
    TimeSpan.FromMinutes(30));
```

## Cron Expression Reference

- `0 * * * *` - Every hour at minute 0
- `0 */6 * * *` - Every 6 hours
- `0 2 * * *` - Daily at 2 AM
- `0 0 * * 0` - Weekly on Sunday at midnight
- `0 0 1 * *` - Monthly on the 1st at midnight

## Troubleshooting

### Jobs Not Running
1. Check Hangfire server is running: Verify `AddHangfireServer()` is called
2. Check database connection: Ensure PostgreSQL is accessible
3. Check logs: Review application logs for Hangfire errors
4. Check dashboard: Verify jobs appear in the dashboard

### Failed Jobs
1. View in dashboard: Check the "Failed" tab
2. View error details: Click on failed job to see exception
3. Retry manually: Use "Retry" button in dashboard
4. Check retry count: Jobs with `[AutomaticRetry]` will retry automatically

### Performance Issues
1. Adjust worker count: Modify `WorkerCount` in `AddHangfireServer()`
2. Use separate queues: Route heavy jobs to dedicated queues
3. Monitor queue length: Use dashboard to see queue depth

## Related Files

- `RecurringJobsService.cs` - Configures recurring jobs
- `HangfireBackgroundJobService.cs` - Wrapper service for Hangfire
- `ApplicationNotificationJobs.cs` - Application-specific notification jobs
- `DataProcessingService.cs` - Data processing operations
- `EmailService.cs` - Email sending service
- `Program.cs` - Hangfire configuration and job registration

