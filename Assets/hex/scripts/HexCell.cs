using UnityEngine;
using UnityEngine.Serialization;

namespace hex
{
    public class HexCell : MonoBehaviour
    {
        public HexCoordinates coordinates;
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
                position.y = value * HexMatrics.elevationStep;
                transform.localPosition = position;
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
    }
}