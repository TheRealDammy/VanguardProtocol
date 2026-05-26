using UnityEngine;

namespace VanguardProtocol.AbilitySystem
{
    [CreateAssetMenu(fileName = "AttributeSetConfig_", menuName = "Vanguard Protocol/Attribute Set Config", order = 0)]
    public class AttributeSetConfig : ScriptableObject
    {
        [Header("Vitals")]
        public float maxHealth = 100f;
        public float armor = 0f;

        [Header("Movement")]
        public float moveSpeed = 5f;

        [Header("Combat")]
        public float attackDamage = 10f;

        [Header("Abilities")]
        public float abilityCooldownRate = 1f; // Multiplier for how quickly abilities cooldown - 1.0 is normal, 2.0 is twice as fast, etc.
    }
}
