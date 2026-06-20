using System.Collections;
using UnityEngine;
using VanguardProtocol.Characters;

namespace VanguardProtocol.AbilitySystem
{
    [CreateAssetMenu(fileName = "GA_", menuName = "Vanguard Protocol/Gameplay Ability")]
    public abstract class GameplayAbility : ScriptableObject
    {
        [Header("Identity")]
        public string abilityName = "New Ability";
        public GameplayTag abilityTag; // Unique tag to identify this ability

        [Header("Cooldown")]
        public float cooldownDuration = 1f; // Cooldown duration in seconds

        [Header("Activation Requirements")]
        public GameplayTag[] requiredTags; // Tags required on the character to activate this ability
        public GameplayTag[] blockedTags; // Tags that prevent activation of this ability

        [Header("Effects")]
        public GameplayEffect[] effectsToApply;

        // runtime state
        [System.NonSerialized] public bool isActive = false;
        [System.NonSerialized] public float lastActivationTime = -999f;

        // -- Public API --
        public bool CanActivate(AbilitySystemComponent asc)
        {
            if (isActive)
                return false;
            if (!IsCoolDownReady(asc))
                return false;
            if (!MeetsTagRequirements(asc))
                return false;
            return true;
        }

        public float GetCooldownRemaining()
        {
            float timeSinceLastActivation = Time.time - lastActivationTime;
            return Mathf.Max(0f, cooldownDuration - timeSinceLastActivation);
        }

        public bool IsCoolDownReady(AbilitySystemComponent asc)
        {
            float adjustedCooldown = cooldownDuration / asc.Owner.Attributes.abilityCooldownRate.CurrentValue; // Adjust cooldown by character's cooldown rate
            return Time.time - lastActivationTime >= adjustedCooldown;
        }

        // called by AbilitySystemComponent when activating this ability
        public void TryActivate(AbilitySystemComponent asc)
        {
            if (!CanActivate(asc)) return;

            lastActivationTime = Time.time;
            isActive = true;

            asc.Owner.Tags.AddTag(GameplayTags.Ability_Active); // Add a generic active tag, could be used for global checks
            asc.StartAbilityCoroutine(AbilitiyCoroutine(asc));
            OnAbilityEnded(asc);
        }

        public void EndAbility(AbilitySystemComponent asc)
        {
            isActive = false;
            asc.Owner.Tags.RemoveTag(GameplayTags.Ability_Active);
            OnAbilityEnded(asc);
        }
        protected abstract IEnumerator ActivateAbility(AbilitySystemComponent asc);

        // cleanup logic when the ability ends, called when the ability is deactivated
        protected virtual void OnAbilityEnded(AbilitySystemComponent asc)
        {
            // Default implementation does nothing, overrided in derived classes
        }

        // -- Private helper methods --
        private IEnumerator AbilitiyCoroutine(AbilitySystemComponent asc)
        {
            try
            {
                yield return ActivateAbility(asc);
            }
            finally
            {
                EndAbility(asc);
            }
        }

        private bool MeetsTagRequirements(AbilitySystemComponent asc)
        {
           var tags = asc.Owner.Tags;

            // Check required tags
            foreach (var reqTag in requiredTags)
            {
                if (!tags.HasTag(reqTag))
                    return false;
            }
            // Check blocked tags
            foreach (var blockTag in blockedTags)
            {
                if (tags.HasTag(blockTag))
                    return false;
            }
            return true;
        }
    }
}
