using System.Collections.Generic;
using UnityEngine;

namespace Sylpheed.UtilityAI
{
    [System.Serializable]
    public sealed class ConsiderationDecorator : IConsideration
    {
        [SerializeField] private Consideration _consideration;
        [SerializeField] private bool _inverted;

        public int Priority => _consideration.Priority;
        public bool ShouldCacheScore => _consideration.ShouldCacheScore;
        public bool RequiresTarget => _consideration.RequiresTarget;
        public IReadOnlyCollection<Tag> RequiredTargetTags => _consideration.RequiredTargetTags;

        public float Evaluate(Decision decision)
        {
            var score = _consideration.Evaluate(decision);
            return _inverted ? 1f - score : score;
        }
    }
}