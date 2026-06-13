using UnityEngine;
using VanguardProtocol.AbilitySystem;

namespace VanguardProtocol.Characters
{
    [RequireComponent(typeof(CharacterBase))]
    public class HealthRegenComponent : MonoBehaviour
    {
        private CharacterBase _character;
        private float _regenDelay;
        private float _regenRate;
        private float _lastDamageTime;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();
            _regenDelay = _character.Config.regenDelay;
            _regenRate = _character.Config.regenRate;
        }

        private void OnEnable() => _character.OnDamageTaken += HandleDamageTaken;
        private void OnDisable() => _character.OnDamageTaken -= HandleDamageTaken;

        private void Start()
        {
            // Allow immediate regen if never hit
            _lastDamageTime = Time.time - _regenDelay;
        }

        private void HandleDamageTaken(CharacterBase source, float amount)
        {
            _lastDamageTime = Time.time;

            if (_character.Tags.HasTag(GameplayTags.State_Regenerating))
            {
                _character.Tags.RemoveTag(GameplayTags.State_Regenerating);
            }
        }

        public void Update()
        {

            if (!_character.IsAlive) return;

            var health = _character.Attributes.health;
            var maxHealth = _character.Attributes.maxHealth;
            float timeSinceHit = Time.time - _lastDamageTime;

            if (health.CurrentValue >= maxHealth.CurrentValue)
            {
                if (_character.Tags.HasTag(GameplayTags.State_Regenerating))
                {
                    _character.Tags.RemoveTag(GameplayTags.State_Regenerating);
                }
                return;
            }

            if (timeSinceHit < _regenDelay) return;

            if (!_character.Tags.HasTag(GameplayTags.State_Regenerating))
            {
                _character.Tags.AddTag(GameplayTags.State_Regenerating);
            }

            _character.Heal(_regenRate * Time.deltaTime, _character);
        }
    }
}