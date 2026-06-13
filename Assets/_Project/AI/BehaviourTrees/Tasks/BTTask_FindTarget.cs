using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.AI;
using VanguardProtocol.Characters;
using VanguardProtocol.Systems;

namespace VanguardProtocol.AI.BehaviourTrees
{
    public class BTTask_FindTarget : BTNode
    {
        public BTTask_FindTarget() : base("Find Target") { }

        public override BTNodeStatus Tick(BTContext ctx)
        {
            if (ctx.Owner.Tags.HasTag(GameplayTags.Status_Blinded))
            {
                ctx.ClearData(BTContext.KEY_TARGET);
                return BTNodeStatus.Failure;
            }

            var enemies = TeamManager.Instance.GetAliveEnemies(ctx.Owner.GetTeam());

            if (enemies == null || enemies.Count == 0)
            {
                ctx.ClearData(BTContext.KEY_TARGET);
                return BTNodeStatus.Failure;
            }

            var best = ThreatScorer.GetHighestThreat(ctx.Owner, enemies, ctx.Owner.GetTeam());

            if (best == null) return BTNodeStatus.Failure;

            ctx.SetTarget(best);

            return BTNodeStatus.Success;
        }
    }

    public class BTTask_MoveToTarget : BTNode
    {
        private readonly float _acceptableRange;

        public BTTask_MoveToTarget(float acceptableRange = 15f) : base("MoveToTarget")
        {
            _acceptableRange = acceptableRange;
        }

        public override BTNodeStatus Tick(BTContext ctx)
        {
            var target = ctx.GetTarget();
            if (target == null) return BTNodeStatus.Failure;

            float dist = ctx.Owner.GetDistanceToTarget(target);

            if (dist <= _acceptableRange)
            {
                ctx.Owner.StopMoving();
                return BTNodeStatus.Success;
            }

            ctx.Owner.MoveTo(target.transform.position);

            return BTNodeStatus.Running;
        }
    }

    public class BTTask_FaceTarget : BTNode
    {
        private readonly float _rotationSpeed;

        public BTTask_FaceTarget(float rotationSpeed = 8f) : base("FaceTarget")
        {
            _rotationSpeed = rotationSpeed;
        }

        public override BTNodeStatus Tick(BTContext ctx)
        {
            var target = ctx.GetTarget();
            if (target == null ) return BTNodeStatus.Failure;

            Vector3 dir = (target.transform.position - ctx.Owner.transform.position).normalized;
            dir.y = 0f;

            if (dir == Vector3.zero) return BTNodeStatus.Success;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            ctx.Owner.transform.rotation = Quaternion.Slerp(ctx.Owner.transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);

            return BTNodeStatus.Success;
        }
    }

    public class BTTask_Retreat : BTNode
    {
        private readonly float _retreatDistance;

        public BTTask_Retreat(float retreatDistance = 10f) : base("Retreat")
        {
            _retreatDistance = retreatDistance;
        }

        public override void OnEnter()
        {
            // Handled by BTCondition
        }

        public override BTNodeStatus Tick(BTContext ctx)
        {
            var target = ctx.GetTarget();

            Vector3 retreatDir = target != null ? (ctx.Owner.transform.position - target.transform.position).normalized : -ctx.Owner.transform.forward;

            Vector3 retreatPoint = ctx.Owner.transform.position + retreatDir * _retreatDistance;

            ctx.Owner.MoveTo(retreatPoint);

            if (ctx.Owner.HasReachedDestination())
            {
                ctx.Owner.StopMoving();
                return BTNodeStatus.Success;
            }

            return BTNodeStatus.Running;
        }
    }

    public class BTTask_SupportAlly : BTNode
    {
        private readonly float _healRange;

        public BTTask_SupportAlly(float healRange = 3f) : base("SupportAlly")
        {
            _healRange = healRange;
        }

        public override BTNodeStatus Tick(BTContext ctx)
        {
            var ally = TeamManager.Instance?.GetLowestHPAlly(ctx.Owner.GetTeam());

            if (ally == null || ally == ctx.Owner) return BTNodeStatus.Failure;

            if (ally.GetHealthPercentage() > 0.25f) return BTNodeStatus.Success;

            ctx.Owner.SetAllyNeed(ally);

            float dist = ctx.Owner.GetDistanceToTarget(ally);
            if (dist > _healRange)
            {
                ctx.Owner.MoveTo(ally.transform.position);
                return BTNodeStatus.Running;
            }

            //In Range - Heal
            ctx.Owner.StopMoving();
            ally.Heal(20f, ctx.Owner);
            return BTNodeStatus.Success;
        }
    }
}
