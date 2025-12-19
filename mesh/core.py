from __future__ import annotations

from dataclasses import dataclass, field
from typing import Dict, Iterable, List, Optional, Sequence, Tuple

Vector = Tuple[float, float, float]


@dataclass
class Vertex:
    id: int
    position: Vector
    halfedge: Optional[int] = None


@dataclass
class HalfEdge:
    id: int
    origin: int
    twin: Optional[int] = None
    next: Optional[int] = None
    prev: Optional[int] = None
    face: Optional[int] = None
    edge: Optional[int] = None


@dataclass
class Edge:
    id: int
    halfedge: int


@dataclass
class Face:
    id: int
    halfedge: int


@dataclass
class MeshChange:
    added_vertices: List[int] = field(default_factory=list)
    added_edges: List[int] = field(default_factory=list)
    added_halfedges: List[int] = field(default_factory=list)
    added_faces: List[int] = field(default_factory=list)
    removed_vertices: Dict[int, Vertex] = field(default_factory=dict)
    removed_edges: Dict[int, Edge] = field(default_factory=dict)
    removed_halfedges: Dict[int, HalfEdge] = field(default_factory=dict)
    removed_faces: Dict[int, Face] = field(default_factory=dict)
    updated_vertices: Dict[int, Vector] = field(default_factory=dict)

    def merge(self, other: "MeshChange") -> None:
        self.added_vertices.extend(other.added_vertices)
        self.added_edges.extend(other.added_edges)
        self.added_halfedges.extend(other.added_halfedges)
        self.added_faces.extend(other.added_faces)
        self.removed_vertices.update(other.removed_vertices)
        self.removed_edges.update(other.removed_edges)
        self.removed_halfedges.update(other.removed_halfedges)
        self.removed_faces.update(other.removed_faces)
        self.updated_vertices.update(other.updated_vertices)


class Mesh:
    def __init__(self) -> None:
        self._next_vertex_id = 1
        self._next_halfedge_id = 1
        self._next_edge_id = 1
        self._next_face_id = 1
        self.vertices: Dict[int, Vertex] = {}
        self.halfedges: Dict[int, HalfEdge] = {}
        self.edges: Dict[int, Edge] = {}
        self.faces: Dict[int, Face] = {}

    def add_vertex(self, position: Vector, change: Optional[MeshChange] = None) -> int:
        vertex_id = self._next_vertex_id
        self._next_vertex_id += 1
        self.vertices[vertex_id] = Vertex(id=vertex_id, position=position)
        if change is not None:
            change.added_vertices.append(vertex_id)
        return vertex_id

    def add_edge(self, origin: int, destination: int, change: Optional[MeshChange] = None) -> int:
        halfedge_id = self._next_halfedge_id
        twin_id = halfedge_id + 1
        self._next_halfedge_id += 2
        edge_id = self._next_edge_id
        self._next_edge_id += 1

        halfedge = HalfEdge(id=halfedge_id, origin=origin, twin=twin_id, edge=edge_id)
        twin = HalfEdge(id=twin_id, origin=destination, twin=halfedge_id, edge=edge_id)
        edge = Edge(id=edge_id, halfedge=halfedge_id)

        self.halfedges[halfedge_id] = halfedge
        self.halfedges[twin_id] = twin
        self.edges[edge_id] = edge

        if self.vertices[origin].halfedge is None:
            self.vertices[origin].halfedge = halfedge_id
        if self.vertices[destination].halfedge is None:
            self.vertices[destination].halfedge = twin_id

        if change is not None:
            change.added_halfedges.extend([halfedge_id, twin_id])
            change.added_edges.append(edge_id)

        return edge_id

    def add_face(self, vertex_cycle: Sequence[int], change: Optional[MeshChange] = None) -> int:
        if len(vertex_cycle) < 3:
            raise ValueError("Faces must have at least 3 vertices.")

        halfedge_ids: List[int] = []
        edge_ids: List[int] = []
        count = len(vertex_cycle)
        for index, origin in enumerate(vertex_cycle):
            destination = vertex_cycle[(index + 1) % count]
            edge_id = self.add_edge(origin, destination, change=change)
            edge_ids.append(edge_id)
            halfedge_ids.append(self.edges[edge_id].halfedge)

        for index, halfedge_id in enumerate(halfedge_ids):
            next_id = halfedge_ids[(index + 1) % count]
            prev_id = halfedge_ids[(index - 1) % count]
            halfedge = self.halfedges[halfedge_id]
            halfedge.next = next_id
            halfedge.prev = prev_id

        face_id = self._next_face_id
        self._next_face_id += 1
        self.faces[face_id] = Face(id=face_id, halfedge=halfedge_ids[0])
        for halfedge_id in halfedge_ids:
            self.halfedges[halfedge_id].face = face_id

        if change is not None:
            change.added_faces.append(face_id)

        return face_id

    def vertex_position(self, vertex_id: int) -> Vector:
        return self.vertices[vertex_id].position

    def set_vertex_position(
        self, vertex_id: int, position: Vector, change: Optional[MeshChange] = None
    ) -> None:
        if change is not None and vertex_id not in change.updated_vertices:
            change.updated_vertices[vertex_id] = self.vertices[vertex_id].position
        self.vertices[vertex_id].position = position

    def face_vertices(self, face_id: int) -> List[int]:
        halfedge_id = self.faces[face_id].halfedge
        vertices = []
        current = halfedge_id
        while True:
            halfedge = self.halfedges[current]
            vertices.append(halfedge.origin)
            current = halfedge.next
            if current == halfedge_id or current is None:
                break
        return vertices

    def delete_face(self, face_id: int, change: Optional[MeshChange] = None) -> None:
        face = self.faces.pop(face_id)
        if change is not None:
            change.removed_faces[face_id] = face

        halfedge_id = face.halfedge
        current = halfedge_id
        while True:
            halfedge = self.halfedges.pop(current)
            if change is not None:
                change.removed_halfedges[halfedge.id] = halfedge
            edge = self.edges.pop(halfedge.edge)
            if change is not None:
                change.removed_edges[edge.id] = edge
            current = halfedge.next
            if current == halfedge_id or current is None:
                break

    def delete_vertex(self, vertex_id: int, change: Optional[MeshChange] = None) -> None:
        vertex = self.vertices.pop(vertex_id)
        if change is not None:
            change.removed_vertices[vertex_id] = vertex

    def apply_change(self, change: MeshChange) -> None:
        for vertex_id, position in change.updated_vertices.items():
            self.vertices[vertex_id].position = position

    def restore_change(self, change: MeshChange) -> None:
        for vertex_id in change.added_vertices:
            self.vertices.pop(vertex_id, None)
        for halfedge_id in change.added_halfedges:
            self.halfedges.pop(halfedge_id, None)
        for edge_id in change.added_edges:
            self.edges.pop(edge_id, None)
        for face_id in change.added_faces:
            self.faces.pop(face_id, None)

        self.vertices.update(change.removed_vertices)
        self.halfedges.update(change.removed_halfedges)
        self.edges.update(change.removed_edges)
        self.faces.update(change.removed_faces)

        for vertex_id, position in change.updated_vertices.items():
            if vertex_id in self.vertices:
                self.vertices[vertex_id].position = position

    def adjacency_vertices(self, vertex_id: int) -> List[int]:
        adjacent = []
        start = self.vertices[vertex_id].halfedge
        if start is None:
            return adjacent
        current = start
        visited = set()
        while current is not None and current not in visited:
            visited.add(current)
            halfedge = self.halfedges[current]
            twin_id = halfedge.twin
            if twin_id is not None:
                adjacent.append(self.halfedges[twin_id].origin)
                current = self.halfedges[twin_id].next
            else:
                break
        return adjacent

    def edge_vertices(self, edge_id: int) -> Tuple[int, int]:
        halfedge = self.halfedges[self.edges[edge_id].halfedge]
        twin = self.halfedges[halfedge.twin] if halfedge.twin is not None else None
        destination = twin.origin if twin is not None else halfedge.origin
        return halfedge.origin, destination

    def iter_faces(self) -> Iterable[int]:
        return self.faces.keys()
