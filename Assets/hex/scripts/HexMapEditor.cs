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
                hexGrid.ColorCell(hit.point, _activeColor);
            }
        }

        public void SelectColor(int index)
        {
            _activeColor = colors[index];
        }
    }
}