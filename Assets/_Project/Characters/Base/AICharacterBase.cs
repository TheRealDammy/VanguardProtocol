using UnityEngine;
using UnityEngine.AI;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.Systems;

namespace VanguardProtocol.Characters
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AbilitySystemComponent))]

    public class AICharacterBase : CharacterBase
    {
        [Header("AI Config")]
        [SerializeField] private Team _team;

        // -- Components --
        public NavMeshAgent Agent { get; private set; }
        public AIStateMachine StateMachine { get; private set; }

        // -- Targeting --
        public CharacterBase CurrentTarget { get; private set; }
        public CharacterBase CurrentAllyNeed { get; private set; }


        private float _lastDamageTime = -999f;
        private bool _isInCover;

        // Properties queried by scoring functions
        public bool IsTakingDamage => Time.time - _lastDamageTime < 1.5f;
        public bool IsInCover => _isInCover;
        public void SetInCover(bool value) => _isInCover = value;

        protected override void Awake()
        {
            base.Awake();

            Agent = GetComponent<NavMeshAgent>();
            StateMachine = GetComponent<AIStateMachine>();

            // sync navmesh agent with character movement
            Attributes.moveSpeed.OnValueChanged += (_, newValue) => Agent.speed = newValue;
        }

        private void Start()
        {

            Agent.speed = Attributes.moveSpeed.CurrentValue;

            OnDeath += _ => Agent.isStopped = true;

            BindDamageTracking();
        }

        private void BindDamageTracking()
        {
            OnDamageTaken += (_, _) => _lastDamageTime = Time.time;
        }

        // -- Navigation & Targeting --
        public void MoveTo(Vector3 destination)
        {
            if (!IsAlive) return;
            
            Agent.isStopped = false;
            Agent.SetDestination(destination);
        }

        public void StopMoving()
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        public bool HasReachedDestination(float threshold = 0.1f)
        {
            if (Agent.pathPending) return false;
            if (Agent.remainingDistance <= threshold || Agent.remainingDistance <= Agent.stoppingDistance) return true;
            return false;
        }

        public float GetDistanceToTarget(Vector3 point)
        {
            return Vector3.Distance(transform.position, point);
        }

        public float GetDistanceToTarget(CharacterBase other)
        {
            if (other == null) return float.MaxValue;
            return Vector3.Distance(transform.position, other.transform.position);
        }

        public void SetTarget(CharacterBase target)
        {
            CurrentTarget = target;
            if (TeamManager.Instance != null)
            {
                TeamManager.Instance.SetCharcterTarget(this, target);
            }
        }

        public void ClearTarget() => SetTarget(null);


        public void SetAllyNeed(CharacterBase ally) => CurrentAllyNeed = ally;

        // -- Line of Sight --
        public bool HasLOS(CharacterBase target)
        {
            if (target == null) return false;

            Vector3 origin = transform.position + Vector3.up * 1.5f; // Adjust for character height
            Vector3 targetPos = target.transform.position + Vector3.up * 1.5f;
            Vector3 direction = targetPos - origin;

            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude))
            {
                return hit.collider.GetComponentInParent<CharacterBase>() == target;
            }

            return false;
        }

        public Team GetTeam() => _team;
    }
}
