using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.Characters;
using VanguardProtocol.Systems;

namespace VanguardProtocol.AI
{
    public static class ThreatScorer
    {
        private const float MaxRange = 50f;
        private const float DamageDealtWeight = 2.0f;
        private const float PlayerPriorityBonus = 50f;
        private const float LowHealthModifier = 30f;
        private const float TargetingPenalty = -40f;

        // tracks damage dealt per charcater to avoid expensive lookups
        private static readonly Dictionary<CharacterBase, float> DamageRegistry = new Dictionary<CharacterBase, float>();

        public static void RegisterDamage(CharacterBase target, float damage)
        {
            if (!DamageRegistry.ContainsKey(target))
                DamageRegistry[target] = 0f;

            DamageRegistry[target] += damage;
        }

        public static void Reset() => DamageRegistry.Clear();

        public static CharacterBase GetHighestThreat(AICharacterBase evaluator, List<CharacterBase> candidates, Team evaluatorTeam)
        {
            if (candidates == null || candidates.Count == 0)
                return null;

            CharacterBase best = null;
            float highestScore = float.MinValue;

            foreach (CharacterBase candidate in candidates)
            {
                float score = ComputeScore(evaluator, candidate, evaluatorTeam);
                if (score > highestScore)
                {
                    highestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        public static float ComputeScore(AICharacterBase evaluator, CharacterBase candidate, Team evaluatorTeam)
        {
            float score = 0f;

            // Damage dealt contribution
            if (DamageRegistry.TryGetValue(candidate, out float damageDealt))
                score += damageDealt * DamageDealtWeight;

            // Proximity contribution
            float distance = Vector3.Distance(evaluator.transform.position, candidate.transform.position);
            score += (1f - Mathf.Clamp01(distance / MaxRange)) * 100f;

            //Player Priority Modifier
            if (candidate is PlayerCharacter)
                score += PlayerPriorityBonus;

            // Low health modifier
            if (candidate.tags.HasTag(GameplayTags.State_LowHealth))
                score += LowHealthModifier;

            // Targeting penalty
            if (TeamManager.Instance != null)
            {
                int targetingCount = TeamManager.Instance.GetTargetingCount(candidate, evaluatorTeam);
                if (targetingCount >= 2)
                    score += TargetingPenalty;
            }

            return score;
        }
    }
}
