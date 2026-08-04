# jot

A tiny local-first command-line task manager. No dependencies, no server —
just a JSON file on disk.

## Install

```bash
pip install -e ".[dev]"
```

## Usage

```bash
jot add "buy milk"
jot list
jot done 1
jot rm 1
jot search milk
```

Tasks are stored in `~/.jot/tasks.json` (override with the `JOT_HOME` env var).

## Development

```bash
pytest
```
