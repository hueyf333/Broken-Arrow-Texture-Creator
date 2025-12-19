from __future__ import annotations

from dataclasses import dataclass
from typing import Callable, List, Optional

from .core import Mesh, MeshChange


class Command:
    def __init__(self, execute: Callable[[Mesh], MeshChange], description: str = "") -> None:
        self._execute = execute
        self.description = description

    def apply(self, mesh: Mesh) -> MeshChange:
        return self._execute(mesh)


@dataclass
class CommandRecord:
    command: Command
    change: MeshChange


class CommandStack:
    def __init__(self) -> None:
        self._undo_stack: List[CommandRecord] = []
        self._redo_stack: List[CommandRecord] = []

    def execute(self, mesh: Mesh, command: Command) -> MeshChange:
        change = command.apply(mesh)
        self._undo_stack.append(CommandRecord(command=command, change=change))
        self._redo_stack.clear()
        return change

    def undo(self, mesh: Mesh) -> Optional[MeshChange]:
        if not self._undo_stack:
            return None
        record = self._undo_stack.pop()
        mesh.restore_change(record.change)
        self._redo_stack.append(record)
        return record.change

    def redo(self, mesh: Mesh) -> Optional[MeshChange]:
        if not self._redo_stack:
            return None
        record = self._redo_stack.pop()
        change = record.command.apply(mesh)
        self._undo_stack.append(CommandRecord(command=record.command, change=change))
        return change
