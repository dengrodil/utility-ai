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
        
        private UtilityAgent _agent;
        
        private void OnEnable()
        {
            _agent = (UtilityAgent)target;
            RequiresConstantRepaint();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI(); // Draw the default inspector
            
            var defaultLabelColor = GUI.color;
            
            EditorGUILayout.Space();
            if (Application.isPlaying)
            {
                // Sort results
                var results = _agent.DecisionResults
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

                    var text = $"[{result.Decision.Score * 100:N0}] {result.Decision.Behavior.name}\t";
                    if (result.Decision.Target) text += $" Target: {result.Decision.Target.name}";
                    if (result.Decision.Data != null) text += $" Data: {result.Decision.Data}";
                    EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
                    
                    GUI.color = defaultLabelColor;
                    
                    // Same decision
                    if (result.IsSameDecision && !Mathf.Approximately(_agent.SameDecisionScoreBonus, 1f))
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField($"[{_agent.SameDecisionScoreBonus * 100:N0}] Same Decision Bonus");
                        EditorGUI.indentLevel--;
                    }
                    
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
                DrawConsideration(consideration, decision);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawConsideration(IConsideration consideration, Decision decision)
        {
            var text = $"[{_agent.GetCachedConsiderationScore(decision, consideration) * 100:N0}] {consideration.Name}";
            EditorGUILayout.LabelField(text);

            // Draw child recursively
            if (consideration.Children != null)
            {
                EditorGUI.indentLevel++;
                foreach (var child in consideration.Children)
                {
                    DrawConsideration(child, decision);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}