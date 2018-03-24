using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace WorldMapGen
{
    // Class to draw warnings for map generator parameters
    [CustomPropertyDrawer(typeof(MapParameters.WarningAttribute))]
    public class WarningDrawer : PropertyDrawer
    {
        // Should the warning be displayed?
        private bool showWarning = false;
        // The attribute corresponding to this drawer
        private MapParameters.WarningAttribute warning;

        public override float GetPropertyHeight(
            SerializedProperty property, GUIContent label)
        {
            // If warning is not displayed, height is the property's usual
            // height
            float height = EditorGUI.GetPropertyHeight(property, true);

            // If warning is displayed, add the warning's height
            if (showWarning)
            {
                height += GUI.skin.GetStyle("helpbox").CalcHeight(
                    new GUIContent(warning.Message),
                    EditorGUIUtility.currentViewWidth);
            }

            return height;
        }

        public override void OnGUI(
            Rect position, SerializedProperty property, GUIContent label)
        {
            // Draw the property
            EditorGUI.PropertyField(position, property, label, true);

            // Cast the attribute
            warning = attribute as MapParameters.WarningAttribute;
            // Get the parameters object
            MapParameters parameters = (
                property.serializedObject.targetObject as
                MapGenerator).Parameters;

            // If the warning condition is true, draw the warning
            MethodInfo condition = parameters.GetType().GetMethod(
                warning.ConditionFunction);
            showWarning = (bool)condition.Invoke(parameters, null);
            if (showWarning)
            {
                // Offset Y for the warning
                position.yMin += EditorGUI.GetPropertyHeight(property, true);

                EditorGUI.HelpBox(
                    position, warning.Message, MessageType.Warning);
            }
        }
    }
}