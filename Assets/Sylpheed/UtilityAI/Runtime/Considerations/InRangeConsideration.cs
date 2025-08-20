using System.Linq;
using UnityEngine;

namespace Sylpheed.UtilityAI.Considerations
{
    [CreateAssetMenu(fileName = "Within Target Proximity", menuName = "Utility AI/Consideration/Within Target Proximity")]
    public class InRangeConsideration : BoolConsideration
    {
        [Header("Within Target Proximity")] 
        [SerializeField] private float _distanceThreshold = 5f;
        
        private readonly RaycastHit[] _hits = new RaycastHit[30];
        
        protected override bool OnEvaluateAsBool(Decision decision)
        {
            var size = Physics.SphereCastNonAlloc(decision.Agent.transform.position, _distanceThreshold, Vector3.down, _hits, _distanceThreshold);
            if (size == 0) return false;
            return _hits
                .Take(size)
                .Select(hit => hit.collider.GetComponentInParent<UtilityTarget>())
                .Any(target => target == decision.Target);
        }
    }
}