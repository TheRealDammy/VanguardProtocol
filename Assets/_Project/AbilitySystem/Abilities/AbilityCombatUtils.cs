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
                    Debug.Log($"[{logTag}] {hit.gameObject.name} has no CharacterBase — skipped");
                    continue;
                }
                if (target == owner) continue;

                bool enemy = IsEnemy(owner, target);
                Debug.Log($"[{logTag}] Checking {target.name} — IsEnemy: {enemy} | " +
                          $"OwnerBlue: {owner.Tags.HasTag(GameplayTags.Team_Blue)} | " +
                          $"TargetBlue: {target.Tags.HasTag(GameplayTags.Team_Blue)}");

                if (!enemy) continue;

                var targetAsc = target.GetComponent<AbilitySystemComponent>();
                if (targetAsc == null)
                {
                    Debug.Log($"[{logTag}] {target.name} has no AbilitySystemComponent — effect not applied");
                    continue;
                }

                foreach (var effect in effects)
                    targetAsc.ApplyEffect(effect, owner);

                Debug.Log($"[{logTag}] {owner.name} affected {target.name}");
            }
        }
    }
}