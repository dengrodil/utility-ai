using System.Collections.Generic;

namespace Sylpheed.UtilityAI
{
    public interface IConsideration
    {
        string Name { get; }
        int Priority { get; }
        bool ShouldCacheScore { get; }
        bool RequiresTarget { get; }
        IReadOnlyCollection<Tag> RequiredTargetTags { get; }

        float Evaluate(Decision decision);
    }
}