using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sylpheed.UtilityAI.Editor
{
    [CustomEditor(typeof(UtilityAgent))]
    public class UtilityAgentEditor : UnityEditor.Editor
    {
        private struct LabelColors
        {
            public static readonly Color Best = Color.green;
            public static readonly Color Scored = Color.orange;
            public static readonly Color Skipped = Color.red;
            public static readonly Color Unscored = Color.gray;
        }
        
        private void OnEnable()
        {
            RequiresConstantRepaint();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI(); // Draw the default inspector

            var agent = (UtilityAgent)target;
            var defaultLabelColor = GUI.color;
            
            EditorGUILayout.Space();
            if (Application.isPlaying)
            {
                // Sort results
                var results = agent.DecisionResults
                    .OrderByDescending(d => d.Best)
                    .ThenByDescending(d => d.Scored)
                    .ThenBy(d => d.Skipped)
                    .ThenByDescending(d => d.Score)
                    .ToList();

                foreach (var result in results)
                {
                    if (result.Best) GUI.color = LabelColors.Best;
                    else if (result.Scored)
                    {
                        if (result.Skipped) GUI.color = LabelColors.Skipped;
                        else GUI.color = LabelColors.Scored;
                    }
                    else GUI.color = LabelColors.Unscored;

                    var text = $"[{result.Decision.Behavior.name}]";
                    if (result.Decision.Target) text += $"\tTarget: {result.Decision.Target.name}";
                    if (result.Decision.Data != null) text += $"\tData: {result.Decision.Data}";
                    text += $"\t=> {result.Decision.Score * 100:N0}";
                    EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
                    
                    GUI.color = defaultLabelColor;
                    
                    DrawConsiderations(result.Decision);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play mode to view utility values.", MessageType.Info);
            }
        }

        private void DrawConsiderations(Decision decision)
        {
            EditorGUI.indentLevel++;

            foreach (var consideration in decision.Behavior.Considerations)
            {
                DrawConsideration(consideration);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawConsideration(IConsideration consideration)
        {
            var text = $"[{consideration.Name}]\t=> {999}";
            EditorGUILayout.LabelField(text);
        }
    }
}