# 🔐 Credentials Setup Guide

> **⚠️ IMPORTANT**: Never commit actual credentials to Git!  
> All credential files are protected by `.gitignore`.

## 📋 Quick Setup Checklist

- [ ] Copy `.env.template` to `.env` and fill in values
- [ ] Copy `src/backend/Core.API/appsettings.Development.json.template` to `appsettings.Development.json` and fill in values
- [ ] Copy `src/frontend/.env.template` to `src/frontend/.env.local` and fill in values
- [ ] Get Google OAuth credentials
- [ ] Get Stripe test credentials
- [ ] Generate JWT secret key

---

## 🚀 Step-by-Step Setup

### 1️⃣ **Backend Configuration**

```powershell
# Navigate to backend API directory
cd src/backend/Core.API

# Copy the template
Copy-Item appsettings.Development.json.template appsettings.Development.json

# Now edit appsettings.Development.json and fill in your credentials
```

**Required Fields:**
- `Authentication:Google:ClientId` - Get from Google Cloud Console
- `Authentication:Google:ClientSecret` - Get from Google Cloud Console
- `JwtSettings:SecretKey` - Generate a random 32+ character string
- `StripeSettings:SecretKey` - Get from Stripe Dashboard

### 2️⃣ **Docker Environment Variables**

```powershell
# Navigate to project root
cd C:\src\Core

# Copy the template
Copy-Item .env.template .env

# Now edit .env and fill in your credentials
```

### 3️⃣ **Frontend Configuration**

```powershell
# Navigate to frontend directory
cd src/frontend

# Copy the template
Copy-Item .env.template .env.local

# Now edit .env.local and fill in your credentials
```

---

## 🔑 Getting Your Credentials

### **Google OAuth 2.0**

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project (or select existing)
3. Enable **Google+ API**
4. Go to **Credentials** → **Create Credentials** → **OAuth client ID**
5. Application type: **Web application**
6. Authorized JavaScript origins:
   - `http://localhost:5111`
   - `http://localhost:3001`
7. Authorized redirect URIs:
   - `http://localhost:5111/api/auth/google-callback`
8. Copy **Client ID** and **Client Secret**

### **Stripe Test Credentials**

1. Go to [Stripe Dashboard](https://dashboard.stripe.com/)
2. Make sure you're in **Test Mode** (toggle in top-right)
3. Go to **Developers** → **API keys**
4. Copy:
   - **Publishable key** (starts with `pk_test_`)
   - **Secret key** (starts with `sk_test_`)
5. For webhook secret (optional for local dev):
   - Go to **Developers** → **Webhooks**
   - Add endpoint: `http://localhost:5111/api/webhooks/stripe`
   - Copy **Signing secret**

### **JWT Secret Key**

Generate a secure random string (32+ characters):

```powershell
# PowerShell command to generate a random key
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

---

## ✅ Verification

After filling in your credentials:

```powershell
# Restart Docker services
cd C:\src\Core
docker-compose down
docker-compose up -d

# Check backend logs
docker-compose logs -f backend

# Test the application
# Frontend: http://localhost:3001
# Backend: http://localhost:5111/swagger
# Hangfire: http://localhost:5111/hangfire
```

---

## 🔒 Security Notes

### **What's Protected:**
- `.env` - Gitignored ✅
- `appsettings.Development.json` - Gitignored ✅
- `src/frontend/.env.local` - Gitignored ✅

### **What's Committed:**
- `.env.template` - Template only (no actual credentials)
- `appsettings.Development.json.template` - Template only
- `src/frontend/.env.template` - Template only
- `appsettings.json` - Base configuration (no secrets)

### **Best Practices:**
1. ✅ Use different credentials for dev/staging/production
2. ✅ Rotate secrets regularly
3. ✅ Never share credentials via email/Slack
4. ✅ Use environment variables in production
5. ✅ Enable 2FA on all cloud accounts

---

## 🆘 Troubleshooting

### **"Authentication failed"**
- Verify Google Client ID and Secret are correct
- Check OAuth redirect URIs match exactly
- Ensure you're using the correct environment (test vs production)

### **"Stripe API key invalid"**
- Make sure you're in Test Mode in Stripe Dashboard
- Verify you copied the full key (including prefix)
- Check for extra spaces or line breaks

### **"Database connection failed"**
- Ensure PostgreSQL container is running: `docker-compose ps`
- Verify connection string port matches (5433)
- Check database name and credentials

---

## 📞 Need Help?

If you run into issues:
1. Check the logs: `docker-compose logs -f backend`
2. Verify all services are healthy: `docker-compose ps`
3. Restart services: `docker-compose restart`
4. Check this guide for common issues

---

## 🎯 Next Steps

After setting up credentials:
1. ✅ Test Google OAuth login
2. ✅ Test Stripe payment processing
3. ✅ Run the application end-to-end
4. ✅ Continue with development

**Happy coding! 🚀**

