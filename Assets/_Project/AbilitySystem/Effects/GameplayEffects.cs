using System.Collections.Generic;
using UnityEngine;
using VanguardProtocol.Characters;

namespace VanguardProtocol.AbilitySystem
{
    public enum EffectDurationType
    {
        Instant,
        Duration,
        Infinite
    }

    [System.Serializable]
    public class AttributeModifierConfig
    {
        public string attributeName;
        public float magnitude;
        public ModificationType modType;
    }

    [CreateAssetMenu(fileName = "GE_", menuName = "Vanguard Protocol/Gameplay Effect")]
    public class GameplayEffect : ScriptableObject
    {
        [Header("Identy")]
        public string effectName = "New Effect";

        [Header("Duration")]
        public EffectDurationType durationType = EffectDurationType.Instant;
        public float duration = 0f; // ignored if instant or infinite
        public float period = 0f; // for periodic effects ignored if instant or infinite

        [Header("Modifiers")]
        public List<AttributeModifierConfig> attributeModifiers = new List<AttributeModifierConfig>();

        [Header("Tags")]
        public List<string> grantedTags = new List<string>(); // Tags added to the character while the effect is active
        public List<string> requiredTags = new List<string>(); // Tags required on the character to apply the effect
        public List<string> blockedTags = new List<string>(); // Tags that prevent the effect from being applied
    }

    // runtime instance of an applied GameplayEffect, tracks remaining duration and other state
    public class ActiveGameplayEffect
    {
        public GameplayEffect spec;
        public CharacterBase source;
        public CharacterBase target;
        public float remainingDuration;
        public float nextTickTime; // for periodic effects
        public List<GameplayTag> grantedTags = new List<GameplayTag>();

        public ActiveGameplayEffect(GameplayEffect spec, CharacterBase source, CharacterBase target)
        {
            this.spec = spec;
            this.source = source;
            this.target = target;
            remainingDuration = spec.duration;
            nextTickTime = spec.period > 0f ? spec.period : float.MaxValue;
            // Convert granted tag strings to GameplayTag instances
            foreach (var tagName in spec.grantedTags)
            {
                grantedTags.Add(new GameplayTag(tagName));
            }
        }
    }
}
