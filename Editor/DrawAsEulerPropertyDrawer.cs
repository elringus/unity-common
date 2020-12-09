using UnityEditor;
using UnityEngine;

namespace UnityCommon
{
    [CustomPropertyDrawer(typeof(DrawAsEulerAttribute))]
    public class DrawAsEulerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI (Rect rect, SerializedProperty property, GUIContent label)
        {
            var euler = property.quaternionValue.eulerAngles;
            EditorGUI.BeginProperty(rect, label, property);
            EditorGUI.BeginChangeCheck();
            euler = EditorGUI.Vector3Field(rect, label, euler);
            if (EditorGUI.EndChangeCheck())
                property.quaternionValue = Quaternion.Euler(euler);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
