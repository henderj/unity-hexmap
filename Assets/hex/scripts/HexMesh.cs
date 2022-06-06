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
            var center = cell.Position;
            var v1 = center + HexMetrics.GetFirstSolidCorner(direction);
            var v2 = center + HexMetrics.GetSecondSolidCorner(direction);

            var e1 = Vector3.Lerp(v1, v2, 1f / 3f);
            var e2 = Vector3.Lerp(v1, v2, 2f / 3f);

            AddTriangle(center, v1, e1);
            AddTriangleColor(cell.color);
            AddTriangle(center, e1, e2);
            AddTriangleColor(cell.color);
            AddTriangle(center, e2, v2);
            AddTriangleColor(cell.color);

            if (direction <= HexDirection.SE)
            {
                TriangulateConnection(direction, cell, v1, e1, e2, v2);
            }
        }

        private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 e1, Vector3 e2, Vector3 v2)
        {
            var neighbor = cell.GetNeighbor(direction);
            if (neighbor == null) return;

            var bridge = HexMetrics.GetBridge(direction);
            var v3 = v1 + bridge;
            var v4 = v2 + bridge;
            v3.y = v4.y = neighbor.Position.y;

            var e3 = Vector3.Lerp(v3, v4, 1f / 3f);
            var e4 = Vector3.Lerp(v3, v4, 2f / 3f);

            if (HexMetrics.GetEdgeType(cell.Elevation, neighbor.Elevation) == HexEdgeType.Slope)
            {
                TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
            }
            else
            {
                AddQuad(v1, e1, v3, e3);
                AddQuadColor(cell.color, neighbor.color);
                AddQuad(e1, e2, e3, e4);
                AddQuadColor(cell.color, neighbor.color);
                AddQuad(e2, v2, e4, v4);
                AddQuadColor(cell.color, neighbor.color);
            }

            var nextNeighbor = cell.GetNeighbor(direction.Next());
            if (direction > HexDirection.E || nextNeighbor == null) return;

            var v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Position.y;
            if (cell.Elevation <= neighbor.Elevation)
            {
                if (cell.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {
                TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
            }
            // AddTriangle(v2, v4, v5);
            // AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
        }

        private void TriangulateEdgeTerraces(Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
        Vector3 endLeft, Vector3 endRight, HexCell endCell)
        {
            var v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
            var v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
            var c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);
            AddQuad(beginLeft, beginRight, v3, v4);
            AddQuadColor(beginCell.color, c2);

            for (var i = 2; i < HexMetrics.terraceSteps; i++)
            {
                var v1 = v3;
                var v2 = v4;
                var c1 = c2;
                v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
                v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
                c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2);
            }

            AddQuad(v3, v4, endLeft, endRight);
            AddQuadColor(c2, endCell.color);
        }

        private void TriangulateCorner(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if (leftEdgeType == HexEdgeType.Slope)
            {
                if (rightEdgeType == HexEdgeType.Slope)
                {
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                    return;
                }
                if (rightEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                    return;
                }
                TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                return;
            }
            if (rightEdgeType == HexEdgeType.Slope)
            {
                if (leftEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                    return;
                }
                TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                return;
            }
            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                if (leftCell.Elevation < rightCell.Elevation)
                {
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);

                }
                else
                {
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
                }
                return;
            }

            AddTriangle(bottom, left, right);
            AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
        }

        private void TriangulateCornerTerraces(
            Vector3 begin, HexCell beginCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            var v3 = HexMetrics.TerraceLerp(begin, left, 1);
            var v4 = HexMetrics.TerraceLerp(begin, right, 1);
            var c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
            var c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);

            AddTriangle(begin, v3, v4);
            AddTriangleColor(beginCell.color, c3, c4);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                var v1 = v3;
                var v2 = v4;
                var c1 = c3;
                var c2 = c4;
                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
                c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);

                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2, c3, c4);
            }

            AddQuad(v3, v4, left, right);
            AddQuadColor(c3, c4, leftCell.color, rightCell.color);
        }

        private void TriangulateCornerTerracesCliff(
            Vector3 begin, HexCell beginCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell
        )
        {
            float b = 1f / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0)
            {
                b = -b;
            }
            var boundary = Vector3.Lerp(begin, right, b);
            Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);

            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);
            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }
            else
            {
                AddTriangle(left, right, boundary);
                AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
            }
        }

        private void TriangulateCornerCliffTerraces(
            Vector3 begin, HexCell beginCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell
        )
        {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0)
            {
                b = -b;
            }
            var boundary = Vector3.Lerp(begin, left, b);
            Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, b);

            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);
            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }
            else
            {
                AddTriangle(left, right, boundary);
                AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
            }
        }
        private void TriangulateBoundaryTriangle(
            Vector3 begin, HexCell beginCell,
            Vector3 left, HexCell leftCell,
            Vector3 boundary, Color boundaryColor
        )
        {
            var v2 = HexMetrics.TerraceLerp(begin, left, 1);
            var c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);

            AddTriangle(begin, v2, boundary);
            AddTriangleColor(beginCell.color, c2, boundaryColor);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                var v1 = v2;
                var c1 = c2;
                v2 = HexMetrics.TerraceLerp(begin, left, i);
                c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);

                AddTriangle(v1, v2, boundary);
                AddTriangleColor(c1, c2, boundaryColor);
            }

            AddTriangle(v2, left, boundary);
            AddTriangleColor(c2, leftCell.color, boundaryColor);
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
            _vertices.Add(Perturb(v1));
            _vertices.Add(Perturb(v2));
            _vertices.Add(Perturb(v3));
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);
        }

        private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            var vertexIndex = _vertices.Count;
            _vertices.Add(Perturb(v1));
            _vertices.Add(Perturb(v2));
            _vertices.Add(Perturb(v3));
            _vertices.Add(Perturb(v4));
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

        private Vector3 Perturb(Vector3 position)
        {
            var sample = HexMetrics.SampleNoise(position);
            position.x += (sample.x * 2f - 1f) * HexMetrics.cellPerturbStrength;
            // position.y += (sample.y * 2f - 1f) * HexMetrics.cellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * HexMetrics.cellPerturbStrength;
            return position;
        }
    }
}