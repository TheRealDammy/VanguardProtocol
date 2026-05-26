using System;
using UnityEngine;
using VanguardProtocol.AbilitySystem;

namespace VanguardProtocol.Characters
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterBase : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private AttributeSetConfig _attributeConfig;

        // -- Core System --
        public AttributeSet attributes { get; private set; }
        public GameplayTagContainer tags { get; private set; }
        public AbilitySystemComponent abilitySystem { get; private set; }

        // -- Identity --
        public GameplayTag roleTag { get; private set; }
        public GameplayTag teamTag { get; private set; }
        public bool isAlive { get; private set; } = true;

        // -- Events --
        public event Action<CharacterBase> OnDeath;
        public event Action<float, float > OnHealthChanged; // (currentHealth, maxHealth)
        public event Action<CharacterBase, float> OnDamageTaken; // (source, damageAmount)

        protected virtual void Awake()
        {
            tags = new GameplayTagContainer();

            if (_attributeConfig == null)
            {
                Debug.LogError("AttributeSetConfig is not assigned on " + gameObject.name);
                return;
            }

            abilitySystem = GetComponent<AbilitySystemComponent>();
            if (abilitySystem == null)
            {
                Debug.LogError("CharacterBase requires an AbilitySystemComponent on the same GameObject.");
                return;
            }

            attributes = new AttributeSet(_attributeConfig);

            BindAttributeListeners();
        }

        private void BindAttributeListeners()
        {
            // Health change -> fire OnHealthChanged event + manage State.LowHealth tag
            attributes.health.OnValueChanged += (oldVal, newVal) =>
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
            bool isLowHealth = currentHealth / attributes.maxHealth.CurrentValue <= 0.3f;

            if (isLowHealth && !tags.HasTag(GameplayTags.State_LowHealth))
            {
                tags.AddTag(GameplayTags.State_LowHealth);
            }
            else if (!isLowHealth && tags.HasTag(GameplayTags.State_LowHealth))
            {
                tags.RemoveTag(GameplayTags.State_LowHealth);
            }
        }

        // -- Public API --
        public void SetRole(GameplayTag roleTag)
        {
            this.roleTag = roleTag;
            tags.AddTag(roleTag);
        }

        public void SetTeam(GameplayTag teamTag)
        {
            this.teamTag = teamTag;
            tags.AddTag(teamTag);
        }

        // -- Damage (armor absorbtion first) --
        public void TakeDamage(float rawdamage, CharacterBase source = null)
        {
            if (!isAlive) return;

            float absorbedDamage = Mathf.Min(rawdamage, attributes.armor.CurrentValue);
            float effectiveDamage = rawdamage - absorbedDamage;

            if (absorbedDamage > 0f)
            {
                attributes.ApplyModification(
                    new AttributeModification(
                        "Armor", ModificationType.Additive, -absorbedDamage, source));
            }

            if (effectiveDamage > 0f)
            {
                attributes.ApplyModification(
                    new AttributeModification(
                        "Health", ModificationType.Additive, -effectiveDamage, source));

                OnDamageTaken?.Invoke(source, effectiveDamage);
            }
        }

        public void Heal(float healAmount, CharacterBase source = null)
        {
            if (!isAlive) return;
            attributes.ApplyModification(
                new AttributeModification(
                    "Health", ModificationType.Additive, healAmount, source));
        }

        public float GetHealthPercentage() => attributes.health.CurrentValue / attributes.maxHealth.CurrentValue;

        // -- Death Handling --
        private void HandleDeath()
        {
            if (!isAlive) return;

            isAlive = false;
            tags.AddTag(GameplayTags.State_Downed);
            tags.RemoveTag(GameplayTags.State_InCombat);

            OnDeath?.Invoke(this);
        }
    }
}
