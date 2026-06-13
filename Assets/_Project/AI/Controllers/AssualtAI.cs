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
        //[SerializeField] private float _engageRange = 8f;
        // _engageRange reserved for future flank-route trigger distance
        [SerializeField] private float _attackRange = 5f;
        [SerializeField] private float _attackDamage = 12f;
        [SerializeField] private float _attackRate = 0.8f;
        [SerializeField] private float _shoulderBashRange = 7f;

        private AICharacterBase _ai;
        private BehaviourTrees.BehaviourTree _bt;
        private float _lastAttackTime;

        private void Awake()
        {
            _ai = GetComponent<AICharacterBase>();
            _bt = GetComponent<BehaviourTrees.BehaviourTree>();
        }

        private void Start()
        {
            _ai.SetRole(GameplayTags.Role_Assault);

            var combatSubtree = BuildCombatSubtree();
            var root = BTBuilder_Common.BuildRoot(combatSubtree);
            _bt.Initialize(_ai, root);
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
                        new BTAction("FireAtTarget", ctx => TryAttack(ctx))
                    })
                ),

                // Flashbang — target visible but still outside attack range
                new BTCondition("CanFlashbang", ctx => CanUseAbility(ctx, GameplayTags.Ability_Rook_Flashbang)
                    && ctx.Owner.GetDistanceToTarget(ctx.GetTarget()) > _attackRange,
                    new BTAction("ThrowFlashbang", ctx => UseAbility(ctx, GameplayTags.Ability_Rook_Flashbang))
                ),

                // Shoulder Bash — gap closer + stagger when just outside attack range
                new BTCondition("CanShoulderBash",ctx => CanUseAbility(ctx, GameplayTags.Ability_Rook_ShoulderBash) && InBashRange(ctx),
                    new BTAction("ShoulderBash", ctx => UseAbility(ctx, GameplayTags.Ability_Rook_ShoulderBash))
                ),

                // Close distance aggressively
                new BTCondition("HasTarget",
                ctx => ctx.GetTarget() != null,
                    new BTSequence("CloseDistance", new List<BTNode>
                    {
                        new BTAction("EnterFlanking", ctx =>
                        {
                            if (!ctx.Owner.StateMachine.IsInState(AIStateType.Flanking))
                            {
                                ctx.Owner.StateMachine.TransitionTo(AIStateType.Flanking);
                            }
                            return BTNodeStatus.Success;
                        }),
                        new BTTask_MoveToTarget(_attackRange)
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

            float damage = _ai.Attributes.attackDamage.CurrentValue;
            target.TakeDamage(damage, _ai);

            ThreatScorer.RegisterDamage(target, damage);

            return BTNodeStatus.Success;
        }

        private bool CanUseAbility(BTContext ctx, GameplayTag abilityTag)
        {
            return ctx.GetTarget() != null
                && ctx.Owner.AbilitySystem.IsAbilityReady(abilityTag);
        }

        private bool InBashRange(BTContext ctx)
        {
            float dist = ctx.Owner.GetDistanceToTarget(ctx.GetTarget());
            return dist > _attackRange && dist <= _shoulderBashRange;
        }

        private BTNodeStatus UseAbility(BTContext ctx, GameplayTag abilityTag)
        {
            bool activated = ctx.Owner.AbilitySystem.TryActivateAbility(abilityTag);
            return activated ? BTNodeStatus.Success : BTNodeStatus.Failure;
        }
    }
}