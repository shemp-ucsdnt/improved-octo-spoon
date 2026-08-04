import os

import pytest

from jot.cli import main


@pytest.fixture(autouse=True)
def jot_home(tmp_path, monkeypatch):
    monkeypatch.setenv("JOT_HOME", str(tmp_path))
    yield tmp_path


def test_add_and_list(capsys):
    main(["add", "buy", "milk"])
    main(["list"])
    out = capsys.readouterr().out
    assert "buy milk" in out


def test_done_and_default_list_hides_it(capsys):
    main(["add", "buy", "milk"])
    main(["done", "1"])
    capsys.readouterr()
    main(["list"])
    out = capsys.readouterr().out
    assert "buy milk" not in out


def test_done_with_all_flag_shows_it(capsys):
    main(["add", "buy", "milk"])
    main(["done", "1"])
    main(["list", "--all"])
    out = capsys.readouterr().out
    assert "buy milk" in out


def test_rm_missing_returns_error_code():
    assert main(["rm", "42"]) == 1


def test_search(capsys):
    main(["add", "buy", "milk"])
    main(["add", "write", "report"])
    capsys.readouterr()
    main(["search", "report"])
    out = capsys.readouterr().out
    assert "write report" in out
    assert "buy milk" not in out
