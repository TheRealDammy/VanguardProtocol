using System.Collections.Generic;
using UnityEngine;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.AI.BehaviourTree;
using VanguardProtocol.AI.BehaviourTrees;
using VanguardProtocol.AI.Utility;
using VanguardProtocol.Characters;

namespace VanguardProtocol.AI
{
    public class AssaultAI : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float _attackRange = 5f;
        [SerializeField] private float _bashRange = 7f;
        [SerializeField] private float _attackRate = 0.8f;

        private AICharacterBase _ai;
        private BehaviourTrees.BehaviourTree _bt;
        private UtilityEvaluator _utility;
        private float _lastAttackTime;

        private void Awake()
        {
            _ai = GetComponent<AICharacterBase>();
            _bt = GetComponent<BehaviourTrees.BehaviourTree>();
            _utility = GetComponent<UtilityEvaluator>();
        }

        private void Start()
        {
            _ai.SetRole(GameplayTags.Role_Assault);
            RegisterScoringFunctions();

            var root = BTBuilder_Common.BuildRoot(BuildUtilityDispatch());
            _bt.Initialize(_ai, root);
        }

        // ── Scoring Registration ──────────────────────────────────────────

        private void RegisterScoringFunctions()
        {
            // Attack — Assault's primary action, no role penalty
            _utility.RegisterAction(new ScoredAction(
                UtilityActionType.AttackEnemy, "Attack",
                ctx => CommonScoringFunctions.AttackEnemy(ctx, GameplayTags.Role_Assault)));

            // Shoulder Bash — ability use with assault-specific context
            _utility.RegisterAction(new ScoredAction(
                UtilityActionType.UseAbility, "ShoulderBash",
                ctx => CommonScoringFunctions.UseAbility(ctx,
                    GameplayTags.Ability_Rook_ShoulderBash,
                    c => InBashRange(c) ? 20f : -30f)));   // bonus only if in bash range

            // Flashbang — ability use, reward when target outside attack range
            _utility.RegisterAction(new ScoredAction(
                UtilityActionType.UseAbility, "Flashbang",
                ctx => CommonScoringFunctions.UseAbility(ctx,
                    GameplayTags.Ability_Rook_Flashbang,
                    c => c.GetTarget() != null &&
                         c.Owner.GetDistanceToTarget(c.GetTarget()) > _attackRange ? 25f : -20f)));

            // Flank — Assault is an aggressive flanker
            _utility.RegisterAction(new ScoredAction(
                UtilityActionType.FlankEnemy, "Flank",
                ctx => CommonScoringFunctions.FlankEnemy(ctx, isSupportRole: false)));

            // Cover — Assault takes cover when hurt, but less eagerly than Support roles
            _utility.RegisterAction(new ScoredAction(
                UtilityActionType.TakeCover, "Cover",
                ctx => CommonScoringFunctions.TakeCover(ctx) - 10f));  // -10 bias: Assault prefers aggression

            // Reposition — avoid camping
            _utility.RegisterAction(new ScoredAction(
                UtilityActionType.Reposition, "Reposition",
                ctx => CommonScoringFunctions.Reposition(ctx)));
        }

        // ── BT Dispatch Map ───────────────────────────────────────────────

        private BTNode_UtilityDispatch BuildUtilityDispatch()
        {
            return new BTNode_UtilityDispatch(
                new Dictionary<UtilityActionType, BTNode>
                {
                    [UtilityActionType.AttackEnemy] = BuildAttackSubtree(),
                    [UtilityActionType.UseAbility] = BuildAbilitySubtree(),
                    [UtilityActionType.FlankEnemy] = BuildFlankSubtree(),
                    [UtilityActionType.TakeCover] = BuildCoverSubtree(),
                    [UtilityActionType.Reposition] = BuildRepositionSubtree()
                });
        }

        // ── Subtrees ──────────────────────────────────────────────────────

        private BTNode BuildAttackSubtree()
        {
            return new BTSequence("Attack", new List<BTNode>
            {
                new BTAction("EnterEngaging", ctx =>
                {
                    if (!ctx.Owner.StateMachine.IsInState(AIStateType.Engaging))
                        ctx.Owner.StateMachine.TransitionTo(AIStateType.Engaging);
                    return BTNodeStatus.Success;
                }),
                new BTTask_FindTarget(),
                new BTCondition("InAttackRange",
                    ctx => InAttackRange(ctx),
                    new BTSequence("FireSequence", new List<BTNode>
                    {
                        new BTTask_FaceTarget(),
                        new BTAction("Fire", ctx => TryAttack(ctx))
                    })
                )
            });
        }

        private BTNode BuildAbilitySubtree()
        {
            return new BTSequence("UseAbility", new List<BTNode>
            {
                new BTTask_FindTarget(),
                new BTTask_FaceTarget(),
                new BTAction("ActivateBestAbility", ctx =>
                {
                    // Try Shoulder Bash first (if in range), then Flashbang
                    if (InBashRange(ctx) &&
                        ctx.Owner.AbilitySystem.TryActivateAbility(GameplayTags.Ability_Rook_ShoulderBash))
                        return BTNodeStatus.Success;

                    if (ctx.Owner.AbilitySystem.TryActivateAbility(GameplayTags.Ability_Rook_Flashbang))
                        return BTNodeStatus.Success;

                    return BTNodeStatus.Failure;
                })
            });
        }

        private BTNode BuildFlankSubtree()
        {
            return new BTSequence("Flank", new List<BTNode>
            {
                new BTAction("EnterFlanking", ctx =>
                {
                    if (!ctx.Owner.StateMachine.IsInState(AIStateType.Flanking))
                        ctx.Owner.StateMachine.TransitionTo(AIStateType.Flanking);
                    return BTNodeStatus.Success;
                }),
                new BTTask_FindTarget(),
                new BTTask_MoveToTarget(_attackRange)
            });
        }

        private BTNode BuildCoverSubtree()
        {
            return new BTSequence("TakeCover", new List<BTNode>
            {
                new BTAction("EnterRetreating", ctx =>
                {
                    if (!ctx.Owner.StateMachine.IsInState(AIStateType.Retreating))
                        ctx.Owner.StateMachine.TransitionTo(AIStateType.Retreating);
                    return BTNodeStatus.Success;
                }),
                new BTTask_Retreat(8f)
            });
        }

        private BTNode BuildRepositionSubtree()
        {
            return new BTAction("Reposition", ctx =>
            {
                var enemy = ctx.GetTarget();
                if (enemy == null) return BTNodeStatus.Failure;

                // Move perpendicular to current facing — simple reposition without SpatialQuery
                Vector3 perpendicular = Vector3.Cross(
                    ctx.Owner.transform.forward,
                    Vector3.up).normalized * 5f;

                ctx.Owner.MoveTo(ctx.Owner.transform.position + perpendicular);
                return BTNodeStatus.Running;
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private bool InAttackRange(BTContext ctx)
        {
            var target = ctx.GetTarget();
            return target != null && ctx.Owner.GetDistanceToTarget(target) <= _attackRange;
        }

        private bool InBashRange(BTContext ctx)
        {
            var target = ctx.GetTarget();
            if (target == null) return false;
            float dist = ctx.Owner.GetDistanceToTarget(target);
            return dist > _attackRange && dist <= _bashRange;
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

            Debug.Log($"[AssaultAI] {_ai.name} → {target.name} for {damage} | " +
                      $"HP: {target.Attributes.health.CurrentValue:F1}");

            return BTNodeStatus.Success;
        }
    }
}