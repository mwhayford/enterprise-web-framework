# Contributing to RentalManager

Thank you for your interest in contributing to RentalManager! This document provides guidelines for contributing to the project.

## Code of Conduct

Please read and follow our [Code of Conduct](CODE_OF_CONDUCT.md) before contributing.

## Development Workflow

### Getting Started

1. **Fork the Repository**: Fork the repository to your GitHub account
2. **Clone Your Fork**: Clone your fork to your local machine
3. **Create a Branch**: Create a feature branch from `main`
   ```bash
   git checkout -b feature/your-feature-name
   ```

### Making Changes

1. **Make Your Changes**: Write clean, well-documented code
2. **Test Your Changes**: Ensure all tests pass and add new tests for new features
3. **Follow Coding Standards**: Adhere to our coding standards (see below)
4. **Commit Your Changes**: Use conventional commit messages (see below)

### Submitting Changes

1. **Push Your Branch**: Push your branch to your fork
2. **Create a Pull Request**: Open a PR from your fork to the main repository
3. **Address Feedback**: Respond to code review feedback promptly
4. **Get Approval**: Wait for at least one code review approval before merging

## Coding Standards

### C# / .NET Standards

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use PascalCase for public members, camelCase for private fields
- Keep methods focused and follow single responsibility principle
- Use meaningful variable and method names
- Add XML comments for public APIs
- Keep files under 500 lines when possible

**Linting Rules** (enforced via StyleCop and Roslynator):
- Follow StyleCop rules (see `.editorconfig`)
- Use `var` when the type is obvious from context
- Use expression-bodied members when appropriate
- Prefer early returns to reduce nesting

### TypeScript / React Standards

- Follow [TypeScript Coding Conventions](https://github.com/Microsoft/TypeScript/wiki/Coding-Guidelines)
- Use functional components with hooks
- Keep components focused and under 200 lines
- Use descriptive prop names and TypeScript interfaces
- Extract reusable logic into custom hooks
- Use meaningful component and function names

**Linting Rules** (enforced via ESLint and Prettier):
- Follow ESLint rules (see `eslint.config.js`)
- Use single quotes for strings
- Use 2 spaces for indentation
- Add trailing commas in multiline arrays/objects
- Use arrow functions for inline callbacks

### General Standards

- Write self-documenting code with clear names
- Add comments only when code cannot be made clearer
- Keep functions focused and small (ideally under 50 lines)
- Avoid deep nesting (max 3-4 levels)
- Remove unused code and imports
- Format code before committing

## Commit Message Conventions

Follow [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

[optional body]

[optional footer(s)]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, missing semicolons, etc.)
- `refactor`: Code refactoring without functionality changes
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `build`: Build system or external dependencies
- `ci`: CI/CD configuration changes
- `chore`: Other changes that don't modify src or test files

**Scopes:**
- `backend`: Backend API changes
- `frontend`: Frontend application changes
- `api`: API changes
- `auth`: Authentication changes
- `payment`: Payment processing changes
- `search`: Search functionality
- `tests`: Test changes
- `ci`: CI/CD changes
- `docs`: Documentation changes
- `deps`: Dependency updates

**Examples:**
```
feat(auth): add Google OAuth authentication
fix(payment): correct Stripe payment processing error
docs(api): update API endpoint documentation
refactor(backend): extract payment service interface
```

**Subject Guidelines:**
- Start with uppercase letter
- Use imperative mood ("add" not "added" or "adds")
- No period at the end
- Be concise but descriptive

## Pull Request Requirements

### Before Submitting

- [ ] Code follows the project's style guidelines
- [ ] All tests pass locally (`dotnet test`, `npm test`)
- [ ] New code has corresponding tests
- [ ] Code has been self-reviewed
- [ ] Documentation is updated
- [ ] Commit messages follow conventional commits
- [ ] No merge conflicts with main branch
- [ ] PR title follows: `type(scope): subject`

### PR Description Template

Include the following in your PR description:

```markdown
## What
Brief description of what changes were made

## Why
Explanation of why this change was necessary

## How
Technical details about the implementation

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests pass
- [ ] Manual testing completed

## Screenshots (if applicable)
Add screenshots for UI changes

## Related Issues
Closes #issue-number
```

### Code Review Process

1. **Automated Checks**: PR must pass all CI checks (build, tests, linting)
2. **Review Requirements**: At least one approval from a maintainer
3. **PR Size**: 
   - Extra Small (<100 lines): Quick review
   - Small (<300 lines): Standard review
   - Medium (<500 lines): Thorough review recommended
   - Large (<1000 lines): Breaking into smaller PRs encouraged
   - Extra Large (1000+): Must be broken into smaller PRs

4. **Review Guidelines**:
   - Be constructive and respectful
   - Comment on code, not the person
   - Suggest alternatives when possible
   - Ask questions if something is unclear
   - Approve when ready, request changes when needed

## Testing Requirements

### Unit Tests

- All domain logic must have unit tests
- Use AAA pattern (Arrange, Act, Assert)
- Maintain at least 80% code coverage
- Test both success and failure scenarios
- Use meaningful test names (e.g., `Should_ReturnUser_When_ValidEmailIsProvided`)

### Integration Tests

- Test database operations with TestContainers
- Test external service integrations
- Use real database instances (not mocks)
- Clean up test data after execution

### End-to-End Tests

- Test critical user workflows
- Test authentication flows
- Test payment processing
- Test search functionality
- Use Playwright for browser automation

### Running Tests

**Backend Tests:**
```bash
cd src/backend
dotnet test
```

**Frontend Tests:**
```bash
cd src/frontend
npm test
```

**E2E Tests:**
```bash
cd tests/RentalManager.E2ETests
npm test
```

## Reporting Bugs

When reporting a bug, please include:

1. **Description**: Clear description of the bug
2. **Steps to Reproduce**: Detailed steps to reproduce the issue
3. **Expected Behavior**: What should happen
4. **Actual Behavior**: What actually happens
5. **Environment**: OS, browser, .NET version, Node version
6. **Screenshots/Logs**: Screenshots or relevant log snippets
7. **Possible Fix**: If you have ideas for a fix

**Template:**
```markdown
## Description
[Brief description of the bug]

## Steps to Reproduce
1. Step 1
2. Step 2
3. Step 3

## Expected Behavior
[What should happen]

## Actual Behavior
[What actually happens]

## Environment
- OS: [e.g., Windows 11]
- Browser: [e.g., Chrome 120]
- .NET Version: [e.g., 9.0]
- Node Version: [e.g., 18.0]

## Screenshots/Logs
[Add screenshots or log snippets]
```

## Requesting Features

When requesting a feature, please include:

1. **Problem Statement**: What problem does this solve?
2. **Proposed Solution**: How should it work?
3. **Alternatives Considered**: What other approaches were considered?
4. **Impact**: Who would benefit from this feature?
5. **Examples**: Real-world examples or mockups

## Getting Help

- **Documentation**: Check the [docs](docs/) folder
- **Issues**: Search existing issues on GitHub
- **Discussions**: Ask questions in GitHub Discussions
- **Code Review**: Request review on your PR

## Recognition

Contributors will be recognized in:
- Project README
- Release notes
- GitHub contributors page

Thank you for contributing to RentalManager! 🎉

