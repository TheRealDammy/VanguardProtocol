using UnityEngine;
using VanguardProtocol.Characters;

namespace VanguardProtocol.AbilitySystem.Abilities
{
    public static class AbilityCombatUtils
    {
        public static bool IsEnemy(CharacterBase a, CharacterBase b)
        {
            bool aBlue = a.Tags.HasTag(GameplayTags.Team_Blue);
            bool aRed = a.Tags.HasTag(GameplayTags.Team_Red);
            bool bBlue = b.Tags.HasTag(GameplayTags.Team_Blue);
            bool bRed = b.Tags.HasTag(GameplayTags.Team_Red);

            if (!aBlue && !aRed) Debug.LogWarning($"[IsEnemy] {a.name} has no team tag!");
            if (!bBlue && !bRed) Debug.LogWarning($"[IsEnemy] {b.name} has no team tag!");

            return (aBlue && bRed) || (aRed && bBlue);
        }

        public static void ApplyEffectToEnemiesInRadius(CharacterBase owner, Vector3 origin, float radius, GameplayEffect[] effects, string logTag)
        {
            var hits = Physics.OverlapSphere(origin, radius);

            foreach (var hit in hits)
            {
                var target = hit.GetComponentInParent<CharacterBase>();
                if (target == null)
                {
                    continue;
                }
                if (target == owner) continue;

                bool enemy = IsEnemy(owner, target);

                if (!enemy) continue;

                var targetAsc = target.GetComponent<AbilitySystemComponent>();
                if (targetAsc == null)
                {
                    continue;
                }

                foreach (var effect in effects)
                    targetAsc.ApplyEffect(effect, owner);
            }
        }
    }
}