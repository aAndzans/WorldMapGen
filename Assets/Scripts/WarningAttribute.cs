using UnityEngine;

namespace WorldMapGen
{
    // Attribute that causes a property to show a warning when a condition
    // is met
    public class WarningAttribute : PropertyAttribute
    {
        // Name of the function that is the condition for the warning (must
        // return bool and take no parameters)
        public string ConditionFunction { get; private set; }
        // The text of the warning message
        public string Message { get; private set; }

        public WarningAttribute(string conditionFunction, string message)
        {
            ConditionFunction = conditionFunction;
            Message = message;
        }
    }
}