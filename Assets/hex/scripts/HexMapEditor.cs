using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace hex
{
    public class HexMapEditor : MonoBehaviour
    {
        public Color[] colors;
        public HexGrid hexGrid;

        private Color _activeColor;
        private int _activeElevation;

        private void Awake()
        {
            SelectColor(0);
        }


        private void Update()
        {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(inputRay, out var hit))
            {
                EditCell(hexGrid.GetCell(hit.point));
            }
        }

        private void EditCell(HexCell cell)
        {
            cell.color = _activeColor;
            cell.Elevation = _activeElevation;
            hexGrid.Refresh();
        }

        public void SelectColor(int index)
        {
            _activeColor = colors[index];
        }

        public void SetElevation(float elevation)
        {
            _activeElevation = (int)elevation;
        }
    }
}