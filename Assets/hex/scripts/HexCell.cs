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
                position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
                transform.localPosition = position;

                var uiPosition = uiRect.localPosition;
                uiPosition.z = -position.y;
                uiRect.localPosition = uiPosition;
            }
        }

        public Vector3 Position => transform.localPosition;

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