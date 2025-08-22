using UnityEngine;
using UnityEngine.AI;

namespace Sylpheed.UtilityAI.Sample
{
    [System.Serializable]
    public class PatrolAction : Action
    {
        [SerializeField] private float _minRadius = 10f;
        [SerializeField] private float _maxRadius = 20f;
        [SerializeField] private float _staminaCost = 1f;

        private NavMeshAgent _navAgent;
        private Stamina _stamina;

        protected override bool OnEnter()
        {
            _navAgent = Agent.GetComponent<NavMeshAgent>();
            if (!_navAgent) return false;
            
            _stamina = Agent.GetComponent<Stamina>();
            
            // Get a random patrol point
            var targetPos = GetRandomDestination();
            if (!targetPos.HasValue) return false;

            _navAgent.isStopped = false;
            _navAgent.SetDestination(targetPos.Value);
            
            return true;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_stamina) _stamina.Current -= deltaTime;
        }

        protected override bool ShouldExit()
        {
            if (_navAgent.pathPending) return false;
            return _navAgent.remainingDistance <= _navAgent.stoppingDistance + 0.01f;
        }

        protected override void OnExit()
        {
            _navAgent.isStopped = true;
        }

        private Vector3? GetRandomDestination()
        {
            var maxRetries = 5;

            for (var i = 0; i < maxRetries; i++)
            {
                var radius = Random.Range(_minRadius, _maxRadius);
                var dir = Random.insideUnitCircle * radius;
                var pos = Agent.transform.position + new Vector3(dir.x, 0, dir.y);
                if (!NavMesh.SamplePosition(pos, out var hit, radius, 1))
                    continue;
                
                return hit.position;
            }
            
            return null;
        }
    }
}