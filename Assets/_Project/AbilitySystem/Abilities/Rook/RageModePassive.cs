using UnityEngine;
using VanguardProtocol.Characters;

namespace VanguardProtocol.AbilitySystem.Abilities
{
    [RequireComponent(typeof(CharacterBase))]
    public class RageModePassive : MonoBehaviour
    {
        [SerializeField] private float _attackMultiplier = 1.3f;

        private CharacterBase _character;
        private bool _isActive;

        private void Awake() => _character = GetComponent<CharacterBase>();

        private void Start()
        {
            if (_character == null || _character.Tags == null)
            {
                Debug.LogError($"[RageMode] Missing CharacterBase/Tags on {gameObject.name}");
                return;
            }

            _character.Tags.OnTagAdded += HandleTagAdded;
            _character.Tags.OnTagRemoved += HandleTagRemoved;
        }

        private void OnDisable()
        {
            if (_character?.Tags == null) return;

            _character.Tags.OnTagAdded -= HandleTagAdded;
            _character.Tags.OnTagRemoved -= HandleTagRemoved;
        }

        private void HandleTagAdded(GameplayTag tag)
        {
            if (tag != GameplayTags.State_LowHealth || _isActive) return;

            _character.Attributes.ApplyModification(new AttributeModification(
                "AttackDamage", ModificationType.Multiplicative, _attackMultiplier));

            _isActive = true;
            Debug.Log($"[RageMode] {_character.name} ATK x{_attackMultiplier}");
        }

        private void HandleTagRemoved(GameplayTag tag)
        {
            if (tag != GameplayTags.State_LowHealth || !_isActive) return;

            _character.Attributes.ApplyModification(new AttributeModification(
                "AttackDamage", ModificationType.Multiplicative, 1f / _attackMultiplier));

            _isActive = false;
            Debug.Log($"[RageMode] {_character.name} ATK restored");
        }
    }
}