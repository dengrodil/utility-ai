using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylpheed.UtilityAI
{
    public class UtilityAgent : MonoBehaviour
    {
        [SerializeField] private BehaviorSet _baseBehaviorSet;
        [SerializeField] private float _targetSearchRadius = 100f;
        [SerializeField] private float _maxTargetsPerDecision = 12;
        [SerializeField] private float _decisionInterval = 1f;
        [SerializeField] private bool _useWeightedScoreProbability = true;
        [Tooltip("If a decision is still being enacted, modify the score of the current decision.")]
        [SerializeField] private float _sameDecisionScoreBonus = 1.25f;
        
        [Header("Debug")]
        [SerializeField] private bool _logToConsole;

        public IReadOnlyList<UtilityTarget> Targets { get; private set; } = new List<UtilityTarget>();
        public Decision CurrentDecision { get; private set; }
        public IReadOnlyCollection<DecisionResult> DecisionResults => _decisionResults;
        
        private List<BehaviorSet> _behaviorSets = new();
        private List<Behavior> _behaviors = new();
        private readonly RaycastHit[] _searchHits = new RaycastHit[30];
        private float _decisionTimer;
        private readonly Dictionary<int, float> _scoreCache = new(); // Key is hash of agent, consideration, data, and target
        private Action _currentAction;
        private Decision _previousDecision;
        private List<DecisionResult> _decisionResults = new();

        private void Awake()
        {
            // Add base behavior
            AddBehaviors(_baseBehaviorSet);
        }

        private void Update()
        {
            UpdateDecisions();
            _currentAction?.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _currentAction?.FixedUpdate(Time.fixedDeltaTime);
        }

        private void UpdateDecisions()
        {
            if (_decisionTimer >= _decisionInterval)
            {
                Think();
                _decisionTimer = 0;
            }
            else
            {
                _decisionTimer += Time.deltaTime;
            }
        }

        private void Think()
        {
            var decisions = BuildDecisions();
            var decision = Decide(decisions);
            EnactDecision(decision);
        }

        private void EnactDecision(Decision decision)
        {
            if (decision == null)
            {
                Log("wasn't able to come up with a decision.");
                return;
            }
            
            // Keep current decision if it doesn't yield to a new action
            if (Decision.IsSimilar(CurrentDecision, decision)) return;
            
            // Interrupt previous action if it's still running
            _currentAction?.Interrupt();
            
            // Enact decision
            Log($"enacted [{decision.Behavior.name}]. Score: {decision.Score:P2}");
            _previousDecision = CurrentDecision;
            CurrentDecision = decision;
            _currentAction = decision.Enact(onExit: () =>
            {
                // TODO: May cause infinite loop when decision always exits
                // Come up with a new decision once current action has concluded
                CurrentDecision = null;
                Think();
            });
        }

        #region Add/Remove Behaviors
        public void AddBehaviors(BehaviorSet behaviorSet)
        {
            if (behaviorSet == null) return;
            
            _behaviorSets.Add(behaviorSet);
            _behaviors.AddRange(behaviorSet.Behaviors);
        }

        public void AddBehaviors(params Behavior[] behaviors)
        {
            _behaviors.AddRange(behaviors);
        }

        public void RemoveBehaviors(BehaviorSet behaviorSet)
        {
            if (behaviorSet == null) return;
            
            _behaviorSets.Remove(behaviorSet);
            foreach (var behavior in behaviorSet.Behaviors)
                _behaviors.Remove(behavior);
        }
        
        public void RemoveBehaviors(params Behavior[] behaviors)
        {
            foreach (var behavior in behaviors)
                _behaviors.Remove(behavior);
        }
        #endregion
        
        private Decision Decide(IReadOnlyList<Decision> decisions)
        {
            if (!decisions.Any()) return null;
            
            // Clear
            _scoreCache.Clear();
            _decisionResults.Clear();
            
            // Evaluate all decisions
            DecisionResult bestResult = null;
            var bestScore = 0f;
            foreach (var decision in decisions)
            {
                // Cache result
                var result = new DecisionResult
                {
                    Decision = decision,
                    IsSameDecision = Decision.IsSimilar(CurrentDecision, decision),
                };
                _decisionResults.Add(result);
                
                // Ignore decision if it can no longer beat the current highest score
                if (decision.MaxScore < bestScore) continue;
                
                // Get score for this decision
                var bonus = EvaluateSimilarDecisionBonus(decision);
                var score = decision.Evaluate(bestScore, bonus, _scoreCache);
                
                // Decision beats current best decision. Update best decision.
                if (score > bestScore)
                {
                    bestResult = result;
                    bestScore = score;
                }
            }

            if (bestResult != null) bestResult.Best = true;
            return bestResult?.Decision;
        }

        private float EvaluateSimilarDecisionBonus(Decision decision)
        {
            if (!Decision.IsSimilar(CurrentDecision, decision)) return 1f;
            if (CurrentDecision.Concluded) return 1f;
            
            return _sameDecisionScoreBonus;
        }

        private IReadOnlyList<Decision> BuildDecisions()
        {
            // Get all targets
            var allTargets = SearchTargets();
            
            // Build decisions from behaviors
            return _behaviors
                .SelectMany(behavior => behavior.BuildDecisions(this, allTargets))
                .OrderByDescending(decision => decision.Behavior.Weight)
                .ToList();
        }

        private IReadOnlyList<UtilityTarget> SearchTargets()
        {
            // Get all utility targets within search radius and sort them by distance
            var size = Physics.SphereCastNonAlloc(transform.position, _targetSearchRadius, Vector3.down, _searchHits, _targetSearchRadius);
            var targets = _searchHits
                .Take(size)
                .Select(hit => hit.collider.GetComponentInParent<UtilityTarget>())
                .Where(target => target && target.enabled)
                .OrderBy(target => target.DistanceFromAgent(this))
                .ToList();

            Targets = targets;
            return targets;
        }

        private void Log(string message)
        {
            if (!_logToConsole) return;
            Debug.Log($"[Agent: {gameObject.name}] {message}");
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkCyan;
            Gizmos.DrawWireSphere(transform.position, _targetSearchRadius);
        }
    }
}