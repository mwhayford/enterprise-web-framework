# MCP Servers Guide for Cursor

## 🎯 What are MCP Servers?

**MCP (Model Context Protocol)** servers extend the AI assistant's capabilities by giving it access to external services, databases, and tools. Think of them as powerful plugins that unlock new superpowers!

## 📍 Configuration Location

Your MCP configuration is stored at:
```
C:\Users\mwhay\AppData\Roaming\Cursor\User\globalStorage\mcp.json
```

## 🚀 Servers You Have Configured

### 1. **Context7** (Upstash Context MCP) 🧠
**What it does:** Provides semantic codebase search and understanding powered by Upstash Vector DB.

**Why it's powerful:**
- Understands your entire codebase semantically (not just text search)
- Can find related code across files based on meaning
- Remembers architectural patterns and relationships
- Great for large codebases where grep isn't enough

**How to enable:**
1. Create a free Upstash account: https://console.upstash.com/
2. Create a Redis database (Vector DB)
3. Get your REST URL and token from the database details
4. Update these values in `mcp.json`:
   ```json
   "UPSTASH_REDIS_REST_URL": "https://your-db.upstash.io",
   "UPSTASH_REDIS_REST_TOKEN": "your-token-here"
   ```
5. Change `"disabled": true` to `"disabled": false`
6. Restart Cursor

**Use cases:**
- "Find all places where we handle user authentication"
- "Show me similar patterns to this payment processing code"
- "What are all the ways we interact with the database?"

---

### 2. **Figma MCP** 🎨
**What it does:** Integrates with Figma to read designs, components, and styles directly.

**Why it's powerful:**
- I can see your actual designs and convert them to code
- Extract colors, typography, spacing directly from design system
- Verify implementation matches designs
- Generate component code from Figma components

**How to enable:**
1. Go to Figma → Settings → Personal Access Tokens
2. Create a new token with read access
3. Update in `mcp.json`:
   ```json
   "FIGMA_PERSONAL_ACCESS_TOKEN": "figd_your-token-here"
   ```
4. Change `"disabled": true` to `"disabled": false`
5. Restart Cursor

**Use cases:**
- "Implement this Figma component: [file-id]"
- "Extract all colors from our design system"
- "Generate a React component matching this Figma frame"
- "What spacing system does our design use?"

---

### 3. **PostgreSQL MCP** 🗄️
**What it does:** Direct access to query and analyze your PostgreSQL database.

**Why it's powerful:**
- I can run queries to understand data patterns
- Generate migration scripts based on schema analysis
- Debug database issues by examining actual data
- Create seed data or test fixtures

**How to enable:**
1. Update the connection string in `mcp.json`:
   ```json
   "args": ["-y", "@modelcontextprotocol/server-postgres", "postgresql://username:password@localhost:5432/CoreDb"]
   ```
2. Change `"disabled": true` to `"disabled": false`
3. Restart Cursor

**Use cases:**
- "Show me all users created in the last week"
- "Analyze the payment table schema and suggest indexes"
- "Generate a migration to add a new column"
- "What's the average subscription duration?"

**⚠️ Security Note:** Only enable for local development databases! Never use production credentials.

---

### 4. **GitHub MCP** 🐙
**What it does:** Integrates with GitHub API to read issues, PRs, repos, and more.

**Why it's powerful:**
- Can read and analyze issues/PRs
- Check repository information
- Search across GitHub
- Understand project history and context

**How to enable:**
1. Go to GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Create a new token with `repo` scope
3. Update in `mcp.json`:
   ```json
   "GITHUB_PERSONAL_ACCESS_TOKEN": "ghp_your-token-here"
   ```
4. Change `"disabled": true` to `"disabled": false`
5. Restart Cursor

**Use cases:**
- "Show me all open issues labeled 'bug'"
- "Summarize the last 10 PRs merged to main"
- "What's the commit history for this file?"
- "Search GitHub for similar implementations"

---

### 5. **Filesystem MCP** 📁
**What it does:** Enhanced file operations beyond the built-in tools.

**Why it's powerful:**
- More advanced file operations
- Better for large directory operations
- Can watch file changes
- Useful for project setup/scaffolding tasks

**How to enable:**
1. The path is already set to your project: `C:\src\Core`
2. Change `"disabled": true` to `"disabled": false`
3. Restart Cursor

**Note:** This server provides similar functionality to built-in tools, so you may not need it.

---

### 6. **Brave Search MCP** 🔍
**What it does:** Web search using Brave Search API (privacy-focused, no tracking).

**Why it's powerful:**
- Real-time web search for current information
- Technical documentation lookup
- Package/library research
- Error message solutions

**How to enable:**
1. Get a Brave Search API key: https://brave.com/search/api/
2. Update in `mcp.json`:
   ```json
   "BRAVE_API_KEY": "BSA-your-key-here"
   ```
3. Change `"disabled": true` to `"disabled": false`
4. Restart Cursor

**Use cases:**
- "Search for the latest .NET 9 performance benchmarks"
- "Find solutions for this error message"
- "What's the current best practice for React state management?"

---

### 7. **AWS Knowledge Base MCP** ☁️
**What it does:** Query AWS documentation and your AWS resources.

**Why it's powerful:**
- Understand AWS service documentation
- Query your actual AWS infrastructure
- Generate Terraform/CloudFormation
- Debug AWS issues

**How to enable:**
1. Use your existing AWS credentials (already configured)
2. Update in `mcp.json` with your credentials
3. Change `"disabled": true` to `"disabled": false`
4. Restart Cursor

**Use cases:**
- "What's the current state of my EKS cluster?"
- "Show me all running EC2 instances and their costs"
- "Generate a Terraform config for this architecture"

---

## 🌟 My Top Recommendations for Your Project

Based on your .NET + React + AWS project, I recommend enabling these **in order of usefulness**:

### **Essential (Enable Now)**
1. **GitHub** - You're already using GitHub, this makes PR reviews and issue tracking much easier
2. **PostgreSQL** - Direct database access will speed up debugging and data analysis

### **Highly Recommended (Enable Soon)**
3. **Figma** - If you have designs in Figma, this will save hours of implementation time
4. **Context7** - For a large codebase like yours with multiple projects, semantic search is invaluable

### **Nice to Have**
5. **Brave Search** - Useful for researching libraries and best practices
6. **AWS Knowledge Base** - Since you're deploying to AWS, this can help with infrastructure questions

### **Optional**
7. **Filesystem** - You probably don't need this since Cursor has good built-in file tools

---

## 📦 Additional Powerful MCP Servers

Here are more servers you might want to add:

### **Development Tools**
- **`@modelcontextprotocol/server-sqlite`** - SQLite database access
- **`@modelcontextprotocol/server-memory`** - Persistent memory across sessions
- **`@modelcontextprotocol/server-sequential-thinking`** - Enhanced reasoning capabilities

### **Cloud Services**
- **`@modelcontextprotocol/server-gdrive`** - Google Drive access
- **`@anthropic/mcp-server-notion`** - Notion workspace integration

### **Databases**
- **`@benborla29/mcp-server-mysql`** - MySQL database access
- **`@benborla29/mcp-server-mongodb`** - MongoDB access

### **AI/ML**
- **`@upstash/mcp-server-qstash`** - Task scheduling and background jobs
- **`@upstash/mcp-server-rag`** - RAG (Retrieval Augmented Generation) for documents

---

## 🔧 How to Add a New MCP Server

1. Find the server on npm: https://www.npmjs.com/search?q=mcp-server
2. Add to your `mcp.json`:
   ```json
   "server-name": {
     "command": "npx",
     "args": ["-y", "package-name"],
     "env": {
       "API_KEY": "your-key"
     },
     "disabled": false
   }
   ```
3. Restart Cursor (Ctrl+Shift+P → "Reload Window")

---

## 🐛 Troubleshooting

### MCP Server Not Working?
1. Check that `"disabled": false`
2. Verify all environment variables are set correctly
3. Restart Cursor completely (close and reopen)
4. Check Cursor's Output panel (View → Output → MCP) for errors

### "Command not found" Error?
- The `npx` command requires Node.js to be installed
- Make sure Node.js is in your PATH
- Try running `npx -y package-name` in your terminal manually

### Rate Limits or Authentication Errors?
- Check that your API keys/tokens are valid
- Some services have rate limits on free tiers
- Verify the token has the correct permissions

---

## 🎓 Learning More

- **Official MCP Docs**: https://modelcontextprotocol.io/
- **MCP Servers List**: https://github.com/modelcontextprotocol/servers
- **Cursor MCP Guide**: https://docs.cursor.com/context/model-context-protocol

---

## ⚡ Quick Start

### To enable GitHub + PostgreSQL (Recommended):

1. Get a GitHub Personal Access Token:
   - Go to: https://github.com/settings/tokens
   - Create new token (classic) with `repo` scope
   - Copy the token

2. Update `mcp.json`:
   ```json
   "github": {
     "env": {
       "GITHUB_PERSONAL_ACCESS_TOKEN": "ghp_YOUR_TOKEN_HERE"
     },
     "disabled": false
   },
   "postgres": {
     "args": ["-y", "@modelcontextprotocol/server-postgres", "postgresql://postgres:postgres@localhost:5432/CoreDb"],
     "disabled": false
   }
   ```

3. Restart Cursor

4. Try it out:
   - "Show me all open issues in this repo"
   - "Query the database for all active subscriptions"

---

## 💡 Pro Tips

1. **Start small**: Enable 1-2 servers at a time to understand their capabilities
2. **Security first**: Never commit API keys. Consider using environment variables
3. **Local only**: Keep database servers pointed at local dev databases only
4. **Monitor usage**: Some APIs have rate limits or costs
5. **Disable when not needed**: Disable servers you're not actively using to reduce startup time

---

**Happy coding with supercharged AI assistance!** 🚀


