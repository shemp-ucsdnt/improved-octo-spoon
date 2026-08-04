"""Command-line interface for jot."""

from __future__ import annotations

import argparse
import sys

from .storage import Store


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="jot", description="A tiny local-first task manager")
    sub = parser.add_subparsers(dest="command", required=True)

    add_p = sub.add_parser("add", help="add a new task")
    add_p.add_argument("text", nargs="+", help="task description")

    list_p = sub.add_parser("list", help="list tasks")
    list_p.add_argument("--all", action="store_true", help="include completed tasks")

    done_p = sub.add_parser("done", help="mark a task as done")
    done_p.add_argument("id", type=int)

    rm_p = sub.add_parser("rm", help="remove a task")
    rm_p.add_argument("id", type=int)

    search_p = sub.add_parser("search", help="search tasks by text")
    search_p.add_argument("query")

    return parser


def format_task(task) -> str:
    mark = "x" if task.done else " "
    return f"[{mark}] {task.id}: {task.text}"


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)
    store = Store()

    if args.command == "add":
        task = store.add(" ".join(args.text))
        print(f"added {task.id}: {task.text}")
        return 0

    if args.command == "list":
        tasks = store.list(include_done=args.all)
        if not tasks:
            print("no tasks")
            return 0
        for task in tasks:
            print(format_task(task))
        return 0

    if args.command == "done":
        try:
            task = store.mark_done(args.id)
        except KeyError as e:
            print(str(e), file=sys.stderr)
            return 1
        print(f"done {task.id}: {task.text}")
        return 0

    if args.command == "rm":
        try:
            store.remove(args.id)
        except KeyError as e:
            print(str(e), file=sys.stderr)
            return 1
        print(f"removed {args.id}")
        return 0

    if args.command == "search":
        results = store.search(args.query)
        if not results:
            print("no matches")
            return 0
        for task in results:
            print(format_task(task))
        return 0

    parser.print_help()
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
