using System.Collections.Generic;
using UnityEngine;
using VanguardProtocol.Characters;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.AI.BehaviourTrees;

namespace VanguardProtocol.AI
{
    public class AssaultAI : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float _engageRange = 8f;
        [SerializeField] private float _attackRange = 5f;
        [SerializeField] private float _attackDamage = 12f;
        [SerializeField] private float _attackRate = 0.8f;

        private AICharacterBase _ai;
        private BehaviourTrees.BehaviourTree _bt;
        private float _lastAttackTime;

        private void Awake()
        {
            Debug.Log($"[AssaultAI] Awake fired on {gameObject.name}");
            _ai = GetComponent<AICharacterBase>();
            _bt = GetComponent<BehaviourTrees.BehaviourTree>();
            Debug.Log($"[AssaultAI] AI: {_ai != null} | BT: {_bt != null}");
        }

        private void Start()
        {
            Debug.Log($"[AssaultAI] Start fired");
            _ai.SetRole(GameplayTags.Role_Assault);

            var combatSubtree = BuildCombatSubtree();
            var root = BTBuilder_Common.BuildRoot(combatSubtree);
            _bt.Initialize(_ai, root);

            if (!_ai.Agent.isOnNavMesh)
                Debug.LogError($"[AssaultAI] {gameObject.name} is NOT on NavMesh — rebake required");
            else
                Debug.Log($"[AssaultAI] {gameObject.name} is on NavMesh");
        }

        private BTNode BuildCombatSubtree()
        {
            return new BTSelector("AssaultCombat", new List<BTNode>
            {
                // Attack if in range
                new BTCondition("InAttackRange",
                    ctx => IsInAttackRange(ctx),
                    new BTSequence("Attack", new List<BTNode>
                    {
                        new BTTask_FaceTarget(),
                        new BTAction("FireAtTarget", ctx =>
                        {
                            return TryAttack(ctx);
                        })
                    })
                ),

                // Close distance aggressively
                new BTCondition("HasTarget",
                    ctx => ctx.GetTarget() != null,
                    new BTSequence("CloseDistance", new List<BTNode>
                    {
                        new BTAction("EnterFlanking", ctx =>
                        {
                            ctx.Owner.StateMachine.TransitionTo(AIStateType.Flanking);
                            return BTNodeStatus.Success;
                        }),
                        new BTTask_MoveToTarget(_engageRange)
                    })
                )
            });
        }

        private bool IsInAttackRange(BTContext ctx)
        {
            var target = ctx.GetTarget();
            return target != null
                && ctx.Owner.GetDistanceToTarget(target) <= _attackRange;
        }

        private BTNodeStatus TryAttack(BTContext ctx)
        {
            if (Time.time - _lastAttackTime < _attackRate)
                return BTNodeStatus.Running;

            var target = ctx.GetTarget();
            if (target == null) return BTNodeStatus.Failure;

            _lastAttackTime = Time.time;

            float damage = _ai.attributes.attackDamage.CurrentValue;
            target.TakeDamage(damage, _ai);

            ThreatScorer.RegisterDamage(target, damage);

            Debug.Log($"[AssaultAI] {_ai.name} attacked {target.name} " +
                      $"for {damage} | Target HP: " +
                      $"{target.attributes.health.CurrentValue:F1}");

            // Rage Mode — State.LowHP tag fires extra aggression
            if (_ai.tags.HasTag(GameplayTags.State_LowHealth))
            {
                target.TakeDamage(damage * 0.3f, _ai);
                Debug.Log($"[AssaultAI] Rage Mode bonus damage!");
            }

            return BTNodeStatus.Success;
        }
    }
}