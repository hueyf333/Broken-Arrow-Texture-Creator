# Primary UI Toolkit Decision

## Requirements

We need a C++ UI toolkit that can support:

- Docking panels
- Property grids/inspectors
- Asset browser views
- Drag-and-drop workflows

## Evaluation

### Qt 6

- **Docking panels**: `QDockWidget` and `QMainWindow` offer production-ready docking.
- **Property grids**: model/view + delegate system supports structured inspectors with validation.
- **Asset browser**: `QTreeView`/`QListView` backed by `QAbstractItemModel` cover rich asset lists.
- **Drag-and-drop**: built-in DnD across widgets and views with MIME data.
- **Strengths**: comprehensive widget set, native integrations, tooling (Designer), strong accessibility.
- **Tradeoffs**: larger dependency footprint and licensing considerations.

### Dear ImGui

- **Docking panels**: docking is supported in the docking branch.
- **Property grids**: requires custom layouts and editing widgets per property type.
- **Asset browser**: achievable, but requires custom rendering and asset list tooling.
- **Drag-and-drop**: supported, but relies on immediate-mode patterns and custom state handling.
- **Strengths**: fast iteration, lightweight, flexible for debug tooling.
- **Tradeoffs**: more custom UI work for production features, less native widget behavior.

## Decision

We will use **Qt 6** as the primary UI toolkit. It provides mature docking, model/view tooling for
property grids and asset browsers, and robust drag-and-drop with less custom infrastructure.
Dear ImGui remains a good option for in-engine debug overlays, but it is not selected for the
main editor UI.
