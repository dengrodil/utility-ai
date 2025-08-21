using UnityEditor;
using UnityEngine.UIElements;

namespace Sylpheed.UtilityAI.Editor
{
    [CustomEditor(typeof(TYPE))]
    public class UtilityAgentEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            
            return base.CreateInspectorGUI();
        }
    }
}