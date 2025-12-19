"""Non-destructive modifier stack and history panel state."""

from dataclasses import dataclass, field
from typing import Dict, List, Optional


@dataclass
class Modifier:
    name: str
    parameters: Dict[str, float]
    enabled: bool = True


@dataclass
class ModifierStack:
    modifiers: List[Modifier] = field(default_factory=list)

    def add(self, modifier: Modifier) -> None:
        self.modifiers.append(modifier)

    def toggle(self, name: str, enabled: bool) -> None:
        for modifier in self.modifiers:
            if modifier.name == name:
                modifier.enabled = enabled
                return
        raise KeyError(f"Modifier '{name}' not found")

    def remove(self, name: str) -> None:
        for index, modifier in enumerate(self.modifiers):
            if modifier.name == name:
                self.modifiers.pop(index)
                return
        raise KeyError(f"Modifier '{name}' not found")


@dataclass
class HistoryEntry:
    label: str
    data: Dict[str, str]


@dataclass
class HistoryPanel:
    entries: List[HistoryEntry] = field(default_factory=list)
    cursor: int = -1

    def push(self, entry: HistoryEntry) -> None:
        if self.cursor < len(self.entries) - 1:
            self.entries = self.entries[: self.cursor + 1]
        self.entries.append(entry)
        self.cursor = len(self.entries) - 1

    def undo(self) -> Optional[HistoryEntry]:
        if self.cursor < 0:
            return None
        entry = self.entries[self.cursor]
        self.cursor -= 1
        return entry

    def redo(self) -> Optional[HistoryEntry]:
        if self.cursor + 1 >= len(self.entries):
            return None
        self.cursor += 1
        return self.entries[self.cursor]
