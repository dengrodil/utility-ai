using UnityEditor;
using UnityEngine;

namespace Sylpheed.UtilityAI.Editor
{
    [CustomPropertyDrawer(typeof(ConsiderationDecorator))]
    public class ConsiderationDecoratorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var considerationProp = property.FindPropertyRelative("_consideration");
            var invertedProp = property.FindPropertyRelative("_inverted");
            var muteProp = property.FindPropertyRelative("_mute");

            GUILayout.Space(-EditorGUIUtility.singleLineHeight);
            EditorGUILayout.BeginHorizontal();
            
            considerationProp.objectReferenceValue = EditorGUILayout.ObjectField(
                considerationProp.objectReferenceValue, 
                typeof(Consideration), 
                false, 
                GUILayout.ExpandWidth(true)
                );

            // GUILayout.FlexibleSpace();
            
            invertedProp.boolValue = EditorGUILayout.ToggleLeft(
                "Inverted", 
                invertedProp.boolValue, 
                GUILayout.ExpandWidth(false),
                GUILayout.MaxWidth(80f)
                );
            
            muteProp.boolValue = EditorGUILayout.ToggleLeft(
                "Mute", 
                muteProp.boolValue, 
                GUILayout.ExpandWidth(false),
                GUILayout.MaxWidth(50f)
            );
            
            EditorGUILayout.EndHorizontal();
            
            property.serializedObject.ApplyModifiedProperties();
        }

        // public override UnityEngine.UIElements.VisualElement CreatePropertyGUI(UnityEditor.SerializedProperty property)
        // {
        //     
        //     return base.CreatePropertyGUI(property);
        // }
    }
}