# Code Quality Standards

## Overview

This project enforces code quality standards through automated linting, formatting, and code analysis tools for both backend (.NET) and frontend (React/TypeScript) codebases.

## Backend (.NET) Code Quality

### Tools

1. **StyleCop Analyzers** (v1.2.0-beta.556)
   - Enforces C# coding style guidelines
   - Ensures consistent code formatting
   - Validates documentation requirements

2. **Roslynator Analyzers** (v4.12.4)
   - Provides additional code analysis rules
   - Suggests code improvements and refactorings
   - Identifies potential bugs and code smells

3. **.NET Analyzers**
   - Built-in Microsoft analyzers
   - Latest analysis level enabled
   - Code style enforcement in build

### Configuration Files

#### `.editorconfig`
- Root configuration file for all editors
- Defines coding standards and formatting rules
- Applies to all C# files in the solution

Key settings:
- Indentation: 4 spaces for C#
- New lines before braces, else, catch, finally
- `var` keyword usage preferences
- Null-checking preferences
- Naming conventions for types, fields, methods
- Async methods must end with "Async"

#### `Directory.Build.props`
- MSBuild properties applied to all projects
- Analyzer package references
- StyleCop configuration

#### `stylecop.json`
- StyleCop-specific settings
- Documentation rules configuration
- Ordering and layout preferences

### Naming Conventions

| Element Type        | Convention           | Example              |
|---------------------|----------------------|----------------------|
| Classes             | PascalCase           | `UserService`        |
| Interfaces          | IPascalCase          | `IUserService`       |
| Methods             | PascalCase           | `GetUserById`        |
| Properties          | PascalCase           | `UserName`           |
| Private fields      | _camelCase           | `_userRepository`    |
| Constants           | PascalCase           | `MaxRetryCount`      |
| Static fields       | PascalCase           | `DefaultTimeout`     |
| Local variables     | camelCase            | `userId`             |
| Parameters          | camelCase            | `userName`           |
| Async methods       | PascalCaseAsync      | `GetUserByIdAsync`   |

### Code Style Rules

#### Braces
- Always use braces for control statements
- New line before open brace

#### Using Directives
- System usings first
- Organize alphabetically
- Place outside namespace

#### this. Qualifier
- Do not use `this.` for members unless required
- Enforce with warnings

#### var Usage
- Use `var` for built-in types (suggestion)
- Use `var` when type is apparent (suggestion)
- Use `var` elsewhere (suggestion)

#### Expression-bodied Members
- Use for single-line methods (suggestion)
- Use for single-line properties (suggestion)
- Do not use for constructors (suggestion)

### Running Backend Analysis

```bash
# Build with analysis
cd src/backend
dotnet build

# Treat warnings as errors (optional)
dotnet build /p:TreatWarningsAsErrors=true

# Run code cleanup
dotnet format
```

### Suppressing Warnings

#### In Code
```csharp
#pragma warning disable SA1200 // Using directive should appear within a namespace declaration
using System;
#pragma warning restore SA1200
```

#### In .editorconfig
```ini
# CA1031: Do not catch general exception types
dotnet_diagnostic.CA1031.severity = none
```

## Frontend (React/TypeScript) Code Quality

### Tools

1. **ESLint** (v9.37.0)
   - JavaScript/TypeScript linting
   - React-specific rules
   - TypeScript-specific rules

2. **Prettier** (v3.6.2)
   - Code formatting
   - Consistent style enforcement
   - Integrated with ESLint

3. **TypeScript Compiler**
   - Type checking
   - Build-time error detection

### Configuration Files

#### `.eslintrc.json`
- ESLint configuration
- Parser and plugin settings
- Rule definitions

Key rules:
- React in JSX scope not required (React 17+)
- No prop-types (using TypeScript)
- Warn on explicit `any` types
- Warn on unused variables (except those prefixed with `_`)
- Error on missing React hooks dependencies (warn)
- Error on console.log (except warn/error)

#### `.prettierrc.json`
- Prettier formatting rules
- Code style preferences

Settings:
- Semicolons: Required
- Quotes: Single quotes
- Print width: 100 characters
- Tab width: 2 spaces
- Trailing commas: ES5
- Arrow function parentheses: Always

#### `.prettierignore`
- Files to exclude from formatting
- Build artifacts and dependencies

### NPM Scripts

```bash
# Lint TypeScript/React code
npm run lint

# Fix linting issues automatically
npm run lint:fix

# Format code with Prettier
npm run format

# Check formatting without modifying files
npm run format:check

# Type check without building
npm run type-check

# Run all checks
npm run lint && npm run format:check && npm run type-check
```

### Code Style Guidelines

#### React Components
- Use functional components with hooks
- Use TypeScript for type safety
- Export as named exports (not default)
- Use PascalCase for component names

```typescript
export const UserProfile = ({ userId }: { userId: string }) => {
  // Component logic
};
```

#### Hooks
- Follow React hooks rules
- Custom hooks start with `use`
- Extract complex logic to custom hooks

```typescript
export const useAuth = () => {
  // Hook logic
};
```

#### File Organization
- One component per file
- Co-locate related files (styles, tests, hooks)
- Use index files for barrel exports

```
components/
  UserProfile/
    index.ts
    UserProfile.tsx
    UserProfile.test.tsx
    use

UserProfile.ts
```

#### Import Order
1. External dependencies (React, libraries)
2. Internal imports (components, utils)
3. Types and interfaces
4. Styles

```typescript
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { Button } from '@/components/ui/button';
import { api } from '@/services/api';

import type { User } from '@/types';
```

#### TypeScript Guidelines
- Prefer interfaces over types for object shapes
- Use type for unions and intersections
- Avoid `any`, use `unknown` if type is truly unknown
- Use const assertions where appropriate

```typescript
// Good
interface User {
  id: string;
  name: string;
}

type Status = 'active' | 'inactive';

// Avoid
let data: any;

// Better
let data: unknown;
```

### Accessibility
- Use semantic HTML elements
- Include ARIA labels where necessary
- Ensure keyboard navigation
- Test with screen readers

## IDE Integration

### Visual Studio / Visual Studio Code
- EditorConfig plugin automatically applies formatting rules
- Analyzers run on build and in real-time
- Quick fixes available for many issues

### VS Code Extensions Recommended
- ESLint
- Prettier
- EditorConfig for VS Code
- C# (OmniSharp)
- C# Dev Kit

### VS Code Settings
```json
{
  "editor.formatOnSave": true,
  "editor.defaultFormatter": "esbenp.prettier-vscode",
  "editor.codeActionsOnSave": {
    "source.fixAll.eslint": true
  },
  "eslint.validate": [
    "javascript",
    "javascriptreact",
    "typescript",
    "typescriptreact"
  ]
}
```

## Pre-commit Hooks (Optional)

### Husky + lint-staged
Automatically lint and format code before committing.

```bash
# Frontend
cd src/frontend
npx husky install
npx husky add .husky/pre-commit "npx lint-staged"
```

`.lintstagedrc.json`:
```json
{
  "*.{ts,tsx}": [
    "eslint --fix",
    "prettier --write"
  ],
  "*.{json,css,md}": [
    "prettier --write"
  ]
}
```

## CI/CD Integration

### GitHub Actions
Code quality checks should run in CI:

```yaml
- name: Lint Backend
  run: |
    cd src/backend
    dotnet format --verify-no-changes

- name: Lint Frontend
  run: |
    cd src/frontend
    npm run lint
    npm run format:check
    npm run type-check
```

## Common Issues and Solutions

### Backend

#### SA1633: File must have header
**Solution**: Disabled in `stylecop.json` with `xmlHeader: false`

#### CS8618: Non-nullable field must contain a non-null value
**Solution**: Set to `suggestion` severity in `.editorconfig`

#### CA1062: Validate arguments of public methods
**Solution**: Disabled in `.editorconfig` (using null-conditional operators instead)

### Frontend

#### ESLint: '@typescript-eslint/no-explicit-any'
**Solution**: Replace `any` with specific types or `unknown`

#### React Hook useEffect has a missing dependency
**Solution**: Add dependency or use `// eslint-disable-next-line react-hooks/exhaustive-deps`

#### Prettier formatting conflicts with ESLint
**Solution**: Using `eslint-config-prettier` to disable conflicting rules

## Code Review Checklist

### Backend
- [ ] No StyleCop warnings
- [ ] No Roslynator suggestions
- [ ] Naming conventions followed
- [ ] Async methods end with "Async"
- [ ] Private fields start with underscore
- [ ] Using directives organized
- [ ] Code documented where necessary

### Frontend
- [ ] No ESLint errors
- [ ] Code formatted with Prettier
- [ ] TypeScript types defined
- [ ] No `any` types
- [ ] React hooks rules followed
- [ ] Accessibility considerations
- [ ] Console.log removed (except debug code)

## Resources

### Backend
- [.NET Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [StyleCop Documentation](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
- [Roslynator](https://github.com/JosefPihrt/Roslynator)

### Frontend
- [ESLint Rules](https://eslint.org/docs/latest/rules/)
- [TypeScript ESLint](https://typescript-eslint.io/)
- [Prettier Options](https://prettier.io/docs/en/options.html)
- [React Hooks Rules](https://react.dev/reference/react/hooks#rules-of-hooks)

## Continuous Improvement

Code quality standards should evolve with the project:
- Review and update rules quarterly
- Address new analyzer recommendations
- Gather team feedback on rule strictness
- Keep analyzer packages up to date
- Document team-specific conventions

