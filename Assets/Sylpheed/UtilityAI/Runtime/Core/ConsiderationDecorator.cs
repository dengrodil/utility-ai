using System.Collections.Generic;
using UnityEngine;

namespace Sylpheed.UtilityAI
{
    [System.Serializable]
    public sealed class ConsiderationDecorator : IConsideration
    {
        [SerializeField] private Consideration _consideration;
        [Tooltip("Flips the score of the consideration (1 - score). This is useful if you want to reuse a Consideration but you want to score it the other way around.")]
        [SerializeField] private bool _inverted;
        [Tooltip("Consideration will not be scored. Resulting to a score of 1. Make sure that there's at least 1 Consideration in the Behavior that isn't muted.")]
        [SerializeField] private bool _mute;

        public string Name => $"{(_inverted ? "Not " : "")}{_consideration.Name}";
        public int Priority => _consideration.Priority;
        public bool ShouldCacheScore => _consideration.ShouldCacheScore;
        public bool RequiresTarget => _consideration.RequiresTarget;
        public IReadOnlyCollection<Tag> RequiredTargetTags => _consideration.RequiredTargetTags;
        public IEnumerable<IConsideration> Children => _consideration.Children;

        public float Evaluate(Decision decision)
        {
            if (_mute) return 1f;
            var score = _consideration.Evaluate(decision);
            return _inverted ? 1f - score : score;
        }

        public override string ToString()
        {
            return _consideration?.ToString();
        }
    }
}