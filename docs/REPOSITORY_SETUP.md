# Repository Setup

## 1. Initialize Git

```bash
git init
git add .
git commit -m "chore: initialize sery baseline architecture"
```

## 2. Create main branches

```bash
git branch -M main
git checkout -b develop
git checkout main
```

## 3. Connect remote and publish

```bash
git remote add origin <github-repo-url>
git push -u origin main
git push -u origin develop
```

## 4. Protect branches in GitHub

- Protect `main` and `develop`
- Require pull request before merge
- Require CI status checks
- Disable force-push and branch deletion

## 5. Daily branch naming

- `feature/<scope-short-description>`
- `fix/<scope-short-description>`
- `chore/<scope-short-description>`
