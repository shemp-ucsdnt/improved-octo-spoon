"""JSON-backed storage for jot tasks."""

from __future__ import annotations

import json
import os
from dataclasses import asdict, dataclass
from pathlib import Path


def default_store_path() -> Path:
    home = os.environ.get("JOT_HOME", str(Path.home() / ".jot"))
    return Path(home) / "tasks.json"


@dataclass
class Task:
    id: int
    text: str
    done: bool = False


class Store:
    def __init__(self, path: Path | None = None):
        self.path = path or default_store_path()

    def _load_state(self) -> dict:
        if not self.path.exists():
            return {"next_id": 1, "tasks": []}
        with open(self.path) as f:
            return json.load(f)

    def _load(self) -> list[dict]:
        return self._load_state()["tasks"]

    def _save(self, tasks: list[dict], next_id: int) -> None:
        self.path.parent.mkdir(parents=True, exist_ok=True)
        with open(self.path, "w") as f:
            json.dump({"next_id": next_id, "tasks": tasks}, f, indent=2)

    def add(self, text: str) -> Task:
        state = self._load_state()
        task = Task(id=state["next_id"], text=text)
        state["tasks"].append(asdict(task))
        self._save(state["tasks"], state["next_id"] + 1)
        return task

    def list(self, include_done: bool = True) -> list[Task]:
        tasks = [Task(**t) for t in self._load()]
        if not include_done:
            tasks = [t for t in tasks if not t.done]
        return tasks

    def mark_done(self, task_id: int) -> Task:
        state = self._load_state()
        for t in state["tasks"]:
            if t["id"] == task_id:
                t["done"] = True
                self._save(state["tasks"], state["next_id"])
                return Task(**t)
        raise KeyError(f"no task with id {task_id}")

    def remove(self, task_id: int) -> None:
        state = self._load_state()
        remaining = [t for t in state["tasks"] if t["id"] != task_id]
        if len(remaining) == len(state["tasks"]):
            raise KeyError(f"no task with id {task_id}")
        self._save(remaining, state["next_id"])

    def search(self, query: str) -> list[Task]:
        query = query.lower()
        return [t for t in self.list() if query in t.text.lower()]
