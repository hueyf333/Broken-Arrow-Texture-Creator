"""UI components for docking panels and context-aware toolbars."""

from dataclasses import dataclass
from typing import Dict, List


@dataclass(frozen=True)
class DockingLayout:
    """Defines a dockable layout with resizable regions."""

    regions: Dict[str, float]

    def resize_region(self, region: str, size: float) -> "DockingLayout":
        if region not in self.regions:
            raise KeyError(f"Unknown region: {region}")
        if size <= 0 or size >= 1:
            raise ValueError("Region size must be between 0 and 1")
        updated = dict(self.regions)
        updated[region] = size
        return DockingLayout(updated)


@dataclass(frozen=True)
class ToolShelf:
    name: str
    tools: List[str]


@dataclass(frozen=True)
class ContextToolbar:
    context: str
    actions: List[str]


class ToolbarRegistry:
    """Tracks toolbars that appear based on user context."""

    def __init__(self) -> None:
        self._toolbars: Dict[str, ContextToolbar] = {}

    def register(self, toolbar: ContextToolbar) -> None:
        self._toolbars[toolbar.context] = toolbar

    def for_context(self, context: str) -> ContextToolbar:
        if context not in self._toolbars:
            raise KeyError(f"No toolbar registered for context '{context}'")
        return self._toolbars[context]


class ToolShelfRegistry:
    """Stores shelves that can be toggled per workspace."""

    def __init__(self) -> None:
        self._shelves: Dict[str, ToolShelf] = {}

    def register(self, shelf: ToolShelf) -> None:
        self._shelves[shelf.name] = shelf

    def list_shelves(self) -> List[ToolShelf]:
        return list(self._shelves.values())
