using UnityEngine;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.AI.BehaviourTrees;
using VanguardProtocol.Characters;
using VanguardProtocol.Systems;

namespace VanguardProtocol.AI.Utility
{
    public static class CommonScoringFunctions
    {
        // ── Score_AttackEnemy ─────────────────────────────────────────────
        // Base: 50. +20 if enemy threatens player. +15 if clear LOS. -30 if low HP.
        public static float AttackEnemy(BehaviourTrees.BTContext ctx, GameplayTag roleTag)
        {
            if (ctx.GetTarget() == null) return 0f;

            float score = 50f;

            var target = ctx.GetTarget();
            var player = GetPlayer(ctx);

            if (player != null && target == player)
                score += 20f;

            if (ctx.Owner.HasLOS(target))
                score += 15f;

            if (ctx.Owner.Tags.HasTag(GameplayTags.State_LowHealth))
                score -= 30f;

            return score;
        }

        // ── Score_TakeCover ───────────────────────────────────────────────
        // Base: 30. +40 if taking damage. +20 if poor cover. -20 if already in cover.
        public static float TakeCover(BehaviourTrees.BTContext ctx)
        {
            float score = 30f;

            if (ctx.Owner.IsTakingDamage)
                score += 40f;

            if (ctx.Owner.IsInCover)
                score -= 20f;

            // No SpatialQuery yet — placeholder: +20 if not in cover
            if (!ctx.Owner.IsInCover)
                score += 20f;

            return score;
        }

        // ── Score_HealAlly ────────────────────────────────────────────────
        // Base: 0. +80 if ally below 25% HP and heal ready. +20 per additional injured.
        public static float HealAlly(BehaviourTrees.BTContext ctx, GameplayTag healAbilityTag)
        {
            if (TeamManager.Instance == null) return 0f;

            if (!ctx.Owner.AbilitySystem.IsAbilityReady(healAbilityTag))
                return 0f;

            float score = 0f;
            int injured = 0;
            var allies = TeamManager.Instance.GetAliveTeamMembers(ctx.Owner.GetTeam());

            foreach (var ally in allies)
            {
                if (ally == ctx.Owner) continue;
                if (ally.GetHealthPercentage() < 0.25f)
                {
                    score += 80f;
                    injured++;
                }
            }

            if (injured > 1)
                score += 20f * (injured - 1);

            return score;
        }

        // ── Score_FlankEnemy ──────────────────────────────────────────────
        // Base: 20. +30 if flank point available. +15 if HP > 60%. -40 if Support/Medic.
        public static float FlankEnemy(BehaviourTrees.BTContext ctx, bool isSupportRole)
        {
            if (ctx.GetTarget() == null) return 0f;

            float score = 20f;

            // SpatialQuery not built yet — placeholder: always assume route available
            score += 30f;

            if (ctx.Owner.GetHealthPercentage() > 0.6f)
                score += 15f;

            if (isSupportRole)
                score -= 40f;

            return score;
        }

        // ── Score_UseAbility ──────────────────────────────────────────────
        // Base: 40. Per-role context modulation via supplied modifier function.
        public static float UseAbility(BehaviourTrees.BTContext ctx,
            GameplayTag abilityTag,
            System.Func<BehaviourTrees.BTContext, float> contextModifier = null)
        {
            if (!ctx.Owner.AbilitySystem.IsAbilityReady(abilityTag))
                return 0f;

            if (ctx.GetTarget() == null)
                return 0f;

            float score = 40f;

            if (contextModifier != null)
                score += contextModifier(ctx);

            return score;
        }

        // ── Score_Reposition ──────────────────────────────────────────────
        // Base: 10. +25 if in same cover > 8s. +20 if better cover exists.
        public static float Reposition(BehaviourTrees.BTContext ctx)
        {
            float score = 10f;

            if (ctx.Owner.StateMachine.TimeInState > 8f && ctx.Owner.IsInCover)
                score += 25f;

            // SpatialQuery not built yet — placeholder: +20 if not moving
            if (ctx.Owner.Agent.velocity.magnitude < 0.1f && !ctx.Owner.IsTakingDamage)
                score += 20f;

            return score;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static CharacterBase GetPlayer(BehaviourTrees.BTContext ctx)
        {
            var team = TeamManager.Instance?.GetAliveTeamMembers(ctx.Owner.GetTeam());
            return team?.Find(c => c is PlayerCharacter);
        }
    }
}