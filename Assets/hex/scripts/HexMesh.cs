﻿using System.Collections.Generic;
using UnityEngine;

namespace hex
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour
    {
        private Mesh _hexMesh;
        private List<Vector3> _vertices;
        private List<int> _triangles;
        private MeshCollider _meshCollider;
        private List<Color> _colors;

        private void Awake()
        {
            GetComponent<MeshFilter>().mesh = _hexMesh = new Mesh();
            _meshCollider = gameObject.AddComponent<MeshCollider>();
            _hexMesh.name = "Hex Mesh";
            _vertices = new List<Vector3>();
            _triangles = new List<int>();
            _colors = new List<Color>();
        }

        public void Triangulate(IEnumerable<HexCell> cells)
        {
            _hexMesh.Clear();
            _vertices.Clear();
            _triangles.Clear();
            _colors.Clear();
            foreach (var c in cells)
            {
                Triangulate(c);
            }

            _hexMesh.vertices = _vertices.ToArray();
            _hexMesh.triangles = _triangles.ToArray();
            _hexMesh.colors = _colors.ToArray();
            _hexMesh.RecalculateNormals();

            _meshCollider.sharedMesh = _hexMesh;
        }

        private void Triangulate(HexCell cell)
        {
            for (var d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                Triangulate(d, cell);
            }
        }

        private void Triangulate(HexDirection direction, HexCell cell)
        {
            var center = cell.transform.localPosition;
            var v1 = center + HexMetrics.GetFirstSolidCorner(direction);
            var v2 = center + HexMetrics.GetSecondSolidCorner(direction);

            AddTriangle(center, v1, v2);
            AddTriangleColor(cell.color);

            if (direction <= HexDirection.SE)
            {
                TriangulateConnection(direction, cell, v1, v2);
            }
        }

        private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
        {
            var neighbor = cell.GetNeighbor(direction);
            if (neighbor == null) return;

            var bridge = HexMetrics.GetBridge(direction);
            var v3 = v1 + bridge;
            var v4 = v2 + bridge;
            v3.y = v4.y = neighbor.Elevation * HexMetrics.elevationStep;

            AddQuad(v1, v2, v3, v4);
            AddQuadColor(cell.color, neighbor.color);

            var nextNeighbor = cell.GetNeighbor(direction.Next());
            if (direction > HexDirection.E || nextNeighbor == null) return;
            var v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;
            AddTriangle(v2, v4, v5);
            AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
        }


        private void AddTriangleColor(Color color)
        {
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
        }

        private void AddTriangleColor(Color c1, Color c2, Color c3)
        {
            _colors.Add(c1);
            _colors.Add(c2);
            _colors.Add(c3);
        }

        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            var vertexIndex = _vertices.Count;
            _vertices.Add(v1);
            _vertices.Add(v2);
            _vertices.Add(v3);
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);
        }

        private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            var vertexIndex = _vertices.Count;
            _vertices.Add(v1);
            _vertices.Add(v2);
            _vertices.Add(v3);
            _vertices.Add(v4);
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 2);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);
            _triangles.Add(vertexIndex + 3);
        }

        private void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
        {
            _colors.Add(c1);
            _colors.Add(c2);
            _colors.Add(c3);
            _colors.Add(c4);
        }

        private void AddQuadColor(Color c1, Color c2)
        {
            _colors.Add(c1);
            _colors.Add(c1);
            _colors.Add(c2);
            _colors.Add(c2);
        }
    }
}