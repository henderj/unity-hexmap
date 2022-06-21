using System;
using TMPro;
using UnityEngine;

namespace hex
{
    public class HexGrid : MonoBehaviour
    {
        public int chunkCountX = 4, chunkCountZ = 3;
        public Color defaultColor = Color.white;
        public HexCell cellPrefab;
        public TMP_Text cellLabelPrefab;
        public HexGridChunk chunkPrefab;
        public Texture2D noiseSource;

        private int cellCountX, cellCountZ;
        private HexGridChunk[] _chunks;
        private HexCell[] _cells;
        private Canvas _gridCanvas;
        private HexMesh _hexMesh;

        private void Awake()
        {
            HexMetrics.noiseSource = noiseSource;

            _gridCanvas = GetComponentInChildren<Canvas>();
            _hexMesh = GetComponentInChildren<HexMesh>();
            cellCountX = chunkCountX * HexMetrics.chunkSizeX;
            cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
            _cells = new HexCell[cellCountX * cellCountZ];
            CreateCells();
        }

        private void Start()
        {
            _hexMesh.Triangulate(_cells);
        }

        private void OnEnable()
        {
            HexMetrics.noiseSource = noiseSource;
            // CreateCells();
        }

        private void CreateChunks()
        {
            _chunks = new HexGridChunk[chunkCountX * chunkCountZ];

            for (int z = 0, i = 0; z < chunkCountZ; z++)
            {
                for (int x = 0; x < chunkCountX; x++)
                {
                    var chunk = _chunks[i++] = Instantiate(chunkPrefab);
                    chunk.transform.SetParent(transform);
                }
            }
        }

        private void CreateCells()
        {
            foreach (var cell in _cells)
            {
                if (cell?.uiRect)
                {
                    Destroy(cell.uiRect.gameObject);
                }
            }
            _cells = new HexCell[cellCountX * cellCountZ];

            for (int z = 0, i = 0; z < cellCountZ; z++)
            {
                for (int x = 0; x < cellCountX; x++)
                {
                    CreateCell(x, z, i++);
                }
            }
        }

        private void CreateCell(int x, int z, int i)
        {
            Vector3 position;
            // ReSharper disable once PossibleLossOfFraction
            position.x = (x + z * 0.5f - z / 2) * (HexMetrics.InnerRadius * 2f);
            position.y = 0f;
            position.z = z * (HexMetrics.OuterRadius * 1.5f);
            var cell = _cells[i] = Instantiate<HexCell>(cellPrefab, transform, false);
            cell.transform.localPosition = position;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            cell.name = cell.coordinates.ToString();
            cell.color = defaultColor;

            if (x > 0)
            {
                cell.SetNeighbor(HexDirection.W, _cells[i - 1]);
            }

            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, _cells[i - cellCountX]);
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, _cells[i - cellCountX - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, _cells[i - cellCountX]);
                    if (x < cellCountX - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, _cells[i - cellCountX + 1]);
                    }
                }
            }

            var label = Instantiate<TMP_Text>(cellLabelPrefab, _gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
            label.SetText(cell.coordinates.ToStringOnSeparateLines());

            cell.uiRect = label.rectTransform;
            cell.Elevation = 0;
        }


        public HexCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            var coordinates = HexCoordinates.FromPosition(position);
            var index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
            return _cells[index];
        }

        public void Refresh()
        {
            _hexMesh.Triangulate(_cells);
        }
    }
}