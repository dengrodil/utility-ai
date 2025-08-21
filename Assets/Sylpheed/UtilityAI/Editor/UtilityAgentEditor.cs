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
                var bestResult = agent.DecisionResults.SingleOrDefault(d => d.Best);
            
                // if (bestResult != null)
                // {
                //     GUI.color = LabelColors.Best;
                //     EditorGUILayout.LabelField($"Current Decision: {bestResult.Decision.Behavior.name}", EditorStyles.boldLabel);
                //     if (bestResult.Decision.Target) EditorGUILayout.LabelField($"Target: {bestResult.Decision.Behavior.name}", EditorStyles.boldLabel);
                // }
            
                // EditorGUILayout.Space();
                // EditorGUILayout.LabelField("Actions/Considerations", EditorStyles.boldLabel);

                foreach (var result in results)
                {
                    if (result.Best) GUI.color = LabelColors.Best;
                    else if (result.Scored)
                    {
                        if (result.Skipped) GUI.color = LabelColors.Skipped;
                        else GUI.color = LabelColors.Scored;
                    }
                    else GUI.color = LabelColors.Unscored;

                    EditorGUILayout.LabelField($"{result.Decision.Behavior.name}, Score: {result.Decision.Score:P0}", EditorStyles.boldLabel);
                    
                    GUI.color = defaultLabelColor;
                }
            
            
                // foreach (AIAction action in agent.actions)
                // {
                //     float utility = action.CalculateUtility(agent.context);
                //     EditorGUILayout.LabelField($"Action: {action.name}, Utility: {utility:F2}");
                //
                //     // Draw the single consideration for the action
                //     DrawConsideration(action.consideration, agent.context, 1);
                // }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play mode to view utility values.", MessageType.Info);
            }
        }

        // private void DrawConsideration(Consideration consideration, Context context, int indentLevel)
        // {
        //     EditorGUI.indentLevel = indentLevel;
        //
        //     if (consideration is CompositeConsideration compositeConsideration)
        //     {
        //         EditorGUILayout.LabelField(
        //             $"Composite Consideration: {compositeConsideration.name}, Operation: {compositeConsideration.operation}"
        //         );
        //
        //         foreach (Consideration subConsideration in compositeConsideration.considerations)
        //         {
        //             DrawConsideration(subConsideration, context, indentLevel + 1);
        //         }
        //     }
        //     else
        //     {
        //         float value = consideration.Evaluate(context);
        //         EditorGUILayout.LabelField($"Consideration: {consideration.name}, Value: {value:F2}");
        //     }
        //
        //     EditorGUI.indentLevel = indentLevel - 1; // Reset indentation after drawing
        // }
        //
        // private AIAction GetChosenAction(Brain brain)
        // {
        //     float highestUtility = float.MinValue;
        //     AIAction chosenAction = null;
        //
        //     foreach (var action in brain.actions)
        //     {
        //         float utility = action.CalculateUtility(brain.context);
        //         if (utility > highestUtility)
        //         {
        //             highestUtility = utility;
        //             chosenAction = action;
        //         }
        //     }
        //
        //     return chosenAction;
        // }
    }
}