using UnityEngine;
using UnityEngine.Serialization;

namespace hex
{
    public class HexCell : MonoBehaviour
    {
        public HexCoordinates coordinates;
        public RectTransform uiRect;
        public Color color;

        public int Elevation
        {
            get
            {
                return elevation;
            }
            set
            {
                elevation = value;
                var position = transform.localPosition;
                position.y = value * HexMetrics.elevationStep;
                transform.localPosition = position;

                var uiPosition = uiRect.localPosition;
                uiPosition.z = elevation * -HexMetrics.elevationStep;
                uiRect.localPosition = uiPosition;
            }
        }

        private int elevation;

        [SerializeField] private HexCell[] neighbors;

        public HexCell GetNeighbor(HexDirection direction)
        {
            return neighbors[(int)direction];
        }

        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            neighbors[(int)direction] = cell;
            cell.neighbors[(int)direction.Opposite()] = this;
        }

        public HexEdgeType GetEdgeType(HexDirection direction)
        {
            return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
        }

        public HexEdgeType GetEdgeType(HexCell otherCell)
        {
            return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
        }
    }
}