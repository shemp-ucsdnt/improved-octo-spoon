# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# set up (creates .venv, installs jot + pytest in editable mode)
python3 -m venv .venv && .venv/bin/pip install -e ".[dev]"

# run the CLI
.venv/bin/jot add "buy milk"
.venv/bin/jot list

# run the full test suite
.venv/bin/pytest -q

# run a single test file or test
.venv/bin/pytest tests/test_storage.py -q
.venv/bin/pytest tests/test_cli.py::test_search -q
```

## Architecture

`jot` is a local-first CLI task manager with no external runtime dependencies. Two modules do all the work:

- `src/jot/storage.py` — the `Store` class owns all persistence. State is a single JSON file (`{"next_id": int, "tasks": [...]}`) at `~/.jot/tasks.json`, overridable via the `JOT_HOME` env var (tests rely on this to isolate state per test via `tmp_path`/`monkeypatch`). `next_id` is a persisted monotonic counter — task IDs are never reused after a `remove`, even though the JSON only stores the tasks that currently exist. Every mutating method (`add`, `mark_done`, `remove`) does a full read-modify-write of the state file; there is no concurrent-access handling.
- `src/jot/cli.py` — `argparse`-based subcommand parser (`add`, `list`, `done`, `rm`, `search`) that translates argv into `Store` calls and formats output. `main(argv)` returns an int exit code and is the entry point tests call directly (not via subprocess), with stdout captured through pytest's `capsys`.

The package is installed via the `jot` console-script entry point defined in `pyproject.toml`, pointing at `jot.cli:main`.
