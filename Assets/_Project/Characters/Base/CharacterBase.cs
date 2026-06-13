using System;
using UnityEngine;
using VanguardProtocol.AbilitySystem;

namespace VanguardProtocol.Characters
{
    public class CharacterBase : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private AttributeSetConfig _attributeConfig;

        // -- Core System --
        public AttributeSet Attributes { get; private set; }
        public GameplayTagContainer Tags { get; private set; }
        public AbilitySystemComponent AbilitySystem { get; private set; }
        public AttributeSetConfig Config => _attributeConfig;

        // -- Identity --
        public GameplayTag RoleTag { get; private set; }
        public GameplayTag TeamTag { get; private set; }
        public bool IsAlive { get; private set; } = true;

        // -- Events --
        public event Action<CharacterBase> OnDeath;
        public event Action<float, float > OnHealthChanged; // (currentHealth, maxHealth)
        public event Action<CharacterBase, float> OnDamageTaken; // (source, damageAmount)

        protected virtual void Awake()
        {
            Tags = new GameplayTagContainer();

            if (_attributeConfig == null)
            {
                Debug.LogError("AttributeSetConfig is not assigned on " + gameObject.name);
                return;
            }

            AbilitySystem = GetComponent<AbilitySystemComponent>();
            if (AbilitySystem == null)
            {
                Debug.LogError("CharacterBase requires an AbilitySystemComponent on the same GameObject.");
                return;
            }

            Attributes = new AttributeSet(_attributeConfig);

            BindAttributeListeners();
        }

        private void BindAttributeListeners()
        {
            // Health change -> fire OnHealthChanged event + manage State.LowHealth tag
            Attributes.health.OnValueChanged += (oldVal, newVal) =>
            {
                OnHealthChanged?.Invoke(oldVal, newVal);
                EvaluateLowHealthTag(newVal);

                if (newVal <= 0f)
                {
                    HandleDeath();
                }
            };
        }

        private void EvaluateLowHealthTag(float currentHealth)
        {
            bool isLowHealth = currentHealth / Attributes.maxHealth.CurrentValue <= 0.3f;

            if (isLowHealth && !Tags.HasTag(GameplayTags.State_LowHealth))
            {
                Tags.AddTag(GameplayTags.State_LowHealth);
            }
            else if (!isLowHealth && Tags.HasTag(GameplayTags.State_LowHealth))
            {
                Tags.RemoveTag(GameplayTags.State_LowHealth);
            }
        }

        // -- Public API --
        public void SetRole(GameplayTag roleTag)
        {
            this.RoleTag = roleTag;
            Tags.AddTag(roleTag);
        }

        public void SetTeam(GameplayTag teamTag)
        {
            this.TeamTag = teamTag;
            Tags.AddTag(teamTag);
        }

        // -- Damage (armor absorbtion first) --
        public void TakeDamage(float rawdamage, CharacterBase source = null)
        {
            if (!IsAlive) return;

            float absorbedDamage = Mathf.Min(rawdamage, Attributes.armor.CurrentValue);
            float effectiveDamage = rawdamage - absorbedDamage;

            if (absorbedDamage > 0f)
            {
                Attributes.ApplyModification(
                    new AttributeModification(
                        "Armor", ModificationType.Additive, -absorbedDamage, source));
            }

            if (effectiveDamage > 0f)
            {
                Attributes.ApplyModification(
                    new AttributeModification(
                        "Health", ModificationType.Additive, -effectiveDamage, source));

                OnDamageTaken?.Invoke(source, effectiveDamage);
            }
        }

        public void Heal(float healAmount, CharacterBase source = null)
        {
            if (!IsAlive) return;
            Attributes.ApplyModification(
                new AttributeModification(
                    "Health", ModificationType.Additive, healAmount, source));
        }

        public float GetHealthPercentage() => Attributes.health.CurrentValue / Attributes.maxHealth.CurrentValue;

        // -- Death Handling --
        private void HandleDeath()
        {
            if (!IsAlive) return;

            IsAlive = false;
            Tags.AddTag(GameplayTags.State_Downed);
            Tags.RemoveTag(GameplayTags.State_InCombat);

            OnDeath?.Invoke(this);
        }
    }
}
