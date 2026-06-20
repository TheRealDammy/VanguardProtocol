using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VanguardProtocol.Characters;

namespace VanguardProtocol.AbilitySystem
{
    public class AbilitySystemComponent : MonoBehaviour
    {
        // -- Owner --
        public CharacterBase Owner { get; private set; }

        // -- Abilities --
        private readonly List<GameplayAbility> _grantedAbilities = new List<GameplayAbility>();

        // -- Active Duration Effects --
        private readonly List<ActiveGameplayEffect> _activeEffects = new List<ActiveGameplayEffect>();

        // -- Events --
        public event Action<GameplayAbility> OnAbilityActivated;
        public event Action<GameplayAbility> OnAbilityCooldownStarted;
        public event Action<GameplayEffect> OnEffectApplied;
        public event Action<ActiveGameplayEffect> OnEffectRemoved;

        // -- Initialization --
        private void Awake()
        {
            Owner = GetComponent<CharacterBase>();
        }

        private void Update()
        {
            TickActiveEffects();
        }

        // -- Ability Management --
        public void GrantAbility(GameplayAbility ability)
        {
            if (_grantedAbilities.Contains(ability)) return;

            // Clone the ability ScriptableObject to create a unique instance for this character
            // This allows each character to track cooldowns and active state independently

            var instance = Instantiate(ability); // Create an instance of the ability ScriptableObject
            _grantedAbilities.Add(instance);
        }

        public void RemoveAbility(GameplayAbility ability)
        {
            if (_grantedAbilities.Contains(ability))
            {
                _grantedAbilities.RemoveAll(a => a.abilityTag == ability.abilityTag);
            }
        }

        public bool TryActivateAbility(GameplayTag abilityTag)
        {
            if (Owner.Tags.HasTag(GameplayTags.Ability_Blocked))
            {
                return false;
            }
            if (Owner.Tags.HasTag(GameplayTags.Status_AbilityDisabled))
            {
                return false;
            }

            var ability = GetAbilityByTag(abilityTag);
            if (ability == null)
            {
                return false;
            }

            if (!ability.CanActivate(this))
            {
                return false;
            }

            ability.TryActivate(this);
            OnAbilityActivated?.Invoke(ability);
            OnAbilityCooldownStarted?.Invoke(ability);
            return true;
        }

        public GameplayAbility GetAbilityByTag(GameplayTag tag)
        {
            return _grantedAbilities.Find(a => a.abilityTag == tag);
        }

        public float GetCooldownRemaining(GameplayTag abilityTag)
        {
            var ability = GetAbilityByTag(abilityTag);
            if (ability == null) return 0f;
            return ability?.GetCooldownRemaining() ?? 0f;
        }

        public bool IsAbilityReady(GameplayTag abilityTag)
        {
            var ability = GetAbilityByTag(abilityTag);
            return ability != null && ability.CanActivate(this);
        }

        public List<GameplayAbility> GetAllAbilities() => _grantedAbilities;

        // called by GameplayAbility when applying its effect
        public void StartAbilityCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }

        // -- Effect Management --
        public void ApplyEffect(GameplayEffect effect, CharacterBase source)
        {
            if (!CanApplyEffect(effect)) return;

            switch (effect.durationType)
            {
                case EffectDurationType.Instant:
                    ApplyModifiers(effect, source);
                    break;
                case EffectDurationType.Duration:
                case EffectDurationType.Infinite:
                    ApplyDurationEffect(effect, source);
                    break;
            }

            OnEffectApplied?.Invoke(effect);
        }

        public void RemoveEffectByName(string effectName)
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                if (_activeEffects[i].spec.effectName == effectName)
                {
                    ExpireEffect(_activeEffects[i]);
                }
            }
        }

        // -- Internal Logic --
        private bool CanApplyEffect(GameplayEffect effect)
        {
            var tags = Owner.Tags;

            // Check required tags
            foreach (var tagName in effect.requiredTags)
            {
                var tag = new GameplayTag(tagName);
                if (!Owner.Tags.HasTag(tag))
                    return false;
            }
            // Check blocked tags
            foreach (var tagName in effect.blockedTags)
            {
                var tag = new GameplayTag(tagName);
                if (Owner.Tags.HasTag(tag))
                    return false;
            }
            return true;
        }

        private void ApplyModifiers(GameplayEffect effect, CharacterBase source)
        {
            foreach (var mod in effect.attributeModifiers)
            {
                Owner.Attributes.ApplyModification(new AttributeModification(mod.attributeName, mod.modType, mod.magnitude, source));
            }
        }

        private void ApplyDurationEffect(GameplayEffect effect, CharacterBase source)
        {
            var activeEffect = new ActiveGameplayEffect(effect, source, Owner);

            // Apply initial modifiers and granted tags
            foreach (var tagName in effect.grantedTags)
            {
                var tag = new GameplayTag(tagName);
                activeEffect.grantedTags.Add(tag);
                Owner.Tags.AddTag(tag);
            }

            // Apply initial modifiers
            if (effect.durationType == EffectDurationType.Duration || effect.durationType == EffectDurationType.Infinite)
            {
                ApplyModifiers(effect, source);
            }

            _activeEffects.Add(activeEffect);
        }

        private void TickActiveEffects()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var activeEffect = _activeEffects[i];

                // Tick duration and handle expiration
                if (activeEffect.spec.period > 0f)
                {
                    activeEffect.nextTickTime -= Time.deltaTime;
                    if (activeEffect.nextTickTime <= 0f)
                    {
                        ApplyModifiers(activeEffect.spec, activeEffect.source);
                        activeEffect.nextTickTime += activeEffect.spec.period; // schedule next tick
                    }
                }

                // Handle expiration for duration-based effects
                if (activeEffect.spec.durationType == EffectDurationType.Infinite) continue;

                activeEffect.remainingDuration -= Time.deltaTime;
                if (activeEffect.remainingDuration <= 0f)
                {
                    ExpireEffect(activeEffect);
                }
            }
        }

        private void ExpireEffect(ActiveGameplayEffect activeEffect)
        {
            // Remove granted tags
            foreach (var tag in activeEffect.grantedTags)
            {
                Owner.Tags.RemoveTag(tag);
            }

            _activeEffects.Remove(activeEffect);
            OnEffectRemoved?.Invoke(activeEffect);
        }
    }
}