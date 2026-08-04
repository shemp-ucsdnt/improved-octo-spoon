import pytest

from jot.storage import Store


@pytest.fixture
def store(tmp_path):
    return Store(path=tmp_path / "tasks.json")


def test_add_and_list(store):
    store.add("buy milk")
    store.add("write report")
    tasks = store.list()
    assert [t.text for t in tasks] == ["buy milk", "write report"]
    assert all(not t.done for t in tasks)


def test_ids_increment_and_persist_after_removal(store):
    a = store.add("first")
    store.remove(a.id)
    b = store.add("second")
    assert b.id == a.id + 1


def test_mark_done(store):
    task = store.add("buy milk")
    done = store.mark_done(task.id)
    assert done.done is True
    assert store.list(include_done=False) == []


def test_mark_done_missing_raises(store):
    with pytest.raises(KeyError):
        store.mark_done(999)


def test_remove(store):
    task = store.add("buy milk")
    store.remove(task.id)
    assert store.list() == []


def test_remove_missing_raises(store):
    with pytest.raises(KeyError):
        store.remove(999)


def test_search(store):
    store.add("buy milk")
    store.add("write report")
    results = store.search("MILK")
    assert len(results) == 1
    assert results[0].text == "buy milk"
