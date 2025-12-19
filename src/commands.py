"""Command system including hotkeys, radial menus, and search."""

from dataclasses import dataclass
from typing import Dict, List


@dataclass(frozen=True)
class Command:
    id: str
    label: str
    category: str


@dataclass(frozen=True)
class Hotkey:
    keys: str
    command_id: str
    context: str = "global"


@dataclass(frozen=True)
class RadialMenu:
    name: str
    command_ids: List[str]


class CommandRegistry:
    def __init__(self) -> None:
        self._commands: Dict[str, Command] = {}
        self._hotkeys: List[Hotkey] = []
        self._radials: Dict[str, RadialMenu] = {}

    def register_command(self, command: Command) -> None:
        self._commands[command.id] = command

    def register_hotkey(self, hotkey: Hotkey) -> None:
        self._hotkeys.append(hotkey)

    def register_radial(self, radial: RadialMenu) -> None:
        self._radials[radial.name] = radial

    def search(self, query: str) -> List[Command]:
        normalized = query.lower().strip()
        return [
            command
            for command in self._commands.values()
            if normalized in command.label.lower()
            or normalized in command.category.lower()
        ]

    def hotkeys_for_context(self, context: str) -> List[Hotkey]:
        return [hotkey for hotkey in self._hotkeys if hotkey.context in {"global", context}]

    def radial_menu(self, name: str) -> RadialMenu:
        if name not in self._radials:
            raise KeyError(f"No radial menu named '{name}'")
        return self._radials[name]


def default_commands() -> CommandRegistry:
    registry = CommandRegistry()
    for command in [
        Command("select.box", "Box Select", "Selection"),
        Command("select.lasso", "Lasso Select", "Selection"),
        Command("mesh.extrude", "Extrude", "Modeling"),
        Command("mesh.bevel", "Bevel", "Modeling"),
        Command("uv.unwrap", "Unwrap", "UV"),
        Command("sculpt.clay", "Clay", "Sculpt"),
        Command("render.start", "Render", "Render"),
    ]:
        registry.register_command(command)

    for hotkey in [
        Hotkey("Q", "select.box"),
        Hotkey("W", "mesh.extrude", "modeling"),
        Hotkey("E", "mesh.bevel", "modeling"),
        Hotkey("U", "uv.unwrap", "uv"),
        Hotkey("S", "sculpt.clay", "sculpt"),
        Hotkey("F12", "render.start", "render"),
    ]:
        registry.register_hotkey(hotkey)

    registry.register_radial(
        RadialMenu(
            "Selection",
            ["select.box", "select.lasso"],
        )
    )
    registry.register_radial(
        RadialMenu(
            "Modeling",
            ["mesh.extrude", "mesh.bevel"],
        )
    )
    return registry
