using System.Collections.Generic;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.Characters;

namespace VanguardProtocol.AI.BehaviourTrees
{
    public static class BTBuilder_Common
    {
        // builds the common root all roles share
        // roleCombat would be injected per role

        public static BTNode BuildRoot(BTNode roleCombatSubtree)
        {
            return new BTSelector("Root", new List<BTNode>
            {
                // 1st priority - self preservation
                new BTCondition("IsLowHP",
                    ctx => ctx.Owner.tags.HasTag(GameplayTags.State_LowHealth),
                     new BTSequence("Retreat", new List<BTNode>
                     {
                        new BTAction("EnterRetreat", ctx =>
                        {
                            ctx.Owner.StateMachine.TransitionTo(AIStateType.Retreating);
                            return BTNodeStatus.Success;
                        }),
                        new BTTask_Retreat(10f)
                     })
                ),

                // 2nd Priority - Aid player if in danger
                new BTCondition("PlayerNeedsHelp", 
                    ctx =>
                    {
                        var player = GetPlayer(ctx);
                        return player != null && player.GetHealthPercentage() < 0.3f && ctx.Owner.abilitySystem.IsAbilityReady(GameplayTags.Ability_Active);
                    },
                    new BTSequence("AidPlayer", new List<BTNode>
                    {
                        new BTAction("EnterSupport", ctx =>
                        {
                            ctx.Owner.StateMachine.TransitionTo(AIStateType.Supporting);
                            return BTNodeStatus.Success;
                        }),
                        new BTTask_SupportAlly(3f)
                    })
                ),

                // Priority 3 — Engage
                new BTCondition("HasEnemies",
                    ctx => Systems.TeamManager.Instance?.GetAliveEnemies(ctx.Owner.GetTeam()).Count > 0,
                    new BTSequence("Engage", new List<BTNode>
                    {
                        new BTAction("EnterEngaging", ctx =>
                        {
                            if (!ctx.Owner.StateMachine.IsInState(AIStateType.Engaging))
                                ctx.Owner.StateMachine.TransitionTo(AIStateType.Engaging);
                            return BTNodeStatus.Success;
                        }),
                        new BTTask_FindTarget(),
                        roleCombatSubtree // Role-specific behaviour
                    })
                ),

                // Priority 4 — Idle
                new BTAction("Idle", ctx =>
                {
                    if (!ctx.Owner.StateMachine.IsInState(AIStateType.Idle))
                        ctx.Owner.StateMachine.TransitionTo(AIStateType.Idle);
                    ctx.Owner.StopMoving();
                    return BTNodeStatus.Running;
                })
            });
        }

        private static Characters.CharacterBase GetPlayer(BTContext ctx)
        {
            var team = Systems.TeamManager.Instance?.GetAliveTeamMembers(ctx.Owner.GetTeam());
            return team?.Find(c => c is Characters.PlayerCharacter);
        }
    }
}