using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylpheed.UtilityAI
{
    [CreateAssetMenu(fileName = "Behavior", menuName = "Utility AI/Behavior")]
    public sealed class Behavior : ScriptableObject
    {
        [Header("Action")]
        [SerializeReference, SubclassSelector] private Action _action;
        
        [Header("Decision")]
        [SerializeReference, SubclassSelector] private Reasoner _reasoner;
        [SerializeField] private ConsiderationDecorator[] _considerations;
        [SerializeField] private float _weight = 1;
        
        [Header("Target")] 
        [Tooltip("When set, decisions will be evaluated per target based on this behavior.")]
        [SerializeField] private bool _requiresTarget;
        [Tooltip("When set, only evaluate targets with the specified tags.")]
        [SerializeField] private Tag[] _requiredTargetTags = Array.Empty<Tag>();
        
        public Action Action => _action;
        public IReadOnlyList<IConsideration> Considerations { get; private set; }
        public float Weight => _weight;
        public bool RequiresTarget => _requiresTarget;
        public IReadOnlyCollection<Tag> RequiredTargetTags => _requiredTargetTags;

        private void OnEnable()
        {
            RebuildCache();
        }
        
        private void OnValidate()
        {
            RebuildCache();
        }

        public IReadOnlyCollection<Decision> BuildDecisions(UtilityAgent agent, IReadOnlyList<UtilityTarget> targets) 
            => _reasoner.BuildDecisions(agent, this, targets);

        private void RebuildCache()
        {
            Considerations = _considerations.OrderByDescending(c => c.Priority).ToArray();
        }
    }
}