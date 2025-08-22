using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylpheed.UtilityAI
{
    public sealed class Decision
    {
        public Behavior Behavior { get; private set; }
        
        public UtilityAgent Agent { get; private set; }
        public UtilityTarget Target { get; private set; }
        
        public float Score { get; private set; }
        public float MaxScore => Behavior.Weight;
        
        private object _data;
        public object Data => _data;
        public T GetData<T>() where T : class => _data as T;
        public bool TryGetData<T>(out T data) where T : class => (data = _data as T) != null;
        
        /// <summary>
        /// A Decision is scored if the Evaluate function was called regardless of its result.
        /// </summary>
        public bool Scored { get; private set; }
        /// <summary>
        /// A Decision is skipped if it cannot score higher than the threshold.
        /// </summary>
        public bool Skipped  { get; private set; }
        /// <summary>
        /// A Decision is concluded if the Action was completed or prematurely halted due to some conditions.
        /// It will not be concluded if the UtilityAgent changed Decision while a Decision is still being enacted.
        /// </summary>
        public bool Concluded { get; private set; }
        
        // TODO: Move to a different builder class
        #region Builder
        public static Decision Create(UtilityAgent agent, Behavior behavior)
        {
            var decision = new Decision()
            {
                Agent = agent,
                Behavior = behavior,
            };
   
            return decision;
        }

        public Decision WithTarget(UtilityTarget target)
        {
            Target = target;
            return this;
        }
            
        public Decision WithData<T>(T data) 
            where T : class
        {
            _data =  data;
            return this;
        }
        #endregion

        /// <summary>
        /// Evaluates all considerations for this behavior against a specific target (if applicable)
        /// </summary>
        /// <param name="scoreThreshold">Stops evaluating remaining considerations if this decision can no longer score higher than the threshold.</param>
        /// <param name="agentBonus">Score bonus provided by the UtilityAgent (eg. same decision bonus)</param>
        /// <param name="scoreCache">Cache of score based on a permutation of agent, target, consideration, and data. If cached, skip evaluation and use cache.</param>
        /// <returns>Score. Result is cached.</returns>
        public float Evaluate(float scoreThreshold, float agentBonus, IDictionary<int, float> scoreCache)
        {
            Scored = true;
            
            // Evaluate each consideration
            var finalScore = 1f;
            var bonus = Behavior.Weight * agentBonus;
            for (var i = 0; i < Behavior.Considerations.Count; i++)
            {
                var consideration = Behavior.Considerations[i];

                // Stop evaluating if this decision is already vetoed by a consideration that scored 0.
                if (Mathf.Approximately(finalScore, 0))
                {
                    Skipped = true;
                    break;
                }
                
                // Stop evaluating if this decision can no longer beat the score threshold
                var projectedMaxScore = Mathf.Pow(finalScore, 1f / (i + 1)) * bonus;
                if (projectedMaxScore < scoreThreshold)
                {
                    Skipped = true;
                    break;
                }
                
                // Evaluate consideration score
                var score = EvaluateConsideration(consideration, scoreCache);
                finalScore *= score;
            }
            
            // Apply compensation factor based on number of considerations
            if (finalScore > 0) finalScore = Mathf.Pow(finalScore, 1f / Behavior.Considerations.Count);
            
            // Apply bonus
            finalScore *= bonus;
            
            Score = finalScore;
            return finalScore;
        }

        private float EvaluateConsideration(IConsideration consideration, IDictionary<int, float> scoreCache)
        {
            // Get cached score
            var hash = BuildConsiderationHash(consideration);
            var cached = scoreCache.TryGetValue(hash, out var score);

            // Skip evaluation if score is already cached.
            if (consideration.ShouldCacheScore && cached) return score;
            
            // No caching or not yet cached. Evaluate.
            score = consideration.Evaluate(this);
            scoreCache[hash] = score;
            return score;
        }

        public int BuildConsiderationHash(IConsideration consideration)
        {
            var hash = 17;
            hash = hash * 23 + (Agent?.GetHashCode() ?? 0);
            hash = hash * 23 + (Target?.GetHashCode() ?? 0);
            hash = hash * 23 + (_data?.GetHashCode() ?? 0);
            hash = hash * 23 + (consideration?.GetHashCode() ?? 0);
            return hash;
        }

        public Action Enact(System.Action onConcluded = null)
        {
            if (Behavior.Action == null) return null;
            
            // Clone action via json serialization
            var json = JsonUtility.ToJson(Behavior.Action);
            if (JsonUtility.FromJson(json, Behavior.Action.GetType()) is not Action action) throw new System.Exception("Unable to create action");

            action.Execute(this, () =>
            {
                Concluded = true;
                onConcluded?.Invoke();
            });
            
            return action;
        }

        /// <summary>
        /// Check if decision will yield to the same action, including data.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsSimilar(Decision a, Decision b)
        {
            if (a == null || b == null) return false;
            if (a.Behavior != b.Behavior) return false;
            if (a.Agent != b.Agent) return false;
            if (a.Target != b.Target) return false;
            if (a._data != b._data) return false;
            
            return true;
        }
    }
}