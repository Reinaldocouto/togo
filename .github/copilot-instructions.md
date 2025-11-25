<!-- Auto-generated: concise, discovery-first guidance for AI coding agents -->
# Copilot / AI Agent Instructions — Repository Discovery Guide

Purpose: Give a fast, reproducible checklist so an AI coding agent becomes productive immediately. This doc intentionally favors discovery-first steps (detect language, build/test commands, CI) rather than assuming a project type.

1) First-step discovery (must run)
- Run: `git ls-files --exclude-standard` and `ls -la` to list repo files.
- Look for language/build manifests: `package.json`, `pyproject.toml`, `requirements.txt`, `Pipfile`, `pom.xml`, `build.gradle`, `go.mod`, `Cargo.toml`, `setup.py`, `Makefile`, `Dockerfile`, `docker-compose.yml`.
- Inspect `README.md`, `.github/workflows/`, and any `CONTRIBUTING.md` for project-specific workflows.

2) Determine project type → then run the relevant quick checks
- Node/JS/TS: if `package.json` exists, run `cat package.json` and prefer `npm ci && npm test` or `pnpm install && pnpm test` if `pnpm-lock.yaml` present. Check for `tsconfig.json` and `src/`.
- Python: if `pyproject.toml`/`requirements.txt`/`setup.py` exist, create a venv and run `python -m venv .venv && source .venv/bin/activate && pip install -r requirements.txt` then `pytest` (or `python -m pytest`). Look for `src/` or top-level packages.
- Java: if `pom.xml` or `gradlew` found, prefer `./mvnw -B test` or `./gradlew test` (use wrapper if present). Check `src/main/java` and `src/test/java` layout.
- Go: if `go.mod` present, run `go test ./...` and inspect `cmd/`, `pkg/`, `internal/` layout.
- Rust: if `Cargo.toml` present, run `cargo test` and inspect `src/`.

3) Key places to investigate (common, high-value file locations)
- Entry points: `cmd/*`, `main.go`, `src/main/*`, `bin/`, or `app/` directories.
- Libraries: `pkg/`, `internal/`, `lib/`, `src/` (language-specific variations).
- Tests: `tests/`, `spec/`, `__tests__`, `*_test.go`, `src/**/*.test.*`.
- CI: `.github/workflows/*.yml` for build and release steps — read names and referenced scripts/commands.
- Infra: `Dockerfile`, `docker-compose.yml`, `terraform/`, `infra/`, `deploy/` — check env patterns and secrets handling (`.env.example`).

4) Project-specific conventions to detect and follow
- Look for workspace-specific scripts in `package.json` (`scripts`), `Makefile` targets, or `gradle` tasks — prefer invoking those instead of guessing commands.
- If `./scripts` or `tools` exists, treat scripts there as canonical developer workflows (lint, format, build, test).
- If there is a `CONTRIBUTING.md` or PR template under `.github/PULL_REQUEST_TEMPLATE.md`, follow the stated branch/commit message conventions.

5) Integration and external dependencies
- Search for `DATABASE_URL`, `REDIS_URL`, `AZURE_`, `AWS_`, `GCP_` environment variables in repo to find infra ties.
- If `docker-compose.yml` or `compose` dir exists, prefer using the compose setup for integration tests (but do not start containers unless asked by a human reviewer).

6) Guidance for making changes (branching / testing / PRs)
- Create a focused branch `copilot/<short-desc>` for implementation.
- Run unit tests and linters referenced by `package.json`/`pyproject.toml`/`Makefile` before opening a PR.
- In the PR description: include a short summary, list files changed, describe test steps and any manual verification required.

7) Safety and permissions
- Do not run arbitrary network-heavy or privileged actions (creating cloud resources, pushing to registries, or running Docker containers) without explicit user approval.

8) When you can't find explicit guidance
- Ask the user what the intended language/runtime is, or propose to run the discovery commands (showing the output) and request permission to run build/test steps.

Examples (detection → action)
- If `package.json` found and `scripts.test` exists: run `npm ci && npm test`.
- If `pyproject.toml` exists and a `tox.ini` is present: run `python -m venv .venv && source .venv/bin/activate && pip install tox && tox -av`.
- If `.github/workflows/ci.yml` references `make test`, run `make test` locally first.

Reference checkpoints for agent output
- Always list the top-level files you used to decide the workflow (e.g., `package.json`, `pyproject.toml`, `Dockerfile`).
- Show the exact commands you propose to run and why.
- If you change files, run the project's tests and include the test results summary in the PR description.

If anything above is unclear or you'd like this tuned to a specific language or repo layout, say which languages or attach the repository listing and I'll iterate.

-- End of file
