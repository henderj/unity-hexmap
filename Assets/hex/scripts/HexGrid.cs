using System;
using TMPro;
using UnityEngine;

namespace hex
{
    public class HexGrid : MonoBehaviour
    {
        public int width = 6;
        public int height = 6;

        public Color defaultColor = Color.white;

        public HexCell cellPrefab;
        public TMP_Text cellLabelPrefab;

        private HexCell[] _cells;
        private Canvas _gridCanvas;
        private HexMesh _hexMesh;

        private void Awake()
        {
            _gridCanvas = GetComponentInChildren<Canvas>();
            _hexMesh = GetComponentInChildren<HexMesh>();
        }

        private void Start()
        {
            _hexMesh.Triangulate(_cells);
        }

        private void OnEnable()
        {
            _cells = new HexCell[width * height];

            for (int z = 0, i = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
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
                    cell.SetNeighbor(HexDirection.SE, _cells[i - width]);
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, _cells[i - width - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, _cells[i - width]);
                    if (x < width - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, _cells[i - width + 1]);
                    }
                }
            }

            var label = Instantiate<TMP_Text>(cellLabelPrefab, _gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
            label.SetText(cell.coordinates.ToStringOnSeparateLines());

            cell.uiRect = label.rectTransform;
        }


        public HexCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            var coordinates = HexCoordinates.FromPosition(position);
            var index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
            return _cells[index];
        }

        public void Refresh()
        {
            _hexMesh.Triangulate(_cells);
        }
    }
}