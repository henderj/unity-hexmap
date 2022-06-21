using UnityEngine;
using UnityEngine.UI;

namespace hex
{
    public class HexGridChunk : MonoBehaviour
    {
        private HexCell[] _cells;
        private HexMesh _hexMesh;
        private Canvas _gridCanvas;

        private void Awake()
        {
            _gridCanvas = GetComponentInChildren<Canvas>();
            _hexMesh = GetComponentInChildren<HexMesh>();

            _cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        }

        private void Start()
        {
            _hexMesh.Triangulate(_cells);
        }
    }
}