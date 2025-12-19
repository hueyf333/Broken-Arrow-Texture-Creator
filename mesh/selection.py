from __future__ import annotations

from dataclasses import dataclass, field
from typing import Iterable, List, Sequence, Tuple

from .core import Mesh, Vector


@dataclass
class Selection:
    vertices: set[int] = field(default_factory=set)
    edges: set[int] = field(default_factory=set)
    faces: set[int] = field(default_factory=set)

    def clear(self) -> None:
        self.vertices.clear()
        self.edges.clear()
        self.faces.clear()


def lasso_select(
    mesh: Mesh,
    lasso_points: Sequence[Tuple[float, float]],
    selection: Selection,
) -> None:
    selection.clear()
    for vertex_id, vertex in mesh.vertices.items():
        if _point_in_polygon((vertex.position[0], vertex.position[1]), lasso_points):
            selection.vertices.add(vertex_id)

    for edge_id, edge in mesh.edges.items():
        v1, v2 = mesh.edge_vertices(edge_id)
        if v1 in selection.vertices and v2 in selection.vertices:
            selection.edges.add(edge_id)

    for face_id in mesh.faces:
        vertices = mesh.face_vertices(face_id)
        if all(vertex_id in selection.vertices for vertex_id in vertices):
            selection.faces.add(face_id)


def select_grow(mesh: Mesh, selection: Selection) -> None:
    grown_vertices = set(selection.vertices)
    for vertex_id in list(selection.vertices):
        for neighbor in mesh.adjacency_vertices(vertex_id):
            grown_vertices.add(neighbor)
    selection.vertices = grown_vertices
    selection.edges = {edge_id for edge_id in mesh.edges if _edge_in_vertices(mesh, edge_id, selection.vertices)}
    selection.faces = {face_id for face_id in mesh.faces if _face_in_vertices(mesh, face_id, selection.vertices)}


def select_shrink(mesh: Mesh, selection: Selection) -> None:
    if not selection.vertices:
        return
    boundary = set()
    for vertex_id in selection.vertices:
        neighbors = mesh.adjacency_vertices(vertex_id)
        if any(neighbor not in selection.vertices for neighbor in neighbors):
            boundary.add(vertex_id)
    selection.vertices -= boundary
    selection.edges = {edge_id for edge_id in mesh.edges if _edge_in_vertices(mesh, edge_id, selection.vertices)}
    selection.faces = {face_id for face_id in mesh.faces if _face_in_vertices(mesh, face_id, selection.vertices)}


def _edge_in_vertices(mesh: Mesh, edge_id: int, vertices: Iterable[int]) -> bool:
    v1, v2 = mesh.edge_vertices(edge_id)
    vertex_set = set(vertices)
    return v1 in vertex_set and v2 in vertex_set


def _face_in_vertices(mesh: Mesh, face_id: int, vertices: Iterable[int]) -> bool:
    vertex_set = set(vertices)
    return all(vertex_id in vertex_set for vertex_id in mesh.face_vertices(face_id))


def _point_in_polygon(point: Tuple[float, float], polygon: Sequence[Tuple[float, float]]) -> bool:
    x, y = point
    inside = False
    if not polygon:
        return False
    j = len(polygon) - 1
    for i in range(len(polygon)):
        xi, yi = polygon[i]
        xj, yj = polygon[j]
        intersect = ((yi > y) != (yj > y)) and (
            x < (xj - xi) * (y - yi) / (yj - yi + 1e-9) + xi
        )
        if intersect:
            inside = not inside
        j = i
    return inside
