from __future__ import annotations

from typing import Iterable, List, Sequence, Tuple

from .core import Mesh, MeshChange, Vector


def _add_vectors(a: Vector, b: Vector) -> Vector:
    return a[0] + b[0], a[1] + b[1], a[2] + b[2]


def _scale_vector(a: Vector, scale: float) -> Vector:
    return a[0] * scale, a[1] * scale, a[2] * scale


def _face_normal(mesh: Mesh, face_id: int) -> Vector:
    vertices = mesh.face_vertices(face_id)
    if len(vertices) < 3:
        return (0.0, 0.0, 1.0)
    p0 = mesh.vertex_position(vertices[0])
    p1 = mesh.vertex_position(vertices[1])
    p2 = mesh.vertex_position(vertices[2])
    u = (p1[0] - p0[0], p1[1] - p0[1], p1[2] - p0[2])
    v = (p2[0] - p0[0], p2[1] - p0[1], p2[2] - p0[2])
    normal = (
        u[1] * v[2] - u[2] * v[1],
        u[2] * v[0] - u[0] * v[2],
        u[0] * v[1] - u[1] * v[0],
    )
    return normal


def extrude_faces(mesh: Mesh, face_ids: Iterable[int], distance: float = 1.0) -> MeshChange:
    change = MeshChange()
    for face_id in face_ids:
        vertices = mesh.face_vertices(face_id)
        normal = _face_normal(mesh, face_id)
        offset = _scale_vector(normal, distance)
        new_vertices = [
            mesh.add_vertex(_add_vectors(mesh.vertex_position(vertex_id), offset), change)
            for vertex_id in vertices
        ]
        mesh.add_face(new_vertices, change)
        for idx, vertex_id in enumerate(vertices):
            next_idx = (idx + 1) % len(vertices)
            side_face = [
                vertex_id,
                vertices[next_idx],
                new_vertices[next_idx],
                new_vertices[idx],
            ]
            mesh.add_face(side_face, change)
    return change


def inset_faces(mesh: Mesh, face_ids: Iterable[int], scale: float = 0.9) -> MeshChange:
    change = MeshChange()
    for face_id in face_ids:
        vertices = mesh.face_vertices(face_id)
        center = _face_center(mesh, vertices)
        inset_vertices = []
        for vertex_id in vertices:
            position = mesh.vertex_position(vertex_id)
            direction = (
                position[0] - center[0],
                position[1] - center[1],
                position[2] - center[2],
            )
            inset_position = _add_vectors(center, _scale_vector(direction, scale))
            inset_vertices.append(mesh.add_vertex(inset_position, change))
        mesh.add_face(inset_vertices, change)
    return change


def bevel_edges(
    mesh: Mesh, edge_ids: Iterable[int], offset: float = 0.1
) -> MeshChange:
    change = MeshChange()
    for edge_id in edge_ids:
        v1, v2 = mesh.edge_vertices(edge_id)
        p1 = mesh.vertex_position(v1)
        p2 = mesh.vertex_position(v2)
        direction = (p2[0] - p1[0], p2[1] - p1[1], p2[2] - p1[2])
        bevel_p1 = _add_vectors(p1, _scale_vector(direction, offset))
        bevel_p2 = _add_vectors(p2, _scale_vector(direction, -offset))
        bv1 = mesh.add_vertex(bevel_p1, change)
        bv2 = mesh.add_vertex(bevel_p2, change)
        mesh.add_edge(bv1, bv2, change)
    return change


def loop_cut_edges(mesh: Mesh, edge_ids: Iterable[int], t: float = 0.5) -> MeshChange:
    change = MeshChange()
    for edge_id in edge_ids:
        v1, v2 = mesh.edge_vertices(edge_id)
        p1 = mesh.vertex_position(v1)
        p2 = mesh.vertex_position(v2)
        new_position = (
            p1[0] + (p2[0] - p1[0]) * t,
            p1[1] + (p2[1] - p1[1]) * t,
            p1[2] + (p2[2] - p1[2]) * t,
        )
        mesh.add_vertex(new_position, change)
    return change


def merge_vertices(mesh: Mesh, vertex_ids: Sequence[int]) -> MeshChange:
    if not vertex_ids:
        return MeshChange()
    change = MeshChange()
    positions = [mesh.vertex_position(vertex_id) for vertex_id in vertex_ids]
    center = _average_vector(positions)
    target = vertex_ids[0]
    mesh.set_vertex_position(target, center, change)
    for vertex_id in vertex_ids[1:]:
        mesh.delete_vertex(vertex_id, change)
    return change


def delete_elements(
    mesh: Mesh,
    vertices: Iterable[int] = (),
    edges: Iterable[int] = (),
    faces: Iterable[int] = (),
) -> MeshChange:
    change = MeshChange()
    for face_id in list(faces):
        if face_id in mesh.faces:
            mesh.delete_face(face_id, change)
    for vertex_id in list(vertices):
        if vertex_id in mesh.vertices:
            mesh.delete_vertex(vertex_id, change)
    for edge_id in list(edges):
        if edge_id in mesh.edges:
            edge = mesh.edges.pop(edge_id)
            if change is not None:
                change.removed_edges[edge_id] = edge
    return change


def dissolve_edges(mesh: Mesh, edge_ids: Iterable[int]) -> MeshChange:
    change = MeshChange()
    for edge_id in edge_ids:
        if edge_id in mesh.edges:
            edge = mesh.edges.pop(edge_id)
            change.removed_edges[edge_id] = edge
    return change


def _average_vector(vectors: List[Vector]) -> Vector:
    if not vectors:
        return (0.0, 0.0, 0.0)
    total = (0.0, 0.0, 0.0)
    for vector in vectors:
        total = _add_vectors(total, vector)
    return _scale_vector(total, 1.0 / len(vectors))


def _face_center(mesh: Mesh, vertex_ids: Sequence[int]) -> Vector:
    positions = [mesh.vertex_position(vertex_id) for vertex_id in vertex_ids]
    return _average_vector(positions)
