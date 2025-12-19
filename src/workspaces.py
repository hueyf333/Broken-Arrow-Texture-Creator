"""Workspace layout definitions for the Broken Arrow editor."""

from dataclasses import dataclass, field
from typing import Dict, List


@dataclass(frozen=True)
class DockPanel:
    name: str
    region: str
    size_hint: float


@dataclass(frozen=True)
class ToolShelf:
    name: str
    tools: List[str]


@dataclass(frozen=True)
class Toolbar:
    name: str
    context: str
    actions: List[str]


@dataclass(frozen=True)
class WorkspaceLayout:
    name: str
    panels: List[DockPanel]
    shelves: List[ToolShelf]
    toolbars: List[Toolbar]
    metadata: Dict[str, str] = field(default_factory=dict)


def modeling_workspace() -> WorkspaceLayout:
    return WorkspaceLayout(
        name="Modeling",
        panels=[
            DockPanel("Viewport", "center", 0.65),
            DockPanel("Outliner", "right", 0.2),
            DockPanel("Properties", "right", 0.2),
            DockPanel("Tool Settings", "left", 0.2),
            DockPanel("Asset Browser", "bottom", 0.15),
        ],
        shelves=[
            ToolShelf("Primary", ["Extrude", "Bevel", "Inset", "Loop Cut"]),
            ToolShelf("Selection", ["Box Select", "Lasso Select", "Select Linked"]),
        ],
        toolbars=[
            Toolbar("Object", "object", ["Move", "Rotate", "Scale", "Pivot"]),
            Toolbar("Mesh", "mesh", ["Merge", "Bridge", "Knife", "Boolean"]),
        ],
        metadata={"default_scene": "modeling_scene"},
    )


def uv_workspace() -> WorkspaceLayout:
    return WorkspaceLayout(
        name="UV",
        panels=[
            DockPanel("Viewport", "center", 0.5),
            DockPanel("UV Editor", "right", 0.35),
            DockPanel("Outliner", "right", 0.15),
            DockPanel("UV Tools", "left", 0.2),
            DockPanel("Texture Preview", "bottom", 0.2),
        ],
        shelves=[
            ToolShelf("Unwrap", ["Unwrap", "Smart UV", "Project From View"]),
            ToolShelf("UV Ops", ["Pack", "Relax", "Minimize Stretch"]),
        ],
        toolbars=[
            Toolbar("UV", "uv", ["Seam", "Pin", "Align", "Snap"]),
            Toolbar("Texture", "texture", ["Paint", "Clone", "Blur", "Smudge"]),
        ],
        metadata={"default_scene": "uv_scene"},
    )


def sculpt_workspace() -> WorkspaceLayout:
    return WorkspaceLayout(
        name="Sculpt",
        panels=[
            DockPanel("Viewport", "center", 0.7),
            DockPanel("Brushes", "left", 0.2),
            DockPanel("Properties", "right", 0.2),
            DockPanel("Layer Stack", "right", 0.1),
        ],
        shelves=[
            ToolShelf("Sculpt", ["Clay", "Grab", "Crease", "Smooth"]),
            ToolShelf("Masking", ["Mask", "Invert Mask", "Hide"]),
        ],
        toolbars=[
            Toolbar("Sculpt", "sculpt", ["Voxel Remesh", "DynTopo", "Symmetry"]),
            Toolbar("Detail", "detail", ["Detail Size", "Auto Smooth", "Stroke"]),
        ],
        metadata={"default_scene": "sculpt_scene"},
    )


def render_workspace() -> WorkspaceLayout:
    return WorkspaceLayout(
        name="Render",
        panels=[
            DockPanel("Viewport", "center", 0.6),
            DockPanel("Camera", "right", 0.2),
            DockPanel("Render Settings", "right", 0.2),
            DockPanel("Timeline", "bottom", 0.2),
        ],
        shelves=[
            ToolShelf("Lighting", ["Area Light", "Point Light", "HDRI"]),
            ToolShelf("Render", ["Render", "Pause", "Region"]),
        ],
        toolbars=[
            Toolbar("Render", "render", ["Samples", "Denoise", "Output"]),
            Toolbar("Compositing", "composite", ["Nodes", "Scopes", "Save"]),
        ],
        metadata={"default_scene": "render_scene"},
    )


def default_workspaces() -> List[WorkspaceLayout]:
    return [
        modeling_workspace(),
        uv_workspace(),
        sculpt_workspace(),
        render_workspace(),
    ]
