# Recommended MCP Servers for Core Project

This document lists the recommended MCP servers for developers working on the Core project. 

**Note:** All credentials must be configured locally in your Cursor MCP config file. See `docs/MCP_SERVERS_GUIDE.md` for setup instructions.

## 🎯 Essential MCP Servers

### 1. GitHub Integration
- **Package:** `@modelcontextprotocol/server-github`
- **Setup:** Personal Access Token with `repo` scope
- **Use for:** Repository queries, issues, PRs, commit history

### 2. Context7 (Upstash)
- **Package:** `@upstash/context-mcp`
- **Setup:** Free Upstash Redis account
- **Use for:** Semantic codebase search and understanding

### 3. PostgreSQL
- **Package:** `@modelcontextprotocol/server-postgres`
- **Setup:** Local database connection (see `appsettings.Development.json`)
- **Connection:** `postgresql://postgres:password@localhost:5433/CoreDb`
- **Use for:** Database schema exploration and queries

## 🚀 Highly Recommended

### 4. AWS Knowledge Base
- **Package:** `@modelcontextprotocol/server-aws-kb`
- **Setup:** Use existing AWS credentials
- **Use for:** Infrastructure queries, cost analysis, resource management

### 5. Brave Search
- **Package:** `@modelcontextprotocol/server-brave-search`
- **Setup:** Free API key from brave.com/search/api
- **Use for:** Technical documentation, best practices research

## 📋 Optional

### 6. Figma (if applicable)
- **Package:** `@figma/mcp-server-figma`
- **Setup:** Figma Personal Access Token
- **Use for:** Design-to-code conversion, design system extraction

## 🔒 Security Notes

- **Never commit** the `mcp.json` file (it's in your Cursor AppData folder, not the repo)
- **Never commit** API keys, tokens, or credentials
- **Use environment variables** where possible
- **Local development only** for PostgreSQL connection strings

## 📖 Setup Guide

For detailed setup instructions, see: [MCP_SERVERS_GUIDE.md](./MCP_SERVERS_GUIDE.md)


