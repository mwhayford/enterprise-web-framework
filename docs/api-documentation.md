# API Documentation

## Overview

This document provides comprehensive documentation for the Enterprise Web Application Framework API. The API follows RESTful principles and uses JWT authentication for secure access.

## Base URL

- **Development**: `https://localhost:7001/api`
- **Staging**: `https://api-staging.yourdomain.com/api`
- **Production**: `https://api.yourdomain.com/api`

## Authentication

All API endpoints require JWT authentication except for the authentication endpoints themselves. Include the JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "data": {},
  "success": true,
  "message": "Operation completed successfully"
}
```

### Error Response
```json
{
  "error": "validation_failed",
  "message": "The request contains invalid data",
  "details": {
    "field": "email",
    "code": "invalid_format"
  }
}
```

## HTTP Status Codes

- `200 OK` - Request successful
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource conflict
- `422 Unprocessable Entity` - Validation failed
- `500 Internal Server Error` - Server error

## API Endpoints

### Authentication Endpoints

#### POST /api/auth/google-login
Initiate Google OAuth authentication flow.

**Response:**
```json
{
  "data": {
    "redirectUrl": "https://accounts.google.com/oauth/authorize?..."
  },
  "success": true
}
```

#### GET /api/auth/google-response
Handle Google OAuth callback and authenticate user.

**Query Parameters:**
- `code` (string, required) - Authorization code from Google
- `state` (string, required) - State parameter for security

**Response:**
```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "user": {
      "id": "user-id",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe"
    }
  },
  "success": true
}
```

#### POST /api/auth/refresh
Refresh expired JWT token.

**Request Body:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

**Response:**
```json
{
  "data": {
    "token": "new_jwt_token_here"
  },
  "success": true
}
```

#### POST /api/auth/logout
Logout user and revoke tokens.

**Response:**
```json
{
  "data": null,
  "success": true,
  "message": "Logged out successfully"
}
```

### User Management Endpoints

#### GET /api/users/me
Get current user profile.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Response:**
```json
{
  "data": {
    "id": "user-id",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "displayName": "John Doe",
    "avatarUrl": "https://example.com/avatar.jpg",
    "stripeCustomerId": "cus_stripe_id",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "success": true
}
```

#### PUT /api/users/me
Update current user profile.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "displayName": "John Doe"
}
```

**Response:**
```json
{
  "data": {
    "id": "user-id",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "displayName": "John Doe",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "success": true
}
```

#### GET /api/users
List users (admin only).

**Headers:**
- `Authorization: Bearer <token>` (required)

**Query Parameters:**
- `page` (number, optional) - Page number (default: 1)
- `pageSize` (number, optional) - Items per page (default: 20)
- `search` (string, optional) - Search term

**Response:**
```json
{
  "data": {
    "users": [
      {
        "id": "user-id",
        "email": "user@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pagination": {
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 5,
      "totalRecords": 100
    }
  },
  "success": true
}
```

### Payment Method Endpoints

#### GET /api/payments/methods
Get user's payment methods.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Response:**
```json
{
  "data": [
    {
      "id": "pm_stripe_id",
      "type": "Card",
      "lastFour": "4242",
      "isDefault": true,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "success": true
}
```

#### POST /api/payments/methods
Add new payment method.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Request Body:**
```json
{
  "paymentMethodToken": "pm_token_from_stripe",
  "type": "Card",
  "isDefault": false
}
```

**Response:**
```json
{
  "data": {
    "id": "pm_stripe_id",
    "type": "Card",
    "lastFour": "4242",
    "isDefault": false,
    "createdAt": "2024-01-01T00:00:00Z"
  },
  "success": true
}
```

#### DELETE /api/payments/methods/{id}
Remove payment method.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Path Parameters:**
- `id` (string, required) - Payment method ID

**Response:**
```json
{
  "data": null,
  "success": true,
  "message": "Payment method removed successfully"
}
```

### Payment Endpoints

#### POST /api/payments/process
Process one-time payment.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Request Body:**
```json
{
  "amount": 1000,
  "currency": "USD",
  "paymentMethodId": "pm_stripe_id",
  "description": "Payment for services"
}
```

**Response:**
```json
{
  "data": {
    "id": "payment-id",
    "amount": 1000,
    "currency": "USD",
    "status": "Succeeded",
    "paymentMethodType": "Card",
    "stripePaymentIntentId": "pi_stripe_id",
    "description": "Payment for services",
    "createdAt": "2024-01-01T00:00:00Z",
    "processedAt": "2024-01-01T00:00:00Z"
  },
  "success": true
}
```

#### GET /api/payments/history
Get payment history.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Query Parameters:**
- `page` (number, optional) - Page number (default: 1)
- `pageSize` (number, optional) - Items per page (default: 20)
- `status` (string, optional) - Filter by status
- `startDate` (string, optional) - Start date filter (ISO 8601)
- `endDate` (string, optional) - End date filter (ISO 8601)

**Response:**
```json
{
  "data": {
    "payments": [
      {
        "id": "payment-id",
        "amount": 1000,
        "currency": "USD",
        "status": "Succeeded",
        "paymentMethodType": "Card",
        "description": "Payment for services",
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pagination": {
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 3,
      "totalRecords": 50
    }
  },
  "success": true
}
```

### Subscription Endpoints

#### GET /api/subscriptions
Get user's subscriptions.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Response:**
```json
{
  "data": [
    {
      "id": "subscription-id",
      "planId": "plan-basic",
      "amount": 2999,
      "currency": "USD",
      "status": "Active",
      "stripeSubscriptionId": "sub_stripe_id",
      "currentPeriodStart": "2024-01-01T00:00:00Z",
      "currentPeriodEnd": "2024-02-01T00:00:00Z",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "success": true
}
```

#### POST /api/subscriptions
Create new subscription.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Request Body:**
```json
{
  "planId": "plan-basic",
  "paymentMethodId": "pm_stripe_id"
}
```

**Response:**
```json
{
  "data": {
    "id": "subscription-id",
    "planId": "plan-basic",
    "amount": 2999,
    "currency": "USD",
    "status": "Active",
    "stripeSubscriptionId": "sub_stripe_id",
    "currentPeriodStart": "2024-01-01T00:00:00Z",
    "currentPeriodEnd": "2024-02-01T00:00:00Z",
    "createdAt": "2024-01-01T00:00:00Z"
  },
  "success": true
}
```

#### PUT /api/subscriptions/{id}
Update subscription.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Path Parameters:**
- `id` (string, required) - Subscription ID

**Request Body:**
```json
{
  "planId": "plan-premium",
  "paymentMethodId": "pm_stripe_id"
}
```

**Response:**
```json
{
  "data": {
    "id": "subscription-id",
    "planId": "plan-premium",
    "amount": 4999,
    "currency": "USD",
    "status": "Active",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "success": true
}
```

#### DELETE /api/subscriptions/{id}
Cancel subscription.

**Headers:**
- `Authorization: Bearer <token>` (required)

**Path Parameters:**
- `id` (string, required) - Subscription ID

**Response:**
```json
{
  "data": {
    "id": "subscription-id",
    "status": "Canceled",
    "canceledAt": "2024-01-01T00:00:00Z"
  },
  "success": true
}
```

### Webhook Endpoints

#### POST /api/webhooks/stripe
Handle Stripe webhook events.

**Headers:**
- `Stripe-Signature` (string, required) - Stripe webhook signature

**Request Body:**
Raw Stripe webhook event payload.

**Response:**
```json
{
  "data": null,
  "success": true
}
```

## Error Codes

### Authentication Errors
- `AUTH_TOKEN_MISSING` - JWT token not provided
- `AUTH_TOKEN_INVALID` - JWT token invalid or expired
- `AUTH_TOKEN_EXPIRED` - JWT token expired
- `AUTH_INSUFFICIENT_PERMISSIONS` - User lacks required permissions

### Validation Errors
- `VALIDATION_FAILED` - Request validation failed
- `INVALID_EMAIL_FORMAT` - Email format is invalid
- `INVALID_PAYMENT_AMOUNT` - Payment amount is invalid
- `INVALID_PAYMENT_METHOD` - Payment method is invalid

### Payment Errors
- `PAYMENT_FAILED` - Payment processing failed
- `PAYMENT_METHOD_NOT_FOUND` - Payment method not found
- `INSUFFICIENT_FUNDS` - Insufficient funds for payment
- `CARD_DECLINED` - Credit card declined

### Subscription Errors
- `SUBSCRIPTION_NOT_FOUND` - Subscription not found
- `SUBSCRIPTION_ALREADY_ACTIVE` - Subscription already active
- `SUBSCRIPTION_CANCELED` - Subscription is canceled
- `INVALID_PLAN_ID` - Plan ID is invalid

## Rate Limiting

The API implements rate limiting to prevent abuse:

- **Authentication endpoints**: 5 requests per minute per IP
- **Payment endpoints**: 10 requests per minute per user
- **General endpoints**: 100 requests per minute per user

Rate limit headers are included in responses:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1640995200
```

## SDKs and Libraries

### JavaScript/TypeScript
```bash
npm install axios
```

### C#
```bash
dotnet add package Microsoft.Extensions.Http
```

### Python
```bash
pip install requests
```

## Testing

### Postman Collection
A Postman collection is available for testing the API endpoints. Import the collection and configure environment variables for different environments.

### cURL Examples

#### Authenticate with Google OAuth
```bash
curl -X GET "https://api.yourdomain.com/api/auth/google-login"
```

#### Get user profile
```bash
curl -X GET "https://api.yourdomain.com/api/users/me" \
  -H "Authorization: Bearer <your-jwt-token>"
```

#### Process payment
```bash
curl -X POST "https://api.yourdomain.com/api/payments/process" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 1000,
    "currency": "USD",
    "paymentMethodId": "pm_stripe_id",
    "description": "Test payment"
  }'
```

## Support

For API support and questions:
- **Email**: api-support@yourdomain.com
- **Documentation**: https://docs.yourdomain.com
- **Status Page**: https://status.yourdomain.com
