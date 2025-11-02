# RentalManager Site Map

## Frontend Routes (React Application)

### Public Routes
- `/` - Landing Page (homepage with features, pricing, testimonials)
- `/login` - Login Page (Google OAuth authentication)
- `/auth-callback` - OAuth callback handler
- `/properties` - Browse Properties (public property listings)
- `/properties/:id` - Property Detail Page (public property information)

### Protected Routes (Requires Authentication)
- `/dashboard` - User Dashboard (overview of user's properties, applications, leases)
- `/search` - Search Page (Elasticsearch-powered search)
- `/payment` - Payment Page (make one-time payments)
- `/payment-success` - Payment Success Confirmation
- `/payment-methods` - Manage Payment Methods (add/edit/delete Stripe payment methods)
- `/subscription` - Subscription Page (manage subscriptions)
- `/subscription-success` - Subscription Success Confirmation
- `/properties/:id/apply` - Property Application Form (submit rental application)
- `/applications/my` - My Applications (user's submitted applications)
- `/admin/applications` - Admin Applications List (all applications - admin only)
- `/admin/applications/:id` - Application Review Page (review application - admin only)
- `/leases` - My Leases (user's active leases)
- `/leases/:id` - Lease Detail Page (detailed lease information)

## Backend API Endpoints (ASP.NET Core)

### Base URL: `http://localhost:5111/api`

### Authentication (`/api/Auth`)
- `GET /api/Auth/google` - Initiate Google OAuth login
- `GET /api/Auth/google-callback` - Handle Google OAuth callback
- `GET /api/Auth/debug-config` - Debug endpoint (shows auth config - dev only)
- `POST /api/Auth/refresh` - Refresh JWT token (requires authentication)
- `POST /api/Auth/logout` - Logout user

### Properties (`/api/Properties`)
- `GET /api/Properties` - Get available properties (public, supports query parameters)
- `GET /api/Properties/{id}` - Get property by ID (public)
- `POST /api/Properties` - Create property (requires authentication)
- `PUT /api/Properties/{id}` - Update property (requires authentication)

### Applications (`/api/Applications`)
- `POST /api/Applications` - Submit property application (requires authentication)
- `GET /api/Applications/my` - Get current user's applications (requires authentication)
- `GET /api/Applications/{id}` - Get application by ID (requires authentication)
- `PUT /api/Applications/{id}` - Update application status (requires authentication)

### Leases (`/api/Leases`)
- `GET /api/Leases` - Get user's leases (requires authentication)
- `GET /api/Leases/{id}` - Get lease by ID (requires authentication)
- `POST /api/Leases` - Create lease (requires authentication)
- `PUT /api/Leases/{id}` - Update lease (requires authentication)

### Users (`/api/Users`)
- `GET /api/Users/me` - Get current user profile (requires authentication)
- `PUT /api/Users/me` - Update current user profile (requires authentication)
- `GET /api/Users/{id}` - Get user by ID (requires authentication)

### Payments (`/api/Payments`)
- `POST /api/Payments` - Process payment (requires authentication)
- `GET /api/Payments` - Get user's payments (requires authentication)
- `GET /api/Payments/{id}` - Get payment by ID (requires authentication)

### Payment Methods (`/api/PaymentMethods`)
- `POST /api/PaymentMethods` - Create payment method (requires authentication)
- `GET /api/PaymentMethods` - Get user's payment methods (requires authentication)
- `DELETE /api/PaymentMethods/{paymentMethodId}` - Delete payment method (requires authentication)
- `PUT /api/PaymentMethods/{paymentMethodId}/set-default` - Set default payment method (requires authentication)

### Subscriptions (`/api/Subscriptions`)
- `POST /api/Subscriptions` - Create subscription (requires authentication)
- `GET /api/Subscriptions` - Get user's subscriptions (requires authentication)
- `PUT /api/Subscriptions/{id}` - Update subscription (requires authentication)
- `DELETE /api/Subscriptions/{id}` - Cancel subscription (requires authentication)

### Search (`/api/Search`)
- `GET /api/Search` - General search (requires authentication)
- `POST /api/Search/index` - Index document (requires authentication)
- `POST /api/Search/users` - Search users (requires authentication)

### Work Orders (`/api/WorkOrders`)
- `GET /api/WorkOrders` - Get work orders (requires authentication)
- `GET /api/WorkOrders/{id}` - Get work order by ID (requires authentication)
- `POST /api/WorkOrders` - Create work order (requires authentication)
- `PUT /api/WorkOrders/{id}` - Update work order (requires authentication)
- `DELETE /api/WorkOrders/{id}` - Delete work order (requires authentication)

### Admin (`/api/Admin`)
- `POST /api/Admin/seed-properties` - Seed test properties (admin only)
  - Query params: `count` (1-10000), `ownerId` (optional)

### Background Jobs (`/api/BackgroundJobs`)
- `GET /api/BackgroundJobs` - Get background job status (requires authentication)
- `POST /api/BackgroundJobs/{jobId}/retry` - Retry failed job (requires authentication)

### Application Settings (`/api/ApplicationSettings`)
- `GET /api/ApplicationSettings` - Get application settings (requires authentication)
- `PUT /api/ApplicationSettings` - Update application settings (requires authentication)

### Health (`/api/Health`)
- `GET /api/Health` - Health check endpoint

### System Endpoints
- `GET /health` - Health check (returns application health status)
- `GET /metrics` - Prometheus metrics endpoint
- `GET /hangfire` - Hangfire dashboard (background jobs monitoring)
- `GET /swagger` - Swagger/OpenAPI documentation (development only)

## User Roles

- **Admin** - Full system access, can seed data, manage all applications
- **Owner** - Property owners, can create/manage properties
- **Resident** - Tenants, can view properties and submit applications
- **Contractor** - Maintenance contractors, can view/manage work orders

## External Services

- **PostgreSQL** - Database (port 5433)
- **Redis** - Caching (port 6380)
- **Elasticsearch** - Search indexing (port 9200)
- **Kafka** - Event bus (port 9092)
- **Prometheus** - Metrics (port 9090)
- **Jaeger** - Distributed tracing (port 16686)
- **Grafana** - Monitoring dashboards (port 3002)

## Authentication Flow

1. User clicks "Login" on `/login`
2. Redirects to `/api/Auth/google` which initiates Google OAuth
3. Google redirects to `/api/Auth/google-callback` with authorization code
4. Backend exchanges code for tokens and creates/updates user
5. Redirects to `/auth-callback` with JWT token
6. Frontend stores token and redirects to `/dashboard`

## Data Flow

### Property Application Flow
1. Browse properties (`/properties`)
2. View property details (`/properties/:id`)
3. Submit application (`/properties/:id/apply`)
4. View application status (`/applications/my`)
5. Admin reviews (`/admin/applications/:id`)
6. Application approved → Lease created (`/leases/:id`)

### Payment Flow
1. Select payment method (`/payment-methods`)
2. Make payment (`/payment`)
3. Payment success (`/payment-success`)

### Subscription Flow
1. Select subscription plan (`/subscription`)
2. Payment processing
3. Subscription success (`/subscription-success`)

