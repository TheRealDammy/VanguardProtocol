using System;
using UnityEngine;

namespace VanguardProtocol.AbilitySystem
{
    public class AttributeSet
    {
        // -- Attributes --
        public GameplayAttribute health { get; private set; }
        public GameplayAttribute maxHealth { get; private set; }
        public GameplayAttribute armor { get; set; }
        public GameplayAttribute attackDamage { get; set; }
        public GameplayAttribute moveSpeed { get; set; }
        public GameplayAttribute abilityCooldownRate { get; set; }

        // Broadcaster before an attribute is modified, allowing listeners to react or even cancel the modification.
        public event Func<AttributeModification, bool> OnPreModify;

        // Broadcaster after an attribute has been modified, allowing listeners to react to the change.
        public event Action<AttributeModification> OnPostModify;

        public AttributeSet(AttributeSetConfig config)
        {
            health = new GameplayAttribute("Health", config.maxHealth);
            maxHealth = new GameplayAttribute("MaxHealth", config.maxHealth);
            armor = new GameplayAttribute("Armor", config.armor);
            attackDamage = new GameplayAttribute("AttackDamage", config.attackDamage);
            moveSpeed = new GameplayAttribute("MoveSpeed", config.moveSpeed);
            abilityCooldownRate = new GameplayAttribute("AbilityCooldownRate", config.abilityCooldownRate);
        }

        public void ApplyModification(AttributeModification mod)
        {
            GameplayAttribute target = GetAttribute(mod.attributeName);
            if (target == null)
            {
                Debug.LogWarning($"Attribute '{mod.attributeName}' not found in AttributeSet.");
                return;
            }

            // Trigger pre-modification event and allow listeners to cancel the modification
            if (OnPreModify != null)
            {
                foreach (Func<AttributeModification, bool> listener in OnPreModify.GetInvocationList())
                {
                    if (!listener.Invoke(mod))
                    {
                        Debug.Log($"Modification to '{mod.attributeName}' was cancelled by a listener.");
                        return; // Modification cancelled
                    }
                }
            }

            float newValue = mod.modType switch
            {
                ModificationType.Override => mod.magnitude,
                ModificationType.Additive => target.CurrentValue + mod.magnitude,
                ModificationType.Multiplicative => target.CurrentValue * mod.magnitude,
                _ => target.CurrentValue
            };

            // Ensure new value does not exceed MaxHealth if modifying Health
            if (mod.attributeName == "Health")
            {
                newValue = Mathf.Min(newValue, maxHealth.CurrentValue);
            }

            target.SetCurrentValue(newValue);

            // Trigger post-modification event
            OnPostModify?.Invoke(mod);
        }

        public GameplayAttribute GetAttribute(string attributeName)
        {
            return attributeName switch
            {
                "Health" => health,
                "MaxHealth" => maxHealth,
                "Armor" => armor,
                "AttackDamage" => attackDamage,
                "MoveSpeed" => moveSpeed,
                "AbilityCooldownRate" => abilityCooldownRate,
                _ => null
            };
        }
    }

    // Supplementary classes for attribute modification
    public enum ModificationType
    {
        Override,
        Additive,
        Multiplicative
    }

    public struct AttributeModification
    {
        public string attributeName;
        public ModificationType modType;
        public float magnitude;
        public object source; // Optional: source of the modification (e.g., ability, item)
        public AttributeModification(string attributeName, ModificationType modType, float magnitude, object source = null)
        {
            this.attributeName = attributeName;
            this.modType = modType;
            this.magnitude = magnitude;
            this.source = source;
        }
    }
}
