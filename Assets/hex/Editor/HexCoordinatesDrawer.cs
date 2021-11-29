using UnityEditor;
using UnityEngine;

namespace hex.Editor
{
    [CustomPropertyDrawer(typeof(HexCoordinates))]
    public class HexCoordinatesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent label)
        {
            var coordinates = new HexCoordinates(property.FindPropertyRelative("x").intValue,
                property.FindPropertyRelative("z").intValue);
            // position = EditorGUI.PrefixLabel(position, new GUIContent("Coordinates"));
            GUI.Label(position, coordinates.ToString());
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}