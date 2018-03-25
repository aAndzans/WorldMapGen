using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace WorldMapGen
{
    // Class to draw warnings for map generator parameters
    [CustomPropertyDrawer(typeof(WarningAttribute))]
    public class WarningDrawer : PropertyDrawer
    {
        // Vertical padding for the warning
        private const float padding = 12.0f;

        // Should the warning be displayed?
        private bool showWarning = false;
        // The attribute corresponding to this drawer
        private WarningAttribute warning;

        public override float GetPropertyHeight(
            SerializedProperty property, GUIContent label)
        {
            // If warning is not displayed, height is the property's usual
            // height
            float height = EditorGUI.GetPropertyHeight(property, true);

            // Get the object that the property belongs to
            object parent = GetParent(property);

            // Cast the attribute
            warning = attribute as WarningAttribute;

            // Check if the warning should be drawn
            MethodInfo condition = parent.GetType().GetMethod(
                warning.ConditionFunction);
            showWarning = (bool)condition.Invoke(parent, null);

            // If warning is displayed, add the warning's height
            if (showWarning)
            {
                height += GUI.skin.GetStyle("helpbox").CalcHeight(
                    new GUIContent(warning.Message),
                    EditorGUIUtility.currentViewWidth) + padding;
            }

            return height;
        }

        public override void OnGUI(
            Rect position, SerializedProperty property, GUIContent label)
        {
            // Draw the property
            EditorGUI.PropertyField(position, property, label, true);
            
            if (showWarning)
            {
                // Offset Y for the warning
                position.yMin += EditorGUI.GetPropertyHeight(property, true);

                // Draw the warning
                EditorGUI.HelpBox(
                    position, warning.Message, MessageType.Warning);
            }
        }

        // Return the object that the given property belongs to
        // This function and the two GetValue() functions below are taken from
        // https://answers.unity.com/questions/425012/get-the-instance-the-serializedproperty-belongs-to.html
        private static object GetParent(SerializedProperty property)
        {
            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            string[] elements = path.Split('.');
            foreach (string element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    string elementName = element.Substring(
                        0, element.IndexOf("["));
                    int index = System.Convert.ToInt32(
                        element.Substring(element.IndexOf("[")).
                            Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue(object source, string name)
        {
            if (source == null)
                return null;
            System.Type type = source.GetType();
            FieldInfo field = type.GetField(
                name, BindingFlags.NonPublic | BindingFlags.Public |
                      BindingFlags.Instance);
            if (field == null)
            {
                PropertyInfo propertyInfo = type.GetProperty(
                    name, BindingFlags.NonPublic | BindingFlags.Public |
                          BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (propertyInfo == null)
                    return null;
                return propertyInfo.GetValue(source, null);
            }
            return field.GetValue(source);
        }

        private static object GetValue(object source, string name, int index)
        {
            IEnumerable enumerable = GetValue(source, name) as IEnumerable;
            IEnumerator enumerator = enumerable.GetEnumerator();
            while (index-- >= 0)
                enumerator.MoveNext();
            return enumerator.Current;
        }
    }
}